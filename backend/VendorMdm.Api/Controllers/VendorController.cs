using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using VendorMdm.Api.Services;
using VendorMdm.Core.Framework.Primitives;
using VendorMdm.Shared.Contracts.Dtos;
using VendorMdm.Shared.Contracts.Mappings;
using VendorMdm.Shared.Models;

namespace VendorMdm.Api.Controllers;

/// <summary>
/// Vendor API Controller.
/// Pattern 6 Compliant: Returns DTOs, never SQL entities.
/// API Versioning: Supports /api/v1/vendor routes (Brain v1.2.0 compliance).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")] // Backwards compatibility during migration
public class VendorController : ControllerBase
{
    private readonly IVendorService _service;
    private readonly ILogger<VendorController> _logger;

    public VendorController(IVendorService service, ILogger<VendorController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateVendor([FromBody] CreateVendorRequestDto request, [FromQuery] bool force = false)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Map request DTO to entity
        var vendor = new Vendor
        {
            LegalName = request.LegalName,
            TaxId = request.TaxId,
            PrimaryContactEmail = request.ContactEmail
        };

        var result = await _service.CreateVendorAsync(vendor, force);

        // Use Result pattern - no try/catch needed for business logic errors
        return result.ToCreatedResult(
            actionName: nameof(GetVendor),
            routeValuesFactory: v => new { id = v.Id },
            dtoTransform: v => v.ToDto());
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVendor(Guid id, [FromBody] Vendor vendor)
    {
        if (id != vendor.Id)
        {
            return BadRequest("ID mismatch");
        }

        var result = await _service.UpdateVendorAsync(vendor);

        // Use Result pattern - returns 204 NoContent on success, 400 BadRequest on failure
        return result.ToActionResult(_ => NoContent());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VendorDto>> GetVendor(Guid id)
    {
        try
        {
            var vendor = await _service.GetVendorByIdAsync(id);
            if (vendor == null) return NotFound();
            return Ok(vendor.ToDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get vendor");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VendorDto>>> GetAllVendors()
    {
        try
        {
            var vendors = await _service.GetAllVendorsAsync();
            return Ok(vendors.ToDtoList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get vendors");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<VendorDto>>> SearchVendors([FromQuery] string query)
    {
        try
        {
            var vendors = await _service.GetAllVendorsAsync();
            if (string.IsNullOrWhiteSpace(query))
            {
                return Ok(vendors.ToDtoList());
            }

            var results = vendors.Where(v =>
                (v.LegalName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (v.TaxId?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)
            ).ToDtoList();

            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search vendors");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
