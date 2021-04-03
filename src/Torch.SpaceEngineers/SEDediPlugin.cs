using System.IO;
using HarmonyLib;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using SpaceEngineers.Game;
using Torch.Core;
using Torch.Core.Plugins;
using Torch.SpaceEngineers.Chat;

namespace Torch.SpaceEngineers
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    // ReSharper disable once InconsistentNaming
    public class SEDediPlugin : Plugin
    {
        public static void Configure(TorchEnvironment config, Harmony patcher)
        {
            config.UserDataPath = Directory.GetParent(typeof(SpaceEngineersGame).Assembly.Location).Parent.FullName;
            config.Services.AddSingleton<ITorchCore, SEDediCore>();
            config.Services.AddSingleton<ChatService>();
            patcher.PatchAll();
        }
    }
}
