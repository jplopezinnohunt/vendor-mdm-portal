# Complete Vendor Management Platform - Service Integration Map

## Overview: How All Services Combine

This document shows how all services (SAP, File Storage, Sanctions Screening, Document Classification) integrate to create a complete vendor management platform.

---

## 🏗️ Complete Architecture - All Services Combined

```
┌───────────────────────────────────────────────────────────────────────────

┐
│                          FRONTEND (Static Web App)                           │
│                        Single React/TypeScript App                           │
└────────────────────────────┬────────────────────────────────────────────────┘
                             │
                    All API calls through
                    Canonical REST API
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                         CANONICAL API LAYER                                  │
│                     /api/sap, /api/files, /api/sanctions                    │
│                                                                              │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐   │
│  │     SAP      │  │  Sanctions   │  │     File     │  │    Master    │   │
│  │  Controller  │  │  Controller  │  │  Controller  │  │     Data     │   │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘   │
└─────────┼──────────────────┼──────────────────┼──────────────────┼──────────┘
          │                  │                  │                  │
          │ DI               │ DI               │ DI               │ DI
          │                  │                  │                  │
┌─────────▼──────────────────▼──────────────────▼──────────────────▼──────────┐
│                         SERVICE ABSTRACTION LAYER                            │
│              All services implement interface-based contracts                │
│                                                                              │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐          │
│  │ISapVendorService │  │ISanctionsService │  │IFileStorageServ. │          │
│  └────────┬─────────┘  └────────┬─────────┘  └────────┬─────────┘          │
└───────────┼──────────────────────┼──────────────────────┼────────────────────┘
            │                      │                      │
   Config decides        Config decides          Config decides
   Mock or Real          Mock or Real            Mock or Real
            │                      │                      │
     ┌──────┴──────┐        ┌──────┴──────┐       ┌──────┴──────┐
     │             │        │             │       │             │
     ▼             ▼        ▼             ▼       ▼             ▼
┌─────────┐   ┌─────────┐ ┌──────────┐  ┌────┐ ┌──────┐  ┌─────────┐
│  MOCK   │   │  REAL   │ │   MOCK   │  │REAL│ │ MOCK │  │  REAL   │
│  SAP    │   │   SAP   │ │SANCTIONS │  │API │ │ FILE │  │ AZURE   │
│Simulate │   │ NCo/MoUV│ │Hardcoded │  │OFAC│ │ Disk │  │  BLOB   │
└─────────┘   └─────────┘ └──────────┘  └────┘ └──────┘  └─────────┘
```

---

## 📊 Complete Vendor Onboarding Flow

**NEW VENDOR → Sanctions Screening → Documents → SAP Validation → Approval → SAP Creation**

### Step 1: Basic Data Capture
User submits: Company name, country, tax ID, contact info

### Step 2: ⚠️ SANCTIONS SCREENING (MANDATORY FIRST STEP)
- Check OFAC SDN, UN, EU sanctions lists
- Check PEP (Politically Exposed Persons)
- Check Adverse Media
- **CLEAR →** Continue | **MATCH →** Reject/Review

### Step 3: Document Upload (File Storage)
**Company Information:**
- Certificate of Incorporation
- Tax ID Certificate
- UBO Declaration

**Banking Details:**
- Bank Statement
- Bank Letter
- **Sanctions Screening Report** ← Stored from Step 2!

### Step 4: SAP Validation
- Validate name format
- Validate IBAN/SWIFT
- Check for duplicates

### Step 5: Workflow Approval
- Requestor → Vendor Unit → BFM → SAP Post
- Each can view/download all documents

### Step 6: Create in SAP
- Creates vendor with SAP number
- Links all documents

---

## 🔗 How Components Integrate - Real Example

### Scenario: Onboarding "Acme Corp SARL" from France

**1. Sanctions Screening (Service 1)**
```
POST /api/sanctions/screen
{
  "entityName": "Acme Corporation SARL",
  "country": "FR"
}
→ Result: CLEAR ✅
```

**2. Store Sanctions Report (Service 2: File Storage)**
```
POST /api/files/upload
- file: sanctions_report_SCR-001.pdf
- category: banking-details/compliance
- documentType: sanctions_screening_report
→ Stored at: vendors/VEN-001/banking/compliance/sanctions-report.pdf
```

**3. Upload Other Documents (Service 2)**
```
POST /api/files/upload (multiple calls)
- certificate_incorporation.pdf → company-info/legal-entity/
- tax_certificate.pdf → company-info/tax-compliance/
- bank_statement.pdf → banking-details/account-verification/
```

**4. SAP Bank Validation (Service 3)**
```
POST /api/sap/validate/bank
{
  "iban": "FR7630006000011234567890189",
  "swift": "BNPAFRPPXXX"
}
→ Result: VALID ✅
```

**5. SAP Duplicate Check (Service 3)**
```
POST /api/sap/vendor/search
{
  "companyName": "Acme Corporation"
}
→ Result: No duplicates found ✅
```

**6. Create in SAP (Service 3)**
```
POST /api/sap/vendor
→ Result: SAP Vendor #10189999 created ✅
```

---

## 🗄️ Database Storage - Where Everything is Saved

### Vendors Table
```sql
Vendors
├─ VendorId: VEN-2025-001
├─ LegalName: Acme Corporation SARL
├─ SapVendorNumber: 10189999
└─ Status: Active
```

### Sanctions Screening Log
```sql
SanctionsScreeningLog
├─ ScreeningId: SCR-2025-001
├─ VendorId: VEN-2025-001
├─ Status: Clear
├─ ScreenedAt: 2025-01-15 10:00:00
└─ Matches: [] (empty - no matches)
```

### File Attachments (Multiple records)
```sql
FileAttachments
├─ FILE-001: certificate_incorporation.pdf
│  └─ Path: vendors/VEN-001/company-info/legal-entity/cert.pdf
├─ FILE-002: bank_statement.pdf
│  └─ Path: vendors/VEN-001/banking/account-verification/bank.pdf
└─ FILE-003: sanctions_screening_report.pdf
   └─ Path: vendors/VEN-001/banking/compliance/sanctions.pdf
   └─ Metadata: {screeningId: "SCR-2025-001", status: "Clear"}
```

---

## 💡 KEY INTEGRATION POINTS

### Integration 1: Sanctions → File Storage
**Sanctions report automatically saved as document**

When sanctions screening completes:
1. Generate PDF report
2. Auto-upload via File Storage service
3. Store in `banking-details/compliance/` folder
4. Link to original screening via metadata

### Integration 2: File Storage → SAP
**Documents referenced during SAP validation**

Before creating in SAP:
1. Check all required documents exist
2. Verify sanctions report shows "CLEAR"
3. Validate bank statement uploaded
4. Confirm document expiry dates

### Integration 3: All Services → Workflow
**Approval process has complete visibility**

Each approver sees:
- Sanctions screening status
- All uploaded documents (download links)
- SAP validation results
- Complete audit trail

---

## 📱 Frontend: Single UI, Multiple Services

```typescript
const OnboardVendor = () => {
  // Step 1: Basic Info
  const submitInfo = async (data) => {
    const vendor = await api.post('/vendors', data);
    
    // Step 2: AUTO-TRIGGER SANCTIONS
    const screening = await api.post('/sanctions/screen', {
      entityName: data.legalName
    });
    
    if (screening.status !== 'Clear') {
      return showError('Sanctions check failed');
    }
    
    nextStep();
  };
  
  // Step 3: Upload Docs
  const uploadDocs = async (files) => {
    for (const file of files) {
      await api.post('/files/upload', {
        file,
        app: 'vendors',
        entityId: vendor.id,
        category: file.category
      });
    }
    
    nextStep();
  };
  
  // Step 4: SAP Validation
  const validateSAP = async () => {
    const bankCheck = await api.post('/sap/validate/bank', {
      iban: vendor.iban
    });
    
    const dupCheck = await api.post('/sap/vendor/search', {
      companyName: vendor.name
    });
    
    if (dupCheck.potentialDuplicates > 0) {
      showWarning('Potential duplicates found');
    }
    
    nextStep();
  };
};
```

---

## ✅ Complete Service Summary

| Service | Mock | Real | Purpose | Integration |
|---------|------|------|---------|-------------|
| **SAP** | In-memory | SAP NCo/MoUV | Vendor CRUD, validation | Final step: create vendor |
| **Sanctions** | Hardcoded | OFAC API/Commercial | Compliance screening | **First step**, report stored |
| **File Storage** | Filesystem | Azure Blob | Document management | Stores sanctions report + all docs |
| **Master Data** | In-memory | SQL DB | Countries, currencies | Lookups for all services |
| **Workflow** | In-memory | SQL DB | Approval routing | References all other services |

---

## 🎯 Progressive Rollout Strategy

### Phase 1: All Mock (Local Dev)
```
Config: All UseMock = true
- SAP: Simulated
- Sanctions: Hardcoded test cases
- Files: Local disk (/tmp)
→ Can develop entire flow without external dependencies
```

### Phase 2: Production with Mock (Azure Deployed)
```
Config: All UseMock = true (on Azure)
- SAP: Still simulated
- Sanctions: Still hardcoded
- Files: Still local disk (persistent volume)
→ Real users can test, no SAP/API costs yet
```

### Phase 3: Activate Real Services (One by one)
```
Week 1: Sanctions.UseMock = false → Connect OFAC API
Week 2: Files.UseMock = false → Connect Azure Blob
Week 3: SAP.UseMock = false → Connect real SAP
→ Progressive activation, easy rollback if issues
```

---

## Summary

**All services work together through:**

1. **Common Pattern:** Interface → Mock → Real
2. **Canonical API:** Frontend calls same endpoints regardless
3. **Data Integration:** Services reference each other's data
4. **Storage Integration:** Sanctions reports stored as files
5. **Workflow Integration:** All services visible in approval process
6. **Progressive Rollout:** Can deploy with Mock, activate Real incrementally

**Result:** Complete platform that works locally, deploys to Azure with Mock, and activates real integrations when ready - all without changing frontend code!
