namespace Torch.Core
{
    /// <summary>
    /// Represents the core's stage in execution.
    /// </summary>
    public enum CoreState
    {
        /// <summary>
        /// The core is about to start.
        /// </summary>
        BeforeStart,

        /// <summary>
        /// The core has started.
        /// </summary>
        AfterStart,

        /// <summary>
        /// The core is about to stop.
        /// </summary>
        BeforeStop,

        /// <summary>
        /// The core has stopped.
        /// </summary>
        AfterStop,
    }
}
