using VendorMdm.Shared.Models;

namespace VendorMdm.Api.Services;

public interface IFundService
{
    Task<Fund> CreateFundAsync(Fund fund);
    Task<Fund> UpdateFundAsync(Fund fund);
    Task<Fund?> GetFundByIdAsync(Guid id);
    Task<List<Fund>> GetAllFundsAsync();
}
