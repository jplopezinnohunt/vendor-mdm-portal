using System;
using System.Collections.Generic;
using System.Linq;
using VendorMdm.Core.Framework.Ontology;
using VendorMdm.Core.Framework.Primitives;
using VendorMdm.Shared.Models;
using VendorMdm.Shared.Ontology.Interfaces;

namespace VendorMdm.Shared.Ontology.Concepts
{
    public class InvitationConcept : IOntologyConcept, IAuditableEntity
    {
        private readonly Guid _id;
        private readonly string _originContext;
        private readonly List<object> _domainEvents = new();
        
        private string _vendorLegalName;
        private string _vendorType;
        private string _primaryContactEmail;
        private string _status;
        private string _invitedByName;
        private DateTime _expiresAt;
        private string _currentStage;

        public Guid Id => _id;
        public string OriginContext => _originContext;
        public string VendorLegalName => _vendorLegalName;
        public string VendorType => _vendorType;
        public string Status => _status;
        public string CurrentStage => _currentStage;

        public InvitationConcept(
            string vendorLegalName, 
            string vendorType, 
            string primaryContactEmail,
            string invitedByName,
            DateTime expiresAt,
            string origin)
        {
            _id = Guid.NewGuid();
            _vendorLegalName = vendorLegalName;
            _vendorType = vendorType;
            _primaryContactEmail = primaryContactEmail;
            _invitedByName = invitedByName;
            _expiresAt = expiresAt;
            _originContext = origin;
            _status = "Pending";
            _currentStage = "InvitationSent";
        }

        public Result ValidateState()
        {
            if (string.IsNullOrWhiteSpace(_vendorLegalName))
                return Result.Fail("Vendor Legal Name is required.");
                
            if (string.IsNullOrWhiteSpace(_primaryContactEmail))
                return Result.Fail("Primary Contact Email is required.");
                
            if (!_primaryContactEmail.Contains("@"))
                return Result.Fail("Invalid email format.");
                
            if (_expiresAt <= DateTime.UtcNow)
                return Result.Fail("Expiration date must be in the future.");
                
            return Result.Ok();
        }

        public Result TransitionTo(string newStatus, string reason)
        {
            var validTransitions = new Dictionary<string, string[]>
            {
                ["Pending"] = new[] { "Accepted", "Expired", "Cancelled" },
                ["Accepted"] = new[] { "Completed", "Cancelled" },
                ["Expired"] = new[] { "Pending" }, // Allow resend
                ["Completed"] = Array.Empty<string>(),
                ["Cancelled"] = Array.Empty<string>()
            };

            if (!validTransitions.ContainsKey(_status))
                return Result.Fail($"Unknown status: {_status}");

            if (!validTransitions[_status].Contains(newStatus))
                return Result.Fail($"Invalid transition from {_status} to {newStatus}");

            _status = newStatus;
            return Result.Ok();
        }

        public IDictionary<string, object> GetFunctionalLogs()
        {
            return new Dictionary<string, object>
            {
                { "Entity", "VendorInvitation" },
                { "Id", _id },
                { "Status", _status },
                { "CurrentStage", _currentStage },
                { "VendorType", _vendorType },
                { "ExpiresAt", _expiresAt }
            };
        }

        public IEnumerable<object> GetDomainEvents() => _domainEvents.AsReadOnly();

        #region IAuditableEntity Implementation

        public string GetEntityType() => "VendorInvitation";

        public AuditableFields GetAuditableFields()
        {
            return new AuditableFields
            {
                // MUST audit these fields (critical for compliance)
                CriticalFields = new List<string>
                {
                    "VendorLegalName",
                    "PrimaryContactEmail",
                    "Status",
                    "CurrentStage",
                    "VendorType"
                },

                // SHOULD audit these fields (important but not critical)
                StandardFields = new List<string>
                {
                    "InvitedByName",
                    "ExpiresAt",
                    "CompletedAt",
                    "VendorApplicationId",
                    "AccountGroup"
                },

                // MUST NOT audit these fields (sensitive data)
                SensitiveFields = new List<string>
                {
                    "InvitationToken",
                    "MfaCode",
                    "MfaCodeExpiresAt"
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
                    { "VendorType", _vendorType },
                    { "VendorLegalName", _vendorLegalName },
                    { "OriginContext", _originContext },
                    { "CurrentStatus", _status },
                    { "CurrentStage", _currentStage },
                    { "InvitedBy", _invitedByName }
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
                "Accepted", "Completed", "Expired", "Cancelled",
                "Resent", "MfaTriggered", "MfaVerified",
                "StageChanged"
            };

            return auditableActions.Contains(action);
        }

        private object? FilterAuditableValues(object? state, AuditableFields auditableFields)
        {
            if (state == null) return null;

            // If state is a VendorInvitation entity, extract only auditable fields
            if (state is VendorInvitation invitation)
            {
                var filtered = new Dictionary<string, object?>();

                // Add critical fields
                if (auditableFields.CriticalFields.Contains("VendorLegalName"))
                    filtered["VendorLegalName"] = invitation.VendorLegalName;
                if (auditableFields.CriticalFields.Contains("PrimaryContactEmail"))
                    filtered["PrimaryContactEmail"] = invitation.PrimaryContactEmail;
                if (auditableFields.CriticalFields.Contains("Status"))
                    filtered["Status"] = invitation.Status;
                if (auditableFields.CriticalFields.Contains("CurrentStage"))
                    filtered["CurrentStage"] = invitation.CurrentStage.ToString();
                if (auditableFields.CriticalFields.Contains("VendorType"))
                    filtered["VendorType"] = invitation.VendorType;

                // Add standard fields
                if (auditableFields.StandardFields.Contains("InvitedByName"))
                    filtered["InvitedByName"] = invitation.InvitedByName;
                if (auditableFields.StandardFields.Contains("ExpiresAt"))
                    filtered["ExpiresAt"] = invitation.ExpiresAt;
                if (auditableFields.StandardFields.Contains("CompletedAt"))
                    filtered["CompletedAt"] = invitation.CompletedAt;

                return filtered;
            }

            // If state is already a dictionary or anonymous object, return as-is
            return state;
        }

        #endregion
    }
}
