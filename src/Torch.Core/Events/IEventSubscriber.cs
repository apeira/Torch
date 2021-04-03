using System;

// TODO create an awaitable event subscriber e.g. subscriber.GetAwaiter()

namespace Torch.Core.Events
{
    /// <summary>
    /// Represents a subscriber of an event from the <see cref="IEventService"/>.
    /// </summary>
    [PublicApi]
    public interface IEventSubscriber
    {
        /// <summary>
        /// The event type this subscriber is subscribed to.
        /// </summary>
        Type EventType { get; }
    }
}