using VendorMdm.Core.Framework.Security.Authorization;
using VendorMdm.Core.Framework.Security.Roles;

namespace VendorMdm.Api.Repositories;

/// <summary>
/// Repository for role permission mappings.
/// Implements IRolePermissionRepository from Core.Framework.
/// </summary>
public class RolePermissionRepository : IRolePermissionRepository
{
    public Task<string[]?> GetPermissionsAsync(string role, string? appContext)
    {
        // For now, we use the static definitions from Core.Framework.
        // In the future, this can be updated to read from a database table
        // to allow dynamic permission management without code changes.
        
        var permissions = CoreRolePermissions.GetPermissions(role);
        return Task.FromResult<string[]?>(permissions);
    }
}
