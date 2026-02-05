# Architecture DNA

---

## 1.1 Hexagonal Architecture (Ports & Adapters)

| Layer | Location | Responsibility |
|-------|----------|----------------|
| **Core Domain** | `VendorMdm.Shared/` | PURE business logic, NO external references |
| **Ontology** | `VendorMdm.Shared/Ontology/` | Domain concepts, validation rules |
| **Inbound Port** | `VendorMdm.Api/Controllers/` | REST APIs, DTOs |
| **Persistence** | `VendorMdm.Api/Data/` | SQL + JSONB hybrid |
| **Outbound Port** | `VendorMdm.Api/Services/` | Domain events |
| **ACL Adapters** | `MigrationRunner/` | External system translation (SAP, etc.) |

---

## 1.2 Hybrid Relational-Document Model

### Use SQL Column when:
- Foreign key constraint required
- Indexing/ORDER BY/GROUP BY needed
- ACID compliance required
- Universal presence (100% of records)

### Use JSONB when:
- High volatility (changes faster than deployments)
- Context-specific (subset of records)
- Read-only payload (frontend display)
- Dynamic hierarchy

### Example Decision Matrix

| Field | SQL Column? | JSONB? | Reason |
|-------|-------------|--------|--------|
| `VendorId` | ✅ | | Foreign key |
| `LegalName` | ✅ | | Universal, indexed |
| `Status` | ✅ | | Workflow state |
| `UiPreferences` | | ✅ | High volatility |
| `CampaignMetadata` | | ✅ | Context-specific |

---

## 1.3 Ontology-Driven Development

**Rule**: Business logic MUST exist in `VendorMdm.Shared/Ontology/Concepts`

**Pattern**: `Service → Ontology Concept → Result`

### Concept Structure
```csharp
public class VendorConcept : IOntologyConcept, IAuditableEntity
{
    // Properties
    public Guid Id { get; set; }
    public string LegalName { get; set; }

    // Validation
    public Result ValidateState()
    {
        if (string.IsNullOrEmpty(LegalName))
            return Result.Fail("Legal name is required");
        return Result.Ok();
    }

    // Business Logic
    public bool IsEligibleForInvitation()
    {
        return Status == "Active" && !HasPendingInvitation;
    }

    // Audit
    public AuditableFields GetAuditableFields() { ... }
}
```

---

## 1.4 API Contract-First

**Rule**: APIs MUST return DTOs, NEVER SQL entities

### Correct Pattern
```
Client → Controller → DTO → Service → Concept → Entity → Database
                  ↓
            Response DTO
```

### Forbidden
```csharp
// ❌ WRONG
return Ok(await _context.Vendors.FindAsync(id));

// ✅ CORRECT
var vendor = await _context.Vendors.FindAsync(id);
return Ok(VendorDto.FromEntity(vendor));
```

---

## 1.5 Event-Driven Architecture

### Domain Events
```csharp
public class VendorCreatedEvent : IDomainEvent
{
    public Guid VendorId { get; set; }
    public string LegalName { get; set; }
    public DateTime OccurredAt { get; set; }
}
```

### Event Publishing
```csharp
await _eventPublisher.PublishAsync(new VendorCreatedEvent
{
    VendorId = vendor.Id,
    LegalName = vendor.LegalName,
    OccurredAt = DateTime.UtcNow
});
```

### Async Side-Effects via Events
- Email notifications
- SAP sync
- Audit logging
- Cache invalidation

---

## 1.6 Directory Structure

```
vendor-mdm-portal/
├── backend/
│   ├── VendorMdm.Api/              # Main API (Inbound Port)
│   │   ├── Controllers/            # REST endpoints
│   │   ├── Services/               # Service layer
│   │   ├── Data/                   # DbContext
│   │   ├── Middleware/             # HTTP middleware
│   │   └── Models/                 # DTOs
│   ├── VendorMdm.Shared/           # Core Domain
│   │   ├── Models/                 # Entities
│   │   ├── Ontology/               # Business concepts
│   │   │   ├── Concepts/           # VendorConcept, EventConcept
│   │   │   ├── Interfaces/         # IOntologyConcept
│   │   │   └── Events/             # Domain events
│   │   └── Contracts/              # DTOs
│   ├── VendorMdm.Core.Framework/   # Shared foundation
│   │   ├── Security/               # Auth services
│   │   ├── Resilience/             # Polly policies
│   │   ├── Logging/                # Structured logging
│   │   └── HealthChecks/           # Health monitoring
│   └── VendorMdm.Infrastructure/   # Data access
│       └── Repositories/           # Repository pattern
└── frontend/                       # React SPA
```
