using VendorMdm.Shared.Constants;

namespace VendorMdm.Shared.Services;

/// <summary>
/// Service for mapping countries to their required document types
/// Supports multi-country vendor onboarding with region-specific compliance
/// </summary>
public class DocumentRequirementsService
{
    /// <summary>
    /// Get required document types for a given country
    /// </summary>
    /// <param name="countryCode">ISO 3166-1 alpha-2 country code (e.g., "AE", "DE", "US")</param>
    /// <returns>List of required document type codes</returns>
    public static List<string> GetRequiredDocumentsByCountry(string countryCode)
    {
        return countryCode.ToUpper() switch
        {
            // ============================================
            // EUROPE
            // ============================================
            
            // Germany
            "DE" => new List<string>
            {
                DocumentType.Handelsregister,
                DocumentType.VatCertificate,
                DocumentType.IbanProof
            },
            
            // United Kingdom
            "GB" => new List<string>
            {
                DocumentType.CompaniesHouseCert,
                DocumentType.VatCertificate,
                DocumentType.EoriNumber,
                DocumentType.IbanProof
            },
            
            // France
            "FR" => new List<string>
            {
                DocumentType.RegistreCommerce,
                DocumentType.VatCertificate,
                DocumentType.IbanProof
            },
            
            // Netherlands
            "NL" => new List<string>
            {
                DocumentType.KvkExtract,
                DocumentType.VatCertificate,
                DocumentType.IbanProof
            },
            
            // Spain
            "ES" => new List<string>
            {
                DocumentType.NifCif,
                DocumentType.VatCertificate,
                DocumentType.IbanProof
            },
            
            // Italy
            "IT" => new List<string>
            {
                DocumentType.RegistroImprese,
                DocumentType.VatCertificate,
                DocumentType.IbanProof
            },
            
            // Switzerland
            "CH" => new List<string>
            {
                DocumentType.Handelsregister,
                DocumentType.VatCertificate,
                DocumentType.IbanProof
            },
            
            // ============================================
            // UAE / GCC
            // ============================================
            
            // United Arab Emirates
            "AE" => new List<string>
            {
                DocumentType.TradeLicense,
                DocumentType.TaxRegistrationNumber,
                DocumentType.IbanProof
            },
            
            // Saudi Arabia
            "SA" => new List<string>
            {
                DocumentType.CommercialRegistration,
                DocumentType.ZatcaVat,
                DocumentType.IbanProof
            },
            
            // ============================================
            // AMERICAS
            // ============================================
            
            // United States
            "US" => new List<string>
            {
                DocumentType.EinLetter,
                DocumentType.W9Form,
                DocumentType.AchAuthorization
            },
            
            // Mexico
            "MX" => new List<string>
            {
                DocumentType.RfcMexico,
                DocumentType.IbanProof
            },
            
            // Brazil
            "BR" => new List<string>
            {
                DocumentType.CnpjBrazil,
                DocumentType.IbanProof
            },
            
            // ============================================
            // ASIA PACIFIC
            // ============================================
            
            // India
            "IN" => new List<string>
            {
                DocumentType.GstCertificate,
                DocumentType.PanCard,
                DocumentType.IbanProof
            },
            
            // Singapore
            "SG" => new List<string>
            {
                DocumentType.UenSingapore,
                DocumentType.IbanProof
            },
            
            // Hong Kong
            "HK" => new List<string>
            {
                DocumentType.BusinessRegHongKong,
                DocumentType.IbanProof
            },
            
            // Australia
            "AU" => new List<string>
            {
                DocumentType.AustralianBusinessNumber,
                DocumentType.GstAustralia,
                DocumentType.IbanProof
            },
            
            // ============================================
            // DEFAULT (International Vendor)
            // ============================================
            _ => new List<string>
            {
                DocumentType.Passport,
                DocumentType.IbanProof
            }
        };
    }
    
    /// <summary>
    /// Get document category for a document type
    /// </summary>
    public static string GetCategoryForDocumentType(string docType)
    {
        return docType switch
        {
            // Legal & Regulatory
            var dt when dt == DocumentType.TradeLicense ||
                       dt == DocumentType.CompaniesHouseCert ||
                       dt == DocumentType.Handelsregister ||
                       dt == DocumentType.RegistreCommerce ||
                       dt == DocumentType.KvkExtract ||
                       dt == DocumentType.RegistroImprese ||
                       dt == DocumentType.CommercialRegistration ||
                       dt == DocumentType.FreeZoneLicense ||
                       dt == DocumentType.CertificateOfIncorporation ||
                       dt == DocumentType.BusinessLicense
                => DocumentCategory.LegalRegulatory,
            
            // Tax & Compliance
            var dt when dt == DocumentType.VatCertificate ||
                       dt == DocumentType.TaxRegistrationNumber ||
                       dt == DocumentType.ZatcaVat ||
                       dt == DocumentType.EoriNumber ||
                       dt == DocumentType.W9Form ||
                       dt == DocumentType.GstCertificate ||
                       dt == DocumentType.PanCard ||
                       dt == DocumentType.TaxClearance ||
                       dt == DocumentType.NifCif
                => DocumentCategory.TaxCompliance,
            
            // Financial & Banking
            var dt when dt == DocumentType.IbanProof ||
                       dt == DocumentType.SepaMandate ||
                       dt == DocumentType.AchAuthorization ||
                       dt == DocumentType.VoidedCheck ||
                       dt == DocumentType.SwiftConfirmation
                => DocumentCategory.FinancialBanking,
            
            // Identity & Verification
            var dt when dt == DocumentType.Passport ||
                       dt == DocumentType.EmiratesId ||
                       dt == DocumentType.Aadhaar ||
                       dt == DocumentType.ResidenceVisa ||
                       dt == DocumentType.VoterId
                => DocumentCategory.IdentityVerification,
            
            // Certifications & Qualifications
            var dt when dt == DocumentType.IsoCertification ||
                       dt == DocumentType.Soc2Certification ||
                       dt == DocumentType.MsmeCertificate
                => DocumentCategory.CertificationsQualifications,
            
            // Insurance & Liability
            var dt when dt == DocumentType.LiabilityInsurance
                => DocumentCategory.InsuranceLiability,
            
            // Default
            _ => DocumentCategory.LegalRegulatory
        };
    }
    
    /// <summary>
    /// Get recommended security level for a document type
    /// </summary>
    public static int GetSecurityLevelForDocumentType(string docType)
    {
        return docType switch
        {
            // Restricted (PII/Highly Sensitive)
            var dt when dt == DocumentType.Passport ||
                       dt == DocumentType.EmiratesId ||
                       dt == DocumentType.Aadhaar ||
                       dt == DocumentType.VoterId ||
                       dt == DocumentType.IbanProof ||
                       dt == DocumentType.AchAuthorization ||
                       dt == DocumentType.VoidedCheck
                => SecurityLevel.Restricted,
            
            // Confidential (Sensitive Business)
            var dt when dt == DocumentType.TaxRegistrationNumber ||
                       dt == DocumentType.VatCertificate ||
                       dt == DocumentType.W9Form ||
                       dt == DocumentType.GstCertificate ||
                       dt == DocumentType.PanCard ||
                       dt == DocumentType.SwiftConfirmation
                => SecurityLevel.Confidential,
            
            // Internal (Standard Business Documents)
            var dt when dt == DocumentType.TradeLicense ||
                       dt == DocumentType.CompaniesHouseCert ||
                       dt == DocumentType.Handelsregister ||
                       dt == DocumentType.IsoCertification
                => SecurityLevel.Internal,
            
            // Default to Internal
            _ => SecurityLevel.Internal
        };
    }
    
    /// <summary>
    /// Check if document type requires expiry tracking
    /// </summary>
    public static bool RequiresExpiryTracking(string docType)
    {
        return docType switch
        {
            var dt when dt == DocumentType.TradeLicense ||
                       dt == DocumentType.VatCertificate ||
                       dt == DocumentType.TaxRegistrationNumber ||
                       dt == DocumentType.Passport ||
                       dt == DocumentType.EmiratesId ||
                       dt == DocumentType.ResidenceVisa ||
                       dt == DocumentType.IsoCertification ||
                       dt == DocumentType.LiabilityInsurance ||
                       dt == DocumentType.GstCertificate
                => true,
            
            _ => false
        };
    }
}
