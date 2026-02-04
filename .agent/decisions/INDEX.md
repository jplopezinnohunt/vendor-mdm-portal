# Architecture Decision Records (ADR) Index

**Purpose**: Document significant architectural decisions with context and rationale.

---

## How to Use ADRs

### When to Create an ADR

Create an ADR when:
- Choosing between multiple valid approaches
- Making decisions that are hard to reverse
- Establishing patterns that will be repeated
- Changing existing architectural decisions

### ADR Lifecycle

| Status | Meaning |
|--------|---------|
| **Proposed** | Under discussion |
| **Accepted** | Decision made and active |
| **Deprecated** | No longer applies to new code |
| **Superseded** | Replaced by another ADR |

---

## Decision Index

| # | Title | Status | Date |
|---|-------|--------|------|
| [001](001-hexagonal-architecture.md) | Hexagonal Architecture | Accepted | 2025-12-18 |
| [002](002-result-pattern-over-exceptions.md) | Result Pattern Over Exceptions | Accepted | 2025-12-18 |
| [003](003-event-driven-side-effects.md) | Event-Driven Side Effects | Accepted | 2026-02-04 |
| [004](004-sqlite-local-sqlserver-azure.md) | SQLite Local / SQL Server Azure | Accepted | 2025-12-18 |
| [005](005-single-source-documentation.md) | Single Source Documentation | Accepted | 2026-02-04 |

---

## By Category

### Architecture
- [001](001-hexagonal-architecture.md) - Hexagonal Architecture

### Code Patterns
- [002](002-result-pattern-over-exceptions.md) - Result Pattern Over Exceptions
- [003](003-event-driven-side-effects.md) - Event-Driven Side Effects

### Infrastructure
- [004](004-sqlite-local-sqlserver-azure.md) - SQLite Local / SQL Server Azure

### Process
- [005](005-single-source-documentation.md) - Single Source Documentation

---

## Template

See [TEMPLATE.md](TEMPLATE.md) for creating new ADRs.

**Naming**: `NNN-short-title.md` (e.g., `006-api-versioning-strategy.md`)
