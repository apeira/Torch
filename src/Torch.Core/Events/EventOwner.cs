using System.Collections.Generic;
using NLog;
using Torch.Core.Extensions;

namespace Torch.Core.Events
{
    internal abstract class EventOwner : IEventOwner
    {
        protected static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public abstract void RemoveSubscriber(IEventSubscriber subscriber);
    }
    
    internal sealed class EventOwner<T> : EventOwner, IEventOwner<T> where T : struct, IEventData
    {
        private readonly List<EventSubscriber<T>> _subscribers = new List<EventSubscriber<T>>();

        public void Raise(T data) => Raise(ref data);
        
        /// <inheritdoc />
        public void Raise(ref T eventData)
        {
            Log.Trace($"Raising event {typeof(T).FullyQualifiedName()} to {_subscribers.Count} subscribers.");
            Log.Trace($"Event data: {eventData.ToString()}");
            foreach (var subscriber in _subscribers)
                subscriber.Invoke(ref eventData);
        }

        public void AddSubscriber(EventSubscriber<T> subscriber)
        {
            _subscribers.Add(subscriber);
        }

        public override void RemoveSubscriber(IEventSubscriber subscriber)
        {
            _subscribers.Remove((EventSubscriber<T>)subscriber);
        }
    }
}