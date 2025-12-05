namespace VendorMdm.Api.Models;

// DTOs for Invitation API

public class CreateInvitationRequest
{
    public string VendorLegalName { get; set; } = string.Empty;
    public string PrimaryContactEmail { get; set; } = string.Empty;
    public int ExpirationDays { get; set; } = 14; // Default 14 days
    public string? Notes { get; set; }
}

public class CreateInvitationResponse
{
    public Guid InvitationId { get; set; }
    public string InvitationToken { get; set; } = string.Empty;
    public string InvitationLink { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class ValidateInvitationResponse
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public string? VendorLegalName { get; set; }
    public string? PrimaryContactEmail { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class InvitationListItem
{
    public Guid Id { get; set; }
    public string VendorLegalName { get; set; } = string.Empty;
    public string PrimaryContactEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string InvitedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public Guid? VendorApplicationId { get; set; }
}

public class InvitationListResponse
{
    public List<InvitationListItem> Invitations { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class CompleteInvitationRequest
{
    public string CompanyName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
