using System;

namespace VendorMdm.Shared.Ontology.AuditModels
{
    /// <summary>
    /// Event-specific audit model.
    /// Defines which fields are captured in audit logs for Event entities.
    /// </summary>
    public class EventAuditModel
    {
        public string SchemaVersion { get; set; } = "v1.0.0";

        // Critical Fields (MUST audit)
        public string? Title { get; set; }
        public string? EventCode { get; set; }
        public string? EventType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }

        // Standard Fields (SHOULD audit)
        public string? CreatedBy { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public int? MaxParticipants { get; set; }
        public bool? IsPublished { get; set; }

        // Metadata
        public int? ParticipantCount { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string? PublishedBy { get; set; }

        public static EventAuditModel FromEntity(Models.Event evt)
        {
            return new EventAuditModel
            {
                Title = evt.Title,
                EventCode = evt.EventCode,
                EventType = evt.EventType,
                StartDate = evt.StartDate,
                EndDate = evt.EndDate,
                CreatedBy = evt.CreatedBy
                // Location and Description are in Data (JSONB), not direct properties
            };
        }
    }
}
