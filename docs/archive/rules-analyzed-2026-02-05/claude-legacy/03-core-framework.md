# Core.Framework Governance

---

## 3.1 ONE LINE Integration

```csharp
// In Program.cs
services.AddCoreFramework(configuration, "VendorMDM");
```

This single line adds:
- Serilog structured logging
- Polly resilience policies
- Health checks
- File storage abstraction
- OpenTelemetry (when enabled)

---

## 3.2 Protection Rules

### FORBIDDEN Actions
```csharp
// ❌ Apps CANNOT implement Core interfaces directly
public class MyAuthService : IAuthenticationService { }

// ❌ Apps CANNOT inherit from Core classes
public class MyLogger : StructuredLogger { }

// ❌ Apps CANNOT modify Core constants
CoreRoles.SystemAdmin = "NewValue";

// ❌ Apps CANNOT add dependencies to Core
```

### ALLOWED Actions (Extension Pattern)
```csharp
// ✅ Create extension methods
public static class AuthenticationExtensions
{
    public static async Task<Result<VendorData>> GetVendorDataAsync(
        this IAuthenticationService auth, Guid vendorId)
    {
        // App-specific logic
    }
}

// ✅ Create adapters/wrappers
public class VendorAuthAdapter
{
    private readonly IAuthenticationService _auth;

    public async Task<Result> AuthenticateVendorAsync(string email)
    {
        return await _auth.AuthenticateAsync(email, "MagicLink");
    }
}

// ✅ Compose Core services
public class VendorService
{
    private readonly IAuthenticationService _auth;
    private readonly IAuditLogService _audit;
    private readonly IStructuredLogger _logger;
}

// ✅ Configure via options
services.AddCoreFramework(configuration, "VendorMDM", options =>
{
    options.EnableDistributedTracing = true;
    options.LogLevel = LogLevel.Information;
});
```

---

## 3.3 Core Change Process

### When to Modify Core
✅ Multiple apps need the functionality
✅ Cross-cutting concern (security, logging)
✅ Stable and well-understood
✅ Benefits ALL apps

### How to Propose Changes
1. **Create ADR** (Architecture Decision Record)
   ```markdown
   # ADR-XXX: Add Feature to Core

   ## Status
   Proposed

   ## Context
   All apps need this functionality.

   ## Decision
   Add X to Core.Framework.

   ## Consequences
   - Breaking change (requires version bump)
   - All apps must update
   ```

2. **Submit PR** (requires 2 approvals from Architecture Team)

3. **Version Bump** (Semantic Versioning)
   - Patch (1.0.X): Bug fixes
   - Minor (1.X.0): New features, backward compatible
   - Major (X.0.0): Breaking changes

4. **Migration Guide** for breaking changes

---

## 3.4 Core Services Reference

### IStructuredLogger
```csharp
// Simple logging
_logger.LogInformation("Vendor created", ("VendorId", vendorId));

// Error logging
_logger.LogError(ex, "Operation failed", ("Context", context));

// Scope (all logs include properties)
using (_logger.BeginScope(("UserId", userId)))
{
    _logger.LogInformation("Processing");
}

// Operation timing (auto start/end)
using (_logger.BeginOperation("ProcessPayment", ("Amount", amount)))
{
    // Business logic
}
```

### CorePolicyRegistry (Polly)
```csharp
// Retry policy
await CorePolicyRegistry.HttpRetryPolicy.ExecuteAsync(async () =>
{
    await _httpClient.PostAsync(url, content);
});

// Circuit breaker + Retry
await CorePolicyRegistry.HttpResilientPolicy.ExecuteAsync(async () =>
{
    await _externalService.CallAsync();
});

// Database retry
await CorePolicyRegistry.DatabaseRetryPolicy.ExecuteAsync(async () =>
{
    await _context.SaveChangesAsync();
});
```

### IHealthCheckService
```csharp
// Check all dependencies
var status = await _healthCheck.CheckAllAsync();
// Returns: Healthy, Degraded, or Unhealthy

// Individual checks
var dbStatus = await _healthCheck.CheckDatabaseAsync();
var blobStatus = await _healthCheck.CheckBlobStorageAsync();
var busStatus = await _healthCheck.CheckServiceBusAsync();
```

### IFileStorageService
```csharp
// Upload
await _storage.UploadAsync(stream, "document.pdf", "vendor-docs");

// Download
var stream = await _storage.DownloadAsync("vendor-docs/document.pdf");

// Generate SAS URL
var url = await _storage.GenerateDownloadUrlAsync("path/file.pdf", TimeSpan.FromHours(1));

// List files
var files = await _storage.ListAsync("vendor-docs/");
```

### IAuditLogService
```csharp
await _auditLog.LogAsync(
    entityType: "Vendor",
    entityId: vendor.Id,
    action: "Created",
    oldValues: null,
    newValues: vendor,
    reason: "User request");
```

---

## 3.5 Directory Structure

```
VendorMdm.Core.Framework/
├── Security/
│   ├── Authentication/
│   │   ├── IAuthenticationService.cs
│   │   └── JwtAuthenticationService.cs
│   ├── Authorization/
│   │   ├── IAuthorizationService.cs
│   │   └── RoleBasedAuthorizationService.cs
│   └── Roles/
│       └── CoreRoles.cs
├── Resilience/
│   ├── IResiliencePolicy.cs
│   ├── ResiliencePolicyService.cs
│   └── CorePolicyRegistry.cs
├── Logging/
│   ├── IStructuredLogger.cs
│   └── SerilogStructuredLogger.cs
├── HealthChecks/
│   ├── IHealthCheckService.cs
│   └── CoreHealthCheckService.cs
├── FileSystem/
│   ├── IFileStorageService.cs
│   └── AzureBlobStorageService.cs
├── Observability/
│   ├── DistributedTracingService.cs
│   └── MetricsService.cs
├── Extensions/
│   └── ServiceCollectionExtensions.cs
├── GOVERNANCE.md
├── CONTRIBUTING.md
└── README.md
```
