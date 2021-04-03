using Torch.Core;

namespace Torch.SpaceEngineers.Players
{
    public class Player : IPlayer
    {
        public string Name { get; set; } = string.Empty;
        
        public ulong SteamId { get; set; }
    }
}