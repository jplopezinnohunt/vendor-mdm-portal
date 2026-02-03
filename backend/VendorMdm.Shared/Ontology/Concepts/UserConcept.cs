using System;
using System.Collections.Generic;
using System.Linq;
using VendorMdm.Core.Framework.Ontology;
using VendorMdm.Core.Framework.Primitives;
using VendorMdm.Shared.Models;
using VendorMdm.Shared.Ontology.Interfaces;

namespace VendorMdm.Shared.Ontology.Concepts
{
    public class UserConcept : IOntologyConcept, IAuditableEntity
    {
        private readonly Guid _id;
        private readonly string _originContext;
        private readonly List<object> _domainEvents = new();
        
        private string _email;
        private string _username;
        private string _fullName;
        private List<string> _roles;
        private bool _isBlocked;
        private string _status;

        public Guid Id => _id;
        public string OriginContext => _originContext;
        public string Email => _email;
        public string Username => _username;
        public string FullName => _fullName;
        public List<string> Roles => _roles;
        public bool IsBlocked => _isBlocked;
        public string Status => _status;

        public UserConcept(
            string email, 
            string username, 
            string fullName,
            List<string> roles,
            string origin)
        {
            _id = Guid.NewGuid();
            _email = email;
            _username = username;
            _fullName = fullName;
            _roles = roles ?? new List<string>();
            _isBlocked = false;
            _status = "Active";
            _originContext = origin;
        }

        public Result ValidateState()
        {
            if (string.IsNullOrWhiteSpace(_email))
                return Result.Fail("Email is required.");
                
            if (!_email.Contains("@"))
                return Result.Fail("Invalid email format.");
                
            if (string.IsNullOrWhiteSpace(_username))
                return Result.Fail("Username is required.");
                
            if (string.IsNullOrWhiteSpace(_fullName))
                return Result.Fail("Full Name is required.");
                
            return Result.Ok();
        }

        public Result GrantRole(string role, string grantedBy)
        {
            if (_roles.Contains(role))
                return Result.Fail($"User already has role: {role}");

            _roles.Add(role);
            return Result.Ok();
        }

        public Result RevokeRole(string role, string revokedBy)
        {
            if (!_roles.Contains(role))
                return Result.Fail($"User does not have role: {role}");

            _roles.Remove(role);
            return Result.Ok();
        }

        public Result BlockUser(string reason)
        {
            if (_isBlocked)
                return Result.Fail("User is already blocked.");

            _isBlocked = true;
            _status = "Blocked";
            return Result.Ok();
        }

        public Result UnblockUser(string reason)
        {
            if (!_isBlocked)
                return Result.Fail("User is not blocked.");

            _isBlocked = false;
            _status = "Active";
            return Result.Ok();
        }

        public IDictionary<string, object> GetFunctionalLogs()
        {
            return new Dictionary<string, object>
            {
                { "Entity", "User" },
                { "Id", _id },
                { "Status", _status },
                { "IsBlocked", _isBlocked },
                { "RoleCount", _roles.Count },
                { "Roles", string.Join(", ", _roles) }
            };
        }

        public IEnumerable<object> GetDomainEvents() => _domainEvents.AsReadOnly();

        #region IAuditableEntity Implementation

        public string GetEntityType() => "User";

        public AuditableFields GetAuditableFields()
        {
            return new AuditableFields
            {
                // MUST audit these fields (critical for compliance)
                CriticalFields = new List<string>
                {
                    "Email",
                    "Username",
                    "Roles",
                    "IsBlocked",
                    "Status"
                },

                // SHOULD audit these fields (important but not critical)
                StandardFields = new List<string>
                {
                    "FullName",
                    "LastLoginAt",
                    "FailedLoginAttempts",
                    "TwoFactorEnabled"
                },

                // MUST NOT audit these fields (sensitive data)
                SensitiveFields = new List<string>
                {
                    "PasswordHash",
                    "PasswordSalt",
                    "TwoFactorSecret",
                    "RecoveryCode",
                    "ApiKey",
                    "RefreshToken"
                }
            };
        }

        public AuditLogEntry CreateAuditEntry(
            string action,
            object? oldState = null,
            object? newState = null,
            string? reason = null)
        {
            var auditableFields = GetAuditableFields();

            return new AuditLogEntry
            {
                EntityType = GetEntityType(),
                EntityId = _id,
                Action = action,
                OldValues = FilterAuditableValues(oldState, auditableFields),
                NewValues = FilterAuditableValues(newState, auditableFields),
                Reason = reason,
                Metadata = new Dictionary<string, object>
                {
                    { "Email", _email },
                    { "Username", _username },
                    { "OriginContext", _originContext },
                    { "CurrentStatus", _status },
                    { "IsBlocked", _isBlocked },
                    { "RoleCount", _roles.Count }
                },
                SchemaVersion = "v1.0.0"
            };
        }

        public bool ShouldAudit(string action)
        {
            // Define which actions should be audited
            var auditableActions = new[]
            {
                "Created", "Updated", "Deleted",
                "RoleGranted", "RoleRevoked",
                "Blocked", "Unblocked",
                "PasswordChanged", "PasswordReset",
                "TwoFactorEnabled", "TwoFactorDisabled",
                "LoginSuccess", "LoginFailed",
                "ApiKeyGenerated", "ApiKeyRevoked"
            };

            return auditableActions.Contains(action);
        }

        private object? FilterAuditableValues(object? state, AuditableFields auditableFields)
        {
            if (state == null) return null;

            // If state is a User entity, extract only auditable fields
            if (state is User user)
            {
                var filtered = new Dictionary<string, object?>();

                // Add critical fields
                if (auditableFields.CriticalFields.Contains("Email"))
                    filtered["Email"] = user.Email;
                if (auditableFields.CriticalFields.Contains("Username"))
                    filtered["Username"] = user.Username;
                if (auditableFields.CriticalFields.Contains("Roles"))
                    filtered["Roles"] = _roles;
                if (auditableFields.CriticalFields.Contains("IsBlocked"))
                    filtered["IsBlocked"] = _isBlocked;
                if (auditableFields.CriticalFields.Contains("Status"))
                    filtered["Status"] = _status;

                // FullName is in Attributes (JSONB), not a direct property
                // Standard fields would need to extract from Data if needed

                return filtered;
            }

            // If state is already a dictionary or anonymous object, return as-is
            return state;
        }

        #endregion
    }
}
