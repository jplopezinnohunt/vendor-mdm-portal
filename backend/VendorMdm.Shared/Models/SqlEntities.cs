using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendorMdm.Shared.Models;

public class ChangeRequest
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Draft"; // Draft, Submitted, Approved, Integrated

    public string? SapVendorId { get; set; } // Null for new vendors

    [Required]
    public Guid RequesterId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class VendorApplication
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string ContactEmail { get; set; } = string.Empty;

    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class WorkflowState
{
    [Key]
    [MaxLength(20)]
    public string StateName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}

public class SapEnvironment
{
    [Key]
    [MaxLength(3)]
    public string EnvironmentCode { get; set; } = string.Empty; // D01, Q01, P01

    public string Description { get; set; } = string.Empty;
}

public class UserRole
{
    [Key]
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = "User"; // Admin, Requester, Approver
}

public class Attachment
{
    [Key]
    public Guid Id { get; set; }
    public Guid LinkedEntityId { get; set; } // ChangeRequest or VendorApplication
    public string FileName { get; set; } = string.Empty;
    public string BlobUrl { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
