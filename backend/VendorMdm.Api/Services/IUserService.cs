using VendorMdm.Shared.Models;

namespace VendorMdm.Api.Services;

/// <summary>
/// Service for managing User canonical entities.
/// Follows the Hexagonal Architecture pattern (Core Domain -> Persistence Port).
/// </summary>
public interface IUserService
{
    Task<User> CreateUserAsync(User user);
    Task<User> UpdateUserAsync(User user);
    Task<User?> UpdateUserRolesAsync(Guid userId, List<string> newRoles, string? authMethod = null);
    Task<User?> GetUserByIdAsync(Guid id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<List<User>> GetAllUsersAsync();
    Task<User?> ToggleBlockStatusAsync(Guid userId, bool isBlocked);
}
