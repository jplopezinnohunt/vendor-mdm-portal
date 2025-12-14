---
description: Schema Compliance Check - Hybrid Relational-Document Model
---

# Schema Compliance Check Workflow

Use this workflow whenever creating or modifying SQL entity models to ensure compliance with the Hybrid Relational-Document Model architectural standard.

## Decision Matrix

For each field you're adding, ask:

### ✅ Use SQL Column if ANY apply:
- Foreign key constraint needed (links to another table)
- Frequently used in WHERE, ORDER BY, GROUP BY clauses
- Financial/critical state requiring ACID compliance
- Present in 100% of records (universal presence)
- Requires strong typing validation (e.g., email, dates)

### ✅ Use JSON Attribute if ANY apply:
- Business requirements change frequently
- Context-specific (only applies to subset of records)
- Primarily read by frontend, rarely queried by backend
- Nested structure not worth normalizing into separate tables
- Presentation-layer specific (UI preferences, metadata)

## Implementation Steps

### 1. Review the Field
Determine which category the field belongs to using the Decision Matrix above.

### 2. Apply the Rule

**If SQL Column:**
```csharp
[Required] // or appropriate validation
[MaxLength(100)] // specify constraints
public string FieldName { get; set; } = string.Empty;
```

**If JSON Attribute:**
No change to entity needed - use existing `Attributes` property.

### 3. Document Usage

**Option A: Add to AttributeModels.cs** (Recommended)
```csharp
public class VendorApplicationAttributes
{
    public string? NewField { get; set; }
    // ... other fields
}
```

**Option B: Document in entity XML comment**
Update the `/// Stores: ...` section in the entity's Attributes property comment.

### 4. Generate Migration (if SQL Column added)

```bash
cd backend/VendorMdm.Api
dotnet ef migrations add Add<FieldName>Column
```

Review the migration file before applying!

### 5. Update DbContext (if needed)

If adding special configuration:
```csharp
modelBuilder.Entity<YourEntity>()
    .Property(e => e.NewField)
    .HasMaxLength(200)
    .IsRequired();
```

## Examples

### ❌ WRONG: Adding notes as SQL column
```csharp
// Don't do this!
[MaxLength(1000)]
public string? Notes { get; set; }
```
**Why wrong?** Notes are read-only payload that changes frequently.

### ✅ CORRECT: Using Attributes for notes
```csharp
// In AttributeModels.cs
public class VendorInvitationAttributes
{
    public string? Notes { get; set; }
}

// Usage in service/controller
using VendorMdm.Shared.Helpers;

var attrs = JsonAttributeHelper.DeserializeAttributes<VendorInvitationAttributes>(
    invitation.Attributes
);
attrs.Notes = "New notes here";
invitation.Attributes = JsonAttributeHelper.SerializeAttributes(attrs);
```

### ✅ CORRECT: Adding legal name as SQL column
```csharp
[Required]
[MaxLength(200)]
public string LegalName { get; set; } = string.Empty;
```
**Why correct?** Universal presence, indexed for search, required for business logic.

### ✅ CORRECT: Complex user profile in JSON
```csharp
// Define structure in AttributeModels.cs
public class UserRoleAttributes
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public UiPreferences? UiPreferences { get; set; }
}

// Usage
var userAttrs = new UserRoleAttributes
{
    FullName = "John Doe",
    Email = "john@example.com",
    UiPreferences = new UiPreferences { Theme = "dark" }
};
user.Attributes = JsonAttributeHelper.SerializeAttributes(userAttrs);
```

## Working with JSON Attributes

### Read Attributes
```csharp
// Full object deserialization
var attrs = JsonAttributeHelper.DeserializeAttributes<VendorApplicationAttributes>(
    application.Attributes
);

// Single key access
var notes = JsonAttributeHelper.GetAttribute<string>(invitation.Attributes, "notes");
```

### Write Attributes
```csharp
// Full object serialization
var attrs = new VendorApplicationAttributes
{
    IndustryCode = "TECH",
    Certifications = new List<string> { "ISO9001", "SOC2" }
};
application.Attributes = JsonAttributeHelper.SerializeAttributes(attrs);

// Single key update
invitation.Attributes = JsonAttributeHelper.SetAttribute(
    invitation.Attributes, 
    "notes", 
    "Updated notes"
);
```

## SQL Server JSON Queries

If you need to filter by JSON attributes in queries:

```csharp
// Using JSON_VALUE in LINQ
var results = await _context.VendorApplications
    .FromSqlRaw(@"
        SELECT * FROM VendorApplications 
        WHERE JSON_VALUE(Attributes, '$.industryCode') = {0}
    ", industryCode)
    .ToListAsync();
```

## Performance Optimization

If a JSON key becomes frequently queried, create a computed column:

```sql
-- Add computed column
ALTER TABLE VendorApplications
ADD IndustryCode AS JSON_VALUE(Attributes, '$.industryCode') PERSISTED;

-- Add index
CREATE INDEX IX_VendorApplications_IndustryCode 
ON VendorApplications(IndustryCode);
```

Then query normally:
```csharp
var apps = await _context.VendorApplications
    .Where(a => a.IndustryCode == "TECH")
    .ToListAsync();
```

## Pre-Commit Checklist

Before committing entity changes:

- [ ] Field categorized using Decision Matrix
- [ ] Correct storage location chosen (SQL Column vs JSON Attribute)
- [ ] XML documentation updated (for SQL columns)
- [ ] AttributeModels.cs updated (for JSON attributes)
- [ ] Migration generated and reviewed (for SQL columns)
- [ ] Helper methods tested (for JSON attributes)
- [ ] No hardcoded strings (use constants/enums where appropriate)

## Common Mistakes to Avoid

1. **❌ Adding sparse fields as SQL columns**
   - If <50% of records will have the value, use JSON

2. **❌ Storing UI-specific data in SQL columns**
   - Theme preferences, dashboard layouts → JSON

3. **❌ Not documenting JSON structure**
   - Always define in AttributeModels.cs for type safety

4. **❌ Querying JSON without indexes**
   - Create computed columns for frequently filtered JSON keys

5. **❌ Forgetting default value**
   - Always initialize Attributes = "{}" in entity constructor

## Questions?

If unsure whether a field should be SQL or JSON:
1. Ask: "Will I need to JOIN on this field?" → SQL
2. Ask: "Will this change with every sprint?" → JSON
3. Ask: "Is this critical for ACID transactions?" → SQL
4. Ask: "Is this presentation/metadata?" → JSON

**When in doubt, prefer JSON** - it's easier to migrate JSON → SQL than SQL → JSON.
