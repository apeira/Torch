using System;
using System.Collections.Generic;

namespace Torch.Core.Events
{
    /// <summary>
    /// <inheritdoc />
    /// This implementation is not thread safe.
    /// </summary>
    internal class EventService : IEventService
    {
        private readonly Dictionary<Type, EventOwner> _eventOwners = new Dictionary<Type, EventOwner>();

        private EventOwner<T> GetOrCreateOwner<T>() where T : struct, IEventData
        {
            if (_eventOwners.TryGetValue(typeof(T), out var owner))
                return (EventOwner<T>)owner;
            
            owner = new EventOwner<T>();
            _eventOwners.Add(typeof(T), owner);
            return (EventOwner<T>)owner;
        }
        
        /// <inheritdoc />
        public IEventOwner<T> Register<T>() where T : struct, IEventData
        {
            return GetOrCreateOwner<T>();
        }

        /// <inheritdoc />
        public IEventSubscriber Subscribe<T>(EventHandler<T> handler) where T : struct, IEventData
        {
            var owner = GetOrCreateOwner<T>();
            var subscriber = new EventSubscriber<T>(handler);
            owner.AddSubscriber(subscriber);
            return subscriber;
        }

        /// <inheritdoc />
        public void Unsubscribe(IEventSubscriber subscriber)
        {
            GetOwner(subscriber.EventType).RemoveSubscriber(subscriber);
        }
        
        private EventOwner GetOwner(Type eventType)
        {
            if (!_eventOwners.TryGetValue(eventType, out var owner))
                throw new InvalidOperationException($"No event defined by the type '{eventType.AssemblyQualifiedName}'.");

            return owner;
        }
    }
}