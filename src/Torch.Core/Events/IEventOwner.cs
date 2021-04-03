namespace Torch.Core.Events
{
    /// <summary>
    /// Represents an object capable of raising an event through the event system.
    /// </summary>
    [PublicApi]
    public interface IEventOwner { }
    
    /// <inheritdoc />
    [PublicApi]
    public interface IEventOwner<T> : IEventOwner where T : struct
    {
        /// <summary>
        /// Raise the event with the provided data, causing all subscribers to be invoked in the order they
        /// subscribed.
        /// </summary>
        /// <param name="eventData">The event data sent to subscribers.</param>
        void Raise(ref T eventData);

        void Raise(T eventData);
    }
}