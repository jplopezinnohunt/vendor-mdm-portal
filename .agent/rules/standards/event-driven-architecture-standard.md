# Event-Driven Architecture Standard

**Category**: Core Development
**Pattern #**: 8
**Status**: MANDATORY (FULLY IMPLEMENTED 2026-02-04)
**Priority**: 🟠 IMPORTANT

### Pattern Overview
Domain Events enable async workflows, decouple side effects from core business logic, and provide real-time frontend updates via SignalR.

---

## Implementation Status

| Component | Status | Location |
|-----------|--------|----------|
| Event Collection (Concepts) | ✅ Complete | `VendorConcept.cs` |
| Event Types | ✅ Complete | `Core.Framework/Events/DomainEvents.cs` |
| Event Dispatcher | ✅ Complete | `Api/Services/Events/DomainEventDispatcher.cs` |
| Event Handlers | ✅ Complete | `Api/Services/Events/SignalREventHandler.cs` |
| SignalR Hub | ✅ Complete | `Api/Hubs/EventHub.cs` |
| Outbox Pattern | ✅ Complete | `Shared/Models/OutboxEvent.cs` |
| Outbox Processor | ✅ Complete | `Api/Services/Events/OutboxProcessor.cs` |
| Frontend Context | ✅ Complete | `frontend/src/context/SignalRContext.tsx` |
| Frontend Hooks | ✅ Complete | `frontend/src/hooks/useSignalR.ts` |

---

## Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         EVENT FLOW ARCHITECTURE                         │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐   │
│  │  Domain Concept │────>│  Domain Event   │────>│  Event          │   │
│  │  or Service     │     │  Dispatcher     │     │  Handlers       │   │
│  └─────────────────┘     └────────┬────────┘     └────────┬────────┘   │
│                                   │                       │             │
│                          ┌────────┴────────┐              │             │
│                          ▼                 ▼              ▼             │
│                   ┌──────────┐     ┌──────────┐   ┌─────────────┐       │
│                   │  Outbox  │     │  Cosmos  │   │  SignalR    │       │
│                   │  (SQL)   │     │  Events  │   │  Handler    │       │
│                   └────┬─────┘     └──────────┘   └──────┬──────┘       │
│                        │                                 │              │
│                        ▼                                 ▼              │
│                   ┌──────────┐                    ┌─────────────┐       │
│                   │  Outbox  │                    │  EventHub   │       │
│                   │ Processor│                    │ /hubs/events│       │
│                   └────┬─────┘                    └──────┬──────┘       │
│                        │                                 │              │
│                        ▼                                 │ WebSocket    │
│                   ┌──────────┐                           ▼              │
│                   │ Service  │                    ┌─────────────┐       │
│                   │   Bus    │                    │  Frontend   │       │
│                   └──────────┘                    │  React App  │       │
│                                                   └─────────────┘       │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Implementation Details

### 1. Event Collection (Concepts)
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

### 2. Event Types
```csharp
// VendorMdm.Core.Framework/Events/DomainEvents.cs
public abstract class DomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
}

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

### 3. Event Dispatcher Interface
```csharp
// VendorMdm.Core.Framework/Events/IDomainEventDispatcher.cs
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<object> events, CancellationToken ct = default);
    Task DispatchAsync<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : class;
}
```

### 4. Event Handler Interface
```csharp
// VendorMdm.Core.Framework/Events/IEventHandler.cs
public interface IEventHandler<in TEvent> where TEvent : class
{
    Task HandleAsync(TEvent @event, CancellationToken ct = default);
}
```

### 5. SignalR Event Handler
```csharp
// VendorMdm.Api/Services/Events/SignalREventHandler.cs
public class SignalREventHandler :
    IEventHandler<VendorCreatedEvent>,
    IEventHandler<VendorStatusChangedEvent>
{
    private readonly IHubContext<EventHub> _hubContext;

    public async Task HandleAsync(VendorStatusChangedEvent @event, CancellationToken ct)
    {
        await _hubContext.SendStatusChangedAsync(
            "Vendor", @event.VendorId.ToString(), @event.OldStatus, @event.NewStatus);
    }
}
```

### 6. Outbox Pattern
```csharp
// Usage in Services
_context.Vendors.Add(vendor);
_context.AddToOutbox(vendorCreatedEvent);  // Same transaction = guaranteed delivery
await _context.SaveChangesAsync();
await _dispatcher.DispatchAsync(vendorCreatedEvent);  // In-process handlers
```

### 7. Frontend Integration
```typescript
// React hook usage
import { useStatusChanged, useNotifications } from '../hooks/useSignalR';

// In component
useStatusChanged((event) => {
  toast.info(`Status changed: ${event.oldStatus} → ${event.newStatus}`);
  refetchData();
});
```

---

## SignalR Events Reference

| Event Name | Payload | Target |
|------------|---------|--------|
| `VendorCreated` | vendorId, legalName, accountGroup | all |
| `StatusChanged` | entityType, entityId, oldStatus, newStatus | all |
| `VendorStatusChanged` | vendorId, oldStatus, newStatus | vendor:{id} |
| `TaskAssigned` | taskType, entityId, description | user:{id} |
| `Notification` | title, message, link | user:{id} |
| `SapSyncResult` | vendorId, success, sapVendorNumber, error | user:{id} |

---

## DI Registration (Program.cs)

```csharp
// Event-Driven Architecture
builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
builder.Services.AddScoped<IEventHandler<VendorCreatedEvent>, SignalREventHandler>();
builder.Services.AddScoped<IEventHandler<VendorStatusChangedEvent>, SignalREventHandler>();
builder.Services.AddHostedService<OutboxProcessor>();
builder.Services.AddSignalR();

// In app pipeline
app.MapHub<EventHub>("/hubs/events");
```

---

## Benefits
- ✅ **Decoupling**: Side effects separate from business logic
- ✅ **Async**: Non-blocking event processing
- ✅ **Real-Time**: Frontend updates via SignalR
- ✅ **Guaranteed Delivery**: Outbox pattern prevents event loss
- ✅ **Extensibility**: Add handlers without changing core logic
- ✅ **Audit Trail**: All events logged to Cosmos DB

**Compliance**: 100% IMPLEMENTED
