# VendorMdm.Core.Framework

**Version**: 1.0.0  
**Status**: ✅ Foundation Complete  
**Target**: .NET 8.0

---

## 🎯 Purpose

`VendorMdm.Core.Framework` is the **shared foundation** for all MDM applications. It provides:

- 🔐 **Security**: Authentication, Authorization, Role Management
- 🛡️ **Resilience**: Circuit Breaker, Retry, Timeout (Polly)
- 📊 **Logging**: Structured logging with Serilog + Application Insights
- 🏥 **Health Checks**: Database, Blob Storage, Service Bus
- 📁 **File Storage**: Azure Blob abstraction
- 🔍 **Observability**: Distributed Tracing (OpenTelemetry) - Coming Soon
- 🧬 **Ontology Framework**: Domain concepts - Already Exists

---

## 🚀 Quick Start

### 1. Add Package Reference

```xml
<ItemGroup>
  <ProjectReference Include="../VendorMdm.Core.Framework/VendorMdm.Core.Framework.csproj" />
</ItemGroup>
```

### 2. Add Core Services (ONE LINE)

```csharp
// Program.cs
using VendorMdm.Core.Framework.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ✨ ONE LINE to add ALL core services
builder.Services.AddCoreFramework(
    builder.Configuration,
    "VendorMDM", // App name
    options =>
    {
        options.EnableDistributedTracing = true;
        options.EnableSecurity = true;
        options.EnableDatabaseHealthCheck = true;
        options.EnableBlobStorageHealthCheck = true;
        options.EnableServiceBusHealthCheck = true;
    });

var app = builder.Build();
```

### 3. Use Core Services

```csharp
// Example: Using Structured Logger
public class VendorService
{
    private readonly IStructuredLogger _logger;
    
    public VendorService(IStructuredLogger logger)
    {
        _logger = logger;
    }
    
    public async Task CreateVendorAsync(Vendor vendor)
    {
        using (_logger.BeginOperation("CreateVendor", ("VendorId", vendor.Id)))
        {
            _logger.LogInformation(
                "Creating vendor",
                ("LegalName", vendor.LegalName),
                ("VendorType", vendor.VendorType));
            
            // ... business logic ...
            
            _logger.LogInformation("Vendor created successfully");
        }
    }
}
```

```csharp
// Example: Using Resilience Policies
public class SapIntegrationService
{
    private readonly IResiliencePolicy _resilience;
    private readonly HttpClient _httpClient;
    
    public async Task SyncVendorAsync(Vendor vendor)
    {
        // Use pre-configured HTTP resilience policy
        await _resilience.ExecuteAsync(
            CorePolicyRegistry.HttpRetry,
            async () =>
            {
                var response = await _httpClient.PostAsJsonAsync("/sap/vendors", vendor);
                response.EnsureSuccessStatusCode();
            });
    }
}
```

```csharp
// Example: Using Authorization
public class VendorController : ControllerBase
{
    private readonly IAuthorizationService _authz;
    
    [HttpPost]
    public async Task<IActionResult> CreateVendor([FromBody] VendorDto dto)
    {
        var userId = User.GetUserId();
        
        // Check permission
        if (!await _authz.HasPermissionAsync(userId, "vendors.create", "VendorMDM"))
        {
            return Forbid();
        }
        
        // ... create vendor ...
    }
}
```

---

## 📦 What's Included

### 🔐 Security

#### Interfaces
- `IAuthenticationService` - Multi-channel authentication (AzureAD, MagicLink, LocalStrong)
- `IAuthorizationService` - Role & permission-based authorization

#### Core Roles
- `SystemAdmin` - Full access across ALL apps
- `AppAdmin` - Full access within ONE app
- `Viewer` - Read-only access
- `Editor` - Can create and edit
- `Approver` - Can approve/reject
- `Auditor` - Can view audit logs

#### Core Permissions
- `users.view`, `users.create`, `users.edit`, `users.delete`
- `roles.view`, `roles.grant`, `roles.revoke`
- `audit.view`, `audit.export`
- `config.view`, `config.edit`

### 🛡️ Resilience (Polly)

#### Pre-configured Policies
- **HttpRetryPolicy**: 3 retries with exponential backoff
- **HttpCircuitBreakerPolicy**: Opens after 5 failures, 30s break
- **DatabaseRetryPolicy**: Handles transient SQL errors
- **ServiceBusRetryPolicy**: 5 retries for Service Bus
- **DefaultTimeoutPolicy**: 30s timeout

#### Usage
```csharp
// Option 1: Use policy directly
var result = await CorePolicyRegistry.HttpResilientPolicy.ExecuteAsync(async () =>
{
    return await _httpClient.GetAsync("/api/data");
});

// Option 2: Use policy service
await _resilience.ExecuteAsync(CorePolicyRegistry.HttpRetry, async () =>
{
    await _httpClient.PostAsync("/api/data", content);
});
```

### 📊 Logging (Serilog)

#### Features
- Structured logging with contextual properties
- Operation scopes with automatic timing
- JSON formatting
- Application Insights integration
- Console output for development

#### Usage
```csharp
// Simple logging
_logger.LogInformation("Vendor created", ("VendorId", vendorId));

// With scope
using (_logger.BeginScope(("UserId", userId), ("TenantId", tenantId)))
{
    _logger.LogInformation("Processing request");
    // All logs in this scope will include UserId and TenantId
}

// Operation scope (auto-timing)
using (_logger.BeginOperation("ProcessPayment", ("Amount", amount)))
{
    // ... business logic ...
    // Automatically logs: "Operation started: ProcessPayment"
    // Automatically logs: "Operation completed: ProcessPayment (Duration: 1234ms)"
}
```

### 🏥 Health Checks

#### Endpoints
- `/health/live` - Liveness probe (Kubernetes)
- `/health/ready` - Readiness probe (Kubernetes)

#### Checks Included
- Database connectivity (SQL Server)
- Blob Storage connectivity (Azure)
- Service Bus connectivity (Azure)

### 📁 File Storage

#### Interface
```csharp
public interface IFileStorageService
{
    Task<Result<string>> UploadAsync(Stream fileStream, string fileName, string containerName);
    Task<Result<Stream>> DownloadAsync(string blobPath);
    Task<Result> DeleteAsync(string blobPath);
    Task<Result<IEnumerable<string>>> ListAsync(string containerName, string prefix = "");
    Task<Result<FileMetadata>> GetMetadataAsync(string blobPath);
    Task<Result<string>> GenerateDownloadUrlAsync(string blobPath, TimeSpan expiresIn);
}
```

---

## 🛡️ Governance

### Protection Rules

**❌ FORBIDDEN** (Build will fail):
1. Apps CANNOT implement Core interfaces directly
2. Apps CANNOT inherit from Core classes
3. Apps CANNOT modify Core constants

**✅ ALLOWED** (Extension pattern):
1. Apps CAN create extension methods
2. Apps CAN create adapters/wrappers
3. Apps CAN compose Core services

### How to Extend

See [CONTRIBUTING.md](./CONTRIBUTING.md) for detailed examples.

**Example: Extension Method**
```csharp
public static class AuthenticationExtensions
{
    public static async Task<Result<VendorData>> GetVendorDataAsync(
        this IAuthenticationService auth,
        Guid vendorId,
        SqlDbContext context)
    {
        // App-specific logic
        var vendor = await context.Vendors.FindAsync(vendorId);
        return Result.Ok(new VendorData { ... });
    }
}
```

---

## 📚 Documentation

- [GOVERNANCE.md](./GOVERNANCE.md) - Protection rules, change process
- [CONTRIBUTING.md](./CONTRIBUTING.md) - How to extend Core safely
- [ARCHITECTURE.md](./ARCHITECTURE.md) - Design principles (Coming Soon)

---

## 🔧 Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=...;"
  },
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=..."
  },
  "Azure": {
    "BlobStorage": {
      "ConnectionString": "DefaultEndpointsProtocol=https;..."
    },
    "ServiceBus": {
      "ConnectionString": "Endpoint=sb://...;..."
    }
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

---

## 🧪 Testing

### Unit Tests

```csharp
public class VendorServiceTests
{
    [Fact]
    public async Task CreateVendor_Success_LogsInformation()
    {
        // Arrange
        var logger = new Mock<IStructuredLogger>();
        var service = new VendorService(logger.Object);
        
        // Act
        await service.CreateVendorAsync(vendor);
        
        // Assert
        logger.Verify(x => x.LogInformation(
            "Vendor created successfully",
            It.IsAny<(string, object)[]>()), Times.Once);
    }
}
```

---

## 📊 Dependencies

```xml
<!-- Core -->
<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" Version="8.0.0" />

<!-- Resilience -->
<PackageReference Include="Polly" Version="8.2.0" />
<PackageReference Include="Polly.Extensions.Http" Version="3.0.0" />

<!-- Logging -->
<PackageReference Include="Serilog" Version="3.1.1" />
<PackageReference Include="Serilog.Sinks.ApplicationInsights" Version="4.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />

<!-- Health Checks -->
<PackageReference Include="AspNetCore.HealthChecks.SqlServer" Version="7.0.0" />
<PackageReference Include="AspNetCore.HealthChecks.AzureStorage" Version="7.0.0" />
<PackageReference Include="AspNetCore.HealthChecks.AzureServiceBus" Version="7.0.0" />
```

---

## 🚀 Roadmap

### ✅ Week 0: Foundation (COMPLETE)
- [x] Governance documentation
- [x] Security interfaces
- [x] Resilience policies
- [x] Structured logging
- [x] Health checks
- [x] File storage interface
- [x] Service collection extensions

### 🔄 Week 1: Implementations (IN PROGRESS)
- [ ] JWT Authentication Service
- [ ] Role-Based Authorization Service
- [ ] Azure Blob Storage Service
- [ ] Health Check Service Implementation

### 📅 Week 2: Observability
- [ ] OpenTelemetry integration
- [ ] Distributed tracing
- [ ] Metrics collection
- [ ] TraceId propagation

### 📅 Week 3: Migration
- [ ] Migrate VendorMDM to Core.Framework
- [ ] Remove duplicate code
- [ ] Update all services to use Core

---

## 🤝 Contributing

See [CONTRIBUTING.md](./CONTRIBUTING.md) for detailed guidelines.

**Quick Summary**:
1. Create extension methods for app-specific logic
2. Use adapters/wrappers to simplify Core interfaces
3. Compose Core services for complex operations
4. Propose changes via ADR for Core modifications

---

## 📄 License

Internal use only - Vendor MDM Portal

---

## 🆘 Support

**Questions?**
- Create GitHub Discussion
- Tag `@architecture-team`

**Found a bug?**
- Create GitHub Issue
- Tag `bug` and `core`

---

**Built with ❤️ for maximum quality and developer experience**
