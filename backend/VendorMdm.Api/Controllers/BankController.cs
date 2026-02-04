using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using VendorMdm.Api.Services;
using VendorMdm.Api.Services.Helpers;
using VendorMdm.Shared.Models;
using VendorMdm.Shared.Models.SapSimulation;

namespace VendorMdm.Api.Controllers;

/// <summary>
/// Bank operations controller
/// UI Configuration: From SAP (show/hide fields, mandatory rules)
/// Validation Logic: From ibankit/international standards (ISO 13616, ISO 9362)
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/bank")]
[Route("api/bank")]
public class BankController : ControllerBase
{
    private readonly ISapVendorService _sapService;
    private readonly IbanValidator _ibanValidator;
    private readonly SwiftValidator _swiftValidator;
    private readonly ILogger<BankController> _logger;

    public BankController(
        ISapVendorService sapService,
        IbanValidator ibanValidator,
        SwiftValidator swiftValidator,
        ILogger<BankController> logger)
    {
        _sapService = sapService;
        _ibanValidator = ibanValidator;
        _swiftValidator = swiftValidator;
        _logger = logger;
    }

    /// <summary>
    /// Get bank field configuration for a specific country FROM SAP
    /// Returns UI configuration: which fields to show/hide, which are mandatory
    /// Does NOT return validation logic - use ibankit (client-side) for that
    /// </summary>
    /// <param name="request">Country code and company code</param>
    /// <returns>Bank UI configuration from SAP business rules</returns>
    /// <remarks>
    /// Example request for France:
    /// POST /api/bank/configuration
    /// {
    ///   "countryCode": "FR",
    ///   "companyCode": "UNES"
    /// }
    /// 
    /// Response includes (from SAP):
    /// - Field visibility (showIBAN: true, showControlKey: false)
    /// - Mandatory flags (ibanMandatory: true)
    /// - SAP mapping (primaryBankKey: "IBAN", paymentMethod: "SEPA_DD")
    /// </remarks>
    [HttpPost("configuration")]
    [ProducesResponseType(typeof(BankCountryConfigResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetConfiguration([FromBody] BankConfigRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CountryCode))
        {
            return BadRequest(new { error = "Country code is required" });
        }

        try
        {
            // Call SAP to get UI configuration (show/hide, mandatory fields)
            var sapConfig = await _sapService.GetBankCountryConfigurationAsync(
                request.CountryCode, 
                request.CompanyCode ?? "UNES");
            
            _logger.LogInformation(
                "Retrieved bank configuration from SAP for country {Country}: IBAN={ShowIBAN}, ControlKey={ShowControlKey}", 
                request.CountryCode, sapConfig.ShowIBAN, sapConfig.ShowControlKey);
            
            return Ok(sapConfig);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Country {Country} not configured in SAP", request.CountryCode);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving bank configuration from SAP for country {Country}", request.CountryCode);
            return StatusCode(500, new { error = "Failed to retrieve configuration from SAP" });
        }
    }

    /// <summary>
    /// Validate IBAN using ISO 13616 MOD-97 algorithm
    /// No SAP call - uses international standard checksum validation
    /// </summary>
    /// <param name="request">IBAN and country code</param>
    /// <returns>Validation result with extracted bank code and account number</returns>
    /// <remarks>
    /// Example:
    /// POST /api/bank/validate-iban
    /// {
    ///   "iban": "FR7630006000011234567890189",
    ///   "country": "FR"
    /// }
    /// 
    /// Returns:
    /// {
    ///   "valid": true,
    ///   "checksumValid": true,
    ///   "bankCode": "30006",
    ///   "accountNumber": "12345678901",
    ///   "country": "FR"
    /// }
    /// </remarks>
    [HttpPost("validate-iban")]
    [ProducesResponseType(typeof(IbanValidation), 200)]
    [ProducesResponseType(400)]
    public IActionResult ValidateIban([FromBody] IbanValidationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Iban))
        {
            return BadRequest(new { error = "IBAN is required" });
        }

        var result = _ibanValidator.Validate(request.Iban);
        
        _logger.LogInformation("IBAN validation: {IBAN} - Valid: {Valid}", 
            request.Iban, result.Valid);

        return Ok(result);
    }

    /// <summary>
    /// Validate SWIFT/BIC code using ISO 9362 format rules
    /// No SAP call - validates format only, does not check if bank exists
    /// </summary>
    /// <param name="request">SWIFT code and country</param>
    /// <returns>Validation result with extracted components</returns>
    /// <remarks>
    /// Example:
    /// POST /api/bank/validate-swift
    /// {
    ///   "swift": "BNPAFRPPXXX",
    ///   "country": "FR"
    /// }
    /// 
    /// Returns:
    /// {
    ///   "valid": true,
    ///   "bankCode": "BNPA",
    ///   "countryCode": "FR",
    ///   "locationCode": "PP",
    ///   "branchCode": "XXX"
    /// }
    /// </remarks>
    [HttpPost("validate-swift")]
    [ProducesResponseType(typeof(SwiftValidation), 200)]
    [ProducesResponseType(400)]
    public IActionResult ValidateSwift([FromBody] SwiftValidationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Swift))
        {
            return BadRequest(new { error = "SWIFT code is required" });
        }

        var result = _swiftValidator.Validate(request.Swift);
        
        _logger.LogInformation("SWIFT validation: {SWIFT} - Valid: {Valid}", 
            request.Swift, result.Valid);

        return Ok(result);
    }
}

// Request/Response models
public class IbanValidationRequest
{
    public string Iban { get; set; } = string.Empty;
    public string? Country { get; set; }
}

public class SwiftValidationRequest
{
    public string Swift { get; set; } = string.Empty;
    public string? Country { get; set; }
}

public class BankConfigRequest
{
    public string CountryCode { get; set; } = string.Empty;
    public string? CompanyCode { get; set; }
}
