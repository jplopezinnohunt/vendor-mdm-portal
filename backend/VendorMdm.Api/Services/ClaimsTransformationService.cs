using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VendorMdm.Api.Data;
using VendorMdm.Shared.Models;

namespace VendorMdm.Api.Services
{
    public class ClaimsTransformationService : IClaimsTransformation
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ClaimsTransformationService> _logger;

        public ClaimsTransformationService(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<ClaimsTransformationService> logger)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var identity = (ClaimsIdentity)principal.Identity;

            // 1. Skip if not authenticated or already processed
            if (identity == null || !identity.IsAuthenticated || identity.HasClaim(c => c.Type == "RBAC_PROCESSED"))
            {
                return principal;
            }

            // 2. Identify User (OID and Email)
            var oid = principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value 
                   ?? principal.FindFirst("oid")?.Value;
            
            var email = principal.FindFirst(ClaimTypes.Email)?.Value 
                     ?? principal.FindFirst(ClaimTypes.Upn)?.Value 
                     ?? principal.FindFirst("preferred_username")?.Value;

            if (string.IsNullOrEmpty(email)) 
            {
                _logger.LogWarning("ClaimsTransformation: No Email found in token.");
                return principal; // Cannot proceed without email
            }

            // 3. Determine Role from Azure Groups (Priority)
            var groupRole = DetermineRoleFromGroups(principal);
            
            // 4. Database Lookup & Sync (Link-on-Login)
            string finalRole = "Viewer"; // Default fallback
            
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<SqlDbContext>();

                // A. Try finding by Azure OID (Fastest)
                var user = oid != null 
                    ? await dbContext.Users.FirstOrDefaultAsync(u => u.AzureAdObjectId == oid) 
                    : null;

                // B. Try finding by Email (Migration / First Login)
                if (user == null)
                {
                    user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
                    
                    if (user != null && oid != null)
                    {
                        // AUTO-LINK: Found by email, so link the OID now
                        user.AzureAdObjectId = oid;
                        user.AuthProvider = "AzureAd";
                        
                        // If we found a Group Role, update the DB role too (Sync)
                        if (groupRole != null && !user.Roles.Contains(groupRole))
                        {
                            _logger.LogInformation("Syncing Role for {Email}: Old Roles -> Adding {NewRole} (Group Match)", email, groupRole);
                            if (!user.Roles.Contains(groupRole)) user.Roles.Add(groupRole);
                        }

                        // Update Last Logon
                        user.LastLogonAt = DateTime.UtcNow;

                        // Save the link
                        await dbContext.SaveChangesAsync();
                    }
                }
                else
                {
                    // User found by OID. Check if we need to sync Group Role
                    bool needsSave = false;

                    if (groupRole != null && !user.Roles.Contains(groupRole))
                    {
                         _logger.LogInformation("Syncing Role for {Email}: Adding {NewRole} (Group Match)", email, groupRole);
                         user.Roles.Add(groupRole);
                         needsSave = true;
                    }

                    // Update Last Logon (throttle to every 5 mins to avoid DB spam if needed, but for now always update)
                    if (!user.LastLogonAt.HasValue || user.LastLogonAt.Value.AddMinutes(5) < DateTime.UtcNow)
                    {
                        user.LastLogonAt = DateTime.UtcNow;
                        needsSave = true;
                    }

                    if (needsSave) await dbContext.SaveChangesAsync();
                }

                // CHECK BLOCKING
                if (user != null && user.IsBlocked) 
                {
                    _logger.LogWarning("Blocked User {Email} attempted login.", email);
                    return principal; // Return without adding any roles/claims -> 401/403
                }

                // C. Determine Final Role
                if (user != null)
                {
                    finalRole = user.Roles.FirstOrDefault() ?? "Viewer";
                }
                else
                {
                    // User not found in DB.
                    // If they have a valid Group Role, we could JIT create them here.
                    // For now, we honour the Group Role conceptually, but since we decided "Viewer" if not in DB:
                    if (groupRole != null)
                    {
                        // Ideally: Create user. 
                        // Current Spec: Fallback to Viewer or Deny. 
                        // Let's stick to returning "Viewer" effectively, unless we want to trust the Group Claims purely in memory.
                        // DECISION: Trust the Group Claim in memory even if not in DB yet (Guest Access).
                        finalRole = groupRole;
                    }
                }
            }

            // 5. Add Capabilities
            var claims = new List<Claim>();
            
            // Avoid duplicates
            if (!principal.IsInRole(finalRole))
            {
                claims.Add(new Claim(ClaimTypes.Role, finalRole));
            }
            
            // Add 'Approver' capability if role implies it
            if (finalRole == "Admin" || finalRole == "VendorAdmin" || finalRole == "Approver")
            {
                if (!principal.IsInRole("Approver")) claims.Add(new Claim(ClaimTypes.Role, "Approver"));
            }

            claims.Add(new Claim("RBAC_PROCESSED", "true"));
            
            identity.AddClaims(claims);
            return principal;
        }

        private string? DetermineRoleFromGroups(ClaimsPrincipal principal)
        {
            var groups = principal.FindAll("groups").Select(c => c.Value).ToHashSet();
            if (groups.Count == 0) return null;

            // Priority checking (Admin > VendorAdmin > Requester > Approver > Viewer)
            // Config keys: "AzureAd:Groups:Admin", etc.
            
            if (IsInConfiguredGroup(groups, "Admin")) return "Admin";
            if (IsInConfiguredGroup(groups, "VendorAdmin")) return "VendorAdmin";
            if (IsInConfiguredGroup(groups, "Requester") || IsInConfiguredGroup(groups, "Requestor")) return "Requestor";
            if (IsInConfiguredGroup(groups, "Approver")) return "Approver";
            if (IsInConfiguredGroup(groups, "Viewer")) return "Viewer";

            return null;
        }

        private bool IsInConfiguredGroup(HashSet<string> userGroups, string roleName)
        {
            var configGroupId = _configuration[$"AzureAd:Groups:{roleName}"];
            return !string.IsNullOrEmpty(configGroupId) && userGroups.Contains(configGroupId);
        }
    }
}
