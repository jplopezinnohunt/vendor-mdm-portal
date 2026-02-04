namespace VendorMdm.Shared.Contracts.Dtos;

/// <summary>
/// Represents the strict API contract for an Employee.
/// Prevents leaking internal SQL Entities or JSONB structures.
/// </summary>
public class EmployeeDto
{
    public Guid Id { get; set; }

    /// <summary>Full name computed from GivenName + Surname</summary>
    public string FullName { get; set; } = string.Empty;

    public string GivenName { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>Business-facing Employee ID (from HR system)</summary>
    public string? EmployeeId { get; set; }

    public string Status { get; set; } = string.Empty;

    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateEmployeeRequestDto
{
    public string GivenName { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? EmployeeId { get; set; }
}

public class UpdateEmployeeRequestDto
{
    public Guid Id { get; set; }
    public string GivenName { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? EmployeeId { get; set; }
}
