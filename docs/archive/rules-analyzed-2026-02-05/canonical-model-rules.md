# Canonical Entity Model - MANDATORY RULES

> [!IMPORTANT]
> **Effective Date**: December 14, 2025  
> **Scope**: All domain entities in the Vendor Platform  
> **Enforcement**: CI/CD pipeline + Code review gates

---

## Overview

The Vendor Platform uses a **Canonical Domain Model** where all domain entities follow strict patterns for consistency, auditability, and multi-app scalability.

**Core Principle**: "One Entity Definition, Many Applications"

---

## Mandatory Rules

### Rule 1: All Entities MUST Inherit CanonicalEntityBase

```csharp
public class MyEntity : CanonicalEntityBase
{
    // Entity-specific columns
}
```

This provides: UUID, versioning, status, source system, data JSON, timestamps, schema version.

---

### Rule 2: Use Column vs JSON Decision Matrix

**SQL Column** (Structured):
- Foreign key relationships
- Frequently indexed/searched fields
- ACID-required transactions
- Universal presence (100% of records)

**Data JSON** (Semi-Structured):
- Schema evolution expected
- Context-specific (sparse) data
- Nested structures
- Presentation/UI data

---

### Rule 3: NO SAP Fields in Domain Entities

**FORBIDDEN**:
```csharp
public string SapVendorId { get; set; }  // ❌ WRONG
```

**REQUIRED**:
```csharp
// Use SapIdMapping table
var sapId = await _sapIdService.GetSapIdAsync(entity.Id, "Vendor");
```

---

### Rule 4: Schema Versioning Required

- All entities have `SchemaVersion` field
- JSON Schema file required in `/Schemas/`
- Validate Data payload before saving

---

### Rule 5: State Machine Required

Define valid status transitions:

```csharp
public static class EntityStatus
{
    public const string Active = "Active";
    public const string Suspended = "Suspended";
    
    public static bool IsValidTransition(string from, string to) { ... }
}
```

---

### Rule 6: Event Sourcing Mandatory

All state changes emit domain events:

```csharp
await EmitDomainEventAsync("EntityCreated", entity.Id, new
{
    entityId = entity.Id,
    entityVersion = entity.EntityVersion,
    correlationId = GetCorrelationId(),  // REQUIRED
    actor = GetCurrentUserId(),          // REQUIRED
    channel = EventChannels.Portal       // REQUIRED
});
```

---

### Rule 7: Entity Versioning for Concurrency

Increment `EntityVersion` on every update:

```csharp
entity.IncrementVersion();  // Updates version + timestamp
await _context.SaveChangesAsync();
```

---

### Rule 8: Source System Tracking

Valid values: `Portal`, `SAP`, `API`, `Migration`, `Batch`

```csharp
var entity = new Entity
{
    SourceSystem = SourceSystems.Portal  // REQUIRED
};
```

---

## Workflow for New Entities

See: [/Users/jplopez/projects/vendor-mdm-portal/.agent/workflows/add-canonical-entity.md](file:///Users/jplopez/projects/vendor-mdm-portal/.agent/workflows/add-canonical-entity.md)

---

## Current Canonical Entities

| Entity | Status | File |
|--------|--------|------|
| `Vendor` | ✅ Implemented | [CanonicalEntities.cs](file:///Users/jplopez/projects/vendor-mdm-portal/backend/VendorMdm.Shared/Models/CanonicalEntities.cs) |
| `VendorInvitationCanonical` | ✅ Implemented | Same file |
| `ChangeRequestCanonical` | ✅ Implemented | Same file |
| `Employee` | 📋 Planned | Use workflow to add |
| `Funds` | 📋 Planned | Use workflow to add |
| `WbsProject` | 📋 Planned | Use workflow to add |

---

## Enforcement

All PRs must pass:
- [ ] Entity inherits `CanonicalEntityBase`
- [ ] No SAP fields present
- [ ] JSON Schema exists
- [ ] State machine defined
- [ ] Events emitted
- [ ] Tests passing

**Violations block merge.**

---

## Questions?

1. Review existing canonical entities in `CanonicalEntities.cs`
2. Follow workflow in `.agent/workflows/add-canonical-entity.md`
3. Ask architecture team
