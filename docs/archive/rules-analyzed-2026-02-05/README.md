# Rules Analysis - 2026-02-05

## Purpose

These documents were analyzed to identify rules that could enhance the Golden Rules brain. This was NOT a mechanical merge - each rule was evaluated for genuine value.

## Analysis Decisions

| Document | Decision | Action Taken |
|----------|----------|--------------|
| `BEST_PRACTICES.md` | Rule 14 (Dependency Health) adds value | **Added to Section 12** |
| `AGENT_MANDATE.md` | Already covered in SDD workflow (Section 2) | **No action** |
| `deployment-process-rules.md` | Already in cicd-setup-standards.md | **No action** |
| `cicd-consistency-rule.md` | Already in cicd-setup-standards.md | **No action** |
| `canonical-model-rules.md` | SAP decoupling + Source System tracking add value | **Added to Section 13** |
| `ui_design_standards.md` | Already in standards/ui-design-standards.md | **No action** |

## Rules Added to Golden Rules (v1.3.0)

### Section 12: Dependency Health Awareness
- `TestConnectionAsync` requirement
- "Truth in Success" principle
- Contextual Error Logs
- UI Fail-Fast pattern

### Section 13: Canonical Entity Decoupling
- NO SAP fields in domain entities
- Source System Tracking (Portal, SAP, API, Migration, Batch)
- Event Sourcing required fields: `correlationId`, `actor`, `channel`

### Section 6.1: Core.Framework Extension Pattern
- Composition over inheritance
- Extension methods allowed
- ADR requirement for Core changes

## Why These Documents Are Archived

These files served their purpose as early documentation. Their valuable content has been:
- **Integrated** into Golden Rules (conscious decision)
- **Consolidated** into standards files
- **Made obsolete** by newer, more comprehensive standards

The Golden Rules brain is now the single source of truth.

## Reference

- Golden Rules: [moderngoldenrules.md](../../../.agent/rules/moderngoldenrules.md)
- Core.Framework Governance: Still active at `backend/VendorMdm.Core.Framework/GOVERNANCE.md`
