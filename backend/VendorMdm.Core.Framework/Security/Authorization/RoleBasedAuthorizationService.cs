using VendorMdm.Core.Framework.Primitives;
using VendorMdm.Core.Framework.Logging;
using VendorMdm.Core.Framework.Security.Roles;

namespace VendorMdm.Core.Framework.Security.Authorization;

/// <summary>
/// Role-based implementation of IAuthorizationService.
/// Supports app-scoped authorization with role-permission mappings.
/// </summary>
public class RoleBasedAuthorizationService : IAuthorizationService
{
    private readonly IStructuredLogger _logger;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IRolePermissionRepository _rolePermissionRepository;

    public RoleBasedAuthorizationService(
        IStructuredLogger logger,
        IUserRoleRepository userRoleRepository,
        IRolePermissionRepository rolePermissionRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userRoleRepository = userRoleRepository ?? throw new ArgumentNullException(nameof(userRoleRepository));
        _rolePermissionRepository = rolePermissionRepository ?? throw new ArgumentNullException(nameof(rolePermissionRepository));
    }

    public async Task<bool> HasPermissionAsync(Guid userId, string permission, string? appContext = null)
    {
        try
        {
            // Get user roles
            var roles = await _userRoleRepository.GetRolesAsync(userId, appContext);
            if (roles == null || roles.Length == 0)
            {
                _logger.LogDebug("User has no roles", ("UserId", userId), ("AppContext", appContext ?? ""));
                return false;
            }

            // Check if any role has the permission
            foreach (var role in roles)
            {
                var permissions = await _rolePermissionRepository.GetPermissionsAsync(role, appContext);
                if (permissions != null && permissions.Contains(permission))
                {
                    _logger.LogDebug("Permission granted", ("UserId", userId), ("Permission", permission), ("Role", role));
                    return true;
                }
            }

            _logger.LogDebug("Permission denied", ("UserId", userId), ("Permission", permission));
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Permission check failed", ("UserId", userId), ("Permission", permission));
            return false;
        }
    }

    public async Task<bool> HasRoleAsync(Guid userId, string role, string? appContext = null)
    {
        try
        {
            var roles = await _userRoleRepository.GetRolesAsync(userId, appContext);
            var hasRole = roles != null && roles.Contains(role);

            _logger.LogDebug(
                hasRole ? "Role check passed" : "Role check failed",
                ("UserId", userId),
                ("Role", role),
                ("AppContext", appContext ?? ""));

            return hasRole;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Role check failed", ("UserId", userId), ("Role", role));
            return false;
        }
    }

    public async Task<Result> GrantRoleAsync(Guid userId, string role, string appContext, Guid grantedBy)
    {
        using var _ = _logger.BeginOperation("GrantRole", ("UserId", userId), ("Role", role), ("GrantedBy", grantedBy));

        try
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(role))
                return Result.Fail("Role is required");

            if (string.IsNullOrWhiteSpace(appContext))
                return Result.Fail("App context is required");

            // Check if user already has the role
            var hasRole = await HasRoleAsync(userId, role, appContext);
            if (hasRole)
            {
                _logger.LogWarning("User already has role", ("UserId", userId), ("Role", role));
                return Result.Fail("User already has this role");
            }

            // Grant role
            await _userRoleRepository.GrantRoleAsync(userId, role, appContext, grantedBy);

            _logger.LogInformation("Role granted successfully", ("UserId", userId), ("Role", role));
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Role grant failed", ("UserId", userId), ("Role", role));
            return Result.Fail("Role grant failed");
        }
    }

    public async Task<Result> RevokeRoleAsync(Guid userId, string role, string appContext, Guid revokedBy)
    {
        using var _ = _logger.BeginOperation("RevokeRole", ("UserId", userId), ("Role", role), ("RevokedBy", revokedBy));

        try
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(role))
                return Result.Fail("Role is required");

            if (string.IsNullOrWhiteSpace(appContext))
                return Result.Fail("App context is required");

            // Check if user has the role
            var hasRole = await HasRoleAsync(userId, role, appContext);
            if (!hasRole)
            {
                _logger.LogWarning("User does not have role", ("UserId", userId), ("Role", role));
                return Result.Fail("User does not have this role");
            }

            // Revoke role
            await _userRoleRepository.RevokeRoleAsync(userId, role, appContext, revokedBy);

            _logger.LogInformation("Role revoked successfully", ("UserId", userId), ("Role", role));
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Role revoke failed", ("UserId", userId), ("Role", role));
            return Result.Fail("Role revoke failed");
        }
    }

    public async Task<Result<string[]>> GetRolesAsync(Guid userId, string appContext)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(appContext))
                return Result.Fail<string[]>("App context is required");

            var roles = await _userRoleRepository.GetRolesAsync(userId, appContext);
            return Result.Ok(roles ?? Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get roles failed", ("UserId", userId), ("AppContext", appContext));
            return Result.Fail<string[]>("Failed to get roles");
        }
    }

    public async Task<Result<string[]>> GetPermissionsAsync(Guid userId, string appContext)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(appContext))
                return Result.Fail<string[]>("App context is required");

            // Get user roles
            var roles = await _userRoleRepository.GetRolesAsync(userId, appContext);
            if (roles == null || roles.Length == 0)
                return Result.Ok(Array.Empty<string>());

            // Get permissions for all roles
            var allPermissions = new HashSet<string>();
            foreach (var role in roles)
            {
                var permissions = await _rolePermissionRepository.GetPermissionsAsync(role, appContext);
                if (permissions != null)
                {
                    foreach (var permission in permissions)
                    {
                        allPermissions.Add(permission);
                    }
                }
            }

            return Result.Ok(allPermissions.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get permissions failed", ("UserId", userId), ("AppContext", appContext));
            return Result.Fail<string[]>("Failed to get permissions");
        }
    }
}

/// <summary>
/// Repository for user role operations.
/// Apps must implement this interface.
/// </summary>
public interface IUserRoleRepository
{
    Task<string[]?> GetRolesAsync(Guid userId, string? appContext);
    Task GrantRoleAsync(Guid userId, string role, string appContext, Guid grantedBy);
    Task RevokeRoleAsync(Guid userId, string role, string appContext, Guid revokedBy);
}

/// <summary>
/// Repository for role permission mappings.
/// Apps must implement this interface.
/// </summary>
public interface IRolePermissionRepository
{
    Task<string[]?> GetPermissionsAsync(string role, string? appContext);
}

/// <summary>
/// Default implementation of IRolePermissionRepository using CoreRolePermissions.
/// </summary>
public class DefaultRolePermissionRepository : IRolePermissionRepository
{
    public Task<string[]?> GetPermissionsAsync(string role, string? appContext)
    {
        // Use core role permissions
        var permissions = CoreRolePermissions.GetPermissions(role);
        return Task.FromResult<string[]?>(permissions);
    }
}
