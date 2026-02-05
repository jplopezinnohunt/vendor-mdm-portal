# Core.Framework Documentation Review - 2026-02-05

## Files Analyzed

| File | Status | Decision |
|------|--------|----------|
| `GOVERNANCE.md` | **ACTIVE** | Key patterns extracted to Golden Rules Section 6.1 |
| `CONTRIBUTING.md` | **ACTIVE** | Standard contribution guide, no changes needed |
| `README.md` | **ACTIVE** | Package documentation, no changes needed |

## GOVERNANCE.md Analysis

### Extracted to Golden Rules (Section 6.1)

The following patterns were added to [moderngoldenrules.md](../../.agent/rules/moderngoldenrules.md) Section 6.1:

1. **FORBIDDEN patterns** (build will fail):
   - Apps CANNOT implement Core interfaces directly
   - Apps CANNOT inherit from Core classes

2. **ALLOWED patterns** (extension approach):
   - Apps CAN create extension methods
   - Apps CAN create adapters/wrappers (composition)

3. **Governance reference**:
   - ADR requirement for Core changes documented

### Why GOVERNANCE.md Stays Active

Unlike the docs/ folder files that were archived, `GOVERNANCE.md` remains active because:

1. **Living Document**: Contains detailed enforcement mechanisms (Roslyn analyzers, build props)
2. **Change Process**: Defines ADR workflow for Core modifications
3. **Team Ownership**: Documents Architecture Team vs App Team responsibilities
4. **Compliance Checklist**: Used during PR reviews

The Golden Rules reference this file rather than duplicating its 370+ lines of detail.

## Relationship

```
Golden Rules (Section 6.1)     GOVERNANCE.md (This folder)
├─ Quick reference patterns    ├─ Full enforcement details
├─ FORBIDDEN/ALLOWED summary   ├─ Roslyn analyzer rules
└─ Link to GOVERNANCE.md       ├─ Change process (ADR)
                               ├─ Emergency bypass
                               └─ Compliance checklist
```

## Future Reviews

- **Next Review**: 2026-03-05 (monthly)
- **Owner**: Architecture Team
- **Trigger**: Any Core.Framework version bump
