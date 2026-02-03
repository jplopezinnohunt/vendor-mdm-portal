# Ontology-Driven Audit Log Standard

**Version**: 2.0.0  
**Status**: MANDATORY  
**Pattern**: Pattern 16 - Audit Trail & Temporal + Ontology Integration  
**Effective Date**: 2026-02-03

---

## 1. Core Principle: Ontology-Driven Auditing

**CRITICAL RULE**: Each entity MUST have an **Audit Model** defined in its Ontology Concept that specifies:

1. **What to audit** (which fields are audit-worthy)
2. **How to audit** (what actions trigger audits)
3. **What to capture** (which values to log)

**Relationship**: `Entity → Ontology Concept → Audit Model → Audit Log`

---

## 2. Ontology Concept Requirements

### 2.1 IAuditableEntity Interface

All auditable entities MUST implement this interface in their Ontology Concept:

```csharp
namespace VendorMdm.Shared.Ontology.Interfaces;

public interface IAuditableEntity
{
    /// <summary>
    /// Get the entity type name for audit logs
    /// </summary>
    string GetEntityType();

    /// <summary>
    /// Get the fields that should be audited
    /// </summary>
    AuditableFields GetAuditableFields();

    /// <summary>
    /// Get the audit log entry for a specific action
    /// </summary>
    AuditLogEntry CreateAuditEntry(
        string action, 
        object? oldState = null, 
        object? newState = null, 
        string? reason = null);

    /// <summary>
    /// Determine if an action should be audited
    /// </summary>
    bool ShouldAudit(string action);
}

public class AuditableFields
{
    /// <summary>
    /// Fields that MUST be audited (e.g., LegalName, Status)
    /// </summary>
    public List<string> CriticalFields { get; set; } = new();

    /// <summary>
    /// Fields that SHOULD be audited (e.g., Email, Phone)
    /// </summary>
    public List<string> StandardFields { get; set; } = new();

    /// <summary>
    /// Fields that MUST NOT be audited (e.g., Password, SSN)
    /// </summary>
    public List<string> SensitiveFields { get; set; } = new();
}

public class AuditLogEntry
{
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public object? OldValues { get; set; }
    public object? NewValues { get; set; }
    public string? Reason { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
```

### 2.2 Example: VendorConcept with Audit Model

```csharp
namespace VendorMdm.Shared.Ontology.Concepts;

public class VendorConcept : IOntologyConcept, IAuditableEntity
{
    // ... existing properties ...

    #region IAuditableEntity Implementation

    public string GetEntityType() => "Vendor";

    public AuditableFields GetAuditableFields()
    {
        return new AuditableFields
        {
            // MUST audit these fields
            CriticalFields = new List<string>
            {
                "LegalName",
                "Status",
                "TaxId",
                "AccountGroup",
                "VerificationStatus"
            },

            // SHOULD audit these fields
            StandardFields = new List<string>
            {
                "PrimaryContactEmail",
                "PrimaryContactName",
                "BusinessType",
                "Country",
                "Currency"
            },

            // MUST NOT audit these fields
            SensitiveFields = new List<string>
            {
                "BankAccountNumber",
                "TaxDocuments",
                "FinancialStatements"
            }
        };
    }

    public AuditLogEntry CreateAuditEntry(
        string action,
        object? oldState = null,
        object? newState = null,
        string? reason = null)
    {
        var auditableFields = GetAuditableFields();
        
        return new AuditLogEntry
        {
            EntityType = GetEntityType(),
            EntityId = this.Id,
            Action = action,
            OldValues = FilterAuditableValues(oldState, auditableFields),
            NewValues = FilterAuditableValues(newState, auditableFields),
            Reason = reason,
            Metadata = new Dictionary<string, object>
            {
                { "VendorType", this.VendorType },
                { "AccountGroup", this.AccountGroup },
                { "SourceSystem", "VendorMdm" }
            }
        };
    }

    public bool ShouldAudit(string action)
    {
        // Define which actions should be audited
        var auditableActions = new[]
        {
            "Created", "Updated", "Deleted",
            "Approved", "Rejected", "Suspended",
            "Verified", "Activated", "Deactivated"
        };

        return auditableActions.Contains(action);
    }

    private object? FilterAuditableValues(
        object? state, 
        AuditableFields auditableFields)
    {
        if (state == null) return null;

        // Filter out sensitive fields
        // Only include critical + standard fields
        // Implementation depends on state type (DTO, Entity, etc.)
        
        return state; // Simplified for example
    }

    #endregion
}
```

---

## 3. Event-Driven Audit Logging

### 3.1 Domain Events → Audit Logs

**PRINCIPLE**: Domain Events should automatically generate Audit Logs.

```csharp
namespace VendorMdm.Shared.Ontology.Events;

public abstract class AuditableDomainEvent : IDomainEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Convert this domain event to an audit log entry
    /// </summary>
    public abstract AuditLogEntry ToAuditEntry();
}

// Example: VendorCreatedEvent
public class VendorCreatedEvent : AuditableDomainEvent
{
    public Guid VendorId { get; set; }
    public string LegalName { get; set; } = string.Empty;
    public string VendorType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public override AuditLogEntry ToAuditEntry()
    {
        return new AuditLogEntry
        {
            EntityType = "Vendor",
            EntityId = VendorId,
            Action = "Created",
            NewValues = new
            {
                LegalName,
                VendorType,
                Status
            },
            Reason = "New vendor created",
            Metadata = new Dictionary<string, object>
            {
                { "EventId", EventId },
                { "EventType", EventType }
            }
        };
    }
}
```

### 3.2 Event Handler Integration

```csharp
public class AuditLogEventHandler : IDomainEventHandler<AuditableDomainEvent>
{
    private readonly IAuditLogService _auditLog;

    public AuditLogEventHandler(IAuditLogService auditLog)
    {
        _auditLog = auditLog;
    }

    public async Task Handle(AuditableDomainEvent domainEvent)
    {
        var auditEntry = domainEvent.ToAuditEntry();

        await _auditLog.LogAsync(
            entityType: auditEntry.EntityType,
            entityId: auditEntry.EntityId,
            action: auditEntry.Action,
            oldValues: auditEntry.OldValues,
            newValues: auditEntry.NewValues,
            reason: auditEntry.Reason);
    }
}
```

---

## 4. Service Integration Pattern

### 4.1 Ontology-Driven Service Implementation

```csharp
public class VendorService : IVendorService
{
    private readonly SqlDbContext _context;
    private readonly ILogger<VendorService> _logger;
    private readonly IAuditLogService _auditLog;
    private readonly IDomainEventPublisher _eventPublisher;

    public async Task<Vendor> CreateVendorAsync(CreateVendorRequest request)
    {
        // 1. Create Ontology Concept
        var vendorConcept = new VendorConcept(
            request.LegalName,
            request.VendorType,
            "VendorService");

        // 2. Validate using Ontology
        var validation = vendorConcept.ValidateState();
        if (validation.IsFailure)
            throw new InvalidOperationException(validation.Error);

        // 3. Create Entity
        var vendor = new Vendor
        {
            Id = vendorConcept.Id,
            LegalName = vendorConcept.LegalName,
            VendorType = vendorConcept.VendorType,
            AccountGroup = vendorConcept.AccountGroup,
            Status = "Pending"
        };

        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync();

        // 4. ✅ ONTOLOGY-DRIVEN AUDIT: Let the Concept create the audit entry
        var auditEntry = vendorConcept.CreateAuditEntry(
            action: "Created",
            newState: vendor,
            reason: "New vendor created via registration");

        await _auditLog.LogAsync(
            entityType: auditEntry.EntityType,
            entityId: auditEntry.EntityId,
            action: auditEntry.Action,
            oldValues: auditEntry.OldValues,
            newValues: auditEntry.NewValues,
            reason: auditEntry.Reason);

        // 5. Publish Domain Event (which also creates audit log)
        var domainEvent = new VendorCreatedEvent
        {
            VendorId = vendor.Id,
            LegalName = vendor.LegalName,
            VendorType = vendor.VendorType,
            Status = vendor.Status
        };

        await _eventPublisher.PublishAsync(domainEvent);

        return vendor;
    }
}
```

---

## 5. Entity-Specific Audit Models

### 5.1 Vendor Audit Model

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

    // Metadata
    public string? VendorType { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? VerifiedBy { get; set; }

    public static VendorAuditModel FromEntity(Vendor vendor)
    {
        return new VendorAuditModel
        {
            LegalName = vendor.LegalName,
            Status = vendor.Status,
            TaxId = vendor.TaxId,
            PrimaryContactEmail = vendor.PrimaryContactEmail,
            // ... map other fields
        };
    }
}
```

### 5.2 VendorInvitation Audit Model

```csharp
public class VendorInvitationAuditModel
{
    // Critical Fields
    public string? Status { get; set; }
    public string? VendorLegalName { get; set; }
    public string? PrimaryContactEmail { get; set; }

    // Standard Fields
    public DateTime? ExpiresAt { get; set; }
    public string? InvitedByName { get; set; }
    public string? CurrentStage { get; set; }

    // Metadata
    public Guid? VendorApplicationId { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool? EmailSent { get; set; }

    public static VendorInvitationAuditModel FromEntity(VendorInvitation invitation)
    {
        return new VendorInvitationAuditModel
        {
            Status = invitation.Status,
            VendorLegalName = invitation.VendorLegalName,
            PrimaryContactEmail = invitation.PrimaryContactEmail,
            ExpiresAt = invitation.ExpiresAt,
            InvitedByName = invitation.InvitedByName,
            CurrentStage = invitation.CurrentStage.ToString()
        };
    }
}
```

### 5.3 Event Audit Model

```csharp
public class EventAuditModel
{
    // Critical Fields
    public string? Title { get; set; }
    public string? EventCode { get; set; }
    public string? EventType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // Standard Fields
    public string? CreatedBy { get; set; }
    public int? ParticipantCount { get; set; }

    // Metadata
    public string? Status { get; set; }
    public bool? IsPublished { get; set; }

    public static EventAuditModel FromEntity(Event evt)
    {
        return new EventAuditModel
        {
            Title = evt.Title,
            EventCode = evt.EventCode,
            EventType = evt.EventType,
            StartDate = evt.StartDate,
            EndDate = evt.EndDate,
            CreatedBy = evt.CreatedBy
        };
    }
}
```

---

## 6. Audit Evolution with Entity Changes

### 6.1 Schema Version Tracking

Each audit model MUST track its schema version:

```csharp
public class VendorAuditModel
{
    public string SchemaVersion { get; set; } = "v1.0.0";
    
    // Fields...
}
```

### 6.2 Migration Strategy

When entity schema changes:

1. ✅ Update Ontology Concept's `GetAuditableFields()`
2. ✅ Update Entity-Specific Audit Model
3. ✅ Increment `SchemaVersion`
4. ✅ Create migration guide in `/docs/audit-migrations/`

**Example Migration**:

```markdown
# Vendor Audit Model Migration: v1.0.0 → v1.1.0

## Changes
- Added: `DataResidencyRegion` (critical field)
- Added: `ComplianceStatus` (standard field)
- Removed: `LegacyVendorCode` (deprecated)

## Impact
- Old audit logs (v1.0.0) remain valid
- New audit logs (v1.1.0) include new fields
- Queries must handle both versions
```

---

## 7. Implementation Checklist

### For Each Entity:

- [ ] Create Ontology Concept implementing `IAuditableEntity`
- [ ] Define `GetAuditableFields()` (critical, standard, sensitive)
- [ ] Implement `CreateAuditEntry()`
- [ ] Implement `ShouldAudit()`
- [ ] Create Entity-Specific Audit Model (e.g., `VendorAuditModel`)
- [ ] Create Domain Events extending `AuditableDomainEvent`
- [ ] Update Service to use Ontology-driven auditing
- [ ] Create unit tests for audit model
- [ ] Document audit model in `/docs/audit-models/`

---

## 8. Directory Structure

```
VendorMdm.Shared/
├── Ontology/
│   ├── Concepts/
│   │   ├── VendorConcept.cs          # Implements IAuditableEntity
│   │   ├── InvitationConcept.cs      # Implements IAuditableEntity
│   │   └── EventConcept.cs           # Implements IAuditableEntity
│   ├── Interfaces/
│   │   └── IAuditableEntity.cs       # Base interface
│   ├── Events/
│   │   ├── AuditableDomainEvent.cs   # Base class
│   │   ├── VendorCreatedEvent.cs     # Specific event
│   │   └── VendorUpdatedEvent.cs     # Specific event
│   └── AuditModels/
│       ├── VendorAuditModel.cs       # Entity-specific model
│       ├── InvitationAuditModel.cs   # Entity-specific model
│       └── EventAuditModel.cs        # Entity-specific model
```

---

## 9. Benefits of Ontology-Driven Auditing

### 9.1 Consistency
- ✅ Audit logic defined once in Ontology Concept
- ✅ All services use same audit model
- ✅ No duplicate audit logic

### 9.2 Evolution
- ✅ Entity changes automatically update audit model
- ✅ Schema versioning built-in
- ✅ Migration path documented

### 9.3 Compliance
- ✅ Sensitive fields explicitly marked
- ✅ Critical fields guaranteed to be audited
- ✅ Audit-worthy actions defined in one place

### 9.4 Testability
- ✅ Audit logic unit-testable in Concept
- ✅ No service-level audit logic to test
- ✅ Event-driven audit logging testable

---

## 10. Agent Behavior

### 10.1 When Creating New Entity

**MANDATORY Steps**:

1. ✅ Create Ontology Concept implementing `IAuditableEntity`
2. ✅ Define auditable fields (critical, standard, sensitive)
3. ✅ Create Entity-Specific Audit Model
4. ✅ Create Domain Events extending `AuditableDomainEvent`
5. ✅ Update Service to use Ontology-driven auditing
6. ✅ Create unit tests
7. ✅ Document in `/docs/audit-models/`

### 10.2 When Modifying Existing Entity

**MANDATORY Steps**:

1. ✅ Update Ontology Concept's `GetAuditableFields()`
2. ✅ Update Entity-Specific Audit Model
3. ✅ Increment `SchemaVersion`
4. ✅ Create migration guide
5. ✅ Update unit tests

---

## 11. Revision History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2026-02-03 | Initial standard |
| 2.0.0 | 2026-02-03 | **Ontology integration** - Entity-specific audit models, event-driven logging |

---

**Status**: ✅ **MANDATORY COMPLIANCE REQUIRED**  
**Effective**: Immediately for all new entities  
**Migration**: Existing entities must be updated within 2 sprints
