using System.Linq;
using Torch.Core.Plugins;

namespace Torch.Core.Commands.Common
{
    public class PluginsCommand : ICommand
    {
        private IPluginService _plugins;

        public PluginsCommand(IPluginService plugins)
        {
            _plugins = plugins;
        }

        public void Execute(ICommandContext context)
        {
            var names = _plugins.Plugins.Select(x => FormatPluginName(x.Value));
            context.Respond($"Loaded plugins: {string.Join(", ", names)}");
        }

        public bool CanExecute(ICommandContext context, ref string reason) => true;

        private string FormatPluginName(Plugin plugin)
        {
            return $"{plugin.Info.Id} {plugin.Info.Version}";
        }
    }
}