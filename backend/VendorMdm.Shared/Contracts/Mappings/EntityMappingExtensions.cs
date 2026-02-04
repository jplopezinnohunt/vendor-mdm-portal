using VendorMdm.Shared.Contracts.Dtos;
using VendorMdm.Shared.Models;

namespace VendorMdm.Shared.Contracts.Mappings;

/// <summary>
/// Extension methods for mapping entities to DTOs.
/// Implements Pattern 6 from moderngoldenrules.md: "No Entity Leaks"
/// APIs MUST return DTOs, never SQL entities.
/// </summary>
public static class EntityMappingExtensions
{
    // ============================================
    // VENDOR MAPPINGS
    // ============================================

    public static VendorDto ToDto(this Vendor entity) => new()
    {
        Id = entity.Id,
        LegalName = entity.LegalName,
        TaxId = entity.TaxId ?? string.Empty,
        Status = entity.Status,
        CreatedAt = entity.CreatedAt,
        LastModifiedAt = entity.UpdatedAt
    };

    public static IEnumerable<VendorDto> ToDtoList(this IEnumerable<Vendor> entities) =>
        entities.Select(e => e.ToDto());

    // ============================================
    // EMPLOYEE MAPPINGS
    // ============================================

    public static EmployeeDto ToDto(this Employee entity) => new()
    {
        Id = entity.Id,
        FullName = $"{entity.GivenName} {entity.Surname}".Trim(),
        GivenName = entity.GivenName,
        Surname = entity.Surname,
        Email = entity.Email,
        EmployeeId = entity.EmployeeId,
        Status = entity.Status,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    public static IEnumerable<EmployeeDto> ToDtoList(this IEnumerable<Employee> entities) =>
        entities.Select(e => e.ToDto());

    // ============================================
    // PROJECT MAPPINGS
    // ============================================

    public static ProjectDto ToDto(this Project entity) => new()
    {
        Id = entity.Id,
        ProjectCode = entity.ProjectCode,
        Name = entity.Name,
        Status = entity.Status,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    public static IEnumerable<ProjectDto> ToDtoList(this IEnumerable<Project> entities) =>
        entities.Select(e => e.ToDto());

    // ============================================
    // FUND MAPPINGS
    // ============================================

    public static FundDto ToDto(this Fund entity) => new()
    {
        Id = entity.Id,
        FundCode = entity.FundCode,
        Name = entity.Name,
        FiscalYear = entity.FiscalYear,
        Status = entity.Status,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    public static IEnumerable<FundDto> ToDtoList(this IEnumerable<Fund> entities) =>
        entities.Select(e => e.ToDto());

    // ============================================
    // CUSTOMER MAPPINGS
    // ============================================

    public static CustomerDto ToDto(this Customer entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        TaxId = entity.TaxId,
        PrimaryContactEmail = entity.PrimaryContactEmail,
        Status = entity.Status,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    public static IEnumerable<CustomerDto> ToDtoList(this IEnumerable<Customer> entities) =>
        entities.Select(e => e.ToDto());

    // ============================================
    // USER MAPPINGS
    // ============================================

    public static UserDto ToDto(this User entity) => new()
    {
        Id = entity.Id,
        Username = entity.Username,
        Email = entity.Email,
        Roles = entity.Roles,
        Status = entity.Status,
        AuthProvider = entity.AuthProvider,
        AuthMethod = entity.AuthMethod,
        TwoFactorEnabled = entity.TwoFactorEnabled,
        IsBlocked = entity.IsBlocked,
        LastLogonAt = entity.LastLogonAt,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    public static UserSummaryDto ToSummaryDto(this User entity) => new()
    {
        Id = entity.Id,
        Username = entity.Username,
        Email = entity.Email,
        Roles = entity.Roles
    };

    public static IEnumerable<UserDto> ToDtoList(this IEnumerable<User> entities) =>
        entities.Select(e => e.ToDto());

    public static IEnumerable<UserSummaryDto> ToSummaryDtoList(this IEnumerable<User> entities) =>
        entities.Select(e => e.ToSummaryDto());
}
