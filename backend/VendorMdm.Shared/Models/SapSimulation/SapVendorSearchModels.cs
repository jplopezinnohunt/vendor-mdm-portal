namespace VendorMdm.Shared.Models.SapSimulation;

/// <summary>
/// Request for searching vendors (duplicate detection)
/// Pattern from UNESCO MoUV system
/// </summary>
public class VendorSearchRequest
{
    public string VendorType { get; set; } = string.Empty;  // "INDV", "COMP"
    public string? FamilyName { get; set; }
    public string? GivenName { get; set; }
    public string? CompanyName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string CompanyCode { get; set; } = "UNES";
    public double SearchThreshold { get; set; } = 0.75;  // Levenshtein similarity threshold
}

/// <summary>
/// Response from vendor search with fuzzy matching results
/// </summary>
public class VendorSearchResponse
{
    public bool DuplicatesFound { get; set; }
    public int MatchCount { get; set; }
    public string SearchAlgorithm { get; set; } = "Levenshtein";
    public double Threshold { get; set; }
    public List<VendorMatchResult> Vendors { get; set; } = new();
    public string ProcessingTime { get; set; } = string.Empty;
}

/// <summary>
/// Individual vendor match result with similarity score
/// </summary>
public class VendorMatchResult
{
    public string VendorName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string SapId { get; set; } = string.Empty;
    public string? ReqId { get; set; }  // MoUV request ID if pending
    public string Country { get; set; } = string.Empty;
    public string CompanyCode { get; set; } = string.Empty;
    public string AccountGroup { get; set; } = string.Empty;
    public string SapStatus { get; set; } = "Valid";  // Valid, Blocked, Deleted
    public bool Blocked { get; set; }
    public double MatchScore { get; set; }  // 0.0 to 1.0
}
