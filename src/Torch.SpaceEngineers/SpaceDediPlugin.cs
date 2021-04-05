using HarmonyLib;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Torch.Core;
using Torch.Core.Plugins;
using Torch.SpaceEngineers.Chat;

namespace Torch.SpaceEngineers
{
    /// <summary>
    /// Provides a core and supporting services for running a Space Engineers dedicated server under Torch.
    /// </summary>
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public class SpaceDediPlugin : Plugin
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpaceDediPlugin"/> class.
        /// </summary>
        /// <param name="chat">Game chat service.</param>
        public SpaceDediPlugin(ChatService chat)
        {
            // Forces services to be instantiated
        }

        /// <summary>
        /// Configure services and execute patches for this plugin.
        /// </summary>
        /// <param name="config">The current Torch environment.</param>
        /// <param name="patcher">A Harmony patcher instance.</param>
        public static void Configure(TorchEnvironment config, Harmony patcher)
        {
            config.Services.AddSingleton<ITorchCore, SpaceDediCore>();
            config.Services.UseCommands(true);
            config.Services.AddSingleton<ChatService>();
            patcher.PatchAll();
        }
    }
}
