# Audit Log Integration (Pattern 16)

---

## 6.1 Ontology-Driven Auditing

**Rule**: Each entity MUST have an Audit Model defined in its Ontology Concept

### Relationship
```
Entity → Ontology Concept → Audit Model → Audit Log
```

---

## 6.2 IAuditableEntity Interface

```csharp
public interface IAuditableEntity
{
    string GetEntityType();
    AuditableFields GetAuditableFields();
    AuditLogEntry CreateAuditEntry(string action, object? oldState, object? newState, string? reason);
    bool ShouldAudit(string action);
}

public class AuditableFields
{
    // MUST audit (e.g., LegalName, Status, TaxId)
    public List<string> CriticalFields { get; set; } = new();

    // SHOULD audit (e.g., Email, Phone)
    public List<string> StandardFields { get; set; } = new();

    // MUST NOT audit (e.g., Password, BankAccount)
    public List<string> SensitiveFields { get; set; } = new();
}
```

---

## 6.3 Audit Log Schema

```sql
CREATE TABLE AuditLogs (
    Id uniqueidentifier PRIMARY KEY,
    EntityType nvarchar(100) NOT NULL,      -- "Vendor", "VendorInvitation"
    EntityId uniqueidentifier NOT NULL,
    Action nvarchar(50) NOT NULL,           -- "Created", "Updated", "Deleted"
    ChangedBy nvarchar(256) NOT NULL,       -- User email
    ChangedByUserId uniqueidentifier NOT NULL,
    ChangedAt datetime2 NOT NULL,
    OldValues nvarchar(max) NULL,           -- JSON snapshot
    NewValues nvarchar(max) NULL,           -- JSON snapshot
    Reason nvarchar(500) NULL,              -- User-provided justification
    IpAddress nvarchar(45) NULL,
    UserAgent nvarchar(500) NULL,
    TenantId uniqueidentifier NULL
);

-- Indexes
CREATE INDEX IX_AuditLogs_EntityType_EntityId ON AuditLogs(EntityType, EntityId);
CREATE INDEX IX_AuditLogs_ChangedAt ON AuditLogs(ChangedAt);
CREATE INDEX IX_AuditLogs_ChangedByUserId ON AuditLogs(ChangedByUserId);
```

---

## 6.4 Entity-Specific Audit Models

### VendorAuditModel
```csharp
public class VendorAuditModel
{
    // Critical Fields (MUST audit)
    public string? LegalName { get; set; }
    public string? Status { get; set; }
    public string? TaxId { get; set; }
    public string? AccountGroup { get; set; }

    // Standard Fields (SHOULD audit)
    public string? PrimaryContactEmail { get; set; }
    public string? BusinessType { get; set; }
    public string? Country { get; set; }

    // Sensitive fields are EXCLUDED
}
```

### VendorInvitationAuditModel
```csharp
public class VendorInvitationAuditModel
{
    public string? Status { get; set; }
    public string? VendorLegalName { get; set; }
    public string? PrimaryContactEmail { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? InvitedByName { get; set; }
    public string? CurrentStage { get; set; }
}
```

---

## 6.5 Service Implementation

### IAuditLogService Interface
```csharp
public interface IAuditLogService
{
    Task LogAsync(
        string entityType,
        Guid entityId,
        string action,
        object? oldValues = null,
        object? newValues = null,
        string? reason = null);

    Task<List<AuditLog>> GetEntityLogsAsync(string entityType, Guid entityId);
    Task<List<AuditLog>> GetUserLogsAsync(Guid userId, DateTime? from, DateTime? to);
}
```

### Usage in Services
```csharp
public class VendorService
{
    private readonly IAuditLogService _auditLog;

    public async Task<Vendor> CreateVendorAsync(CreateVendorRequest request)
    {
        // Create vendor
        var vendor = new Vendor { ... };
        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync();

        // Log the action
        await _auditLog.LogAsync(
            entityType: "Vendor",
            entityId: vendor.Id,
            action: "Created",
            newValues: new
            {
                LegalName = vendor.LegalName,
                Status = vendor.Status,
                VendorType = vendor.VendorType
            },
            reason: "New vendor registration");

        return vendor;
    }

    public async Task<Vendor> UpdateVendorAsync(Guid id, UpdateVendorRequest request)
    {
        var vendor = await _context.Vendors.FindAsync(id);
        var oldValues = new { vendor.LegalName, vendor.Status };

        // Update vendor
        vendor.LegalName = request.LegalName;
        await _context.SaveChangesAsync();

        // Log with old and new values
        await _auditLog.LogAsync(
            entityType: "Vendor",
            entityId: vendor.Id,
            action: "Updated",
            oldValues: oldValues,
            newValues: new { vendor.LegalName, vendor.Status },
            reason: request.Reason);

        return vendor;
    }
}
```

---

## 6.6 Auditable Actions

| Entity | Actions to Audit |
|--------|-----------------|
| Vendor | Created, Updated, Deleted, Approved, Rejected, Suspended |
| VendorInvitation | Created, Resent, Completed, Expired, Cancelled |
| Event | Created, Updated, Published, Cancelled |
| User | Created, RoleChanged, Deactivated |

---

## 6.7 API Endpoints

```csharp
[ApiController]
[Route("api/auditlog")]
[Authorize]
public class AuditLogController : ControllerBase
{
    // GET /api/auditlog/{entityType}/{entityId}
    [HttpGet("{entityType}/{entityId}")]
    public async Task<ActionResult<List<AuditLog>>> GetEntityLogs(
        string entityType, Guid entityId);

    // GET /api/auditlog/user/{userId} (Admin only)
    [HttpGet("user/{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<AuditLog>>> GetUserLogs(
        Guid userId, DateTime? from, DateTime? to);
}
```

---

## 6.8 Schema Evolution

### Version Tracking
```csharp
public class VendorAuditModel
{
    public string SchemaVersion { get; set; } = "v1.0.0";
    // ... fields
}
```

### Migration Strategy
When entity schema changes:
1. Update Ontology Concept's `GetAuditableFields()`
2. Update Entity-Specific Audit Model
3. Increment `SchemaVersion`
4. Create migration guide in `/docs/audit-migrations/`

---

## 6.9 Implementation Checklist

For each entity:
- [ ] Create Ontology Concept implementing `IAuditableEntity`
- [ ] Define `GetAuditableFields()` (critical, standard, sensitive)
- [ ] Implement `CreateAuditEntry()`
- [ ] Implement `ShouldAudit()`
- [ ] Create Entity-Specific Audit Model
- [ ] Update Service to use Ontology-driven auditing
- [ ] Create unit tests
