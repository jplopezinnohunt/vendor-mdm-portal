# Retrospective Governance Standard

**Category**: Governance & Process
**Section**: 10
**Status**: MANDATORY

---

## Definition

Lessons learned MUST be captured after significant implementations to prevent repeating mistakes and improve agent effectiveness.

---

## Rules

1. **BEFORE WORK**: Check INDEX.md ONLY if `Pending > 0`. If all applied, skip - brain has everything
2. **AFTER WORK**: Document issues encountered in retrospective
3. **MANDATORY**: Apply learnings to brain rules immediately - no "Pending" items
4. **RETENTION**: INDEX.md kept forever, active retrospectives for 3 months
5. **SIZE LIMITS**: INDEX.md max 200 lines, individual retrospectives 300-500 lines
6. **EFFICIENCY**: Once applied to brain, retrospective is historical only - don't re-read

---

## Implementation

### Directory Structure

```
.agent/retrospectives/
  ├── INDEX.md                  ← ALWAYS READ THIS FIRST (30 sec)
  ├── active/                   ← Current quarter (max 10 files)
  │   └── YYYY-QX-topic.md
  ├── archived/                 ← Past quarters (reference only)
  │   └── YYYY-QX-summary.md
  └── learnings-database.md     ← Aggregated patterns (optional)
```

### INDEX.md Format

```markdown
# Retrospectives Index
**Last Updated**: YYYY-MM-DD

## Top 5 Critical Learnings
1. ❌ ISSUE → ✅ SOLUTION (Source: YYYY-MM-DD Topic)
2. ❌ `env.IsStaging()` doesn't exist → ✅ Use `env.EnvironmentName == "Staging"`
3. ❌ `Headers.Add()` throws ASP0019 → ✅ Use `Headers["X-Frame"] = "DENY"`

## Applied to Brain Rules
- [x] Section 7.B: Environment detection rule
- [x] Section 7.B: Header syntax rule
- [ ] Section X.Y: Pending pattern Z

## Active Retrospectives (Current Quarter)
- [2026-02-03: Security Hardening](active/2026-Q1-security-hardening.md) - Key: Headers, Middleware
```

### Individual Retrospective Format

```markdown
# Retrospective: [Topic]
**Date**: YYYY-MM-DD
**Feature**: [Feature name]
**Duration**: [Time spent]

## Summary
[Brief description of what was implemented]

## Issues Encountered

### Issue 1: [Title]
- **Symptom**: [What happened]
- **Root Cause**: [Why it happened]
- **Solution**: [How it was fixed]
- **Prevention**: [How to avoid in future]
- **Applied to Brain**: Section X.Y ✅

## Performance Benchmarks
- [Metric 1]: [Value]
- [Metric 2]: [Value]

## Learnings for Brain Rules
1. [Learning 1] → Add to Section X
2. [Learning 2] → Add to Section Y
```

---

## Agent Workflow

### Before Starting Work

1. Read `.agent/retrospectives/INDEX.md` (if exists)
2. Apply top learnings to current task
3. Avoid documented mistakes

### After Completing Work

1. Document issues encountered in retrospective
2. Update `INDEX.md` with top 3-5 learnings
3. **MANDATORY: Apply learnings to brain rules immediately**
   - Update relevant sections in `moderngoldenrules.md`
   - Mark as `[x] Applied` in INDEX.md
   - Commit rule updates with retrospective reference
4. Do NOT leave "Pending" items - apply them before closing

---

## What to Document

**MUST Document**:
- ❌ Bugs found after implementation (runtime issues)
- ⚠️ Warnings that took >5 min to fix
- 🔧 Tool workarounds needed
- 📋 Patterns that should be in brain rules
- ⏱️ Performance benchmarks achieved

**DON'T Document**:
- Expected behavior
- User-specific preferences
- One-time issues

---

## Retrospective → Brain Rule Lifecycle

```
1. Implementation finds issue
   ↓
2. Documented in retrospective
   ↓
3. Added to INDEX.md
   ↓
4. Brain rule updated (moderngoldenrules.md)
   ↓
5. Retrospective marked as "Applied: ✅"
   ↓
6. Future agents follow updated rule (no repeat bug)
```

---

## Retention Policy

**Keep Forever**:
- `INDEX.md` (always current, max 200 lines)

**Keep for 3 Months**:
- Individual retrospectives in `active/`

**Quarterly Aggregation**:
- Combine `active/` → `archived/YYYY-QX-summary.md`
- Clear `active/` folder
- Update `INDEX.md` with aggregated learnings

**Delete After 2 Years**:
- Archived summaries (learnings already in brain rules)

---

## Size Management

**Target Sizes**:
- `INDEX.md`: 50-200 lines (quick read)
- Individual retrospective: 300-500 lines (detailed)
- Learnings database: 500-1000 lines (comprehensive)

**File Count Limits**:
- Active: Max 10-12 files per quarter
- Archived: Max 4-8 quarterly summaries
- Total: ~15 files maximum (with 2-year purge)

---

## Success Metrics

**Effectiveness Indicators**:
- Repeated bugs decrease over time
- Implementation speed increases (fewer trial-and-error)
- Brain rule updates cite retrospective evidence
- New agents ramp up faster (read INDEX.md)

---

## Anti-Patterns

❌ Skipping retrospective after significant work
❌ Leaving learnings as "Pending" in INDEX.md
❌ Not applying learnings to brain rules
❌ INDEX.md exceeding 200 lines
❌ Documenting obvious/expected behavior

---

## Reference

- **Golden Rules**: Section 10
- **Brain Architecture**: [BRAIN-ARCHITECTURE.md](../BRAIN-ARCHITECTURE.md)
- **Compliance Logic**: [compliance-logic-standard.md](compliance-logic-standard.md)
