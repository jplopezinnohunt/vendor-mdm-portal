# Sanctions Screening Service - Research & Implementation Plan

## Executive Summary

Sanctions screening is a **critical compliance requirement** that must be performed at the start of vendor evaluation. This service checks vendors against global watchlists and sanctions databases to prevent engagement with prohibited entities.

**Research Sources:** 18 industry best practice sources  
**Compliance Bodies:** OFAC (US), UN, EU, UK, AUSTRAC, and 100+ jurisdictions  
**API Providers:** Sanctions.io, OFAC-API.com, Dilisense, Refinitiv World-Check, Dow Jones, and others  

---

## Business Requirement

> **"When you start a vendor evaluation, the first thing to do is sanctions screening against official databases"**

This is a **mandatory first step** in vendor onboarding:
1. Vendor submits information
2. **→ SANCTIONS SCREENING** (immediate API call)
3. If clear → Continue onboarding
4. If match → Enhanced Due Diligence (EDD) or rejection

---

## What is Sanctions Screening?

### Purpose
Check vendor entities (companies, individuals, UBOs) against:
- **Sanctions Lists** - Prohibited entities (OFAC SDN, UN, EU)
- **PEP Lists** - Politically Exposed Persons
- **Watchlists** - Adverse media, criminal records
- **Enforcement Lists** - Debarred/excluded parties

### Compliance Requirement
- **Legal Obligation** in most jurisdictions
- **Severe Penalties** for violations:
  - Fines up to millions (e.g., $500M+ OFAC fines)
  - Criminal charges
  - Loss of business licenses
  - Reputational damage

---

## Major Sanctions Lists

### United States
- **OFAC SDN List** (Specially Designated Nationals)
- **OFAC Consolidated Sanctions List**
- **FBI Most Wanted**
- **BIS Denied Persons** (Bureau of Industry & Security)

### European Union
- **EU Financial Sanctions List**
- **EU Consolidated List**

### United Nations
- **UN Security Council Consolidated List**

### United Kingdom
- **UK HM Treasury Sanctions List**

### Other Jurisdictions
- Canada, Australia, Japan, Singapore, Switzerland
- **100+ country-specific lists**

### Special Categories
- **PEP Lists** - Politically Exposed Persons
- **Adverse Media** - Negative news, criminal activity
- **Debarment Lists** - World Bank, ADB, EBRD

---

## Industry Best Practices

### 1. Comprehensive Multi-Jurisdictional Coverage
✅ Screen against **ALL relevant lists** in jurisdictions where you operate  
✅ If you transact in USD, you **MUST** comply with US (OFAC) sanctions  
✅ Use providers that aggregate 100+ global sources  

### 2. Continuous Monitoring
✅ Screen at **onboarding** AND **ongoing**  
✅ Lists update frequently (sometimes **hourly**)  
✅ Re-screen existing vendors when lists update  
✅ Automated daily/hourly updates  

### 3. Risk-Based Approach (RBA)
✅ Assign vendor risk levels (Low/Medium/High/Critical)  
✅ Higher-risk vendors → More frequent screening  
✅ Critical vendors → Real-time monitoring  

### 4. Automated Screening
✅ Use APIs for automation  
✅ Reduce manual errors  
✅ Handle large volumes efficiently  

### 5. Advanced Matching Techniques
✅ **Fuzzy matching** - Handle spelling variations  
✅ **Phonetic matching** - Sound-alike names  
✅ **Alias matching** - Known alternative names  
✅ **Transliteration** - Different alphabets (Cyrillic, Arabic)  
✅ Configurable **threshold** (e.g., 85% match = alert)  

### 6. Transaction-Level Screening
✅ Screen payments, not just entities  
✅ Detect risks in downstream transactions  

### 7. Comprehensive Audit Trail
✅ Log every screening action  
✅ Document decisions on matches  
✅ Retain records for regulatory audits  

### 8. False Positive Management
✅ Clear procedures for investigating matches  
✅ Whitelist known false positives  
✅ Balance accuracy with operational efficiency  

---

## API Integration Best Practices

### 1. Real-Time + Batch Support
- **Real-Time:** Immediate screening during onboarding
- **Batch:** Periodic re-screening of entire vendor database

### 2. Extensive Data Coverage
- **100+ global lists** from official sources
- Daily or **hourly data updates**
- Historical data for point-in-time checks

### 3. Smart Matching
- Configurable fuzzy match threshold
- Reduced false positives
- Explanation of match reasons

### 4. Audit-Ready Results
- Complete log of every match
- User decisions captured
- Exportable reports

### 5. High Reliability
- 99.99%+ uptime SLA
- Fast response times (<5 seconds)
- Fallback mechanisms

### 6. Developer-Friendly
- RESTful API
- Comprehensive documentation
- Code examples, SDKs
- Postman collections

---

## Service Architecture (Mock/Real Pattern)

### Interface Design

```csharp
public interface ISanctionsScreeningService
{
    // Screen a single entity
    Task<ScreeningResult> ScreenEntityAsync(ScreeningRequest request);
    
    // Batch screening
    Task<List<ScreeningResult>> ScreenBatchAsync(List<ScreeningRequest> requests);
    
    // Check screening status
    Task<ScreeningResult> GetScreeningResultAsync(string screeningId);
    
    // Get lists last updated timestamp
    Task<ListsUpdateInfo> GetListsUpdateInfoAsync();
}
```

### Models

```csharp
public class ScreeningRequest
{
    public string EntityType { get; set; }        // "Individual", "Company", "UBO"
    public string VendorId { get; set; }
    public string EntityName { get; set; }
    public string? AlternativeName { get; set; }
    public DateTime? DateOfBirth { get; set; }    // For individuals
    public string? PlaceOfBirth { get; set; }
    public string? Nationality { get; set; }
    public string? TaxId { get; set; }
    public string? RegistrationNumber { get; set; } // For companies
    public string? CountryOfIncorporation { get; set; }
    public List<string>? Aliases { get; set; }
}

public class ScreeningResult
{
    public string ScreeningId { get; set; }
    public DateTime ScreenedAt { get; set; }
    public string Status { get; set; }            // "Clear", "PotentialMatch", "Confirmed Match"
    public RiskLevel OverallRisk { get; set; }    // Low, Medium, High, Critical
    public List<SanctionsMatch> Matches { get; set; }
    public bool RequiresReview { get; set; }
    public string? RecommendedAction { get; set; }
}

public class SanctionsMatch
{
    public string ListName { get; set; }          // "OFAC SDN", "UN Sanctions"
    public string ListSource { get; set; }        // "ofac.treas.gov"
    public string EntryId { get; set; }
    public string MatchedName { get; set; }
    public decimal MatchScore { get; set; }       // 0.00 - 1.00
    public string MatchType { get; set; }         // "Name", "Alias", "AssociatedEntity"
    public string Reason { get; set; }            // Why sanctioned
    public string Sanctions Details { get; set; } // Programs, dates
    public DateTime? ListUpdateDate { get; set; }
}

public enum RiskLevel
{
    Clear = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}
```

---

## Implementation Phases

### Phase 1: Mock Service (Immediate)
```csharp
public class SanctionsScreeningSimulationService : ISanctionsScreeningService
{
    // Hardcoded test cases
    // - Clear: "John Smith" → No matches
    // - Match: "Vladimir Putin" → OFAC/UN match
    // - PEP: "Joe Biden" → PEP match
    // Configurable match threshold
    // Simulated latency (1-3 seconds)
}
```

### Phase 2: Real Service - Free OFAC API
```csharp
public class SanctionsScreeningOfacService : ISanctionsScreeningService
{
    // Use free OFAC-API.com or similar
    // Limited to OFAC SDN list only
    // Good for initial production deployment
    // Limitations: US only, no PEP, basic matching
}
```

### Phase 3: Real Service - Commercial Provider
```csharp
public class SanctionsScreeningCommercialService : ISanctionsScreeningService
{
    // Integrate with Refinitiv World-Check, Dow Jones, or Sanctions.io
    // 100+ global lists
    // Advanced fuzzy matching
    // PEP, adverse media, UBO screening
    // Continuous monitoring
    // Cost: $$$
}
```

---

## Commercial Provider Options

### Tier 1: Enterprise Solutions
1. **Refinitiv World-Check**
   - Most comprehensive database
   - 100+ million profiles
   - Real-time updates
   - Cost: High (enterprise pricing)

2. **Dow Jones Risk & Compliance**
   - Extensive watchlist coverage
   - Adverse media scanning
   - Cost: High

3. **LexisNexis Bridger**
   - Global sanctions & PEP data
   - AI-powered matching
   - Cost: High

### Tier 2: Mid-Market Solutions
4. **Sanctions.io**
   - API-first approach
   - Good coverage (OFAC, UN, EU)
   - Developer-friendly
   - Cost: Medium (~$99-499/month)

5. **Dilisense**
   - 40+ official lists
   - Real-time API
   - Cost: Medium

6. **ComplyAdvantage**
   - AI-powered screening
   - Good for fintech
   - Cost: Medium

### Tier 3: Free/Low-Cost Options
7. **OFAC-API.com**
   - Free tier available
   - OFAC SDN only
   - Basic matching
   - Cost: Free - $49/month

8. **OpenSanctions.org**
   - Open-source database
   - Aggregates public lists
   - API available
   - Cost: Free (donations accepted)

---

## Configuration

### appsettings.json

```json
{
  "Services": {
    "SanctionsScreening": {
      "UseMock": true,
      "RealProvider": "OfacApi",
      "AutoScreenOnOnboarding": true,
      "MatchThreshold": 0.85,
      "MockSettings": {
        "SimulateLatency": true,
        "LatencyMs": 2000,
        "ForcedMatches": ["Vladimir Putin", "Osama Bin Laden"]
      },
      "OfacApiSettings": {
        "BaseUrl": "https://api.ofac-api.com/v4",
        "ApiKey": "from-keyvault",
        "Timeout": 10
      },
      "CommercialSettings": {
        "Provider": "Refinitiv",
        "BaseUrl": "https://api-worldcheck.refinitiv.com/v2",
        "ApiKey": "from-keyvault",
        "ListsEnabled": ["OFAC", "UN", "EU", "PEP", "AdverseMedia"]
      }
    }
  }
}
```

---

## Integration Points

### 1. Vendor Onboarding
```csharp
// In VendorOnboardingService.cs
public async Task OnboardVendorAsync(VendorRegistrationData vendor)
{
    // Step 1: SANCTIONS SCREENING (FIRST!)
    var screeningRequest = new ScreeningRequest
    {
        EntityType = "Company",
        VendorId = vendor.Id,
        EntityName = vendor.LegalName,
        CountryOfIncorporation = vendor.Country,
        RegistrationNumber = vendor.TaxId
    };
    
    var screeningResult = await _sanctionsScreening.ScreenEntityAsync(screeningRequest);
    
    if (screeningResult.Status == "ConfirmedMatch" || screeningResult.OverallRisk == RiskLevel.Critical)
    {
        // REJECT immediately
        throw new ComplianceViolationException("Vendor matched sanctions list");
    }
    
    if (screeningResult.Status == "PotentialMatch" && screeningResult.RequiresReview)
    {
        // Route to compliance team for manual review
        await _workflow.CreateComplianceReviewTask(vendor.Id, screeningResult);
    }
    
    // Step 2: Continue with normal KYC/onboarding
    // ...
}
```

### 2. UBO Screening
```csharp
// Screen all Ultimate Beneficial Owners
public async Task ScreenUBOsAsync(string vendorId, List<UBO> ubos)
{
    var requests = ubos.Select(ubo => new ScreeningRequest
    {
        EntityType = "Individual",
        VendorId = vendorId,
        EntityName = $"{ubo.FirstName} {ubo.LastName}",
        DateOfBirth = ubo.DateOfBirth,
        Nationality = ubo.Nationality
    }).ToList();
    
    var results = await _sanctionsScreening.ScreenBatchAsync(requests);
    
    foreach (var result in results.Where(r => r.RequiresReview))
    {
        await _compliance.FlagForReview(vendorId, result);
    }
}
```

### 3. Continuous Monitoring
```csharp
// Background job - re-screen all active vendors daily
public async Task RescreenAllVendorsAsync()
{
    var activeVendors = await _db.Vendors
        .Where(v => v.Status == "Active")
        .ToListAsync();
    
    var requests = activeVendors.Select(v => new ScreeningRequest { /* ... */ }).ToList();
    var results = await _sanctionsScreening.ScreenBatchAsync(requests);
    
    foreach (var result in results.Where(r => r.Status != "Clear"))
    {
        await _compliance.CreateAlert(result);
    }
}
```

---

## Compliance & Audit

### Screening Log Table

```sql
CREATE TABLE SanctionsScreeningLog (
    ScreeningId NVARCHAR(50) PRIMARY KEY,
    VendorId NVARCHAR(50) NOT NULL,
    EntityType NVARCHAR(20) NOT NULL,
    EntityName NVARCHAR(500) NOT NULL,
    ScreenedAt DATETIME2 NOT NULL,
    Status NVARCHAR(20) NOT NULL,     -- Clear, PotentialMatch, ConfirmedMatch
    OverallRisk NVARCHAR(20),
    MatchCount INT NOT NULL DEFAULT 0,
    MatchDetailsJson NVARCHAR(MAX),    -- JSON of all matches
    ReviewedBy NVARCHAR(100),
    ReviewedAt DATETIME2,
    ReviewDecision NVARCHAR(20),       -- Approved, Rejected, EscalatedToRisk
    ReviewNotes NVARCHAR(MAX),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    INDEX IX_SanctionsScreeningLog_VendorId (VendorId),
    INDEX IX_SanctionsScreeningLog_Status (Status),
    INDEX IX_SanctionsScreeningLog_ScreenedAt (ScreenedAt DESC),
    
    CONSTRAINT FK_SanctionsScreeningLog_Vendor 
        FOREIGN KEY (VendorId) REFERENCES Vendors(VendorId)
);
```

### Audit Reports

```sql
-- Vendors screened in last 30 days
SELECT Status, COUNT(*) as Count
FROM SanctionsScreeningLog
WHERE ScreenedAt >= DATEADD(DAY, -30, GETUTCDATE())
GROUP BY Status;

-- Pending review
SELECT v.VendorName, s.EntityName, s.MatchCount, s.ScreenedAt
FROM SanctionsScreeningLog s
JOIN Vendors v ON s.VendorId = v.VendorId
WHERE s.Status = 'PotentialMatch'
AND s.ReviewedAt IS NULL
ORDER BY s.ScreenedAt DESC;

-- High-risk vendors requiring action
SELECT v.VendorId, v.VendorName, s.OverallRisk, s.MatchDetailsJson
FROM SanctionsScreeningLog s
JOIN Vendors v ON s.VendorId = v.VendorId
WHERE s.OverallRisk IN ('High', 'Critical')
AND s.ReviewDecision IS NULL;
```

---

## Regulatory Reporting

Many jurisdictions require **OFAC reporting**:
- File **Blocked Property Report** within 10 days
- Annual reporting of blocked assets
- Real-time notification for confirmed matches

Implement:
```csharp
public async Task ReportToRegulatorsAsync(ScreeningResult matchResult)
{
    if (matchResult.Status == "ConfirmedMatch")
    {
        // Auto-file regulatory report
        await _ofacReporting.FileBlockedPropertyReportAsync(matchResult);
        
        // Freeze vendor account
        await _vendors.FreezeVendorAsync(matchResult.VendorId);
        
        // Notify compliance officer
        await _notifications.SendComplianceAlertAsync(matchResult);
    }
}
```

---

## Testing Strategy

### Mock Service Test Cases
```csharp
[Fact]
public async Task ScreenEntity_SanctionedName_Returns ConfirmedMatch()
{
    var request = new ScreeningRequest { EntityName = "Vladimir Putin" };
    var result = await _service.ScreenEntityAsync(request);
    Assert.Equal("ConfirmedMatch", result.Status);
    Assert.True(result.Matches.Any());
}

[Fact]
public async Task ScreenEntity_CleanName_ReturnsClear()
{
    var request = new ScreeningRequest { EntityName = "Acme Corporation" };
    var result = await _service.ScreenEntityAsync(request);
    Assert.Equal("Clear", result.Status);
}
```

---

## Cost Considerations

### Free Tier (OFAC API)
- **Cost:** $0 - $49/month
- **Coverage:** OFAC SDN only
- **Suitable for:** MVP, US-only operations
- **Limitation:** No PEP, no EU/UN, basic matching

### Mid-Tier (Sanctions.io)
- **Cost:** $99 - $499/month
- **Coverage:** 40+ lists
- **Suitable for:** Growing businesses, multi-jurisdiction
- **Good balance** of coverage and cost

### Enterprise (Refinitiv/Dow Jones)
- **Cost:** $5,000 - $50,000/year
- **Coverage:** 100+ million profiles
- **Suitable for:** Large enterprises, banks, regulated industries
- **Full compliance** capability

---

## Next Steps

### Immediate (This Sprint)
1. Review and approve sanctions screening architecture
2. Implement ISanctionsScreeningService interface
3. Build Mock service with test cases
4. Add SanctionsScreeningLog table
5. Integrate into vendor onboarding workflow

### Short-Term (Next Sprint)
6. Integrate free OFAC API for production
7. Build compliance review UI for potential matches
8. Add audit reports
9. Test end-to-end workflow

### Medium-Term (Next Month)
10. Evaluate commercial providers (Sanctions.io, World-Check)
11. Implement batch re-screening background job
12. Add UBO screening
13. Build regulatory reporting capability

### Long-Term (Next Quarter)
14. Continuous monitoring with webhooks
15. AI-powered false positive reduction
16. Transaction-level screening
17. Advanced analytics dashboard

---

## Summary

Sanctions screening is:
- ✅ **Mandatory compliance requirement**
- ✅ **First step in vendor onboarding**
- ✅ **Multi-jurisdictional** (OFAC, UN, EU, 100+ lists)
- ✅ **Continuous process** (not one-time)
- ✅ **Automated via API** for efficiency
- ✅ **Audit trail required** for regulators
- ✅ **Severe penalties** for violations

We will implement this as a **canonical service** following the Mock/Real pattern:
- **Mock:** Hardcoded test cases for development
- **Real Tier 1:** Free OFAC API for initial production
- **Real Tier 2:** Commercial provider for full compliance

This ensures we meet compliance requirements while maintaining flexibility for future enhancements.
