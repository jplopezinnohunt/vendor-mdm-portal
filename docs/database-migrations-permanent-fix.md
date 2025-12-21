# Database Migrations: Root Cause & Permanent Fix

## The Problem We Solved

### What Happened
When deploying CDM to Azure, EF Core migrations were generating **SQLite types** (TEXT) instead of **SQL Server types** (nvarchar, uniqueidentifier), causing deployment failures with errors like:

```
Column 'Id' in table 'Employees' is of a type that is invalid for use as a key column in an index.
```

### Why It Happened
The application is designed to support **multiple database providers**:
- **Local Development**: SQLite (lightweight, no installation)
- **Azure Production**: SQL Server (enterprise-grade)

EF Core selects the provider based on runtime configuration in `Program.cs`. However, when generating migrations with `dotnet ef migrations add`, EF Core was reading the local Development configuration (`appsettings.Development.json`) which defaults to SQLite, causing migration files to be generated with SQLite-specific column types.

---

## The Permanent Solution

### SqlDbContextFactory
Created `backend/VendorMdm.Api/Data/SqlDbContextFactory.cs`:

```csharp
public class SqlDbContextFactory : IDesignTimeDbContextFactory<SqlDbContext>
{
    public SqlDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Sql") 
            ?? "Server=tcp:sql-vendor-mdm-dev.database.windows.net,1433;Initial Catalog=VendorMdmDb;Authentication=Active Directory Default;...";

        var optionsBuilder = new DbContextOptionsBuilder<SqlDbContext>();
        optionsBuilder.UseSqlServer(connectionString); // ALWAYS SQL Server

        return new SqlDbContext(optionsBuilder.Options);
    }
}
```

### How It Works
- `IDesignTimeDbContextFactory` is used **exclusively** by `dotnet ef` commands
- Takes precedence over `Program.cs` configuration
- **Forces SQL Server provider** regardless of environment
- Ensures migrations are always generated with SQL Server-compatible types

---

## Migration Best Practices (Going Forward)

### 1. Creating New Migrations

**ALWAYS use this command format**:

```bash
cd backend/VendorMdm.Api
dotnet ef migrations add [MigrationName] --context SqlDbContext
```

**Expected output** (confirms SQL Server is being used):
```
🔧 SqlDbContextFactory: Using SQL Server for migrations
   Connection: Server=tcp:sql-vendor-mdm-dev.database.windows.net,1433
Done. To undo this action, use 'ef migrations remove'
```

### 2. Applying Migrations to Azure (Manual)

**Local execution via Azure CLI**:

```bash
cd backend/VendorMdm.Api
ConnectionStrings__Sql="Server=tcp:sql-vendor-mdm-dev.database.windows.net,1433;Initial Catalog=VendorMdmDb;Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False;" \
dotnet ef database update --context SqlDbContext
```

**Via GitHub Actions** (recommended for production):
- Workflow: `.github/workflows/deploy-database-migrations.yml`
- Trigger: Manual via GitHub UI
- Uses Service Principal authentication

### 3. Verifying Migrations

**Check migration files** for correct types:
- ❌ **Bad**: `[Id] TEXT NOT NULL` (SQLite)
- ✅ **Good**: `[Id] nvarchar(450) NOT NULL` (SQL Server)

**Verify in Azure**:
```bash
sqlcmd -S sql-vendor-mdm-dev.database.windows.net -d VendorMdmDb -G -Q \
  "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' ORDER BY TABLE_NAME;"
```

---

## What Not To Do

❌ **NEVER** run `dotnet ef migrations add` without the Factory in place  
❌ **NEVER** manually edit migration files  
❌ **NEVER** use `sqlcmd` to apply migration scripts with `TEXT` types  
❌ **NEVER** delete `SqlDbContextFactory.cs`  

---

## Troubleshooting

### Problem: Migration still shows SQLite types

**Solution**: Verify Factory exists and restart IDE/terminal:
```bash
ls -la backend/VendorMdm.Api/Data/SqlDbContextFactory.cs
```

### Problem: "Cannot find the Factory" error

**Cause**: Factory class has compile errors  
**Solution**: Check Factory file for syntax errors, rebuild project

### Problem: Migration applied but tables don't exist

**Cause**: Migration partially failed  
**Solution**:
1. Check `__EFMigrationsHistory` table
2. Delete failed migration entry
3. Reapply with `dotnet ef database update`

---

## Summary

✅ **Root Cause**: EF Core using SQLite provider for migration generation  
✅ **Permanent Fix**: `SqlDbContextFactory` forces SQL Server  
✅ **Prevention**: Factory is committed to repo, always used by `dotnet ef`  
✅ **Verification**: Migration files contain `nvarchar`, `uniqueidentifier`, etc.  

**This issue will not happen again** as long as the Factory remains in the codebase.
