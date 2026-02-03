using System;
using System.Collections.Generic;
using System.Linq;
using VendorMdm.Core.Framework.Ontology;
using VendorMdm.Core.Framework.Primitives;
using VendorMdm.Shared.Models;
using VendorMdm.Shared.Ontology.Interfaces;

namespace VendorMdm.Shared.Ontology.Concepts
{
    public class EventConcept : IOntologyConcept, IAuditableEntity
    {
        private readonly Guid _id;
        private readonly string _originContext;
        private readonly List<object> _domainEvents = new();
        
        private string _title;
        private string _eventCode;
        private string _eventType;
        private DateTime _startDate;
        private DateTime _endDate;
        private string _status;
        private string _createdBy;

        public Guid Id => _id;
        public string OriginContext => _originContext;
        public string Title => _title;
        public string EventCode => _eventCode;
        public string EventType => _eventType;
        public string Status => _status;

        public EventConcept(string title, string eventCode, string eventType, DateTime start, DateTime end, string createdBy, string origin)
        {
            _id = Guid.NewGuid();
            _title = title;
            _eventCode = eventCode;
            _eventType = eventType;
            _startDate = start;
            _endDate = end;
            _createdBy = createdBy;
            _originContext = origin;
            _status = "Draft";
        }

        public Result ValidateState()
        {
            if (string.IsNullOrWhiteSpace(_title))
                return Result.Fail("Event Title is required.");
                
            if (string.IsNullOrWhiteSpace(_eventCode))
                return Result.Fail("Event Code is required.");
                
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
                { "EventType", _eventType },
                { "Duration_Days", (_endDate - _startDate).TotalDays }
            };
        }

        public IEnumerable<object> GetDomainEvents() => _domainEvents.AsReadOnly();

        #region IAuditableEntity Implementation

        public string GetEntityType() => "Event";

        public AuditableFields GetAuditableFields()
        {
            return new AuditableFields
            {
                // MUST audit these fields (critical for compliance)
                CriticalFields = new List<string>
                {
                    "Title",
                    "EventCode",
                    "EventType",
                    "StartDate",
                    "EndDate",
                    "Status"
                },

                // SHOULD audit these fields (important but not critical)
                StandardFields = new List<string>
                {
                    "CreatedBy",
                    "Location",
                    "Description",
                    "MaxParticipants",
                    "IsPublished"
                },

                // MUST NOT audit these fields (sensitive data)
                SensitiveFields = new List<string>
                {
                    "ParticipantPersonalData",
                    "PrivateNotes",
                    "InternalComments"
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
                    { "EventType", _eventType },
                    { "EventCode", _eventCode },
                    { "OriginContext", _originContext },
                    { "CurrentStatus", _status },
                    { "CreatedBy", _createdBy }
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
                "Published", "Cancelled", "Completed",
                "ParticipantAdded", "ParticipantRemoved",
                "StatusChanged"
            };

            return auditableActions.Contains(action);
        }

        private object? FilterAuditableValues(object? state, AuditableFields auditableFields)
        {
            if (state == null) return null;

            // If state is an Event entity, extract only auditable fields
            if (state is Event evt)
            {
                var filtered = new Dictionary<string, object?>();

                // Add critical fields
                if (auditableFields.CriticalFields.Contains("Title"))
                    filtered["Title"] = evt.Title;
                if (auditableFields.CriticalFields.Contains("EventCode"))
                    filtered["EventCode"] = evt.EventCode;
                if (auditableFields.CriticalFields.Contains("EventType"))
                    filtered["EventType"] = evt.EventType;
                if (auditableFields.CriticalFields.Contains("StartDate"))
                    filtered["StartDate"] = evt.StartDate;
                if (auditableFields.CriticalFields.Contains("EndDate"))
                    filtered["EndDate"] = evt.EndDate;
                if (auditableFields.CriticalFields.Contains("Status"))
                    filtered["Status"] = _status;

                // Add standard fields
                if (auditableFields.StandardFields.Contains("CreatedBy"))
                    filtered["CreatedBy"] = evt.CreatedBy;

                return filtered;
            }

            // If state is already a dictionary or anonymous object, return as-is
            return state;
        }

        #endregion
    }
}
