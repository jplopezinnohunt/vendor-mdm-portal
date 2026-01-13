using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using VendorMdm.Api.Services.Helpers;
using VendorMdm.Shared.Models.Sanctions;

namespace VendorMdm.Api.Services;

/// <summary>
/// Screening service that downloads the official US Treasury OFAC SDN List (CSV)
/// and performs local fuzzy matching.
/// Free, no API key required, high reliability.
/// Source: https://sanctionslistservice.ofac.treas.gov/api/publicationpreview/exports/sdn.csv
/// </summary>
public class SanctionsScreeningOfacSourceService : ISanctionsScreeningService
{
    private readonly ILogger<SanctionsScreeningOfacSourceService> _logger;
    private readonly HttpClient _httpClient;
    private readonly LevenshteinMatcher _matcher;
    
    // Cache the list in memory to avoid downloading on every request
    private static List<OfacSdnEntry> _cachedEntries = new();
    private static DateTime _lastUpdate = DateTime.MinValue;
    private static readonly SemaphoreSlim _lock = new(1, 1);
    
    // Refresh list every 24 hours
    private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(24);
    private readonly string _sourceUrl;

    public SanctionsScreeningOfacSourceService(
        ILogger<SanctionsScreeningOfacSourceService> logger,
        HttpClient httpClient,
        IConfiguration configuration,
        LevenshteinMatcher matcher)
    {
        _logger = logger;
        _httpClient = httpClient;
        _matcher = matcher;
        _sourceUrl = configuration["Services:SanctionsScreening:OfacSettings:SourceUrl"] 
                     ?? "https://sanctionslistservice.ofac.treas.gov/api/publicationpreview/exports/sdn.csv";
        
        // Add User-Agent to avoid 403 Forbidden from some government servers
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
    }

    public async Task<ScreeningResult> ScreenEntityAsync(ScreeningRequest request)
    {
        await EnsureListLoadedAsync();

        var screeningId = Guid.NewGuid().ToString();
        var matches = new List<SanctionsMatch>();

        // normalize input
        var searchName = request.EntityName.ToLowerInvariant().Trim();
        var isPerson = request.EntityType?.Equals("Person", StringComparison.OrdinalIgnoreCase) == true;

        // Perform parallel screening
        // We only check name matching since we don't have structured DOB/Address in the CSV easily acccessible without complex parsing
        // The SDN CSV format is: Ent_Num,SDN_Name,SDN_Type,Program,Title,Call_Sign,Vess_type,Tonnage,GRT,Vess_flag,Vess_owner,Remarks
        
        // Simple matching strategy:
        // 1. Exact match (fast)
        // 2. Fuzzy match (slower)
        
        var potentialMatches = _cachedEntries
            // Filter by type if possible (individual vs entity) - SDN_Type isn't always clean, usually "individual" or "vessel" or "aircraft" or others
            // For safety, we check all.
            .AsParallel()
            .Select(entry => 
            {
                var score = _matcher.CalculateSimilarity(searchName, entry.Name.ToLowerInvariant());
                return new { Entry = entry, Score = score };
            })
            .Where(x => x.Score >= 0.70) // Threshold
            .OrderByDescending(x => x.Score)
            .Take(10) // Top 10
            .ToList();

        foreach (var m in potentialMatches)
        {
            matches.Add(new SanctionsMatch
            {
                ListName = "US Treasury OFAC SDN",
                ListSource = "treasury.gov",
                EntryId = m.Entry.Id,
                MatchedName = m.Entry.Name,
                MatchScore = (decimal)m.Score,
                MatchType = m.Score == 1.0 ? "Exact" : "Fuzzy",
                Reason = m.Entry.Program,
                SanctionsDetails = m.Entry.Remarks,
                ListUpdateDate = _lastUpdate,
                ScoreComponents = new MatchScoreComponents { NameScore = (decimal)m.Score }
            });
        }

        // Determine Risk
        var risk = RiskLevel.Clear;
        var status = "Clear";
        var requiresReview = false;
        string? action = null;

        if (matches.Any())
        {
            var maxScore = matches.Max(m => m.MatchScore);
            if (maxScore >= 0.95m)
            {
                risk = RiskLevel.Critical;
                status = "ConfirmedMatch";
                requiresReview = true;
                action = "BLOCK - High confidence match found in OFAC SDN List.";
            }
            else if (maxScore >= 0.80m)
            {
                risk = RiskLevel.High;
                status = "PotentialMatch";
                requiresReview = true;
                action = "REVIEW - Potential match found.";
            }
            else
            {
                risk = RiskLevel.Medium;
                status = "PotentialMatch";
                action = "MONITOR - Low confidence match.";
            }
        }

        return new ScreeningResult
        {
            ScreeningId = screeningId,
            ScreenedAt = DateTime.UtcNow,
            VendorId = request.VendorId,
            OverallRisk = risk,
            Status = status,
            Matches = matches,
            RequiresReview = requiresReview,
            RecommendedAction = action,
            TotalListsChecked = 1
        };
    }

    public async Task<List<ScreeningResult>> ScreenBatchAsync(List<ScreeningRequest> requests)
    {
        var results = new List<ScreeningResult>();
        foreach(var r in requests) results.Add(await ScreenEntityAsync(r));
        return results;
    }

    public Task<ScreeningResult> GetScreeningResultAsync(string screeningId)
    {
        throw new NotImplementedException("Persistence not implemented for raw source service.");
    }

    public async Task<ListsUpdateInfo> GetListsUpdateInfoAsync()
    {
        await EnsureListLoadedAsync();
        return new ListsUpdateInfo
        {
            LastUpdated = _lastUpdate,
            TotalEntries = _cachedEntries.Count,
            TotalLists = 1
        };
    }

    private async Task EnsureListLoadedAsync()
    {
        if (_cachedEntries.Any() && DateTime.UtcNow - _lastUpdate < _cacheDuration)
            return;

        await _lock.WaitAsync();
        try
        {
            if (_cachedEntries.Any() && DateTime.UtcNow - _lastUpdate < _cacheDuration)
                return;

            _logger.LogInformation("Downloading OFAC SDN List from {Url}", _sourceUrl);

            using var response = await _httpClient.GetAsync(_sourceUrl);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false, // SDN CSV often has odd headers or none, officially it's usually headless data lines
                MissingFieldFound = null,
                BadDataFound = null,
            });

            var newEntries = new List<OfacSdnEntry>();

            // OFAC SDN CSV format (legacy):
            // 0: Ent_Num
            // 1: SDN_Name
            // 2: SDN_Type
            // 3: Program
            // 4: Title
            // 5: Call_Sign
            // 6: Vess_type
            // 7: Tonnage
            // 8: GRT
            // 9: Vess_flag
            // 10: Vess_owner
            // 11: Remarks

            while (await csv.ReadAsync())
            {
                try 
                {
                    // Minimal parsing
                    var entNum = csv.GetField(0);
                    var name = csv.GetField(1);
                    var type = csv.GetField(2);
                    var program = csv.GetField(3);
                    var remarks = csv.GetField(11);

                    if (!string.IsNullOrWhiteSpace(name) && name != "-0- " && !name.StartsWith("SDN_Name")) // Skip junk/header
                    {
                        newEntries.Add(new OfacSdnEntry
                        {
                            Id = entNum ?? Guid.NewGuid().ToString(),
                            Name = name,
                            Type = type ?? "Entity",
                            Program = program ?? "",
                            Remarks = remarks ?? ""
                        });
                    }
                }
                catch
                {
                    // Ignore bad lines
                }
            }

            _cachedEntries = newEntries;
            _lastUpdate = DateTime.UtcNow;
            
            _logger.LogInformation("Loaded {Count} entries from OFAC SDN List", _cachedEntries.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download/parse OFAC SDN list");
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }

    private class OfacSdnEntry
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string Program { get; set; } = "";
        public string Remarks { get; set; } = "";
    }
}
