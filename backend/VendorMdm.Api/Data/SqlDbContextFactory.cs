using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VendorMdm.Api.Data;

/// <summary>
/// Design-time factory for SqlDbContext to FORCE SQL Server provider for ALL migrations.
/// This ensures that migrations are always generated with SQL Server types (nvarchar, uniqueidentifier, etc.)
/// instead of SQLite types (TEXT, etc.), regardless of local development environment.
/// 
/// CRITICAL: This factory is used by 'dotnet ef' commands and takes precedence over Program.cs configuration.
/// </summary>
public class SqlDbContextFactory : IDesignTimeDbContextFactory<SqlDbContext>
{
    public SqlDbContext CreateDbContext(string[] args)
    {
        // DEVELOPMENT ESCAPE HATCH: Allow SQLite for local 'database update' if explicitly requested
        var forceSqlite = Environment.GetEnvironmentVariable("FORCE_SQLITE_MIGRATION");
        if (forceSqlite == "true")
        {
             var sqliteConnection = Environment.GetEnvironmentVariable("ConnectionStrings__Sql") ?? "Data Source=app.db";
             var sqliteBuilder = new DbContextOptionsBuilder<SqlDbContext>();
             sqliteBuilder.UseSqlite(sqliteConnection);
             Console.WriteLine("🔧 SqlDbContextFactory: Using SQLite for local migration apply");
             return new SqlDbContext(sqliteBuilder.Options);
        }

        // ALWAYS use SQL Server for migrations
        // Read connection string from environment variable (set by developer or CI/CD)
        // Fallback to dev Azure SQL if not set
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Sql") 
            ?? "Server=tcp:sql-vendor-mdm-dev.database.windows.net,1433;Initial Catalog=VendorMdmDb;Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False;";

        var optionsBuilder = new DbContextOptionsBuilder<SqlDbContext>();
        
        // FORCE SQL Server provider - NEVER use SQLite for migrations
        optionsBuilder.UseSqlServer(connectionString);
        
        Console.WriteLine("🔧 SqlDbContextFactory: Using SQL Server for migrations");
        Console.WriteLine($"   Connection: {connectionString.Split(';')[0]}");

        return new SqlDbContext(optionsBuilder.Options);
    }
}
