using System;

namespace Torch.Core.Plugins
{
    /// <summary>
    /// An exception thrown when a plugin fails to load.
    /// </summary>
    [Serializable]
    public class PluginLoadException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginLoadException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public PluginLoadException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginLoadException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="inner">The exception thrown during plugin loading.</param>
        public PluginLoadException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
