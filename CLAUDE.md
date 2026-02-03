# CLAUDE.md - Agent Navigation & Integration Index

**Version**: 3.0.0
**Date**: 2026-02-03
**Purpose**: Integration point between `.agent/rules/` (Antigravity-maintained) and `.claude/` (handover docs)
**Status**: ACTIVE

---

## 🧠 START HERE - MASTER AUTHORITY

**BEFORE EVERY TASK, READ THIS FIRST:**

### [.agent/rules/moderngoldenrules.md](.agent/rules/moderngoldenrules.md)

This is your **Executive Directive** and **System Logic**. It defines:
- ✅ **Spec-Driven Development (SDD)**: Mandatory 4-phase workflow
- ✅ **Standards Brain**: Which standard to read for which task type
- ✅ **Performance DNA**: Doherty Threshold (<400ms), Generated Columns, Domain Events
- ✅ **Build Hygiene**: Clean sweep protocol, Interface integrity, Observability requirements

**This file is maintained by Antigravity** and is the **single source of truth** for agent behavior.

---

## 📚 Documentation Architecture

```
CLAUDE.md (you are here)
    ↓
    ├─ MASTER AUTHORITY
    │   └─ .agent/rules/moderngoldenrules.md ← READ THIS FIRST
    │
    ├─ STANDARDS BRAIN (Task-Specific References)
    │   └─ .agent/rules/standards/*.md ← Read based on task type
    │
    └─ IMPLEMENTATION GUIDES (Current State & Patterns)
        └─ .claude/*.md ← Detailed patterns for this project
```

---

## 🔍 Quick Navigation: Which File to Read When?

### Every Task (MANDATORY)
1. **[moderngoldenrules.md](.agent/rules/moderngoldenrules.md)** - Executive directive, SDD workflow

### Task-Specific Standards (Read as cited in moderngoldenrules.md)

| Your Task | Read This Standard |
|-----------|-------------------|
| **UI/UX, React components, forms** | [ui-design-standards.md](.agent/rules/standards/ui-design-standards.md) |
| **Database schema, Entity design, SQL vs JSONB** | [data-model-standards.md](.agent/rules/standards/data-model-standards.md) |
| **Architecture, Services, Ports & Adapters** | [hexagonal-architecture-standards.md](.agent/rules/standards/hexagonal-architecture-standards.md) |
| **Security, Auth, RBAC** | [security-architecture.md](.agent/rules/standards/security-architecture.md) |
| **EF Migrations, Schema changes** | [database-migration-standards.md](.agent/rules/standards/database-migration-standards.md) |
| **Git branching, Deployment, SAP alignment** | [git-branching-sap-standards.md](.agent/rules/standards/git-branching-sap-standards.md) |
| **CI/CD, GitHub Actions** | [cicd-setup-standards.md](.agent/rules/standards/cicd-setup-standards.md) |
| **Audit, Compliance logging** | [audit-log-integration-standards.md](.agent/rules/standards/audit-log-integration-standards.md) |
| **Domain Events, Messaging, Async** | [event-driven-architecture-standard.md](.agent/rules/standards/event-driven-architecture-standard.md) |
| **Ontology, Concepts, Domain modeling** | [ontology-modeling-standard.md](.agent/rules/standards/ontology-modeling-standard.md) |
| **JSONB search, Performance, Indexing** | [performance-generated-columns.md](.agent/rules/standards/performance-generated-columns.md) |
| **Rate limiting, API throttling** | [rate-limiting-standard.md](.agent/rules/standards/rate-limiting-standard.md) |
| **Repository pattern, Data access** | [repository-pattern-standard.md](.agent/rules/standards/repository-pattern-standard.md) |

### Implementation Guides (Project-Specific Details)

| Your Task | Read This Guide |
|-----------|----------------|
| **Critical rules (Zero Data Loss, Pre-Commit, Interface Integrity)** | [.claude/00-critical-rules.md](.claude/00-critical-rules.md) |
| **Architecture details (Hexagonal, Hybrid Model, Ontology)** | [.claude/01-architecture.md](.claude/01-architecture.md) |
| **Security implementation (8 Layers, RBAC, Input Validation)** | [.claude/02-security.md](.claude/02-security.md) |
| **Core.Framework usage (ONE LINE Integration, Extension Pattern)** | [.claude/03-core-framework.md](.claude/03-core-framework.md) |
| **Git/CI/CD specifics (Branches, Conventional Commits, Deployment)** | [.claude/04-git-cicd.md](.claude/04-git-cicd.md) |
| **Observability (Health Checks, Logging, OpenTelemetry)** | [.claude/05-observability.md](.claude/05-observability.md) |
| **Audit logging (IAuditableEntity, Schema, Implementation)** | [.claude/06-audit-logging.md](.claude/06-audit-logging.md) |
| **Compliance (Data Residency, Multi-Tenancy, GDPR)** | [.claude/07-compliance.md](.claude/07-compliance.md) |
| **Implementation specs (Week-by-Week, Pattern Checklist)** | [.claude/08-implementation-specs.md](.claude/08-implementation-specs.md) |

---

## ⚙️ Mandatory Workflow (From moderngoldenrules.md)

**NEVER skip this workflow. It's defined in [moderngoldenrules.md](.agent/rules/moderngoldenrules.md#2-governance-spec-driven-development-sdd)**

```
Phase 1: Spec
  ├─ Create specs/spec_[name].md
  ├─ Cite relevant standards (compliance sidebar)
  └─ Get user approval

Phase 2: Plan
  ├─ Create implementation_plan.md
  ├─ Create scripts/verification/verify_[name].sh
  └─ Get user approval

Phase 3: Implementation
  └─ Execute following all standards

Phase 4: Verification
  ├─ Run verification script
  ├─ Run pre-commit checks
  └─ Conventional commit
```

**Refusal Protocol**: If asked to skip the spec, politely decline per moderngoldenrules.md Section 2.

---

## 🎯 Decision Tree

```
New Task?
  ↓
  1. READ: moderngoldenrules.md (ALWAYS)
  2. READ: Relevant standard from table above (based on task type)
  3. READ: Relevant .claude/ guide (for implementation patterns)
  4. FOLLOW: SDD workflow (Spec → Plan → Implement → Verify)
  5. COMMIT: With pre-commit checks + conventional commit message
```

---

## ✅ Pre-Commit Checklist (Mandatory)

**From [.claude/00-critical-rules.md](.claude/00-critical-rules.md) and moderngoldenrules.md:**

```bash
# 1. Build (MUST be 0 errors)
dotnet build --configuration Release
npm run build

# 2. Migration size (if applicable, MUST be < 50KB)
ls -lh backend/VendorMdm.Api/Migrations/*.cs | grep -v Designer

# 3. No secrets in git
git status

# 4. Run verification script
bash scripts/verification/verify_[name].sh
```

**FORBIDDEN**:
- ❌ Delete/reset database files without explicit approval
- ❌ `rm -rf` without approval
- ❌ Skip verification scripts
- ❌ Commit to `main` branch (always use `feature/*` from `develop`)

---

## 🚦 Current Project State

### Completed ✅
- Week 0-1: Core Foundation (Governance, Interfaces, Services)
- Core.Framework with 6 core services
- Build passing (0 errors, 0 warnings in Release)
- Application running (Backend: http://localhost:5001, Frontend: http://localhost:3000)

### Recent Fixes (2026-02-03) ✅
- JWT configuration (user secrets)
- FileStorage conditional registration (UseMock pattern)
- Authorization policies (ApproverOnly, RequestorOnly, AdminOnly)
- Authentication scheme (Cookie auth for local dev)
- Duplicate key fix in EventDetail.tsx

### In Progress 🔄
- Week 3: Migration to Core.Framework
  - ✅ Reference Core.Framework
  - ✅ IStructuredLogger pilot
  - ❌ Polly policies pending
  - ❌ Full logging migration pending

### Pending ⏳
- Week 2: Observability (OpenTelemetry)
- Weeks 4-10: Health Checks, API Versioning, SAP Integration

---

## 🔗 Related Documentation

- **Core.Framework Governance**: `backend/VendorMdm.Core.Framework/GOVERNANCE.md`
- **Core.Framework Contributing**: `backend/VendorMdm.Core.Framework/CONTRIBUTING.md`
- **Specs Directory**: `specs/` (create specs here per SDD workflow)
- **Verification Scripts**: `scripts/verification/` (automated tests for specs)

---

## 📞 When in Doubt

1. **Standards question?** → Re-read [moderngoldenrules.md](.agent/rules/moderngoldenrules.md)
2. **Implementation pattern?** → Check relevant `.claude/` guide
3. **Workflow question?** → Follow SDD (Spec → Plan → Implement → Verify)
4. **Still unclear?** → Ask the user

**Never**:
- Skip the spec creation
- Skip the verification script
- Commit without pre-commit checks
- Bypass the governance in moderngoldenrules.md

---

**End of CLAUDE.md** - This is a navigation index. All authoritative content is in the referenced files.
