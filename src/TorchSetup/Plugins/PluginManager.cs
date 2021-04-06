using System;
using System.Collections.Generic;
using System.IO;
using SemVer;
using Version = SemVer.Version;

namespace TorchSetup.Plugins
{
    public class PluginManager
    {
        private string _pluginDir;
        private List<PluginSpecification> _currentPlugins;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginManager"/> class.
        /// </summary>
        /// <param name="localPluginDir"></param>
        /// <param name="torchLoadOrderFile"></param>
        public PluginManager(string localPluginDir, string torchLoadOrderFile)
        {
            if (!Directory.Exists(localPluginDir))
                throw new DirectoryNotFoundException(localPluginDir);

            if (!File.Exists(torchLoadOrderFile))
                throw new FileNotFoundException(torchLoadOrderFile);

            _pluginDir = localPluginDir;
            _currentPlugins = new List<PluginSpecification>();

            foreach (var dir in File.ReadLines(torchLoadOrderFile))
            {
                var pluginDir = Path.Combine(_pluginDir, dir);
                var specFile = Path.Combine(pluginDir, "plugin.yaml");
                if (!Directory.Exists(pluginDir))
                    _currentPlugins.Add(PluginSpecification.Load(specFile));
            }
        }

        public void InstallPlugin(string pluginId, Range allowedVersions)
        {
            
        }
    }
}
