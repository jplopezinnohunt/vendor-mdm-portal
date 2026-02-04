# SAP Integration Standard

**Category**: Integration & Infrastructure
**Pattern #**: 15
**Status**: MANDATORY for SAP operations

---

## Definition

SAP RFC integration MUST use adapter pattern with simulation mode for local development.

---

## Rules

1. **ALWAYS** use adapter pattern for SAP calls
2. **ALWAYS** implement simulation mode for local dev
3. **ALWAYS** handle SAP errors gracefully with Result pattern
4. **NEVER** hardcode SAP credentials

---

## Implementation

### Interface

```csharp
public interface ISapIntegrationService
{
    Task<Result<string>> CreateVendorAsync(VendorSapPayload payload);
    Task<Result<string>> UpdateVendorAsync(string sapVendorId, VendorSapPayload payload);
    Task<Result<VendorSapData>> GetVendorAsync(string sapVendorId);
}
```

### Real Implementation

```csharp
public class SapIntegrationService : ISapIntegrationService
{
    private readonly ISapRfcClient _rfcClient;
    private readonly IStructuredLogger _logger;

    public async Task<Result<string>> CreateVendorAsync(VendorSapPayload payload)
    {
        try
        {
            var result = await _rfcClient.ExecuteAsync(
                "ZBAPI_VENDOR_CREATE",
                new {
                    VENDOR_NAME = payload.Name,
                    VENDOR_TAX_ID = payload.TaxId,
                    VENDOR_COUNTRY = payload.Country
                });

            _logger.LogInformation("SAP vendor created", new {
                sapVendorId = result.VendorId,
                operation = "ZBAPI_VENDOR_CREATE"
            });

            return Result<string>.Success(result.VendorId);
        }
        catch (SapException ex)
        {
            _logger.LogError("SAP vendor creation failed", new {
                error = ex.Message,
                payload = payload.Name
            });
            return Result<string>.Failure($"SAP Error: {ex.Message}");
        }
    }
}
```

### Simulation Implementation

```csharp
public class SimulatedSapIntegrationService : ISapIntegrationService
{
    private readonly IStructuredLogger _logger;

    public Task<Result<string>> CreateVendorAsync(VendorSapPayload payload)
    {
        var simulatedId = $"SAP-SIM-{Guid.NewGuid():N}".Substring(0, 10);

        _logger.LogInformation("[SIMULATION MODE] SAP vendor created", new {
            sapVendorId = simulatedId,
            vendorName = payload.Name,
            operation = "ZBAPI_VENDOR_CREATE"
        });

        return Task.FromResult(Result<string>.Success(simulatedId));
    }
}
```

### Registration (Program.cs)

```csharp
if (builder.Configuration.GetValue<bool>("Sap:UseMock"))
{
    builder.Services.AddSingleton<ISapIntegrationService, SimulatedSapIntegrationService>();
}
else
{
    builder.Services.AddSingleton<ISapIntegrationService, SapIntegrationService>();
}
```

---

## SAP Operations

| Operation | BAPI | Description |
|-----------|------|-------------|
| Create Vendor | ZBAPI_VENDOR_CREATE | Create new vendor master |
| Update Vendor | ZBAPI_VENDOR_UPDATE | Update vendor details |
| Get Vendor | ZBAPI_VENDOR_GET | Retrieve vendor data |
| Block Vendor | ZBAPI_VENDOR_BLOCK | Block vendor for posting |

---

## Reference

- **Interface**: `Api/Services/External/ISapIntegrationService.cs`
- **Golden Rules**: Section 10.4 Pattern 15
