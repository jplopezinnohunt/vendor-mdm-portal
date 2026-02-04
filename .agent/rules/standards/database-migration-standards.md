# Database Migration Standard

**Category**: Operations & Quality
**Pattern #**: 19
**Status**: MANDATORY
**Priority**: 🔴 CRITICAL

**Applies To**: All EF Core migrations, database schema changes, and build operations

---

## 1. Database Type Compatibility

### Problem
EF Core auto-generates SQL Server-specific types (`nvarchar(max)`) even when using SQLite for local development, causing migration failures.

### Rules
- **ALWAYS** check the active database provider before generating migrations
- **NEVER** use database-specific type annotations (e.g., `[Column(TypeName = "nvarchar(max)")]`) in shared model classes
- **ALWAYS** configure column types in `DbContext.OnModelCreating()` with provider-specific logic if needed
- **MANDATORY** post-generation step: Search migration files for `nvarchar(max)` and replace with `TEXT` for SQLite compatibility

### Implementation
```csharp
// ❌ WRONG - Database-specific annotation in model
public class MyEntity : CanonicalEntityBase
{
    [Column(TypeName = "nvarchar(max)")]  // SQLite incompatible
    public string Data { get; set; }
}

// ✅ CORRECT - Configure in DbContext
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<MyEntity>(entity =>
    {
        // Use TEXT for SQLite, nvarchar(max) for SQL Server
        entity.Property(e => e.Data)
            .HasColumnType("TEXT")  // or conditional logic
            .IsRequired()
            .HasDefaultValue("{}");
    });
}
```

### Post-Migration Checklist
After running `dotnet ef migrations add`:
1. Search migration file for `"nvarchar(max)"`
2. Replace all instances with `"TEXT"` for SQLite
3. Rebuild project
4. Apply migration

---

## 2. Build & Process Management

### Problem
Parallel execution of build and kill commands causes Exit Code 143 (SIGTERM during build).

### Rules
- **ALWAYS** use `waitForPreviousTools: true` for build operations
- **NEVER** run `pkill -f dotnet` while a build is in progress
- **ALWAYS** terminate running processes **before** starting new builds
- **MANDATORY** sequence: `pkill` → wait → `build` → `run`

### Correct Execution Pattern
```bash
# Step 1: Kill existing processes
pkill -f dotnet

# Step 2: Clean build artifacts (optional but recommended)
rm -rf backend/VendorMdm.Api/bin backend/VendorMdm.Api/obj

# Step 3: Build (wait for completion)
dotnet build backend/VendorMdm.Api

# Step 4: Apply migrations (if needed)
dotnet ef database update --project backend/VendorMdm.Api --no-build

# Step 5: Run application
dotnet run --project backend/VendorMdm.Api --urls "http://localhost:5001" --no-build
```

### Anti-Pattern to Avoid
```bash
# ❌ WRONG - Parallel execution
dotnet build backend/VendorMdm.Api &
pkill -f dotnet  # Kills the build process!
```

---

## 3. Property Initializers vs Service Defaults

### Problem
Property initializers in models mask null checks in service layer, preventing default value assignment.

### Rules
- **ALWAYS** check for both `null` AND the default initialized value when overriding defaults
- **NEVER** rely solely on `string.IsNullOrEmpty()` when property has an initializer
- **DOCUMENT** all property initializers that may be overridden by services

### Implementation
```csharp
// Model with initializer
public class Vendor : CanonicalEntityBase
{
    public string SourceSystem { get; set; } = "Portal";  // Default initializer
}

// ❌ WRONG - Only checks for null/empty
if (string.IsNullOrEmpty(vendor.SourceSystem))
    vendor.SourceSystem = SourceSystems.GetDefaultSource(typeof(Vendor));
// Result: "Portal" is never overridden to "SAP"!

// ✅ CORRECT - Checks for both null and default value
if (string.IsNullOrEmpty(vendor.SourceSystem) || vendor.SourceSystem == SourceSystems.Portal)
    vendor.SourceSystem = SourceSystems.GetDefaultSource(typeof(Vendor));
// Result: "Portal" is correctly overridden to "SAP"
```

---

## 4. Stale Build Artifacts

### Problem
Using `--no-build` flag after code changes executes old compiled DLLs, causing logic bugs to persist.

### Rules
- **NEVER** use `--no-build` when debugging logic issues
- **ALWAYS** perform full rebuild after editing service layer code
- **MANDATORY** clean `bin/obj` directories when troubleshooting persistent bugs
- **ONLY** use `--no-build` for confirmed unchanged code (e.g., re-running tests)

### Clean Build Protocol
```bash
# When debugging logic issues:
rm -rf backend/VendorMdm.Api/bin backend/VendorMdm.Api/obj
rm -rf backend/VendorMdm.Shared/bin backend/VendorMdm.Shared/obj
dotnet build backend/VendorMdm.Api
dotnet run --project backend/VendorMdm.Api --urls "http://localhost:5001"  # No --no-build
```

---

## 5. Port Binding & Process State

### Problem
Multiple application instances attempting to bind to the same port cause Exit Code 134.

### Rules
- **ALWAYS** verify no processes are listening on target port before starting app
- **MANDATORY** `pkill -f dotnet` before every `dotnet run` command
- **NEVER** start multiple instances of the same application simultaneously

### Pre-Flight Check
```bash
# Verify port availability
lsof -i :5001

# If occupied, kill processes
pkill -f dotnet

# Then start application
dotnet run --project backend/VendorMdm.Api --urls "http://localhost:5001"
```

---

## 6. Fresh Database State Protocol

### Problem
Stale migrations or corrupted database state cause unpredictable test results.

### Rules
- **MANDATORY** for each new baseline migration:
  1. Delete existing database file (`*.db`)
  2. Delete `Migrations/` folder
  3. Generate fresh migration
  4. Apply migration
- **NEVER** attempt to fix migration errors by editing existing migration files
- **ALWAYS** regenerate migrations when schema changes significantly

### Fresh Start Procedure
```bash
# 1. Clean database state
rm backend/VendorMdm.Api/vendormdm.db
rm -rf backend/VendorMdm.Api/Migrations

# 2. Generate fresh baseline
dotnet ef migrations add InitialCanonical --project backend/VendorMdm.Api --context SqlDbContext

# 3. Patch for SQLite compatibility (if needed)
# Search for "nvarchar(max)" and replace with "TEXT"

# 4. Rebuild
dotnet build backend/VendorMdm.Api

# 5. Apply migration
dotnet ef database update --project backend/VendorMdm.Api --context SqlDbContext --no-build
```

---

## Pre-Flight Checklist for Entity Migrations

Before adding a new canonical entity or modifying schema:

- [ ] Determine database provider (SQLite for local, SQL Server for prod)
- [ ] Clean build artifacts: `rm -rf backend/**/bin backend/**/obj`
- [ ] Terminate running processes: `pkill -f dotnet`
- [ ] For baseline migrations: Delete database file and `Migrations/` folder
- [ ] Generate migration
- [ ] **CRITICAL**: Search migration file for `nvarchar(max)`, replace with `TEXT`
- [ ] Rebuild project (full build, no `--no-build`)
- [ ] Apply migration with `--no-build` flag
- [ ] Start application (verify no port conflicts)
- [ ] Execute validation tests (curl or Swagger)

---

## Exit Code Reference

| Exit Code | Meaning | Common Cause | Solution |
|-----------|---------|--------------|----------|
| 0 | Success | - | Continue |
| 1 | General Error | Migration syntax error, SQLite incompatibility | Check migration SQL, replace `nvarchar(max)` |
| 134 | SIGABRT | Port already in use | `pkill -f dotnet` before starting app |
| 143 | SIGTERM | Process killed mid-execution | Use sequential execution, don't `pkill` during build |

---

## Enforcement

These rules are **mandatory** for all database schema changes. Violations will result in:
1. Multiple troubleshooting iterations (60+ interactions observed)
2. Stale build artifacts causing false test results
3. Port binding conflicts requiring manual cleanup
4. Migration rollbacks and database resets

**Auto-apply these rules in all EF Core migration workflows.**

---

## 7. Azure Deployment (CI/CD Pipeline)

### Environment-Specific Process

| Environment | Database | How to Apply Migrations |
|-------------|----------|------------------------|
| **Local** | SQLite | `dotnet ef database update` (direct) |
| **Azure DEV/STAGING/PROD** | SQL Server | **GitHub Actions ONLY** |

### NEVER Do This

- ❌ Create manual SQL scripts for Azure
- ❌ Run EF migrations directly against Azure SQL
- ❌ Use Azure Portal SQL Query Editor for schema changes

### ALWAYS Do This

- ✅ Commit migration files to git
- ✅ Merge to `develop` (or target branch)
- ✅ Trigger **"Deploy Database Migrations"** workflow in GitHub Actions UI

### GitHub Actions Migration Workflow

1. Go to: https://github.com/jplopezinnohunt/vendor-mdm-portal/actions
2. Click **"Deploy Database Migrations"**
3. Click **"Run workflow"**
4. Select environment (dev/staging/prod)
5. Workflow automatically:
   - Generates migration SQL from EF Core
   - Patches `TEXT` → `nvarchar(max)` for SQL Server
   - Applies to Azure SQL Database
   - Shows preview before applying

### Why GitHub Actions?

- Automatic type conversion (SQLite ↔ SQL Server)
- Audit trail in GitHub
- Rollback capability
- Environment isolation
- Zero manual SQL script errors

---

## 8. CI/CD Troubleshooting Guide

### Common Issues & Solutions

| Issue | Symptom | Solution |
|-------|---------|----------|
| Script gen fails | "JWT SecretKey required" | Add dummy connection string env var in workflow |
| DROP COLUMN blocked | Zero Data Loss check fails | Add data migration SQL BEFORE drop (UPDATE SET...) |
| TEXT type error | `ALTER COLUMN TEXT` fails on SQL Server | Workflow patches TEXT→nvarchar(max) in script |
| Azure AD token too long | sqlcmd `-P` fails (128 char limit) | Use PowerShell `Invoke-Sqlcmd` with `-AccessToken` |
| FK constraint error | Can't alter column with FK | Drop FK first, alter, recreate FK |

### SQLite vs SQL Server Type Mapping

```
SQLite          → SQL Server
TEXT            → nvarchar(max)
INTEGER         → int / bigint
REAL            → float
BLOB            → varbinary(max)
```

### Migration Workflow Flow

```
1. Generate SQL script (EF Core)    ← Uses SqlDbContextFactory
2. Patch TEXT → nvarchar(max)       ← sed replacement
3. Check for destructive changes    ← Zero Data Loss
4. Add firewall rule               ← GitHub runner IP
5. Execute via PowerShell          ← Invoke-Sqlcmd with AAD token
6. Remove firewall rule            ← Cleanup
```

### CRITICAL Rule

**NEVER** use `dotnet ef database update` for Azure - it uses migration code directly which has SQLite types. Always use the workflow which executes the PATCHED SQL script.

---

## Reference

- **CI/CD Standard**: [cicd-setup-standards.md](cicd-setup-standards.md)
- **Zero Data Loss**: [zero-data-loss-standard.md](zero-data-loss-standard.md)
- **Golden Rules**: Section 8 (Pre-Commit Protocol)
