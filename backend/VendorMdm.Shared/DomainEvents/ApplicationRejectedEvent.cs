using VendorMdm.Shared.Models;

namespace VendorMdm.Shared.DomainEvents;

public class ApplicationRejectedEvent : EnhancedDomainEvent
{
    public ApplicationRejectedEvent(Guid applicationId, string reason, string approverId = "System")
    {
        Id = Guid.NewGuid().ToString();
        EventType = "ApplicationRejected";
        EntityId = applicationId.ToString();
        Timestamp = DateTime.UtcNow;
        Actor = approverId;
        Channel = EventChannels.Api;

        Data = new
        {
            ApplicationId = applicationId,
            RejectionReason = reason,
            ApproverId = approverId,
            RejectedAt = Timestamp
        };
    }
}
