# Data Residency Standard

**Category**: Integration & Infrastructure
**Pattern #**: 17
**Status**: MANDATORY for multi-region deployments

---

## Definition

Data MUST be stored in the region specified by regulatory requirements with configurable per-deployment settings.

---

## Rules

1. **ALWAYS** track data region in metadata
2. **ALWAYS** route storage to correct regional endpoint
3. **NEVER** transfer PII across regions without consent
4. **ALWAYS** log cross-region data access

---

## Implementation

### Data Region Enum

```csharp
public enum DataRegion
{
    EU,      // Europe (GDPR)
    US,      // United States
    APAC,    // Asia Pacific
    UK       // United Kingdom (post-Brexit)
}
```

### Entity with Region

```csharp
public interface IRegionAwareEntity
{
    DataRegion DataRegion { get; set; }
}

public class Vendor : CanonicalEntityBase, IRegionAwareEntity
{
    public DataRegion DataRegion { get; set; }
    // ... other properties
}
```

### Region-Aware Storage

```csharp
public class RegionAwareStorageService
{
    private readonly Dictionary<DataRegion, IFileStorageService> _regionalStorage;

    public async Task<Result<string>> UploadAsync(
        Stream content,
        string fileName,
        DataRegion region)
    {
        var storage = _regionalStorage[region];
        return await storage.UploadAsync(content, fileName, "documents");
    }
}
```

### Configuration

```json
{
  "DataResidency": {
    "DefaultRegion": "EU",
    "Regions": {
      "EU": {
        "StorageAccount": "stvendormdmeu",
        "DatabaseServer": "sql-vendor-mdm-eu.database.windows.net"
      },
      "US": {
        "StorageAccount": "stvendormdmus",
        "DatabaseServer": "sql-vendor-mdm-us.database.windows.net"
      }
    }
  }
}
```

### Region Detection from User

```csharp
public DataRegion DetermineRegion(User user)
{
    // Based on user's country
    return user.Country switch
    {
        "DE" or "FR" or "IT" or "ES" => DataRegion.EU,
        "US" or "CA" => DataRegion.US,
        "UK" or "GB" => DataRegion.UK,
        "JP" or "AU" or "SG" => DataRegion.APAC,
        _ => DataRegion.EU  // Default to EU (strictest)
    };
}
```

### Cross-Region Access Logging

```csharp
public async Task<Result<Vendor>> GetVendorAsync(Guid id, DataRegion userRegion)
{
    var vendor = await _context.Vendors.FindAsync(id);
    if (vendor == null)
        return Result<Vendor>.Failure("Not found");

    // Log cross-region access
    if (vendor.DataRegion != userRegion)
    {
        _logger.LogWarning("Cross-region data access", new {
            vendorId = id,
            vendorRegion = vendor.DataRegion,
            userRegion,
            requiresConsent = true
        });
    }

    return Result<Vendor>.Success(vendor);
}
```

---

## Compliance Mapping

| Region | Regulation | Requirements |
|--------|------------|--------------|
| EU | GDPR | Data stays in EU, consent required |
| US | Various | State-specific (CCPA, etc.) |
| UK | UK GDPR | Separate from EU post-Brexit |
| APAC | PDPA/PIPL | Country-specific rules |

---

## Reference

- **Configuration**: `appsettings.json` → DataResidency
- **Golden Rules**: Section 10.4 Pattern 17
