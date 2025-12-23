using Microsoft.AspNetCore.Mvc;
using VendorMdm.Api.Services;
using VendorMdm.Shared.Models;

namespace VendorMdm.Api.Controllers;

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
    public async Task<ActionResult<Vendor>> CreateVendor([FromBody] Vendor vendor)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var createdVendor = await _service.CreateVendorAsync(vendor);
            return CreatedAtAction(nameof(GetVendor), new { id = createdVendor.Id }, createdVendor);
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
    public async Task<ActionResult<Vendor>> GetVendor(Guid id)
    {
        try 
        {
            var vendor = await _service.GetVendorByIdAsync(id);
            if (vendor == null) return NotFound();
            return Ok(vendor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get vendor");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Vendor>>> GetAllVendors()
    {
        try
        {
            var vendors = await _service.GetAllVendorsAsync();
            return Ok(vendors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get vendors");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Vendor>>> SearchVendors([FromQuery] string query)
    {
        // Simple search implementation
        try
        {
            var vendors = await _service.GetAllVendorsAsync();
            if (string.IsNullOrWhiteSpace(query))
            {
                return Ok(vendors); // Return all if no query
            }

            var results = vendors.Where(v => 
                (v.LegalName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (v.TaxId?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)
            ).ToList();

            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search vendors");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
