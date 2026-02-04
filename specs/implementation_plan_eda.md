# Implementation Plan: Event-Driven Architecture Completion

**Spec Reference**: [spec_event_driven_architecture.md](spec_event_driven_architecture.md)
**Date**: 2026-02-04
**Status**: APPROVED (Auto-approved per user directive)

---

## Phase 1: Core Framework Event Infrastructure

### Step 1.1: Create IDomainEventDispatcher Interface
- File: `backend/VendorMdm.Core.Framework/Events/IDomainEventDispatcher.cs`
- Dependencies: None

### Step 1.2: Create IEventHandler<T> Interface
- File: `backend/VendorMdm.Core.Framework/Events/IEventHandler.cs`
- Dependencies: None

### Step 1.3: Create DomainEventDispatcher Implementation
- File: `backend/VendorMdm.Api/Services/Events/DomainEventDispatcher.cs`
- Dependencies: IDomainEventDispatcher, IEventHandler<T>

---

## Phase 2: Outbox Pattern

### Step 2.1: Create OutboxEvent Entity
- File: `backend/VendorMdm.Shared/Models/OutboxEvent.cs`
- Dependencies: None

### Step 2.2: Add OutboxEvents to DbContext
- File: `backend/VendorMdm.Api/Data/SqlDbContext.cs`
- Dependencies: OutboxEvent entity

### Step 2.3: Create EF Migration
- Command: `dotnet ef migrations add AddOutboxEvents`
- Dependencies: DbContext changes

### Step 2.4: Create OutboxProcessor Background Service
- File: `backend/VendorMdm.Api/Services/Events/OutboxProcessor.cs`
- Dependencies: OutboxEvent, IServiceBusService

---

## Phase 3: SignalR Hub

### Step 3.1: Add SignalR NuGet Package
- Package: Microsoft.AspNetCore.SignalR (already in ASP.NET Core)
- Dependencies: None

### Step 3.2: Create EventHub
- File: `backend/VendorMdm.Api/Hubs/EventHub.cs`
- Dependencies: None

### Step 3.3: Create SignalREventHandler
- File: `backend/VendorMdm.Api/Services/Events/SignalREventHandler.cs`
- Dependencies: EventHub, IEventHandler<T>

### Step 3.4: Register SignalR in Program.cs
- File: `backend/VendorMdm.Api/Program.cs`
- Dependencies: EventHub

---

## Phase 4: Service Integration

### Step 4.1: Wire Event Dispatch in VendorService
- File: `backend/VendorMdm.Api/Services/VendorService.cs`
- Dependencies: IDomainEventDispatcher

### Step 4.2: Wire Event Dispatch in Other Services
- Files: ChangeRequestService, InvitationService, etc.
- Dependencies: IDomainEventDispatcher

---

## Phase 5: Frontend SignalR Integration

### Step 5.1: Install @microsoft/signalr Package
- Command: `npm install @microsoft/signalr`
- Location: frontend/

### Step 5.2: Create SignalRContext Provider
- File: `frontend/src/context/SignalRContext.tsx`
- Dependencies: @microsoft/signalr

### Step 5.3: Create useSignalR Hook
- File: `frontend/src/hooks/useSignalR.ts`
- Dependencies: SignalRContext

### Step 5.4: Integrate SignalRProvider in App
- File: `frontend/src/App.tsx`
- Dependencies: SignalRContext

### Step 5.5: Add Real-Time Updates to Dashboards
- Files: ApproverDashboard.tsx, EventDashboard.tsx
- Dependencies: useSignalR hook

---

## Phase 6: Brain Rules Update

### Step 6.1: Add EDA Mandatory Evaluation Rule
- File: `.agent/rules/moderngoldenrules.md`
- Section: New section after Section 10

---

## Phase 7: Verification

### Step 7.1: Create Verification Script
- File: `scripts/verification/verify_eda_completion.sh`

### Step 7.2: Run All Checks
- Backend build
- Frontend build
- Verification script

### Step 7.3: Commit
- Conventional commit message
- Reference spec

---

## Execution Order

```
1.1 → 1.2 → 1.3 (Core interfaces)
      ↓
2.1 → 2.2 → 2.3 → 2.4 (Outbox)
      ↓
3.2 → 3.3 → 3.4 (SignalR)
      ↓
4.1 → 4.2 (Service wiring)
      ↓
5.1 → 5.2 → 5.3 → 5.4 → 5.5 (Frontend)
      ↓
6.1 (Brain rules)
      ↓
7.1 → 7.2 → 7.3 (Verification & Commit)
```

---

**Plan Status**: APPROVED
**Next Phase**: Implementation
