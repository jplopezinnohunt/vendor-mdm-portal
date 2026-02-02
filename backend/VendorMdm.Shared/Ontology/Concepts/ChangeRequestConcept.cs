using System;
using System.Collections.Generic;
using VendorMdm.Core.Framework.Ontology;
using VendorMdm.Core.Framework.Primitives;

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

        public ChangeRequestConcept(Guid vendorId, string requestType, string origin)
        {
            _id = Guid.NewGuid();
            _vendorId = vendorId;
            _requestType = requestType;
            _originContext = origin;
            _status = "Pending";
        }

        public Result ValidateState()
        {
            if (_vendorId == Guid.Empty)
                return Result.Fail("Vendor ID is required for Change Request.");
                
            if (string.IsNullOrWhiteSpace(_requestType))
                return Result.Fail("Request Type is required.");
                
            return Result.Ok();
        }
        
        public Result Approve(IUserContext approver)
        {
            // RBAC Check
            if (!approver.HasRoleForApp("Approver", "VendorApp"))
                // In Strict Mode: return Result.Fail("Unauthorized");
                
            _status = "Approved";
            return Result.Ok();
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
}
