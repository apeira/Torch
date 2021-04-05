using System.Collections.Generic;

namespace Torch.Core.Plugins
{
    /// <summary>
    /// Handles configuring and loading Torch plugins.
    /// </summary>
    public interface IPluginService
    {
        /// <summary>
        /// Gets currently active plugins.
        /// </summary>
        IReadOnlyDictionary<string, Plugin> Plugins { get; }
    }
}
