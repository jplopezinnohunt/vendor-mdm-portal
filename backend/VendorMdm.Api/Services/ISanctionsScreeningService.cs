using VendorMdm.Shared.Models.Sanctions;

namespace VendorMdm.Api.Services;

/// <summary>
/// Interface for sanctions screening service
/// Checks entities against global sanctions lists (OFAC, UN, EU, PEPs, etc.)
/// </summary>
public interface ISanctionsScreeningService
{
    /// <summary>
    /// Screen a single entity against all sanctions lists
    /// </summary>
    Task<ScreeningResult> ScreenEntityAsync(ScreeningRequest request);

    /// <summary>
    /// Batch screening for multiple entities
    /// </summary>
    Task<List<ScreeningResult>> ScreenBatchAsync(List<ScreeningRequest> requests);

    /// <summary>
    /// Get screening result by ID
    /// </summary>
    Task<ScreeningResult> GetScreeningResultAsync(string screeningId);

    /// <summary>
    /// Get information about when lists were last updated
    /// </summary>
    Task<ListsUpdateInfo> GetListsUpdateInfoAsync();
}
