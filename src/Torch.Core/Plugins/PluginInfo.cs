using YamlDotNet.Serialization;
using Version = SemVer.Version;

namespace Torch.Core.Plugins
{
    /// <summary>
    /// Describes a plugin. Intended for serialization to/from YAML with <see cref="YamlDotNet"/>.
    /// </summary>
    public class PluginInfo
    {
        /// <summary>
        /// Gets the plugin's ID.
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// Gets the loaded version of the plugin.
        /// </summary>
        public Version Version { get; internal set; }

        /// <summary>
        /// Gets the name of the DLL file that contains the <see cref="Plugin"/> subclass for this plugin.
        /// </summary>
        public string EntryPoint { get; internal set; }

        /// <summary>
        /// Gets the root folder of the plugin.
        /// </summary>
        [YamlIgnore]
        public string BasePath { get; internal set; }
    }
}
