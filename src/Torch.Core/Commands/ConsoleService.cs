using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
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
        private ConsoleKey _previousKey;

        // Command History
        private List<string> _commandHistory = new List<string>();
        private int _commandIndex = -1;
        private string _currentCommand;


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
            _stdOut = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            var notifyingOut = new NotifyingTextWriter(_stdOut);
            Console.SetOut(notifyingOut);
            notifyingOut.BeforeWrite += HandleBeforeWrite;
            notifyingOut.AfterWrite += HandleAfterWrite;
            EnableAnsiEscape();
            RewriteUserInput();
            Task.Run(() => Listener(), _cancelTokenSource.Token);
        }

        private void Listener()
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

                            // Add this command to the beginning of the command history
                            _commandHistory.Insert(0, input);

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
                            // No need to run this if the history is empty
                            if (_commandHistory.Count == 0)
                                break;

                            string previousCommand = IteratePreviousCommands(key.Key);
                            _currentUserInput.Append(previousCommand);
                            _inputCursor = previousCommand.Length;
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


                    _previousKey = key.Key;
                    RewriteUserInput();
                }
                catch (Exception e)
                {
                    _log.Error(e);
                }
            }
        }



        private string IteratePreviousCommands(ConsoleKey currentKey)
        {
            // Reset variables. (after you use a different function, current command index is reset)
            if (_previousKey != ConsoleKey.UpArrow && _previousKey != ConsoleKey.DownArrow)
            {
                _commandIndex = -1;
                _currentCommand = _currentUserInput.ToString();
            }

            _currentUserInput.Clear();
            if (currentKey == ConsoleKey.UpArrow)
                _commandIndex++;

            if (currentKey == ConsoleKey.DownArrow)
                _commandIndex--;

            // If Index is -1 we need to return our initial command
            if (_commandIndex <= -1)
            {
                _commandIndex = -1;
                return _currentCommand;
            }

            // If index is over length we need to reset it back to max
            if (_commandIndex >= _commandHistory.Count - 1)
                _commandIndex = _commandHistory.Count - 1;

            // Return specified command index
            return _commandHistory[_commandIndex];
        }

        private void HandleAfterWrite()
        {
            _consoleWritePosition = (Console.CursorLeft, Console.CursorTop);
            RewriteUserInput();
        }

        private void HandleBeforeWrite()
        {
            Console.CursorVisible = false;
            ClearUserInput();
            Console.SetCursorPosition(_consoleWritePosition.Left, _consoleWritePosition.Top);
            Console.CursorVisible = true;
        }

        private void RewriteUserInput()
        {
            Console.CursorVisible = false;
            ClearUserInput();
            var (left, top) = GetUserCursor();
            Console.SetCursorPosition(0, top);
            _stdOut!.Write($"> \u001b[33;1m{_currentUserInput}\u001b[0m");
            Console.SetCursorPosition(left, top);
            Console.CursorVisible = true;
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

        // https://docs.microsoft.com/en-us/windows/console/setconsolemode
        private void EnableAnsiEscape()
        {
            var stdOut = NativeMethods.GetStdHandle(-11);
            if (stdOut == IntPtr.Zero)
                throw new InvalidOperationException("stdOut == NULL");

            // ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004
            NativeMethods.GetConsoleMode(stdOut, out var mode);
            mode |= 0x4;
            NativeMethods.SetConsoleMode(stdOut, mode);
        }

        private class NativeMethods
        {
            [DllImport("kernel32")]
            public static extern bool SetConsoleMode(IntPtr handle, uint mode);

            [DllImport("kernel32")]
            public static extern bool GetConsoleMode(IntPtr handle, out uint mode);

            [DllImport("kernel32")]
            public static extern IntPtr GetStdHandle(int stdHandle);
        }
    }
}
