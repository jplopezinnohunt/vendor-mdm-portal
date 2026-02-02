using System;

namespace VendorMdm.Core.Framework.Events
{
    /// <summary>
    /// Base class for all domain events.
    /// </summary>
    public abstract class DomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
        public string EventType => GetType().Name;
    }

    /// <summary>
    /// Raised when a Vendor is created.
    /// </summary>
    public class VendorCreatedEvent : DomainEvent
    {
        public Guid VendorId { get; }
        public string LegalName { get; }
        public string AccountGroup { get; }

        public VendorCreatedEvent(Guid vendorId, string legalName, string accountGroup)
        {
            VendorId = vendorId;
            LegalName = legalName;
            AccountGroup = accountGroup;
        }
    }

    /// <summary>
    /// Raised when a Vendor status changes.
    /// </summary>
    public class VendorStatusChangedEvent : DomainEvent
    {
        public Guid VendorId { get; }
        public string OldStatus { get; }
        public string NewStatus { get; }

        public VendorStatusChangedEvent(Guid vendorId, string oldStatus, string newStatus)
        {
            VendorId = vendorId;
            OldStatus = oldStatus;
            NewStatus = newStatus;
        }
    }
}
