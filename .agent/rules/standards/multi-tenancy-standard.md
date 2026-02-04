# Multi-Tenancy Standard

**Category**: Integration & Infrastructure
**Pattern #**: 18
**Status**: MANDATORY

---

## Definition

Tenant isolation MUST be enforced via global query filters. NEVER expose cross-tenant data.

---

## Rules

1. **ALWAYS** filter by TenantId in queries
2. **ALWAYS** use global query filters in DbContext
3. **NEVER** allow cross-tenant data access
4. **ALWAYS** set TenantId from authenticated context

---

## Implementation

### Tenant Context

```csharp
public interface ITenantContext
{
    Guid TenantId { get; }
    string TenantName { get; }
}

public class TenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Guid TenantId => GetTenantIdFromClaims();
    public string TenantName => GetTenantNameFromClaims();

    private Guid GetTenantIdFromClaims()
    {
        var claim = _httpContextAccessor.HttpContext?.User
            .FindFirst("tenant_id")?.Value;
        return Guid.TryParse(claim, out var tenantId)
            ? tenantId
            : throw new UnauthorizedAccessException("No tenant context");
    }
}
```

### Entity with TenantId

```csharp
public interface ITenantEntity
{
    Guid TenantId { get; set; }
}

public class Vendor : CanonicalEntityBase, ITenantEntity
{
    public Guid TenantId { get; set; }
    // ... other properties
}
```

### Global Query Filter (DbContext)

```csharp
public class SqlDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply tenant filter to all tenant entities
        modelBuilder.Entity<Vendor>()
            .HasQueryFilter(v => v.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<Document>()
            .HasQueryFilter(d => d.TenantId == _tenantContext.TenantId);

        // ... repeat for all tenant entities
    }
}
```

### Auto-Set TenantId (SaveChanges Interceptor)

```csharp
public override int SaveChanges()
{
    foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
    {
        if (entry.State == EntityState.Added)
        {
            entry.Entity.TenantId = _tenantContext.TenantId;
        }
    }
    return base.SaveChanges();
}
```

### Cross-Tenant Access (Admin Only)

```csharp
// ONLY for system admin operations
public async Task<List<Vendor>> GetAllVendorsAcrossTenantsAsync()
{
    // Must have ITAdmin role
    if (!_userContext.HasRole(Roles.ITAdmin))
        throw new UnauthorizedAccessException();

    return await _context.Vendors
        .IgnoreQueryFilters()  // Bypass tenant filter
        .ToListAsync();
}
```

---

## Anti-Patterns

❌ Manual `WHERE TenantId = ...` in every query
❌ Forgetting to set TenantId on new entities
❌ Allowing cross-tenant queries without admin check
❌ Storing TenantId in session instead of claims

---

## Reference

- **Interface**: `Core.Framework/Tenancy/ITenantContext.cs`
- **Golden Rules**: Section 10.4 Pattern 18
