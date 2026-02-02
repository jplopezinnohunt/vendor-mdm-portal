using VendorMdm.Shared.Models;

namespace VendorMdm.Infrastructure.Ports
{
    /// <summary>
    /// Repository for VendorInvitation entity.
    /// Provides domain-specific query methods.
    /// </summary>
    public interface IVendorInvitationRepository
    {
        Task<VendorInvitation?> GetByIdAsync(Guid id);
        Task<VendorInvitation?> GetByTokenAsync(string token);
        Task<IEnumerable<VendorInvitation>> GetAllAsync();
        Task<IEnumerable<VendorInvitation>> GetByStatusAsync(string status);
        Task<VendorInvitation> CreateAsync(VendorInvitation invitation);
        Task UpdateAsync(VendorInvitation invitation);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}
