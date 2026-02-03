using System;

namespace VendorMdm.Shared.Ontology.AuditModels
{
    /// <summary>
    /// Vendor-specific audit model.
    /// Defines which fields are captured in audit logs for Vendor entities.
    /// </summary>
    public class VendorAuditModel
    {
        public string SchemaVersion { get; set; } = "v1.0.0";

        // Critical Fields (MUST audit)
        public string? LegalName { get; set; }
        public string? Status { get; set; }
        public string? TaxId { get; set; }
        public string? AccountGroup { get; set; }
        public string? VendorType { get; set; }

        // Standard Fields (SHOULD audit)
        public string? PrimaryContactEmail { get; set; }
        public string? PrimaryContactName { get; set; }
        public string? Country { get; set; }
        public string? Currency { get; set; }
        public string? PaymentTerms { get; set; }

        // Metadata
        public DateTime? VerifiedAt { get; set; }
        public string? VerifiedBy { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public static VendorAuditModel FromEntity(Models.Vendor vendor)
        {
            return new VendorAuditModel
            {
                LegalName = vendor.LegalName,
                Status = vendor.Status,
                TaxId = vendor.TaxId,
                PrimaryContactEmail = vendor.PrimaryContactEmail
                // Country and Currency are in Data (JSONB), not direct properties
            };
        }
    }
}
