namespace VendorMdm.Shared.Constants;

/// <summary>
/// Global document category codes (universal across all countries)
/// Inspired by SAP Ariba document classification
/// </summary>
public static class DocumentCategory
{
    /// <summary>Legal & Regulatory Documents</summary>
    public const string LegalRegulatory = "DOC_LEG_REG";
    
    /// <summary>Financial & Banking Documents</summary>
    public const string FinancialBanking = "DOC_FIN_BANK";
    
    /// <summary>Tax & Compliance Documents</summary>
    public const string TaxCompliance = "DOC_TAX_COMP";
    
    /// <summary>Identity & Verification Documents</summary>
    public const string IdentityVerification = "DOC_ID_VERIFY";
    
    /// <summary>Certifications & Qualifications</summary>
    public const string CertificationsQualifications = "DOC_CERT_QUAL";
    
    /// <summary>Insurance & Liability Documents</summary>
    public const string InsuranceLiability = "DOC_INS_LIAB";
}

/// <summary>
/// Global document type codes (extensible by region)
/// Format: DOCTYPE_{SHORT_NAME}
/// </summary>
public static class DocumentType
{
    // ============================================
    // EUROPE (EU27, UK, Switzerland, Norway)
    // ============================================
    
    /// <summary>VAT Registration Certificate (EU/UK)</summary>
    public const string VatCertificate = "DOCTYPE_VAT_CERT";
    
    /// <summary>Companies House Certificate (UK)</summary>
    public const string CompaniesHouseCert = "DOCTYPE_COMP_REG";
    
    /// <summary>Handelsregister Excerpt (DE, AT, CH)</summary>
    public const string Handelsregister = "DOCTYPE_HANDELSREG";
    
    /// <summary>Registre du Commerce - RCS (FR, LU, BE)</summary>
    public const string RegistreCommerce = "DOCTYPE_RCS";
    
    /// <summary>KVK Extract (Netherlands)</summary>
    public const string KvkExtract = "DOCTYPE_KVK";
    
    /// <summary>Registro Imprese - REA (Italy)</summary>
    public const string RegistroImprese = "DOCTYPE_REA";
    
    /// <summary>NIF/CIF Certificate (Spain, Portugal)</summary>
    public const string NifCif = "DOCTYPE_NIF_CIF";
    
    /// <summary>EORI Number Certificate (EU + UK Customs)</summary>
    public const string EoriNumber = "DOCTYPE_EORI";
    
    /// <summary>IBAN Verification Letter (EU Banking)</summary>
    public const string IbanProof = "DOCTYPE_IBAN_PROOF";
    
    /// <summary>SEPA Direct Debit Mandate (Eurozone)</summary>
    public const string SepaMandate = "DOCTYPE_SEPA_MANDATE";
    
    /// <summary>Tax Clearance Certificate (EU)</summary>
    public const string TaxClearance = "DOCTYPE_TAX_CLEAR";
    
    /// <summary>Beneficial Ownership Declaration (AML/KYC)</summary>
    public const string BeneficialOwnership = "DOCTYPE_BENEFICIAL";
    
    /// <summary>GDPR Data Processing Agreement</summary>
    public const string GdprDpa = "DOCTYPE_GDPR_DPA";
    
    // ============================================
    // UAE / GCC (United Arab Emirates, Saudi Arabia, etc.)
    // ============================================
    
    /// <summary>Trade License (UAE, Bahrain, Oman)</summary>
    public const string TradeLicense = "DOCTYPE_TRD_LIC";
    
    /// <summary>Commercial Registration (Saudi Arabia)</summary>
    public const string CommercialRegistration = "DOCTYPE_COMM_REG";
    
    /// <summary>Free Zone License (UAE Free Zones)</summary>
    public const string FreeZoneLicense = "DOCTYPE_FREEZONE_LIC";
    
    /// <summary>Memorandum of Association (UAE/GCC)</summary>
    public const string MemorandumOfAssociation = "DOCTYPE_MOA";
    
    /// <summary>Tax Registration Number - TRN (UAE VAT)</summary>
    public const string TaxRegistrationNumber = "DOCTYPE_TRN";
    
    /// <summary>ZATCA VAT Certificate (Saudi Arabia)</summary>
    public const string ZatcaVat = "DOCTYPE_ZATCA";
    
    /// <summary>Emirates ID (UAE National ID)</summary>
    public const string EmiratesId = "DOCTYPE_EMIRATES_ID";
    
    /// <summary>GCC Residence Visa</summary>
    public const string ResidenceVisa = "DOCTYPE_RESIDENCE";
    
    // ============================================
    // USA
    // ============================================
    
    /// <summary>EIN Assignment Letter (IRS)</summary>
    public const string EinLetter = "DOCTYPE_EIN_LETTER";
    
    /// <summary>Certificate of Incorporation (State)</summary>
    public const string CertificateOfIncorporation = "DOCTYPE_CERT_INCORP";
    
    /// <summary>Business License (City/County)</summary>
    public const string BusinessLicense = "DOCTYPE_BUS_LIC";
    
    /// <summary>Certificate of Authority (Foreign Corp Qualification)</summary>
    public const string CertificateOfAuthority = "DOCTYPE_FOREIGN_QUAL";
    
    /// <summary>W-9 Form (Tax Certification)</summary>
    public const string W9Form = "DOCTYPE_W9";
    
    /// <summary>Sales Tax Permit</summary>
    public const string SalesTaxPermit = "DOCTYPE_SALES_TAX";
    
    /// <summary>1099-MISC Form</summary>
    public const string Form1099 = "DOCTYPE_1099_MISC";
    
    /// <summary>SOC 2 Certification (Security Compliance)</summary>
    public const string Soc2Certification = "DOCTYPE_SOC2";
    
    /// <summary>ACH Authorization Form</summary>
    public const string AchAuthorization = "DOCTYPE_ACH_AUTH";
    
    /// <summary>Voided Check (Bank Verification)</summary>
    public const string VoidedCheck = "DOCTYPE_VOID_CHECK";
    
    // ============================================
    // INDIA
    // ============================================
    
    /// <summary>GST Registration Certificate</summary>
    public const string GstCertificate = "DOCTYPE_GST_CERT";
    
    /// <summary>PAN Card (Permanent Account Number)</summary>
    public const string PanCard = "DOCTYPE_PAN_CARD";
    
    /// <summary>TAN Certificate (Tax Deduction Account Number)</summary>
    public const string TanCertificate = "DOCTYPE_TAN_CERT";
    
    /// <summary>MSME/Udyog Aadhaar Certificate</summary>
    public const string MsmeCertificate = "DOCTYPE_MSME_CERT";
    
    /// <summary>Import Export Code (IEC)</summary>
    public const string IecCode = "DOCTYPE_IEC";
    
    /// <summary>Corporate Identity Number (CIN)</summary>
    public const string CorporateIdentityNumber = "DOCTYPE_CIN";
    
    /// <summary>TDS Certificate (Form 16A)</summary>
    public const string TdsCertificate = "DOCTYPE_TDS_CERT";
    
    /// <summary>Form 15CA/15CB (Foreign Remittance)</summary>
    public const string Form15CA = "DOCTYPE_FORM_15CA_CB";
    
    /// <summary>Aadhaar Card (National Biometric ID)</summary>
    public const string Aadhaar = "DOCTYPE_AADHAAR";
    
    /// <summary>Voter ID</summary>
    public const string VoterId = "DOCTYPE_VOTER_ID";
    
    // ============================================
    // APAC (Singapore, Hong Kong, Australia)
    // ============================================
    
    /// <summary>Unique Entity Number (Singapore)</summary>
    public const string UenSingapore = "DOCTYPE_UEN";
    
    /// <summary>Business Registration Certificate (Hong Kong)</summary>
    public const string BusinessRegHongKong = "DOCTYPE_BR_CERT";
    
    /// <summary>Australian Business Number (ABN)</summary>
    public const string AustralianBusinessNumber = "DOCTYPE_ABN";
    
    /// <summary>GST Registration (Australia)</summary>
    public const string GstAustralia = "DOCTYPE_GST_AU";
    
    // ============================================
    // LATAM (Mexico, Brazil, Chile)
    // ============================================
    
    /// <summary>RFC Certificate (Mexico)</summary>
    public const string RfcMexico = "DOCTYPE_RFC";
    
    /// <summary>CNPJ Certificate (Brazil)</summary>
    public const string CnpjBrazil = "DOCTYPE_CNPJ";
    
    /// <summary>RUT Certificate (Chile)</summary>
    public const string RutChile = "DOCTYPE_RUT";
    
    // ============================================
    // GLOBAL (All Countries)
    // ============================================
    
    /// <summary>Passport (International ID)</summary>
    public const string Passport = "DOCTYPE_PASSPORT";
    
    /// <summary>SWIFT Confirmation Letter</summary>
    public const string SwiftConfirmation = "DOCTYPE_SWIFT_CONF";
    
    /// <summary>ISO Certification (9001, 27001, etc.)</summary>
    public const string IsoCertification = "DOCTYPE_ISO_CERT";
    
    /// <summary>Professional Liability Insurance</summary>
    public const string LiabilityInsurance = "DOCTYPE_LIAB_INS";
    
    /// <summary>Power of Attorney</summary>
    public const string PowerOfAttorney = "DOCTYPE_POA";
}

/// <summary>
/// Security level definitions for document classification
/// </summary>
public static class SecurityLevel
{
    /// <summary>Public - Non-sensitive information</summary>
    public const int Public = 1;
    
    /// <summary>Internal - Standard business documents</summary>
    public const int Internal = 2;
    
    /// <summary>Confidential - Sensitive business information</summary>
    public const int Confidential = 3;
    
    /// <summary>Restricted - PII/Highly sensitive (passports, IDs, bank details)</summary>
    public const int Restricted = 4;
}

/// <summary>
/// Document status workflow states
/// </summary>
public static class DocumentStatus
{
    /// <summary>Uploaded but not yet verified</summary>
    public const string Pending = "Pending";
    
    /// <summary>Verified by compliance/approver</summary>
    public const string Verified = "Verified";
    
    /// <summary>Rejected due to invalid/expired/poor quality</summary>
    public const string Rejected = "Rejected";
    
    /// <summary>Archived after retention period or vendor termination</summary>
    public const string Archived = "Archived";
    
    /// <summary>Expired (based on ExpiryDate)</summary>
    public const string Expired = "Expired";
}
