using System;
using System.Reflection;
using HarmonyLib;
using NLog;

namespace Torch.SpaceEngineers.Patches
{
    /// <summary>
    /// Skips the GALogger initialization code because it overwrites our NLog configuration.
    /// </summary>
    [HarmonyPatch]
    public class GALoggerConstructorPatch
    {
        private const string TYPE_NAME = "GameAnalyticsSDK.Net.Logging.GALogger";

        private static MethodBase TargetMethod()
        {
            Assembly.LoadFrom("GameAnalytics.Mono.dll");
            return AccessTools.Constructor(AccessTools.TypeByName(TYPE_NAME));
        }

        private static bool Prefix()
        {
            AccessTools.Field(AccessTools.TypeByName(TYPE_NAME), "logger").SetValue(null, LogManager.GetLogger(TYPE_NAME));
            return false;
        }
    }
}
