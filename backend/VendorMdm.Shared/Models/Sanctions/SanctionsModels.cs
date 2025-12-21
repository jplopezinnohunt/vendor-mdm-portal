namespace VendorMdm.Shared.Models.Sanctions;

/// <summary>
/// Request to screen an entity against sanctions lists
/// </summary>
public class ScreeningRequest
{
    /// <summary>
    /// Type of entity being screened
    /// </summary>
    public string EntityType { get; set; } = null!; // "Individual", "Company", "UBO"

    /// <summary>
    /// Vendor ID for tracking
    /// </summary>
    public string VendorId { get; set; } = null!;

    /// <summary>
    /// Primary name to screen
    /// </summary>
    public string EntityName { get; set; } = null!;

    /// <summary>
    /// Alternative names or aliases
    /// </summary>
    public string? AlternativeName { get; set; }

    /// <summary>
    /// Date of birth (for individuals)
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Place of birth
    /// </summary>
    public string? PlaceOfBirth { get; set; }

    /// <summary>
    /// Nationality/nationalities
    /// </summary>
    public List<string>? Nationalities { get; set; }

    /// <summary>
    /// Tax ID or registration number
    /// </summary>
    public string? TaxId { get; set; }

    /// <summary>
    /// Country of incorporation (for companies)
    /// </summary>
    public string? CountryOfIncorporation { get; set; }

    /// <summary>
    /// Address information
    /// </summary>
    public AddressInfo? Address { get; set; }

    /// <summary>
    /// Additional aliases
    /// </summary>
    public List<string>? Aliases { get; set; }
}

/// <summary>
/// Address information for screening
/// </summary>
public class AddressInfo
{
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = null!;
}

/// <summary>
/// Result of sanctions screening
/// </summary>
public class ScreeningResult
{
    public string ScreeningId { get; set; } = null!;
    public DateTime ScreenedAt { get; set; }
    public string VendorId { get; set; } = null!;
    public string Status { get; set; } = null!; // "Clear", "PotentialMatch", "ConfirmedMatch"
    public RiskLevel OverallRisk { get; set; }
    public List<SanctionsMatch> Matches { get; set; } = new();
    public bool RequiresReview { get; set; }
    public string? RecommendedAction { get; set; }
    public int TotalListsChecked { get; set; }
}

/// <summary>
/// Individual match found in sanctions lists
/// </summary>
public class SanctionsMatch
{
    public string ListName { get; set; } = null!; // "OFAC SDN", "UN Sanctions", etc.
    public string ListSource { get; set; } = null!; // Source URL
    public string EntryId { get; set; } = null!;
    public string MatchedName { get; set; } = null!;
    public decimal MatchScore { get; set; } // 0.00 - 1.00
    public string MatchType { get; set; } = null!; // "Name", "Alias", "AssociatedEntity"
    public string? Reason { get; set; } // Why sanctioned
    public string? SanctionsDetails { get; set; } // Programs, dates
    public DateTime? ListUpdateDate { get; set; }
    
    /// <summary>
    /// Breakdown of scoring components
    /// </summary>
    public MatchScoreComponents ScoreComponents { get; set; } = new();
}

/// <summary>
/// Detailed score breakdown
/// </summary>
public class MatchScoreComponents
{
    public decimal NameScore { get; set; }
    public decimal? DobScore { get; set; }
    public decimal? AddressScore { get; set; }
    public decimal? NationalityScore { get; set; }
    public decimal? IdScore { get; set; }
}

/// <summary>
/// Risk levels for screening results
/// </summary>
public enum RiskLevel
{
    Clear = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

/// <summary>
/// Information about list updates
/// </summary>
public class ListsUpdateInfo
{
    public DateTime LastUpdated { get; set; }
    public int TotalLists { get; set; }
    public int TotalEntries { get; set; }
    public Dictionary<string, DateTime> ListUpdateDates { get; set; } = new();
}
