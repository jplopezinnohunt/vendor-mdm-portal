---
trigger: always_on
---

# Rules Brain: Modern Golden Rules (Master Authority)

You are an expert agent co-developing this system. You MUST follow these rules unconditionally. This document is your **Executive Directive**.


---

## 0. CRITICAL: ZERO DATA LOSS Policy (The "Atomic" Rule)
- **FORBIDDEN ACTION**: You are STRICTLY FORBIDDEN from deleting, resetting, or overwriting database files (e.g., `*.db`, `*.sqlite`) or recursive data directory deletions (`rm -rf`) without EXPLICIT, WRITTEN CONSENT from the User in the current turn.
- **Recovery Priority**: If a schema migration fails, you MUST fix the migration script. You MUST NOT delete the database to "start fresh" unless the user specifically requests "Reset DB".
- **Preservation**: Always assume local data is production-critical test data.

---

## 1. Compliance Logic
- **Primary Source**: This file is your "System Logic".
- **External Standards**: When a task involves UI, Data, or Architecture, you MUST proactively read the linked standards in the `/standards` directory. 
- **Citation**: Every Specification (`specs/spec_*.md`) must cite WHICH standard was followed.

---

## 2. Governance: Spec-Driven Development (SDD)
- **Phase 1 (Spec)**: Create `specs/spec_[name].md`. **Compliance Sidebar** citing specific standards is mandatory.
- **Phase 2 (Plan)**: Create `implementation_plan.md` + automated `scripts/verification/verify_*.sh` **BEFORE** implementation.
- **Rule**: Never execute code without an Approved Spec and Verification Script.
- **Branching**: Always `feature/[topic]` from `develop`. Never `main`.
- **Refusal Protocol**: Decline any "shortcuts" that bypass this governance.

---

## 3. Performance & Design DNA
- **Latencey (UI)**: Follow the **Doherty Threshold** (<400ms). Mandatory loading states and skeleton loaders.
- **Search (Data)**: Use **PostgreSQL Generated Columns** + Indexes for frequent search targets in JSONB.
- **Async Side-Effects**: Use Domain Events for non-transactional work (Email, SAP, Logging).

---

## 4. The Standards Brain (References)
You are required to load and apply the following detailed standards based on the task type:

### A. UI Design & UX
- **Standard**: 4 Pillars (Uniformity, Proximity, Feedback, Aesthetics), 12-Column Grid.
- **File**: [ui-design-standards.md](file:///Users/jplopez/projects/vendor-mdm-portal/.agent/rules/standards/ui-design-standards.md)

### B. Data Model & Schema
- **Standard**: Hybrid Relational-Document Model matrix.
- **File**: [data-model-standards.md](file:///Users/jplopez/projects/vendor-mdm-portal/.agent/rules/standards/data-model-standards.md)

### C. Architecture & Integration
- **Standard**: Hexagonal Adapters, Simulation First, EDA/Event-Driven logic.
- **File**: [hexagonal-architecture-standards.md](file:///Users/jplopez/projects/vendor-mdm-portal/.agent/rules/standards/hexagonal-architecture-standards.md)

### D. Production Readiness & CI/CD
- **Standard**: Zero-downtime, Middleware sequencing, Asset integrity.
- **File**: [database-migration-standards.md](file:///Users/jplopez/projects/vendor-mdm-portal/.agent/rules/standards/database-migration-standards.md)
- **CI/CD Setup**: [cicd-setup-standards.md](file:///Users/jplopez/projects/vendor-mdm-portal/.agent/rules/standards/cicd-setup-standards.md)

### E. Git & SAP Alignment
- **Standard**: Mirror SAP environments (D01, Q01, P01) across Git branches (`develop`, `release`, `main`).
- **File**: [git-branching-sap-standards.md](file:///Users/jplopez/projects/vendor-mdm-portal/.agent/rules/standards/git-branching-sap-standards.md)

### F. Audit Log Integration (Pattern 16)
- **Standard**: Ontology-driven audit logging for all entities. Each entity MUST have an audit model that evolves with schema changes.
- **File**: [audit-log-integration-standards.md](file:///Users/jplopez/projects/vendor-mdm-portal/.agent/rules/standards/audit-log-integration-standards.md)
- **Status**: MANDATORY for all Create/Update/Delete operations

---

## 5. Build & Process Hygiene
- **Clean Sweep Protocol**: Before builds or migrations, execute `pkill -f dotnet` and clean `bin/obj` artifacts to prevent Exit Code 143/134.
- **Interface Integrity**: When changing an interface, update ALL implementations (Mock, Real, Simulation, Test) in one atomic turn.
- **Hygiene**: Pinned dependencies, `no-any` TypeScript, mandatory verification scripts with auth headers.
- **Observability**: `traceparent` propagation + `TraceId` UI overlays.
- **Simulation**: [SIMULATION MODE] logs for all external mocks.

---

## 6. The Architecture DNA (Micro-App Standard)
**Status**: MANDATORY for all new features.

1.  **The Ontology Rule**: Business Logic MUST exist in `VendorMdm.Shared/Ontology/Concepts`. Services are merely coordinators.
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
-   **Session Lifetime**: MUST be Configurable (Admin Parameter). Default: **15 Minutes** (Sliding).
-   **Ghost User Block**: Users present in Azure AD but missing from DB MUST be blocked in Production.

### B. Network & Transport
-   **Strict Headers**: `HSTS` (Strict-Transport-Security), `CSP` (Content-Security-Policy), and `X-Frame-Options: DENY` are MANDATORY.
-   **CORS Strictness**: Production CORS MUST be restricted to the specific `App:BaseUrl`. NO Localhost allowed in Prod.
-   **Rate Limiting**: All Public (`AllowAnonymous`) endpoints MUST have IP-based Rate Limiting (5 req/min).

### C. Input Hygiene
-   **Anti-XSS**: All DTO strings MUST be sanitized (`IInputSanitizer`) before reaching the Domain Layer.
-   **DTO Enforcement**: Never accept raw JSONB or Entity objects from the client.

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

