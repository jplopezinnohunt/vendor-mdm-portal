# Retrospective: CI/CD Database Migrations Hardening

**Date**: 2026-02-04
**Branch**: `feature/event-driven-architecture-completion`
**Author**: Agent
**Status**: Completed

---

## Summary

Fixed multiple issues in the GitHub Actions database migration workflow. The session required 8 iterations to resolve authentication, type conversion, and tooling issues between SQLite (local) and SQL Server (Azure).

---

## What Was Done

### Issue 1: Script Generation Failed
- **Symptom**: "JWT SecretKey required" error during `dotnet ef migrations script`
- **Cause**: Azure Login happened AFTER script generation step
- **Fix**: Added dummy connection string env var for script generation

### Issue 2: Zero Data Loss Check Blocked Migration
- **Symptom**: Workflow blocked DROP COLUMN Role
- **Cause**: Migration dropped Role before preserving data
- **Fix**: Added UPDATE SET migration before DROP COLUMN

### Issue 3: Pattern Detection Failed
- **Symptom**: Smart detection didn't recognize safe migration
- **Cause**: Complex grep pattern failed on idempotent script format
- **Fix**: Simplified to separate grep checks

### Issue 4: SQLite TEXT Type Error
- **Symptom**: `ALTER COLUMN TEXT` failed on SQL Server
- **Cause**: Migrations generated with SQLite types
- **Fix**: Workflow patches TEXT → nvarchar(max) in script

### Issue 5: dotnet ef update Bypassed Patch
- **Symptom**: Patched script not used
- **Cause**: `dotnet ef database update` runs original migrations
- **Fix**: Execute patched SQL script via sqlcmd/PowerShell

### Issue 6: sqlcmd -P Token Too Long
- **Symptom**: "Argument too long (maximum is 128)"
- **Cause**: Azure AD tokens exceed 128 chars
- **Fix**: Tried SQLCMDPASSWORD env var

### Issue 7: SQLCMDPASSWORD Failed
- **Symptom**: "Invalid value: 'edit.com'"
- **Cause**: sqlcmd Azure AD auth quirks
- **Fix**: PowerShell `Invoke-Sqlcmd -AccessToken`

---

## Learnings

### 1. ❌ NEVER use `dotnet ef database update` for Azure
**Issue**: It runs migration code directly with embedded SQLite types
**Solution**: ✅ Use workflow that executes PATCHED SQL script
**Brain Rule**: Section 2.1 (already documented)

```bash
# ❌ FORBIDDEN for Azure
dotnet ef database update

# ✅ CORRECT: Let workflow execute patched script
# GitHub Actions → Generate script → Patch → Execute via PowerShell
```

### 2. ✅ Use PowerShell Invoke-Sqlcmd for Azure AD Auth
**Issue**: sqlcmd has 128 char limit on -P flag, env var unreliable
**Solution**: ✅ PowerShell handles long tokens correctly
**Impact**: Eliminated authentication failures

```powershell
# ✅ CORRECT: Works with Azure AD tokens
$token = az account get-access-token --resource https://database.windows.net/ --query accessToken -o tsv
Invoke-Sqlcmd -ServerInstance 'server.database.windows.net' -Database 'db' -AccessToken $token -Query $script
```

### 3. ✅ Dummy Connection String for Script Generation
**Issue**: EF needs valid connection string format for design-time
**Solution**: ✅ Provide dummy SQL Server string (no actual connection)
**Impact**: Script generation works without Azure credentials

```yaml
env:
  ConnectionStrings__Sql: "Server=localhost;Database=VendorMdmDb;Trusted_Connection=True;"
```

### 4. ✅ Data Migration Pattern for Safe DROP COLUMN
**Issue**: DROP COLUMN blocked by Zero Data Loss check
**Solution**: ✅ Add UPDATE SET before DROP, workflow detects pattern
**Impact**: Safe migrations proceed automatically

```csharp
// ✅ CORRECT: Migrate data BEFORE drop
migrationBuilder.AddColumn("Roles", "Users", ...);
migrationBuilder.Sql("UPDATE Users SET Roles = ...");
migrationBuilder.DropColumn("Role", "Users");
```

### 5. ⚠️ SQLite vs SQL Server Type Mapping
**Critical Knowledge**: Local development uses SQLite, Azure uses SQL Server

| SQLite | SQL Server |
|--------|------------|
| TEXT | nvarchar(max) |
| INTEGER | int/bigint |
| REAL | float |
| BLOB | varbinary(max) |

---

## Brain Rule Updates Required

### Applied (2026-02-04)
- [x] **Section 2.1**: Already documented migration process
- [x] **Section 2.2**: Added CI/CD Troubleshooting Guide (NEW)

### Pending
- [ ] None (all learnings applied during session)

---

## Metrics

| Metric | Value |
|--------|-------|
| Iterations to Success | 8 |
| Time Lost to Issues | ~45 minutes |
| Root Causes | 3 (auth, types, tooling) |
| Workflow Commits | 7 |
| Documentation Added | 2 sections |

---

## Files Changed

### Modified
```
.github/workflows/deploy-database-migrations.yml (7 commits)
.github/workflows/azure-static-web-apps.yml (path filter added)
.agent/rules/moderngoldenrules.md (Section 2.2 added)
backend/VendorMdm.Api/Migrations/20260128083656_AddEventManagementTables.cs (data migration)
```

---

## Recommendations for Future

1. **Before adding new migrations**: Test script generation locally with SQL Server types
2. **Before Azure deployment**: Review generated SQL for TEXT types
3. **When auth fails**: Use PowerShell Invoke-Sqlcmd, not sqlcmd CLI
4. **When DROP COLUMN needed**: Add UPDATE SET migration first

---

**Grade**: B+ (completed but required many iterations)
**Agent Confidence**: High (root causes documented, workflow stable)
