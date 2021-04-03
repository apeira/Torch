namespace Torch.Core.Commands
{
    public class DelegateProcessorStep : IProcessorStep
    {
        private readonly CommandProcessorDel _processor;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateProcessorStep"/> class.
        /// </summary>
        /// <param name="processor">The processor delegate.</param>
        public DelegateProcessorStep(CommandProcessorDel processor)
        {
            _processor = processor;
        }

        public static implicit operator DelegateProcessorStep(CommandProcessorDel del) => new (del);

        /// <inheritdoc/>
        public void Process(ICommandContext context, NextProcessorDel next)
        {
            _processor(context, next);
        }
    }
}
