namespace Torch.Core.Commands
{
    public interface IProcessorStep
    {
        /// <summary>
        /// Called in sequence with other <see cref="IProcessorStep"/>s to control how a command
        /// is handled. This method should invoke '<see cref="next"/>' to continue processing the command.
        /// To abort the command, do not invoke '<see cref="next"/>'.
        /// </summary>
        /// <param name="context">The command context.</param>
        /// <param name="next">Invokes the next <see cref="IProcessorStep"/> in the pipeline.</param>
        public void Process(ICommandContext context, NextProcessorDel next);
    }
}
