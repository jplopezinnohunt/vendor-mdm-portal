# Global Document Taxonomy - Multi-Country Vendor Management

## Purpose

Universal document classification system for **vendor master data management** across UAE, US, Europe, India, and other global markets. Inspired by SAP Ariba's document categorization.

---

## Folder Structure Pattern

```
{environment}/{tenantId}/{entityType}/{entityRef}/{categoryCode}/{docTypeCode}/{timestamp}_{guid}_{locale}_{sanitized-filename}.{ext}
```

**Example**:
```
prod/emea-hq/vendors/v-123/DOC_LEG_REG/DOCTYPE_VAT_CERT/20260103_a1b2c3d4_de-DE_vat-certificate.pdf
prod/gcc-dubai/vendors/v-456/DOC_LEG_REG/DOCTYPE_TRD_LIC/20260103_e5f6g7h8_ar-AE_trade-license.pdf
prod/amer-us/vendors/v-789/DOC_FIN_BANK/DOCTYPE_ACH_FORM/20260103_i9j0k1l2_en-US_ach-authorization.pdf
```

---

## Universal Document Categories

### DOC_LEG_REG - Legal & Regulatory
**Purpose**: Business registration, licenses, permits  
**Retention**: Permanent (as long as vendor is active)  
**Security Level**: 2 (Internal)

### DOC_FIN_BANK - Financial & Banking
**Purpose**: Bank details, payment instructions  
**Retention**: 7 years post-termination  
**Security Level**: 3 (Confidential)

### DOC_TAX_COMP - Tax & Compliance
**Purpose**: Tax registrations, compliance certificates  
**Retention**: 10 years (regulatory standard)  
**Security Level**: 3 (Confidential)

### DOC_ID_VERIFY - Identity & Verification
**Purpose**: Personal identification (for individual vendors/signatories)  
**Retention**: 5 years post-termination  
**Security Level**: 4 (Restricted - PII)

### DOC_CERT_QUAL - Certifications & Qualifications
**Purpose**: ISO certs, industry qualifications  
**Retention**: Valid period + 2 years  
**Security Level**: 2 (Internal)

### DOC_INS_LIAB - Insurance & Liability
**Purpose**: Liability insurance, professional indemnity  
**Retention**: Valid period + 7 years  
**Security Level**: 2 (Internal)

---

## Document Types by Region

### 🌍 Europe (EU27, UK, CH, Norway)

#### Legal & Registration

| Code | Document | Countries | Notes |
|------|----------|-----------|-------|
| **DOCTYPE_VAT_CERT** | VAT Registration Certificate | EU27, UK, CH, Norway | Primary tax ID for B2B |
| **DOCTYPE_COMP_REG** | Companies House Certificate | UK | Certificate of Incorporation |
| **DOCTYPE_HANDELSREG** | Handelsregister Excerpt | DE, AT, CH | Commercial Register Extract |
| **DOCTYPE_RCS** | Registre du Commerce (RCS) | FR, LU, BE | Trade Register |
| **DOCTYPE_KVK** | KVK Extract | NL | Dutch Chamber of Commerce |
| **DOCTYPE_REA** | Registro Imprese (REA) | IT | Italian Business Registry |
| **DOCTYPE_NIF_CIF** | NIF/CIF Certificate | ES, PT | Tax ID Certificate |
| **DOCTYPE_EORI** | EORI Number Certificate | EU27 + UK | Customs/Import-Export ID |

#### Tax & Compliance

| Code | Document | Countries | Notes |
|------|----------|-----------|-------|
| **DOCTYPE_INTRASTAT** | Intrastat Declaration | EU27 | For intra-EU traders |
| **DOCTYPE_TAX_CLEAR** | Tax Clearance Certificate | ALL | Proof of tax compliance |
| **DOCTYPE_GDPR_DPA** | Data Processing Agreement | EU27, UK | GDPR compliance |
| **DOCTYPE_BENEFICIAL** | Beneficial Ownership Declaration | EU27, UK | AML/KYC requirement |

#### Banking

| Code | Document | Countries | Notes |
|------|----------|-----------|-------|
| **DOCTYPE_IBAN_PROOF** | IBAN Verification Letter | EU27, UK, CH | Bank letter with IBAN |
| **DOCTYPE_SEPA_MANDATE** | SEPA Direct Debit Mandate | EU Eurozone | For recurring payments |
| **DOCTYPE_SWIFT_CONF** | SWIFT Confirmation | ALL | International wire setup |

---

### 🇦🇪 UAE / GCC

#### Legal & Registration

| Code | Document | Countries | Notes |
|------|----------|-----------|-------|
| **DOCTYPE_TRD_LIC** | Trade License | UAE, Bahrain, Oman | Primary business license |
| **DOCTYPE_COMM_REG** | Commercial Registration | KSA | Ministry of Commerce cert |
| **DOCTYPE_FREEZONE_LIC** | Free Zone License | UAE, KSA | Dubai Airport FZ, JAFZA, etc. |
| **DOCTYPE_MOA** | Memorandum of Association | UAE, GCC | Company charter |

#### Tax & Compliance

| Code | Document | Countries | Notes |
|------|----------|-----------|-------|
| **DOCTYPE_TRN** | Tax Registration Number (TRN) | UAE | VAT registration (5%) |
| **DOCTYPE_ZATCA** | ZATCA VAT Certificate | KSA | Zakat, Tax & Customs (15%) |
| **DOCTYPE_VAT_GCC** | GCC VAT Certificate | Bahrain, Oman | VAT registration |

#### Identity (for individual vendors)

| Code | Document | Countries | Notes |
|------|----------|-----------|-------|
| **DOCTYPE_EMIRATES_ID** | Emirates ID | UAE | National ID |
| **DOCTYPE_PASSPORT** | Passport | ALL | International ID |
| **DOCTYPE_RESIDENCE** | Residence Visa | UAE, GCC | Expat work permit |

---

### 🇺🇸 USA

#### Legal & Registration

| Code | Document | Countries | Notes |
|------|----------|-----------|-------|
| **DOCTYPE_EIN_LETTER** | EIN Assignment Letter | US | IRS tax ID letter |
| **DOCTYPE_CERT_INCORP** | Certificate of Incorporation | US | State-issued cert |
| **DOCTYPE_BUS_LIC** | Business License | US | City/County license |
| **DOCTYPE_FOREIGN_QUAL** | Certificate of Authority | US | Foreign corp qualification |
| **DOCTYPE_W9** | W-9 Form | US | Tax certification |

#### Tax & Compliance

| Code | Document | Countries | Notes |
|------|----------|-----------|-------|
| **DOCTYPE_SALES_TAX** | Sales Tax Permit | US (varies by state) | Resale certificate |
| **DOCTYPE_1099_MISC** | 1099-MISC | US | Independent contractor tax |
| **DOCTYPE_SOC2** | SOC 2 Certification | US | Security compliance |

#### Banking

| Code | Document | Countries | Notes |
|------|----------|-----------|-------|
| **DOCTYPE_ACH_AUTH** | ACH Authorization Form | US | Electronic payment setup |
| **DOCTYPE_VOID_CHECK** | Voided Check | US | Bank account verification |

---

### 🇮🇳 India

#### Legal & Registration

| Code | Document | Countries | Notes |
|------|----------|-----------|-------|
| **DOCTYPE_GST_CERT** | GST Registration Certificate | India | Goods & Services Tax |
| **DOCTYPE_PAN_CARD** | PAN Card | India | Permanent Account Number |
| **DOCTYPE_TAN_CERT** | TAN Certificate | India | Tax Deduction Account Number |
| **DOCTYPE_MSME_CERT** | MSME/Udyog Aadhaar | India | SME certification |
| **DOCTYPE_IEC** | Import Export Code | India | Customs clearance |
| **DOCTYPE_CIN** | Corporate Identity Number | India | MCA registration |

#### Tax & Compliance

| Code | Document | Countries | Notes |
|------|----------|-----------|-------|
| **DOCTYPE_TDS_CERT** | TDS Certificate (Form 16A) | India | Tax deducted at source |
| **DOCTYPE_FORM_15CA_CB** | Form 15CA/15CB | India | Foreign remittance compliance |

#### Identity

| Code | Document | Countries | Notes |
|------|----------|-----------|-------|
| **DOCTYPE_AADHAAR** | Aadhaar Card | India | National biometric ID |
| **DOCTYPE_VOTER_ID** | Voter ID | India | Alternative ID |

---

### 🌏 Other Global Markets

#### APAC (Singapore, Hong Kong, Australia)

| Code | Document | Countries | Notes |
|------|----------|-----------|-------|
| **DOCTYPE_UEN** | Unique Entity Number | Singapore | ACRA registration |
| **DOCTYPE_BR_CERT** | Business Registration Certificate | Hong Kong | Companies Registry |
| **DOCTYPE_ABN** | Australian Business Number | Australia | Tax office registration |
| **DOCTYPE_GST_AU** | GST Registration | Australia | 10% GST |

#### Latin America (Mexico, Brazil, Chile)

| Code | Document | Countries | Notes |
|------|----------|-----------|-------|
| **DOCTYPE_RFC** | RFC Certificate | Mexico | Federal Tax Registry |
| **DOCTYPE_CNPJ** | CNPJ Certificate | Brazil | Legal entity tax ID |
| **DOCTYPE_RUT** | RUT Certificate | Chile | Single Tax Roll |

---

## Implementation in Code

### Database Enum (C#)

```csharp
public static class DocumentCategory
{
    public const string LegalRegulatory = "DOC_LEG_REG";
    public const string FinancialBanking = "DOC_FIN_BANK";
    public const string TaxCompliance = "DOC_TAX_COMP";
    public const string IdentityVerification = "DOC_ID_VERIFY";
    public const string CertificationsQualifications = "DOC_CERT_QUAL";
    public const string InsuranceLiability = "DOC_INS_LIAB";
}

public static class DocumentType
{
    // Europe
    public const string VatCertificate = "DOCTYPE_VAT_CERT";
    public const string CompaniesHouseCert = "DOCTYPE_COMP_REG";
    public const string Handelsregister = "DOCTYPE_HANDELSREG";
    public const string EoriNumber = "DOCTYPE_EORI";
    public const string IbanProof = "DOCTYPE_IBAN_PROOF";
    
    // UAE/GCC
    public const string TradeLicense = "DOCTYPE_TRD_LIC";
    public const string TaxRegistrationNumber = "DOCTYPE_TRN";
    public const string EmiratesId = "DOCTYPE_EMIRATES_ID";
    
    // USA
    public const string EinLetter = "DOCTYPE_EIN_LETTER";
    public const string W9Form = "DOCTYPE_W9";
    public const string AchAuthorization = "DOCTYPE_ACH_AUTH";
    
    // India
    public const string GstCertificate = "DOCTYPE_GST_CERT";
    public const string PanCard = "DOCTYPE_PAN_CARD";
    public const string Aadhaar = "DOCTYPE_AADHAAR";
    
    // Global
    public const string Passport = "DOCTYPE_PASSPORT";
}
```

### Country-to-DocType Mapping

```csharp
public static Dictionary<string, List<string>> GetRequiredDocumentsByCountry(string countryCode)
{
    return countryCode.ToUpper() switch
    {
        "AE" => new() { // UAE
            DocumentType.TradeLicense,
            DocumentType.TaxRegistrationNumber,
            DocumentType.IbanProof
        },
        "DE" => new() { // Germany
            DocumentType.Handelsregister,
            DocumentType.VatCertificate,
            DocumentType.IbanProof
        },
        "GB" => new() { // UK
            DocumentType.CompaniesHouseCert,
            DocumentType.VatCertificate,
            DocumentType.EoriNumber,
            DocumentType.IbanProof
        },
        "US" => new() { // USA
            DocumentType.EinLetter,
            DocumentType.W9Form,
            DocumentType.AchAuthorization
        },
        "IN" => new() { // India
            DocumentType.GstCertificate,
            DocumentType.PanCard,
            DocumentType.IbanProof
        },
        _ => new() { DocumentType.Passport } // Fallback
    };
}
```

---

## Localization Support

### Document Name Translations

```json
{
  "DOCTYPE_VAT_CERT": {
    "en-GB": "VAT Registration Certificate",
    "de-DE": "Umsatzsteuer-Identifikationsnummer (USt-IdNr.)",
    "fr-FR": "Numéro de TVA intracommunautaire",
    "es-ES": "Certificado de IVA",
    "ar-AE": "شهادة التسجيل الضريبي"
  },
  "DOCTYPE_TRD_LIC": {
    "en-AE": "Trade License",
    "ar-AE": "الرخصة التجارية"
  },
  "DOCTYPE_GST_CERT": {
    "en-IN": "GST Registration Certificate",
    "hi-IN": "जीएसटी पंजीकरण प्रमाणपत्र"
  }
}
```

---

## Validation Rules by Market

| Market | Max File Size | Allowed Formats | Expiry Tracking | OCR Priority |
|--------|--------------|-----------------|-----------------|--------------|
| **EU** | 10MB | PDF, JPG, PNG | VAT cert: 10 years | High (multi-lang) |
| **UAE** | 10MB | PDF, JPG, PNG | Trade Lic: Annual | Medium (Arabic+English) |
| **US** | 10MB | PDF, JPG, PNG, DOCX | EIN: Permanent | Low (English only) |
| **India** | 10MB | PDF, JPG, PNG | GST: Annual | High (Hindi+English) |

---

## References

- SAP Ariba Document Management
- EU PEPPOL Document Standards
- eForms (EU public procurement)
- UAE Federal Tax Authority Guidelines
- US IRS Publication 1179
- Indian GST Network (GSTN) Standards
