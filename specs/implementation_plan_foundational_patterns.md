# Implementation Plan: Foundational Patterns Hardening

**Spec Reference**: [spec_foundational_patterns_hardening.md](spec_foundational_patterns_hardening.md)
**Date**: 2026-02-04
**Status**: APPROVED

---

## Implementation Order

Per moderngoldenrules.md, implementation follows priority order with consideration for:
- **Logs Management**: All soft delete operations must be audited
- **Access Controls**: Soft delete requires authorization; deleted records hidden by default
- **Queries**: Global query filters exclude soft-deleted records

---

## Phase 1: Soft Delete Infrastructure

### 1.1 Create ISoftDeletable Interface
**File**: `backend/VendorMdm.Shared/Ontology/Interfaces/ISoftDeletable.cs`

```csharp
namespace VendorMdm.Shared.Ontology.Interfaces;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    string? DeletedBy { get; set; }

    void SoftDelete(string deletedBy);
    void Restore(string restoredBy);
}
```

### 1.2 Update CanonicalEntityBase
**File**: `backend/VendorMdm.Shared/Models/CanonicalEntityBase.cs`

Add after line 89 (SchemaVersion):
- `IsDeleted` (bool, default false)
- `DeletedAt` (DateTime?, nullable)
- `DeletedBy` (string?, MaxLength 256)
- `SoftDelete()` and `Restore()` methods

### 1.3 Add Global Query Filters
**File**: `backend/VendorMdm.Api/Data/SqlDbContext.cs`

In `ConfigureCanonicalEntities()`, add `HasQueryFilter(e => !e.IsDeleted)` for:
- Vendor
- VendorInvitationCanonical
- ChangeRequestCanonical
- Employee
- Project
- Fund
- Customer
- User
- DocumentRegistry

### 1.4 Add Soft Delete Audit Action
**File**: `backend/VendorMdm.Shared/Constants/AuditActions.cs` (NEW)

Define standard audit actions including "SoftDeleted" and "Restored".

### 1.5 Database Migration
**File**: `backend/VendorMdm.Api/Migrations/[timestamp]_AddSoftDeleteFields.cs`

---

## Phase 2: Status Constants

### 2.1 Create Missing Status Classes
**Location**: `backend/VendorMdm.Shared/Constants/`

| File | Contents |
|------|----------|
| `ApplicationStatus.cs` | Draft, Submitted, PendingReview, UnderReview, Approved, Rejected, Completed |
| `UserStatus.cs` | Pending, Active, Suspended, Archived |
| `DocumentStatus.cs` | Pending, Uploaded, Verified, Rejected, Archived |
| `GdprStatus.cs` | Deleted, ProcessingRestricted, ObjectionPending, Anonymized |
| `InvitationStatus.cs` | Pending, Accepted, Expired, Completed, Cancelled |
| `VendorTypes.cs` | Individual, Organization, StartUp, Company, Participant |

### 2.2 Update Controllers with Constants
Replace hardcoded strings in:
- `InvitationController.cs` (lines 300, 324, 438)
- `AuthController.cs` (lines 54, 129)
- `GdprController.cs` (lines 97, 148, 174)
- `ChangeRequestController.cs` (line 30)

---

## Phase 3: DTO Pattern Enforcement

### 3.1 Create Missing DTOs
**Location**: `backend/VendorMdm.Shared/Contracts/Dtos/`

| File | Fields |
|------|--------|
| `EmployeeDto.cs` | Id, FullName (computed), Email, EmployeeId, Status |
| `ProjectDto.cs` | Id, ProjectCode, Name, Status, StartDate, EndDate |
| `FundDto.cs` | Id, FundCode, Name, FiscalYear, Status |
| `UserDto.cs` | Id, Username, Email, Roles, Status, LastLogonAt |
| `CustomerDto.cs` | Id, Name, TaxId, PrimaryContactEmail, Status |

### 3.2 Create Mapping Extensions
**File**: `backend/VendorMdm.Shared/Contracts/Mappings/EntityMappingExtensions.cs`

Static extension methods:
- `ToDto()` for each entity type
- `ToDtoList()` for collections

### 3.3 Update Controllers
Fix all 23 entity-leak violations:
- `VendorController.cs` - 5 methods
- `UserController.cs` - 6 methods
- `EmployeeController.cs` - 4 methods
- `ProjectController.cs` - 3 methods
- `FundController.cs` - 3 methods
- `EventController.cs` - 4 methods
- `ChangeRequestController.cs` - 2 methods

---

## Phase 4: Evolution Infrastructure

### 4.1 API Version Header Middleware
**File**: `backend/VendorMdm.Api/Middleware/ApiVersionHeaderMiddleware.cs`

Adds `X-API-Version: 1.0` header to all responses.

### 4.2 Register Middleware
**File**: `backend/VendorMdm.Api/Program.cs`

Add middleware registration in pipeline.

---

## Logging & Access Control Considerations

### Structured Logging for Soft Delete

```csharp
// When soft-deleting
_logger.LogInformation("Entity soft deleted", new
{
    entityType = entity.GetType().Name,
    entityId = entity.Id,
    deletedBy = deletedBy,
    deletedAt = entity.DeletedAt,
    reason = reason
});

// When restoring
_logger.LogInformation("Entity restored from soft delete", new
{
    entityType = entity.GetType().Name,
    entityId = entity.Id,
    restoredBy = restoredBy,
    originalDeletedAt = previousDeletedAt
});
```

### Audit Trail for Soft Delete

New audit actions in AuditLog:
- `SoftDeleted` - Record old state, no new state
- `Restored` - Record old state (deleted), new state (restored)

### Access Control Rules

| Operation | Required Role | Query Filter Behavior |
|-----------|--------------|----------------------|
| View (default) | Viewer+ | Excludes IsDeleted=true |
| View (include deleted) | Admin | Explicit `.IgnoreQueryFilters()` |
| Soft Delete | Admin | Must have authorization |
| Restore | Admin | Must have authorization |
| Hard Delete | FORBIDDEN | Never allowed without explicit approval |

### Query Patterns

```csharp
// Default query (soft-deleted excluded automatically)
var vendors = await _context.Vendors.ToListAsync();

// Admin query (include soft-deleted)
var allVendors = await _context.Vendors
    .IgnoreQueryFilters()
    .ToListAsync();

// Only soft-deleted records
var deletedVendors = await _context.Vendors
    .IgnoreQueryFilters()
    .Where(v => v.IsDeleted)
    .ToListAsync();
```

---

## File Summary

### New Files (16)
1. `Shared/Ontology/Interfaces/ISoftDeletable.cs`
2. `Shared/Constants/ApplicationStatus.cs`
3. `Shared/Constants/UserStatus.cs`
4. `Shared/Constants/DocumentStatus.cs`
5. `Shared/Constants/GdprStatus.cs`
6. `Shared/Constants/InvitationStatus.cs`
7. `Shared/Constants/VendorTypes.cs`
8. `Shared/Constants/AuditActions.cs`
9. `Shared/Contracts/Dtos/EmployeeDto.cs`
10. `Shared/Contracts/Dtos/ProjectDto.cs`
11. `Shared/Contracts/Dtos/FundDto.cs`
12. `Shared/Contracts/Dtos/UserDto.cs`
13. `Shared/Contracts/Dtos/CustomerDto.cs`
14. `Shared/Contracts/Mappings/EntityMappingExtensions.cs`
15. `Api/Middleware/ApiVersionHeaderMiddleware.cs`
16. `Api/Migrations/[timestamp]_AddSoftDeleteFields.cs`

### Modified Files (12)
1. `Shared/Models/CanonicalEntityBase.cs`
2. `Api/Data/SqlDbContext.cs`
3. `Api/Controllers/VendorController.cs`
4. `Api/Controllers/UserController.cs`
5. `Api/Controllers/EmployeeController.cs`
6. `Api/Controllers/ProjectController.cs`
7. `Api/Controllers/FundController.cs`
8. `Api/Controllers/EventController.cs`
9. `Api/Controllers/ChangeRequestController.cs`
10. `Api/Controllers/InvitationController.cs`
11. `Api/Controllers/AuthController.cs`
12. `Api/Controllers/GdprController.cs`

---

## Verification Checklist

See `scripts/verification/verify_foundational_patterns.sh`
