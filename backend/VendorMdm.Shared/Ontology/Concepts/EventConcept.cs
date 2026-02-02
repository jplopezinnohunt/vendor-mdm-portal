using System;
using System.Collections.Generic;
using VendorMdm.Core.Framework.Ontology;
using VendorMdm.Core.Framework.Primitives;

namespace VendorMdm.Shared.Ontology.Concepts
{
    public class EventConcept : IOntologyConcept
    {
        private readonly Guid _id;
        private readonly string _originContext;
        private readonly List<object> _domainEvents = new();
        
        private string _title;
        private DateTime _startDate;
        private DateTime _endDate;
        private string _status;

        public Guid Id => _id;
        public string OriginContext => _originContext;

        public EventConcept(string title, DateTime start, DateTime end, string origin)
        {
            _id = Guid.NewGuid();
            _title = title;
            _startDate = start;
            _endDate = end;
            _originContext = origin;
            _status = "Draft";
        }

        public Result ValidateState()
        {
            if (string.IsNullOrWhiteSpace(_title))
                return Result.Fail("Event Title is required.");
                
            if (_startDate >= _endDate)
                return Result.Fail("Start Date must be before End Date.");
            
            if (_startDate < DateTime.UtcNow.AddDays(-1)) // Allow some clock skew
                return Result.Fail("Cannot create events in the past.");
                
            return Result.Ok();
        }

        public IDictionary<string, object> GetFunctionalLogs()
        {
             return new Dictionary<string, object>
            {
                { "Entity", "Event" },
                { "Id", _id },
                { "Status", _status },
                { "Duration_Days", (_endDate - _startDate).TotalDays }
            };
        }

        public IEnumerable<object> GetDomainEvents() => _domainEvents.AsReadOnly();
    }
}
