# ADR-001: Hexagonal Architecture

**Status**: Accepted
**Date**: 2025-12-18
**Deciders**: Architecture Team

---

## Context

The Vendor MDM Portal needs to integrate with multiple external systems (SAP, Salesforce, Azure AD) while maintaining a clean, testable core domain. We need an architecture that:
- Isolates business logic from infrastructure
- Makes testing easy
- Allows swapping external dependencies
- Supports multiple deployment scenarios

---

## Decision

We will use **Hexagonal Architecture (Ports & Adapters)** with:
- Core domain in `VendorMdm.Shared` (no external dependencies)
- Inbound ports (Controllers) in `VendorMdm.Api`
- Outbound ports (Repositories, External Services) as interfaces
- Adapters for each external system

---

## Consequences

### Positive

- Business logic is isolated and testable
- External systems can be mocked easily
- Clear separation of concerns
- Supports multiple database providers (SQLite, SQL Server)

### Negative

- More boilerplate code (interfaces, adapters)
- Steeper learning curve for new developers
- Risk of over-abstraction

### Neutral

- Requires discipline to maintain boundaries

---

## Alternatives Considered

### Option A: Traditional N-Tier

**Pros**: Simple, familiar
**Cons**: Business logic often leaks into service layer
**Why rejected**: Harder to test, tight coupling to infrastructure

### Option B: Clean Architecture

**Pros**: Well-documented, strict layers
**Cons**: More complex, many projects
**Why rejected**: Hexagonal is simpler while achieving same goals

---

## References

- [hexagonal-architecture-standards.md](../rules/standards/hexagonal-architecture-standards.md)
