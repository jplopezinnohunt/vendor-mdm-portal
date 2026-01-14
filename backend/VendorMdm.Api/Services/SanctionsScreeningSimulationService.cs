using VendorMdm.Api.Services.Helpers;
using VendorMdm.Shared.Models.Sanctions;

namespace VendorMdm.Api.Services;

/// <summary>
/// MOCK implementation of sanctions screening service
/// Uses hardcoded test cases for development and testing
/// </summary>
public class SanctionsScreeningSimulationService : ISanctionsScreeningService
{
    private readonly ILogger<SanctionsScreeningSimulationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly LevenshteinMatcher _fuzzyMatcher;
    private readonly Dictionary<string, ScreeningResult> _screeningHistory;
    private readonly List<MockSanctionsEntry> _mockSanctionsList;

    public SanctionsScreeningSimulationService(
        ILogger<SanctionsScreeningSimulationService> logger,
        IConfiguration configuration,
        LevenshteinMatcher fuzzyMatcher)
    {
        _logger = logger;
        _configuration = configuration;
        _fuzzyMatcher = fuzzyMatcher;
        _screeningHistory = new Dictionary<string, ScreeningResult>();
        
        // Initialize mock sanctions list with test data
        _mockSanctionsList = InitializeMockSanctionsList();
        
        _logger.LogInformation("MOCK SANCTIONS SCREENING: Initialized with {Count} test entries", _mockSanctionsList.Count);
    }

    public async Task<ScreeningResult> ScreenEntityAsync(ScreeningRequest request)
    {
        _logger.LogInformation(
            "MOCK: Screening entity {EntityName} ({EntityType}) for vendor {VendorId}",
            request.EntityName, request.EntityType, request.VendorId);

        // Simulate API latency
        var simulateLatency = _configuration.GetValue<bool>("Services:SanctionsScreening:MockSettings:SimulateLatency", false);
        if (simulateLatency)
        {
            var latencyMs = _configuration.GetValue<int>("Services:SanctionsScreening:MockSettings:LatencyMs", 2000);
            await Task.Delay(latencyMs);
        }

        var screeningId = Guid.NewGuid().ToString();
        var matches = new List<SanctionsMatch>();

        // Check against mock sanctions list using weighted scoring
        foreach (var entry in _mockSanctionsList)
        {
            var nameScore = CalculateNameScore(request.EntityName, entry.Name);
            
            // Only consider if name similarity is above threshold
            if (nameScore < 0.50m)
                continue;

            var dobScore = CalculateDobScore(request.DateOfBirth, entry.DateOfBirth);
            var nationalityScore = CalculateNationalityScore(request.Nationalities, entry.Nationalities);
            
            // Weighted total score (name: 60%, DOB: 25%, nationality: 15%)
            var totalScore = (nameScore * 0.60m) +
                           (dobScore * 0.25m) +
                           (nationalityScore * 0.15m);

            // Only report matches above threshold
            var threshold = _configuration.GetValue<decimal>("Services:SanctionsScreening:MatchThreshold", 0.75m);
            if (totalScore >= threshold)
            {
                matches.Add(new SanctionsMatch
                {
                    ListName = entry.ListName,
                    ListSource = entry.ListSource,
                    EntryId = entry.EntryId,
                    MatchedName = entry.Name,
                    MatchScore = totalScore,
                    MatchType = "Name",
                    Reason = entry.Reason,
                    SanctionsDetails = entry.Details,
                    ListUpdateDate = entry.ListUpdateDate,
                    ScoreComponents = new MatchScoreComponents
                    {
                        NameScore = nameScore,
                        DobScore = dobScore,
                        NationalityScore = nationalityScore
                    }
                });
            }
        }

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
                recommendedAction = "REJECT - High confidence sanctions match. Block immediately and escalate to compliance officer.";
            }
            else if (highestScore >= 0.75m)
            {
                status = "PotentialMatch";
                riskLevel = RiskLevel.High;
                requiresReview = true;
                recommendedAction = "REVIEW REQUIRED - Potential match found. Manual review by compliance team required before proceeding.";
            }
            else
            {
                status = "PotentialMatch";
                riskLevel = RiskLevel.Medium;
                requiresReview = false;
                recommendedAction = "MONITOR - Low confidence match. Can proceed with enhanced monitoring.";
            }
        }

        var result = new ScreeningResult
        {
            ScreeningId = screeningId,
            ScreenedAt = DateTime.UtcNow,
            VendorId = request.VendorId,
            Status = status,
            OverallRisk = riskLevel,
            Matches = matches.OrderByDescending(m => m.MatchScore).ToList(),
            RequiresReview = requiresReview,
            RecommendedAction = recommendedAction,
            TotalListsChecked = 5 // Mock: simulating 5 lists
        };

        // Store in history
        _screeningHistory[screeningId] = result;

        _logger.LogInformation(
            "MOCK: Screening complete - Status: {Status}, Matches: {MatchCount}, Risk: {Risk}",
            result.Status, result.Matches.Count, result.OverallRisk);

        return result;
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

    public Task<ListsUpdateInfo> GetListsUpdateInfoAsync()
    {
        return Task.FromResult(new ListsUpdateInfo
        {
            LastUpdated = DateTime.UtcNow.AddHours(-2), // Mock: updated 2 hours ago
            TotalLists = 5,
            TotalEntries = _mockSanctionsList.Count,
            ListUpdateDates = new Dictionary<string, DateTime>
            {
                ["OFAC SDN"] = DateTime.UtcNow.AddDays(-1),
                ["UN Sanctions"] = DateTime.UtcNow.AddDays(-3),
                ["EU Sanctions"] = DateTime.UtcNow.AddHours(-12),
                ["UK HM Treasury"] = DateTime.UtcNow.AddDays(-2),
                ["PEP Database"] = DateTime.UtcNow.AddDays(-7)
            }
        });
    }

    public Task<bool> TestConnectionAsync()
    {
        _logger.LogInformation("MOCK SANCTIONS SCREENING: Testing connection (always success)");
        return Task.FromResult(true);
    }

    // Helper methods

    private decimal CalculateNameScore(string name1, string name2)
    {
        if (string.IsNullOrWhiteSpace(name1) || string.IsNullOrWhiteSpace(name2))
            return 0.0m;

        // Exact match
        if (name1.Equals(name2, StringComparison.OrdinalIgnoreCase))
            return 1.0m;

        // Fuzzy match using Levenshtein
        return (decimal)_fuzzyMatcher.CalculateSimilarity(name1, name2);
    }

    private decimal CalculateDobScore(DateTime? dob1, DateTime? dob2)
    {
        if (dob1 == null || dob2 == null)
            return 0.5m; // Neutral score when DOB is missing

        if (dob1 == dob2)
            return 1.0m;

        var daysDiff = Math.Abs((dob1.Value - dob2.Value).TotalDays);
        
        if (daysDiff <= 1) return 0.95m; // ±1 day tolerance
        if (daysDiff <= 7) return 0.85m; // Same week
        if (daysDiff <= 365) return 0.50m; // Same year
        
        return 0.0m;
    }

    private decimal CalculateNationalityScore(List<string>? nationalities1, List<string>? nationalities2)
    {
        if (nationalities1 == null || nationalities2 == null || !nationalities1.Any() || !nationalities2.Any())
            return 0.5m; // Neutral score when nationality is missing

        var commonCount = nationalities1.Intersect(nationalities2, StringComparer.OrdinalIgnoreCase).Count();
        var totalUnique = nationalities1.Union(nationalities2, StringComparer.OrdinalIgnoreCase).Count();

        return (decimal)commonCount / totalUnique;
    }

    private List<MockSanctionsEntry> InitializeMockSanctionsList()
    {
        // Hardcoded test cases for testing
        var forcedMatches = _configuration.GetSection("Services:SanctionsScreening:MockSettings:ForcedMatches")
            .Get<string[]>() ?? Array.Empty<string>();

        var list = new List<MockSanctionsEntry>
        {
            // Test case: Obvious match (should always trigger)
            new MockSanctionsEntry
            {
                EntryId = "OFAC-SDN-001",
                ListName = "OFAC SDN",
                ListSource = "ofac.treas.gov",
                Name = "VLADIMIR PUTIN",
                DateOfBirth = new DateTime(1952, 10, 7),
                Nationalities = new List<string> { "RU" },
                Reason = "Ukraine-related sanctions",
                Details = "President of Russian Federation. Sanctioned 2022-02-24.",
                ListUpdateDate = DateTime.UtcNow.AddDays(-1)
            },
            // Test case: Common name (false positive test)
            new MockSanctionsEntry
            {
                EntryId = "UN-001",
                ListName = "UN Sanctions",
                ListSource = "un.org/sanctions",
                Name = "JOHN SMITH",
                DateOfBirth = new DateTime(1965, 5, 15),
                Nationalities = new List<string> { "SY" },
                Reason = "Syria sanctions",
                Details = "Designated terrorist, Syria conflict.",
                ListUpdateDate = DateTime.UtcNow.AddDays(-3)
            },
            // Test case: PEP
            new MockSanctionsEntry
            {
                EntryId = "PEP-001",
                ListName = "PEP Database",
                ListSource = "worldcheck.refinitiv.com",
                Name = "JOE BIDEN",
                DateOfBirth = new DateTime(1942, 11, 20),
                Nationalities = new List<string> { "US" },
                Reason = "Politically Exposed Person",
                Details = "President of the United States",
                ListUpdateDate = DateTime.UtcNow.AddDays(-7)
            }
        };

        // Add any additional forced matches from configuration
        foreach (var forcedName in forcedMatches)
        {
            if (!list.Any(e => e.Name.Equals(forcedName, StringComparison.OrdinalIgnoreCase)))
            {
                list.Add(new MockSanctionsEntry
                {
                    EntryId = $"MOCK-{Guid.NewGuid()}",
                    ListName = "MOCK Test List",
                    ListSource = "mock.local",
                    Name = forcedName,
                    DateOfBirth = null,
                    Nationalities = new List<string>(),
                    Reason = "Test entry from configuration",
                    Details = "Configured forced match for testing",
                    ListUpdateDate = DateTime.UtcNow
                });
            }
        }

        return list;
    }

    private class MockSanctionsEntry
    {
        public string EntryId { get; set; } = null!;
        public string ListName { get; set; } = null!;
        public string ListSource { get; set; } = null!;
        public string Name { get; set; } = null!;
        public DateTime? DateOfBirth { get; set; }
        public List<string> Nationalities { get; set; } = new();
        public string Reason { get; set; } = null!;
        public string Details { get; set; } = null!;
        public DateTime ListUpdateDate { get; set; }
    }
}
