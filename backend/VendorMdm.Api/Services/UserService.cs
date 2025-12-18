using VendorMdm.Api.Data;
using VendorMdm.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace VendorMdm.Api.Services;

public class UserService : IUserService
{
    private readonly SqlDbContext _context;
    private readonly CosmosRepository _cosmosRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(
        SqlDbContext context, 
        CosmosRepository cosmosRepository,
        ILogger<UserService> logger)
    {
        _context = context;
        _cosmosRepository = cosmosRepository;
        _logger = logger;
    }

    public async Task<User> CreateUserAsync(User user)
    {
        // 0. Validate (already done by Controller/Schema usually, but good to be safe)
        if (user == null) throw new ArgumentNullException(nameof(user));

        // 1. SQL Persistence (State Store)
        user.Id = Guid.NewGuid();
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        user.EntityVersion = 1;
        user.Status = "Active"; // Default
        user.SourceSystem = "Portal"; 
        user.SchemaVersion = "v1.0.0";

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("User created in SQL: {Id}", user.Id);

        // 2. Functional Log (Cosmos Artifact)
        try 
        {
            await _cosmosRepository.SaveArtifactAsync(user.Id.ToString(), user);
        }
        catch (Exception ex)
        {
            // Non-blocking failure for artifact
            _logger.LogError(ex, "Failed to save User artifact to Cosmos: {Id}", user.Id);
        }

        // 3. Outbound Port (Event Bus)
        try
        {
            var domainEvent = new DomainEvent
            {
                EventType = "UserCreated",
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
            _logger.LogError(ex, "Failed to emit UserCreated event: {Id}", user.Id);
        }

        return user;
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        // 1. SQL
        user.UpdatedAt = DateTime.UtcNow;
        user.EntityVersion++; // Optimistic Concurrency

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        // 2. Artifact
        await _cosmosRepository.SaveArtifactAsync(user.Id.ToString(), user);

        // 3. Event
        var domainEvent = new DomainEvent
        {
            EventType = "UserUpdated",
            EntityId = user.Id.ToString(),
            Data = user,
            Timestamp = DateTime.UtcNow,
            Source = "VendorMdm.Api",
            SchemaVersion = user.SchemaVersion
        };
        await _cosmosRepository.LogDomainEventAsync(domainEvent);

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
}
