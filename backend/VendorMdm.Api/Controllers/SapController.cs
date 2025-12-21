using Microsoft.AspNetCore.Mvc;
using VendorMdm.Api.Services;
using VendorMdm.Shared.Models.SapSimulation;

namespace VendorMdm.Api.Controllers;

/// <summary>
/// SAP Integration API
/// Provides vendor management operations (Search, CRUD, Validation)
/// Supports both simulation and real SAP RFC implementations
/// </summary>
[ApiController]
[Route("api/sap")]
[Produces("application/json")]
public class SapController : ControllerBase
{
    private readonly ISapVendorService _sapService;
    private readonly ILogger<SapController> _logger;

    public SapController(
        ISapVendorService sapService,
        ILogger<SapController> logger)
    {
        _sapService = sapService;
        _logger = logger;
    }

    /// <summary>
    /// Search for existing vendors (duplicate detection)
    /// </summary>
    /// <remarks>
    /// Uses Levenshtein fuzzy matching algorithm with configurable threshold (default 0.75).
    /// 
    /// Example request for person:
    /// 
    ///     POST /api/sap/vendor/search
    ///     {
    ///       "vendorType": "INDV",
    ///       "familyName": "Smith",
    ///       "givenName": "John",
    ///       "companyCode": "UNES",
    ///       "searchThreshold": 0.75
    ///     }
    ///     
    /// Example request for company:
    /// 
    ///     POST /api/sap/vendor/search
    ///     {
    ///       "vendorType": "COMP",
    ///       "companyName": "Acme Corporation",
    ///       "companyCode": "UNES",
    ///       "searchThreshold": 0.75
    ///     }
    /// </remarks>
    [HttpPost("vendor/search")]
    [ProducesResponseType(typeof(VendorSearchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VendorSearchResponse>> SearchVendors(
        [FromBody] VendorSearchRequest request)
    {
        try
        {
            var result = await _sapService.SearchVendorsAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching vendors");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get complete vendor master data from SAP
    /// </summary>
    /// <param name="vendorNumber">SAP vendor number (e.g., "10189999")</param>
    /// <param name="companyCode">Company code (e.g., "UNES")</param>
    [HttpGet("vendor/{vendorNumber}")]
    [ProducesResponseType(typeof(VendorGetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VendorGetResponse>> GetVendor(
        string vendorNumber,
        [FromQuery] string companyCode = "UNES")
    {
        try
        {
            var result = await _sapService.GetVendorAsync(vendorNumber, companyCode);
            
            if (!result.Success)
                return NotFound(new { error = result.ErrorMessage });
            
            return Ok(result);
        }
        catch (NotImplementedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, 
                new { error = "Real SAP not configured", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vendor {VendorNumber}", vendorNumber);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Create new vendor in SAP
    /// </summary>
    [HttpPost("vendor")]
    [ProducesResponseType(typeof(VendorCreateResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VendorCreateResponse>> CreateVendor(
        [FromBody] VendorCreateRequest request)
    {
        try
        {
            var result = await _sapService.CreateVendorAsync(request);
            
            if (!result.Success)
                return BadRequest(new { errors = result.Errors });
            
            return CreatedAtAction(
                nameof(GetVendor), 
                new { vendorNumber = result.VendorNumber, companyCode = request.CompanyCode },
                result);
        }
        catch (NotImplementedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, 
                new { error = "Real SAP not configured", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vendor");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update existing vendor in SAP
    /// </summary>
    [HttpPut("vendor/{vendorNumber}")]
    [ProducesResponseType(typeof(VendorUpdateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VendorUpdateResponse>> UpdateVendor(
        string vendorNumber,
        [FromBody] VendorUpdateRequest request)
    {
        try
        {
            var result = await _sapService.UpdateVendorAsync(vendorNumber, request);
            
            if (!result.Success)
                return BadRequest(new { errors = result.Errors });
            
            return Ok(result);
        }
        catch (NotImplementedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, 
                new { error = "Real SAP not configured", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vendor {VendorNumber}", vendorNumber);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Validate vendor name against SAP business rules
    /// </summary>
    /// <remarks>
    /// SAP Name Validation Rules:
    /// - Maximum 35 characters per field
    /// - Only A-Z, 0-9, space, hyphen, period, comma
    /// - No leading/trailing spaces
    /// - No consecutive spaces
    /// - Cannot be purely numeric
    ///     
    /// Example request:
    ///     
    ///     POST /api/sap/validate/name
    ///     {
    ///       "name": "John Smith",
    ///       "nameType": "PERSON"
    ///     }
    /// </remarks>
    [HttpPost("validate/name")]
    [ProducesResponseType(typeof(NameValidationResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<NameValidationResult>> ValidateName(
        [FromBody] NameValidationRequest request)
    {
        try
        {
            var result = await _sapService.ValidateNameAsync(request);
            return Ok(result);
        }
        catch (NotImplementedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, 
                new { error = "Real SAP not configured", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating name");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Validate bank account details
    /// </summary>
    /// <remarks>
    /// Country-specific validation rules:
    /// - SEPA countries (FR, DE, ES, IT, etc.): IBAN + BIC required
    /// - US: Routing number + Account number required
    /// - Others: SWIFT + Account number
    ///     
    /// Example request for France:
    ///     
    ///     POST /api/sap/validate/bank
    ///     {
    ///       "bankCountry": "FR",
    ///       "iban": "FR7630006000011234567890189",
    ///       "swift": "BNPAFRPPXXX",
    ///       "accountNumber": "12345678901"
    ///     }
    ///     
    /// Example request for US:
    ///     
    ///     POST /api/sap/validate/bank
    ///     {
    ///       "bankCountry": "US",
    ///       "routingNumber": "021000021",
    ///       "accountNumber": "1234567890",
    ///       "swift": "CHASUS33",
    ///       "accountType": "Checking"
    ///     }
    /// </remarks>
    [HttpPost("validate/bank")]
    [ProducesResponseType(typeof(BankValidationResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<BankValidationResult>> ValidateBank(
        [FromBody] BankValidationRequest request)
    {
        try
        {
            var result = await _sapService.ValidateBankAsync(request);
            return Ok(result);
        }
        catch (NotImplementedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, 
                new { error = "Real SAP not configured", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating bank");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Check for duplicate bank accounts
    /// </summary>
    /// <remarks>
    /// Prevents same IBAN being registered to multiple vendors.
    ///     
    /// Example request:
    ///     
    ///     POST /api/sap/bank/check-duplicate
    ///     {
    ///       "iban": "FR7630006000011234567890189",
    ///       "companyCode": "UNES",
    ///       "excludeVendorId": "10189999"
    ///     }
    /// </remarks>
    [HttpPost("bank/check-duplicate")]
    [ProducesResponseType(typeof(BankDuplicateCheckResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<BankDuplicateCheckResult>> CheckBankDuplicate(
        [FromBody] BankDuplicateCheckRequest request)
    {
        try
        {
            var result = await _sapService.CheckBankDuplicateAsync(request);
            return Ok(result);
        }
        catch (NotImplementedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, 
                new { error = "Real SAP not configured", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking bank duplicate");
            return BadRequest(new { error = ex.Message });
        }
    }
}
