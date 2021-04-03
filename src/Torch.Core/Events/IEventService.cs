namespace Torch.Core.Events
{
    /// <summary>
    /// Provides a loosely coupled system for raising and handling events.
    /// </summary>
    [PublicApi]
    public interface IEventService
    {
        /// <summary>
        /// Registers an event type. This can only be done once per type.
        /// </summary>
        /// <typeparam name="T">The event type.</typeparam>
        /// <returns>An <see cref="IEventOwner{T}"/> that can be used to raise the event.</returns>
        IEventOwner<T> Register<T>() where T : struct, IEventData;
        
        /// <summary>
        /// Subscribes to a registered event. When the event is raised through <see cref="IEventOwner{T}.Raise"/>,
        /// all subscribers are called in the order they subscribed.
        /// </summary>
        /// <param name="handler">The event handler to subscribe.</param>
        /// <typeparam name="T">The event type.</typeparam>
        /// <returns>An object that can be used to unsubscribe from the event.</returns>
        IEventSubscriber Subscribe<T>(EventHandler<T> handler) where T : struct, IEventData;
        
        /// <summary>
        /// Unsubscribes the given <see cref="IEventSubscriber"/> from the event it was subscribed to.
        /// </summary>
        /// <param name="subscriber">The subscriber to unsubscribe.</param>
        void Unsubscribe(IEventSubscriber subscriber);
    }
}