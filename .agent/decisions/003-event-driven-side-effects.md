# ADR-003: Event-Driven Side Effects

**Status**: Accepted
**Date**: 2026-02-04
**Deciders**: Architecture Team

---

## Context

When domain operations occur (vendor created, status changed), we need to:
- Update the frontend in real-time
- Sync with external systems (SAP)
- Log audit events
- Send notifications

Embedding all this in service methods creates tight coupling and makes testing difficult.

---

## Decision

We will use **Domain Events** with:
- Event collection in Concepts (not throw, collect)
- Outbox pattern for guaranteed delivery
- In-process dispatch for SignalR
- Background processor for external integrations

---

## Consequences

### Positive

- Decoupled side effects
- Testable (mock event handlers)
- Guaranteed delivery (Outbox)
- Real-time frontend updates
- Easy to add new handlers

### Negative

- Eventual consistency (not immediate)
- More moving parts
- Debugging requires tracing events

### Neutral

- Events are auditable by design

---

## Alternatives Considered

### Option A: Direct Calls in Service

**Pros**: Simple, immediate
**Cons**: Tight coupling, hard to test, slow operations
**Why rejected**: Doesn't scale, makes services bloated

### Option B: Message Queue Only

**Pros**: Fully decoupled
**Cons**: Complex infrastructure, no in-process handlers
**Why rejected**: Overkill for current needs

---

## References

- [event-driven-architecture-standard.md](../rules/standards/event-driven-architecture-standard.md)
