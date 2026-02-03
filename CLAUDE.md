# CLAUDE.md - Agent Instructions for Vendor MDM Portal

**Version**: 2.0.0
**Date**: 2026-02-03
**Author**: Claude (Opus 4.5)
**Status**: ACTIVE

---

## Quick Reference

This file serves as an index to the detailed agent instructions located in `.claude/`. Each section is documented in its own file for better management and maintainability.

---

## Documentation Structure

| File | Description | Priority |
|------|-------------|----------|
| [00-critical-rules.md](.claude/00-critical-rules.md) | Zero Data Loss, Pre-Commit Protocol, Interface Integrity | **HIGHEST** |
| [01-architecture.md](.claude/01-architecture.md) | Hexagonal Architecture, Hybrid Model, Ontology | HIGH |
| [02-security.md](.claude/02-security.md) | 8 Security Layers, RBAC, Input Validation | **CRITICAL** |
| [03-core-framework.md](.claude/03-core-framework.md) | ONE LINE Integration, Governance, Extension Patterns | HIGH |
| [04-git-cicd.md](.claude/04-git-cicd.md) | SAP-Aligned Branching, Conventional Commits, Deployment | HIGH |
| [05-observability.md](.claude/05-observability.md) | Health Checks, Structured Logging, OpenTelemetry | HIGH |
| [06-audit-logging.md](.claude/06-audit-logging.md) | IAuditableEntity, Audit Schema, Implementation | HIGH |
| [07-compliance.md](.claude/07-compliance.md) | Data Residency, Multi-Tenancy, GDPR Rights | HIGH |
| [08-implementation-specs.md](.claude/08-implementation-specs.md) | Week-by-Week Specs, Pattern Checklist | MEDIUM |

---

## Critical Rules Summary (ZERO TOLERANCE)

**ALWAYS READ FIRST**: [.claude/00-critical-rules.md](.claude/00-critical-rules.md)

### Zero Data Loss Policy
- **FORBIDDEN**: Delete/reset database files without EXPLICIT WRITTEN CONSENT
- **FORBIDDEN**: `rm -rf` without explicit approval
- **RECOVERY**: Fix migration scripts, NEVER delete to "start fresh"

### Pre-Commit Verification (MANDATORY)
```bash
# 1. Build
dotnet build --configuration Release  # 0 errors
npm run build                         # 0 errors

# 2. Migration size (MUST be < 50KB)
ls -lh backend/VendorMdm.Api/Migrations/*.cs | grep -v Designer

# 3. No secrets in git
git status
```

### Interface Integrity Rule
When changing an interface, update ALL implementations in one atomic turn.

---

## Architecture Summary

**Full details**: [.claude/01-architecture.md](.claude/01-architecture.md)

### Hexagonal Architecture
```
VendorMdm.Shared/          → Core Domain (PURE, NO external refs)
VendorMdm.Shared/Ontology/ → Business logic in Concepts
VendorMdm.Api/Controllers/ → Inbound Ports (REST)
VendorMdm.Api/Data/        → Persistence (SQL + JSONB)
VendorMdm.Api/Services/    → Outbound Ports (Events)
```

### Hybrid Model Decision
- **SQL Column**: Foreign keys, indexes, ACID, universal presence
- **JSONB**: Volatile data, context-specific, read-only payloads

---

## Security Summary (ZERO TOLERANCE)

**Full details**: [.claude/02-security.md](.claude/02-security.md)

### 8 Security Layers (Iron Dome)
1. Authentication (Azure AD, JWT)
2. Authorization (App-scoped RBAC)
3. Network (HSTS, CSP, CORS)
4. Input (XSS sanitization)
5. Rate Limiting (5 req/min anonymous)
6. Ghost User Blocking
7. Session (15-min sliding)
8. Secrets (KeyVault/UserSecrets)

### NEVER
- Hardcode secrets
- Accept raw JSONB from client
- Return SQL entities directly

---

## Core.Framework Summary

**Full details**: [.claude/03-core-framework.md](.claude/03-core-framework.md)

### ONE LINE Integration
```csharp
services.AddCoreFramework(configuration, "VendorMDM");
```

### Extension Pattern (ONLY)
- ✅ Create extension methods
- ✅ Create adapters/wrappers
- ❌ NEVER implement Core interfaces
- ❌ NEVER inherit from Core classes

---

## Git & CI/CD Summary

**Full details**: [.claude/04-git-cicd.md](.claude/04-git-cicd.md)

### Branch Strategy
| Branch | Environment | SAP |
|--------|-------------|-----|
| `main` | PRODUCTION | P01 |
| `develop` | DEV | D01 |
| `feature/*` | Local | Mocks |
| `hotfix/*` | PRODUCTION | P01 |

### Conventional Commits
```
feat: → Minor version
fix:  → Patch version
```

---

## Current Project State

### Completed ✅
- Week 0-1: Core Foundation (Governance, Interfaces, Services)
- Core.Framework with 6 core services
- Build passing (0 errors)

### In Progress 🔄
- Week 3: Migration to Core.Framework
  - ✅ Reference Core.Framework
  - ✅ IStructuredLogger pilot
  - ❌ Polly policies pending
  - ❌ Full logging migration pending

### Pending ⏳
- Week 2: Observability (OpenTelemetry)
- Weeks 4-10: Health Checks, API Versioning, SAP Integration, etc.

### Metrics
- Patterns: 16/24 (67%)
- Functional Items: 0/6 (0%)

---

## File Locations

```
vendor-mdm-portal/
├── .claude/                           # Agent instructions (this index)
│   ├── 00-critical-rules.md
│   ├── 01-architecture.md
│   ├── 02-security.md
│   ├── 03-core-framework.md
│   ├── 04-git-cicd.md
│   ├── 05-observability.md
│   ├── 06-audit-logging.md
│   ├── 07-compliance.md
│   └── 08-implementation-specs.md
├── .agent/rules/                      # Original standards
│   ├── modern-golden-rules.md
│   └── standards/
├── backend/
│   ├── VendorMdm.Api/
│   ├── VendorMdm.Shared/
│   └── VendorMdm.Core.Framework/
│       ├── GOVERNANCE.md
│       └── CONTRIBUTING.md
├── frontend/
└── CLAUDE.md                          # This index file
```

---

## Agent Behavior Checklist

### Before Starting Any Task
- [ ] Read relevant `.claude/` file for the task domain
- [ ] Check current branch state
- [ ] Verify no uncommitted changes

### Before Every Commit
- [ ] Build passes (0 errors)
- [ ] Migration size < 50KB
- [ ] No sensitive data
- [ ] Conventional commit message

### After Deployment
- [ ] Wait for GitHub Actions
- [ ] Test health endpoints
- [ ] Report status to user

---

## Decision Tree

```
┌─ New Task?
│
├─ Involves Data? ─────► Read 01-architecture.md
├─ Involves Auth? ─────► Read 02-security.md
├─ Involves Core? ─────► Read 03-core-framework.md
├─ Involves Git? ──────► Read 04-git-cicd.md
├─ Involves Logs? ─────► Read 05-observability.md
├─ Involves Audit? ────► Read 06-audit-logging.md
├─ Involves GDPR? ─────► Read 07-compliance.md
├─ Need Specs? ────────► Read 08-implementation-specs.md
│
└─ Ready to Commit? ───► ALWAYS run pre-commit verification
```

---

**Remember**: When in doubt, read the `.claude/` files. When unsure, ask the user. When implementing, follow the patterns.
