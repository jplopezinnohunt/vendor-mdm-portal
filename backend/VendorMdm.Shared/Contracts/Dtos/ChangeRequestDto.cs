using System;

namespace VendorMdm.Shared.Contracts.Dtos
{
    public class ChangeRequestDto
    {
        public Guid Id { get; set; }
        public Guid VendorId { get; set; }
        public string RequestType { get; set; } = string.Empty; // e.g., BankUpdate, AddressChange
        public string Status { get; set; } = string.Empty; // Pending, Approved, Rejected
        public string ChangedDataJson { get; set; } = string.Empty; // Serialized diff
    }
}
