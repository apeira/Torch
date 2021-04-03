namespace Torch.Core.Events.Common
{
    [PublicApi]
    public struct GameStateChangeEvent : IEventData
    {
        public readonly GameState NewState;
        public GameStateChangeEvent(GameState newState)
        {
            NewState = newState;
        }
    }

    [PublicApi]
    public enum GameState
    {
        BeforeLoad,
        AfterLoad,
        Running,
        BeforeStop,
        AfterStop
    }
}