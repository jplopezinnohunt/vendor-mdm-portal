# API Versioning Standard

**Category**: Architecture & Design
**Pattern #**: 5
**Status**: MANDATORY
**Priority**: 🟠 IMPORTANT

---

## Definition

All APIs MUST support versioning to enable backwards-compatible evolution without breaking existing clients.

---

## Rules

1. **ALWAYS** version APIs from day one
2. **NEVER** make breaking changes without version increment
3. **ALWAYS** support at least N-1 version (deprecation period)
4. **NEVER** remove API versions without 6-month notice

---

## Versioning Strategy

### URL Path Versioning (PRIMARY)

```
/api/v1/vendors
/api/v2/vendors
```

**Why**: Explicit, cacheable, easy to route, visible in logs.

### Header Versioning (SECONDARY)

```http
GET /api/vendors
Api-Version: 2024-01-15
X-Api-Version: 1.0
```

**Use for**: Minor version increments within major version.

---

## Implementation

### Controller Setup

```csharp
// ✅ CORRECT: Versioned controller
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class VendorsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetVendors() { }
}

// ✅ CORRECT: Multiple versions in same controller
[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class VendorsController : ControllerBase
{
    [HttpGet]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetVendorsV1() { }

    [HttpGet]
    [MapToApiVersion("2.0")]
    public async Task<IActionResult> GetVendorsV2() { }
}
```

### Program.cs Configuration

```csharp
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version")
    );
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
```

### Version Header Middleware

```csharp
public class ApiVersionHeaderMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Api-Version"] = "1.0";
            context.Response.Headers["X-Api-Supported-Versions"] = "1.0, 2.0";
            context.Response.Headers["X-Api-Deprecated-Versions"] = "";
            return Task.CompletedTask;
        });
        await next(context);
    }
}
```

---

## Version Lifecycle

```
┌─────────────┐    ┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│   Preview   │───>│   Current   │───>│ Deprecated  │───>│   Retired   │
│  (v2-beta)  │    │    (v2)     │    │    (v1)     │    │  (removed)  │
└─────────────┘    └─────────────┘    └─────────────┘    └─────────────┘
     3 months          Active           6 months           Archive
```

| Phase | Duration | Support Level |
|-------|----------|---------------|
| **Preview** | 1-3 months | Best effort, may change |
| **Current** | Until next major | Full support, bug fixes |
| **Deprecated** | 6 months minimum | Security fixes only |
| **Retired** | - | No support, returns 410 Gone |

---

## Breaking vs Non-Breaking Changes

### Non-Breaking (No Version Bump)

- ✅ Adding new optional fields to response
- ✅ Adding new endpoints
- ✅ Adding new optional query parameters
- ✅ Relaxing validation rules
- ✅ Adding new enum values (if client handles unknown)

### Breaking (Requires Version Bump)

- ❌ Removing fields from response
- ❌ Renaming fields
- ❌ Changing field types
- ❌ Removing endpoints
- ❌ Adding required parameters
- ❌ Tightening validation rules
- ❌ Changing response structure

---

## Deprecation Process

### 1. Announce Deprecation

```csharp
[ApiVersion("1.0", Deprecated = true)]
[Route("api/v{version:apiVersion}/[controller]")]
public class VendorsV1Controller : ControllerBase { }
```

### 2. Add Sunset Header

```http
Sunset: Sat, 01 Jul 2026 00:00:00 GMT
Deprecation: true
Link: </api/v2/vendors>; rel="successor-version"
```

### 3. Log Usage

```csharp
_logger.LogWarning("Deprecated API called", new {
    version = "1.0",
    endpoint = "/api/v1/vendors",
    clientId = context.User.Identity.Name
});
```

### 4. Return 410 Gone After Sunset

```csharp
if (apiVersion.MajorVersion < 1)
{
    return StatusCode(410, new {
        error = "API_VERSION_RETIRED",
        message = "This API version has been retired",
        successor = "/api/v2/vendors"
    });
}
```

---

## DTO Versioning

```csharp
// V1 DTO
public class VendorDtoV1
{
    public Guid Id { get; set; }
    public string Name { get; set; }  // Renamed in V2
}

// V2 DTO
public class VendorDtoV2
{
    public Guid Id { get; set; }
    public string LegalName { get; set; }  // More specific
    public string TradingName { get; set; }  // New field
}

// Mapping
public static class VendorMappingExtensions
{
    public static VendorDtoV1 ToV1Dto(this Vendor vendor) =>
        new() { Id = vendor.Id, Name = vendor.LegalName };

    public static VendorDtoV2 ToV2Dto(this Vendor vendor) =>
        new() { Id = vendor.Id, LegalName = vendor.LegalName, TradingName = vendor.TradingName };
}
```

---

## Swagger/OpenAPI Configuration

```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Vendor MDM API",
        Version = "v1",
        Description = "DEPRECATED - Use v2"
    });
    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "Vendor MDM API",
        Version = "v2",
        Description = "Current version"
    });
});

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "V2 (Current)");
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "V1 (Deprecated)");
});
```

---

## Anti-Patterns

❌ No versioning from start (retrofit is painful)
❌ Version in query string only (`?v=1`)
❌ Breaking changes without version bump
❌ Removing versions without notice
❌ Too many active versions (max 3)
❌ Different versioning per endpoint

---

## Agent Behavior

**Before Adding Endpoint**:
1. ✅ Check if change is breaking
2. ✅ If breaking, create new version
3. ✅ Update Swagger docs
4. ✅ Add deprecation notice to old version

**Before Removing Endpoint**:
1. ✅ Mark as deprecated for 6 months
2. ✅ Log usage to identify affected clients
3. ✅ Notify clients via Sunset header
4. ✅ Return 410 after sunset date

---

## Reference

- **Middleware**: `Api/Middleware/ApiVersionHeaderMiddleware.cs`
- **NuGet**: `Microsoft.AspNetCore.Mvc.Versioning`
- **Golden Rules**: Section 4, Category 1 (Architecture & Design)
