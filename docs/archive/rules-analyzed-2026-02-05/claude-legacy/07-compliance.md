# Advanced Compliance Patterns (UN/GDPR)

---

## 7.1 Pattern Overview

| Pattern | Description | Priority |
|---------|-------------|----------|
| 14 | Data Residency & Compliance | HIGH |
| 15 | Multi-Tenancy | HIGH |
| 16 | Audit Trail & Temporal | HIGH |
| 17 | Data Privacy & Masking (GDPR) | HIGH |

---

## 7.2 Pattern 14: Data Residency

### Purpose
Store data in specific geographic regions per compliance requirements.

### Entity Extension
```csharp
public class Vendor
{
    public string DataResidencyRegion { get; set; } // "EU", "US", "APAC", "GLOBAL"
    public string ComplianceFramework { get; set; } // "GDPR", "CCPA", "UN-DPA"
    public bool RequiresLocalStorage { get; set; }
}
```

### Regional Repository
```csharp
public class RegionalRepositoryFactory
{
    public IRepository<T> GetRepositoryForRegion(string region)
    {
        return region switch
        {
            "EU" => _euRepository,     // EU database
            "US" => _usRepository,     // US database
            "APAC" => _apacRepository, // APAC database
            _ => _globalRepository     // Default
        };
    }
}
```

### Configuration
```json
{
  "ConnectionStrings": {
    "Default": "...",
    "EU": "Server=eu-sql.database.windows.net;...",
    "US": "Server=us-sql.database.windows.net;...",
    "APAC": "Server=apac-sql.database.windows.net;..."
  }
}
```

---

## 7.3 Pattern 15: Multi-Tenancy

### Purpose
Isolate data between different UN agencies/organizations.

### ITenantContext
```csharp
public interface ITenantContext
{
    Guid TenantId { get; }
    string TenantName { get; }
    string TenantCode { get; } // "UNDP", "UNICEF", "WHO"
}
```

### Tenant-Aware Entities
```csharp
public interface ITenantEntity
{
    Guid TenantId { get; set; }
}

public class Vendor : ITenantEntity
{
    public Guid TenantId { get; set; }
    // ... other fields
}
```

### Automatic Filtering
```csharp
public class TenantRepository<T> : IRepository<T> where T : ITenantEntity
{
    private readonly ITenantContext _tenantContext;

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _context.Set<T>()
            .Where(e => e.TenantId == _tenantContext.TenantId)
            .ToListAsync();
    }
}
```

### Tenant Resolution Middleware
```csharp
public class TenantResolutionMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Extract from subdomain: undp.vendorportal.com
        var host = context.Request.Host.Host;
        var tenantCode = host.Split('.')[0]; // "undp"

        // Or from header: X-Tenant-Code: UNDP
        tenantCode = context.Request.Headers["X-Tenant-Code"];

        // Set tenant context
        _tenantContext.SetTenant(tenantCode);

        await _next(context);
    }
}
```

---

## 7.4 Pattern 17: Data Privacy & Masking (GDPR)

### PII Classification
```csharp
[AttributeUsage(AttributeTargets.Property)]
public class PiiAttribute : Attribute
{
    public PiiLevel Level { get; set; } // High, Medium, Low
    public bool RequiresEncryption { get; set; }
    public bool RequiresMasking { get; set; }
}

public class Vendor
{
    [Pii(Level = PiiLevel.High, RequiresEncryption = true)]
    public string TaxId { get; set; }

    [Pii(Level = PiiLevel.Medium, RequiresMasking = true)]
    public string Email { get; set; }

    [Pii(Level = PiiLevel.High, RequiresEncryption = true)]
    public string BankAccount { get; set; }
}
```

### Data Masking Service
```csharp
public class DataMaskingService
{
    public string MaskEmail(string email)
    {
        // john.doe@example.com → j***@example.com
        var parts = email.Split('@');
        return $"{parts[0][0]}***@{parts[1]}";
    }

    public string MaskTaxId(string taxId)
    {
        // 123-45-6789 → ***-**-6789
        return $"***-**-{taxId.Substring(taxId.Length - 4)}";
    }

    public string MaskBankAccount(string account)
    {
        // 1234567890 → ******7890
        return $"******{account.Substring(account.Length - 4)}";
    }
}
```

### Field-Level Encryption
```csharp
public class FieldEncryptionService
{
    public string Encrypt(string plainText)
    {
        var key = await _keyVault.GetEncryptionKey();
        return EncryptAes256(plainText, key);
    }

    public string Decrypt(string cipherText)
    {
        var key = await _keyVault.GetEncryptionKey();
        return DecryptAes256(cipherText, key);
    }
}
```

---

## 7.5 GDPR Rights Endpoints

```csharp
[ApiController]
[Route("api/gdpr")]
public class GdprController : ControllerBase
{
    // Right to Access (Article 15)
    [HttpGet("data-export/{vendorId}")]
    public async Task<IActionResult> ExportPersonalData(Guid vendorId)
    {
        // Export all vendor data as JSON
    }

    // Right to Rectification (Article 16)
    [HttpPut("data-correction/{vendorId}")]
    public async Task<IActionResult> CorrectPersonalData(
        Guid vendorId, [FromBody] CorrectionRequest request)
    {
        // Update incorrect data
    }

    // Right to Erasure - Right to be Forgotten (Article 17)
    [HttpDelete("data-deletion/{vendorId}")]
    public async Task<IActionResult> DeletePersonalData(Guid vendorId)
    {
        // Anonymize or delete vendor data
        // Keep audit trail but remove PII
    }

    // Right to Data Portability (Article 20)
    [HttpGet("data-portability/{vendorId}")]
    public async Task<IActionResult> ExportPortableData(Guid vendorId)
    {
        // Export in machine-readable format (JSON/CSV)
    }

    // Right to Restrict Processing (Article 18)
    [HttpPut("processing-restriction/{vendorId}")]
    public async Task<IActionResult> RestrictProcessing(Guid vendorId)
    {
        // Mark vendor as restricted
    }

    // Right to Object (Article 21)
    [HttpPost("processing-objection/{vendorId}")]
    public async Task<IActionResult> ObjectToProcessing(
        Guid vendorId, [FromBody] ObjectionRequest request)
    {
        // Record objection
    }

    // Right to Not Be Subject to Automated Decision (Article 22)
    [HttpPost("automated-decision-review/{vendorId}")]
    public async Task<IActionResult> RequestHumanReview(Guid vendorId)
    {
        // Flag for human review
    }
}
```

---

## 7.6 Data Retention Policy

```csharp
public class DataRetentionService
{
    public async Task EnforceRetentionPolicy()
    {
        // Delete vendors inactive for > 7 years (GDPR requirement)
        var cutoffDate = DateTime.UtcNow.AddYears(-7);
        var inactiveVendors = await _context.Vendors
            .Where(v => v.LastActivityAt < cutoffDate)
            .ToListAsync();

        foreach (var vendor in inactiveVendors)
        {
            await AnonymizeVendor(vendor);
            _logger.LogInformation("Vendor anonymized for GDPR retention",
                ("VendorId", vendor.Id));
        }
    }

    private async Task AnonymizeVendor(Vendor vendor)
    {
        vendor.LegalName = "[ANONYMIZED]";
        vendor.TaxId = null;
        vendor.Email = null;
        vendor.Phone = null;
        vendor.Address = null;
        vendor.IsAnonymized = true;
        vendor.AnonymizedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
```

---

## 7.7 PII Masking in Logs

```csharp
public class PiiMaskingLogger : ILogger
{
    private static readonly Regex EmailRegex = new(@"\b[\w.-]+@[\w.-]+\.\w+\b");
    private static readonly Regex SsnRegex = new(@"\b\d{3}-\d{2}-\d{4}\b");
    private static readonly Regex CreditCardRegex = new(@"\b\d{4}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}\b");

    public void LogInformation(string message, params object[] args)
    {
        var maskedMessage = MaskPii(message);
        _innerLogger.LogInformation(maskedMessage, args);
    }

    private string MaskPii(string message)
    {
        message = EmailRegex.Replace(message, m => MaskEmail(m.Value));
        message = SsnRegex.Replace(message, "***-**-****");
        message = CreditCardRegex.Replace(message, "****-****-****-****");
        return message;
    }
}
```

---

## 7.8 Compliance Checklist

### GDPR (7 Rights)
- [ ] Right to Access
- [ ] Right to Rectification
- [ ] Right to Erasure
- [ ] Right to Restrict Processing
- [ ] Right to Data Portability
- [ ] Right to Object
- [ ] Right to Not Be Subject to Automated Decisions

### Data Residency
- [ ] EU data in EU databases
- [ ] US data in US databases
- [ ] Region enforcement (cannot move EU data to US)

### Multi-Tenancy
- [ ] Tenant isolation verified
- [ ] No cross-tenant data leaks
- [ ] Tenant context propagated

### PII Protection
- [ ] Sensitive fields encrypted at rest
- [ ] PII masked in logs
- [ ] Retention policy enforced
