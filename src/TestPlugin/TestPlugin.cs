using System;
using HarmonyLib;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Torch.Core;
using Torch.Core.Commands;
using Torch.Core.Permissions;
using Torch.Core.Plugins;

/*
 * Plugins should have the following file/folder structure:
 * [plugin folder]
 *   - info.yaml
 *   - bin
 *     - native (if applicable)
 *       - native DLL files
 *     - .NET DLL files
 *
 * info.yaml
 *   id: 'your-plugin-id'
 *   version: '1.0.3-beta' (just an example)
 *   entryPoint: 'MyPluginImplementation.dll'
 *
 * To easily load a plugin for debugging, simply add the path to your plugin folder to torch.yaml's plugin list.
 */

namespace TestPlugin
{
    /// <summary>
    /// The entry point of the plugin. Any plugin must define exactly one subtype of <see cref="Plugin"/>
    /// to be valid.
    /// </summary>
    public class TestPlugin : Plugin
    {
        /// <summary>
        /// This method should be used to configure the Torch instance before it starts running. This can include
        /// modifying the logger configuration or service provider. Defining this method is optional.
        /// </summary>
        /// <param name="env">The Torch environment configuration.</param>
        /// <param name="patcher">The Harmony patcher instance for this plugin.</param>
        [UsedImplicitly]
        public static void Configure(TorchEnvironment env, Harmony patcher)
        {
            Console.WriteLine("TEST PLUGIN - Configure");
            env.Services.AddSingleton<ITorchCore, TestCore>();
            env.Services.UseCommands(true);
            env.Services.UsePermissions();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestPlugin"/> class.
        /// The plugin constructor can receive services by dependency injection.
        /// </summary>
        public TestPlugin(ICommandService commands, IPermissionService permissions)
        {
            Console.WriteLine("TEST PLUGIN - Constructor");
            commands.RegisterDelegate("ping", ctx => ctx.Respond("pong"));
            
            permissions.SetDefaultAccess(PermissionModifier.Deny);
            permissions.GetPermissions("misc", "console").AddExpression(new PermissionExpression("cmd.ping"));
            commands.AddProcessorStep((ctx, next) =>
            {
                if (ctx.Sender is ConsoleService)
                {
                    var node = $"cmd.{ctx.CommandName}";

                    if (!permissions.IsAllowed("misc", "console", node))
                    {
                        ctx.Respond($"You do not have permission to use this command (node {node}).");
                        return;
                    }
                }

                next(ctx);
            });
        }
    }
}
