# Specification: Foundational Patterns Hardening

**Spec ID**: SPEC-2026-02-04-001
**Version**: 1.0.0
**Date**: 2026-02-04
**Author**: Agent
**Status**: DRAFT - Awaiting Approval

---

## 1. Executive Summary

This specification addresses critical gaps in the 18 Foundational Patterns defined in [moderngoldenrules.md](.agent/rules/moderngoldenrules.md) Section 10. The system will evolve with new entities and metadata; these changes ensure safe evolution without breaking clients.

### Compliance Sidebar
| Standard | Section | Compliance |
|----------|---------|------------|
| moderngoldenrules.md | Section 10.3 Pattern 11 | Soft Delete |
| moderngoldenrules.md | Section 6 Rule 4 | No Entity Leaks (DTOs) |
| moderngoldenrules.md | Section 10.2 Pattern 7 | State Machines (Constants) |
| data-model-standards.md | Hybrid Model | Schema Evolution |

---

## 2. Problem Statement

### 2.1 Current Gaps Identified

| Gap | Severity | Impact |
|-----|----------|--------|
| **No Soft Delete** | CRITICAL | GDPR non-compliance, data loss risk |
| **Entity Leaks (23 methods)** | CRITICAL | API contract breakage on entity changes |
| **Hardcoded Status Strings (10+)** | HIGH | Scattered business rules, maintenance burden |
| **Missing Status Constants (6 types)** | HIGH | Inconsistent state machine enforcement |
| **No API Versioning Prep** | MEDIUM | Future breaking changes |

### 2.2 Affected Components

**Entities Requiring Soft Delete (9)**:
- Vendor, VendorInvitationCanonical, ChangeRequestCanonical
- Employee, Project, Fund, Customer, User
- DocumentRegistry

**Controllers Returning Entities (7)**:
- VendorController, UserController, EventController
- EmployeeController, ProjectController, FundController
- ChangeRequestController

**Missing DTO Types (5)**:
- EmployeeDto, ProjectDto, FundDto, UserDto, CustomerDto

---

## 3. Proposed Solution

### 3.1 Phase 1: Soft Delete Infrastructure

#### 3.1.1 Add ISoftDeletable Interface
```csharp
// VendorMdm.Shared/Ontology/Interfaces/ISoftDeletable.cs
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    string? DeletedBy { get; set; }
}
```

#### 3.1.2 Update CanonicalEntityBase
```csharp
// Add to CanonicalEntityBase.cs
public bool IsDeleted { get; set; } = false;
public DateTime? DeletedAt { get; set; }
public string? DeletedBy { get; set; }
```

#### 3.1.3 Add Global Query Filter
```csharp
// SqlDbContext.cs - OnModelCreating
modelBuilder.Entity<Vendor>().HasQueryFilter(e => !e.IsDeleted);
modelBuilder.Entity<Employee>().HasQueryFilter(e => !e.IsDeleted);
// ... for all 9 entity types
```

#### 3.1.4 Add SoftDelete Extension Method
```csharp
// VendorMdm.Core.Framework/Extensions/SoftDeleteExtensions.cs
public static void SoftDelete<T>(this T entity, string deletedBy) where T : ISoftDeletable
{
    entity.IsDeleted = true;
    entity.DeletedAt = DateTime.UtcNow;
    entity.DeletedBy = deletedBy;
}
```

---

### 3.2 Phase 2: DTO Pattern Enforcement

#### 3.2.1 Create Missing DTOs

**Location**: `VendorMdm.Shared/Contracts/`

| DTO | Fields |
|-----|--------|
| EmployeeDto | Id, FullName, Email, Department, Status |
| ProjectDto | Id, Name, Description, Status, StartDate, EndDate |
| FundDto | Id, Name, Code, Amount, Currency, Status |
| UserDto | Id, Email, DisplayName, Roles, Status, LastLoginAt |
| CustomerDto | Id, Name, Type, Status, ContactEmail |

#### 3.2.2 Create Mapping Extensions

**Location**: `VendorMdm.Shared/Contracts/Mappings/`

```csharp
public static class EntityMappingExtensions
{
    public static VendorDto ToDto(this Vendor entity) => new()
    {
        Id = entity.Id,
        LegalName = entity.LegalName,
        // ... only public fields
    };

    public static EmployeeDto ToDto(this Employee entity) => // ...
    public static ProjectDto ToDto(this Project entity) => // ...
    // ... for all entity types
}
```

#### 3.2.3 Update Controllers

Fix all 23 controller methods to return DTOs:

| Controller | Before | After |
|------------|--------|-------|
| VendorController.GetVendor | `ActionResult<Vendor>` | `ActionResult<VendorDto>` |
| UserController.GetUser | `ActionResult<User>` | `ActionResult<UserDto>` |
| ... | ... | ... |

---

### 3.3 Phase 3: Status Constants Centralization

#### 3.3.1 Create Missing Status Classes

**Location**: `VendorMdm.Shared/Constants/`

```csharp
// ApplicationStatus.cs
public static class ApplicationStatus
{
    public const string Draft = "Draft";
    public const string Submitted = "Submitted";
    public const string PendingReview = "PendingReview";
    public const string UnderReview = "UnderReview";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Completed = "Completed";

    public static readonly IReadOnlyList<string> All = new[] { ... };
    public static bool IsValid(string status) => All.Contains(status);
}

// UserStatus.cs
public static class UserStatus
{
    public const string Pending = "Pending";
    public const string Active = "Active";
    public const string Suspended = "Suspended";
    public const string Archived = "Archived";
}

// DocumentStatus.cs
public static class DocumentStatus
{
    public const string Draft = "Draft";
    public const string Uploaded = "Uploaded";
    public const string Verified = "Verified";
    public const string Rejected = "Rejected";
    public const string Archived = "Archived";
}

// GdprStatus.cs
public static class GdprStatus
{
    public const string Deleted = "Deleted";
    public const string ProcessingRestricted = "ProcessingRestricted";
    public const string ObjectionPending = "ObjectionPending";
    public const string Anonymized = "Anonymized";
}
```

#### 3.3.2 Create VendorTypes Constants

```csharp
// VendorTypes.cs
public static class VendorTypes
{
    public const string Individual = "Individual";
    public const string Organization = "Organization";
    public const string StartUp = "StartUp";
    public const string Company = "Company";
    public const string Participant = "Participant";

    public static bool IsValid(string type) => All.Contains(type);
}
```

#### 3.3.3 Replace Hardcoded Strings

Update all 10+ locations with magic strings to use constants:

| File | Line | Before | After |
|------|------|--------|-------|
| InvitationController.cs | 300 | `"Submitted"` | `ApplicationStatus.Submitted` |
| InvitationController.cs | 324 | `"PendingReview"` | `ApplicationStatus.PendingReview` |
| AuthController.cs | 129 | `"Active"` | `UserStatus.Active` |
| GdprController.cs | 97 | `"Deleted"` | `GdprStatus.Deleted` |
| ... | ... | ... | ... |

---

### 3.4 Phase 4: Evolution Infrastructure (Foundation Only)

#### 3.4.1 Add API Version Header Middleware (Preparation)

```csharp
// Middleware/ApiVersionHeaderMiddleware.cs
public class ApiVersionHeaderMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        context.Response.Headers["X-API-Version"] = "1.0";
        context.Response.Headers["X-API-Deprecation"] = ""; // Empty = not deprecated
        await next(context);
    }
}
```

#### 3.4.2 Add IEntityFactory Interface (Preparation)

```csharp
// VendorMdm.Core.Framework/Factories/IEntityFactory.cs
public interface IEntityFactory<TEntity> where TEntity : CanonicalEntityBase
{
    TEntity Create();
    TEntity Create(string sourceSystem);
    Result<TEntity> CreateWithValidation(Action<TEntity> configure);
}
```

---

## 4. Database Migration

### 4.1 Migration: Add Soft Delete Fields

```csharp
// Migration name: AddSoftDeleteFields
migrationBuilder.AddColumn<bool>("IsDeleted", "Vendors", defaultValue: false);
migrationBuilder.AddColumn<DateTime?>("DeletedAt", "Vendors", nullable: true);
migrationBuilder.AddColumn<string>("DeletedBy", "Vendors", maxLength: 256, nullable: true);

// Repeat for all 9 entity tables
// Add index for IsDeleted (partial index for false values)
migrationBuilder.CreateIndex("IX_Vendors_IsDeleted", "Vendors", "IsDeleted")
    .HasFilter("[IsDeleted] = 0");
```

### 4.2 Migration Size Estimate

- 9 tables × 3 columns = 27 ALTER statements
- Estimated size: ~15KB (well under 50KB limit)

---

## 5. Files to Create/Modify

### 5.1 New Files (12)

| Path | Purpose |
|------|---------|
| `Shared/Ontology/Interfaces/ISoftDeletable.cs` | Soft delete interface |
| `Shared/Constants/ApplicationStatus.cs` | Application workflow states |
| `Shared/Constants/UserStatus.cs` | User lifecycle states |
| `Shared/Constants/DocumentStatus.cs` | Document lifecycle states |
| `Shared/Constants/GdprStatus.cs` | GDPR request states |
| `Shared/Constants/VendorTypes.cs` | Vendor type classification |
| `Shared/Contracts/EmployeeDto.cs` | Employee DTO |
| `Shared/Contracts/ProjectDto.cs` | Project DTO |
| `Shared/Contracts/FundDto.cs` | Fund DTO |
| `Shared/Contracts/UserDto.cs` | User DTO |
| `Shared/Contracts/CustomerDto.cs` | Customer DTO |
| `Shared/Contracts/Mappings/EntityMappingExtensions.cs` | Entity-to-DTO mappers |

### 5.2 Modified Files (15)

| Path | Changes |
|------|---------|
| `Shared/Models/CanonicalEntityBase.cs` | Add ISoftDeletable fields |
| `Api/Data/SqlDbContext.cs` | Add query filters |
| `Api/Controllers/VendorController.cs` | Return DTOs |
| `Api/Controllers/UserController.cs` | Return DTOs |
| `Api/Controllers/EmployeeController.cs` | Return DTOs |
| `Api/Controllers/ProjectController.cs` | Return DTOs |
| `Api/Controllers/FundController.cs` | Return DTOs |
| `Api/Controllers/EventController.cs` | Return DTOs |
| `Api/Controllers/ChangeRequestController.cs` | Return DTOs |
| `Api/Controllers/InvitationController.cs` | Use status constants |
| `Api/Controllers/AuthController.cs` | Use status constants |
| `Api/Controllers/GdprController.cs` | Use status constants |
| `Core.Framework/Extensions/SoftDeleteExtensions.cs` | Soft delete helper |
| `Api/Migrations/YYYYMMDD_AddSoftDeleteFields.cs` | DB migration |
| `.claude/08-implementation-specs.md` | Update status |

---

## 6. Verification Criteria

### 6.1 Build Verification
- [ ] `dotnet build --configuration Release` passes with 0 errors
- [ ] `npm run build` passes with 0 errors
- [ ] Migration size < 50KB

### 6.2 Pattern Compliance Verification
- [ ] All 9 entity types implement ISoftDeletable
- [ ] All DbContext entities have HasQueryFilter for IsDeleted
- [ ] 0 controllers return raw entities (all return DTOs)
- [ ] 0 hardcoded status strings in controllers

### 6.3 Regression Verification
- [ ] Existing API responses unchanged (DTO fields match previous entity fields)
- [ ] No data loss in existing records
- [ ] Soft delete queries exclude deleted records by default

---

## 7. Rollback Plan

If issues arise:
1. Remove query filters from DbContext (immediate)
2. Revert controller return types (if DTO mapping fails)
3. Keep IsDeleted fields in DB (no data loss, can be ignored)

---

## 8. Success Metrics

| Metric | Before | After |
|--------|--------|-------|
| Foundational Patterns Implemented | 17/18 | 18/18 |
| Entity Leak Violations | 23 | 0 |
| Hardcoded Status Strings | 10+ | 0 |
| Soft Delete Coverage | 0% | 100% |

---

## 9. Approval

**Awaiting User Approval**

- [ ] Spec reviewed and approved
- [ ] Ready for Phase 2 (Implementation Plan)

---

**End of Specification**
