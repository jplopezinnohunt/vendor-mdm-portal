# Soft Delete Standard

**Category**: Security & Compliance
**Pattern #**: 11
**Status**: MANDATORY

---

## Definition

NEVER hard delete data. All deletions MUST use soft delete with `IsDeleted` flag.

---

## Rules

1. **NEVER** use `DELETE FROM` or `Remove()` for business data
2. **ALWAYS** set `IsDeleted = true` instead
3. **ALWAYS** use global query filters to exclude deleted records
4. **ALWAYS** log soft deletes with reason

---

## Implementation

### ISoftDeletable Interface

```csharp
// Shared/Ontology/Interfaces/ISoftDeletable.cs
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    string? DeletedBy { get; set; }
}
```

### CanonicalEntityBase (Already Includes)

```csharp
public abstract class CanonicalEntityBase : ISoftDeletable
{
    // ... other properties ...

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
```

### Global Query Filter (DbContext)

```csharp
// SqlDbContext.cs - ConfigureCanonicalEntities
modelBuilder.Entity<Vendor>(entity =>
{
    entity.HasQueryFilter(e => !e.IsDeleted);  // ← Automatic filtering
    entity.HasIndex(e => e.IsDeleted);
});
```

### Soft Delete Method

```csharp
public async Task<Result> SoftDeleteAsync(Guid id, string userId, string reason)
{
    var entity = await _context.Vendors.FindAsync(id);
    if (entity == null)
        return Result.Failure("Entity not found");

    entity.IsDeleted = true;
    entity.DeletedAt = DateTime.UtcNow;
    entity.DeletedBy = userId;

    await _context.SaveChangesAsync();

    _logger.LogInformation("Entity soft deleted", new {
        entityType = "Vendor",
        entityId = id,
        deletedBy = userId,
        reason
    });

    return Result.Success();
}
```

### Querying Deleted Records (Admin Only)

```csharp
// Include soft-deleted records (for admin restore)
var allVendors = await _context.Vendors
    .IgnoreQueryFilters()  // ← Bypass soft delete filter
    .Where(v => v.IsDeleted)
    .ToListAsync();
```

---

## Anti-Patterns

❌ Using `_context.Vendors.Remove(entity)`
❌ Using raw SQL `DELETE FROM`
❌ Forgetting `HasQueryFilter` in DbContext
❌ Not logging soft deletes

---

## Reference

- **Interface**: `Shared/Ontology/Interfaces/ISoftDeletable.cs`
- **Base Class**: `Shared/Models/CanonicalEntityBase.cs`
- **Golden Rules**: Section 10.3 Pattern 11
