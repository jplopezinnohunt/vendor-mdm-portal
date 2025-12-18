---
description: Workflow for adding new canonical entities (Hexagonal/Serverless Pattern)
---

# Add New Canonical Entity Workflow

This workflow guides you through adding a new canonical entity to the platform following the **Hexagonal Architecture** and **Hybrid Relational-Document** principles.

## Prerequisites
- [ ] Entity business requirements defined
- [ ] Key structured fields identified (for SQL columns)
- [ ] Schema attributes defined (for JSONB)

---

## Step 10: Mandatory Verification (Deployment Gate)

**STOP**: You cannot mark this task as complete until you have verified:

1.  **Build Success**: `dotnet build` returns 0 errors.
2.  **Runtime Verification**:
    -   Run API: `dotnet run --project backend/VendorMdm.Api`
    -   Open Swagger: `http://localhost:5001/swagger`
    -   Execute `POST /api/[EntityName]` -> 201 Created
    -   Execute `GET /api/[EntityName]/{id}` -> 200 OK
3.  **Schema Compliance**:
    -   Verify the `Data` column in SQL contains valid JSON corresponding to your schema.

---

## Step 11: Document Entity

Create a new schema file in `backend/VendorMdm.Shared/Schemas/[entity-name]-v1.0.0.json`.

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "$id": "https://platform.vendor.com/schemas/[entity]/v1.0.0",
  "title": "[Entity] Canonical Entity",
  "type": "object",
  "additionalProperties": false,
  "required": ["keyField1", "email"],
  "properties": {
    "keyField1": { "type": "string", "maxLength": 100 },
    "email": { "type": "string", "format": "email" },
    "attributes": { "type": "object" }
  }
}
```

---

## Step 2: Create Canonical Model (Core Domain)

Edit `backend/VendorMdm.Shared/Models/CanonicalEntities.cs`. Add the class inheriting from `CanonicalEntityBase`.

```csharp
/// <summary>
/// Canonical [Entity] entity.
/// </summary>
public class [EntityName] : CanonicalEntityBase
{
    // Structured Columns (Indexed/Identity)
    [Required]
    [MaxLength(100)]
    public string [KeyField] { get; set; } = string.Empty;

    // Inherits 'Data' (JSONB) for all other properties
}
```

---

## Step 3: Update Persistence Layer (Port Implementation)

1. Edit `backend/VendorMdm.Api/Data/SqlDbContext.cs`:
   - Add `public DbSet<[EntityName]> [EntityPlural] { get; set; }`
   - In `OnModelCreating`, add configuration:
     ```csharp
     modelBuilder.Entity<[EntityName]>(entity =>
     {
         entity.HasKey(e => e.Id);
         entity.HasIndex(e => e.[KeyField]); // Index critical fields
         entity.Property(e => e.Data).HasColumnType("nvarchar(max)").IsRequired().HasDefaultValue("{}");
     });
     ```

2. Create Migration:
   // turbo
   ```bash
   dotnet dotnet-ef migrations add Add[EntityName]CanonicalEntity --context SqlDbContext --project backend/VendorMdm.Api --startup-project backend/VendorMdm.Api
   ```

3. Apply Migration:
   // turbo
   ```bash
   dotnet dotnet-ef database update --context SqlDbContext --project backend/VendorMdm.Api --startup-project backend/VendorMdm.Api
   ```

---

## Step 4: Create Service Layer (Inbound/Outbound Logic)

1. Create Interface `backend/VendorMdm.Api/Services/I[EntityName]Service.cs`.
2. Create Implementation `backend/VendorMdm.Api/Services/[EntityName]Service.cs`.

**Pattern:**
1. **Validate**: Check constraints.
2. **SQL Persistence**: Save state to SQL.
3. **Functional Log**: Save artifact to Cosmos DB (`SaveArtifactAsync`).
4. **Event Bus**: Emit domain event to Cosmos DB (`LogDomainEventAsync`).

```csharp
public async Task<[EntityName]> CreateAsync([EntityName] entity)
{
    // 1. SQL
    _context.[EntityPlural].Add(entity);
    await _context.SaveChangesAsync();

    // 2. Artifact
    await _cosmosRepository.SaveArtifactAsync(entity.Id.ToString(), entity);

    // 3. Event
    await _cosmosRepository.LogDomainEventAsync(new DomainEvent { ... });

    return entity;
}
```

3. Register in `Program.cs`:
   ```csharp
   builder.Services.AddScoped<I[EntityName]Service, [EntityName]Service>();
   ```

---

## Step 5: Create- [ ] API controller created with correct Error Handling (try/catch -> 500)(Inbound Port)

Create `backend/VendorMdm.Api/Controllers/[EntityName]Controller.cs`.

- Use `[ApiController]` and `[Route("api/[controller]")]`.
- Inject `I[EntityName]Service` (NEVER `SqlDbContext`).
- Endpoint checks `ModelState`, calls Service, returns `ActionResult`.

---

## Step 6: Verify

1. Build project:
   // turbo
   ```bash
   dotnet build backend/VendorMdm.Api
   ```
2. Test creation via Swagger or Integration Test.
