namespace Torch.Core.Commands
{
    /// <summary>
    /// Represents a command processor step as a delegate. See <see cref="IProcessorStep"/>.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <param name="next">The next command processor in the pipeline.</param>
    public delegate void CommandProcessorDel(ICommandContext context, NextProcessorDel next);
}
