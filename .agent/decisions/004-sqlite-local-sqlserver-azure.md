# ADR-004: SQLite Local / SQL Server Azure

**Status**: Accepted
**Date**: 2025-12-18
**Deciders**: Architecture Team

---

## Context

We need a database strategy that supports:
- Fast local development
- Production-grade Azure deployment
- CI/CD automation
- Cost-effective development

---

## Decision

We will use:
- **SQLite** for local development (file-based, zero config)
- **Azure SQL Server** for all deployed environments
- **GitHub Actions** for migration deployment (with type patching)

---

## Consequences

### Positive

- Zero database setup for developers
- Fast local development (no network)
- Production uses enterprise-grade SQL Server
- Single codebase, EF Core abstraction

### Negative

- Type differences (TEXT vs nvarchar)
- Migration scripts need patching
- Some SQL features unavailable locally

### Neutral

- Requires discipline to use EF Core abstractions

---

## Alternatives Considered

### Option A: SQL Server Everywhere

**Pros**: Identical environments
**Cons**: Docker required locally, slower, complex setup
**Why rejected**: Developer friction too high

### Option B: PostgreSQL

**Pros**: Great features, free
**Cons**: Azure PostgreSQL is expensive, different from target
**Why rejected**: Azure SQL is primary target

---

## Implementation Notes

- EF Core migrations generate SQLite-compatible SQL
- GitHub Actions workflow patches TEXT → nvarchar(max)
- Never run `dotnet ef database update` against Azure directly

---

## References

- [database-migration-standards.md](../rules/standards/database-migration-standards.md)
