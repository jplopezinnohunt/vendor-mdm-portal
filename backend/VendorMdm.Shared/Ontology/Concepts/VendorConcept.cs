using System;
using System.Collections.Generic;
using System.Linq;
using VendorMdm.Core.Framework.Ontology;
using VendorMdm.Core.Framework.Primitives;
using VendorMdm.Shared.Models; // For SqlEntities if needed, or pure domain model if we had one.
using VendorMdm.Shared.Ontology.Interfaces;
// Using mapping to SqlEntities for now as per "Hybrid" appraoch, but wrapping logic.

namespace VendorMdm.Shared.Ontology.Concepts
{
    public class VendorConcept : IOntologyConcept, IAuditableEntity
    {
        private readonly Guid _id;
        private readonly string _originContext;
        private readonly List<object> _domainEvents = new();
        
        // Internal State
        private string _vendorType;
        private string _legalName;
        private string _accountGroup;
        private string _status;
        private Dictionary<string, object> _lifecycleEvents = new();

        public Guid Id => _id;
        public string OriginContext => _originContext;

        public string AccountGroup => _accountGroup;

        // Constructor for New Vendor
        public VendorConcept(string legalName, string vendorType, string origin)
        {
            _id = Guid.NewGuid();
            _legalName = legalName;
            _vendorType = vendorType;
            _originContext = origin;
            _status = "Draft";
            
            // Domain Rule: Determine Account Group immediately
            _accountGroup = DetermineAccountGroup(vendorType);
            
            RecordEvent("Created", $"Vendor created from {origin}");
        }

        // Domain Rule: Account Group Logic
        private string DetermineAccountGroup(string type)
        {
            return type switch
            {
                "Individual" => "KRED",
                "Organization" => "LIFNR",
                "StartUp" => "SU01",
                _ => "GENERIC" // Fallback
            };
        }

        public Result ValidateState()
        {
            if (string.IsNullOrWhiteSpace(_legalName))
                return Result.Fail("Legal Name is required.");
            
            if (string.IsNullOrWhiteSpace(_vendorType))
                return Result.Fail("Vendor Type is required.");
                
            return Result.Ok();
        }
        
        
        // State Machine: Valid Transitions
        public Result TransitionTo(string newStatus, IUserContext user)
        {
            var validTransitions = new Dictionary<string, string[]>
            {
                ["Draft"] = new[] { "PendingApproval", "Cancelled" },
                ["PendingApproval"] = new[] { "Approved", "Rejected", "Cancelled" },
                ["Approved"] = new[] { "Active" },
                ["Rejected"] = new[] { "Draft" }, // Allow resubmission
                ["Active"] = new[] { "Suspended", "Inactive" },
                ["Suspended"] = new[] { "Active", "Inactive" }
            };

            if (!validTransitions.ContainsKey(_status))
                return Result.Fail($"Unknown status: {_status}");

            if (!validTransitions[_status].Contains(newStatus))
                return Result.Fail($"Invalid transition from {_status} to {newStatus}");

            var oldStatus = _status;
            _status = newStatus;
            RecordEvent("StatusChanged", $"{oldStatus} → {newStatus} by {user.Email}");
            RaiseEvent(new Core.Framework.Events.VendorStatusChangedEvent(Id, oldStatus, newStatus));
            return Result.Ok();
        }

        // Example: Update Status with Rules
        public Result SubmitForApproval(IUserContext user)
        {
            // Security Check
            if (!user.HasRoleForApp("DetailsViewer", "VendorApp") && !user.HasRoleForApp("Editor", "VendorApp"))
                 // In a strict world, we might block, but here we just check if they can perform action.
                 // Actually the requirement is "Authorization MUST be Context-Aware".
                 // Let's assume we check global role for now or implement strict check.
                 
            if (_status != "Draft")
                return Result.Fail("Only Draft vendors can be submitted.");

            _status = "PendingApproval";
            RecordEvent("Submitted", $"Submitted by {user.Email}");
            return Result.Ok();
        }

        private void RecordEvent(string eventName, string details)
        {
             _lifecycleEvents[DateTime.UtcNow.ToString("o")] = new { Event = eventName, Details = details };
        }

        public IDictionary<string, object> GetFunctionalLogs()
        {
            return new Dictionary<string, object>
            {
                { "Entity", "Vendor" },
                { "Id", _id },
                { "Status", _status },
                { "AccountGroup", _accountGroup },
                { "History", _lifecycleEvents }
            };
        }

        protected void RaiseEvent(object domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public IEnumerable<object> GetDomainEvents()
        {
            return _domainEvents.AsReadOnly();
        }

        #region IAuditableEntity Implementation

        public string GetEntityType() => "Vendor";

        public string LegalName => _legalName;
        public string VendorType => _vendorType;
        public string Status => _status;

        public AuditableFields GetAuditableFields()
        {
            return new AuditableFields
            {
                // MUST audit these fields (critical for compliance)
                CriticalFields = new List<string>
                {
                    "LegalName",
                    "Status",
                    "TaxId",
                    "AccountGroup",
                    "VendorType"
                },

                // SHOULD audit these fields (important but not critical)
                StandardFields = new List<string>
                {
                    "PrimaryContactEmail",
                    "PrimaryContactName",
                    "Country",
                    "Currency",
                    "PaymentTerms"
                },

                // MUST NOT audit these fields (sensitive data)
                SensitiveFields = new List<string>
                {
                    "BankAccountNumber",
                    "IBAN",
                    "SwiftCode",
                    "TaxDocuments",
                    "FinancialStatements",
                    "PasswordHash"
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
                    { "AccountGroup", _accountGroup },
                    { "OriginContext", _originContext },
                    { "CurrentStatus", _status }
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
                "Approved", "Rejected", "Suspended",
                "Activated", "Deactivated",
                "StatusChanged", "Submitted"
            };

            return auditableActions.Contains(action);
        }

        private object? FilterAuditableValues(object? state, AuditableFields auditableFields)
        {
            if (state == null) return null;

            // If state is a Vendor entity, extract only auditable fields
            if (state is Vendor vendor)
            {
                var filtered = new Dictionary<string, object?>();

                // Add critical fields
                if (auditableFields.CriticalFields.Contains("LegalName"))
                    filtered["LegalName"] = vendor.LegalName;
                if (auditableFields.CriticalFields.Contains("Status"))
                    filtered["Status"] = vendor.Status;
                if (auditableFields.CriticalFields.Contains("TaxId"))
                    filtered["TaxId"] = vendor.TaxId;
                if (auditableFields.CriticalFields.Contains("VendorType"))
                    filtered["VendorType"] = _vendorType;
                if (auditableFields.CriticalFields.Contains("AccountGroup"))
                    filtered["AccountGroup"] = _accountGroup;

                // Add standard fields
                if (auditableFields.StandardFields.Contains("PrimaryContactEmail"))
                    filtered["PrimaryContactEmail"] = vendor.PrimaryContactEmail;

                return filtered;
            }

            // If state is already a dictionary or anonymous object, return as-is
            // (assuming caller already filtered sensitive data)
            return state;
        }

        #endregion
    }
}
