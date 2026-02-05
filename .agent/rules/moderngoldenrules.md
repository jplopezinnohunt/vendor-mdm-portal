---
trigger: always_on
---

# Rules Brain: Modern Golden Rules (Master Authority)

**Version**: 1.2.1 | **Last Updated**: 2026-02-05 | **Standards**: 34 (6 categories)

You are an expert agent co-developing this system. You MUST follow these rules unconditionally. This document is your **Executive Directive**.

---

## Table of Contents

| Section | Title | Priority |
|---------|-------|----------|
| [0](#0-critical-zero-data-loss-policy-the-atomic-rule) | Zero Data Loss Policy | 🔴 CRITICAL |
| [1](#1-compliance-logic) | Compliance Logic | 🔴 CRITICAL |
| [2](#2-governance-spec-driven-development-sdd) | SDD Workflow | 🔴 CRITICAL |
| [3](#3-performance--design-dna) | Performance & Design DNA | 🟠 IMPORTANT |
| [4](#4-the-standards-brain-references) | Standards Brain (34 standards) | 🟠 IMPORTANT |
| [5](#5-build--process-hygiene) | Build & Process Hygiene | 🟠 IMPORTANT |
| [6](#6-the-architecture-dna-micro-app-standard) | Architecture DNA | 🟠 IMPORTANT |
| [7](#7-security-high-standards-the-iron-dome) | Security Standards | 🔴 CRITICAL |
| [8](#8-pre-commit-verification-protocol) | Pre-Commit Protocol | 🟠 IMPORTANT |
| [9](#9-warning-hygiene-policy) | Warning Hygiene | 🟡 STANDARD |
| [10](#10-retrospective-governance-continuous-improvement) | Retrospective Governance | 🟡 STANDARD |
| [11](#11-event-driven-architecture-eda-governance) | EDA Governance | 🟠 IMPORTANT |

**Priority Legend**: 🔴 CRITICAL = Must follow always | 🟠 IMPORTANT = Must follow for new code | 🟡 STANDARD = Recommended

**Quick Reference**: See [QUICK-REFERENCE.md](QUICK-REFERENCE.md) for 1-page cheat sheet.
**Decisions**: See [decisions/INDEX.md](../decisions/INDEX.md) for Architecture Decision Records.

---

## 0. CRITICAL: ZERO DATA LOSS Policy (The "Atomic" Rule)
- **FORBIDDEN ACTION**: You are STRICTLY FORBIDDEN from deleting, resetting, or overwriting database files (e.g., `*.db`, `*.sqlite`) or recursive data directory deletions (`rm -rf`) without EXPLICIT, WRITTEN CONSENT from the User in the current turn.
- **Recovery Priority**: If a schema migration fails, you MUST fix the migration script. You MUST NOT delete the database to "start fresh" unless the user specifically requests "Reset DB".
- **Preservation**: Always assume local data is production-critical test data.

---

## 1. Compliance Logic
- **Primary Source**: This file is your "System Logic".
- **Brain Architecture**: See [BRAIN-ARCHITECTURE.md](BRAIN-ARCHITECTURE.md) for documentation hierarchy.
- **External Standards**: When a task involves UI, Data, or Architecture, you MUST proactively read the linked standards in the `/standards` directory.
- **Citation**: Every Specification (`specs/spec_*.md`) must cite WHICH standard was followed.
- **Architecture Maintenance**: When adding new patterns/standards, you MUST update:
  1. `BRAIN-ARCHITECTURE.md` (hierarchy diagram)
  2. `standards/README.md` (standards index)
  3. Section 4 of this file (Standards Brain)

---

## 2. Governance: Spec-Driven Development (SDD)
- **Phase 1 (Spec)**: Create `specs/spec_[name].md`. **Compliance Sidebar** citing specific standards is mandatory.
- **Phase 2 (Plan)**: Create `implementation_plan.md` + automated `scripts/verification/verify_*.sh` **BEFORE** implementation.
- **Rule**: Never execute code without an Approved Spec and Verification Script.
- **Branching**: Always `feature/[topic]` from `develop`. Never `main`.
- **Refusal Protocol**: Decline any "shortcuts" that bypass this governance.

---

## 3. Performance & Design DNA

### 3.1 Performance Targets (KPIs)

| Metric | Target | Measurement |
|--------|--------|-------------|
| **UI Response** | < 400ms | Time to interactive (Doherty Threshold) |
| **API Response (p50)** | < 100ms | Backend response time |
| **API Response (p95)** | < 500ms | Backend response time |
| **API Response (p99)** | < 1000ms | Backend response time |
| **Database Query** | < 50ms | Single query execution |
| **Build Time (Backend)** | < 60s | `dotnet build` |
| **Build Time (Frontend)** | < 30s | `npm run build` |
| **Test Coverage** | > 75% | Combined unit + integration |
| **Lighthouse Score** | > 90 | Performance + Accessibility |
| **Bundle Size** | < 500KB | Initial JS bundle (gzipped) |

### 3.2 Design Rules

- **Latency (UI)**: Follow the **Doherty Threshold** (<400ms). Mandatory loading states and skeleton loaders.
- **Search (Data)**: Use **PostgreSQL Generated Columns** + Indexes for frequent search targets in JSONB.
- **Async Side-Effects**: Use Domain Events for non-transactional work (Email, SAP, Logging).

---

## 4. The Standards Brain (References)

**Index**: See [standards/README.md](standards/README.md) for full list.

**34 standards** organized in **6 categories**:

| Category | Count |
|----------|-------|
| Architecture & Design | 5 |
| Core Development | 6 |
| Security & Compliance | 4 |
| Integration & Infrastructure | 5 |
| Operations & Quality | 7 |
| Governance & Process | 7 |
| **Total** | **34** |

You MUST load and apply the relevant standard based on task type.

### Category 1: Architecture & Design (5 standards)
| Pattern | Standard |
|---------|----------|
| Hexagonal Architecture | [hexagonal-architecture-standards.md](standards/hexagonal-architecture-standards.md) |
| Hybrid Data Model | [data-model-standards.md](standards/data-model-standards.md) |
| Ontology Modeling | [ontology-modeling-standard.md](standards/ontology-modeling-standard.md) |
| Repository Pattern | [repository-pattern-standard.md](standards/repository-pattern-standard.md) |
| API Versioning | [api-versioning-standard.md](standards/api-versioning-standard.md) |

### Category 2: Core Development (6 standards)
| Pattern | Standard |
|---------|----------|
| Result Pattern | [result-pattern-standard.md](standards/result-pattern-standard.md) |
| Structured Logging | [logging-standard.md](standards/logging-standard.md) |
| State Machines | [state-machine-standard.md](standards/state-machine-standard.md) |
| Event-Driven Architecture | [event-driven-architecture-standard.md](standards/event-driven-architecture-standard.md) |
| Testing | [testing-standard.md](standards/testing-standard.md) |
| Error Handling | [error-handling-standard.md](standards/error-handling-standard.md) |

### Category 3: Security & Compliance (4 standards)
| Pattern | Standard |
|---------|----------|
| Security Architecture | [security-architecture.md](standards/security-architecture.md) |
| Audit Logging | [audit-log-integration-standards.md](standards/audit-log-integration-standards.md) |
| Soft Delete | [soft-delete-standard.md](standards/soft-delete-standard.md) |
| GDPR & PII | [gdpr-pii-standard.md](standards/gdpr-pii-standard.md) |

### Category 4: Integration & Infrastructure (5 standards)
| Pattern | Standard |
|---------|----------|
| SAP Integration | [sap-integration-standard.md](standards/sap-integration-standard.md) |
| File Storage | [file-storage-standard.md](standards/file-storage-standard.md) |
| Email Service | [email-service-standard.md](standards/email-service-standard.md) |
| Multi-Tenancy | [multi-tenancy-standard.md](standards/multi-tenancy-standard.md) |
| Data Residency | [data-residency-standard.md](standards/data-residency-standard.md) |

### Category 5: Operations & Quality (7 standards)
| Pattern | Standard |
|---------|----------|
| CI/CD Setup | [cicd-setup-standards.md](standards/cicd-setup-standards.md) |
| Database Migrations | [database-migration-standards.md](standards/database-migration-standards.md) |
| Git & SAP Branching | [git-branching-sap-standards.md](standards/git-branching-sap-standards.md) |
| Rate Limiting | [rate-limiting-standard.md](standards/rate-limiting-standard.md) |
| Performance | [performance-generated-columns.md](standards/performance-generated-columns.md) |
| UI Design | [ui-design-standards.md](standards/ui-design-standards.md) |
| Accessibility (WCAG) | [accessibility-standard.md](standards/accessibility-standard.md) |

### Category 6: Governance & Process (7 standards)
| Pattern | Standard | Section |
|---------|----------|---------|
| Zero Data Loss | [zero-data-loss-standard.md](standards/zero-data-loss-standard.md) | 0 |
| Compliance Logic | [compliance-logic-standard.md](standards/compliance-logic-standard.md) | 1 |
| SDD Workflow | [sdd-workflow-standard.md](standards/sdd-workflow-standard.md) | 2 |
| Build Hygiene | [build-hygiene-standard.md](standards/build-hygiene-standard.md) | 5 |
| Pre-Commit Protocol | [pre-commit-standard.md](standards/pre-commit-standard.md) | 8 |
| Warning Hygiene | [warning-hygiene-standard.md](standards/warning-hygiene-standard.md) | 9 |
| Retrospective Governance | [retrospective-standard.md](standards/retrospective-standard.md) | 10 |

### Standard Selection Decision Tree

```
What are you implementing?
│
├─► New API endpoint?
│   ├─► Read: hexagonal-architecture-standards.md
│   ├─► Read: api-versioning-standard.md
│   └─► Read: result-pattern-standard.md
│
├─► Database changes?
│   ├─► Read: database-migration-standards.md
│   └─► Read: data-model-standards.md
│
├─► State changes (status transitions)?
│   ├─► Read: state-machine-standard.md
│   └─► Read: event-driven-architecture-standard.md
│
├─► External integration (SAP, Email)?
│   ├─► Read: sap-integration-standard.md (if SAP)
│   ├─► Read: email-service-standard.md (if Email)
│   └─► Read: event-driven-architecture-standard.md
│
├─► UI component?
│   ├─► Read: ui-design-standards.md
│   └─► Read: accessibility-standard.md
│
├─► Security feature?
│   ├─► Read: security-architecture.md
│   └─► Read: gdpr-pii-standard.md (if PII involved)
│
├─► Error handling?
│   ├─► Read: error-handling-standard.md
│   └─► Read: result-pattern-standard.md
│
├─► Testing?
│   └─► Read: testing-standard.md
│
└─► Not sure?
    └─► Start with: hexagonal-architecture-standards.md
```

### Pattern Violations (FORBIDDEN)

❌ Business logic in Controllers or Services
❌ Hard deletes (use soft delete)
❌ Hardcoded secrets (use KeyVault/UserSecrets)
❌ Returning SQL Entities from API (use DTOs)
❌ Throwing exceptions for business failures (use Result)
❌ String interpolation in logs
❌ Cross-tenant data access without admin check

### Future Patterns (Roadmap)

- [x] API Versioning ✅ (now in standards)
- [x] Accessibility (WCAG) ✅ (now in standards)
- [ ] Circuit Breaker
- [ ] Response Caching
- [ ] Background Jobs
- [ ] Feature Flags

---

## 5. Build & Process Hygiene
- **Clean Sweep Protocol**: Before builds or migrations, execute `pkill -f dotnet` and clean `bin/obj` artifacts to prevent Exit Code 143/134.
- **Interface Integrity**: When changing an interface, update ALL implementations (Mock, Real, Simulation, Test) in one atomic turn.
- **Duplicate Type Check**: Before creating new classes/constants, ALWAYS search for existing definitions:
  ```bash
  grep -r "class TypeName\|static class TypeName" backend/
  ```
  **Rationale**: Prevents CS0101 duplicate type errors (learned from DocumentStatus incident).
- **Hygiene**: Pinned dependencies, `no-any` TypeScript, mandatory verification scripts with auth headers.
- **Observability**: `traceparent` propagation + `TraceId` UI overlays.
- **Simulation**: [SIMULATION MODE] logs for all external mocks.

---

## 6. The Architecture DNA (Micro-App Standard)
**Status**: MANDATORY for all new features.

1.  **The Ontology Rule**: See [ontology-modeling-standard.md](standards/ontology-modeling-standard.md) for full definition.
2.  **The Core Framework**: Apps MUST depend on `VendorMdm.Core.Framework` for base interfaces (`IOntologyConcept`, `IUserContext`).
3.  **App-Scoped Security**: Authorization MUST be Context-Aware. `IUserContext.HasRoleForApp` is the only valid check.
4.  **No Entity Leaks**: APIs MUST return DTOs (`Shared.Contracts`). Returning SQL Entities is FORBIDDEN.
5.  **Observability**: Every Concept MUST implement `GetFunctionalLogs()`. Traceability from API -> Concept -> DB is required.

---

## 7. Security High Standards (The Iron Dome)
**Status**: ZERO TOLERANCE for violations.

### A. Authentication & Session
-   **No Hardcoded Secrets**: All keys MUST come from KeyVault (Prod) or UserSecrets (Dev).
-   **Signed Impersonation**: Impersonation cookies/tokens MUST be cryptographically signed.
-   **Session Lifetime**: MUST be Configurable (Admin Parameter). Default: **2 Hours** (Corporate standard for internal apps).
-   **Session Storage**: Store `sessionTimestamp` in localStorage on login; check expiration on app load.
-   **Session Cleanup**: On expiration, clear all auth data: `localToken`, `mockUser`, `sessionTimestamp`.
-   **Token Storage**: Standardize on single key `localToken` (not `token`, `authToken`, etc.).
-   **Ghost User Block**: Users present in Azure AD but missing from DB MUST be blocked in Production.

### B. Network & Transport
-   **Strict Headers**: `HSTS` (Strict-Transport-Security), `CSP` (Content-Security-Policy), and `X-Frame-Options: DENY` are MANDATORY.
-   **CSP for WebSockets (Dev)**: `connect-src` MUST include `ws://localhost:* wss://localhost:*` for SignalR in development.
-   **WebSocket Auth Pattern**: WebSockets CANNOT send custom HTTP headers. Use query string for mock auth (`?mockUser=Role`), use `accessTokenFactory` for real JWT tokens.
-   **Backend WebSocket Auth**: Middleware MUST check BOTH `X-Mock-User` header AND `?mockUser` query param for hub paths.
-   **CORS Strictness**: Production CORS MUST be restricted to the specific `App:BaseUrl`. NO Localhost allowed in Prod.
-   **Rate Limiting**: All Public (`AllowAnonymous`) endpoints MUST have IP-based Rate Limiting (5 req/min).
-   **Environment Detection**: NEVER use `env.IsStaging()` - it doesn't exist. Use `env.EnvironmentName == "Staging"`:
    ```csharp
    // ❌ BROKEN (compiles but fails at runtime)
    if (env.IsStaging()) { ... }

    // ✅ CORRECT
    if (env.EnvironmentName == "Staging") { ... }
    ```
-   **Header Syntax**: Use indexer syntax, NOT `Add()` method (prevents ASP0019 warning):
    ```csharp
    // ❌ FORBIDDEN (throws on duplicate keys)
    context.Response.Headers.Add("X-Frame-Options", "DENY");

    // ✅ CORRECT (idempotent, no exceptions)
    context.Response.Headers["X-Frame-Options"] = "DENY";
    ```

### C. Input Hygiene
-   **Anti-XSS**: All DTO strings MUST be sanitized (`IInputSanitizer`) before reaching the Domain Layer.
-   **DTO Enforcement**: Never accept raw JSONB or Entity objects from the client.

### D. Input Validation & Sanitization
-   **Global Action Filter**: Register `InputSanitizationActionFilter` to scan all DTO properties automatically.
-   **IInputSanitizer Interface**: Use `Core.Framework/Security/IInputSanitizer.cs` for consistent sanitization.
-   **Performance Target**: <10ms per request for reflection-based property scanning.
-   **Pattern**: Sanitize at API boundary, validate in Domain/Concept layer.
    ```csharp
    // Program.cs - Register global filter
    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<InputSanitizationActionFilter>();
    });
    ```

---

## 8. Pre-Commit Verification Protocol
**MANDATORY CHECKS** before every commit:

### 1. Build Verification
```bash
# Backend
cd backend/VendorMdm.Api
dotnet build --configuration Release
# Expected: Build succeeded, 0 Error(s)

# Frontend
cd frontend
npm run build
# Expected: ✓ built in ~4s, 0 errors
```

### 2. Migration Size Check
```bash
ls -lh backend/VendorMdm.Api/Migrations/*.cs | grep -v Designer | grep -v Snapshot
# All files must be < 50KB
# If any file > 50KB, STOP and split migration
```

### 2.1 Database Migration Deployment (CRITICAL)

**Quick Reference**:
- **Local**: `dotnet ef database update` (direct)
- **Azure**: GitHub Actions workflow ONLY (never direct EF commands)

**Full Details**: See [database-migration-standards.md](standards/database-migration-standards.md) for:
- Environment-specific process
- CI/CD troubleshooting guide
- SQLite ↔ SQL Server type mapping
- Workflow step-by-step

### 3. Alignment Verification
```bash
./scripts/verify-alignment.sh
# Expected: ✓ ALL CHECKS PASSED
```

### 4. Git Status Review
```bash
git status
# Review all changed files
# Ensure no unintended changes
# Verify no sensitive data (keys, passwords)
```

### 5. Warning Review
```bash
# Review all build warnings
# Fix critical warnings immediately
# Document acceptable warnings in commit message
```

### 6. Verification Script Pattern
**IMPORTANT**: Verification scripts must handle errors per-test, not exit on first failure:
```bash
# ❌ DON'T: Exit on first error (stops script early)
set -e
curl http://localhost:5001/health  # Fails → script stops

# ✅ DO: Accumulate failures, exit at end
FAIL_COUNT=0
if ! curl -s http://localhost:5001/health; then
    echo "FAIL: Health check"
    ((FAIL_COUNT++))
fi
# ... more tests ...
if [ $FAIL_COUNT -gt 0 ]; then
    echo "FAILED: $FAIL_COUNT tests"
    exit 1
fi
```

**AGENT BEHAVIOR**:
- Agent MUST run these checks before proposing commit
- Agent MUST report any failures to user
- Agent MUST NOT commit if checks fail
- Agent MUST suggest fixes for any issues found

**Exceptions**:
- Hotfixes may skip alignment verification if time-critical
- Must be explicitly approved by user
- Must be documented in commit message

---

## 9. Warning Hygiene Policy

**TARGET**: Zero warnings in production builds.

### Acceptable Warnings
- **Obsolete Property Warnings**: If migration to new pattern is planned and documented
- **Nullable Reference Warnings**: If false positive (verified manually)
- **Performance Suggestions**: If not critical to current release

### Unacceptable Warnings
- ❌ Duplicate keys in object literals
- ❌ Unused variables or imports
- ❌ Unreachable code
- ❌ Type mismatches
- ❌ Missing await on async calls

### Warning Categories

**Critical (Fix Immediately)**:
```
- CS0618: Obsolete member usage (if no migration plan)
- CS8600-CS8629: Nullable reference warnings (potential NullReferenceException)
- ASP0019: Headers.Add() may throw on duplicate - use indexer syntax instead
- TS2322: Type mismatch
- TS2345: Argument type mismatch
```

**Important (Fix Before Merge)**:
```
- CS0162: Unreachable code
- CS0219: Variable assigned but never used
- TS6133: Declared but never used
- TS7006: Implicit 'any' type
```

**Minor (Fix When Convenient)**:
```
- CS1591: Missing XML documentation
- Performance suggestions
- Code style warnings
```

### Agent Behavior

**Before Commit**:
1. ✅ Review all warnings from build output
2. ✅ Categorize warnings (Critical/Important/Minor)
3. ✅ Fix all Critical warnings
4. ✅ Fix all Important warnings
5. ✅ Document Minor warnings in commit message

**Commit Message Format**:
```
feat: Add new feature

WARNINGS:
- CS1591: Missing XML docs (will add in next PR)
- Performance: Large bundle size (will optimize later)
```

**Reporting**:
- Agent MUST report warning count to user
- Agent MUST highlight critical warnings
- Agent MUST suggest fixes for warnings
- Agent MUST create issues for deferred warnings

**Example**:
```
⚠️ Build Warnings Summary:
- Critical: 0
- Important: 2 (unused variables - fixed)
- Minor: 5 (XML docs - deferred)

All critical and important warnings resolved.
Minor warnings documented in commit message.
```

---

## 10. Retrospective Governance (Continuous Improvement)

**Status**: MANDATORY for agents after each significant implementation.

### Purpose
Capture lessons learned to prevent repeating mistakes and improve agent effectiveness across sessions.

### Value Proposition
- **Prevents Bugs**: Document runtime issues (e.g., `env.IsStaging()` doesn't exist)
- **Saves Time**: 15-30 min saved per future session by avoiding known pitfalls
- **Improves Brain**: Retrospectives feed updates to this file
- **Organizational Memory**: Future agents learn from past implementations

### Structure

```
.agent/retrospectives/
  ├── INDEX.md                  ← Check Pending count (skip if 0)
  ├── active/                   ← Current quarter (max 10 files)
  │   └── YYYY-QX-topic.md
  ├── archived/                 ← Past quarters (historical only)
  │   └── YYYY-QX-summary.md
  └── learnings-database.md     ← Aggregated patterns (optional)
```

### Agent Workflow

**Before Starting Work**:
1. Check INDEX.md for `Brain Rules Pending` count
2. If Pending = 0 → Skip INDEX.md (brain already has all learnings)
3. If Pending > 0 → Read and apply pending learnings first

**After Completing Work** (for significant features):
1. Document issues encountered in retrospective
2. Update `INDEX.md` with top 3-5 learnings
3. **MANDATORY: Apply learnings to brain rules immediately**
   - Update relevant sections in this file (moderngoldenrules.md)
   - Mark as `[x] Applied` in INDEX.md
   - Commit rule updates with retrospective reference
4. Do NOT leave "Pending" items - apply them before closing the task

**Efficiency Principle**: Once applied to brain, retrospective is historical only

### End of Conversation Protocol (MANDATORY)

Before closing any significant conversation:

```
1. IDENTIFY learnings from this conversation
   └── What bugs were found?
   └── What patterns worked/failed?
   └── What should future agents know?

2. DOCUMENT in retrospective (INDEX.md)
   └── Add to "Top Critical Learnings" section
   └── Mark source and date

3. APPLY to brain rules immediately ← CRITICAL
   └── Update relevant section in moderngoldenrules.md
   └── Update relevant standard in standards/*.md
   └── Mark as [x] Applied in INDEX.md

4. COMMIT all changes
   └── Brain rules + retrospective + code
   └── Push to develop

5. VERIFY Pending = 0
   └── No orphan learnings
   └── Next conversation starts clean
```

**FORBIDDEN**: Closing conversation with Pending > 0

### INDEX.md Format

```markdown
# Retrospectives Index
**Last Updated**: YYYY-MM-DD

## Top 5 Critical Learnings
1. ❌ ISSUE → ✅ SOLUTION (Source: YYYY-MM-DD Topic)
2. ...

## Pending Brain Rule Updates
- [ ] Section X.Y: Add pattern Z
- [ ] Section A.B: Update example

## Active Retrospectives (Current Quarter)
- [YYYY-MM-DD: Topic](active/YYYY-QX-topic.md) - Key: ...
```

### Retention Policy

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

### What to Document

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

### Integration with Brain Rules

**Retrospective → Brain Rule Lifecycle**:
```
1. Implementation finds issue
   ↓
2. Documented in retrospective
   ↓
3. Added to INDEX.md
   ↓
4. Brain rule updated (this file)
   ↓
5. Retrospective marked as "Applied: ✅"
   ↓
6. Future agents follow updated rule (no repeat bug)
```

### Example Learnings

**ASP.NET Core Patterns**:
- ❌ `env.IsStaging()` doesn't exist → ✅ Use `env.EnvironmentName == "Staging"`
- ❌ `Headers.Add()` throws ASP0019 → ✅ Use `Headers["X-Frame"] = "DENY"`

**Tool Workarounds**:
- ✅ Write tool requires read first → Use bash heredoc for new files
- ⚠️ Verification scripts with `set -e` exit early → Handle errors per-test

**Performance Baselines**:
- ✅ Input sanitization: <10ms per request (reflection-based)
- ✅ Security headers: <5ms overhead (early pipeline)

### Size Management

**Target Sizes**:
- `INDEX.md`: 50-200 lines (quick read)
- Individual retrospective: 300-500 lines (detailed)
- Learnings database: 500-1000 lines (comprehensive)

**File Count Limits**:
- Active: Max 10-12 files per quarter
- Archived: Max 4-8 quarterly summaries
- Total: ~15 files maximum (with 2-year purge)

### Success Metrics

**Effectiveness Indicators**:
- Repeated bugs decrease over time
- Implementation speed increases (fewer trial-and-error)
- Brain rule updates cite retrospective evidence
- New agents ramp up faster (read INDEX.md)

---

## 11. Event-Driven Architecture (EDA) Governance

**Status**: MANDATORY for all features involving state changes or integrations.

### 11.1 Proactive Evaluation Requirement

**CRITICAL**: Agents MUST proactively evaluate EDA requirements when:
- Implementing features with state changes (status transitions, workflows)
- Building integrations with external systems (SAP, Salesforce, Email)
- Adding functionality that requires real-time frontend updates
- Creating asynchronous side-effects (notifications, logging, sync)

**DO NOT** wait for the user to ask about events. Evaluate EDA applicability immediately.

### 11.2 EDA Checklist (Mandatory During Spec Phase)

When creating a specification (`specs/spec_*.md`), evaluate and document:

```markdown
## Event-Driven Architecture Evaluation

### Events to Emit
| Event Type | Trigger | Handlers Needed |
|------------|---------|-----------------|
| [EntityCreated] | [Create operation] | SignalR, Outbox |
| [StatusChanged] | [Status transition] | SignalR, SAP Sync |

### Real-Time Requirements
- [ ] Frontend needs push updates
- [ ] External system needs notification
- [ ] Audit trail requires event logging

### Integration Pattern
- [ ] In-process dispatch (IEventHandler<T>)
- [ ] Outbox for guaranteed delivery
- [ ] Service Bus for external systems
- [ ] SignalR for frontend push
```

(See Section 11.2 checklist above)

### 11.3 Implementation Requirements

**Pattern 6 Extended: Event Sourcing**

Every domain action that changes state MUST:
1. Create a strongly-typed domain event (Core.Framework.Events)
2. Add event to Outbox in same transaction as entity change
3. Dispatch to in-process handlers after save
4. Log to Cosmos for audit trail

**Example Implementation**:
```csharp
// After entity save
var statusChangedEvent = new VendorStatusChangedEvent(vendor.Id, oldStatus, newStatus);
_context.AddToOutbox(statusChangedEvent);  // Guaranteed delivery
await _context.SaveChangesAsync();
await _dispatcher.DispatchAsync(statusChangedEvent);  // In-process (SignalR)
```

### 11.4 SignalR Events (Frontend Push)

**Mandatory Events** (push to connected clients):
- `StatusChanged`: Any entity status transition
- `VendorCreated`: New vendor created
- `TaskAssigned`: Workflow task assigned to user
- `Notification`: User-targeted notifications
- `SapSyncResult`: SAP integration completed/failed

**Hub Endpoint**: `/hubs/events`

### 11.5 Agent Behavior

**Before Implementation**:
1. ✅ Evaluate if feature involves state changes or integrations
2. ✅ Document events in spec (Section 12.2 checklist)
3. ✅ Identify real-time requirements
4. ✅ Plan event handlers needed

**During Implementation**:
1. ✅ Emit domain events from Concepts/Services
2. ✅ Add to Outbox for guaranteed delivery
3. ✅ Dispatch to in-process handlers
4. ✅ Connect SignalR for frontend updates

**Violation**: Implementing state-changing features without EDA evaluation is a governance violation.

### 11.6 Reference Files

| Component | Location |
|-----------|----------|
| Domain Events | `Core.Framework/Events/DomainEvents.cs` |
| Event Dispatcher | `Api/Services/Events/DomainEventDispatcher.cs` |
| SignalR Hub | `Api/Hubs/EventHub.cs` |
| Outbox Entity | `Shared/Models/OutboxEvent.cs` |
| Frontend Context | `frontend/src/context/SignalRContext.tsx` |
| Frontend Hooks | `frontend/src/hooks/useSignalR.ts` |
| Standard | `.agent/rules/standards/event-driven-architecture-standard.md` |

---

