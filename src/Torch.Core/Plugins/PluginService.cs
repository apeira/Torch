using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Torch.Core.Plugins
{
    /// <inheritdoc />
    public class PluginService : IPluginService
    {
        private readonly Dictionary<string, Plugin> _plugins = new();
        private readonly List<(Type, PluginInfo)> _pluginsToActivate = new();
        private readonly Dictionary<string, Harmony> _patchers = new();
        private readonly IDeserializer _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithTypeConverter(new YamlVersionConverter())
            .Build();

        private bool _loaded;
        private bool _activated;

        /// <inheritdoc />
        public IReadOnlyDictionary<string, Plugin> Plugins => _plugins;

        /// <summary>
        /// Gets the Harmony patchers by plugin ID.
        /// </summary>
        public IReadOnlyDictionary<string, Harmony> Patchers => _patchers;

        /// <summary>
        /// Loads a plugin directly by DLL path with dummy metadata. Only for testing/debugging.
        /// </summary>
        /// <param name="asm">An assembly containing a <see cref="Plugin"/> subclass.</param>
        /// <param name="env">The current Torch environment.</param>
        [Conditional("DEBUG")]
        public void LoadPluginDebug(Assembly asm, TorchEnvironment env)
        {
            var pluginType = asm.GetTypes().SingleOrDefault(x => x.IsSubclassOf(typeof(Plugin)))!;
            var info = new PluginInfo
            {
                EntryPoint = asm.Location,
                Id = pluginType.FullDescription(),
                Version = new SemVer.Version("0.0.0-debug"),
            };

            ConfigurePlugin(info.Id, pluginType, env);
            _pluginsToActivate.Add((pluginType, info));
        }

        /// <summary>
        /// Loads plugins from a list of plugin folders. Folder structure must be the standard plugin
        /// format.
        /// </summary>
        /// <param name="pluginFolderPaths">The set of plugin folders to load from.</param>
        /// <param name="env">The current Torch environment.</param>
        /// <exception cref="PluginLoadException">The plugin entry DLL is doesn't exist or doesn't
        /// contain a <see cref="Plugin"/> subclass.</exception>
        public void LoadPlugins(IEnumerable<string> pluginFolderPaths, TorchEnvironment env)
        {
            if (_loaded)
                throw new PluginLoadException("Plugins have already been loaded.");

            _loaded = true;

            foreach (var pluginFolderPath in pluginFolderPaths)
            {
                var info = GetPluginInfo(pluginFolderPath);
                var pluginEntryDllPath = Path.Combine(pluginFolderPath, "bin", info.EntryPoint);

                if (!File.Exists(pluginEntryDllPath))
                    throw new PluginLoadException($"The file '{pluginEntryDllPath}' does not exist.");

                var asm = Assembly.LoadFrom(pluginEntryDllPath);
                var pluginType = asm.GetTypes().SingleOrDefault(x => x.IsSubclassOf(typeof(Plugin)));

                if (pluginType == null)
                {
                    throw new PluginLoadException($"The plugin '{info.Id}' does not have exactly one entry point.");
                }

                ConfigurePlugin(info.Id, pluginType, env);
                _pluginsToActivate.Add((pluginType, info));
            }
        }

        /// <summary>
        /// Instantiates the plugins loaded with <see cref="PluginService.LoadPlugins"/> using dependency
        /// injection.
        /// </summary>
        /// <param name="serviceProvider">The service provider to use to activate plugin objects.</param>
        /// <exception cref="PluginLoadException">A plugin constructor threw an exception.</exception>
        public void ActivatePlugins(IServiceProvider serviceProvider)
        {
            if (_activated)
                throw new PluginLoadException("Plugins have already been activated.");

            _activated = true;

            foreach (var (pluginType, pluginInfo) in _pluginsToActivate)
            {
                var pluginId = pluginInfo.Id;
                try
                {
                    var plugin = (Plugin)ActivatorUtilities.CreateInstance(serviceProvider, pluginType);
                    plugin.Info = pluginInfo;
                    _plugins.Add(pluginId, plugin);
                }
                catch (Exception ex)
                {
                    throw new PluginLoadException($"The plugin '{pluginId}' failed to activate.", ex);
                }
            }
        }

        private PluginInfo GetPluginInfo(string pluginFolderPath)
        {
            var infoFilePath = Path.Combine(pluginFolderPath, "info.yaml");
            if (!File.Exists(infoFilePath))
                throw new PluginLoadException($"The plugin located at '{pluginFolderPath}' does not have a valid info.yaml file.");

            using var file = File.OpenRead(infoFilePath);
            using var reader = new StreamReader(file);
            return _yamlDeserializer.Deserialize<PluginInfo>(reader);
        }

        /// <summary>
        /// Searches for a static Plugin.Configure(TorchEnvironment, Harmony) method and invokes it if found.
        /// </summary>
        /// <param name="pluginId">The plugin ID.</param>
        /// <param name="entryType">The plugin entry type (subclass of <see cref="Plugin"/>).</param>
        /// <param name="env">The Torch environment.</param>
        /// <exception cref="PluginLoadException">The Configure method threw an exception.</exception>
        private void ConfigurePlugin(string pluginId, Type entryType, TorchEnvironment env)
        {
            var configureMethod = entryType.GetMethod("Configure", BindingFlags.Public | BindingFlags.Static);
            if (configureMethod == null)
                return;

            try
            {
                var patcher = new Harmony(pluginId);
                _patchers[pluginId] = patcher;
                configureMethod.Invoke(null, new object[] { env, patcher });
            }
            catch (Exception ex)
            {
                throw new PluginLoadException($"The plugin '{pluginId}' threw an exception while configuring.", ex);
            }
        }
    }
}
