using Microsoft.Extensions.DependencyInjection;
using Torch.Core.Commands;
using Torch.Core.Permissions;

namespace Torch.Core
{
    /// <summary>
    /// Extensions to simplify adding core services to a Torch configuration.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the default command service and common commands. Optionally enables an interactive console
        /// interface to input commands.
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="enableInteractiveConsole"></param>
        public static void UseCommands(this IServiceCollection serviceCollection, bool enableInteractiveConsole)
        {
            if (enableInteractiveConsole)
                serviceCollection.AddSingleton<ConsoleService>();
            
            serviceCollection.AddSingleton<ICommandService>(provider =>
            {
                var commands = new CommandService(provider);
                commands.RegisterTransient<StopCommand>("stop");
                commands.RegisterTransient<PluginsCommand>("plugins");
                provider.GetService<ConsoleService>()?.AttachCommandService(commands);
                return commands;
            });
        }

        /// <summary>
        /// Adds the default permission service.
        /// </summary>
        /// <param name="serviceCollection"></param>
        public static void UsePermissions(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IPermissionService, PermissionService>();
        }
    }
}