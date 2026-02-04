using System;
using System.Collections.Generic;
using VendorMdm.Core.Framework.Ontology;
using VendorMdm.Core.Framework.Primitives;
using VendorMdm.Shared.Constants;

namespace VendorMdm.Shared.Ontology.Concepts
{
    public class ChangeRequestConcept : IOntologyConcept
    {
        private readonly Guid _id;
        private readonly string _originContext;
        private readonly Guid _vendorId;
        private readonly List<object> _domainEvents = new();

        private string _requestType;
        private string _status;

        public Guid Id => _id;
        public string OriginContext => _originContext;
        public string Status => _status;

        public ChangeRequestConcept(Guid vendorId, string requestType, string origin)
        {
            _id = Guid.NewGuid();
            _vendorId = vendorId;
            _requestType = requestType;
            _originContext = origin;
            _status = ChangeRequestStatus.Draft;
        }

        /// <summary>
        /// Constructor for hydrating from existing entity
        /// </summary>
        public ChangeRequestConcept(Guid id, Guid vendorId, string requestType, string status, string origin)
        {
            _id = id;
            _vendorId = vendorId;
            _requestType = requestType;
            _originContext = origin;
            _status = status;
        }

        public Result ValidateState()
        {
            if (_vendorId == Guid.Empty)
                return Result.Fail("Vendor ID is required for Change Request.");

            if (string.IsNullOrWhiteSpace(_requestType))
                return Result.Fail("Request Type is required.");

            if (!ChangeRequestStatus.IsValid(_status))
                return Result.Fail($"Invalid status: {_status}");

            return Result.Ok();
        }

        /// <summary>
        /// Transitions to a new status using state machine validation.
        /// </summary>
        public Result TransitionTo(string newStatus)
        {
            if (!ChangeRequestStatus.IsValid(newStatus))
                return Result.Fail($"Invalid target status: {newStatus}");

            if (!ChangeRequestStatus.IsValidTransition(_status, newStatus))
                return Result.Fail($"Invalid transition from '{_status}' to '{newStatus}'. Allowed: [{string.Join(", ", ChangeRequestStatus.GetAllowedTransitions(_status))}]");

            var oldStatus = _status;
            _status = newStatus;

            // Emit domain event for status change
            _domainEvents.Add(new ChangeRequestStatusChangedEvent(_id, oldStatus, newStatus));

            return Result.Ok();
        }

        /// <summary>
        /// Submits the change request for review.
        /// </summary>
        public Result Submit()
        {
            return TransitionTo(ChangeRequestStatus.Submitted);
        }

        /// <summary>
        /// Approves the change request with RBAC check.
        /// </summary>
        public Result Approve(IUserContext approver)
        {
            // RBAC Check
            if (!approver.HasRoleForApp("Approver", "VendorApp"))
                return Result.Fail("Unauthorized: User does not have Approver role for VendorApp.");

            if (!ChangeRequestStatus.CanApprove(_status))
                return Result.Fail($"Cannot approve change request in status '{_status}'.");

            return TransitionTo(ChangeRequestStatus.Approved);
        }

        /// <summary>
        /// Rejects the change request with RBAC check.
        /// </summary>
        public Result Reject(IUserContext approver, string reason)
        {
            if (!approver.HasRoleForApp("Approver", "VendorApp"))
                return Result.Fail("Unauthorized: User does not have Approver role for VendorApp.");

            if (!ChangeRequestStatus.CanReject(_status))
                return Result.Fail($"Cannot reject change request in status '{_status}'.");

            return TransitionTo(ChangeRequestStatus.Rejected);
        }

        /// <summary>
        /// Cancels the change request.
        /// </summary>
        public Result Cancel()
        {
            if (ChangeRequestStatus.IsTerminal(_status))
                return Result.Fail($"Cannot cancel change request in terminal status '{_status}'.");

            return TransitionTo(ChangeRequestStatus.Cancelled);
        }

        public IDictionary<string, object> GetFunctionalLogs()
        {
             return new Dictionary<string, object>
            {
                { "Entity", "ChangeRequest" },
                { "Id", _id },
                { "VendorId", _vendorId },
                { "Status", _status }
            };
        }

        public IEnumerable<object> GetDomainEvents() => _domainEvents.AsReadOnly();
    }

    /// <summary>
    /// Domain event emitted when a change request status changes.
    /// </summary>
    public record ChangeRequestStatusChangedEvent(Guid ChangeRequestId, string OldStatus, string NewStatus);
}
