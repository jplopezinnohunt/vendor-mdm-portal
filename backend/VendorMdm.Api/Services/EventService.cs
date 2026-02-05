using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VendorMdm.Api.Data;
using VendorMdm.Core.Framework.Primitives;
using VendorMdm.Shared.Models;
using VendorMdm.Api.Models;


namespace VendorMdm.Api.Services;

public interface IEventService
{
    Task<Result<Event>> CreateEventAsync(Event eventEntity);
    Task<Result<Event>> GetEventByIdAsync(Guid id);
    Task<Result<List<Event>>> GetAllEventsAsync();
    Task<Result<List<EventParticipant>>> AddParticipantsAsync(Guid eventId, List<EventParticipant> participants);
    Task<Result<List<EventParticipant>>> GetParticipantsAsync(Guid eventId);
    Task<Result<EventParticipant>> UpdateParticipantAsync(Guid eventId, Guid participantId, EventParticipant updatedParticipant);
    Task<Result<int>> InviteTier3ParticipantsAsync(Guid eventId, List<Guid> participantIds, Guid requestedBy, string requestedByName);
}

public class EventService : IEventService
{
    private readonly SqlDbContext _context;
    private readonly IInvitationService _invitationService;
    private readonly ILogger<EventService> _logger;

    public EventService(SqlDbContext context, IInvitationService invitationService, ILogger<EventService> logger)
    {
        _context = context;
        _invitationService = invitationService;
        _logger = logger;
    }

    public async Task<Result<EventParticipant>> UpdateParticipantAsync(Guid eventId, Guid participantId, EventParticipant updated)
    {
        var invitation = await _context.VendorInvitations
            .FirstOrDefaultAsync(p => p.EventId == eventId && p.Id == participantId);

        if (invitation == null)
            return Result.Fail<EventParticipant>($"Participant with ID '{participantId}' not found in event '{eventId}'.");

        // Only allow updates if not yet linked to SAP or final state
        if (invitation.Status == "SAP_CREATED" || invitation.Status == "Completed")
        {
            return Result.Fail<EventParticipant>("Cannot update participant in final state.");
        }

        invitation.VendorLegalName = updated.FullName;
        invitation.PrimaryContactEmail = updated.Email;
        invitation.Tier = updated.Tier;
        // invitation.Attributes = updated.Attributes; // Optional

        await _context.SaveChangesAsync();

        // Map back to EventParticipant for return
        return Result.Ok(new EventParticipant
        {
            Id = invitation.Id,
            EventId = invitation.EventId ?? Guid.Empty,
            FullName = invitation.VendorLegalName,
            Email = invitation.PrimaryContactEmail,
            Tier = invitation.Tier ?? "Tier_3",
            Status = invitation.Status,
            VendorInviteId = invitation.Id
        });
    }

    public async Task<Result<Event>> CreateEventAsync(Event eventEntity)
    {
        eventEntity.Id = Guid.NewGuid();
        eventEntity.CreatedAt = DateTime.UtcNow;

        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        return Result.Ok(eventEntity);
    }

    public async Task<Result<Event>> GetEventByIdAsync(Guid id)
    {
        var eventEntity = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (eventEntity == null)
            return Result.Fail<Event>($"Event with ID '{id}' not found.");

        return Result.Ok(eventEntity);
    }

    public async Task<Result<List<Event>>> GetAllEventsAsync()
    {
        var events = await _context.Events.OrderByDescending(e => e.CreatedAt).ToListAsync();
        return Result.Ok(events);
    }

    public async Task<Result<List<EventParticipant>>> AddParticipantsAsync(Guid eventId, List<EventParticipant> participants)
    {
        var eventExists = await _context.Events.AnyAsync(e => e.Id == eventId);
        if (!eventExists)
            return Result.Fail<List<EventParticipant>>($"Event with ID '{eventId}' not found.");

        var addedParticipants = new List<EventParticipant>();

        foreach (var p in participants)
        {
            // Check duplicates in VendorInvitations
            var exists = await _context.VendorInvitations
                .AnyAsync(i => i.EventId == eventId && i.PrimaryContactEmail == p.Email);

            if (!exists)
            {
                var invitation = new VendorInvitation
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    VendorLegalName = p.FullName,
                    PrimaryContactEmail = p.Email,
                    VendorType = "Individual",
                    Tier = p.Tier, // Tier_1/2/3
                    Status = InvitationStatus.Draft, // Draft until invited
                    InvitationToken = Guid.NewGuid().ToString(), // Placeholder
                    ExpiresAt = DateTime.UtcNow.AddDays(30), // Placeholder
                    InvitedBy = Guid.Empty, // System or unknown for draft
                    InvitedByName = "System",
                    CreatedAt = DateTime.UtcNow,
                    CurrentStage = "Draft",
                    Attributes = "{}"
                };

                _context.VendorInvitations.Add(invitation);

                // Map back to return
                p.Id = invitation.Id;
                p.Status = "Draft";
                addedParticipants.Add(p);
            }
        }

        await _context.SaveChangesAsync();
        return Result.Ok(addedParticipants);
    }

    public async Task<Result<List<EventParticipant>>> GetParticipantsAsync(Guid eventId)
    {
        // Query VendorInvitations for this event
        var invitations = await _context.VendorInvitations
            .Where(i => i.EventId == eventId)
            .ToListAsync();

        // Project to EventParticipant DTO model for frontend compatibility
        var participants = invitations.Select(i => new EventParticipant
        {
            Id = i.Id,
            EventId = i.EventId ?? Guid.Empty,
            FullName = i.VendorLegalName,
            Email = i.PrimaryContactEmail,
            Tier = i.Tier ?? "Tier_3",
            Status = i.Status, // Draft, Pending, etc.
            VendorInviteId = i.Id,
            Attributes = i.Attributes
        }).ToList();

        return Result.Ok(participants);
    }

    public async Task<Result<int>> InviteTier3ParticipantsAsync(Guid eventId, List<Guid> participantIds, Guid requestedBy, string requestedByName)
    {
        int successCount = 0;

        foreach (var id in participantIds)
        {
            try
            {
                // Verify it belongs to this event first?
                var inv = await _context.VendorInvitations
                    .FirstOrDefaultAsync(i => i.Id == id && i.EventId == eventId);

                if (inv == null) continue;
                if (inv.Tier != "TIER_3") continue;
                if (inv.Status != InvitationStatus.Draft && inv.Status != InvitationStatus.Pending && inv.Status != InvitationStatus.Expired) continue;

                // Use ResendInvitationAsync to handle Token Generation, Status Update, and Email Sending
                // This converts Draft -> Pending automatically if logic allows, or we update status first?
                // ResendInvitationAsync logic: Generates Token, Sets Status=Pending, Sends Email.
                // It works for any non-completed invitation.

                var result = await _invitationService.ResendInvitationAsync(id, requestedBy);

                if (result.Success)
                {
                    successCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to invite participant {Id}", id);
            }
        }

        // SaveChanges is handled inside ResendInvitationAsync per iteration.
        return Result.Ok(successCount);
    }
}
