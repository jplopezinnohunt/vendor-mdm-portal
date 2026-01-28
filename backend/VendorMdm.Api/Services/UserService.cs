using VendorMdm.Api.Data;
using VendorMdm.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace VendorMdm.Api.Services;

public class UserService : IUserService
{
    private readonly SqlDbContext _context;
    private readonly ICosmosRepository _cosmosRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(
        SqlDbContext context, 
        ICosmosRepository cosmosRepository,
        ILogger<UserService> logger)
    {
        _context = context;
        _cosmosRepository = cosmosRepository;
        _logger = logger;
    }

    public async Task<User> CreateUserAsync(User user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        // 1. Check if exists
        if (await _context.Users.AnyAsync(u => u.Email == user.Email))
        {
            throw new InvalidOperationException($"User with email {user.Email} already exists.");
        }

        // 2. Setup defaults
        if (user.Roles == null || !user.Roles.Any()) user.Roles = new List<string> { "Viewer" };
        
        user.Id = Guid.NewGuid();
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        user.EntityVersion = 1;
        user.Status = "Active";
        user.SourceSystem = "Portal"; 
        user.SchemaVersion = "v1.0.0";
        user.AuthProvider = "Local"; // Default for API created users

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("User created in SQL: {Id} ({Roles})", user.Id, string.Join(",", user.Roles));

        // 3. Artifact & Event
        await LogUserArtifactAsync(user, "UserCreated");

        return user;
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        user.EntityVersion++;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        await LogUserArtifactAsync(user, "UserUpdated");
        return user;
    }

    public async Task<User?> UpdateUserRolesAsync(Guid userId, List<string> newRoles, string? authMethod = null)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return null;

        var oldRoles = string.Join(",", user.Roles);
        user.Roles = newRoles;
        
        if (!string.IsNullOrEmpty(authMethod))
        {
            user.AuthMethod = authMethod;
        }

        user.UpdatedAt = DateTime.UtcNow;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("User Updated: {Id} Roles: {OldRoles} -> {NewRoles}, Auth: {AuthMethod}", userId, oldRoles, string.Join(",", newRoles), authMethod ?? "Unchanged");

        await LogUserArtifactAsync(user, "UserRoleUpdated");
        return user;
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        var users = await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        // Self-Healing: Fix "Pending" users who have actually logged in
        // This handles legacy users from before the fix in AuthController
        bool changesMade = false;
        foreach (var user in users)
        {
            if (user.Status == "Pending" && user.LastLogonAt.HasValue)
            {
                user.Status = "Active";
                user.InvitationToken = null; // Clean up
                user.InvitationExpiresAt = null;
                changesMade = true;
            }
        }

        if (changesMade)
        {
            await _context.SaveChangesAsync();
        }

        return users;
    }

    public async Task<User?> ToggleBlockStatusAsync(Guid userId, bool isBlocked)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return null;

        user.IsBlocked = isBlocked;
        user.UpdatedAt = DateTime.UtcNow;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        _logger.LogWarning("User Block Status Changed: {Id} -> {IsBlocked}", userId, isBlocked);
        
        await LogUserArtifactAsync(user, isBlocked ? "UserBlocked" : "UserUnblocked");
        return user;
    }

    private async Task LogUserArtifactAsync(User user, string eventType)
    {
        try 
        {
            await _cosmosRepository.SaveArtifactAsync(user.Id.ToString(), user);
            
            var domainEvent = new DomainEvent
            {
                EventType = eventType,
                EntityId = user.Id.ToString(),
                Data = user,
                Timestamp = DateTime.UtcNow,
                Source = "VendorMdm.Api",
                SchemaVersion = user.SchemaVersion
            };
            await _cosmosRepository.LogDomainEventAsync(domainEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log User artifact/event for {Id}", user.Id);
        }
    }
}
