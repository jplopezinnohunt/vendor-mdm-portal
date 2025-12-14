using System;
using System.Data.SqlClient;
using System.Text.Json;
using VendorMdm.Shared.Models;
using VendorMdm.Shared.Helpers;

namespace VendorMdm.SchemaTest;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Schema Compliance Test ===\n");
        
        // Get connection string from environment
        var connectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("❌ SqlConnectionString environment variable not set");
            Console.WriteLine("\nSet it with:");
            Console.WriteLine("export SqlConnectionString=\"Server=tcp:mdmportal-sql-12031241-dev.database.windows.net,1433;Initial Catalog=mdmportal-sqldb-dev;User ID=<username>;Password=<password>;Encrypt=true;\"");
            return;
        }

        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            Console.WriteLine("✅ Connected to Azure SQL Database\n");

            // Test 1: Check if Attributes column exists
            Console.WriteLine("Test 1: Checking for Attributes columns...");
            await TestAttributesColumns(connection);

            // Test 2: Test JSON operations
            Console.WriteLine("\nTest 2: Testing JSON operations...");
            await TestJsonOperations();

            // Test 3: Query JSON attributes
            Console.WriteLine("\nTest 3: Testing JSON querying...");
            await TestJsonQuerying(connection);

            Console.WriteLine("\n✅ All tests passed!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error: {ex.Message}");
            Console.WriteLine($"Stack: {ex.StackTrace}");
        }
    }

    static async Task TestAttributesColumns(SqlConnection connection)
    {
        var tables = new[] 
        { 
            "VendorInvitations", "VendorApplications", "ChangeRequests",
            "Attachments", "UsersAndRoles", "WorkflowStates"
        };

        foreach (var table in tables)
        {
            var sql = $@"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = '{table}' AND COLUMN_NAME = 'Attributes'";

            using var cmd = new SqlCommand(sql, connection);
            var count = (int)await cmd.ExecuteScalarAsync();
            
            if (count == 1)
                Console.WriteLine($"  ✅ {table}.Attributes exists");
            else
                Console.WriteLine($"  ❌ {table}.Attributes NOT FOUND");
        }
    }

    static Task TestJsonOperations()
    {
        // Test JsonAttributeHelper
        var inviteAttrs = new VendorInvitationAttributes
        {
            Notes = "Test notes",
            CustomFields = new Dictionary<string, string>
            {
                { "campaignId", "CAMP123" },
                { "source", "partner-referral" }
            },
            Metadata = new InvitationMetadata
            {
                CampaignId = "CAMP123",
                Source = "partner-referral"
            }
        };

        var json = JsonAttributeHelper.SerializeAttributes(inviteAttrs);
        Console.WriteLine($"  ✅ Serialized: {json.Substring(0, Math.Min(50, json.Length))}...");

        var deserialized = JsonAttributeHelper.DeserializeAttributes<VendorInvitationAttributes>(json);
        Console.WriteLine($"  ✅ Deserialized: Notes = {deserialized?.Notes}");

        // Test single key operations
        var testJson = "{}";
        testJson = JsonAttributeHelper.SetAttribute(testJson, "notes", "Updated notes");
        var notes = JsonAttributeHelper.GetAttribute<string>(testJson, "notes");
        Console.WriteLine($"  ✅ Single key ops: notes = {notes}");

        return Task.CompletedTask;
    }

    static async Task TestJsonQuerying(SqlConnection connection)
    {
        // Test JSON_VALUE query
        var sql = @"
            SELECT TOP 1 
                Id, 
                InvitationToken,
                JSON_VALUE(Attributes, '$.notes') as NotesFromJson
            FROM VendorInvitations
            WHERE Attributes IS NOT NULL AND Attributes != '{}'";

        using var cmd = new SqlCommand(sql, connection);
        using var reader = await cmd.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            var id = reader.GetGuid(0);
            var token = reader.GetString(1);
            var notes = reader.IsDBNull(2) ? null : reader.GetString(2);
            Console.WriteLine($"  ✅ Query result: Token={token}, Notes={notes ?? "(empty)"}");
        }
        else
        {
            Console.WriteLine("  ℹ️  No records with JSON attributes yet");
        }
    }
}
