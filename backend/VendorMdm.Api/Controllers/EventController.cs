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
        
        var created = await _eventService.CreateEventAsync(eventEntity);
        return CreatedAtAction(nameof(GetEvent), new { id = created.Id }, created);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllEvents()
    {
        var events = await _eventService.GetAllEventsAsync();
        return Ok(events);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEvent(Guid id)
    {
        var evt = await _eventService.GetEventByIdAsync(id);
        if (evt == null) return NotFound();
        return Ok(evt);
    }

    [HttpPost("{id}/participants")]
    public async Task<IActionResult> AddParticipants(Guid id, [FromBody] List<EventParticipant> participants)
    {
        try
        {
            var added = await _eventService.AddParticipantsAsync(id, participants);
            return Ok(added);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Event not found");
        }
    }

    [HttpGet("{id}/participants")]
    public async Task<IActionResult> GetParticipants(Guid id)
    {
        var participants = await _eventService.GetParticipantsAsync(id);
        return Ok(participants);
    }

    [HttpPut("{id}/participants/{participantId}")]
    public async Task<IActionResult> UpdateParticipant(Guid id, Guid participantId, [FromBody] EventParticipant participant)
    {
        if (participantId != participant.Id) return BadRequest("ID mismatch");
        
        try
        {
            var updated = await _eventService.UpdateParticipantAsync(id, participantId, participant);
            if (updated == null) return NotFound();
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/invite-tier3")]
    public async Task<IActionResult> InviteParticipants(Guid id, [FromBody] InviteRequestDto request)
    {
        // Get user info from token
        // For simplicity reusing a default ID if claim missing, but in prod use User.Claims
        var requestedBy = Guid.Empty; // Should be User ID
        var requestedByName = User.Identity?.Name ?? "Unknown";

        var count = await _eventService.InviteTier3ParticipantsAsync(id, request.ParticipantIds, requestedBy, requestedByName);
        return Ok(new { InvitedCount = count });
    }
}

public class InviteRequestDto
{
    public List<Guid> ParticipantIds { get; set; } = new();
}
