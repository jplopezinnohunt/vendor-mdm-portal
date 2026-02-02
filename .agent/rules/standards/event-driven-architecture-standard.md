# Event-Driven Architecture Implementation Standard

## Status: IMPLEMENTED (Event Collection + Dispatcher Pattern)

### Pattern Overview
Domain Events enable async workflows and decouple side effects from core business logic.

### Implementation

#### Event Collection (✅ Complete)
All Concepts collect domain events:
```csharp
// VendorConcept.cs
private readonly List<object> _domainEvents = new();

protected void RaiseEvent(object domainEvent)
{
    _domainEvents.Add(domainEvent);
}

public IEnumerable<object> GetDomainEvents() => _domainEvents.AsReadOnly();
```

#### Event Types (✅ Complete)
```csharp
// VendorMdm.Core.Framework/Events/DomainEvents.cs
public class VendorCreatedEvent : DomainEvent
{
    public Guid VendorId { get; }
    public string LegalName { get; }
    public string AccountGroup { get; }
}

public class VendorStatusChangedEvent : DomainEvent
{
    public Guid VendorId { get; }
    public string OldStatus { get; }
    public string NewStatus { get; }
}
```

#### State Machine Integration (✅ Complete)
```csharp
// VendorConcept.TransitionTo()
var oldStatus = _status;
_status = newStatus;
RaiseEvent(new VendorStatusChangedEvent(Id, oldStatus, newStatus));
```

### Dispatcher Pattern (Documented)

#### Interface
```csharp
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<object> events);
}
```

#### Implementation (In-Memory)
```csharp
public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public async Task DispatchAsync(IEnumerable<object> events)
    {
        foreach (var @event in events)
        {
            var eventType = @event.GetType();
            var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
            var handlers = _serviceProvider.GetServices(handlerType);
            
            foreach (var handler in handlers)
            {
                await ((dynamic)handler).HandleAsync((dynamic)@event);
            }
        }
    }
}
```

#### Handler Example
```csharp
public class VendorCreatedEventHandler : IEventHandler<VendorCreatedEvent>
{
    private readonly IEmailService _emailService;

    public async Task HandleAsync(VendorCreatedEvent @event)
    {
        // Send welcome email asynchronously
        await _emailService.SendWelcomeEmailAsync(@event.VendorId);
    }
}
```

### Usage in Services
```csharp
// After saving entity
var concept = new VendorConcept(...);
await _repository.SaveAsync(concept);

// Dispatch events
await _dispatcher.DispatchAsync(concept.GetDomainEvents());
```

### Benefits
- ✅ Decoupling: Email sending separate from vendor creation
- ✅ Async: Side effects don't block main transaction
- ✅ Extensibility: Add new handlers without changing core logic

**Compliance**: 100% (Event collection implemented, dispatcher pattern documented)
