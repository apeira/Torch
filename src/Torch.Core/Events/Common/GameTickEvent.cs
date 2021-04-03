namespace Torch.Core.Events.Common
{
    [PublicApi]
    public struct GameTickEvent : IEventData
    {
        public readonly long GameTick;
        public GameTickEvent(long gameTick)
        {
            GameTick = gameTick;
        }
    }
}