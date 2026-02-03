using System;

namespace VendorMdm.Shared.Ontology.AuditModels
{
    /// <summary>
    /// VendorInvitation-specific audit model.
    /// Defines which fields are captured in audit logs for VendorInvitation entities.
    /// </summary>
    public class InvitationAuditModel
    {
        public string SchemaVersion { get; set; } = "v1.0.0";

        // Critical Fields (MUST audit)
        public string? Status { get; set; }
        public string? VendorLegalName { get; set; }
        public string? PrimaryContactEmail { get; set; }
        public string? VendorType { get; set; }
        public string? CurrentStage { get; set; }

        // Standard Fields (SHOULD audit)
        public DateTime? ExpiresAt { get; set; }
        public string? InvitedByName { get; set; }
        public string? AccountGroup { get; set; }

        // Metadata
        public Guid? VendorApplicationId { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool? EmailSent { get; set; }
        public string? EmailError { get; set; }

        public static InvitationAuditModel FromEntity(Models.VendorInvitation invitation)
        {
            return new InvitationAuditModel
            {
                Status = invitation.Status,
                VendorLegalName = invitation.VendorLegalName,
                PrimaryContactEmail = invitation.PrimaryContactEmail,
                VendorType = invitation.VendorType,
                CurrentStage = invitation.CurrentStage.ToString(),
                ExpiresAt = invitation.ExpiresAt,
                InvitedByName = invitation.InvitedByName,
                AccountGroup = invitation.AccountGroup,
                VendorApplicationId = invitation.VendorApplicationId,
                CompletedAt = invitation.CompletedAt
            };
        }
    }
}
