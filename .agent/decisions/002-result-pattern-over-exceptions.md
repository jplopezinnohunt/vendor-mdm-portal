# ADR-002: Result Pattern Over Exceptions

**Status**: Accepted
**Date**: 2025-12-18
**Deciders**: Architecture Team

---

## Context

Services need to communicate success/failure to callers. Two main approaches:
1. Throw exceptions for failures
2. Return Result objects

We needed a consistent approach across the codebase.

---

## Decision

We will use the **Result Pattern** (`Result<T>`) for all business logic outcomes:
- `Result.Success(value)` for successful operations
- `Result.Failure("message")` for business failures
- Exceptions ONLY for unexpected system failures

---

## Consequences

### Positive

- Explicit error handling (compiler enforces checking)
- No hidden control flow
- Easier to test
- Better performance (no exception overhead)
- Clear distinction: business error vs system error

### Negative

- More verbose than exceptions
- Requires discipline to propagate results
- Some developers unfamiliar with pattern

### Neutral

- Need to map Result to HTTP status in controllers

---

## Alternatives Considered

### Option A: Exceptions for Everything

**Pros**: Familiar, less code
**Cons**: Hidden control flow, expensive, unclear intent
**Why rejected**: Makes code harder to reason about

### Option B: Nullable Returns

**Pros**: Simple
**Cons**: No error message, null reference risks
**Why rejected**: Loses error context

---

## References

- [result-pattern-standard.md](../rules/standards/result-pattern-standard.md)
- [error-handling-standard.md](../rules/standards/error-handling-standard.md)
