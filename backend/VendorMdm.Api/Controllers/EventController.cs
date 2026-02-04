using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorMdm.Api.Services;
using VendorMdm.Shared.Models;

namespace VendorMdm.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/events")]
[Route("api/events")]
[Authorize(Roles = "Requestor,MasterDataUnit,Admin")] // Enforcing Role Requirement
public class EventController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly ILogger<EventController> _logger;

    public EventController(IEventService eventService, ILogger<EventController> logger)
    {
        _eventService = eventService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateEvent([FromBody] Event eventEntity)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Set CreatedBy from Claims if needed, but for now assuming it's passed or handled by service
        // Ideally: var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var result = await _eventService.CreateEventAsync(eventEntity);
        if (result.IsFailure)
            return BadRequest(new { message = result.Error });

        return CreatedAtAction(nameof(GetEvent), new { id = result.Value.Id }, result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllEvents()
    {
        var result = await _eventService.GetAllEventsAsync();
        if (result.IsFailure)
            return BadRequest(new { message = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEvent(Guid id)
    {
        var result = await _eventService.GetEventByIdAsync(id);
        if (result.IsFailure)
            return NotFound(new { message = result.Error });

        return Ok(result.Value);
    }

    [HttpPost("{id}/participants")]
    public async Task<IActionResult> AddParticipants(Guid id, [FromBody] List<EventParticipant> participants)
    {
        var result = await _eventService.AddParticipantsAsync(id, participants);
        if (result.IsFailure)
        {
            if (result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { message = result.Error });

            return BadRequest(new { message = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpGet("{id}/participants")]
    public async Task<IActionResult> GetParticipants(Guid id)
    {
        var result = await _eventService.GetParticipantsAsync(id);
        if (result.IsFailure)
            return BadRequest(new { message = result.Error });

        return Ok(result.Value);
    }

    [HttpPut("{id}/participants/{participantId}")]
    public async Task<IActionResult> UpdateParticipant(Guid id, Guid participantId, [FromBody] EventParticipant participant)
    {
        if (participantId != participant.Id) return BadRequest("ID mismatch");

        var result = await _eventService.UpdateParticipantAsync(id, participantId, participant);
        if (result.IsFailure)
        {
            if (result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { message = result.Error });

            return BadRequest(new { message = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpPost("{id}/invite-tier3")]
    public async Task<IActionResult> InviteParticipants(Guid id, [FromBody] InviteRequestDto request)
    {
        // Get user info from token
        // For simplicity reusing a default ID if claim missing, but in prod use User.Claims
        var requestedBy = Guid.Empty; // Should be User ID
        var requestedByName = User.Identity?.Name ?? "Unknown";

        var result = await _eventService.InviteTier3ParticipantsAsync(id, request.ParticipantIds, requestedBy, requestedByName);
        if (result.IsFailure)
            return BadRequest(new { message = result.Error });

        return Ok(new { InvitedCount = result.Value });
    }
}

public class InviteRequestDto
{
    public List<Guid> ParticipantIds { get; set; } = new();
}
