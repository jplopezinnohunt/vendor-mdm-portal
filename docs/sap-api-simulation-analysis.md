# SAP API Simulation - Analysis & Implementation Plan

**Branch:** `feature/sap-api-simulation`  
**Based on:** UNESCO MoUV System Analysis  
**Date:** December 20, 2025

---

## Executive Summary

This document outlines the implementation of SAP API simulation services for local development and testing, based on patterns analyzed from the UNESCO vendor management system (MoUV). The simulation will enable developers to work on vendor onboarding flows without requiring direct connectivity to SAP systems.

### Key Learnings from UNESCO MoUV System

1. **Dual-Source Duplicate Detection**: Searches both Cosmos DB (pending vendors) AND SAP (existing vendors)
2. **Fuzzy Name Matching**: Uses Levenshtein distance with ~75% threshold
3. **Country-Dependent Bank Validation**: Dynamic field visibility and validation rules based on bank country
4. **Multi-Phase Validation**: Separate validation for IBAN, SWIFT/BIC, account numbers
5. **Vendor Search**: Combined search across internal DB and SAP with result merging

---

## 1. Core SAP Services to Simulate

### 1.1 **Vendor Search / Duplicate Detection**
**Purpose:** Find existing vendors in SAP to prevent duplicates  
**UNESCO Pattern:** `SearchExistingVendors`

**Request:**
```json
{
  "vendorType": "INDV",
  "familyName": "TestUser",
  "givenName": "Analysis",
  "dateOfBirth": "1990-01-01",
  "companyCode": "UNES",
  "searchThreshold": 0.75
}
```

**Response:**
```json
{
  "duplicatesFound": true,
  "matchCount": 6,
  "searchAlgorithm": "Levenshtein",
  "threshold": 0.75,
  "vendors": [
    {
      "vendorName": "Test TEST",
      "dateOfBirth": "1990-01-01",
      "sapId": "10189999",
      "country": "France",
      "company": "UNES",
      "accountGroup": "SCSA",
      "sapStatus": "Valid",
      "blocked": false,
      "matchScore": 0.85
    }
  ],
  "processingTime": "145ms"
}
```

### 1.2 **Vendor Get (BAPI_VENDOR_GETDETAIL)**
**Purpose:** Retrieve full vendor master data from SAP  
**SAP Table:** LFA1 (General Data), LFBK (Bank Data), LFB1 (Company Code Data)

**Request:**
```json
{
  "vendorNumber": "10189999",
  "companyCode": "UNES"
}
```

**Response:**
```json
{
  "success": true,
  "vendor": {
    "sapId": "10189999",
    "legalName": "TESTUSER Analysis",
    "accountGroup": "INDV",
    "country": "FR",
    "blocked": false,
    "deletionFlag": false,
    "generalData": {
      "title": "Mr",
      "name1": "TESTUSER",
      "name2": "Analysis",
      "searchTerm": "TESTUSER",
      "street": "123 Main Street",
      "postalCode": "75001",
      "city": "Paris",
      "country": "FR",
      "language": "EN",
      "telephone": "+33123456789",
      "email": "test@example.com"
    },
    "bankAccounts": [
      {
        "bankCountry": "FR",
        "bankKey": "30006",
        "bankAccount": "12345678901",
        "iban": "FR7630006000011234567890189",
        "swift": "BNPAFRPPXXX",
        "accountHolder": "TestUser Analysis",
        "bankName": "BNP Paribas"
      }
    ],
    "companyCodeData": {
      "companyCode": "UNES",
      "reconciliationAccount": "1110010000",
      "paymentTerms": "Z001",
      "paymentMethods": ["T", "C"],
      "currency": "EUR"
    }
  }
}
```

### 1.3 **Vendor Update (BAPI_VENDOR_CHANGE)**
**Purpose:** Update existing vendor master data in SAP

**Request:**
```json
{
  "vendorNumber": "10189999",
  "companyCode": "UNES",
  "changes": {
    "generalData": {
      "telephone": "+33123456999",
      "email": "newemail@example.com"
    },
    "bankAccounts": [
      {
        "operation": "UPDATE",
        "bankCountry": "FR",
        "bankKey": "30006",
        "bankAccount": "12345678901",
        "iban": "FR7630006000011234567890189"
      }
    ]
  }
}
```

**Response:**
```json
{
  "success": true,
  "vendorNumber": "10189999",
  "message": "Vendor updated successfully",
  "warnings": [],
  "errors": [],
  "sapReturn": {
    "type": "S",
    "id": "VK",
    "number": "001",
    "message": "Vendor 10189999 changed"
  }
}
```

### 1.4 **Name Validation**
**Purpose:** Validate vendor names against SAP business rules

**UNESCO Rules:**
- Maximum length: 35 characters (SAP NAME1 field)
- Allowed characters: A-Z, 0-9, space, hyphen, period, comma
- No leading/trailing spaces
- No consecutive spaces
- Must not be purely numeric

**Request:**
```json
{
  "name": "TestUser Analysis",
  "nameType": "PERSON"
}
```

**Response:**
```json
{
  "valid": true,
  "normalized": "TESTUSER Analysis",
  "warnings": [],
  "errors": [],
  "sapFormat": {
    " name1": "TESTUSER",
    "name2": "Analysis",
    "searchTerm": "TESTUSER"
  }
}
```

### 1.5 **Bank Account Validation**
**Purpose:** Validate bank details including IBAN, SWIFT, routing numbers

**Request:**
```json
{
  "bankCountry": "FR",
  "iban": "FR7630006000011234567890189",
  "swift": "BNPAFRPPXXX",
  "accountNumber": "12345678901"
}
```

**Response:**
```json
{
  "valid": true,
  "validations": {
    "iban": {
      "valid": true,
      "checksum": "valid",
      "format": "valid",
      "country": "FR",
      "bankCode": "30006",
      "accountNumber": "00011234567890189"
    },
    "swift": {
      "valid": true,
      "bankCode": "BNPA",
      "countryCode": "FR",
      "locationCode": "PP",
      "branchCode": "XXX"
    },
    "accountNumber": {
      "valid": true,
      "format": "numeric",
      "length": 11
    }
  },
  "warnings": [],
  "errors": []
}
```

---

## 2. Implementation Architecture

### 2.1 Project Structure

```
backend/
└── VendorMdm.Api/
    ├── Controllers/
    │   └── SapSimulationController.cs        # API endpoints
    ├── Services/
    │   ├── ISapSimulationService.cs          # Interface
    │   ├── SapSimulationService.cs           # Implementation
    │   ├── SapVendorSearchService.cs         # Duplicate detection
    │   ├── SapBankValidationService.cs       # Bank validation
    │   └── SapNameValidationService.cs       # Name validation
    └── Models/
        └── SapSimulation/
            ├── SapVendorSearchModels.cs
            ├── SapVendorGetModels.cs
            ├── SapVendorUpdateModels.cs
            ├── SapValidationModels.cs
            └── SapBankModels.cs
```

### 2.2 Data Storage Strategy

Following the **Hybrid Relational-Document Model**:

**Structured (SQL Columns):**
- `VendorId` (PK)
- `SapNumber` (indexed)
- `LegalName` (indexed for search)
- `AccountGroup`
- `CompanyCode`
- `Status`
- `CreatedAt`, `UpdatedAt`

**Semi-Structured (JSONB):**
```json
{
  "generalData": {...},
  "bankAccounts": [...],
  "companyCodeData": {...},
  "sapMetadata": {
    "lastSyncAt": "2025-12-20T10:00:00Z",
    "sapSystem": "D01",
    "sapClient": "100"
  }
}
```

### 2.3 Mock Data Seed

Create realistic test data:
- 100+ vendor records across different account groups
- Various countries (France, US, Germany, Argentina, etc.)
- Different vendor types (Individuals, Companies, Events)
- Multiple bank accounts per vendor
- Realistic names for fuzzy matching tests

---

## 3. Key Algorithms

### 3.1 Levenshtein Distance for Fuzzy Matching

```csharp
public class VendorSearchService
{
    public double CalculateLevenshteinSimilarity(string source, string target)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
            return 0.0;

        int distance = ComputeLevenshteinDistance(source.ToUpper(), target.ToUpper());
        int maxLength = Math.Max(source.Length, target.Length);
        
        return 1.0 - ((double)distance / maxLength);
    }

    private int ComputeLevenshteinDistance(string s, string t)
    {
        int[,] d = new int[s.Length + 1, t.Length + 1];

        for (int i = 0; i <= s.Length; i++)
            d[i, 0] = i;
        for (int j = 0; j <= t.Length; j++)
            d[0, j] = j;

        for (int j = 1; j <= t.Length; j++)
        {
            for (int i = 1; i <= s.Length; i++)
            {
                int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        return d[s.Length, t.Length];
    }

    public async Task<List<VendorMatch>> SearchVendors(string searchTerm, double threshold = 0.75)
    {
        // Get all vendors from mock DB
        var allVendors = await _vendorRepository.GetAllAsync();
        
        // Calculate similarity scores
        var matches = allVendors
            .Select(v => new VendorMatch
            {
                Vendor = v,
                Score = CalculateLevenshteinSimilarity(searchTerm, v.LegalName)
            })
            .Where(m => m.Score >= threshold)
            .OrderByDescending(m => m.Score)
            .Take(20)
            .ToList();

        return matches;
    }
}
```

### 3.2 IBAN Validation

```csharp
public class IbanValidator
{
    public IbanValidationResult Validate(string iban)
    {
        var result = new IbanValidationResult();
        
        // Remove spaces and convert to uppercase
        iban = iban?.Replace(" ", "").ToUpper();
        
        if (string.IsNullOrEmpty(iban))
        {
            result.Valid = false;
            result.Errors.Add( "IBAN cannot be empty");
            return result;
        }

        // Check length (varies by country)
        var countryCode = iban.Substring(0, 2);
        var expectedLength = GetIbanLength(countryCode);
        
        if (iban.Length != expectedLength)
        {
            result.Valid = false;
            result.Errors.Add($"Invalid length for {countryCode}. Expected {expectedLength}, got {iban.Length}");
            return result;
        }

        // Validate checksum (mod 97)
        if (!ValidateIbanChecksum(iban))
        {
            result.Valid = false;
            result.Errors.Add("Invalid IBAN checksum");
            return result;
        }

        result.Valid = true;
        result.Country = countryCode;
        result.BankCode = ExtractBankCode(iban, countryCode);
        result.AccountNumber = ExtractAccountNumber(iban, countryCode);
        
        return result;
    }

    private bool ValidateIbanChecksum(string iban)
    {
        // Move first 4 characters to end
        string rearranged = iban.Substring(4) + iban.Substring(0, 4);
        
        // Replace letters with numbers (A=10, B=11, ..., Z=35)
        string numericIban = string.Empty;
        foreach (char c in rearranged)
        {
            if (char.IsLetter(c))
                numericIban += (c - 'A' + 10).ToString();
            else
                numericIban += c;
        }

        // Calculate mod 97
        BigInteger ibanNumber = BigInteger.Parse(numericIban);
        return ibanNumber % 97 == 1;
    }

    private int GetIbanLength(string countryCode)
    {
        return countryCode switch
        {
            "FR" => 27,
            "DE" => 22,
            "GB" => 22,
            "US" => 0,  // No IBAN
            "ES" => 24,
            "IT" => 27,
            "AR" => 24,
            _ => 0
        };
    }
}
```

### 3.3 SAP Name Validation

```csharp
public class SapNameValidator
{
    private const int MAX_NAME_LENGTH = 35;
    private static readonly Regex AllowedCharsRegex = 
        new Regex(@"^[A-Za-z0-9 \-\.,]+$");

    public NameValidationResult Validate(string name, string nameType)
    {
        var result = new NameValidationResult();
        
        if (string.IsNullOrWhiteSpace(name))
        {
            result.Valid = false;
            result.Errors.Add("Name cannot be empty");
            return result;
        }

        // Trim and normalize
        name = name.Trim();
        
        // Check length
        if (name.Length > MAX_NAME_LENGTH)
        {
            result.Valid = false;
            result.Errors.Add($"Name exceeds maximum length of {MAX_NAME_LENGTH} characters");
        }

        // Check allowed characters
        if (!AllowedCharsRegex.IsMatch(name))
        {
            result.Valid = false;
            result.Errors.Add("Name contains invalid characters. Only A-Z, 0-9, space, hyphen, period, and comma allowed");
        }

        // Check for purely numeric
        if (name.All(char.IsDigit))
        {
            result.Valid = false;
            result.Errors.Add("Name cannot be purely numeric");
        }

        // Check for consecutive spaces
        if (name.Contains("  "))
        {
            result.Warnings.Add("Name contains consecutive spaces");
            name = Regex.Replace(name, @"\s+", " ");
        }

        // Normalize to SAP format (uppercase first part)
        result.Normalized = name;
        result.SapFormat = ConvertToSapFormat(name, nameType);
        result.Valid = result.Valid && !result.Errors.Any();

        return result;
    }

    private SapNameFormat ConvertToSapFormat(string name, string nameType)
    {
        if (nameType == "PERSON")
        {
            var parts = name.Split(' ', 2);
            return new SapNameFormat
            {
                Name1 = parts[0].ToUpper(),
                Name2 = parts.Length > 1 ? parts[1] : "",
                SearchTerm = parts[0].ToUpper()
            };
        }
        else
        {
            return new SapNameFormat
            {
                Name1 = name.Length > 35 ? name.Substring(0, 35) : name,
                Name2 = name.Length > 35 ? name.Substring(35, Math.Min(35, name.Length - 35)) : "",
                SearchTerm = name.Substring(0, Math.Min(20, name.Length)).ToUpper()
            };
        }
    }
}
```

---

## 4. Configuration

### 4.1 appsettings.json

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
    }
  }
}
```

### 4.2 Service Registration

```csharp
// Program.cs
builder.Services.AddScoped<ISapSimulationService, SapSimulationService>();
builder.Services.AddScoped<ISapVendorSearchService, SapVendorSearchService>();
builder.Services.AddScoped<ISapBankValidationService, SapBankValidationService>();
builder.Services.AddScoped<ISapNameValidationService, SapNameValidationService>();

// Configure based on appsettings
if (builder.Configuration.GetValue<bool>("SapSimulation:Enabled"))
{
    builder.Services.AddSingleton<ISapDataStore, InMemorySapDataStore>();
}
```

---

## 5. API Endpoints

```csharp
[ApiController]
[Route("api/sap-simulation")]
[Produces("application/json")]
public class SapSimulationController : ControllerBase
{
    private readonly ISapSimulationService _sapService;

    [HttpPost("vendor/search")]
    [ProducesResponseType(typeof(VendorSearchResponse), 200)]
    public async Task<ActionResult<VendorSearchResponse>> SearchVendors(
        [FromBody] VendorSearchRequest request)
    {
        var result = await _sapService.SearchVendorsAsync(request);
        return Ok(result);
    }

    [HttpGet("vendor/{vendorNumber}")]
    [ProducesResponseType(typeof(VendorGetResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<VendorGetResponse>> GetVendor(
        string vendorNumber,
        [FromQuery] string companyCode)
    {
        var result = await _sapService.GetVendorAsync(vendorNumber, companyCode);
        if (result == null)
            return NotFound(new { message = $"Vendor {vendorNumber} not found" });
            
        return Ok(result);
    }

    [HttpPut("vendor/{vendorNumber}")]
    [ProducesResponseType(typeof(VendorUpdateResponse), 200)]
    public async Task<ActionResult<VendorUpdateResponse>> UpdateVendor(
        string vendorNumber,
        [FromBody] VendorUpdateRequest request)
    {
        var result = await _sapService.UpdateVendorAsync(vendorNumber, request);
        return Ok(result);
    }

    [HttpPost("validate/name")]
    [ProducesResponseType(typeof(NameValidationResult), 200)]
    public async Task<ActionResult<NameValidationResult>> ValidateName(
        [FromBody] NameValidationRequest request)
    {
        var result = await _sapService.ValidateNameAsync(request);
        return Ok(result);
    }

    [HttpPost("validate/bank")]
    [ProducesResponseType(typeof(BankValidationResult), 200)]
    public async Task<ActionResult<BankValidationResult>> ValidateBank(
        [FromBody] BankValidationRequest request)
    {
        var result = await _sapService.ValidateBankAsync(request);
        return Ok(result);
    }
}
```

---

## 6. Testing Strategy

### 6.1 Unit Tests

```csharp
public class SapSimulationServiceTests
{
    [Fact]
    public async Task SearchVendors_WithValidName_ReturnsMatches()
    {
        // Arrange
        var service = CreateService();
        var request = new VendorSearchRequest
        {
            FamilyName = "TestUser",
            GivenName = "Analysis",
            SearchThreshold = 0.75
        };

        // Act
        var result = await service.SearchVendorsAsync(request);

        // Assert
        Assert.True(result.DuplicatesFound);
        Assert.NotEmpty(result.Vendors);
        Assert.All(result.Vendors, v => Assert.True(v.MatchScore >= 0.75));
    }

    [Theory]
    [InlineData("FR7630006000011234567890189", "FR", true)]
    [InlineData("INVALID", "FR", false)]
    public async Task ValidateBank_WithIban_ReturnsCorrectResult(
        string iban, string country, bool expectedValid)
    {
        // Arrange
        var service = CreateService();
        var request = new BankValidationRequest
        {
            BankCountry = country,
            Iban = iban
        };

        // Act
        var result = await service.ValidateBankAsync(request);

        // Assert
        Assert.Equal(expectedValid, result.Valid);
    }
}
```

### 6.2 Integration Tests

```csharp
public class SapSimulationControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SapSimulationControllerTests(WebApplicationFactory<Program>> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SearchVendors_ReturnsOkWithResults()
    {
        // Arrange
        var request = new VendorSearchRequest
        {
            FamilyName = "Smith",
            GivenName = "John"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sap-simulation/vendor/search", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<VendorSearchResponse>();
        Assert.NotNull(result);
    }
}
```

---

## 7. Migration Path

### 7.1 Toggle Between Simulation and Real SAP

```csharp
public interface ISapVendorService
{
    Task<VendorSearchResponse> SearchVendorsAsync(VendorSearchRequest request);
    Task<VendorGetResponse> GetVendorAsync(string vendorNumber, string companyCode);
    Task<VendorUpdateResponse> UpdateVendorAsync(string vendorNumber, VendorUpdateRequest request);
}

// Simulation implementation
public class SapSimulationVendorService : ISapVendorService
{
    // In-memory mock implementation
}

// Real SAP implementation (future)
public class SapRfcVendorService : ISapVendorService
{
    // SAP NCo / RFC implementation
}

// Configure in Program.cs
if (builder.Configuration.GetValue<bool>("SapSimulation:Enabled"))
{
    builder.Services.AddScoped<ISapVendorService, SapSimulationVendorService>();
}
else
{
    builder.Services.AddScoped<ISapVendorService, SapRfcVendorService>();
}
```

---

## 8. Next Steps

1. **Create Models** - Define all request/response DTOs
2. **Implement Services** - Build simulation services with mock data
3. **Add Controllers** - Create API endpoints with Swagger documentation
4. **Seed Mock Data** - Generate realistic test data
5. **Write Tests** - Unit and integration tests
6. **Update Documentation** - API docs and usage examples
7. **Integration** - Connect to existing vendor onboarding flow

---

## 9. Success Criteria

- [ ] All three API operations (Get, Update, Search) functional
- [ ] Fuzzy matching with Levenshtein distance working
- [ ] Bank validation for SEPA countries (FR, DE, ES, IT)
- [ ] Bank validation for US (routing + account number)
- [ ] Name validation following SAP rules
- [ ] Unit tests passing with >80% coverage
- [ ] Swagger documentation complete
- [ ] Mock data seed with 100+ vendors
- [ ] Can toggle between simulation and real SAP via config
- [ ] Integration with existing `VendorSapMapper` works seamlessly

---

