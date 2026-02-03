using Microsoft.EntityFrameworkCore;
using VendorMdm.Api.Data;
using VendorMdm.Core.Framework.Security.Authorization;
using VendorMdm.Core.Framework.Primitives;
using System.Text.Json;

namespace VendorMdm.Api.Repositories;

/// <summary>
/// Repository for user-role mappings.
/// Implements IUserRoleRepository from Core.Framework.
/// Adapts to VendorMdm.Shared.Models.UserRole schema (Username-based).
/// </summary>
public class UserRoleRepository : IUserRoleRepository
{
    private readonly SqlDbContext _context;

    public UserRoleRepository(SqlDbContext context)
    {
        _context = context;
    }

    public async Task<string[]?> GetRolesAsync(Guid userId, string? appContext)
    {
        // Join UserRole with User to filter by UserId (since UserRole uses Username)
        var query = from ur in _context.UsersAndRoles
                    join u in _context.Users on ur.Username equals u.Username
                    where u.Id == userId
                    select new { ur.Role, ur.Attributes };

        // Execute query
        var roleEntities = await query.ToListAsync();

        // Filter by AppContext from Attributes if needed
        if (!string.IsNullOrEmpty(appContext))
        {
            var filteredRoles = new List<string>();
            foreach (var r in roleEntities)
            {
                if (string.IsNullOrEmpty(r.Attributes) || r.Attributes == "{}") continue;
                
                try 
                {
                    using var doc = JsonDocument.Parse(r.Attributes);
                    if (doc.RootElement.TryGetProperty("appContext", out var ctx) && 
                        ctx.GetString() == appContext)
                    {
                        filteredRoles.Add(r.Role);
                    }
                }
                catch { /* Ignore parse errors */ }
            }
            return filteredRoles.ToArray();
        }

        return roleEntities.Select(r => r.Role).Distinct().ToArray();
    }

    public async Task GrantRoleAsync(Guid userId, string role, string appContext, Guid grantedBy)
    {
        // 1. Get User to find Username
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return; // or throw?

        // 2. Check if role exists
        // We can't easily check AppContext in SQL if it's in JSON without computed column.
        // For now, let's just check Role+Username to avoid basic dupes, or fetch all roles for user.
        var existingRoles = await _context.UsersAndRoles
            .Where(ur => ur.Username == user.Username && ur.Role == role)
            .ToListAsync();

        foreach (var r in existingRoles)
        {
            // Check AppContext in Attributes
            if (string.IsNullOrEmpty(appContext)) return; // Already exists (global)

            try
            {
                 using var doc = JsonDocument.Parse(r.Attributes);
                 if (doc.RootElement.TryGetProperty("appContext", out var ctx) && 
                     ctx.GetString() == appContext)
                 {
                     return; // Already granted for this context
                 }
            }
            catch {}
        }
        
        // 3. Create Attributes JSON
        var attributes = new Dictionary<string, object>();
        if (!string.IsNullOrEmpty(appContext)) attributes["appContext"] = appContext;
        attributes["grantedBy"] = grantedBy;
        attributes["grantedAt"] = DateTime.UtcNow;

        var userRole = new VendorMdm.Shared.Models.UserRole
        {
            Id = Guid.NewGuid(),
            Username = user.Username,
            Role = role,
            Attributes = JsonSerializer.Serialize(attributes)
        };

        _context.UsersAndRoles.Add(userRole);
        await _context.SaveChangesAsync();
    }

    public async Task RevokeRoleAsync(Guid userId, string role, string appContext, Guid revokedBy)
    {
        // 1. Get User
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return;

        // 2. Find Role assignments
        var roles = await _context.UsersAndRoles
            .Where(ur => ur.Username == user.Username && ur.Role == role)
            .ToListAsync();

        foreach (var r in roles)
        {
             bool match = false;
             if (string.IsNullOrEmpty(appContext)) 
             {
                 // Revoking global role? Or all? 
                 // If attributes doesn't have appContext, it's global?
                 // Let's assume lenient matching: if passed appContext is null, revoke any matching role? 
                 // Or better strict: if appContext null, remove roles without appContext.
                 if (r.Attributes == "{}" || !r.Attributes.Contains("appContext")) match = true;
             }
             else
             {
                 if (r.Attributes.Contains(appContext)) match = true; // Simple check
             }

             if (match)
             {
                 _context.UsersAndRoles.Remove(r);
             }
        }

        await _context.SaveChangesAsync();
    }
}
