namespace VendorMdm.Core.Framework.Events;

/// <summary>
/// Handles a specific type of domain event.
/// Multiple handlers can be registered for the same event type.
/// Handlers are resolved from DI and executed by the IDomainEventDispatcher.
/// </summary>
/// <typeparam name="TEvent">The type of event this handler processes</typeparam>
public interface IEventHandler<in TEvent> where TEvent : class
{
    /// <summary>
    /// Handles the domain event asynchronously.
    /// </summary>
    /// <param name="event">The domain event to handle</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}

/// <summary>
/// Marker interface for event handlers.
/// Used for DI registration and discovery.
/// </summary>
public interface IEventHandler
{
}
