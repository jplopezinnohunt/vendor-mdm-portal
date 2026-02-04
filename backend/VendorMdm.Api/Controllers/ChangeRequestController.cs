using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using VendorMdm.Api.Services;
using VendorMdm.Shared.Constants;
using VendorMdm.Shared.Models;

namespace VendorMdm.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]
public class ChangeRequestController : ControllerBase
{
    private readonly IChangeRequestRepository _repository;

    public ChangeRequestController(IChangeRequestRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Modification API: Accepts flexible CDM JSON payloads.
    /// Route: POST /api/ChangeRequest
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateChangeRequest([FromBody] CreateChangeRequestDto dto)
    {
        var request = new ChangeRequest
        {
            Id = Guid.NewGuid(),
            RequesterId = dto.RequesterId,
            SapVendorId = dto.SapVendorId,
            Status = ChangeRequestStatus.Draft
        };

        var createdRequest = await _repository.CreateRequestAsync(request, dto.Payload);
        // Note: Redirects to the Effective State view
        return CreatedAtAction(nameof(GetEffectiveVendorState), new { id = dto.SapVendorId ?? createdRequest.Id.ToString() }, createdRequest);
    }

    /// <summary>
    /// Internal/Debug: Direct access to Change Request entity.
    /// Route: GET /api/vendor/changerequest/{id}
    /// </summary>
    [HttpGet("changerequest/{id}")]
    public async Task<IActionResult> GetChangeRequest(Guid id)
    {
        var request = await _repository.GetRequestAsync(id);
        if (request == null) return NotFound();
        return Ok(request);
    }

    [HttpPost("changerequest/{id}/approve")]
    public async Task<IActionResult> ApproveChangeRequest(Guid id)
    {
        try
        {
            await _repository.ApproveRequestAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            // State machine validation failure
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Read API: Retrieves current vendor master data (SAP) + Overlay.
    /// Route: GET /api/vendor/{id}
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEffectiveVendorState(string id)
    {
        // Using 'id' which could be a UUID for new vendors or SAP ID for existing.
        // The Repository logic handles the lookup.
        var result = await _repository.GetEffectiveVendorStateAsync(id);
        return Ok(result);
    }
}

public class CreateChangeRequestDto
{
    public Guid RequesterId { get; set; }
    public string? SapVendorId { get; set; }
    public object Payload { get; set; } = new object();
}
