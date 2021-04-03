using System;

namespace Torch.Core.Events
{
    public delegate void EventHandler<T>(ref T eventData) where T : struct;
    
    internal class EventSubscriber<T> : IEventSubscriber where T : struct
    {
        private readonly EventHandler<T> _handler;
        
        public Type EventType => typeof(T);

        public EventSubscriber(EventHandler<T> handler)
        {
            _handler = handler;
        }
        
        public void Invoke(ref T eventData)
        {
            _handler.Invoke(ref eventData);
        }
    }
}