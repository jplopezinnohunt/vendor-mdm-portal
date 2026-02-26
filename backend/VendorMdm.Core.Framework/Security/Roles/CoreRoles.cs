namespace VendorMdm.Core.Framework.Security.Roles;

/// <summary>
/// Core role definitions shared across all MDM applications.
/// Apps can extend with app-specific roles.
/// </summary>
public static class CoreRoles
{
    /// <summary>
    /// System administrator - Full access across ALL apps
    /// </summary>
    public const string SystemAdmin = "SystemAdmin";

    /// <summary>
    /// Application administrator - Full access within ONE app
    /// </summary>
    public const string AppAdmin = "AppAdmin";

    /// <summary>
    /// Viewer - Read-only access
    /// </summary>
    public const string Viewer = "Viewer";

    /// <summary>
    /// Editor - Can create and edit, but not approve
    /// </summary>
    public const string Editor = "Editor";

    /// <summary>
    /// Approver - Can approve/reject submissions
    /// </summary>
    public const string Approver = "Approver";

    /// <summary>
    /// Auditor - Can view audit logs and reports
    /// </summary>
    public const string Auditor = "Auditor";

    // ============================================
    // App-Specific Roles (Vendor MDM Portal)
    // ============================================

    /// <summary>MDM Administrator - Full admin within MDM portal</summary>
    public const string MDMAdmin = "MDMAdmin";

    /// <summary>IT Administrator - IT operations and configuration</summary>
    public const string ITAdmin = "ITAdmin";

    /// <summary>Requestor - Can submit vendor requests</summary>
    public const string Requestor = "Requestor";

    /// <summary>Vendor - External vendor user</summary>
    public const string Vendor = "Vendor";

    /// <summary>
    /// All core roles
    /// </summary>
    public static readonly string[] All = new[]
    {
        SystemAdmin,
        AppAdmin,
        Viewer,
        Editor,
        Approver,
        Auditor,
        MDMAdmin,
        ITAdmin,
        Requestor,
        Vendor
    };

    /// <summary>
    /// Checks if a role is a core role.
    /// </summary>
    public static bool IsCoreRole(string role) => All.Contains(role);
}

/// <summary>
/// Core permission definitions shared across all MDM applications.
/// Permissions follow the pattern: "{resource}.{action}"
/// </summary>
public static class CorePermissions
{
    // User Management
    public const string UsersView = "users.view";
    public const string UsersCreate = "users.create";
    public const string UsersEdit = "users.edit";
    public const string UsersDelete = "users.delete";

    // Role Management
    public const string RolesView = "roles.view";
    public const string RolesGrant = "roles.grant";
    public const string RolesRevoke = "roles.revoke";

    // Audit Logs
    public const string AuditView = "audit.view";
    public const string AuditExport = "audit.export";

    // System Configuration
    public const string ConfigView = "config.view";
    public const string ConfigEdit = "config.edit";

    /// <summary>
    /// All core permissions
    /// </summary>
    public static readonly string[] All = new[]
    {
        UsersView, UsersCreate, UsersEdit, UsersDelete,
        RolesView, RolesGrant, RolesRevoke,
        AuditView, AuditExport,
        ConfigView, ConfigEdit
    };
}

/// <summary>
/// Role-to-Permission mappings for core roles.
/// </summary>
public static class CoreRolePermissions
{
    public static readonly Dictionary<string, string[]> Mappings = new()
    {
        [CoreRoles.SystemAdmin] = CorePermissions.All,
        
        [CoreRoles.AppAdmin] = new[]
        {
            CorePermissions.UsersView,
            CorePermissions.UsersCreate,
            CorePermissions.UsersEdit,
            CorePermissions.RolesView,
            CorePermissions.RolesGrant,
            CorePermissions.RolesRevoke,
            CorePermissions.AuditView,
            CorePermissions.ConfigView,
            CorePermissions.ConfigEdit
        },
        
        [CoreRoles.Viewer] = new[]
        {
            CorePermissions.UsersView,
            CorePermissions.RolesView,
            CorePermissions.AuditView,
            CorePermissions.ConfigView
        },
        
        [CoreRoles.Editor] = new[]
        {
            CorePermissions.UsersView,
            CorePermissions.UsersCreate,
            CorePermissions.UsersEdit,
            CorePermissions.RolesView,
            CorePermissions.ConfigView
        },
        
        [CoreRoles.Approver] = new[]
        {
            CorePermissions.UsersView,
            CorePermissions.RolesView,
            CorePermissions.AuditView,
            CorePermissions.ConfigView
        },
        
        [CoreRoles.Auditor] = new[]
        {
            CorePermissions.AuditView,
            CorePermissions.AuditExport
        }
    };

    /// <summary>
    /// Gets all permissions for a role.
    /// </summary>
    public static string[] GetPermissions(string role)
    {
        return Mappings.TryGetValue(role, out var permissions) 
            ? permissions 
            : Array.Empty<string>();
    }
}
