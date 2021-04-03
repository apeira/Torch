using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Torch.Core.Commands
{
    /// <summary>
    /// Hooks the console standard input and output into the command system.
    /// TODO: Figure out why this crashes when Torch is run in Windows Terminal
    /// </summary>
    public class ConsoleService
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        private ICommandService? _commandService;
        private readonly CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();
        private readonly StringBuilder _currentUserInput = new StringBuilder();
        private (int Left, int Top) _consoleWritePosition;
        private TextWriter? _stdOut;
        private int _inputCursor;

        public ConsoleService(ITorchCore core)
        {
            core.StateChanged += OnGameStateChange;
        }

        internal void AttachCommandService(ICommandService commands)
        {
            _commandService = commands;
        }

        private void OnGameStateChange(CoreState state)
        {
            switch (state)
            {
                case CoreState.AfterStart:
                    StartListener();
                    break;
                case CoreState.BeforeStop:
                    _cancelTokenSource.Cancel();
                    break;
            }
        }

        private void StartListener()
        {
            _consoleWritePosition = (Console.CursorLeft, Console.CursorTop);
            _stdOut = new StreamWriter(Console.OpenStandardOutput()){AutoFlush = true};
            var notifyingOut = new NotifyingTextWriter(_stdOut);
            Console.SetOut(notifyingOut);
            notifyingOut.BeforeWrite += HandleBeforeWrite;
            notifyingOut.AfterWrite += HandleAfterWrite;

            RewriteUserInput();
            Task.Run(() =>
            {
                while (!_cancelTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        var key = Console.ReadKey(true);

                        switch (key.Key)
                        {
                            case ConsoleKey.Enter:
                                if (_currentUserInput.Length == 0)
                                    return;
                                
                                var input = _currentUserInput.ToString();
                                _currentUserInput.Clear();
                                _inputCursor = 0;

                                if (_commandService != null)
                                    _commandService?.Execute(this, input, Console.WriteLine);
                                else
                                    Console.WriteLine("The command service is not available.");
                                break;
                            case ConsoleKey.Backspace:
                                if (_currentUserInput.Length > 0)
                                {
                                    _currentUserInput.Remove(_inputCursor - 1, 1);
                                    _inputCursor--;
                                }
                                break;
                            case ConsoleKey.Delete:
                                if (_inputCursor < _currentUserInput.Length)
                                    _currentUserInput.Remove(_inputCursor, 1);
                                break;
                            case ConsoleKey.LeftArrow:
                                if (_inputCursor > 0)
                                    _inputCursor--;
                                break;
                            case ConsoleKey.RightArrow:
                                if (_inputCursor < _currentUserInput.Length)
                                    _inputCursor++;
                                break;
                            case ConsoleKey.UpArrow:
                            case ConsoleKey.DownArrow:
                                // TODO support command history
                                break;
                            case ConsoleKey.Tab:
                                // TODO support tab completion
                                break;
                            default:
                                if (_inputCursor == _currentUserInput.Length)
                                    _currentUserInput.Append(key.KeyChar);
                                else
                                    _currentUserInput.Insert(_inputCursor, key.KeyChar);
                                _inputCursor++;
                                break;
                        }

                        RewriteUserInput();
                    }
                    catch (Exception e)
                    {
                        _log.Error(e);
                    }
                }
            }, _cancelTokenSource.Token);
        }

        private void HandleAfterWrite()
        {
            _consoleWritePosition = (Console.CursorLeft, Console.CursorTop);
            RewriteUserInput();
        }

        private void HandleBeforeWrite()
        {
            ClearUserInput();
            Console.SetCursorPosition(_consoleWritePosition.Left, _consoleWritePosition.Top);
        }

        private void RewriteUserInput()
        {
            ClearUserInput();
            var (left, top) = GetUserCursor();
            Console.SetCursorPosition(0, top);
            _stdOut!.Write($"> {_currentUserInput}");
            Console.SetCursorPosition(left, top);
        }

        private void ClearUserInput()
        {
            Console.SetCursorPosition(0, GetUserCursor().Top);
            _stdOut!.Write(new string(' ', Console.BufferWidth - 1));
        }

        private (int Left, int Top) GetUserCursor()
        {
            var windowBottom = Console.WindowHeight - 1;
            var textBottom = _consoleWritePosition.Top + 1;

            return (_inputCursor + 2, Math.Max(windowBottom, textBottom));
        }
    }
}