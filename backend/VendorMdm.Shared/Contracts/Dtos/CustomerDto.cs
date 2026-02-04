namespace VendorMdm.Shared.Contracts.Dtos;

/// <summary>
/// Represents the strict API contract for a Customer.
/// Prevents leaking internal SQL Entities or JSONB structures.
/// </summary>
public class CustomerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string? PrimaryContactEmail { get; set; }
    public string Status { get; set; } = string.Empty;

    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateCustomerRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string? PrimaryContactEmail { get; set; }
}

public class UpdateCustomerRequestDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string? PrimaryContactEmail { get; set; }
}
