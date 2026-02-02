using System;

namespace VendorMdm.Shared.Contracts.Dtos
{
    public class EventDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // e.g., Published, Draft
    }
}
