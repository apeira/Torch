using System;

namespace Torch.Core
{
    /// <summary>
    /// Represents an object responsible for controlling the lifetime of the Torch application.
    /// </summary>
    public interface ITorchCore
    {
        event Action<CoreState> StateChanged;
        
        CoreState State { get; }

        /// <summary>
        /// Begins executing the core and stops on its own or after SignalStop is called.
        /// </summary>
        void Run();

        /// <summary>
        /// Signal the core to complete execution and stop.
        /// </summary>
        void SignalStop(Action<ITorchCore> callback = null);
    }

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
