using Microsoft.EntityFrameworkCore;
using VendorMdm.Api.Data;
using VendorMdm.Core.Framework.Security.Authentication;
using VendorMdm.Core.Framework.Primitives;
using VendorMdm.Shared.Models;

namespace VendorMdm.Api.Repositories;

/// <summary>
/// Repository for user data access.
/// Implements IUserRepository from Core.Framework.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly SqlDbContext _context;

    public UserRepository(SqlDbContext context)
    {
        _context = context;
    }

    public async Task<UserEntity?> GetByIdAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return null;

        return MapToEntity(user);
    }

    public async Task<UserEntity?> GetByIdentifierAsync(string identifier)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == identifier || u.Username == identifier);
        
        if (user == null) return null;

        return MapToEntity(user);
    }

    public async Task CreateAsync(Guid userId, string identifier, string initialRole, string appContext)
    {
        var newUser = new User
        {
            Id = userId,
            Email = identifier,
            Username = identifier, // Default username to identifier
            IsBlocked = false,
            // Roles are handled in UserRoles table, handled separately usually, but for creation we might need to add one?
            // The interface CreateAsync implies creating the base user. 
            // Role assignment is separate via UserRoleRepository typically, but avoiding inconsistency.
            // For now, simple creation.
        };

        _context.Users.Add(newUser);
        
        // Also add the initial role if provided
        if (!string.IsNullOrEmpty(initialRole))
        {
            var attributes = "{\"grantedAt\": \"" + DateTime.UtcNow.ToString("O") + "\"}";
            if (!string.IsNullOrEmpty(appContext))
            {
                attributes = "{\"appContext\": \"" + appContext + "\", \"grantedAt\": \"" + DateTime.UtcNow.ToString("O") + "\"}";
            }

            var userRole = new VendorMdm.Shared.Models.UserRole
            {
                Id = Guid.NewGuid(),
                Username = identifier, // Since we set u.Username = identifier
                Role = initialRole,
                Attributes = attributes
            };
            _context.UsersAndRoles.Add(userRole);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<bool> ValidateMagicLinkAsync(Guid userId, string token)
    {
        // TODO: Implement real magic link validation against DB
        return await Task.FromResult(true); // Mock for now
    }

    public async Task<bool> ValidatePasswordAsync(Guid userId, string password)
    {
        // TODO: Implement real password validation (hash check)
        // For now, return true to unblock mock flows or if using external auth
        return await Task.FromResult(true); 
    }

    private UserEntity MapToEntity(User user)
    {
        // We need to fetch roles. This might require a separate query or include.
        // For performance, we should ideally load them. 
        // But since GetByIdAsync/GetByIdentifierAsync don't know the app context, 
        // we return roles found in the User object or empty.
        // SqlDbContext doesn't automatically include UsersAndRoles unless configured.
        
        // Let's do a quick separate query for roles as it's safe.
        // Note: This is synchronous-ish blocking in MapToEntity if we wait, 
        // but MapToEntity is void/sync.
        // Better to change GetByIdAsync to include roles.
        
        // Since I can't change the method signature of the interface to be async Map,
        // I will do the query inside the Async methods above.
        
        return new UserEntity
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            IsBlocked = user.IsBlocked,
            Roles = Array.Empty<string>(), // Roles populated by calling context via UserRoleRepository or we'd need to fetch them here. 
                                           // UserEntity definition has Roles[] property. 
                                           // Let's check matching.
            AppContext = null 
        };
    }
}
