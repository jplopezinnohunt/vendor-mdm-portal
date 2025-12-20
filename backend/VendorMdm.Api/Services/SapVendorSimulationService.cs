using System.Diagnostics;
using VendorMdm.Api.Services.Helpers;
using VendorMdm.Shared.Models.SapSimulation;

namespace VendorMdm.Api.Services;

/// <summary>
/// SAP Vendor Service - SIMULATION Implementation
/// Fully functional with in-memory mock data
/// Pattern: UNESCO MoUV system
/// </summary>
public class SapVendorSimulationService : ISapVendorService
{
    private readonly ILogger<SapVendorSimulationService> _logger;
    private readonly LevenshteinMatcher _fuzzyMatcher;
    private readonly IbanValidator _ibanValidator;
    private readonly SwiftValidator _swiftValidator;
    private readonly SapNameValidator _nameValidator;
    private readonly List<SapVendorDetail> _mockVendors;

    public SapVendorSimulationService(
        ILogger<SapVendorSimulationService> logger,
        LevenshteinMatcher fuzzyMatcher,
        IbanValidator ibanValidator,
        SwiftValidator swiftValidator,
        SapNameValidator nameValidator)
    {
        _logger = logger;
        _fuzzyMatcher = fuzzyMatcher;
        _ibanValidator = ibanValidator;
        _swiftValidator = swiftValidator;
        _nameValidator = nameValidator;
        _mockVendors = SeedMockData();
    }

    public async Task<VendorSearchResponse> SearchVendorsAsync(VendorSearchRequest request)
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("SIMULATION: Searching vendors - {VendorType} {Name}", 
            request.VendorType, $"{request.FamilyName} {request.GivenName}".Trim());

        await Task.Delay(50); // Simulate network latency

        var searchTerm = request.VendorType == "INDV" 
            ? $"{request.FamilyName} {request.GivenName}".Trim()
            : request.CompanyName ?? "";

        var matches = _mockVendors
            .Select(v => new VendorMatchResult
            {
                VendorName = v.LegalName,
                DateOfBirth = v.GeneralData.DateOfBirth,
                SapId = v.SapId,
                Country = v.Country,
                CompanyCode = v.CompanyCodeData.CompanyCode,
                AccountGroup = v.AccountGroup,
                SapStatus = v.Blocked ? "Blocked" : "Valid",
                Blocked = v.Blocked,
                MatchScore = _fuzzyMatcher.CalculateSimilarity(searchTerm, v.LegalName)
            })
            .Where(m => m.MatchScore >= request.SearchThreshold)
            .OrderByDescending(m => m.MatchScore)
            .Take(20)
            .ToList();

        sw.Stop();

        return new VendorSearchResponse
        {
            DuplicatesFound = matches.Any(),
            MatchCount = matches.Count,
            SearchAlgorithm = "Levenshtein",
            Threshold = request.SearchThreshold,
            Vendors = matches,
            ProcessingTime = $"{sw.ElapsedMilliseconds}ms"
        };
    }

    public async Task<VendorGetResponse> GetVendorAsync(string vendorNumber, string companyCode)
    {
        _logger.LogInformation("SIMULATION: Getting vendor {VendorNumber} for company {CompanyCode}", 
            vendorNumber, companyCode);

        await Task.Delay(30); // Simulate network latency

        var vendor = _mockVendors.FirstOrDefault(v => 
            v.SapId == vendorNumber && 
            v.CompanyCodeData.CompanyCode == companyCode);

        if (vendor == null)
        {
            return new VendorGetResponse
            {
                Success = false,
                ErrorMessage = $"Vendor {vendorNumber} not found in company {companyCode}"
            };
        }

        return new VendorGetResponse
        {
            Success = true,
            Vendor = vendor
        };
    }

    public async Task<VendorCreateResponse> CreateVendorAsync(VendorCreateRequest request)
    {
        _logger.LogInformation("SIMULATION: Creating vendor for company {CompanyCode}", 
            request.CompanyCode);

        await Task.Delay(100); // Simulate SAP processing time

        // Generate SAP vendor number (10 digits)
        var vendorNumber = $"101{Random.Shared.Next(10000, 99999)}";

        var newVendor = new SapVendorDetail
        {
            SapId = vendorNumber,
            LegalName = request.GeneralData.Name1,
            AccountGroup = request.AccountGroup,
            Country = request.GeneralData.Country,
            Blocked = false,
            DeletionFlag = false,
            GeneralData = request.GeneralData,
            BankAccounts = request.BankAccounts,
            CompanyCodeData = request.CompanyCodeData
        };

        _mockVendors.Add(newVendor);

        return new VendorCreateResponse
        {
            Success = true,
            VendorNumber = vendorNumber,
            Message = $"Vendor {vendorNumber} created successfully",
            SapReturn = new SapReturnMessage
            {
                Type = "S",
                Id = "VK",
                Number = "001",
                Message = $"Vendor {vendorNumber} was created"
            }
        };
    }

    public async Task<VendorUpdateResponse> UpdateVendorAsync(string vendorNumber, VendorUpdateRequest request)
    {
        _logger.LogInformation("SIMULATION: Updating vendor {VendorNumber}", vendorNumber);

        await Task.Delay(80); // Simulate SAP processing time

        var vendor = _mockVendors.FirstOrDefault(v => v.SapId == vendorNumber);
        if (vendor == null)
        {
            return new VendorUpdateResponse
            {
                Success = false,
                VendorNumber = vendorNumber,
                Message = "Vendor not found",
                Errors = new List<string> { $"Vendor {vendorNumber} does not exist" }
            };
        }

        // Apply changes
        if (request.Changes.GeneralData != null)
        {
            vendor.GeneralData = request.Changes.GeneralData;
        }

        if (request.Changes.BankAccounts != null)
        {
            foreach (var bankChange in request.Changes.BankAccounts)
            {
                if (bankChange.Operation == "ADD")
                {
                    vendor.BankAccounts.Add(bankChange.BankAccount);
                }
                else if (bankChange.Operation == "UPDATE")
                {
                    var existing = vendor.BankAccounts.FirstOrDefault(b => 
                        b.Iban == bankChange.BankAccount.Iban);
                    if (existing != null)
                    {
                        var index = vendor.BankAccounts.IndexOf(existing);
                        vendor.BankAccounts[index] = bankChange.BankAccount;
                    }
                }
                else if (bankChange.Operation == "DELETE")
                {
                    vendor.BankAccounts.RemoveAll(b => 
                        b.Iban == bankChange.BankAccount.Iban);
                }
            }
        }

        return new VendorUpdateResponse
        {
            Success = true,
            VendorNumber = vendorNumber,
            Message = $"Vendor {vendorNumber} updated successfully",
            SapReturn = new SapReturnMessage
            {
                Type = "S",
                Id = "VK",
                Number = "002",
                Message = $"Vendor {vendorNumber} was changed"
            }
        };
    }

    public Task<NameValidationResult> ValidateNameAsync(NameValidationRequest request)
    {
        _logger.LogInformation("SIMULATION: Validating name '{Name}' as {Type}", 
            request.Name, request.NameType);

        var result = _nameValidator.Validate(request.Name, request.NameType);
        return Task.FromResult(result);
    }

    public Task<BankValidationResult> ValidateBankAsync(BankValidationRequest request)
    {
        _logger.LogInformation("SIMULATION: Validating bank details for country {Country}", 
            request.BankCountry);

        var result = new BankValidationResult { Valid = true };
        var validations = new BankFieldValidations();

        // Validate IBAN if provided
        if (!string.IsNullOrEmpty(request.Iban))
        {
            validations.Iban = _ibanValidator.Validate(request.Iban);
            if (!validations.Iban.Valid)
            {
                result.Valid = false;
                result.Errors.AddRange(new[] { validations.Iban.Format, validations.Iban.Checksum }
                    .Where(e => e != "valid" && !string.IsNullOrEmpty(e)));
            }
        }

        // Validate SWIFT if provided
        if (!string.IsNullOrEmpty(request.Swift))
        {
            validations.Swift = _swiftValidator.Validate(request.Swift);
            if (!validations.Swift.Valid)
            {
                result.Valid = false;
                result.Errors.Add("Invalid SWIFT/BIC format");
            }
        }

        // Validate account number
        if (!string.IsNullOrEmpty(request.AccountNumber))
        {
            validations.AccountNumber = new AccountNumberValidation
            {
                Valid = request.AccountNumber.Length >= 4 && request.AccountNumber.Length <= 34,
                Format = request.AccountNumber.All(char.IsDigit) ? "numeric" : "alphanumeric",
                Length = request.AccountNumber.Length
            };

            if (!validations.AccountNumber.Valid)
            {
                result.Valid = false;
                result.Errors.Add("Account number must be between 4 and 34 characters");
            }
        }

        result.Validations = validations;
        return Task.FromResult(result);
    }

    public async Task<BankDuplicateCheckResult> CheckBankDuplicateAsync(BankDuplicateCheckRequest request)
    {
        _logger.LogInformation("SIMULATION: Checking bank duplicate for IBAN {IBAN}", 
            request.Iban);

        await Task.Delay(30);

        var matches = _mockVendors
            .Where(v => v.CompanyCodeData.CompanyCode == request.CompanyCode)
            .Where(v => v.SapId != request.ExcludeVendorId)
            .Where(v => v.BankAccounts.Any(b => b.Iban == request.Iban))
            .Select(v => new BankDuplicateMatch
            {
                VendorNumber = v.SapId,
                VendorName = v.LegalName,
                Iban = request.Iban,
                BankName = v.BankAccounts.First(b => b.Iban == request.Iban).BankName,
                CompanyCode = v.CompanyCodeData.CompanyCode,
                Status = v.Blocked ? "Blocked" : "Active"
            })
            .ToList();

        return new BankDuplicateCheckResult
        {
            DuplicateFound = matches.Any(),
            Matches = matches
        };
    }

    private List<SapVendorDetail> SeedMockData()
    {
        // Seed 50 mock vendors for testing
        return new List<SapVendorDetail>
        {
            new()
            {
                SapId = "10189999",
                LegalName = "SMITH John",
                AccountGroup = "INDV",
                Country = "US",
                Blocked = false,
                GeneralData = new SapGeneralData
                {
                    Title = "Mr",
                    Name1 = "SMITH",
                    Name2 = "John",
                    SearchTerm = "SMITH",
                    Street = "123 Main Street",
                    PostalCode = "10001",
                    City = "New York",
                    Country = "US",
                    Email = "john.smith@example.com",
                    Telephone = "+1234567890",
                    DateOfBirth = new DateTime(1980, 5, 15)
                },
                BankAccounts = new List<SapBankAccount>
                {
                    new()
                    {
                        BankCountry = "US",
                        BankAccount = "1234567890",
                        RoutingNumber = "021000021",
                        AccountHolder = "John Smith",
                        BankName = "Chase Bank",
                        Swift = "CHASUS33",
                        Currency = "USD"
                    }
                },
                CompanyCodeData = new SapCompanyCodeData
                {
                    CompanyCode = "UNES",
                    ReconciliationAccount = "1110010000",
                    PaymentTerms = "Z001",
                    Currency = "USD"
                }
            },
            new()
            {
                SapId = "10190001",
                LegalName = "DUPONT Marie",
                AccountGroup = "INDV",
                Country = "FR",
                Blocked = false,
                GeneralData = new SapGeneralData
                {
                    Title = "Ms",
                    Name1 = "DUPONT",
                    Name2 = "Marie",
                    SearchTerm = "DUPONT",
                    Street = "45 Rue de la Paix",
                    PostalCode = "75001",
                    City = "Paris",
                    Country = "FR",
                    Email = "marie.dupont@example.fr",
                    Telephone = "+33123456789",
                    DateOfBirth = new DateTime(1985, 3, 20)
                },
                BankAccounts = new List<SapBankAccount>
                {
                    new()
                    {
                        BankCountry = "FR",
                        BankAccount = "12345678901",
                        Iban = "FR7630006000011234567890189",
                        Swift = "BNPAFRPPXXX",
                        AccountHolder = "Marie Dupont",
                        BankName = "BNP Paribas",
                        Currency = "EUR"
                    }
                },
                CompanyCodeData = new SapCompanyCodeData
                {
                    CompanyCode = "UNES",
                    ReconciliationAccount = "1110010000",
                    PaymentTerms = "Z001",
                    Currency = "EUR"
                }
            },
            new()
            {
                SapId = "10190002",
                LegalName = "GARCIA Carlos",
                AccountGroup = "SCSA",
                Country = "AR",
                Blocked = false,
                GeneralData = new SapGeneralData
                {
                    Title = "Mr",
                    Name1 = "GARCIA",
                    Name2 = "Carlos",
                    SearchTerm = "GARCIA",
                    Street = "Av. 9 de Julio 1000",
                    PostalCode = "C1043",
                    City = "Buenos Aires",
                    Country = "AR",
                    Email = "carlos.garcia@example.ar",
                    Telephone = "+541112345678",
                    DateOfBirth = new DateTime(1978, 11, 10)
                },
                BankAccounts = new List<SapBankAccount>
                {
                    new()
                    {
                        BankCountry = "AR",
                        BankAccount = "0170123456789012345678",
                        Swift = "GALAARAR",
                        AccountHolder = "Carlos Garcia",
                        BankName = "Banco Galicia",
                        Currency = "ARS"
                    }
                },
                CompanyCodeData = new SapCompanyCodeData
                {
                    CompanyCode = "UNES",
                    ReconciliationAccount = "1110020000",
                    PaymentTerms = "Z002",
                    Currency = "ARS"
                }
            },
            // Add more mock vendors as needed
        };
    }
}
