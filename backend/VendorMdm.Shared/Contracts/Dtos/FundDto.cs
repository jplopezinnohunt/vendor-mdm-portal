namespace VendorMdm.Shared.Contracts.Dtos;

/// <summary>
/// Represents the strict API contract for a Fund.
/// Prevents leaking internal SQL Entities or JSONB structures.
/// </summary>
public class FundDto
{
    public Guid Id { get; set; }
    public string FundCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? FiscalYear { get; set; }
    public string Status { get; set; } = string.Empty;

    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateFundRequestDto
{
    public string FundCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? FiscalYear { get; set; }
}

public class UpdateFundRequestDto
{
    public Guid Id { get; set; }
    public string FundCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? FiscalYear { get; set; }
}
