# Specification: Event-Driven Architecture Completion

**Version**: 1.0.0
**Date**: 2026-02-04
**Author**: Agent
**Status**: APPROVED (Auto-approved per user directive)

---

## Compliance Sidebar

| Standard | Section | Compliance |
|----------|---------|------------|
| [event-driven-architecture-standard.md](.agent/rules/standards/event-driven-architecture-standard.md) | Dispatcher Pattern | IMPLEMENTING |
| [hexagonal-architecture-standards.md](.agent/rules/standards/hexagonal-architecture-standards.md) | Outbound Port (Event Bus) | IMPLEMENTING |
| [moderngoldenrules.md](.agent/rules/moderngoldenrules.md) | Section 3: Async Side-Effects | IMPLEMENTING |
| [moderngoldenrules.md](.agent/rules/moderngoldenrules.md) | Section 10.2 Pattern 6: Event Sourcing | IMPLEMENTING |

---

## 1. Problem Statement

The current EDA implementation has **strong foundations** but **critical gaps**:

1. **Events are collected but never dispatched** - `VendorConcept.GetDomainEvents()` is never called
2. **No real-time frontend updates** - Frontend polls API, no push mechanism
3. **No guaranteed delivery** - Events can be lost if service crashes after DB commit
4. **No event handler infrastructure** - `IEventHandler<T>` not implemented

---

## 2. Requirements

### 2.1 Functional Requirements

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-01 | Implement `IDomainEventDispatcher` that dispatches events to registered handlers | P0 |
| FR-02 | Implement `IEventHandler<T>` infrastructure with DI registration | P0 |
| FR-03 | Implement SignalR Hub for real-time frontend notifications | P0 |
| FR-04 | Implement Outbox Pattern for guaranteed event delivery | P1 |
| FR-05 | Wire event dispatch in all service save operations | P0 |
| FR-06 | Implement frontend React hooks for SignalR connection | P0 |
| FR-07 | Push events: StatusChanged, WorkflowTask, SapSync, Notification | P0 |

### 2.2 Non-Functional Requirements

| ID | Requirement | Target |
|----|-------------|--------|
| NFR-01 | Event dispatch latency | < 50ms in-process |
| NFR-02 | SignalR message delivery | < 100ms to connected clients |
| NFR-03 | Outbox processing interval | 5 seconds |
| NFR-04 | Zero event loss for critical events | 100% with outbox |

---

## 3. Architecture Design

### 3.1 Component Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         BACKEND (VendorMdm.Api)                         │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐   │
│  │  Domain Concept │────>│  Domain Event   │────>│  Event          │   │
│  │  (VendorConcept)│     │  Dispatcher     │     │  Handlers       │   │
│  └─────────────────┘     └────────┬────────┘     └────────┬────────┘   │
│                                   │                       │             │
│                                   │                       ▼             │
│                          ┌────────┴────────┐     ┌─────────────────┐   │
│                          │                 │     │ SignalR         │   │
│                          ▼                 ▼     │ Event Handler   │   │
│                   ┌──────────┐     ┌──────────┐  └────────┬────────┘   │
│                   │  Outbox  │     │  Service │           │             │
│                   │  Writer  │     │  Bus     │           │             │
│                   └────┬─────┘     └──────────┘           │             │
│                        │                                  │             │
│                        ▼                                  │             │
│                   ┌──────────┐                            │             │
│                   │  Outbox  │     ┌──────────────────────┘             │
│                   │  Table   │     │                                    │
│                   └────┬─────┘     │                                    │
│                        │           ▼                                    │
│                        │    ┌─────────────────┐                         │
│                        │    │  SignalR Hub    │                         │
│                        │    │  /hubs/events   │                         │
│                        │    └────────┬────────┘                         │
│                        │             │                                  │
└────────────────────────┼─────────────┼──────────────────────────────────┘
                         │             │ WebSocket
                         │             ▼
┌────────────────────────┼─────────────────────────────────────────────────┐
│                        │     FRONTEND (React)                            │
├────────────────────────┼─────────────────────────────────────────────────┤
│                        │                                                 │
│  ┌─────────────────────┼──────────────────────────────────────────────┐ │
│  │                     │      useSignalR Hook                          │ │
│  │                     ▼                                               │ │
│  │  ┌──────────────────────────────────────────────────────────────┐  │ │
│  │  │  SignalRContext Provider                                      │  │ │
│  │  │  - Connection management                                      │  │ │
│  │  │  - Auto-reconnect                                             │  │ │
│  │  │  - Event subscription                                         │  │ │
│  │  └──────────────────────────────────────────────────────────────┘  │ │
│  │                          │                                          │ │
│  │          ┌───────────────┼───────────────┐                          │ │
│  │          ▼               ▼               ▼                          │ │
│  │   ┌────────────┐  ┌────────────┐  ┌────────────┐                   │ │
│  │   │ Dashboard  │  │ Approval   │  │ Vendor     │                   │ │
│  │   │ Updates    │  │ Notify     │  │ Status     │                   │ │
│  │   └────────────┘  └────────────┘  └────────────┘                   │ │
│  └─────────────────────────────────────────────────────────────────────┘ │
│                                                                          │
└──────────────────────────────────────────────────────────────────────────┘
```

### 3.2 Event Types to Support

| Event Type | Trigger | SignalR Method | Target |
|------------|---------|----------------|--------|
| `VendorCreated` | Vendor save | `VendorCreated` | Approvers |
| `VendorStatusChanged` | Status transition | `StatusChanged` | All connected |
| `WorkflowTaskAssigned` | Approval request | `TaskAssigned` | Specific user |
| `SapSyncCompleted` | SAP integration | `SapSyncResult` | Requestor |
| `NotificationCreated` | Any notification | `Notification` | Specific user |

---

## 4. Implementation Details

### 4.1 Backend Components

#### 4.1.1 IDomainEventDispatcher
```csharp
// Location: VendorMdm.Core.Framework/Events/IDomainEventDispatcher.cs
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<object> events, CancellationToken ct = default);
}
```

#### 4.1.2 IEventHandler<T>
```csharp
// Location: VendorMdm.Core.Framework/Events/IEventHandler.cs
public interface IEventHandler<TEvent> where TEvent : class
{
    Task HandleAsync(TEvent @event, CancellationToken ct = default);
}
```

#### 4.1.3 DomainEventDispatcher Implementation
```csharp
// Location: VendorMdm.Api/Services/Events/DomainEventDispatcher.cs
// Uses IServiceProvider to resolve handlers
// Dispatches to all registered handlers for each event type
```

#### 4.1.4 SignalR Hub
```csharp
// Location: VendorMdm.Api/Hubs/EventHub.cs
// Methods: JoinGroup, LeaveGroup
// Server-to-client: StatusChanged, TaskAssigned, Notification, etc.
```

#### 4.1.5 Outbox Entity
```csharp
// Location: VendorMdm.Shared/Models/OutboxEvent.cs
// Fields: Id, EventType, Payload, Status, CreatedAt, ProcessedAt, RetryCount
```

### 4.2 Frontend Components

#### 4.2.1 SignalR Context
```typescript
// Location: frontend/src/context/SignalRContext.tsx
// Provides connection state, subscribe/unsubscribe methods
```

#### 4.2.2 useSignalR Hook
```typescript
// Location: frontend/src/hooks/useSignalR.ts
// Custom hook for component-level event subscription
```

---

## 5. Files to Create/Modify

### 5.1 New Files

| File | Purpose |
|------|---------|
| `backend/VendorMdm.Core.Framework/Events/IDomainEventDispatcher.cs` | Dispatcher interface |
| `backend/VendorMdm.Core.Framework/Events/IEventHandler.cs` | Handler interface |
| `backend/VendorMdm.Api/Services/Events/DomainEventDispatcher.cs` | Dispatcher implementation |
| `backend/VendorMdm.Api/Services/Events/SignalREventHandler.cs` | Handler that pushes to SignalR |
| `backend/VendorMdm.Api/Hubs/EventHub.cs` | SignalR Hub |
| `backend/VendorMdm.Shared/Models/OutboxEvent.cs` | Outbox entity |
| `backend/VendorMdm.Api/Services/Events/OutboxProcessor.cs` | Background outbox processor |
| `frontend/src/context/SignalRContext.tsx` | React context for SignalR |
| `frontend/src/hooks/useSignalR.ts` | Custom hook |
| `scripts/verification/verify_eda_completion.sh` | Verification script |

### 5.2 Files to Modify

| File | Change |
|------|--------|
| `backend/VendorMdm.Api/Program.cs` | Register SignalR, dispatcher, handlers |
| `backend/VendorMdm.Api/Services/VendorService.cs` | Dispatch events after save |
| `backend/VendorMdm.Api/Data/SqlDbContext.cs` | Add OutboxEvents DbSet |
| `frontend/src/App.tsx` | Wrap with SignalRProvider |
| `frontend/package.json` | Add @microsoft/signalr |
| `.agent/rules/moderngoldenrules.md` | Add EDA mandatory evaluation rule |

---

## 6. Acceptance Criteria

- [ ] Events are dispatched after entity save operations
- [ ] SignalR hub accepts connections at `/hubs/events`
- [ ] Frontend receives real-time status change notifications
- [ ] Outbox table stores events for guaranteed delivery
- [ ] Build passes with 0 errors
- [ ] Verification script passes all checks
- [ ] Brain rules updated to mandate EDA evaluation

---

## 7. Verification Script

Location: `scripts/verification/verify_eda_completion.sh`

Tests:
1. Backend build succeeds
2. SignalR hub endpoint responds
3. Event dispatcher is registered in DI
4. Frontend build succeeds
5. SignalR package installed
6. Outbox table exists in migration

---

**Spec Status**: APPROVED
**Next Phase**: Implementation Plan
