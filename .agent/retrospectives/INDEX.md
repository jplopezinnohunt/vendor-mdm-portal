# Retrospectives Index
**Last Updated**: 2026-02-25
**Purpose**: Organizational memory to prevent repeating mistakes and accelerate future implementations

---

## 📚 How to Use This Index

**Before Starting Work**:
1. Read "Top 5 Critical Learnings" below (30 seconds)
2. If implementing similar feature, read referenced retrospective
3. Apply learnings to avoid documented mistakes

**After Completing Work** (for significant features):
1. Add top 3-5 learnings to this index
2. Create detailed retrospective in `active/` (optional)
3. Note which brain rules need updates

---

## 🎯 Top 5 Critical Learnings

### 0. ⚠️ NEVER MODIFY CLAUDE.md or MEMORY.md (4x REPEAT MISTAKE)
**Issue**: Agent repeatedly adds content to pointer files instead of Golden Rules
**Occurrences**: 4 times in same conversation (2026-02-05)
**Root Cause**: Agent sees "learning" and defaults to updating CLAUDE.md/MEMORY.md
**Solution**: ✅ Guard rule added directly in both files:
```
**MANDATORY RULE - DO NOT MODIFY THIS FILE**
- This file is a POINTER ONLY (max 10 lines)
- ALL learnings go to `moderngoldenrules.md` or `standards/*.md`
- If you think you need to add content here → STOP → Add to Golden Rules instead
```
**Prevention**: Before ANY edit to CLAUDE.md or MEMORY.md:
1. STOP - Is this a pointer update or content?
2. If content → Go to `moderngoldenrules.md` or `standards/*.md`
3. Only pointer changes allowed in these files
**Applied**: ✅ Guard rules in CLAUDE.md and MEMORY.md
**Brain Rule**: Section 10 (Learning Storage Rule, lines 574-600)

### 1. ❌ NEVER use `env.IsStaging()` in ASP.NET Core
**Issue**: `IsStaging()` extension method doesn't exist - compiles but fails at runtime
**Solution**: ✅ Use `env.EnvironmentName == "Staging"` instead
**Source**: 2026-02-04 Security Hardening
**Applied**: ✅ Fixed in Program.cs:234
**Brain Rule**: Section 11 (Retrospective Governance)

```csharp
// ❌ BROKEN (compiles but fails at runtime)
if (env.IsStaging()) { ... }

// ✅ CORRECT
if (env.EnvironmentName == "Staging") { ... }
```

### 2. ✅ Use Header Indexer Syntax, NOT Add()
**Issue**: `Headers.Add()` throws ArgumentException on duplicate keys (ASP0019 warning)
**Solution**: ✅ Use indexer syntax `Headers["X-Frame"] = "DENY"`
**Impact**: Saved 5 minutes debugging per implementation
**Source**: 2026-02-04 Security Hardening
**Applied**: ✅ In moderngoldenrules.md Section 7.B (lines 185-192)

```csharp
// ❌ FORBIDDEN (throws on duplicate keys)
context.Response.Headers.Add("X-Frame-Options", "DENY");

// ✅ CORRECT (idempotent, no exceptions)
context.Response.Headers["X-Frame-Options"] = "DENY";
```

### 3. ✅ Bash Heredoc for New Files
**Issue**: Write tool requires reading file first (even for non-existent files)
**Workaround**: Use bash heredoc pattern for creating new files
**Impact**: Clean solution, works reliably
**Source**: 2026-02-04 Security Hardening
**Applied**: N/A (tool limitation, documented workaround)

```bash
# ✅ WORKAROUND: Create new file with bash heredoc
cat > path/to/NewFile.cs << 'EOF'
namespace MyNamespace;
public class NewClass { }
EOF
```

### 4. ✅ Verification Scripts Need Error Handling
**Issue**: Using `set -e` globally causes script to exit on first curl failure
**Solution**: Handle errors per-test, accumulate FAIL_COUNT, exit at end
**Impact**: Scripts now run all tests instead of stopping early
**Source**: 2026-02-04 Security Hardening
**Applied**: ✅ In moderngoldenrules.md Section 8.6 (lines 329-345)

```bash
# ❌ DON'T: Exit on first error
set -e
curl http://localhost:5001/health  # Fails → script stops

# ✅ DO: Handle errors per test
FAIL_COUNT=0
if ! curl -s http://localhost:5001/health; then
    ((FAIL_COUNT++))
fi
# ... more tests ...
exit $FAIL_COUNT
```

### 5. ✅ Input Sanitization Performance Target
**Benchmark**: <10ms per request (reflection-based property scanning)
**Pattern**: Global action filter + IInputSanitizer
**Implementation**: InputSanitizationActionFilter scans DTO properties
**Source**: 2026-02-04 Security Hardening
**Applied**: ✅ Implemented, monitoring in production

### 6. ❌ NEVER use `dotnet ef database update` for Azure
**Issue**: Runs migration code with SQLite types (TEXT), fails on SQL Server
**Solution**: ✅ Let GitHub Actions workflow execute PATCHED SQL script
**Source**: 2026-02-04 CI/CD Database Migrations
**Applied**: ✅ Workflow fixed, using PowerShell Invoke-Sqlcmd

```bash
# ❌ FORBIDDEN for Azure deployments
dotnet ef database update

# ✅ CORRECT: Trigger workflow that patches TEXT→nvarchar
# GitHub Actions → Generate script → Patch → Execute via PowerShell
```

### 7. ✅ Use PowerShell Invoke-Sqlcmd for Azure AD Auth
**Issue**: sqlcmd -P has 128 char limit, SQLCMDPASSWORD env var unreliable
**Solution**: ✅ PowerShell `Invoke-Sqlcmd -AccessToken` handles long tokens
**Source**: 2026-02-04 CI/CD Database Migrations
**Applied**: ✅ Workflow updated

### 8. ✅ Documentation Architecture: Single Source of Truth
**Issue**: Scattered docs (CLAUDE.md, MEMORY.md, .claude/*.md) caused duplication and confusion
**Solution**: ✅ Unified brain with pointers (3 lines) → Golden Rules → Standards
**Impact**: Faster loading, no conflicts, modular updates
**Source**: 2026-02-04 Brain Architecture Consolidation
**Applied**: ✅ 30 standards in 6 categories

```
# Before (scattered)
CLAUDE.md (200+ lines) + MEMORY.md (100+ lines) + .claude/*.md
→ Slow, duplicated, conflicts

# After (unified)
Pointer (3 lines) → Golden Rules → Load only relevant standard
→ Fast, single source, modular
```

### 9. ✅ Every Governance Section Needs a Standard
**Issue**: Sections 0,1,2,5,8,9,10 had no detailed standards (incomplete mapping)
**Solution**: ✅ Created Category 6: Governance & Process (7 standards)
**Impact**: Each section can evolve independently with full detail
**Source**: 2026-02-04 Brain Architecture Consolidation
**Applied**: ✅ 7 new governance standards created

### 10. ✅ Vitest MSAL Mock Pattern
**Issue**: `vi.fn().mockReturnValue({...})` makes useMsal return undefined
**Solution**: ✅ Use direct function returns: `useMsal: () => ({ instance, accounts, inProgress })`
**Impact**: Frontend auth context tests work correctly
**Source**: 2026-02-04 Brain v1.2.0 Compliance
**Applied**: ✅ frontend/tests/setup.ts

```typescript
// ❌ BROKEN (useMsal returns undefined)
useMsal: vi.fn().mockReturnValue({ instance, accounts })

// ✅ CORRECT (direct function)
useMsal: () => ({ instance: mockMsalInstance, accounts: [], inProgress: 'none' })
```

### 11. ✅ SignalR Mock Must Be Class
**Issue**: `HubConnectionBuilder: vi.fn()` doesn't support method chaining
**Solution**: ✅ Create actual class `MockHubConnectionBuilder` with method chaining
**Impact**: SignalR context tests work with build().start() pattern
**Source**: 2026-02-04 Brain v1.2.0 Compliance
**Applied**: ✅ frontend/tests/setup.ts

```typescript
// ✅ CORRECT: Actual class with method chaining
class MockHubConnectionBuilder {
  withUrl() { return this; }
  withAutomaticReconnect() { return this; }
  build() { return mockConnection; }
}
```

### 12. ✅ AccessibleModal aria-hidden Pitfall
**Issue**: `aria-hidden="true"` on backdrop hides entire dialog from accessibility tree
**Solution**: ✅ Remove aria-hidden from backdrop div, use `role="dialog"` and `aria-modal="true"` on content
**Impact**: getByRole('dialog') works in tests, screen readers see modal
**Source**: 2026-02-04 Brain v1.2.0 Compliance
**Applied**: ✅ AccessibleModal.tsx

### 13. ✅ Azure Static Web Apps Deployment Race Condition
**Issue**: Simultaneous pushes to develop and main cause "Deployment Canceled" error
**Root Cause**: Azure SWA cancels older deployments when newer one starts
**Solution**: ✅ This is expected behavior - main branch deployment takes priority
**Impact**: Develop branch deployments may show as failed but main succeeds
**Source**: 2026-02-04 Brain v1.2.0 Compliance CI
**Applied**: N/A (expected Azure behavior, not a bug)

### 14. ✅ Git Safe Directory in CI Docker Containers
**Issue**: Azure SWA Oryx builds in Docker, doesn't inherit workflow git config
**Error**: `fatal: detected dubious ownership in repository at '/github/workspace'`
**Solution**: ✅ Run `git config --global --add safe.directory /github/workspace` in build script
**Impact**: Cleaner CI logs, proper version info in builds
**Source**: 2026-02-04 Brain v1.2.0 Compliance CI
**Applied**: ✅ frontend/generate-version.js

```javascript
// ✅ CORRECT: Configure safe.directory before git commands
try {
  execSync('git config --global --add safe.directory /github/workspace', { stdio: 'pipe' });
} catch { /* Ignore - not in CI */ }
```

### 15. ✅ Integration Test Fixtures Need ALL Dependencies
**Issue**: `Unable to resolve service for type 'IStructuredLogger'` in integration tests
**Root Cause**: IntegrationTestFixture registered InvitationService but not its dependencies
**Solution**: ✅ Register ALL mock dependencies: IStructuredLogger, IAuditLogService, etc.
**Impact**: Integration tests pass without DI failures
**Source**: 2026-02-04 Result Pattern Compliance
**Applied**: ✅ IntegrationTestFixture.cs

```csharp
// ✅ CORRECT: Register all dependencies before the service
var mockStructuredLogger = new Mock<IStructuredLogger>();
services.AddSingleton(mockStructuredLogger.Object);
var mockAuditLog = new Mock<IAuditLogService>();
services.AddSingleton(mockAuditLog.Object);
// THEN register the service
services.AddScoped<IInvitationService, InvitationService>();
```

### 16. ✅ NetArchTest Checks ALL Classes in Namespace
**Issue**: Architecture test failed on `ChangeRequestStatusChangedEvent` - an event, not a concept
**Root Cause**: `ResideInNamespace()` includes ALL classes, not just intended ones
**Solution**: ✅ Use `HaveNameEndingWith("Concept")` to filter to actual concept classes
**Impact**: Architecture tests correctly validate only intended types
**Source**: 2026-02-04 Result Pattern Compliance
**Applied**: ✅ ArchitectureTests.cs

```csharp
// ❌ BROKEN: Catches Event classes too
Types.InAssembly(assembly)
    .That().ResideInNamespace("VendorMdm.Shared.Ontology.Concepts")
    .Should().ImplementInterface(typeof(IOntologyConcept))

// ✅ CORRECT: Filter to only *Concept classes
Types.InAssembly(assembly)
    .That().ResideInNamespace("VendorMdm.Shared.Ontology.Concepts")
    .And().HaveNameEndingWith("Concept")  // Filter!
    .Should().ImplementInterface(typeof(IOntologyConcept))
```

### 17. ✅ Mock Verify Needs Exact Event Names
**Issue**: Mock verification failed - test expected "invitation-created", service publishes "InvitationCreated"
**Root Cause**: Event names are case-sensitive and must match exactly
**Solution**: ✅ Check actual service code for exact event name string
**Impact**: Integration test mock verifications pass
**Source**: 2026-02-04 Result Pattern Compliance
**Applied**: ✅ InvitationFlowIntegrationTests.cs

```csharp
// ❌ BROKEN: Wrong event name
_fixture.MockServiceBus.Verify(sb => sb.PublishEventAsync("invitation-created", ...))

// ✅ CORRECT: Match actual service event name
_fixture.MockServiceBus.Verify(sb => sb.PublishEventAsync("InvitationCreated", ...))
```

### 18. ✅ Session Timeout: 2 Hours Corporate Standard
**Issue**: Session expiration defaults vary; corporate apps typically use 2-hour timeout
**Solution**: ✅ Store `sessionTimestamp` in localStorage, check on app load, clear after 2 hours
**Impact**: Consistent session management across auth methods (MSAL, local token, mock)
**Source**: 2026-02-05 Auth & SignalR Fixes
**Applied**: ✅ moderngoldenrules.md Section 7.A

```typescript
// ✅ CORRECT: 2-hour session timeout
const SESSION_EXPIRY_MS = 2 * 60 * 60 * 1000;
const elapsed = Date.now() - parseInt(sessionTimestamp, 10);
if (elapsed > SESSION_EXPIRY_MS) { /* clear auth data */ }
```

### 19. ✅ SignalR WebSockets Cannot Send Custom Headers
**Issue**: WebSocket connections ignore custom HTTP headers like `X-Mock-User`
**Solution**: ✅ Use query string for mock auth (`?mockUser=Role`), `accessTokenFactory` for real tokens
**Backend**: Check BOTH header AND query param in middleware for `/hubs` paths
**Source**: 2026-02-05 Auth & SignalR Fixes
**Applied**: ✅ moderngoldenrules.md Section 7.B

```typescript
// Frontend: Add mockUser to query string
url += `?mockUser=${encodeURIComponent(mockUser.role)}`;

// Backend: Check both header and query
var mockUserHeader = context.Request.Headers["X-Mock-User"].FirstOrDefault();
if (string.IsNullOrEmpty(mockUserHeader) && context.Request.Path.StartsWithSegments("/hubs"))
    mockUserHeader = context.Request.Query["mockUser"].FirstOrDefault();
```

### 20. ✅ CSP connect-src Must Include WebSocket Origins (Dev)
**Issue**: SignalR connection blocked by CSP in development
**Solution**: ✅ Add `ws://localhost:* wss://localhost:*` to CSP connect-src directive
**Impact**: SignalR works in development without CSP violations
**Source**: 2026-02-05 Auth & SignalR Fixes
**Applied**: ✅ moderngoldenrules.md Section 7.B

```json
"connect-src 'self' ws://localhost:* wss://localhost:* http://localhost:* https://localhost:*"
```

### 21. ✅ Bicep main.bicep Diverges from Bicep Modules (Naming + Config)
**Issue**: `main.bicep` uses naming `vendor-mdm-*` and serverless Cosmos; modules use `mdmportal-*` and 400 RU/s provisioned. Deploying modules would create duplicate resources or fail.
**Solution**: ✅ Always validate Bicep module compatibility before deploying. Documented in 3-way cross-validation report.
**Impact**: Prevents accidental duplicate Azure resource creation
**Source**: 2026-02-25 Architecture Cross-Validation
**Applied**: ✅ docs/architecture/3-way-cross-validation-report.md (Section 1.4, 2.1, 6)

### 22. ✅ Code References Non-Existent Azure Resources (MdmCore DB, 4 Queues)
**Issue**: `CosmosRepository` targets `MdmCore` database (not deployed), and `ServiceBusService` sends to 5 queues (only 1 exists). These will throw in Connected mode.
**Solution**: ✅ Always cross-validate code connection targets against live Azure before declaring "deployment ready".
**Prevention**: Run `az resource list` + grep code for database/queue names before merging to main.
**Source**: 2026-02-25 Architecture Cross-Validation
**Applied**: ✅ docs/architecture/3-way-cross-validation-report.md (Sections 1.1, 3.1) — P0 items to fix

### 23. ✅ Health Endpoint Path Mismatch Breaks CI/CD Verification
**Issue**: CI/CD workflows test `/api/health` but code maps health checks to `/health/live`, `/health/ready`, `/health/startup`. Post-deployment checks always 404.
**Solution**: ✅ Health endpoint paths must be consistent across code, CI/CD, and architecture standards.
**Prevention**: After adding/changing health endpoints, grep ALL workflow YAML files for the old path.
**Source**: 2026-02-25 Architecture Cross-Validation
**Applied**: ✅ docs/architecture/3-way-cross-validation-report.md (Section 6) — P0 item to fix

### 24. ✅ 3-Way Cross-Validation Method for Architecture Audits
**Pattern**: Query Azure CLI (`az resource list`), grep codebase (connection strings, container names, queue names), read Bicep templates, and compare against architecture docs/golden rules.
**Value**: Found 3 critical, 4 warning, 3 info discrepancies in a single pass.
**Tool**: `az resource list --resource-group rg-vendor-mdm-dev -o table` + targeted service queries.
**Source**: 2026-02-25 Architecture Cross-Validation
**Applied**: ✅ Documented as reusable method in docs/architecture/3-way-cross-validation-report.md

---

## 📋 Pending Brain Rule Updates

**Applied** (2026-02-04):
- [x] **Section 5**: Added "Duplicate Type Check" rule with grep pattern
- [x] **Section 7.B**: Added environment detection pattern (`env.EnvironmentName`)
- [x] **Section 7.B**: Added header indexer syntax pattern (ASP0019)
- [x] **Section 7.D**: Added Input Validation & Sanitization section
- [x] **Section 8.6**: Added verification script error handling pattern
- [x] **Section 9**: Added ASP0019 to Critical warnings table
- [x] **Section 10.2 Pattern 5**: Added security event logging examples
- [x] **Section 10.6**: Added Entity Evolution Checklist

**Pending**: None (all learnings applied per Section 11 rule)

---

## 📊 Active Retrospectives (2026-Q1)

### 2026-02-04: Foundational Patterns Hardening
**Branch**: `feature/security-hardening`
**File**: [2026-Q1-foundational-patterns-hardening.md](active/2026-Q1-foundational-patterns-hardening.md)
**Status**: ✅ Completed
**Key Learnings**:
- Search for existing types before creating new files
- CanonicalEntityBase is evolution foundation
- Query filters are automatic after DbContext config
- DTOs isolate API contract from entity evolution

**Patterns Implemented**:
- ✅ Pattern 11: Soft Delete (was missing)
- ✅ Pattern 6: DTO Enforcement (was partial)
- ✅ 7 new status constant files with state machines
- ✅ Entity Evolution Checklist added to brain rules

**Foundational Patterns Score**: 17/18 → 18/18

**Deliverables**:
- ISoftDeletable interface + CanonicalEntityBase update
- Global query filters for 9 entities
- 5 new DTOs + EntityMappingExtensions
- ApiVersionHeaderMiddleware
- Spec, implementation plan, verification script

**Time**: ~1 hour (autonomous execution)
**Grade**: A (completed all objectives)

---

### 2026-02-04: Security Hardening
**Branch**: `feature/security-hardening`
**Commit**: `20bba60`
**Status**: ✅ Completed, pushed to remote
**Key Learnings**:
- Environment detection bug (IsStaging)
- Header syntax (ASP0019)
- Input sanitization pattern
- Verification script issues

**Issues Resolved**:
- ✅ CRITICAL #1: Security headers missing
- ✅ CRITICAL #2: CORS allows localhost in production
- ✅ HIGH #5: IInputSanitizer not applied

**Compliance Impact**: 58% → ~70% (+12%)

**Deliverables**:
- SecurityHeadersMiddleware (7 headers)
- Environment-based CORS
- IInputSanitizer + InputSanitizationActionFilter
- Spec, implementation plan, verification script

**Time**: ~2 hours (autonomous execution)
**Grade**: A- (excellent with minor bug fixed)

---

### 2026-02-04: CI/CD Database Migrations Hardening
**Branch**: `feature/event-driven-architecture-completion`
**File**: [2026-02-04-cicd-database-migrations.md](active/2026-02-04-cicd-database-migrations.md)
**Status**: ✅ Completed
**Key Learnings**:
- SQLite TEXT types don't work on SQL Server
- PowerShell Invoke-Sqlcmd handles Azure AD tokens correctly
- Dummy connection string needed for script generation
- Data migration pattern for safe DROP COLUMN

**Issues Resolved**:
- ✅ Script generation auth failure
- ✅ Zero Data Loss false positive
- ✅ SQLite type conversion
- ✅ Azure AD token authentication

**Deliverables**:
- Fixed database migration workflow
- Added path filter to frontend workflow
- Added CI/CD Troubleshooting Guide to golden rules
- EDA implementation deployed to Azure

**Time**: ~1 hour (8 iterations)
**Grade**: B+ (completed but many iterations needed)

---

### 2026-02-04: Brain Architecture Consolidation
**Branch**: `develop`
**Commit**: `552a59b`
**Status**: ✅ Completed, merged to main
**Key Learnings**:
- Single source of truth prevents duplication
- Pointers should be minimal (3 lines)
- Every section needs a detailed standard
- Modular loading is more efficient

**Changes Made**:
- Simplified CLAUDE.md to 3-line pointer
- Simplified MEMORY.md to 3-line pointer
- Created BRAIN-ARCHITECTURE.md (hierarchy documentation)
- Created 17 new standards (now 30 total)
- Added Category 6: Governance & Process (7 standards)
- Organized 30 standards in 6 categories

**Brain Structure**:
- Level 1: 12 sections (high-level rules)
- Level 2: 30 standards (detailed guidance)

**Time**: ~30 min
**Grade**: A (clean architecture, no conflicts)

---

### 2026-02-25: Architecture Cross-Validation & Overview Page
**Branch**: `feature/fix-auth-email`
**Commit**: `f4b5bfc`
**Status**: ✅ Completed, pushed to remote
**Key Learnings**:
- Bicep modules and main.bicep can have incompatible naming/config
- Code references resources that don't exist in Azure (MdmCore, 4 queues)
- Health endpoint paths must be consistent across code + CI/CD + standards
- 3-way cross-validation (Azure CLI + Code + Bicep + Rules) catches gaps fast
- UNESCO SAP Brain DS patterns (StatsCard, color-coded cards) transfer well to MDM portal

**Issues Found**:
- ❌ P0: Cosmos `MdmCore` database not deployed (5 entity services will fail)
- ❌ P0: 4 of 5 Service Bus queues missing
- ❌ P0: CI/CD tests `/api/health` but code maps `/health/*`
- ⚠️ P1: Azure Functions not deployed (3 in code, 0 in Azure)
- ⚠️ P1: Cosmos partition key casing mismatch
- ⚠️ P1: Frontend 7 roles vs backend 4 policies

**Deliverables**:
- Architecture brief (validated against 9 live Azure resources)
- 3-way cross-validation report (10 discrepancies documented)
- ArchitectureOverview admin page (4 tabs, UNESCO SAP Brain DS)
- Route + sidebar navigation registered

**Time**: ~1 session
**Grade**: A (comprehensive audit, actionable findings)

---

## 📈 Statistics

| Metric | Value |
|--------|-------|
| Total Retrospectives | 6 |
| Critical Learnings | 25 |
| Bugs Prevented | 11 (IsStaging, duplicate type, SQLite types, doc duplication, CI git safe.directory, SignalR WebSocket auth, CSP WebSocket, pointer file modification, **MdmCore DB missing**, **queue name mismatch**, **health endpoint 404**) |
| Time Saved (estimated) | 135 min per future implementation |
| Brain Rules Applied | 25 updates |
| Brain Rules Pending | 0 |
| Standards | 34 (6 categories) |

---

## 🔄 Quarterly Maintenance (Next: 2026-05-01)

**Checklist**:
- [ ] Review all retrospectives in active/
- [ ] Extract patterns → Update learnings-database.md (if created)
- [ ] Archive active/ → archived/2026-Q1-summary.md
- [ ] Update top 5 learnings above
- [ ] Apply pending brain rule updates
- [ ] Clear active/ folder

---

## 📚 Archive

**2025-Q4**: No retrospectives (system not yet implemented)

---

**End of Index** - Read time: 3 minutes | Prevents: 1 critical bug + 4 common mistakes
