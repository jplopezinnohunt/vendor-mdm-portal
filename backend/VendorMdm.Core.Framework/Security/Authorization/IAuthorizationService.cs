using VendorMdm.Core.Framework.Primitives;

namespace VendorMdm.Core.Framework.Security.Authorization;

/// <summary>
/// Core authorization service for all MDM applications.
/// Provides role-based and permission-based authorization with app context.
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// Checks if a user has a specific permission.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="permission">Permission to check (e.g., "vendors.create")</param>
    /// <param name="appContext">Optional app context (VendorMDM, EmployeeMDM, etc.)</param>
    /// <returns>True if user has permission</returns>
    Task<bool> HasPermissionAsync(
        Guid userId, 
        string permission, 
        string? appContext = null);

    /// <summary>
    /// Checks if a user has a specific role.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="role">Role to check (e.g., "Admin", "Approver")</param>
    /// <param name="appContext">Optional app context</param>
    /// <returns>True if user has role</returns>
    Task<bool> HasRoleAsync(
        Guid userId, 
        string role, 
        string? appContext = null);

    /// <summary>
    /// Grants a role to a user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="role">Role to grant</param>
    /// <param name="appContext">App context</param>
    /// <param name="grantedBy">User ID of who granted the role</param>
    /// <returns>Success or failure</returns>
    Task<Result> GrantRoleAsync(
        Guid userId, 
        string role, 
        string appContext, 
        Guid grantedBy);

    /// <summary>
    /// Revokes a role from a user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="role">Role to revoke</param>
    /// <param name="appContext">App context</param>
    /// <param name="revokedBy">User ID of who revoked the role</param>
    /// <returns>Success or failure</returns>
    Task<Result> RevokeRoleAsync(
        Guid userId, 
        string role, 
        string appContext, 
        Guid revokedBy);

    /// <summary>
    /// Gets all roles for a user in a specific app context.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="appContext">App context</param>
    /// <returns>List of roles</returns>
    Task<Result<string[]>> GetRolesAsync(
        Guid userId, 
        string appContext);

    /// <summary>
    /// Gets all permissions for a user in a specific app context.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="appContext">App context</param>
    /// <returns>List of permissions</returns>
    Task<Result<string[]>> GetPermissionsAsync(
        Guid userId, 
        string appContext);
}
