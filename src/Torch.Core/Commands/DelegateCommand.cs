using System;

namespace Torch.Core.Commands
{
    public class DelegateCommand : ICommand
    {
        private readonly CommandExecuteDel _execute;
        private readonly CommandCanExecuteDel? _canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand"/> class.
        /// </summary>
        /// <param name="execute">A delegate that performs the command action.</param>
        /// <param name="canExecute">A delegate that checks if the command can be executed.</param>
        public DelegateCommand(CommandExecuteDel execute, CommandCanExecuteDel? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentException($"'{nameof(execute)}' cannot be null.");
            _canExecute = canExecute;
        }

        public static implicit operator DelegateCommand(CommandExecuteDel del) => new (del);

        /// <inheritdoc/>
        public void Execute(ICommandContext context)
        {
            _execute(context);
        }

        /// <inheritdoc/>
        public bool CanExecute(ICommandContext context, ref string reason)
        {
            return _canExecute?.Invoke(context, ref reason) ?? true;
        }
    }
}