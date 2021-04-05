namespace Torch.Core.Plugins
{
    public abstract class Plugin
    {
        /// <summary>
        /// Provides metadata about a plugin.
        /// <remarks>This is assigned a non-null value immediately when <see cref="Plugin"/>
        /// is instantiated in <see cref="PluginService.ActivatePlugins"/></remarks>
        /// </summary>
        public PluginInfo Info { get; internal set; } = null!;
    }
}
