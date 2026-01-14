using VendorMdm.Shared.Models;

namespace VendorMdm.Shared.DomainEvents;

public class ApplicationApprovedEvent : EnhancedDomainEvent
{
    public ApplicationApprovedEvent(Guid applicationId, Guid vendorId, string approverId = "System")
    {
        Id = Guid.NewGuid().ToString();
        EventType = "ApplicationApproved";
        EntityId = applicationId.ToString();
        Timestamp = DateTime.UtcNow;
        Actor = approverId;
        Channel = EventChannels.Api;

        Data = new
        {
            ApplicationId = applicationId,
            VendorId = vendorId,
            ApproverId = approverId,
            ApprovedAt = Timestamp
        };
    }
}
