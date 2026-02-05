# Vendor MDM Portal - Backlog & Strategy

**Version**: 1.1.0 | **Last Updated**: 2026-02-05 | **Status**: Active

> **Reference**: This document is referenced from [Specifications Index](../.agent/rules/specs/INDEX.md).

---

## Prioritization Framework

### Priority Levels

| Level | Criteria | SLA |
|-------|----------|-----|
| 🔴 **P0 - Critical** | Blocks production, security risk, data loss | Immediate |
| 🟠 **P1 - High** | Blocks major features, quality issues | This sprint |
| 🟡 **P2 - Medium** | Improves quality, reduces tech debt | Next sprint |
| 🟢 **P3 - Low** | Nice to have, future enhancement | Backlog |

### Effort Estimates

| Size | Effort | Example |
|------|--------|---------|
| **XS** | < 2 hours | Add missing reference |
| **S** | 2-4 hours | Create test file |
| **M** | 1-2 days | Implement CI workflow |
| **L** | 3-5 days | Full feature |
| **XL** | 1-2 weeks | Major integration |

---

## 1. Critical (P0) - Immediate Action Required

### 1.1 🔴 Re-enable Azure AD Authentication

**Issue**: Authentication disabled in Azure DEV since 2024-12-11
**Impact**: Security vulnerability, improper access control in dev environment
**Effort**: S (2-4 hours)

**Reference**: [docs/PENDING-AUTH-REENABLE.md](PENDING-AUTH-REENABLE.md)

**Action Items**:
- [ ] Review current authSettings in Azure Portal
- [ ] Update main.bicep with proper authSettings
- [ ] Test authentication flow
- [ ] Update documentation

---

## 2. High Priority (P1) - This Sprint

### 2.1 🟠 Fix Backend Project Structure

**Issue**: Missing solution file and project references
**Impact**: Build complexity, duplicate models, IDE issues
**Effort**: XS (< 2 hours)

**Source**: [project-structure.md](architecture/project-structure.md)

**Action Items**:
- [ ] Create `backend/VendorMdm.sln` solution file
- [ ] Add `VendorMdm.Shared` reference to `VendorMdm.Api`
- [ ] Remove duplicate models from Api project
- [ ] Verify build works with solution file

### 2.2 🟠 Implement CI/CD Pipelines

**Issue**: No automated build, test, or deployment
**Impact**: Manual errors, slow feedback, no quality gate
**Effort**: M (1-2 days)

**Source**: [project-structure.md](architecture/project-structure.md) - Score 0/10

**Action Items**:
- [ ] Create `.github/workflows/ci.yml` (build + test on PRs)
- [ ] Create `.github/workflows/deploy-dev.yml` (deploy to dev on merge)
- [ ] Create `.github/workflows/deploy-prod.yml` (deploy to prod with approval)
- [ ] Add branch protection rules

### 2.3 🟠 Add Backend Test Coverage

**Issue**: No unit or integration tests for backend
**Impact**: Regressions, low confidence in changes
**Effort**: L (3-5 days for initial coverage)

**Source**: [project-structure.md](architecture/project-structure.md) - Score 3/10

**Action Items**:
- [ ] Create `backend/VendorMdm.Api.Tests` project (xUnit)
- [ ] Add tests for InvitationService
- [ ] Add tests for VendorApplicationService
- [ ] Add integration tests for key API endpoints
- [ ] Target: 50% coverage initially, 75% goal

---

## 3. Medium Priority (P2) - Next Sprint

### 3.1 🟢 Security Hardening (MOSTLY COMPLETE)

**Status**: ✅ Core security implemented (verified 2026-02-05)
**Effort**: Remaining items only

**Completed**:
- [x] API rate limiting for public endpoints (`Program.cs:128-133`, 5+ endpoints)
- [x] Security headers (HSTS, CSP, X-Frame-Options) via `SecurityHeadersMiddleware.cs`
- [x] Input sanitization via `InputSanitizationActionFilter` (global filter)
- [x] CSP policies with WebSocket support

**Remaining**:
- [ ] Remove hardcoded credentials from Bicep templates
- [ ] Security audit of Key Vault usage

### 3.2 🟡 Expand Frontend Testing

**Issue**: Only 4 frontend test files exist (Elements, AuthContext, SignalRContext, AccessibleModal)
**Impact**: UI regressions, limited coverage
**Effort**: M (1-2 days)

**Current State**:
- `tests/Elements.test.tsx` - Button component
- `tests/context/AuthContext.test.tsx` - Auth context
- `tests/context/SignalRContext.test.tsx` - SignalR context
- `tests/components/AccessibleModal.test.tsx` - Accessibility
- ✅ CI now runs `npm run test:run` (added 2026-02-05)

**Action Items**:
- [ ] Add tests for VendorRegistration page
- [ ] Add tests for InvitationRegistration page
- [ ] Add service layer mock tests (vendorService, eventService)
- [ ] Consider E2E testing with Playwright

### 3.3 🟡 Document Taxonomy Implementation

**Issue**: Document classification system not fully implemented
**Impact**: Limited document management capabilities
**Effort**: M (1-2 days)

**Reference**: [docs/features/attachments/](features/attachments/)

**Action Items**:
- [ ] Implement document type classification
- [ ] Add required vs optional document rules
- [ ] Implement validation rules per vendor type

### 3.4 🟡 Process Pending Analysis Files

**Issue**: 14 files still need analysis and organization
**Impact**: Scattered documentation, unclear status
**Effort**: S (2-4 hours)

**Files in `docs/pending-analysis/`**:
- [ ] `rules/DATABASE_SCHEMA.md` - Move to reference
- [ ] `rules/git-workflow-best-practices.md` - Consolidate with standards
- [ ] `architecture/*.md` (5 files) - Evaluate relevance
- [ ] `implementation/*.md` (7 files) - Archive or update
- [ ] `integration/*.md` (2 files) - Keep as backlog reference

---

## 4. Low Priority (P3) - Future Sprints

### 4.1 🟢 SAP Integration (Phase 1)

**Status**: ⏸️ Waiting for SAP D01 Access
**Effort**: XL (1-2 weeks)

**Reference**: [docs/backlog/sap-simulation/](backlog/sap-simulation/)

**When to Activate**: After SAP D01 environment access is granted

**Scope**:
- [ ] Implement real SAP BAPI client
- [ ] Replace mock service with real integration
- [ ] Test vendor creation in SAP
- [ ] Test vendor update sync

### 4.2 🟢 Sanctions Screening Service

**Status**: 📋 Planned
**Effort**: L (3-5 days)

**Reference**: [docs/pending-analysis/integration/sanctions-screening-service-plan.md](pending-analysis/integration/sanctions-screening-service-plan.md)

**Phases**:
1. [ ] Implement Mock service with test cases
2. [ ] Integrate free OFAC API
3. [ ] Add compliance review UI
4. [ ] Evaluate commercial providers

### 4.3 🟢 Email Improvements

**Effort**: M (1-2 days)

**Action Items**:
- [ ] Reminder emails before invitation expiry
- [ ] HTML email templates
- [ ] Multi-language support
- [ ] Bulk invitation upload (CSV)

### 4.4 🟢 Analytics Dashboard

**Effort**: L (3-5 days)

**Action Items**:
- [ ] Invitation metrics (completion rate, avg time)
- [ ] Vendor onboarding funnel
- [ ] Approval workflow metrics
- [ ] System health dashboard

### 4.5 🟢 Advanced Features

**Effort**: Various

**Ideas**:
- [ ] Vendor risk scoring
- [ ] Automated vendor re-verification
- [ ] API for external integrations
- [ ] Mobile-responsive improvements
- [ ] Offline capability (PWA)

---

## 5. Completed Items

### Recently Completed (2026-02-05)

| Item | Status | Notes |
|------|--------|-------|
| ✅ Documentation cleanup | Complete | Organized 37+ files |
| ✅ Golden Rules v1.3.0 → v1.7.0 | Complete | Added Pre-Merge Protocol, Self-Audit Gates |
| ✅ Visual architecture guides | Complete | Consolidated to 3 guides |
| ✅ Solution Specification | Complete | Modular specs in `.agent/rules/specs/solution/` |
| ✅ Functional Flows (5 roles) | Complete | Admin, Approver, Requester, Vendor, MD-Team |
| ✅ Business Processes (5 flows) | Complete | Direct/Event Invitation, Self/MD Modification, MD Creation |
| ✅ Entity-Process Map | Complete | Shows how entities connect to processes |
| ✅ Specs Structure | Complete | solution/, functional/, processes/, features/ |
| ✅ CI/CD - Frontend tests | Complete | Added `npm run test:run` to verify-pr.yml |
| ✅ CI/CD - Migration validation | Complete | Added size check + conflict marker check |
| ✅ Pre-commit hooks | Complete | `scripts/hooks/pre-commit` + install script |
| ✅ Security audit | Complete | Verified: Rate limiting, CSP, Input sanitization all implemented |
| ✅ Brain rules accuracy | Complete | Updated Section 7, 8 with implementation status |
| ✅ CodeQL security scanning | Complete | `.github/workflows/security-scan.yml` |
| ✅ Dependency review | Complete | Runs on PRs, blocks high-severity + GPL |
| ✅ Secrets scanning | Complete | TruffleHog integration |
| ✅ Frontend tests expanded | Complete | 7 test files (72+ passing tests) |

---

## 6. Sprint Planning Guide

### Recommended First Sprint

**Theme**: Foundation & Quality

| Item | Priority | Effort | Total |
|------|----------|--------|-------|
| Re-enable Azure AD Auth | P0 | S | 2-4h |
| Fix project structure | P1 | XS | 2h |
| Create CI workflow | P1 | S | 4h |
| Add first backend tests | P1 | M | 8h |
| **Total** | - | - | **~18h** |

### Recommended Second Sprint

**Theme**: Security & Testing

| Item | Priority | Effort | Total |
|------|----------|--------|-------|
| Deploy workflows | P1 | S | 4h |
| More backend tests | P1 | M | 8h |
| Security hardening | P2 | M | 8h |
| Frontend tests | P2 | S | 4h |
| **Total** | - | - | **~24h** |

---

## 7. Dependencies Map

```
┌─────────────────────────────────────────────────────────────────┐
│                    DEPENDENCY GRAPH                              │
│                                                                  │
│  [Re-enable Auth]                                                │
│        │                                                         │
│        └──► [Security Hardening] ──► [Production Release]       │
│                                                                  │
│  [Fix Project Structure]                                         │
│        │                                                         │
│        └──► [Backend Tests] ──► [CI/CD] ──► [Production Release]│
│                    │                                             │
│                    └──► [Integration Tests] ──► [SAP Integration]│
│                                                                  │
│  [SAP D01 Access]                                                │
│        │                                                         │
│        └──► [SAP Integration] ──► [Sanctions Screening]         │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 8. Review Schedule

| Review Type | Frequency | Participants |
|-------------|-----------|--------------|
| Backlog Grooming | Weekly | Team |
| Priority Review | Bi-weekly | Tech Lead + PO |
| Strategy Review | Monthly | All stakeholders |

---

## Change Log

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2026-02-05 | Initial Backlog created from docs analysis |

---

*This document is maintained as part of project governance. Items move through priorities based on business needs and dependencies.*
