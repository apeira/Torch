namespace Torch.Core.Commands
{
    public interface ICommand
    {
        /// <summary>
        /// Executes the command with the given command context. This should only be called if <see cref="CanExecute"/>
        /// returns <c>true</c>
        /// </summary>
        /// <param name="context">The execution context for this command.</param>
        void Execute(ICommandContext context);

        /// <summary>
        /// Checks whether it is currently valid to <see cref="Execute"/> this command with the given <see cref="ICommandContext"/>.
        /// </summary>
        /// <param name="context">The execution context for this command.</param>
        /// <param name="reason">The reason that this command can't be executed (if applicable).</param>
        /// <returns>True if the command can be executed, otherwise false.</returns>
        bool CanExecute(ICommandContext context, ref string reason);
    }
}
