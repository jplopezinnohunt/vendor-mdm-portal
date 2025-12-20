---
description: Service Management - Mock/Real Implementation Standard
---

# Service Management Standard

**Purpose:** Define the canonical process for adding Mock and Real service implementations with progressive deployment support.

**When to use:** Any time you need to integrate with an external system (SAP, Azure AD, SendGrid, etc.)

---

## Core Principles

1. **All integrations go through Canonical API** - Frontend never calls external systems directly
2. **Interface-based design** - All services implement an interface
3. **Mock services ship to production** - Not just for local dev
4. **Configuration-driven selection** - Toggle Mock↔Real via `appsettings.json`
5. **Progressive activation** - Activate services one at a time
6. **Same API contract** - Mock and Real implementations are interchangeable

---

## Standard Service Architecture

Every external system integration has **3 implementations**:

```
ISomeService (Interface)
├── SomeSimulationService (Mock - always implement first)
├── SomeRealService (Real implementation)
└── SomeAlternativeService (Optional alternative real implementation)
```

**Example: SAP**
```
ISapVendorService
├── SapVendorSimulationService (Mock with in-memory data)
├── SapVendorRfcService (Real - Direct SAP NCo)
└── SapVendorMouvProxyService (Real - Via MoUV API)
```

---

## Step-by-Step Process

### Step 1: Define the Interface

**Location:** `/backend/VendorMdm.Api/Services/I{ServiceName}.cs`

```csharp
namespace VendorMdm.Api.Services;

/// <summary>
/// Interface for {System} integration
/// Allows Mock and Real implementations to be swapped via configuration
/// </summary>
public interface I{ServiceName}
{
    // Define all operations
    Task<Result> DoSomethingAsync(Request request);
}
```

**Rules:**
- ✓ Use async methods (Task<T>)
- ✓ Accept request DTOs, return response DTOs
- ✓ Document expected behavior in XML comments
- ✓ Think about what Frontend needs, not what backend system provides

---

### Step 2: Create Models

**Location:** `/backend/VendorMdm.Shared/Models/{System}/{ModelName}.cs`

```csharp
namespace VendorMdm.Shared.Models.{System};

public class {Operation}Request
{
    // Input parameters
}

public class {Operation}Response
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    // Result data
}
```

**Rules:**
- ✓ Separate file per logical group (Search, Get, Update, Validation)
- ✓ Always include Success + ErrorMessage in responses
- ✓ Use nullable reference types appropriately
- ✓ Document fields with XML comments

---

### Step 3: Implement Mock Service (REQUIRED - Always First)

**Location:** `/backend/VendorMdm.Api/Services/{ServiceName}SimulationService.cs`

```csharp
namespace VendorMdm.Api.Services;

/// <summary>
/// MOCK implementation of {Service}
/// Used for local development AND production deployment until real system is connected
/// </summary>
public class {ServiceName}SimulationService : I{ServiceName}
{
    private readonly ILogger<{ServiceName}SimulationService> _logger;
    private readonly List<MockData> _mockData;

    public {ServiceName}SimulationService(ILogger<{ServiceName}SimulationService> logger)
    {
        _logger = logger;
        _mockData = SeedMockData();
    }

    public async Task<Result> DoSomethingAsync(Request request)
    {
        _logger.LogInformation("SIMULATION: {Operation} called with {Params}", 
            nameof(DoSomethingAsync), request);

        // Simulate network latency (optional)
        await Task.Delay(50);

        // Mock business logic
        var result = ProcessMockRequest(request);

        return result;
    }

    private List<MockData> SeedMockData()
    {
        // Create realistic test data (50-100 records minimum)
        return new List<MockData> { /* ... */ };
    }
}
```

**Mock Service Requirements:**
- ✓ Log all operations with "SIMULATION:" prefix
- ✓ Seed realistic test data (not just 1-2 records)
- ✓ Implement full CRUD if applicable
- ✓ Add optional latency simulation
- ✓ Return realistic responses (success + errors)
- ✓ Keep state in-memory (stateless is preferred)

---

### Step 4: Implement Real Service (When Ready)

**Location:** `/backend/VendorMdm.Api/Services/{ServiceName}RealService.cs`

```csharp
namespace VendorMdm.Api.Services;

/// <summary>
/// REAL implementation of {Service}
/// Connects to actual {System} via {Protocol/SDK}
/// </summary>
public class {ServiceName}RealService : I{ServiceName}
{
    private readonly ILogger<{ServiceName}RealService> _logger;
    private readonly IConfiguration _configuration;
    // Add dependencies for real connection (HTTP client, SDK, etc.)

    public {ServiceName}RealService(
        ILogger<{ServiceName}RealService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<Result> DoSomethingAsync(Request request)
    {
        _logger.LogInformation("REAL: {Operation} called", nameof(DoSomethingAsync));

        try
        {
            // Call real system
            var response = await CallExternalSystemAsync(request);
            return MapResponse(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling {System}", "{SystemName}");
            return new Result 
            { 
                Success = false, 
                ErrorMessage = ex.Message 
            };
        }
    }
}
```

**Real Service Requirements:**
- ✓ Log all operations with "REAL:" prefix
- ✓ Handle exceptions gracefully
- ✓ Return errors in same format as Mock
- ✓ Use configuration for connection details
- ✓ Implement retry logic if appropriate
- ✓ Add circuit breaker for resilience (optional)

---

### Step 5: Add Configuration

**Location:** `/backend/VendorMdm.Api/appsettings.json`

```json
{
  "Services": {
    "{ServiceName}": {
      "Comment": "Toggle between Mock and Real implementation",
      "UseMock": true,
      "RealProvider": "{ProviderName}",
      "MockSettings": {
        "SimulateLatency": false,
        "LatencyMs": 100
      },
      "RealSettings": {
        "BaseUrl": "https://external-system.com/api",
        "ApiKey": "@Microsoft.KeyVault(SecretUri=https://...)",
        "Timeout": 30
      }
    }
  }
}
```

**Configuration Rules:**
- ✓ Always default `UseMock: true`
- ✓ Group settings by Mock vs Real
- ✓ Use KeyVault references for secrets
- ✓ Document purpose in comments

---

### Step 6: Register Services

**Location:** `/backend/VendorMdm.Api/Program.cs`

```csharp
// {ServiceName} Service
var use{ServiceName}Mock = builder.Configuration.GetValue<bool>(
    "Services:{ServiceName}:UseMock", true);

if (use{ServiceName}Mock)
{
    builder.Services.AddScoped<I{ServiceName}, {ServiceName}SimulationService>();
    Console.WriteLine("✓ {ServiceName}: MOCK (Simulation)");
}
else
{
    builder.Services.AddScoped<I{ServiceName}, {ServiceName}RealService>();
    Console.WriteLine("✓ {ServiceName}: REAL ({Provider})");
}
```

**Registration Rules:**
- ✓ Add after similar service registrations
- ✓ Default to Mock if config missing
- ✓ Log which implementation is active
- ✓ Use consistent lifetime (AddScoped for most services)

---

### Step 7: Create Controller (If Needed)

**Location:** `/backend/VendorMdm.Api/Controllers/{ServiceName}Controller.cs`

```csharp
[ApiController]
[Route("api/{service-name}")]
[Produces("application/json")]
public class {ServiceName}Controller : ControllerBase
{
    private readonly I{ServiceName} _service;
    private readonly ILogger<{ServiceName}Controller> _logger;

    public {ServiceName}Controller(
        I{ServiceName} service,
        ILogger<{ServiceName}Controller> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Operation description
    /// </summary>
    [HttpPost("operation")]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Response>> Operation([FromBody] Request request)
    {
        try
        {
            var result = await _service.DoSomethingAsync(request);
            
            if (!result.Success)
                return BadRequest(new { error = result.ErrorMessage });
            
            return Ok(result);
        }
        catch (NotImplementedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, 
                new { error = "Real service not configured", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}", nameof(Operation));
            return BadRequest(new { error = ex.Message });
        }
    }
}
```

**Controller Rules:**
- ✓ Inject interface, not concrete implementation
- ✓ Add Swagger documentation
- ✓ Handle NotImplementedException (mock → real transition)
- ✓ Return consistent error format
- ✓ Log errors

---

### Step 8: Write Tests

**Location:** `/backend/VendorMdm.Tests/Services/{ServiceName}Tests.cs`

```csharp
public class {ServiceName}SimulationServiceTests
{
    [Fact]
    public async Task Operation_WithValidInput_ReturnsSuccess()
    {
        // Arrange
        var service = CreateMockService();
        var request = new Request { /* ... */ };

        // Act
        var result = await service.DoSomethingAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task Operation_WithInvalidInput_ReturnsError()
    {
        // Test error cases
    }

    private {ServiceName}SimulationService CreateMockService()
    {
        var logger = Mock.Of<ILogger<{ServiceName}SimulationService>>();
        return new {ServiceName}SimulationService(logger);
    }
}
```

**Testing Requirements:**
- ✓ Test Mock implementation thoroughly
- ✓ Test Real implementation when available
- ✓ Test error handling
- ✓ Test edge cases
- ✓ >80% code coverage minimum

---

## Deployment Strategy

### Local Development
```bash
# appsettings.Development.json
{
  "Services": {
    "{ServiceName}": { "UseMock": true }
  }
}
```

### Azure Deployment - Phase 1 (Mock)
```bash
az webapp config appsettings set \
  --name app-vendor-mdm-dev \
  --settings "Services__{ServiceName}__UseMock=true"
```

### Azure Deployment - Phase 2 (Real)
```bash
az webapp config appsettings set \
  --name app-vendor-mdm-dev \
  --settings "Services__{ServiceName}__UseMock=false" \
             "Services__{ServiceName}__RealSettings__BaseUrl=https://..." \
             "Services__{ServiceName}__RealSettings__ApiKey=@Microsoft.KeyVault(...)"
```

---

## Checklist

When adding a new service integration:

- [ ] Interface defined (`I{ServiceName}.cs`)
- [ ] Models created (Request/Response DTOs)
- [ ] Mock service implemented (`{ServiceName}SimulationService.cs`)
- [ ] Mock service has realistic test data (50+ records)
- [ ] Real service skeleton created (`{ServiceName}RealService.cs`)
- [ ] Configuration added to `appsettings.json`
- [ ] Service registered in `Program.cs`
- [ ] Controller created (if needed)
- [ ] Swagger documentation complete
- [ ] Unit tests written (Mock implementation)
- [ ] Integration tests written
- [ ] Build succeeds
- [ ] Tested locally with Mock
- [ ] Documentation updated

---

## Examples

See these implementations as reference:

- **SAP Integration:** Perfect example of Mock + Real + Alternative (MoUV)
  - Interface: `ISapVendorService.cs`
  - Mock: `SapVendorSimulationService.cs`
  - Real: `SapVendorRfcService.cs`
  - Alternative: `SapVendorMouvProxyService.cs` (future)

- **Future Examples:**
  - RBAC: Mock (static roles) vs Real (Azure AD)
  - Master Data: Mock (hardcoded) vs Real (SQL)
  - Email: Mock (console) vs Real (SendGrid)

---

## Anti-Patterns (Don't Do This)

❌ **Don't hardcode environment checks in service code**
```csharp
// BAD
public async Task DoSomething()
{
    if (Environment.IsDevelopment())
        return MockData();
    else
        return await CallRealAPI();
}
```

✅ **Do use dependency injection**
```csharp
// GOOD - DI handles this based on config
public async Task DoSomething()
{
    return await _service.DoSomethingAsync();
}
```

❌ **Don't create separate APIs for Mock and Real**
```csharp
// BAD
[Route("api/sap-mock")]
[Route("api/sap-real")]
```

✅ **Do use same API with backend toggle**
```csharp
// GOOD
[Route("api/sap")]  // Same endpoint always
```

❌ **Don't throw NotImplementedException in production Mock**
```csharp
// BAD
public Task<Result> DoSomething()
{
    throw new NotImplementedException();
}
```

✅ **Do implement full Mock functionality**
```csharp
// GOOD
public Task<Result> DoSomething()
{
    return Task.FromResult(new Result { Success = true, Data = mockData });
}
```

---

## Summary

**Golden Rules:**
1. Interface first, implementations second
2. Always implement Mock before Real
3. Mock services go to production
4. Configuration toggles implementations
5. Frontend never knows the difference
6. Same API contract for Mock and Real
7. Progressive activation in production

This standard ensures consistent, testable, deployable service integrations across the entire platform.
