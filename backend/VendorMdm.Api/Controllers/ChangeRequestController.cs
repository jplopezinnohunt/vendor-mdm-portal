using Microsoft.AspNetCore.Mvc;
using VendorMdm.Api.Models;
using VendorMdm.Api.Services;

namespace VendorMdm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChangeRequestController : ControllerBase
{
    private readonly IChangeRequestRepository _repository;

    public ChangeRequestController(IChangeRequestRepository repository)
    {
        _repository = repository;
    }

    [HttpPost]
    public async Task<IActionResult> CreateChangeRequest([FromBody] CreateChangeRequestDto dto)
    {
        var request = new ChangeRequest
        {
            Id = Guid.NewGuid(),
            RequesterId = dto.RequesterId,
            SapVendorId = dto.SapVendorId,
            Status = "Draft"
        };

        var createdRequest = await _repository.CreateRequestAsync(request, dto.Payload);
        return CreatedAtAction(nameof(GetChangeRequest), new { id = createdRequest.Id }, createdRequest);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetChangeRequest(Guid id)
    {
        var request = await _repository.GetRequestAsync(id);
        if (request == null) return NotFound();
        return Ok(request);
    }

    [HttpPost("{id}/approve")]
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
    }
}

public class CreateChangeRequestDto
{
    public Guid RequesterId { get; set; }
    public string? SapVendorId { get; set; }
    public object Payload { get; set; } = new object();
}
