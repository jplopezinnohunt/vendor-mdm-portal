# SAP Simulation Services - Implementation Status

**Branch:** `feature/sap-api-simulation`  
**Created:** December 20, 2025  
**Status:** Phase 1 - Core Models & Helpers Complete

---

## ✅ Completed

### 1. Core Models Created
- ✅ `SapVendorSearchModels.cs` - Duplicate detection with fuzzy matching
- ✅ `SapVendorGetModels.cs` - Vendor detail retrieval (LFA1, LFBK, LFB1)
- ✅ `SapVendorUpdateModels.cs` - Create & update operations
- ✅ `SapValidationModels.cs` - Name and bank validation

### 2. Service Interfaces
- ✅ `ISapVendorService.cs` - Complete SAP service contract

### 3. Helper Algorithms
- ✅ `LevenshteinMatcher.cs` - Fuzzy string matching (0.75 threshold)
- ✅ `IbanValidator.cs` - IBAN validation with mod-97 checksum

---

## 🚧 Next Steps

### Phase 1: Complete Core Services (Priority)

1. **Create `SapNameValidator.cs`**
   ```csharp
   - Validate 35-char max length
   - Check allowed characters (A-Z, 0-9, space, hyphen, period,  comma)
   - No leading/trailing spaces
   - No consecutive spaces
   - Cannot be purely numeric
   - Convert to SAP format (NAME1, NAME2, SEARCHTERM)
   ```

2. **Create `SwiftValidator.cs`**
   ```csharp
   - Validate 8 or 11 character format
   - Extract: BankCode (4), CountryCode (2), LocationCode (2), BranchCode (3 optional)
   - Validate country code matches IBAN if both present
   ```

3. **Create `SapVendorSimulationService.cs`** (Main Implementation)
   - Implement all `ISapVendorService` methods
   - Use in-memory data store
   - Levenshtein-based search
   - Mock 100+ vendor records
   - Realistic latency simulation (100-500ms optional)

4. **Create SapSimulationController.cs**
   ```csharp
   [ApiController]
   [Route("api/sap-simulation")]
   public class SapSimulationController
   {
       // POST /api/sap-simulation/vendor/search
       // GET /api/sap-simulation/vendor/{vendorNumber}
       // POST /api/sap-simulation/vendor
       // PUT /api/sap-simulation/vendor/{vendorNumber}
       // POST /api/sap-simulation/validate/name
       // POST /api/sap-simulation/validate/bank
       // POST /api/sap-simulation/bank/check-duplicate
   }
   ```

5. **Service Registration in `Program.cs`**
   ```csharp
   if (builder.Configuration.GetValue<bool>("SapSimulation:Enabled"))
   {
       builder.Services.AddScoped<ISapVendorService, SapVendorSimulationService>();
       builder.Services.AddSingleton<LevenshteinMatcher>();
       builder.Services.AddSingleton<IbanValidator>();
       builder.Services.AddSingleton<SwiftValidator>();
       builder.Services.AddSingleton<SapNameValidator>();
   }
   ```

### Phase 2: Testing

6. **Unit Tests**
   - `LevenshteinMatcherTests.cs`
   - `IbanValidatorTests.cs`
   - `SapNameValidatorTests.cs`
   - `SwiftValidatorTests.cs`
   - `SapVendorSimulationServiceTests.cs`

7. **Integration Tests**
   - `SapSimulationControllerTests.cs`
   - End-to-end duplicate detection flow
   - Bank validation flow

### Phase 3: Documentation

8. **Swagger/OpenAPI**
   - Document all endpoints
   - Add request/response examples
   - Include validation rules

9. **README**
   - Setup instructions
   - API usage examples
   - Configuration options

---

## 📋 Configuration Template

Add to `appsettings.json`:

```json
{
  "SapSimulation": {
    "Enabled": true,
    "Mode": "InMemory",
    "MockDataSeed": true,
    "SimulateLatency": false,
    "LatencyMs": {
      "Min": 100,
      "Max": 500
    },
    "DuplicateSearchThreshold": 0.75,
    "SapSystem": {
      "SystemId": "D01",
      "Client": "100",
      "Environment": "DEVELOPMENT"
    },
    "ValidationRules": {
      "MaxNameLength": 35,
      "MaxSearchTermLength": 20,
      "AllowedNameCharacters": "A-Za-z0-9 \\-\\.,",
      "SupportedCountries": ["FR", "DE", "GB", "ES", "IT", "US", "AR", "BR", "MX", "CA"]
    }
  }
}
```

---

## 🎯 Success Criteria

- [ ] All SAP service interfaces implemented
- [ ] Fuzzy vendor search working (Levenshtein)
- [ ] IBAN validation for SEPA countries
- [ ] SWIFT validation
- [ ] Name validation (SAP rules)
- [ ] Bank duplicate check
- [ ] 100+ mock vendor records
- [ ] Unit tests >80% coverage
- [ ] Integration tests for critical paths
- [ ] Swagger documentation complete
- [ ] Can toggle simulation via config

---

## 🔄 Migration to Real SAP

When ready to connect to real SAP:

1. **Create `SapVendorRfcService.cs`** implementing `ISapVendorService`
2. **Update `appsettings.json`**:
   ```json
   {
     "SapSimulation": {
       "Enabled": false
     },
     "SapConnection": {
       "Host": "your-sap-server",
       "SystemNumber": "00",
       "Client": "100",
       "User": "RFC_USER",
       "Password": "from-keyvault",
       "Language": "EN"
     }
   }
   ```
3. **Update `Program.cs`** service registration
4. **No controller changes needed** - same API contract

---

## 📌 Key Design Decisions

### 1. Interface-Based Architecture
- **Why:** Allows swapping between simulation and real implementation
- **Pattern:** Dependency injection with configuration-based selection
- **Benefit:** Zero code changes in controllers when migrating

### 2. Levenshtein Distance for Fuzzy Matching
- **Why:** UNESCO MoUV uses this algorithm successfully
- **Threshold:** 0.75 (75% similarity) to catch duplicates
- **Performance:** O(n*m) but acceptable for vendor names (<50 chars)

### 3. Comprehensive Validation
- **Country-Specific Bank Rules:** Each country has different requirements
- **SAP Name Rules:** Strict 35-char limit, character restrictions
- **Checksum Validation:** IBAN mod-97, routing number mod-10

### 4. Semi-Structured Data (JSONB)
- **Structured:** VendorId, SapNumber, LegalName, Status (SQL columns)
- **Semi-Structured:** generalData, bankAccounts, companyCodeData (JSONB)
- **Reason:** Follows Hybrid Relational-Document Model standard

---

## 🔗 Related Files

- UNESCO Reference: `/docs/reference/unesco-vendor-management-reference.html`
- Analysis Document: `/docs/sap-api-simulation-analysis.md`
- Complete Plan: `/docs/simulation-services-complete-plan.md`
- Existing SAP Mapper: `/backend/VendorMdm.Shared/Mapping/SapMapperService.cs`

---

## 🚀 Quick Start Commands

```bash
# Switch to branch
git checkout feature/sap-api-simulation

# Build
dotnet build backend/VendorMdm.sln

# Run API
cd backend/VendorMdm.Api
dotnet run

# Run tests (once created)
dotnet test backend/VendorMdm.sln

# View Swagger
# Navigate to: https://localhost:5001/swagger
```

---

## 📝 Notes

- **Email Service:** Skipped - already implemented in existing codebase
- **Audit Trail:** Review existing implementation before enhancing
- **Workflow Service:** Review existing implementation before enhancing
- **Priority:** SAP integration is the main goal of this branch

---

**Last Updated:** December 20, 2025
**Next Action:** Implement `SapVendorSimulationService.cs`
