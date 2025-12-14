using Microsoft.Data.SqlClient;
using System;
using System.IO;
using System.Threading.Tasks;

namespace VendorMdm.MigrationRunner;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Canonical Model Migration Runner ===");
        Console.WriteLine();

        // Get connection string from environment or args
        var connectionString = Environment.GetEnvironmentVariable("AZURE_SQL_CONNECTION_STRING");
        if (string.IsNullOrEmpty(connectionString) && args.Length > 0)
        {
            connectionString = args[0];
        }

        if (string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("❌ ERROR: No connection string provided.");
            Console.WriteLine("Usage: dotnet run \"<connection-string>\"");
            Console.WriteLine("   OR: Set AZURE_SQL_CONNECTION_STRING environment variable");
            return;
        }

        // Read the safe migration SQL script
        var scriptPath = Path.Combine("..", "..", "..", "..", "docs", "azure-sql-safe-migration.sql");
        if (!File.Exists(scriptPath))
        {
            Console.WriteLine($"❌ ERROR: Migration script not found at {scriptPath}");
            return;
        }

        var migrationSql = await File.ReadAllTextAsync(scriptPath);
        Console.WriteLine($"📄 Loaded migration script: {scriptPath}");
        Console.WriteLine();

        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            Console.WriteLine("✅ Connected to Azure SQL successfully");
            Console.WriteLine($"   Database: {connection.Database}");
            Console.WriteLine($"   Server: {connection.DataSource}");
            Console.WriteLine();

            // Execute the migration script
            Console.WriteLine("🚀 Executing migration...");
            Console.WriteLine();

            using var command = new SqlCommand(migrationSql, connection);
            command.CommandTimeout = 300; // 5 minutes

            using var reader = await command.ExecuteReaderAsync();
            
            // Read results
            while (await reader.ReadAsync())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    Console.Write($"{reader.GetValue(i)} ");
                }
                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine("✅ Migration completed successfully!");
            Console.WriteLine();
            Console.WriteLine("Created tables:");
            Console.WriteLine("  - Vendors (Canonical master)");
            Console.WriteLine("  - VendorInvitationsCanonical");
            Console.WriteLine("  - ChangeRequestsCanonical");
            Console.WriteLine("  - ExternalSystemMappings (Multi-system ACL)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR: Migration failed");
            Console.WriteLine($"   {ex.Message}");
            Console.WriteLine();
            Console.WriteLine("Stack trace:");
            Console.WriteLine(ex.StackTrace);
            Environment.ExitCode = 1;
        }
    }
}
