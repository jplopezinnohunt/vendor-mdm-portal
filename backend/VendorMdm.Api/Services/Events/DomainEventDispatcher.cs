using System.Collections.Concurrent;
using VendorMdm.Core.Framework.Events;

namespace VendorMdm.Api.Services.Events;

/// <summary>
/// Default implementation of IDomainEventDispatcher.
/// Resolves handlers from DI and dispatches events to all registered handlers.
/// </summary>
public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventDispatcher> _logger;
    private static readonly ConcurrentDictionary<Type, Type> _handlerTypeCache = new();

    public DomainEventDispatcher(
        IServiceProvider serviceProvider,
        ILogger<DomainEventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DispatchAsync(IEnumerable<object> events, CancellationToken cancellationToken = default)
    {
        foreach (var @event in events)
        {
            if (@event == null) continue;

            try
            {
                await DispatchSingleEventAsync(@event, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dispatching event {EventType}", @event.GetType().Name);
                // Continue dispatching remaining events
            }
        }
    }

    public async Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        if (@event == null) return;

        await DispatchSingleEventAsync(@event, cancellationToken);
    }

    private async Task DispatchSingleEventAsync(object @event, CancellationToken cancellationToken)
    {
        var eventType = @event.GetType();
        var handlerType = GetHandlerType(eventType);

        _logger.LogInformation(
            "Dispatching event {EventType} at {Timestamp}",
            eventType.Name,
            DateTime.UtcNow);

        using var scope = _serviceProvider.CreateScope();
        var handlers = scope.ServiceProvider.GetServices(handlerType);

        var handlerCount = 0;
        foreach (var handler in handlers)
        {
            if (handler == null) continue;

            try
            {
                handlerCount++;
                _logger.LogDebug(
                    "Invoking handler {HandlerType} for event {EventType}",
                    handler.GetType().Name,
                    eventType.Name);

                // Use reflection to call HandleAsync with the correct event type
                var handleMethod = handler.GetType()
                    .GetMethod("HandleAsync", new[] { eventType, typeof(CancellationToken) });

                if (handleMethod != null)
                {
                    var task = (Task?)handleMethod.Invoke(handler, new[] { @event, cancellationToken });
                    if (task != null)
                    {
                        await task;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Handler {HandlerType} failed processing event {EventType}",
                    handler.GetType().Name,
                    eventType.Name);
                // Continue with next handler
            }
        }

        _logger.LogInformation(
            "Event {EventType} dispatched to {HandlerCount} handlers",
            eventType.Name,
            handlerCount);
    }

    private static Type GetHandlerType(Type eventType)
    {
        return _handlerTypeCache.GetOrAdd(eventType, type =>
            typeof(IEventHandler<>).MakeGenericType(type));
    }
}
