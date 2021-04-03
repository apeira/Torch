using System.Collections.Generic;
using JetBrains.Annotations;

namespace Torch.Core
{
    /// <summary>
    /// Type representing torch.yaml. Intended to be serialized to and from YAML with YamlDotNet.
    /// </summary>
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class TorchConfiguration
    {
        /// <summary>
        /// Gets a list of paths to plugin directories that should be loaded.
        /// </summary>
        public List<string> Plugins { get; private set; }
        
        /// <summary>
        /// Gets whether JSON logging should be enabled.
        /// </summary>
        public bool LogToJson { get; private set; }
    }
}
