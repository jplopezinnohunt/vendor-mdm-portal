using System.Text.Json;
using VendorMdm.Shared.Models.Sanctions;

namespace VendorMdm.Api.Services;

/// <summary>
/// REAL implementation using OpenSanctions.org API
/// Free for non-commercial use, aggregates 300+ sources including OFAC, UN, EU
/// https://www.opensanctions.org/docs/api/
/// </summary>
public class SanctionsScreeningOpenSanctionsService : ISanctionsScreeningService
{
    private readonly ILogger<SanctionsScreeningOpenSanctionsService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, ScreeningResult> _screeningHistory;

    public SanctionsScreeningOpenSanctionsService(
        ILogger<SanctionsScreeningOpenSanctionsService> logger,
        IConfiguration configuration,
        HttpClient httpClient)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClient;
        _screeningHistory = new Dictionary<string, ScreeningResult>();

        // Configure HTTP client
        var baseUrl = _configuration["Services:SanctionsScreening:RealSettings:BaseUrl"] 
            ?? "https://api.opensanctions.org/";
        _httpClient.BaseAddress = new Uri(baseUrl);
        
        // API key is optional for basic usage but recommended
        var apiKey = _configuration["Services:SanctionsScreening:RealSettings:ApiKey"];
        if (!string.IsNullOrEmpty(apiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"ApiKey {apiKey}");
        }

        _logger.LogInformation("OPENSANCTIONS API: Initialized with base URL {BaseUrl}", baseUrl);
    }

    public async Task<ScreeningResult> ScreenEntityAsync(ScreeningRequest request)
    {
        _logger.LogInformation(
            "OPENSANCTIONS: Screening entity {EntityName} ({EntityType}) for vendor {VendorId}",
            request.EntityName, request.EntityType, request.VendorId);

        var screeningId = Guid.NewGuid().ToString();

        try
        {
            // Call OpenSanctions search API
            // https://api.opensanctions.org/search/default?q={name}
            var searchUrl = $"search/default?q={Uri.EscapeDataString(request.EntityName)}";
            
            // Add additional parameters for better matching
            if (request.DateOfBirth.HasValue)
            {
                searchUrl += $"&born_after={request.DateOfBirth.Value:yyyy-MM-dd}";
                searchUrl += $"&born_before={request.DateOfBirth.Value:yyyy-MM-dd}";
            }

            if (request.Nationalities?.Any() == true)
            {
                searchUrl += $"&countries={string.Join(",", request.Nationalities)}";
            }

            var response = await _httpClient.GetAsync(searchUrl);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<OpenSanctionsSearchResponse>(content);

            if (apiResponse == null || apiResponse.Results == null)
            {
                throw new InvalidOperationException("Invalid response from OpenSanctions API");
            }

            // Convert API results to our model
            var matches = apiResponse.Results
                .Select(result => MapToSanctionsMatch(result, request))
                .Where(match => match.MatchScore >= 0.50m) // Filter low-confidence matches
                .OrderByDescending(m => m.MatchScore)
                .ToList();

            // Determine status and risk level
            var status = "Clear";
            var riskLevel = RiskLevel.Clear;
            var requiresReview = false;
            string? recommendedAction = null;

            if (matches.Any())
            {
                var highestScore = matches.Max(m => m.MatchScore);
                
                if (highestScore >= 0.90m)
                {
                    status = "ConfirmedMatch";
                    riskLevel = RiskLevel.Critical;
                    requiresReview = true;
                    recommendedAction = "REJECT - High confidence sanctions match. Block immediately.";
                }
                else if (highestScore >= 0.75m)
                {
                    status = "PotentialMatch";
                    riskLevel = RiskLevel.High;
                    requiresReview = true;
                    recommendedAction = "REVIEW REQUIRED - Manual compliance review needed.";
                }
                else
                {
                    status = "PotentialMatch";
                    riskLevel = RiskLevel.Medium;
                    requiresReview = false;
                    recommendedAction = "MONITOR - Can proceed with monitoring.";
                }
            }

            var result = new ScreeningResult
            {
                ScreeningId = screeningId,
                ScreenedAt = DateTime.UtcNow,
                VendorId = request.VendorId,
                Status = status,
                OverallRisk = riskLevel,
                Matches = matches,
                RequiresReview = requiresReview,
                RecommendedAction = recommendedAction,
                TotalListsChecked = 300 // OpenSanctions aggregates 300+ sources
            };

            _screeningHistory[screeningId] = result;

            _logger.LogInformation(
                "OPENSANCTIONS: Screening complete - Status: {Status}, Matches: {MatchCount}",
                result.Status, result.Matches.Count);

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "OPENSANCTIONS: HTTP request failed");
            throw new InvalidOperationException(
                "Failed to connect to OpenSanctions API. Check network connectivity and API key.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OPENSANCTIONS: Unexpected error during screening");
            throw;
        }
    }

    public async Task<List<ScreeningResult>> ScreenBatchAsync(List<ScreeningRequest> requests)
    {
        var results = new List<ScreeningResult>();
        
        foreach (var request in requests)
        {
            var result = await ScreenEntityAsync(request);
            results.Add(result);
        }

        return results;
    }

    public Task<ScreeningResult> GetScreeningResultAsync(string screeningId)
    {
        if (_screeningHistory.TryGetValue(screeningId, out var result))
        {
            return Task.FromResult(result);
        }

        throw new KeyNotFoundException($"Screening result {screeningId} not found");
    }

    public async Task<ListsUpdateInfo> GetListsUpdateInfoAsync()
    {
        try
        {
            // OpenSanctions provides metadata about the data
            var response = await _httpClient.GetAsync("metadata");
            response.EnsureSuccessStatusCode();

            // Return basic info
            // (In production, parse the actual metadata response)
            return new ListsUpdateInfo
            {
                LastUpdated = DateTime.UtcNow,
                TotalLists = 300,
                TotalEntries = 0, // Would be in metadata
                ListUpdateDates = new Dictionary<string, DateTime>
                {
                    ["OpenSanctions"] = DateTime.UtcNow
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching lists update info");
            throw;
        }
    }

    private SanctionsMatch MapToSanctionsMatch(OpenSanctionsResult result, ScreeningRequest request)
    {
        // Map OpenSanctions result to our model
        return new SanctionsMatch
        {
            ListName = result.Dataset ?? "OpenSanctions",
            ListSource = "opensanctions.org",
            EntryId = result.Id ?? Guid.NewGuid().ToString(),
            MatchedName = result.Caption ?? "Unknown",
            MatchScore = result.Score ?? 0.5m,
            MatchType = "Name",
            Reason = string.Join(", ", result.Topics ?? new List<string>()),
            SanctionsDetails = result.Schema ?? "Unknown",
            ListUpdateDate = DateTime.TryParse(result.LastChange, out var date) ? date : DateTime.UtcNow,
            ScoreComponents = new MatchScoreComponents
            {
                NameScore = result.Score ?? 0.5m  // OpenSanctions provides overall score
            }
        };
    }

    // DTOs for OpenSanctions API responses
    private class OpenSanctionsSearchResponse
    {
        public List<OpenSanctionsResult>? Results { get; set; }
        public int? Total { get; set; }
    }

    private class OpenSanctionsResult
    {
        public string? Id { get; set; }
        public string? Caption { get; set; }
        public string? Schema { get; set; }
        public List<string>? Topics { get; set; }
        public List<string>? Countries { get; set; }
        public string? Dataset { get; set; }
        public decimal? Score { get; set; }
        public string? LastChange { get; set; }
    }
}
