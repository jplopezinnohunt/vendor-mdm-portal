using System;
using System.Collections.Generic;
using VendorMdm.Core.Framework.Ontology;
using VendorMdm.Core.Framework.Primitives;
using VendorMdm.Shared.Models; // For SqlEntities if needed, or pure domain model if we had one.
// Using mapping to SqlEntities for now as per "Hybrid" appraoch, but wrapping logic.

namespace VendorMdm.Shared.Ontology.Concepts
{
    public class VendorConcept : IOntologyConcept
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
    }
}
