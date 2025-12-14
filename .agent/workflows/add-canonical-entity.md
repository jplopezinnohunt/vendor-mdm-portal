---
description: Workflow for adding new canonical entities (Employee, Funds, WBS Project, etc.)
---

# Add New Canonical Entity Workflow

This workflow guides you through adding a new canonical entity to the platform following mandatory canonical model principles.

## Prerequisites

- [ ] Entity requirements documented
- [ ] Entity lifecycle states defined
- [ ] SAP mapping requirements (if applicable) identified

---

## Step 1: Define Entity Model

Create entity class in `backend/VendorMdm.Shared/Models/CanonicalEntities.cs`:

```csharp
/// <summary>
/// Canonical [EntityName] entity
/// [Brief description of purpose]
/// </summary>
public class [EntityName] : CanonicalEntityBase
{
    // Structured columns (for indexing/querying)
    
    [Required]
    [MaxLength(200)]
    public string [KeyField] { get; set; } = string.Empty;
    
    // Additional indexed fields...
    
    // Inherited from CanonicalEntityBase:
    // - Id, EntityVersion, Status, SourceSystem
    // - Data (JSON), CreatedAt, UpdatedAt, SchemaVersion
}
```

**Decision Matrix for Fields:**
- **SQL Column**: Foreign keys, indexed fields, universal fields
- **Data JSON**: Volatile fields, context-specific, nested structures

---

## Step 2: Define Status Enum and State Machine

Add status constants and validation:

```csharp
public static class [EntityName]Status
{
    public const string [Status1] = "[Status1]";
    public const string [Status2] = "[Status2]";
    // ... more statuses
    
    private static readonly Dictionary<string, HashSet<string>> ValidTransitions = new()
    {
        [Status1] = new() { Status2, ... },
        [Status2] = new() { ... }
    };
    
    public static bool IsValidTransition(string from, string to)
    {
        return ValidTransitions.ContainsKey(from) && 
               ValidTransitions[from].Contains(to);
    }
}
```

---

## Step 3: Create JSON Schema

Create `backend/VendorMdm.Shared/Schemas/[entity-name]-v1.0.0.json`:

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "$id": "https://platform.vendor.com/schemas/[entity-name]/v1.0.0",
  "title": "[EntityName] Canonical Entity",
  "type": "object",
  "required": ["field1", "field2"],
  "properties": {
    "field1": { "type": "string", "maxLength": 200 },
    "field2": { "type": "number" },
    "nestedObject": {
      "type": "object",
      "properties": {
        "subField": { "type": "string" }
      }
    }
  }
}
```

---

## Step 4: Add to DbContext

Update `backend/VendorMdm.Api/Data/SqlDbContext.cs`:

```csharp
public DbSet<[EntityName]> [EntityNames] { get; set; }
```

Add configuration in `OnModelCreating`:

```csharp
// Configure indexes
modelBuilder.Entity<[EntityName]>()
    .HasIndex(e => e.[KeyField]);

modelBuilder.Entity<[EntityName]>()
    .HasIndex(e => e.Status);

// Configure constraints
modelBuilder.Entity<[EntityName]>()
    .Property(e => e.Data)
    .HasColumnType("nvarchar(max)");
```

---

## Step 5: Create Migration

// turbo
```bash
cd backend/VendorMdm.Api
dotnet ef migrations add Add[EntityName]CanonicalEntity --context SqlDbContext
```

Review migration file, then apply:

// turbo
```bash
dotnet ef database update
```

---

## Step 6: Create Service Layer

Create `backend/VendorMdm.Api/Services/[EntityName]Service.cs`:

```csharp
public interface I[EntityName]Service
{
    Task<[EntityName]> CreateAsync(Create[EntityName]Request request);
    Task<[EntityName]> UpdateAsync(Guid id, Update[EntityName]Request request);
    Task<[EntityName]> GetByIdAsync(Guid id);
}

public class [EntityName]Service : I[EntityName]Service
{
    private readonly SqlDbContext _context;
    private readonly ILogger<[EntityName]Service> _logger;
    private readonly Container _cosmosEventsContainer;
    private readonly Container _cosmosArtifactsContainer;
    
    public async Task<[EntityName]> CreateAsync(Create[EntityName]Request request)
    {
        // STEP A: SQL - Create canonical entity
        var entity = new [EntityName]
        {
            [KeyField] = request.[KeyField],
            Status = [EntityName]Status.[InitialStatus],
            SourceSystem = SourceSystems.Portal,
            EntityVersion = 1,
            SchemaVersion = "v1.0.0",
            Data = JsonConvert.SerializeObject(new { /* data payload */ })
        };
        
        // Validate canonical fields
        entity.ValidateCanonicalFields();
        
        _context.[EntityNames].Add(entity);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("[EntityName] created: {Id}", entity.Id);
        
        // STEP B: Cosmos - Store artifact (non-blocking)
        try
        {
            await SaveArtifactAsync(entity.Id.ToString(), new
            {
                entityId = entity.Id,
                fullPayload = request,
                createdAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store artifact for {Id}", entity.Id);
        }
        
        // STEP C: Cosmos - Emit event (non-blocking)
        try
        {
            await EmitDomainEventAsync("[EntityName]Created", entity.Id.ToString(), new
            {
                entityId = entity.Id,
                entityVersion = entity.EntityVersion,
                // ... event data
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to emit event for {Id}", entity.Id);
        }
        
        return entity;
    }
    
    private async Task EmitDomainEventAsync(string eventType, string entityId, object data)
    {
        var evt = new EnhancedDomainEvent
        {
            EventType = eventType,
            EntityId = entityId,
            CorrelationId = GetCorrelationId(),
            Actor = GetCurrentUserId(),
            Channel = EventChannels.Portal,
            Data = data
        };
        
        await _cosmosEventsContainer.CreateItemAsync(evt, new PartitionKey(eventType));
    }
}
```

---

## Step 7: Add SAP Mapping (if applicable)

If entity maps to SAP, create mapper in `backend/VendorMdm.Shared/Mapping/`:

```csharp
public interface I[EntityName]SapMapper
{
    Task<Sap[EntityName]> ToSapAsync([EntityName] entity);
    Task<[EntityName]> FromSapAsync(Sap[EntityName] sapEntity);
}

public class [EntityName]SapMapper : I[EntityName]SapMapper
{
    private readonly ISapIdMappingService _sapIdService;
    
    public async Task<Sap[EntityName]> ToSapAsync([EntityName] entity)
    {
        // Map canonical entity → SAP structure
        var sapId = await _sapIdService.GetOrCreateSapIdAsync(
            entity.Id, 
            nameof([EntityName])
        );
        
        return new Sap[EntityName]
        {
            SapId = sapId,
            // ... map fields
        };
    }
}
```

Add mapping record:
```csharp
await _context.SapIdMappings.AddAsync(new SapIdMapping
{
    CanonicalEntityId = entity.Id,
    EntityType = nameof([EntityName]),
    SapId = sapId,
    SapEnvironment = "D01"
});
```

---

## Step 8: Create API Controller

Create `backend/VendorMdm.Api/Controllers/[EntityName]Controller.cs`:

```csharp
[ApiController]
[Route("api/[controller]")]
public class [EntityName]Controller : ControllerBase
{
    private readonly I[EntityName]Service _service;
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Create[EntityName]Request request)
    {
        var entity = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var entity = await _service.GetByIdAsync(id);
        return Ok(entity);
    }
}
```

---

## Step 9: Add Tests

Create `backend/VendorMdm.Api.Tests/[EntityName]ServiceTests.cs`:

```csharp
[TestClass]
public class [EntityName]ServiceTests
{
    [TestMethod]
    public async Task Create_Should_SetCanonicalFields()
    {
        var entity = await _service.CreateAsync(new Create[EntityName]Request { ... });
        
        Assert.IsNotNull(entity.Id);
        Assert.AreEqual(1, entity.EntityVersion);
        Assert.AreEqual(SourceSystems.Portal, entity.SourceSystem);
        Assert.AreEqual("v1.0.0", entity.SchemaVersion);
    }
    
    [TestMethod]
    public async Task Update_Should_IncrementVersion()
    {
        var entity = await _service.UpdateAsync(id, request);
        Assert.AreEqual(2, entity.EntityVersion);
    }
}
```

---

## Step 10: Document Entity

Update `docs/DATABASE_SCHEMA.md`:

```markdown
### [EntityName]

**Purpose**: [Description]

#### Structured Columns
| Column | Type | Purpose |
|--------|------|---------|
| Id | UNIQUEIDENTIFIER | Canonical ID |
| [KeyField] | NVARCHAR(200) | [Description] |
| Status | NVARCHAR(50) | Lifecycle state |

#### Data JSON Schema
\```typescript
interface [EntityName]Data {
  field1: string;
  nestedObject?: {
    subField: string;
  };
}
\```
```

---

## Checklist

Before marking entity as complete:

- [ ] Entity inherits `CanonicalEntityBase`
- [ ] Status enum and state machine defined
- [ ] JSON Schema created and validated
- [ ] DbContext updated with DbSet and indexes
- [ ] EF Migration created and applied
- [ ] Service layer implements A→B→C→D pattern (SQL→Artifact→Event→Bus)
- [ ] SAP mapper created (if SAP integration needed)
- [ ] No SAP fields in entity model
- [ ] API controller created
- [ ] Unit tests passing
- [ ] Documentation updated
- [ ] Code review approved

---

## Examples

See existing canonical entities:
- `Vendor` - file:///Users/jplopez/projects/vendor-mdm-portal/backend/VendorMdm.Shared/Models/CanonicalEntities.cs
- `VendorInvitationCanonical`
- `ChangeRequestCanonical`
