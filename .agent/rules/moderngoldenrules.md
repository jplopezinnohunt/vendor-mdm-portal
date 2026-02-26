---
trigger: always_on
---

# Rules Brain: Modern Golden Rules (Master Authority)

**Version**: 1.9.0 | **Last Updated**: 2026-02-26 | **Standards**: 35 (6 categories)

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
| [10](#10-retrospective-governance-continuous-improvement) | Retrospective Governance | 🔴 CRITICAL |
| [11](#11-critical-thinking--continuous-improvement) | Critical Thinking | 🔴 CRITICAL |
| [12](#12-dependency-health-awareness) | Dependency Health Awareness | 🟠 IMPORTANT |
| [13](#13-canonical-entity-decoupling-sap-independence) | Canonical Entity Decoupling | 🟠 IMPORTANT |
| [14](#14-event-driven-architecture-eda-governance) | EDA Governance | 🟠 IMPORTANT |
| [15](#15-pre-merge-build-protocol) | Pre-Merge Build Protocol | 🔴 CRITICAL |
| [16](#16-self-audit--enforcement-gates) | Self-Audit & Enforcement | 🔴 CRITICAL |

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
- **Specifications**: See [specs/INDEX.md](specs/INDEX.md) for complete specs hierarchy:
  - `specs/solution/` - Current system state (WHAT exists)
  - `specs/functional/` - Role-based flows (WHO does what)
  - `specs/processes/` - Business processes (HOW things happen)
  - `specs/features/` - In-progress work (temporal)
- **Backlog**: See [docs/BACKLOG.md](../../docs/BACKLOG.md) for prioritized work items.
- **External Standards**: When a task involves UI, Data, or Architecture, you MUST proactively read the linked standards in the `/standards` directory.
- **Citation**: Every Specification (`specs/spec_*.md`) must cite WHICH standard was followed.
- **Architecture Maintenance**: When adding new patterns/standards, you MUST update:
  1. `BRAIN-ARCHITECTURE.md` (hierarchy diagram)
  2. `standards/README.md` (standards index)
  3. Section 4 of this file (Standards Brain)

### 1.1 Solution Context Protocol (MANDATORY)

**Status**: 🔴 CRITICAL | Agents MUST understand the system before modifying it.

#### Before Starting Work (READ)

**MANDATORY**: Read solution specs to understand WHAT exists before any implementation task.

```
Agent Workflow - START OF CONVERSATION:
1. Read specs/solution/INDEX.md     ← System overview
2. Read specs/solution/CORE.md      ← Entities, architecture, tech stack
3. Read specs/solution/FLOWS.md     ← State machines, workflows
4. Read specs/solution/INTEGRATIONS.md ← External systems
```

**Why**: Prevents duplicate code, conflicting patterns, and architectural drift.

**Verification**: Agent MUST be able to answer:
- What are the core entities?
- What state machines exist?
- What integrations are active vs. mocked?
- What patterns does this codebase use?

#### After Completing Work (WRITE)

**MANDATORY**: Update solution specs when features are completed or system changes.

```
Agent Workflow - END OF IMPLEMENTATION:
1. IF new entity added      → Update CORE.md (entities section)
2. IF new workflow/state    → Update FLOWS.md (add state machine)
3. IF new integration       → Update INTEGRATIONS.md (add system)
4. IF new process           → Create/update processes/*.md
5. IF role capability added → Update functional/*.md (role file)
```

**Update Checklist**:
| Change Type | Update Required |
|-------------|-----------------|
| New Entity | CORE.md → Entities section |
| New State Machine | FLOWS.md → Add diagram |
| New Integration | INTEGRATIONS.md → Add system |
| New Business Process | processes/*.md → Create/update |
| New Role Capability | functional/*.md → Update role |
| Tech Stack Change | CORE.md → Tech Stack section |
| Pattern Change | CORE.md → Key Patterns section |

**FORBIDDEN**:
- ❌ Completing implementation without updating solution specs
- ❌ Adding features that contradict existing flows
- ❌ Creating duplicate entities/patterns without checking CORE.md first

**Agent Behavior**:
```
BEFORE implementation:
  └── "I've read the solution specs. The system has [X entities],
       [Y state machines], and uses [Z patterns]."

AFTER implementation:
  └── "I've updated [CORE.md/FLOWS.md/etc] to reflect the new
       [entity/workflow/integration]."
```

---

## 2. Governance: Spec-Driven Development (SDD)
- **Phase 1 (Spec)**: Create `specs/spec_[name].md`. **Compliance Sidebar** citing specific standards is mandatory.
- **Phase 2 (Plan)**: Create `implementation_plan.md` + automated `scripts/verification/verify_*.sh` **BEFORE** implementation.
- **Rule**: Never execute code without an Approved Spec and Verification Script.
- **Refusal Protocol**: Decline any "shortcuts" that bypass this governance.

### Branching Rules (Agent Behavior)

**MANDATORY**: Create `feature/[topic]` branch for implementation work.

| Work Type | Branch | Example |
|-----------|--------|---------|
| New feature | `feature/topic-name` | `feature/session-expiration` |
| Bug fix | `bugfix/issue-desc` | `bugfix/signalr-auth` |
| Hotfix (urgent prod) | `hotfix/critical-issue` | `hotfix/login-broken` |
| Docs/config only | Direct to `develop` | Small README updates |

**Agent Workflow**:
```bash
# 1. Start of implementation conversation
git checkout develop && git pull origin develop
git checkout -b feature/topic-name

# 2. Work and commit
git add . && git commit -m "feat: description"

# 3. Push and merge
git push origin feature/topic-name
# Create PR → develop → main
```

**FORBIDDEN**:
- ❌ Direct commits to `main` (NEVER)
- ❌ Direct commits to `develop` for implementation work
- ❌ Force push to shared branches

**Detailed Standards**: See [git-branching-sap-standards.md](standards/git-branching-sap-standards.md) and [docs/git-workflow-best-practices.md](../../docs/git-workflow-best-practices.md)

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

### Category 4: Integration & Infrastructure (6 standards)
| Pattern | Standard |
|---------|----------|
| SAP Integration | [sap-integration-standard.md](standards/sap-integration-standard.md) |
| File Storage | [file-storage-standard.md](standards/file-storage-standard.md) |
| Email Service | [email-service-standard.md](standards/email-service-standard.md) |
| Multi-Tenancy | [multi-tenancy-standard.md](standards/multi-tenancy-standard.md) |
| Data Residency | [data-residency-standard.md](standards/data-residency-standard.md) |
| Deployment Environment | [deployment-environment-standard.md](standards/deployment-environment-standard.md) |

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
├─► Deployment / CORS / SignalR / environment issue?
│   ├─► Read: deployment-environment-standard.md
│   └─► Read: cicd-setup-standards.md
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
- **DI Validation**: `dotnet build` does NOT validate DI. You MUST run `dotnet run` after any DI registration change. Build succeeds but app crashes at startup if services are unresolvable.
  ```
  ❌ dotnet build (passes) → dotnet run (CRASH: Unable to resolve service)
  ✅ dotnet build (passes) → dotnet run (starts) → curl /api/health (200 OK)
  ```
  **Rationale**: DI validation only runs at `BuildServiceProvider()` during startup, not at compile time. Learned from CosmosRepository incident (2026-02-25).
- **Concrete vs Interface DI**: When changing `AddTransient<ConcreteType>()` to `AddScoped<IInterface, ConcreteType>()`, ALWAYS grep for services that inject the concrete type directly. Both registrations may be needed:
  ```csharp
  builder.Services.AddScoped<ICosmosRepository, CosmosRepository>(); // Interface
  builder.Services.AddScoped<CosmosRepository>(); // Concrete (if other services inject directly)
  ```
- **Duplicate Type Check**: Before creating new classes/constants, ALWAYS search for existing definitions:
  ```bash
  grep -r "class TypeName\|static class TypeName" backend/
  ```
  **Rationale**: Prevents CS0101 duplicate type errors (learned from DocumentStatus incident).
- **Route Ambiguity Check**: `[Route("api/[controller]")]` on `HealthController` already resolves to `api/health`. Adding `[Route("api/health")]` causes `AmbiguousMatchException` at runtime. The `[controller]` token = lowercase class name minus "Controller" suffix.
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

### 6.1 Core.Framework Extension Pattern

**Full Governance**: See [GOVERNANCE.md](../../backend/VendorMdm.Core.Framework/GOVERNANCE.md)

**FORBIDDEN** (Build will fail):
```csharp
// ❌ Apps CANNOT implement Core interfaces
public class MyAuthService : IAuthenticationService { }

// ❌ Apps CANNOT inherit from Core classes
public class MyLogger : StructuredLogger { }
```

**ALLOWED** (Extension pattern):
```csharp
// ✅ Apps CAN create extension methods
public static class AuthExtensions
{
    public static async Task<Result<VendorData>> GetVendorDataAsync(
        this IAuthenticationService auth, Guid vendorId) { ... }
}

// ✅ Apps CAN create adapters/wrappers (composition)
public class VendorAuthAdapter
{
    private readonly IAuthenticationService _auth;
    public VendorAuthAdapter(IAuthenticationService auth) => _auth = auth;
}
```

**Rationale**: Core.Framework is the shared foundation for ALL MDM applications. Modifications require ADR + Architecture Team approval.

---

## 7. Security High Standards (The Iron Dome)
**Status**: ZERO TOLERANCE for violations.

### Implementation Status (Verified 2026-02-05)
| Feature | Status | Location |
|---------|--------|----------|
| Security Headers (HSTS, CSP, X-Frame) | ✅ Implemented | `Middleware/SecurityHeadersMiddleware.cs` |
| Rate Limiting (5 req/min) | ✅ Implemented | `Program.cs:128-133`, 5+ endpoints |
| Input Sanitization (Global Filter) | ✅ Implemented | `Program.cs:103`, `Filters/InputSanitizationActionFilter.cs` |
| OpenAPI/Swagger | ✅ Implemented | `Program.cs:381-382` |

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
-   **SignalR Dev Proxy**: In development, route SignalR through Vite proxy (`/hubs` → backend) instead of direct cross-origin connections. This avoids CORS issues entirely. ASP.NET CORS middleware does NOT reliably add headers to SignalR negotiate endpoint even with `RequireCors()`.
    ```typescript
    // vite.config.ts - proxy SignalR hub
    '/hubs': { target: 'http://127.0.0.1:5001', changeOrigin: true, ws: true }
    // SignalRContext.tsx - use relative URL (empty base = same origin via proxy)
    const apiBaseUrl = import.meta.env.VITE_API_URL || '';
    ```
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

### Enforcement Mechanism (Updated 2026-02-05)
| Mechanism | Status | Location |
|-----------|--------|----------|
| Pre-commit Git Hook | ✅ Available | `scripts/hooks/pre-commit` |
| CI PR Validation | ✅ Implemented | `.github/workflows/verify-pr.yml` |
| Migration Size Check | ✅ In CI | `verify-pr.yml:migration-validation` |
| Security Scanning (SAST) | ✅ Implemented | `.github/workflows/security-scan.yml` |
| Dependency Review | ✅ Implemented | Blocks high-severity + GPL licenses |
| Secrets Scanning | ✅ Implemented | TruffleHog on all pushes |

**Install hooks**: `./scripts/install-hooks.sh`

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

### Learning Storage Rule (CRITICAL)

**VALID storage locations for learnings:**
- ✅ `moderngoldenrules.md` (this file) - patterns, rules, examples
- ✅ `standards/*.md` - detailed guidance per topic
- ✅ `retrospectives/INDEX.md` - tracking and history

**FORBIDDEN storage locations:**
- ❌ `CLAUDE.md` - pointer only, NO learnings
- ❌ `MEMORY.md` - pointer only, NO learnings
- ❌ Any other markdown file

**Verification Check** (agent MUST run before saving learnings):
```
IF learning_to_save THEN
  IF target_file IN ["CLAUDE.md", "MEMORY.md"] THEN
    STOP → "FORBIDDEN: Use moderngoldenrules.md or standards/*.md"
  ELSE IF target_file IN ["moderngoldenrules.md", "standards/*.md", "INDEX.md"] THEN
    PROCEED → Save learning
  END
END
```

**Why**: CLAUDE.md and MEMORY.md are loaded on EVERY conversation. Learnings there cause:
- Bloated context (slower responses)
- Duplicate information (conflicts with golden rules)
- Maintenance burden (two places to update)

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
   - Update relevant standard in standards/*.md (if detailed)
   - Mark as `[x] Applied` in INDEX.md
   - Commit rule updates with retrospective reference
4. Do NOT leave "Pending" items - apply them before closing the task
5. **VERIFY**: No learnings written to CLAUDE.md or MEMORY.md

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

## 11. Critical Thinking & Continuous Improvement

**Status**: 🔴 CRITICAL | Agents MUST challenge and suggest improvements.

### 11.1 The Mandate

**MANDATORY**: Agents must not passively execute tasks. They must:
1. **Challenge** assumptions, approaches, and existing patterns
2. **Suggest** improvements to code, architecture, and processes
3. **Question** specifications that seem outdated or incorrect
4. **Propose** better alternatives when they exist

### 11.2 When to Apply Critical Thinking

| Scenario | Required Action |
|----------|-----------------|
| Reading existing code | Identify potential improvements |
| Following a specification | Verify spec matches reality (code is truth) |
| Implementing a feature | Challenge if approach is optimal |
| Finding inconsistencies | Report and suggest resolution |
| Discovering patterns | Propose for standardization |
| Seeing technical debt | Document and propose cleanup |

### 11.3 Critical Thinking Protocol

**Before Implementation**:
```
1. READ the specification
2. VERIFY against actual code (code is truth)
3. IDENTIFY gaps or inconsistencies
4. CHALLENGE if the approach is optimal
5. SUGGEST improvements if found
6. PROCEED only after validation
```

**During Implementation**:
```
1. QUESTION each decision point
   └── "Is this the best way?"
   └── "Does this follow our patterns?"
   └── "Could this be simpler?"

2. IDENTIFY improvement opportunities
   └── Code quality
   └── Performance
   └── Maintainability
   └── Security

3. DOCUMENT findings for future
```

**After Implementation**:
```
1. REVIEW what was built
2. COMPARE with specification
3. SUGGEST spec updates if needed
4. PROPOSE learnings for brain rules
```

### 11.4 Constructive Criticism Standards

**GOOD**: Specific, actionable, with alternative
```
"The current approach uses N+1 queries. I suggest using
Include() to eager-load related entities, which would
reduce database calls from 100 to 1."
```

**BAD**: Vague, negative, no alternative
```
"This code is inefficient."
```

### 11.5 Improvement Categories

| Category | Examples |
|----------|----------|
| **Architecture** | Pattern violations, coupling issues, missing abstractions |
| **Performance** | N+1 queries, missing indexes, inefficient algorithms |
| **Security** | Input validation gaps, missing auth checks, exposed secrets |
| **Maintainability** | Code duplication, missing documentation, unclear naming |
| **Consistency** | Deviations from standards, inconsistent patterns |
| **Specifications** | Outdated docs, missing features, incorrect flows |

### 11.6 Agent Behavior

**MANDATORY Actions**:
- ✅ Point out when specs don't match code
- ✅ Suggest better approaches when known
- ✅ Challenge user assumptions respectfully
- ✅ Propose updates to brain rules when patterns emerge
- ✅ Report technical debt discovered during work

**FORBIDDEN Actions**:
- ❌ Silently ignore known issues
- ❌ Implement knowing there's a better way
- ❌ Follow outdated specs without verification
- ❌ Skip suggesting improvements to avoid conflict

### 11.7 The "Code is Truth" Principle

When specifications and code conflict:
```
1. CODE wins (it's what actually runs)
2. Identify the discrepancy
3. Update specs to match reality
4. OR fix code if spec is correct
5. Never assume specs are current
```

### 11.8 Improvement Workflow

```
Agent finds issue
       │
       ▼
Document the finding
       │
       ▼
Propose solution
       │
       ▼
┌──────┴──────┐
▼             ▼
Minor       Major
(fix now)   (backlog item)
   │             │
   ▼             ▼
Implement   Add to BACKLOG.md
   │             │
   ▼             ▼
Update      Create issue
brain       if needed
```

### 11.9 Success Metrics

**Effectiveness Indicators**:
- Specifications stay current (code = spec alignment)
- Brain rules evolve from learnings
- Technical debt is identified and tracked
- Patterns are standardized proactively
- User gets better outcomes through agent suggestions

---

## 12. Dependency Health Awareness

**Status**: MANDATORY for all external integrations.

### 12.1 The Problem

Systems fail silently when external dependencies (SAP, Email, Storage) are down, leading to fragmented state and poor UX.

### 12.2 Connectivity Probes (REQUIRED)

Every external service client MUST implement a health check method:

```csharp
public interface IExternalService
{
    Task<Result<ConnectionStatus>> TestConnectionAsync();  // REQUIRED
}
```

**Expose via API**: `/api/system/data-sources` or `/api/health`

### 12.3 Truth in Success (NO Silent Masking)

**CRITICAL**: If an external call fails, NEVER return `Success: true`.

```csharp
// ❌ WRONG - Masking failure
try {
    await _emailService.SendAsync(email);
} catch {
    _logger.LogError("Email failed");
    return Result.Success();  // WRONG: User thinks email was sent
}

// ✅ CORRECT - Truthful response
try {
    await _emailService.SendAsync(email);
    return Result.Success();
} catch (Exception ex) {
    _logger.LogError(ex, "Email failed");
    return Result.Failure("Email delivery failed. Please try again.");
}
```

### 12.4 Contextual Error Logs

Critical failures MUST log the current configuration state:

```csharp
_logger.LogError(
    "SMTP send failed. Config: {SmtpEnabled}, Host: {Host}, Port: {Port}",
    _config.SmtpEnabled,
    _config.SmtpHost,
    _config.SmtpPort
);
```

**Rationale**: Instant diagnosis without rechecking config files.

### 12.5 UI Fail-Fast

Frontend MUST query health status and warn users BEFORE they initiate workflows that depend on failing services.

```typescript
// ✅ Check before workflow
const { data: health } = await api.get('/api/system/data-sources');
if (!health.sap.connected) {
    showWarning("SAP is currently unavailable. Submission will be queued.");
}
```

---

## 13. Canonical Entity Decoupling (SAP Independence)

**Status**: MANDATORY for all domain entities.

### 13.1 NO External System Fields in Domain

**FORBIDDEN**: Adding external system IDs directly to entities.

```csharp
// ❌ WRONG - SAP coupling in domain
public class Vendor
{
    public string SapVendorId { get; set; }      // FORBIDDEN
    public string SalesforceId { get; set; }    // FORBIDDEN
}

// ✅ CORRECT - Use mapping service
var sapId = await _sapIdService.GetSapIdAsync(vendor.Id, "Vendor");
```

**Pattern**: Use `SapIdMapping` table to store external system mappings.

### 13.2 Source System Tracking

All canonical entities MUST track their origin:

```csharp
public enum SourceSystem
{
    Portal,     // Created via web UI
    SAP,        // Synced from SAP
    API,        // Created via API
    Migration,  // Data migration
    Batch       // Batch processing
}

entity.SourceSystem = SourceSystem.Portal;  // REQUIRED
```

### 13.3 Event Sourcing Required Fields

Every domain event MUST include these fields:

```csharp
await EmitDomainEventAsync("VendorCreated", new
{
    entityId = vendor.Id,
    correlationId = GetCorrelationId(),  // REQUIRED - trace across systems
    actor = GetCurrentUserId(),          // REQUIRED - who did this
    channel = EventChannels.Portal,      // REQUIRED - where it came from
    timestamp = DateTimeOffset.UtcNow
});
```

**Why**: Enables complete audit trail across distributed systems.

### 13.4 Reference

Full canonical entity rules: [docs/canonical-model-rules.md](../../docs/canonical-model-rules.md)

---

## 14. Event-Driven Architecture (EDA) Governance

**Status**: MANDATORY for all features involving state changes or integrations.

### 14.1 Proactive Evaluation Requirement

**CRITICAL**: Agents MUST proactively evaluate EDA requirements when:
- Implementing features with state changes (status transitions, workflows)
- Building integrations with external systems (SAP, Salesforce, Email)
- Adding functionality that requires real-time frontend updates
- Creating asynchronous side-effects (notifications, logging, sync)

**DO NOT** wait for the user to ask about events. Evaluate EDA applicability immediately.

### 14.2 EDA Checklist (Mandatory During Spec Phase)

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

(See Section 14.2 checklist above)

### 14.3 Implementation Requirements

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

### 14.4 SignalR Events (Frontend Push)

**Mandatory Events** (push to connected clients):
- `StatusChanged`: Any entity status transition
- `VendorCreated`: New vendor created
- `TaskAssigned`: Workflow task assigned to user
- `Notification`: User-targeted notifications
- `SapSyncResult`: SAP integration completed/failed

**Hub Endpoint**: `/hubs/events`

### 14.5 Agent Behavior

**Before Implementation**:
1. ✅ Evaluate if feature involves state changes or integrations
2. ✅ Document events in spec (Section 14.2 checklist)
3. ✅ Identify real-time requirements
4. ✅ Plan event handlers needed

**During Implementation**:
1. ✅ Emit domain events from Concepts/Services
2. ✅ Add to Outbox for guaranteed delivery
3. ✅ Dispatch to in-process handlers
4. ✅ Connect SignalR for frontend updates

**Violation**: Implementing state-changing features without EDA evaluation is a governance violation.

### 14.6 Reference Files

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

## 15. Pre-Merge Build Protocol

**Status**: 🔴 CRITICAL | **Source**: Incident 2026-01-10 (Lesson Learned)

### 15.1 The Rule: "Build Before Push"

**NEVER** push a merge commit to `main` or `develop` without running a local build of **BOTH** frontend and backend immediately after the merge operation.

**Rationale**: Merges can result in syntax errors, orphaned code, or duplicate logic that git auto-merge doesn't catch.

### 15.2 Mandatory Post-Merge Validation

After completing any merge operation (before pushing):

```bash
# 1. Backend Compilation Check
dotnet build backend/VendorMdm.Api/VendorMdm.Api.csproj
# MUST return "0 Error(s)"

# 2. Frontend Build Check
cd frontend && npm run build && cd ..
# MUST complete without error
```

### 15.3 Merge-to-Main Workflow

```bash
# 1. Pre-merge preparation
git checkout develop && git pull origin develop
dotnet build backend/VendorMdm.Api/VendorMdm.Api.csproj
cd frontend && npm run build && cd ..

# 2. Execute merge (DO NOT PUSH YET)
git checkout main
git pull origin main
git merge develop --no-ff -m "Merge develop into main: [Description]"

# 3. Post-merge validation (MANDATORY)
dotnet build backend/VendorMdm.Api/VendorMdm.Api.csproj
cd frontend && npm run build && cd ..

# 4. Only push AFTER step 3 passes
git push origin main
```

### 15.4 Verification Checklist

| Check | Command | Expected |
|-------|---------|----------|
| Backend Build | `dotnet build` | 0 Error(s) |
| Frontend Build | `npm run build` | ✓ built without error |
| Conflict Markers | `grep -r "<<<<<<" .` | No results |
| Git Status | `git status` | Clean working tree |

### 15.5 Agent Behavior

**Before Proposing Merge**:
1. ✅ Run backend build on source branch
2. ✅ Run frontend build on source branch
3. ✅ Execute merge locally (not push)
4. ✅ Run BOTH builds after merge
5. ✅ Only then push if all pass

**If Build Fails After Merge**:
- ❌ DO NOT force push
- ❌ DO NOT skip validation
- ✅ Fix the issue locally
- ✅ Re-run validation
- ✅ Push only when clean

### 15.6 Post-Deployment Verification (MANDATORY)

**Status**: 🔴 CRITICAL | **Source**: Incident 2026-02-26 (Reported success without verifying live site)

**The Rule**: NEVER report a deployment as "successful" based solely on CI/CD pipeline status. Always verify the live endpoint returns HTTP 200.

**After Every Push That Triggers Deployment**:

```bash
# 1. Wait for CI to complete
gh run list --branch <branch> --limit 3

# 2. Check ALL CI jobs pass (not just the workflow status)
gh run view <run-id> --json jobs --jq '.jobs[] | "\(.name) | \(.conclusion)"'

# 3. Verify live endpoints (MANDATORY - do not skip)
curl -s -o /dev/null -w "HTTP %{http_code}" <SITE_URL>       # Must be 200
curl -s -o /dev/null -w "HTTP %{http_code}" <API_URL>/api/health  # Must be 200
```

**Live Endpoints to Verify**:

| Service | URL | Expected |
|---------|-----|----------|
| Frontend (SWA) | `https://thankful-field-0258f8110.3.azurestaticapps.net` | HTTP 200 |
| Backend API (Dev) | `https://app-vendor-mdm-api-dev.azurewebsites.net/api/health` | HTTP 200 |
| Backend API (Prod) | `https://app-vendor-mdm-api-prod.azurewebsites.net/api/health` | HTTP 200 |

**Agent Behavior**:
- ❌ NEVER say "deployed successfully" without `curl` verification
- ❌ NEVER guess or construct URLs — always extract from deploy logs
- ✅ Always `curl` the live site after deployment
- ✅ Report the actual HTTP status code to the user
- ✅ If verification fails, diagnose the root cause immediately

### 15.6 Reference

Full checklist: [docs/deployment/strict-deployment-checklist.md](../../docs/deployment/strict-deployment-checklist.md)

---

## 16. Self-Audit & Enforcement Gates

**Status**: 🔴 CRITICAL | **Source**: Learned violation 2026-02-05

**Purpose**: Rules without enforcement are suggestions. This section ensures ALL brain rules are followed through mandatory checkpoints.

### 16.1 The Self-Audit Principle

Agents MUST audit themselves against the brain rules. No external enforcement exists - the agent IS the enforcement mechanism.

**Core Truth**: If the agent doesn't self-enforce, the brain is worthless.

### 16.2 START Checkpoint (Beginning of Conversation)

**MANDATORY**: Before any implementation work, agent MUST output:

```
═══════════════════════════════════════════════════════════
✅ START CHECKPOINT - Brain Rules Acknowledged
═══════════════════════════════════════════════════════════
📖 Solution Context (Section 1.1):
   - CORE.md: [X] entities, [Y] patterns acknowledged
   - FLOWS.md: [X] state machines acknowledged
   - INTEGRATIONS.md: [X] active, [Y] mocked acknowledged

🔒 Critical Rules Confirmed:
   - [ ] Section 0: Zero Data Loss - No DB deletions without consent
   - [ ] Section 2: SDD - Spec exists or will be created
   - [ ] Section 7: Security - No hardcoded secrets
   - [ ] Section 8: Pre-commit checks will be run

📋 Task Understanding:
   - Task type: [Implementation/Research/Fix/Documentation]
   - Relevant standards to load: [List from Section 4]
═══════════════════════════════════════════════════════════
```

**FORBIDDEN**: Starting implementation without this checkpoint.

### 16.3 END Checkpoint (Before Closing Conversation)

**MANDATORY**: Before closing any significant conversation, agent MUST output:

```
═══════════════════════════════════════════════════════════
✅ END CHECKPOINT - Brain Rules Compliance Audit
═══════════════════════════════════════════════════════════
📝 Work Completed:
   - [Summary of what was done]

📊 Rule Compliance:
   - [ ] Section 0: No unauthorized data deletion
   - [ ] Section 1.1: Solution specs updated (if applicable)
   - [ ] Section 2: SDD workflow followed (if implementation)
   - [ ] Section 7: Security standards met
   - [ ] Section 8: Pre-commit checks passed
   - [ ] Section 9: Warnings addressed
   - [ ] Section 10: Retrospective completed
   - [ ] Section 11: Critical thinking applied
   - [ ] Section 15: Build verified (if merge)
   - [ ] Section 15.6: Live deployment verified with curl (if deploy)

📚 Retrospective (Section 10):
   - Learnings identified: [Yes/No/None]
   - INDEX.md updated: [Yes/No/N/A]
   - Brain rules updated: [Yes/No/N/A]
   - Pending count: [0]

💾 Commits:
   - [Commit hash]: [Message]
═══════════════════════════════════════════════════════════
```

**FORBIDDEN**: Closing conversation without this checkpoint (for significant work).

### 16.4 Continuous Self-Audit (During Work)

At key decision points, agent should pause and verify:

| Decision Point | Self-Audit Question |
|---------------|---------------------|
| Before creating entity | "Did I check CORE.md for existing entities?" (Section 1.1) |
| Before deleting data | "Did user explicitly consent?" (Section 0) |
| Before external integration | "Did I evaluate EDA requirements?" (Section 14) |
| Before commit | "Did I run build checks?" (Section 8) |
| Before merge | "Did I run post-merge builds?" (Section 15) |
| After deployment | "Did I curl the live URL and get HTTP 200?" (Section 15.6) |
| Before closing | "Did I complete retrospective?" (Section 10) |

### 16.5 Violation Response Protocol

If a violation is identified (by user or self-discovered):

```
1. STOP current work immediately
2. ACKNOWLEDGE the violation explicitly
3. COMPLETE the missed requirement
4. DOCUMENT in retrospective (why it happened, how to prevent)
5. UPDATE brain rules if pattern emerges
6. RESUME original work
```

**Example Response**:
```
⚠️ VIOLATION DETECTED: Section 10 (Retrospective) not completed

I acknowledge this violation. Stopping current work to:
1. Create retrospective entry
2. Document learnings
3. Update brain rules
4. Then resume/close properly
```

### 16.6 Checkpoint Exemptions

Checkpoints may be abbreviated for:
- Pure research/exploration (no implementation)
- Single-line fixes (trivial changes)
- Documentation-only changes

Even with exemptions, agent must acknowledge: "Abbreviated checkpoint - [reason]"

### 16.7 The Accountability Chain

```
Brain Rules (this file)
       │
       ▼
Agent reads and acknowledges (START checkpoint)
       │
       ▼
Agent works following rules
       │
       ▼
Agent self-audits continuously
       │
       ▼
Agent completes compliance audit (END checkpoint)
       │
       ▼
Retrospective captures learnings
       │
       ▼
Brain rules improve
       │
       ▼
Next conversation benefits
```

### 16.8 Success Metrics

**Effectiveness Indicators**:
- Zero unacknowledged rule violations
- Checkpoints visible in every significant conversation
- Retrospectives completed consistently
- Brain rules evolve from learnings
- User trust in agent compliance increases

---

