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

### F. Audit Log Integration
- **Standard**: Ontology-driven audit logging for all entities. Each entity MUST have an audit model that evolves with schema changes.
- **File**: [audit-log-integration-standards.md](file:///Users/jplopez/projects/vendor-mdm-portal/.agent/rules/standards/audit-log-integration-standards.md)
- **Pattern Reference**: See Section 10.3 Pattern 10 (Audit Trail)
- **Status**: MANDATORY for all Create/Update/Delete operations

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

1.  **The Ontology Rule**: See Section 10.1 Pattern 3 (Ontology-Driven Development) for full definition.
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

## 10. Foundational Patterns (The Implementation DNA)

**Status**: MANDATORY for all new features and refactoring.

These 18 patterns define HOW the system is built. Every new feature MUST conform to these patterns.

### 10.1 Architecture Patterns (3)

**1. Hexagonal Architecture (Ports & Adapters)**
- Core Domain MUST be in `VendorMdm.Shared/` with ZERO external dependencies
- Inbound Ports: REST API Controllers (`VendorMdm.Api/Controllers/`)
- Outbound Ports: Event Bus, SAP, Email (`VendorMdm.Api/Services/`)
- **Rule**: Business logic NEVER in Controllers or Services

**2. Hybrid Relational-Document Model**
- **SQL Columns**: Foreign keys, indexes, ACID, universal fields
- **JSONB**: Volatile data, context-specific, read-only payloads
- **Reference**: [data-model-standards.md](file:///Users/jplopez/projects/vendor-mdm-portal/.agent/rules/standards/data-model-standards.md)

**3. Ontology-Driven Development**
- Business logic MUST exist in `VendorMdm.Shared/Ontology/Concepts/`
- Services are coordinators, NOT decision-makers
- Every Concept MUST implement `IOntologyConcept`

---

### 10.2 Core Patterns (4)

**4. Result Pattern**
- ALL service methods return `Result<T>` or `Result`
- NEVER throw exceptions for business logic failures
- Pattern: `return Result<T>.Success(value)` or `Result.Failure("error")`

**5. Structured Logging**
- Use `IStructuredLogger` from Core.Framework
- Log format: `logger.LogInformation("Action completed", new { vendorId, status })`
- NEVER use string interpolation in logs

**6. Event Sourcing (Partial)**
- Domain Events for state changes
- Async side-effects: Email, SAP, Logging
- Pattern: Publish events, don't await side-effects

**7. State Machines**
- Workflow transitions defined explicitly
- Example: `Draft → Submitted → UnderReview → Approved → Integrated`
- Validation before state changes

---

### 10.3 Security & Compliance Patterns (6)

**8. Multi-Channel Authentication**
- Azure AD (Production)
- JWT (API clients)
- Magic Links (Passwordless)
- Cookie (Local dev with MockAuth)

**9. Role-Based Authorization**
- App-scoped RBAC: `IUserContext.HasRoleForApp(app, role)`
- Roles: Requestor, Approver, MDMAdmin, ITAdmin
- NEVER use global admin checks

**10. Audit Trail**
- ALL entities implement `IAuditableEntity`
- Automatic tracking: CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
- Interceptor pattern for auto-population

**11. Soft Delete**
- NEVER hard delete data
- Use `IsDeleted` flag
- Filter queries: `.Where(x => !x.IsDeleted)`

**12. PII Masking**
- Mask sensitive data in logs
- Pattern: `email.Substring(0, 3) + "***"`
- NEVER log full credit cards, passwords, SSN

**13. GDPR Compliance**
- Right to be forgotten (anonymization)
- Data export capability
- Consent tracking

---

### 10.4 Integration & Infrastructure Patterns (5)

**14. File Storage Abstraction**
- Interface: `IFileStorageService`
- Implementations: Simulation (local), AzureBlob (prod)
- **UseMock pattern**: Configuration-driven selection

**15. SAP RFC Integration**
- Adapter pattern for SAP calls
- Operations: ZBAPI_VENDOR_CREATE, ZBAPI_VENDOR_UPDATE
- Simulation mode for local dev

**16. Email Templating**
- Interface: `IEmailService`
- Channels: Azure Function, SMTP, Console (local)
- Template-based rendering

**17. Data Residency**
- Region-aware storage
- Configurable per deployment
- Metadata: `DataRegion` field

**18. Multi-Tenancy**
- Tenant isolation via SQL filters
- Pattern: `.Where(x => x.TenantId == currentTenant)`
- NEVER expose cross-tenant data

---

### 10.5 Compliance Enforcement

**Before Implementation**:
1. Read relevant pattern definition above
2. Check if pattern already exists in codebase
3. Follow existing implementation style
4. If pattern missing, implement per standard

**During Code Review**:
- Verify pattern compliance
- Check for anti-patterns (e.g., business logic in controllers)
- Ensure consistency with existing code

**Pattern Violations**:
- ❌ **FORBIDDEN**: Business logic in Controllers
- ❌ **FORBIDDEN**: Hard deletes
- ❌ **FORBIDDEN**: Hardcoded secrets
- ❌ **FORBIDDEN**: Returning SQL Entities from API
- ❌ **FORBIDDEN**: Throwing exceptions for business failures

**Future Patterns (Roadmap)** - Not yet implemented:
- [ ] API Versioning (Week 5)
- [ ] Circuit Breaker (Week 3)
- [ ] Response Caching (Week 6)
- [ ] Background Jobs (Week 9)
- [ ] Feature Flags (Week 10)
- [ ] Code Splitting (Week 7)
- [ ] Distributed Tracing (Week 6)

---

### 10.6 Entity Evolution Checklist

**Status**: MANDATORY when adding or modifying entities.

When adding a **new entity**:

```
□ 1. SEARCH for existing types
     grep -r "class EntityName" backend/

□ 2. INHERIT from CanonicalEntityBase
     public class NewEntity : CanonicalEntityBase { }
     // Automatically gets: Id, Status, SourceSystem, Data, timestamps, soft delete

□ 3. ADD to SqlDbContext
     public DbSet<NewEntity> NewEntities { get; set; }

□ 4. CONFIGURE in ConfigureCanonicalEntities()
     modelBuilder.Entity<NewEntity>(entity => {
         entity.HasKey(e => e.Id);
         entity.HasIndex(e => e.IsDeleted);
         entity.HasQueryFilter(e => !e.IsDeleted);  // Pattern 11
         entity.Property(e => e.Data).IsRequired().HasDefaultValue("{}");
     });

□ 5. CREATE DTO (Pattern 6: No Entity Leaks)
     // Shared/Contracts/Dtos/NewEntityDto.cs
     public class NewEntityDto { /* only public API fields */ }

□ 6. ADD ToDto() mapping extension
     // Shared/Contracts/Mappings/EntityMappingExtensions.cs
     public static NewEntityDto ToDto(this NewEntity entity) => new() { ... };

□ 7. CREATE Status Constants (if applicable)
     // Shared/Constants/NewEntityStatus.cs with state machine

□ 8. IMPLEMENT Concept (if business logic needed)
     // Shared/Ontology/Concepts/NewEntityConcept.cs : IOntologyConcept, IAuditableEntity

□ 9. CREATE Migration
     dotnet ef migrations add AddNewEntity
     // Verify migration < 50KB
```

When **modifying an existing entity**:

```
□ 1. UPDATE entity fields (CanonicalEntityBase or entity-specific)

□ 2. UPDATE DTO if API contract changes
     // Add new fields to DTO, map in ToDto()

□ 3. UPDATE Concept if business rules change

□ 4. CREATE Migration for schema changes
     dotnet ef migrations add UpdateEntityName
     // Verify migration < 50KB

□ 5. UPDATE Status Constants if workflow changes

□ 6. RUN verification script
     ./scripts/verification/verify_foundational_patterns.sh
```

When **adding integrations**:

```
□ 1. USE ExternalSystemMapping for ID correlation
     // Maps CanonicalEntityId ↔ ExternalSystemId

□ 2. RESPECT Query Filters (soft delete automatic)

□ 3. USE SourceSystem tracking
     entity.SourceSystem = SourceSystems.Sap;  // or HR, Finance, etc.

□ 4. RETURN DTOs, never entities
     return entity.ToDto();

□ 5. LOG with IStructuredLogger
     _logger.LogInformation("Integration sync", new { entityId, source });
```

---

**Agent Behavior**:
- When implementing a new feature, FIRST identify which patterns apply
- THEN read the pattern definition above
- THEN implement following the pattern
- If uncertain, ask the user which pattern to follow

---

## 11. Retrospective Governance (Continuous Improvement)

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
  ├── INDEX.md                  ← ALWAYS READ THIS FIRST (30 sec)
  ├── active/                   ← Current quarter (max 10 files)
  │   └── YYYY-QX-topic.md
  ├── archived/                 ← Past quarters (reference only)
  │   └── YYYY-QX-summary.md
  └── learnings-database.md     ← Aggregated patterns (optional)
```

### Agent Workflow

**Before Starting Work**:
1. Read `.agent/retrospectives/INDEX.md` (if exists)
2. Apply top learnings to current task
3. Avoid documented mistakes

**After Completing Work** (for significant features):
1. Document issues encountered in retrospective
2. Update `INDEX.md` with top 3-5 learnings
3. **MANDATORY: Apply learnings to brain rules immediately**
   - Update relevant sections in this file (moderngoldenrules.md)
   - Mark as `[x] Applied` in INDEX.md
   - Commit rule updates with retrospective reference
4. Do NOT leave "Pending" items - apply them before closing the task

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

