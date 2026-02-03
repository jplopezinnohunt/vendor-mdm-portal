using Microsoft.EntityFrameworkCore;
using VendorMdm.Infrastructure.Ports;
using VendorMdm.Shared.Models;

namespace VendorMdm.Infrastructure.Repositories
{
    /// <summary>
    /// SQL implementation of VendorInvitation repository.
    /// Encapsulates all database access for invitations.
    /// Uses generic DbContext to avoid circular dependencies.
    /// </summary>
    public class VendorInvitationRepository : IVendorInvitationRepository
    {
        private readonly DbContext _context;

        public VendorInvitationRepository(DbContext context)
        {
            _context = context;
        }

        public async Task<VendorInvitation?> GetByIdAsync(Guid id)
        {
            return await _context.Set<VendorInvitation>().FindAsync(id);
        }

        public async Task<VendorInvitation?> GetByTokenAsync(string token)
        {
            return await _context.Set<VendorInvitation>()
                .FirstOrDefaultAsync(i => i.InvitationToken == token);
        }

        public async Task<IEnumerable<VendorInvitation>> GetAllAsync()
        {
            return await _context.Set<VendorInvitation>()
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<VendorInvitation>> GetByStatusAsync(string status)
        {
            return await _context.Set<VendorInvitation>()
                .Where(i => i.Status == status)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<VendorInvitation> CreateAsync(VendorInvitation invitation)
        {
            _context.Set<VendorInvitation>().Add(invitation);
            await _context.SaveChangesAsync();
            return invitation;
        }

        public async Task UpdateAsync(VendorInvitation invitation)
        {
            _context.Set<VendorInvitation>().Update(invitation);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var invitation = await GetByIdAsync(id);
            if (invitation != null)
            {
                _context.Set<VendorInvitation>().Remove(invitation);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Set<VendorInvitation>().AnyAsync(i => i.Id == id);
        }
    }
}
