namespace VendorMdm.Shared.Constants;

/// <summary>
/// Vendor type classifications.
/// Defines the types of vendors in the system.
/// </summary>
public static class VendorTypes
{
    // ============================================
    // Legal Structure Types
    // ============================================

    /// <summary>Individual person acting as vendor</summary>
    public const string Individual = "Individual";

    /// <summary>Registered company or organization</summary>
    public const string Organization = "Organization";

    /// <summary>Company registered as a corporation</summary>
    public const string Company = "Company";

    /// <summary>Startup or emerging business</summary>
    public const string StartUp = "StartUp";

    /// <summary>Non-profit organization</summary>
    public const string NonProfit = "NonProfit";

    /// <summary>Government entity</summary>
    public const string Government = "Government";

    // ============================================
    // Event/Participant Types
    // ============================================

    /// <summary>Physical attendee or participant</summary>
    public const string Participant = "Participant";

    /// <summary>Physical entity (legacy type)</summary>
    public const string Physical = "Physical";

    // ============================================
    // Validation
    // ============================================

    public static readonly string[] All =
    {
        Individual, Organization, Company, StartUp, NonProfit, Government,
        Participant, Physical
    };

    public static bool IsValid(string vendorType) => All.Contains(vendorType);

    /// <summary>
    /// Check if vendor type requires additional documentation
    /// </summary>
    public static bool RequiresCompanyDocuments(string vendorType) =>
        vendorType is Organization or Company or StartUp or NonProfit;

    /// <summary>
    /// Check if vendor type is subject to special tax rules
    /// </summary>
    public static bool IsNonProfitOrGov(string vendorType) =>
        vendorType is NonProfit or Government;
}
