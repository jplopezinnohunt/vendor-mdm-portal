using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using VendorMdm.Shared.Models;

namespace VendorMdm.Api.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(SqlDbContext context, ILogger logger)
    {
        // Ensure database created
        await context.Database.EnsureCreatedAsync();

        // Seed Bootstrap Admin
        const string adminEmail = "MDMUNESCOADM@unesco.org";
        const string adminUsername = "MDMUNESCOADM";
        
        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Username == adminUsername || u.Email == adminEmail);
        
        if (adminUser == null)
        {
            logger.LogInformation("Seeding Bootstrap Admin User: {Username}", adminUsername);
            
            adminUser = new User
            {
                Id = Guid.NewGuid(),
                Username = adminUsername,
                Email = adminEmail,

                Roles = new List<string> { "Admin" },
                Status = "Active",
                SourceSystem = "System",
                AuthProvider = "Local",
                AuthMethod = "LocalStrong",
                SchemaVersion = "v1.0.0",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PasswordHash = HashPassword("UNESCO_MDM_2026!") // Default Password - Should be changed
            };
            
            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
            
            logger.LogInformation("Bootstrap Admin Created. Id: {Id}", adminUser.Id);
        }

        else
        {
            var modified = false;
            // Ensure Role is Admin
            if (!adminUser.Roles.Contains("Admin"))
            {
                if (!adminUser.Roles.Any()) adminUser.Roles = new List<string>();
                adminUser.Roles.Add("Admin");
                modified = true;
            }

            // Fix: Ensure AuthMethod is LocalStrong for this user
            if (adminUser.AuthMethod != "LocalStrong")
            {
                adminUser.AuthMethod = "LocalStrong";
                modified = true;
            }

            if (modified)
            {
                await context.SaveChangesAsync();
                logger.LogInformation("Bootstrap Admin updated (Roles/AuthMethod).");
            }
        }

        // --- MIGRATION: Fix Role Naming ("Requester" -> "Requestor") ---
        var usersWithLegacyRole = await context.Users
            .Where(u => u.Roles.Contains("Requester")) // Can't easily use JSONB query in EF Core LINQ depending on provider, so client-side filter is safer for minor migration
            .ToListAsync();

        if (usersWithLegacyRole.Any())
        {
            logger.LogInformation("Found {Count} users with legacy 'Requester' role. Migrating...", usersWithLegacyRole.Count);
            foreach (var user in usersWithLegacyRole)
            {
                // Remove legacy
                user.Roles.RemoveAll(r => r == "Requester");
                
                // Add new if not exists
                if (!user.Roles.Contains("Requestor"))
                {
                    user.Roles.Add("Requestor");
                }
                
                // Deduplicate just in case
                user.Roles = user.Roles.Distinct().ToList();
            }
            await context.SaveChangesAsync();
            logger.LogInformation("Role migration complete.");
        }
    }

    private static string HashPassword(string password)
    {
        // Simple SHA256 for the bootstrap user. 
        // In a full Local Auth system, use PBKDF2 or BCrypt.
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
