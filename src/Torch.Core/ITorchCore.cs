using System;

namespace Torch.Core
{
    /// <summary>
    /// Represents an object responsible for controlling the lifetime of the Torch application.
    /// </summary>
    public interface ITorchCore
    {
        /// <summary>
        /// Raised when <see cref="State"/> is set.
        /// </summary>
        event Action<CoreState>? StateChanged;

        /// <summary>
        /// Gets the current execution stage of the core.
        /// </summary>
        CoreState State { get; }

        /// <summary>
        /// Begins executing the core and stops on its own or after SignalStop is called.
        /// </summary>
        void Run();

        /// <summary>
        /// Signal the core to complete execution and stop.
        /// </summary>
        /// <param name="callback">A callback to invoke when the core has stopped.</param>
        void SignalStop(Action<ITorchCore>? callback = null);
    }
}
