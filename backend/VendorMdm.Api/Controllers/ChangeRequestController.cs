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

        var result = await _repository.CreateRequestAsync(request, dto.Payload);
        if (result.IsFailure)
            return BadRequest(new { message = result.Error });

        // Note: Redirects to the Effective State view
        return CreatedAtAction(nameof(GetEffectiveVendorState), new { id = dto.SapVendorId ?? result.Value.Id.ToString() }, result.Value);
    }

    /// <summary>
    /// Internal/Debug: Direct access to Change Request entity.
    /// Route: GET /api/changerequest/{id}
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetChangeRequest(Guid id)
    {
        var result = await _repository.GetRequestAsync(id);
        if (result.IsFailure)
            return NotFound(new { message = result.Error });

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> ApproveChangeRequest(Guid id)
    {
        var result = await _repository.ApproveRequestAsync(id);
        if (result.IsFailure)
        {
            // Determine if it's a not-found error or a validation error
            if (result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { message = result.Error });

            return BadRequest(new { message = result.Error });
        }

        return NoContent();
    }

    /// <summary>
    /// Read API: Retrieves current vendor master data (SAP) + Overlay.
    /// Route: GET /api/changerequest/vendor/{id}
    /// </summary>
    [HttpGet("vendor/{id}")]
    public async Task<IActionResult> GetEffectiveVendorState(string id)
    {
        // Using 'id' which could be a UUID for new vendors or SAP ID for existing.
        // The Repository logic handles the lookup.
        var result = await _repository.GetEffectiveVendorStateAsync(id);
        if (result.IsFailure)
            return NotFound(new { message = result.Error });

        return Ok(result.Value);
    }
}

public class CreateChangeRequestDto
{
    public Guid RequesterId { get; set; }
    public string? SapVendorId { get; set; }
    public object Payload { get; set; } = new object();
}
