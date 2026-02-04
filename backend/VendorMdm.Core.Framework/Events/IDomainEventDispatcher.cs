namespace VendorMdm.Core.Framework.Events;

/// <summary>
/// Dispatches domain events to registered handlers.
/// Events collected by domain concepts are dispatched after successful persistence.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatches a collection of domain events to all registered handlers.
    /// Handlers are resolved from DI and executed in sequence.
    /// </summary>
    /// <param name="events">The domain events to dispatch</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task DispatchAsync(IEnumerable<object> events, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatches a single domain event to all registered handlers.
    /// </summary>
    /// <typeparam name="TEvent">The event type</typeparam>
    /// <param name="event">The domain event to dispatch</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class;
}
