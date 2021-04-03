using Torch.Core;

namespace Torch.SpaceEngineers.Players
{
    public static class PlayerExtensions
    {
        public static ulong SteamId(this IPlayer player) => ((Player)player).SteamId;
    }
}