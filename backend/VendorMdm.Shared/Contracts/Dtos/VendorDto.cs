using System;
using System.Collections.Generic;

namespace VendorMdm.Shared.Contracts.Dtos
{
    /// <summary>
    /// Represents the strict API contract for a Vendor.
    /// Prevents leaking internal SQL Entities or JSONB structures.
    /// </summary>
    public class VendorDto
    {
        public Guid Id { get; set; }
        public string LegalName { get; set; } = string.Empty;
        public string TaxId { get; set; } = string.Empty;
        public string VendorType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        
        // Flattened or structured attributes (No raw JSON strings!)
        public string? Region { get; set; }
        public string? AccountGroup { get; set; }
        
        // Metadata
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }
    }

    public class CreateVendorRequestDto
    {
        public string LegalName { get; set; } = string.Empty;
        public string TaxId { get; set; } = string.Empty;
        public string VendorType { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
    }
}
