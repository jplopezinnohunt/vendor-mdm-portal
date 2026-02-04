using VendorMdm.Core.Framework.Primitives;
using VendorMdm.Shared.Models;

namespace VendorMdm.Api.Services;

public interface IVendorService
{
    /// <summary>
    /// Creates a new vendor with sanctions screening and duplicate detection.
    /// Returns Result.Fail if validation fails (duplicates, sanctions, missing fields).
    /// </summary>
    Task<Result<Vendor>> CreateVendorAsync(Vendor vendor, bool forceCreation = false);

    /// <summary>
    /// Updates an existing vendor.
    /// </summary>
    Task<Result<Vendor>> UpdateVendorAsync(Vendor vendor);

    Task<Vendor?> GetVendorByIdAsync(Guid id);
    Task<Vendor?> GetVendorByEmailAsync(string email);
    Task<List<Vendor>> GetAllVendorsAsync();
}
