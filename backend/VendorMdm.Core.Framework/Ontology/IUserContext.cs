using System;
using System.Collections.Generic;

namespace VendorMdm.Core.Framework.Ontology
{
    /// <summary>
    /// Represents the Security Context of the current actor.
    /// Enforces Application-Scoped RBAC.
    /// </summary>
    public interface IUserContext
    {
        Guid UserId { get; }
        string Email { get; }
        
        /// <summary>
        /// The Global Role (e.g., "VendorAdmin", "Viewer").
        /// </summary>
        string GlobalRole { get; }
        
        /// <summary>
        /// Determines if the user has a specific role for a specific application scope.
        /// </summary>
        /// <param name="role">The role to check (e.g., "Approver")</param>
        /// <param name="appScope">The target application (e.g., "EventApp")</param>
        /// <returns>True if authorized.</returns>
        bool HasRoleForApp(string role, string appScope);

        /// <summary>
        /// Validates if the user's session is active and secure.
        /// </summary>
        bool IsSessionValid();
    }
}
