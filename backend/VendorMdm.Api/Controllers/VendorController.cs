using Microsoft.AspNetCore.Mvc;
using VendorMdm.Api.Services;
using VendorMdm.Shared.Contracts.Dtos;
using VendorMdm.Shared.Contracts.Mappings;
using VendorMdm.Shared.Models;

namespace VendorMdm.Api.Controllers;

/// <summary>
/// Vendor API Controller.
/// Pattern 6 Compliant: Returns DTOs, never SQL entities.
/// </summary>
[ApiController]
[Route("api/[controller]")]
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
    public async Task<ActionResult<VendorDto>> CreateVendor([FromBody] CreateVendorRequestDto request, [FromQuery] bool force = false)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Map request DTO to entity
            var vendor = new Vendor
            {
                LegalName = request.LegalName,
                TaxId = request.TaxId,
                PrimaryContactEmail = request.ContactEmail
            };

            var createdVendor = await _service.CreateVendorAsync(vendor, force);
            return CreatedAtAction(nameof(GetVendor), new { id = createdVendor.Id }, createdVendor.ToDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create vendor");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVendor(Guid id, [FromBody] Vendor vendor)
    {
        if (id != vendor.Id)
        {
            return BadRequest("ID mismatch");
        }

        try
        {
            await _service.UpdateVendorAsync(vendor);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update vendor");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
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
