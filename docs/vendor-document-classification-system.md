# Vendor Document Classification System - Industry Best Practices

## Executive Summary

Based on research of KYC/KYB (Know Your Customer/Business) standards, vendor master data management best practices, and banking verification requirements, this document defines a comprehensive document classification system for vendor onboarding.

**Key Document Categories:**
1. **Company Information** (Legal Entity Verification)
2. **Banking Details** (Payment Account Verification)

---

## Document Classification Taxonomy

![Document Classification Taxonomy](images/document-classification-taxonomy.png)

### Category 1: Company Information Documents

#### Purpose
Verify legal existence, ownership, financial standing, and compliance status of the vendor entity.

#### Sub-Categories & Document Types

```
COMPANY_INFORMATION/
├── LEGAL_ENTITY/
│   ├── certificate_of_incorporation
│   ├── business_registration_certificate
│   ├── articles_of_incorporation
│   ├── articles_of_organization
│   ├── partnership_deed
│   ├── trust_deed
│   └── business_license
│
├── TAX_COMPLIANCE/
│   ├── tax_identification_certificate  (EIN, TIN, VAT)
│   ├── w9_form                           (US)
│   ├── w8_form                           (Non-US)
│   ├── tax_registration_certificate
│   └── tax_clearance_certificate
│
├── IDENTITY_VERIFICATION/
│   ├── INDIVIDUAL/  (for sole proprietors, key personnel)
│   │   ├── passport
│   │   ├── national_id_card
│   │   ├── drivers_license
│   │   └── residence_permit
│   │
│   └── ENTITY/
│       ├── trade_license
│       ├── professional_license
│       └── operating_permit
│
├── ADDRESS_VERIFICATION/
│   ├── utility_bill                      (< 3 months old)
│   ├── lease_agreement
│   ├── property_tax_bill
│   └── bank_statement                    (with address)
│
├── OWNERSHIP_STRUCTURE/
│   ├── ubo_declaration                   (Ultimate Beneficial Owner)
│   ├── shareholder_registry
│   ├── organizational_chart
│   ├── bylaws
│   └── operating_agreement
│
├── FINANCIAL_STANDING/
│   ├── audited_financial_statements      (Last 2-3 years)
│   ├── balance_sheet
│   ├── profit_loss_statement
│   ├── cash_flow_statement
│   └── credit_report
│
└── CERTIFICATIONS_COMPLIANCE/
    ├── iso_certification                 (e.g., ISO 9001, 27001)
    ├── industry_specific_license
    ├── insurance_certificate             (Liability, Professional)
    ├── code_of_conduct
    └── anti_corruption_declaration
```

### Category 2: Banking Details Documents

#### Purpose
Verify bank account ownership, legitimacy, and ensure secure payment processing.

####  Sub-Categories & Document Types

```
BANKING_DETAILS/
├── ACCOUNT_VERIFICATION/
│   ├── bank_letter                       (Official letterhead)
│   ├── bank_statement                    (< 3 months, showing account details)
│   ├── voided_check                      (US/Canada)
│   ├── bank_account_certificate
│   └── swift_bic_confirmation
│
├── ACCOUNT_OWNERSHIP/
│   ├── account_authorization_letter      (Signed by authorized signatory)
│   ├── board_resolution                  (For bank account opening/changes)
│   └── signatory_specimen                (Authorized signatories)
│
├── PAYMENT_DETAILS/
│   ├── ach_authorization_form            (US)
│   ├── sepa_mandate                      (Europe)
│   ├── wire_transfer_instructions
│   └── payment_terms_agreement
│
└── COMPLIANCE/
    ├── aml_questionnaire                 (Anti-Money Laundering)
    ├── sanctions_screening_result
    ├── pep_declaration                   (Politically Exposed Person)
    └── source_of_funds_declaration
```

---

## Document Metadata Schema

### Core Metadata Fields

```csharp
public class DocumentMetadata
{
    // Classification
    public string DocumentCategory { get; set; }        // "COMPANY_INFORMATION" | "BANKING_DETAILS"
    public string DocumentSubCategory { get; set; }     // "LEGAL_ENTITY", "TAX_COMPLIANCE", etc.
    public string DocumentType { get; set; }             // "certificate_of_incorporation", etc.
    
    // Identification
    public string DocumentId { get; set; }
    public string VendorId { get; set; }
    public string FileName { get; set; }
    public string StoragePath { get; set; }
    
    // Content
    public string ContentType { get; set; }             // "application/pdf", "image/jpeg"
    public long FileSizeBytes { get; set; }
    public int PageCount { get; set; }
    
    // Validity
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsExpired { get; set; }
    
    // Geographic
    public string IssuingCountry { get; set; }
    public string IssuingAuthority { get; set; }
    
    // Verification Status
    public string VerificationStatus { get; set; }      // "Pending", "Verified", "Rejected", "Expired"
    public string VerifiedBy { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? RejectionReason { get; set; }
    
    // Security
    public bool IsConfidential { get; set; }
    public string SecurityClassification { get; set; }  // "Public", "Internal", "Confidential", "Restricted"
    public bool ContainsPII { get; set; }                // Personally Identifiable Information
    
    // Processing
    public string VirusScanStatus { get; set; }         // "Pending", "Clean", "Infected"
    public string OcrStatus { get; set; }                // "Pending", "Completed", "Failed"
    public string? ExtractedText { get; set; }
    public Dictionary<string, string> ExtractedData { get; set; }  // Key-value pairs from OCR
    
    // Compliance
    public bool RequiresAnnualRenewal { get; set; }
    public DateTime? NextReviewDate { get; set; }
    public string ComplianceStatus { get; set; }        // "Compliant", "NonCompliant", "UnderReview"
    
    // Audit Trail
    public string UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    
    // Additional
    public Dictionary<string, string> CustomMetadata { get; set; }
    public List<string> Tags { get; set; }
}
```

---

## Risk-Based Document Requirements

### Vendor Risk T iers

```csharp
public enum VendorRiskLevel
{
    Low = 1,      // Standard suppliers, low-value transactions
    Medium = 2,   // Regular suppliers, moderate-value transactions
    High = 3,     // Critical suppliers, high-value transactions
    Critical = 4  // Strategic suppliers, very high-value, regulated industries
}
```

### Document Requirements by Risk Level

| Document Type | Low Risk | Medium Risk | High Risk | Critical |
|---------------|----------|-------------|-----------|----------|
| **Company Information** ||||
| Certificate of Incorporation | Required | Required | Required | Required |
| Tax ID Certificate | Required | Required | Required | Required |
| Business License | Optional | Required | Required | Required |
| Financial Statements | Not Required | Last 1 year | Last 2 years | Last 3 years |
| UBO Declaration | Not Required | Optional | Required | Required |
| ISO Certification | Not Required | Optional | Recommended | Required |
| Insurance Certificate | Not Required | Optional | Required | Required |
| **Banking Details** ||||
| Bank Letter/Statement | Required | Required | Required | Required |
| Voided Check | Optional | Required | Required | Required |
| Account Authorization | Required | Required | Required | Required |
| AML Questionnaire | Not Required | Optional | Required | Required |
| Sanctions Screening | Automated | Automated | Manual + Automated | Enhanced + Continuous |

---

## Document Lifecycle Management

### Stages

```
1. UPLOADED → 2. UNDER_REVIEW → 3. VERIFIED → 4. ACTIVE → 5. EXPIRING_SOON → 6. EXPIRED → 7. ARCHIVED
```

### Automated Workflows

#### Expiry Monitoring

```csharp
// Auto-flag documents expiring within configurable timeframes
public class DocumentExpiryRule
{
    public string DocumentType { get; set; }
    public int WarningDays { get; set; }           // Days before expiry to send warning
    public bool RequiresRenewal { get; set; }
    public bool BlockPayments { get; set; }         // Block payments if document expired?
}

// Examples:
Insurance Certificate → Warning: 30 days, RequiresRenewal: true, BlockPayments: true
Tax Clearance → Warning: 60 days, RequiresRenewal: true, BlockPayments: true
Bank Statement → Warning: 90 days, RequiresRenewal: false, BlockPayments: false
```

#### Validation Rules

```csharp
public class DocumentValidationRule
{
    public string DocumentType { get; set; }
    public bool RequiresManualReview { get; set; }
    public List<string> RequiredFields { get; set; }
    public int MinimumPageCount { get; set; }
    public int MaximumAgeDays { get; set; }         // Max age for document to be valid
    public List<string> AllowedFormats { get; set; }
}
```

---

## Integration with AI/OCR Services

### Extractable Data Points

#### From Company Documents

```json
{
  "certificate_of_incorporation": {
    "company_name": "string",
    "registration_number": "string",
    "incorporation_date": "date",
    "registered_address": "string",
    "company_type": "string"
  },
  "tax_certificate": {
    "tax_id": "string",
    "tax_id_type": "EIN|TIN|VAT",
    "issuing_country": "string",
    "issue_date": "date"
  },
  "financial_statement": {
    "period_end_date": "date",
    "total_revenue": "decimal",
    "total_assets": "decimal",
    "total_liabilities": "decimal",
    "net_income": "decimal"
  }
}
```

#### From Banking Documents

```json
{
  "bank_statement": {
    "account_holder_name": "string",
    "account_number": "string",
    "bank_name": "string",
    "bank_address": "string",
    "iban": "string",
    "swift_bic": "string",
    "routing_number": "string",
    "statement_date": "date",
    "currency": "string"
  },
  "bank_letter": {
    "account_holder_name": "string",
    "account_number": "string",
    "account_type": "Checking|Savings|Business",
    "bank_name": "string",
    "branch_name": "string",
    "swift_bic": "string",
    "iban": "string",
    "signatory_name": "string",
    "signatory_title": "string",
    "letter_date": "date"
  }
}
```

---

## Storage Path Structure (Updated)

### Hierarchical Organization

```
Container: vendor-documents

vendor-documents/
├── {vendor-id}/
│   ├── company-information/
│   │   ├── legal-entity/
│   │   │   ├── certificate-of-incorporation-{guid}.pdf
│   │   │   └── business-license-{guid}.pdf
│   │   ├── tax-compliance/
│   │   │   ├── tax-id-certificate-{guid}.pdf
│   │   │   └── w9-form-{guid}.pdf
│   │   ├── identity-verification/
│   │   │   └── passport-ceo-{guid}.pdf
│   │   ├── address-verification/
│   │   │   └── utility-bill-{guid}.pdf
│   │   ├── ownership-structure/
│   │   │   └── ubo-declaration-{guid}.pdf
│   │   ├── financial-standing/
│   │   │   ├── financial-statements-2024-{guid}.pdf
│   │   │   └── audit-report-2024-{guid}.pdf
│   │   └── certifications/
│   │       └── iso-27001-certificate-{guid}.pdf
│   │
│   └── banking-details/
│       ├── account-verification/
│       │   ├── bank-statement-{guid}.pdf
│       │   └── voided-check-{guid}.pdf
│       ├── account-ownership/
│       │   └── account-authorization-letter-{guid}.pdf
│       ├── payment-details/
│       │   └── wire-transfer-instructions-{guid}.pdf
│       └── compliance/
│           └── aml-questionnaire-{guid}.pdf
```

---

## Database Schema (Updated)

### SQL Table: VendorDocuments

```sql
CREATE TABLE VendorDocuments (
    DocumentId NVARCHAR(50) PRIMARY KEY,
    VendorId NVARCHAR(50) NOT NULL,
    
    -- Classification
    DocumentCategory NVARCHAR(50) NOT NULL,         -- COMPANY_INFORMATION, BANKING_DETAILS
    DocumentSubCategory NVARCHAR(50) NOT NULL,      -- LEGAL_ENTITY, TAX_COMPLIANCE, etc.
    DocumentType NVARCHAR(100) NOT NULL,             -- certificate_of_incorporation, etc.
    
    -- File Info
    FileName NVARCHAR(255) NOT NULL,
    StoragePath NVARCHAR(500) NOT NULL,
    ContentType NVARCHAR(100) NOT NULL,
    FileSizeBytes BIGINT NOT NULL,
    PageCount INT,
    
    -- Validity
    IssueDate DATE,
    ExpiryDate DATE,
    IsExpired AS (CASE WHEN ExpiryDate < GETUTCDATE() THEN 1 ELSE 0 END) PERSISTED,
    
    -- Geographic
    IssuingCountry NVARCHAR(2),                      -- ISO country code
    IssuingAuthority NVARCHAR(200),
    
    -- Verification
    VerificationStatus NVARCHAR(20) NOT NULL DEFAULT 'Pending',  -- Pending, Verified, Rejected, Expired
    VerifiedBy NVARCHAR(100),
    VerifiedAt DATETIME2,
    RejectionReason NVARCHAR(MAX),
    
    -- Security
    IsConfidential BIT NOT NULL DEFAULT 1,
    SecurityClassification NVARCHAR(20) DEFAULT 'Confidential',  -- Public, Internal, Confidential, Restricted
    ContainsPII BIT NOT NULL DEFAULT 0,
    
    -- Processing
    VirusScanStatus NVARCHAR(20) DEFAULT 'Pending',  -- Pending, Clean, Infected
    OcrStatus NVARCHAR(20),                           -- Pending, Completed, Failed
    ExtractedText NVARCHAR(MAX),
    ExtractedDataJson NVARCHAR(MAX),                  -- JSON of extracted key-value pairs
    
    -- Compliance
    RequiresAnnualRenewal BIT NOT NULL DEFAULT 0,
    NextReviewDate DATE,
    ComplianceStatus NVARCHAR(20),                    -- Compliant, NonCompliant, UnderReview
    
    -- Audit
    UploadedBy NVARCHAR(100) NOT NULL,
    UploadedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastModifiedBy NVARCHAR(100),
    LastModifiedAt DATETIME2,
    
    -- Additional
    CustomMetadataJson NVARCHAR(MAX),
    Tags NVARCHAR(500),
    
    -- Indexes
    INDEX IX_VendorDocuments_VendorId (VendorId),
    INDEX IX_VendorDocuments_Category (DocumentCategory, DocumentSubCategory),
    INDEX IX_VendorDocuments_Type (DocumentType),
    INDEX IX_VendorDocuments_ExpiryDate (ExpiryDate) WHERE ExpiryDate IS NOT NULL,
    INDEX IX_VendorDocuments_Verification (VerificationStatus, VerifiedAt DESC),
    
    -- Foreign Key
    CONSTRAINT FK_VendorDocuments_Vendor FOREIGN KEY (VendorId) REFERENCES Vendors(VendorId)
);
```

---

## API Design

### Upload with Classification

```typescript
POST /api/vendor-documents/upload

// Form data
{
  file: File,
  vendorId: "VEN-12345",
  documentCategory: "COMPANY_INFORMATION",
  documentSubCategory: "LEGAL_ENTITY",
  documentType: "certificate_of_incorporation",
  issueDate: "2020-01-15",
  expiryDate: null,  // No expiry
  issuingCountry: "US",
  issuingAuthority: "Delaware Secretary of State",
  requiresAnnualRenewal: false,
  isConfidential: true,
  containsPII: false
}

// Response
{
  "success": true,
  "documentId": "DOC-2025-00123",
  "storagePath": "VEN-12345/company-information/legal-entity/certificate-of-incorporation-abc123.pdf",
  "verificationStatus": "Pending",
  "ocrStatus": "Pending"
}
```

### List by Category

```typescript
GET /api/vendor-documents?vendorId=VEN-12345&category=BANKING_DETAILS&subCategory=ACCOUNT_VERIFICATION

// Response
{
  "documents": [
    {
      "documentId": "DOC-2025-00456",
      "documentType": "bank_statement",
      "fileName": "Chase_Statement_Dec2024.pdf",
      "uploadedAt": "2024-12-15T10:30:00Z",
      "verificationStatus": "Verified",
      "expiryDate": null,
      "isExpired": false
    }
  ]
}
```

### Bulk Verification

```typescript
POST /api/vendor-documents/bulk-verify

{
  "documentIds": ["DOC-2025-00123", "DOC-2025-00124"],
  "verificationStatus": "Verified",
  "verifiedBy": "approver@company.com",
  "notes": "All documents reviewed and approved"
}
```

---

## Compliance & Audit

### Regulatory Alignment

- **SOX (Sarbanes-Oxley):** Complete audit trail, document retention
- **GDPR:** PII flagging, right to erasure, data minimization
- **AML/KYC:** Beneficial owner identification, sanctions screening
- **ISO 27001:** Secure storage, access control, encryption

### Audit Reports

```sql
-- Documents expiring in next 30 days
SELECT VendorId, DocumentType, ExpiryDate
FROM VendorDocuments
WHERE ExpiryDate BETWEEN GETUTCDATE() AND DATEADD(DAY, 30, GETUTCDATE())
AND RequiresAnnualRenewal = 1;

-- Unverified documents older than 7 days
SELECT VendorId, DocumentType, UploadedAt, 
       DATEDIFF(DAY, UploadedAt, GETUTCDATE()) AS DaysUnverified
FROM VendorDocuments
WHERE VerificationStatus = 'Pending'
AND DATEDIFF(DAY, UploadedAt, GETUTCDATE()) > 7;

-- Vendors with missing required documents (by risk level)
SELECT v.VendorId, v.RiskLevel, vd.DocumentType
FROM Vendors v
CROSS APPLY (SELECT * FROM RequiredDocumentsByRiskLevel WHERE RiskLevel = v.RiskLevel) rd
LEFT JOIN VendorDocuments vd ON v.VendorId = vd.VendorId AND vd.DocumentType = rd.DocumentType
WHERE vd.DocumentId IS NULL;
```

---

## Future Enhancements

1. **AI-Powered Validation**
   - Auto-detect document type from content
   - Extract and validate key fields automatically
   - Flag discrepancies (e.g., name mismatch)

2. **Blockchain for Immutability**
   - Store document hashes on blockchain
   - Tamper-proof verification

3. **Real-Time Bank Account Verification**
   - Integrate with Plaid, GIACT, Trustpair
   - Instant account ownership validation

4. **Smart Expiry Management**
   - Auto-request renewals 60 days before expiry
   - Suspend vendor payments if critical docs expired

5. **OCR with Azure Form Recognizer**
   - Custom models for each document type
   - 95%+ accuracy on structured documents

---

## Summary

This document classification system provides:

✅ **Comprehensive taxonomy** for vendor documents  
✅ **Risk-based requirements** for different vendor tiers  
✅ **Automated lifecycle management** with expiry tracking  
✅ **AI/OCR-ready metadata** for future automation  
✅ **Compliance-aligned** with KYC/KYB, SOX, GDPR standards  
✅ **Scalable structure** for hierarchical storage  
✅ **Audit-ready** with complete traceability  

This foundation enables future AI analysis, automated validation, and intelligent document processing while maintaining industry-standard classification and compliance.
