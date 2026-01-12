namespace VendorMdm.Api.Models;

// DTOs for Invitation API

public class CreateInvitationRequest
{
    public string VendorLegalName { get; set; } = string.Empty;
    public string PrimaryContactEmail { get; set; } = string.Empty;
    public string VendorType { get; set; } = "Physical"; // Physical, Company, Meeting, Participant
    public int ExpirationDays { get; set; } = 14; // Default 14 days
    public string? AccountGroup { get; set; } // Specific UNESCO Account Group
    public string? CompanyCode { get; set; } // UNESCO Entity (UNES, IIEP, etc.)
    public string? Country { get; set; } // Initial context country
    public string? Currency { get; set; } // Order currency (Internal)
    public string? SapLanguage { get; set; } // Communication language (Internal)
    public string? TaxCode1 { get; set; } // Fiscal classification
    public string? TaxCode2 { get; set; } // Secondary tax code
    public string? PermittedPayee { get; set; } // SAP ID for alternative payee
    public DateTime? DateOfBirth { get; set; } // For physical person duplicate check context
    public string? Notes { get; set; }
    public bool ForceCreation { get; set; } = false;
}

public class CreateInvitationResponse
{
    public Guid InvitationId { get; set; }
    public string InvitationToken { get; set; } = string.Empty;
    public string InvitationLink { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool EmailSent { get; set; } = true;
    public string? EmailError { get; set; }
}

public class ValidateInvitationResponse
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public string? VendorLegalName { get; set; }
    public string? PrimaryContactEmail { get; set; }
    public string? VendorType { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? CurrentStage { get; set; }
    public Dictionary<string, object> Attributes { get; set; } = new();
}

public class InvitationListItem
{
    public Guid Id { get; set; }
    public string VendorLegalName { get; set; } = string.Empty;
    public string PrimaryContactEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CurrentStage { get; set; } = string.Empty;
    public string InvitedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public Guid? VendorApplicationId { get; set; }
    public bool EmailSent { get; set; } = true;
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
    public Dictionary<string, object> Attributes { get; set; } = new();
}

public class MfaVerifyRequest
{
    public string Code { get; set; } = string.Empty;
}

public class VerifyMfaResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? CurrentStage { get; set; }
    public Dictionary<string, object> Attributes { get; set; } = new();
    
    // Include full details needed for wizard restoration
    public string? VendorLegalName { get; set; }
    public string? PrimaryContactEmail { get; set; }
    public string? VendorType { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
