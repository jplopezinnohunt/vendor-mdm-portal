# ADR-005: Single Source Documentation (Brain Architecture)

**Status**: Accepted
**Date**: 2026-02-04
**Deciders**: Development Team

---

## Context

Documentation was scattered across multiple files:
- CLAUDE.md (200+ lines)
- MEMORY.md (100+ lines)
- Various .claude/*.md files

This caused:
- Duplication and conflicts
- Slow agent loading
- Inconsistent rules
- Hard to maintain

---

## Decision

We will use a **hierarchical single-source architecture**:
- Pointer files (CLAUDE.md, MEMORY.md) → 3 lines only
- Master Authority (moderngoldenrules.md) → executive rules
- Detailed Standards (standards/*.md) → 34 specific patterns
- Retrospectives (retrospectives/*.md) → organizational memory
- Decisions (decisions/*.md) → architectural decisions

---

## Consequences

### Positive

- Single source of truth
- No duplication
- Fast agent loading (load only relevant standard)
- Easy to maintain and evolve
- Clear hierarchy

### Negative

- Requires navigation between files
- Initial learning curve

### Neutral

- More files, but better organized

---

## Alternatives Considered

### Option A: Single Monolithic File

**Pros**: Everything in one place
**Cons**: Too large, slow to load, hard to maintain
**Why rejected**: Doesn't scale

### Option B: Wiki-Style Links

**Pros**: Flexible
**Cons**: No clear hierarchy, hard to enforce
**Why rejected**: Lacks structure

---

## References

- [BRAIN-ARCHITECTURE.md](../rules/BRAIN-ARCHITECTURE.md)
