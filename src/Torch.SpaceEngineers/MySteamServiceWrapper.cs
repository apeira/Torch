using System;
using System.Reflection;
using VRage.GameServices;

namespace Torch.SpaceEngineers
{
    /// <summary>
    ///     Provides static accessor for MySteamService because Keen made it internal
    /// </summary>
    public static class MySteamServiceWrapper
    {
        private static readonly MethodInfo _getGameService;

        static MySteamServiceWrapper()
        {
            var type = Type.GetType("VRage.Steam.MySteamService, VRage.Steam");
            var prop = type.GetProperty("Static", BindingFlags.Static | BindingFlags.Public);
            _getGameService = prop.GetGetMethod();
        }

        public static IMyGameService Static => (IMyGameService)_getGameService.Invoke(null, null);

        public static IMyGameService Init(bool dedicated, uint appId)
        {
            return (IMyGameService)Activator.CreateInstance(Type.GetType("VRage.Steam.MySteamService, VRage.Steam"), dedicated, appId);
        }
    }
}