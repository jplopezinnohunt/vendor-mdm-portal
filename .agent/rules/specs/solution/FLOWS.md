# Solution Spec: Flows

**Focus**: State Machines & Technical Flows

> **Business Processes**: See [processes/](../processes/) for role-based business flows.

---

## 1. Vendor Invitation Flow

```
Admin creates invitation
        │
        ▼
   [PENDING] ──────────────┐
        │                  │
   Vendor clicks          Time exceeds
        │                  │
        ▼                  ▼
   [ACCEPTED]         [EXPIRED]
        │                  │
   Submits form      Admin resends
        │                  │
        ▼                  │
   [COMPLETED] ◄───────────┘
```

**States**: Pending → Accepted → Completed | Expired | Cancelled

---

## 2. Vendor Application Flow

```
   [DRAFT]
       │
   Submit
       ▼
   [SUBMITTED]
       │
   Review starts
       ▼
   [UNDER_REVIEW]
       │
   ┌───┴───┐
   ▼       ▼
[APPROVED] [REJECTED]
```

---

## 3. Change Request Flow

```
   [DRAFT]
       │
   Submit
       ▼
   [SUBMITTED]
       │
   Review
       ▼
   ┌───┴───┐
   ▼       ▼
[APPROVED] [REJECTED]
       │
   SAP Sync
       ▼
   [INTEGRATED]
```

---

## 4. Hybrid Data Flow (Every Write)

```
1. API receives request
       │
2. Validate & process
       │
3. SQL: Save metadata
       │
4. Cosmos: Save artifact
       │
5. Cosmos: Emit event
       │
6. Service Bus: Queue (if async needed)
       │
7. Return response
```

---

## 5. Real-Time Update Flow

```
Domain Event
     │
     ▼
EventDispatcher
     │
     ├──► SignalR Hub ──► Frontend
     │
     └──► Outbox ──► Service Bus ──► Workers
```
