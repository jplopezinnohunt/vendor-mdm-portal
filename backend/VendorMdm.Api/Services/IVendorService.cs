using VendorMdm.Shared.Models;

namespace VendorMdm.Api.Services;

public interface IVendorService
{
    Task<Vendor> CreateVendorAsync(Vendor vendor, bool forceCreation = false);
    Task<Vendor> UpdateVendorAsync(Vendor vendor);
    Task<Vendor?> GetVendorByIdAsync(Guid id);
    Task<Vendor?> GetVendorByEmailAsync(string email);
    Task<List<Vendor>> GetAllVendorsAsync();
}
