using Torch.Core;

namespace Torch.SpaceEngineers.Players
{
    public class Player : IPlayer
    {
        public string Name { get; set; }
        
        public ulong SteamId { get; set; }
    }
}