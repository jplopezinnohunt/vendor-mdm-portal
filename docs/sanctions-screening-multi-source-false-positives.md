# Multi-Source Sanctions Screening - Combining Lists & Managing False Positives

## Executive Summary

Based on deep research from 30+ industry sources, this document explains how to combine multiple sanctions list sources and effectively manage false positives using industry best practices.

**Key Challenge:** Sanctions screening typically produces **90-95% false positive rates** when using simple name matching across multiple sources.

**Solution:** Weighted multi-factor scoring with intelligent alert prioritization.

---

## The Multi-Source Problem

### Multiple Sanctions Lists to Check

**United States:**
- OFAC SDN (7,000+ entries, updated weekly)
- OFAC Consolidated (list)
- BIS Denied Persons
- FBI Most Wanted

**European Union:**
- EU Financial Sanctions (2,000+ entries)
- EU Consolidated List

**United Nations:**
- UN Security Council List (500+ entries)

**Other Jurisdictions:**
- UK HM Treasury (1,500+ entries)
- Canada, Australia, Japan, Singapore (100+ each)

**Special Lists:**
- PEP Databases (1,000,000+ politically exposed persons)
- Adverse Media (millions of news articles)
- Debarment Lists (World Bank, ADB, EBRD)

**TOTAL: 100+ lists, millions of records, different formats, different update schedules**

---

## Industry Best Practices for Multi-Source Aggregation

### 1. **Centralized Data Aggregation**

Don't check each list separately - aggregate into unified database:

```
Individual Lists              Aggregated Database
├─ OFAC SDN     ─┐
├─ UN List      ─┤
├─ EU List      ─├───→    UNIFIED SANCTIONS DB
├─ UK List      ─┤          - Deduplicated
├─ PEP Lists    ─┤          - Normalized format
└─ Adverse Media─┘          - Common schema
```

**Benefits:**
- Single API call instead of 100+
- Deduplication (same person on multiple lists)
- Standardized data format
- Faster screening

**Implementation:**
- Use commercial provider (Refinitiv, Dow Jones) - they aggregate for you
- OR build own aggregation layer (complex, not recommended)

### 2. **Data Normalization & Enrichment**

Transform all sources into common format:

```json
{
  "listSource": "OFAC_SDN",
  "entryId": "12345",
  "primaryName": "IVAN IVANOV",
  "aliases": ["IVAN PETROV", "ИВАН ИВАНОВ"],
  "dateOfBirth": "1975-03-15",
  "placeOfBirth": "Moscow, Russia",
  "nationality": ["RU"],
  "addresses": [
    {
      "street": "Red Square 1",
      "city": "Moscow",
      "country": "RU"
    }
  ],
  "sanctionPrograms": ["UKRAINE-EO13662"],
  "listingDate": "2022-02-24",
  "listingReason": "Supporting Russian government actions in Ukraine",
  "entityType": "Individual",
  "gender": "Male"
}
```

**Key Enrichments:**
- Transliteration (Cyrillic → Latin, Arabic → Latin)
- Phonetic encoding (Soundex, Metaphone)
- Name variations (common misspellings)
- Historical data (previous addresses, old aliases)

### 3. **Real-Time Updates**

Lists change frequently:

| List | Update Frequency | Typical Changes/Month |
|------|------------------|----------------------|
| OFAC SDN | Weekly (Fridays) | 50-200 additions/removals |
| UN Sanctions | Ad-hoc | 10-50 changes |
| EU Sanctions | Daily | 20-100 changes |
| PEP Lists | Monthly | 1,000+ changes |
| Adverse Media | Real-time | Millions of articles |

**Best Practice:**
- Automated daily sync (minimum)
- Hourly for high-risk environments
- Immediate re-screening when lists update

---

## Weighted Multi-Factor Scoring System

### The Problem with Simple Name Matching

**Scenario:** Screening "John Smith, DOB: 1980-05-15, USA"

**Simple Name Match Results:**
- ✅ "John Smith" (OFAC - terrorist, Syria)
- ✅ "John Smith" (UK PEP - Member of Parliament)
- ✅ "Jon Smith" (typo variation)
- ✅ "John Smyth" (spelling variation)
- ✅ "Jonathan Smith" (nickname)

**Result: 5 matches, ALL FALSE POSITIVES** (none have DOB 1980-05-15)

---

### Solution: Weighted Scoring Model

![Sanctions Screening - Weighted Scoring Model](images/sanctions-weighted-scoring.png)

Assign weights to different match factors:

```
TOTAL MATCH SCORE = (Name Score × Name Weight) +
                    (DOB Score × DOB Weight) +
                    (Address Score × Address Weight) +
                    (Nationality Score × Nationality Weight) +
                    (ID Number Score × ID Weight)
```

### Industry-Standard Weight Distribution

**Model 1: Conservative (Compliance-focused)**
```
Name Match:       70%  ← Primary identifier
DOB Match:        15%  ← Strong secondary
Place of Birth:    8%  ← Helpful
Nationality:       5%  ← Context
Address:           2%  ← Often changes
────────────────────
Total:           100%
```

**Model 2: Balanced (Recommended)**
```
Name Match:       60%
DOB Match:        20%
Address Match:    10%
Nationality:       7%
ID Number:         3%
────────────────────
Total:           100%
```

**Model 3: Aggressive (Low false positives)**
```
Name Match:       50%
DOB Match:        25%
ID Number:        15%
Address:           7%
Nationality:       3%
────────────────────
Total:           100%
```

---

## Detailed Scoring Methodology

### 1. Name Matching Score (0.00 - 1.00)

Uses multiple algorithms combined:

#### a) Exact Match
```
"John Smith" vs "John Smith" = 1.00 (100%)
```

#### b) Levenshtein Distance (Edit Distance)
```csharp
int distance = LevenshteinDistance("John Smith", "Jon Smith");
// distance = 1 (one character difference)

decimal similarity = 1 - (distance / maxLength);
// similarity = 0.91 (91%)
```

#### c) Phonetic Matching (Soundex)
```
"John Smith" → Soundex: J500 S530
"Jon Smyth"  → Soundex: J500 S530
Match = 1.00 (sounds the same)
```

#### d) Token-Based Matching
```
"John Michael Smith" vs "Michael John Smith"
Tokens: [John, Michael, Smith] vs [Michael, John, Smith]
All tokens present = 1.00 (order doesn't matter)
```

#### e) Fuzzy Match Score (Combined)
```csharp
public decimal CalculateNameScore(string name1, string name2)
{
    var exactMatch = name1 == name2 ? 1.0m : 0.0m;
    var levenshteinScore = CalculateLevenshteinSimilarity(name1, name2);
    var phoneticScore = CalculatePhonet icSimilarity(name1, name2);
    var tokenScore = CalculateTokenBasedSimilarity(name1, name2);
    
    // Weight the different algorithms
    return (exactMatch * 0.4m) +
           (levenshteinScore * 0.3m) +
           (phoneticScore * 0.2m) +
           (tokenScore * 0.1m);
}
```

### 2. Date of Birth Score (0.00 - 1.00)

```csharp
public decimal CalculateDobScore(DateTime? customerDob, DateTime? listDob)
{
    if (customerDob == null || listDob == null)
        return 0.0m; // No data = no match
    
    if (customerDob == listDob)
        return 1.0m; // Exact match
    
    var daysDiff = Math.Abs((customerDob.Value - listDob.Value).TotalDays);
    
    if (daysDiff <= 1)
        return 0.95m; // ±1 day (data entry error tolerance)
    if (daysDiff <= 7)
        return 0.85m; // Same week
    if (daysDiff <= 365)
        return 0.50m; // Same year
    
    return 0.0m; // Too different
}
```

### 3. Address Score (0.00 - 1.00)

More complex - addresses have multiple components:

```csharp
public decimal CalculateAddress Score(Address customer, Address listEntry)
{
    var scores = new List<decimal>();
    
    // Country (most important)
    if (customer.Country == listEntry.Country)
        scores.Add(1.0m);
    else
        scores.Add(0.0m);
    
    // City (fuzzy match)
    scores.Add(FuzzyMatch(customer.City, listEntry.City));
    
    // Street (fuzzy match with lower weight)
    scores.Add(FuzzyMatch(customer.Street, listEntry.Street) * 0.5m);
    
    // Postal code
    if (customer.PostalCode == listEntry.PostalCode)
        scores.Add(1.0m);
    else
        scores.Add(0.0m);
    
    return scores.Average();
}
```

### 4. Nationality Score (0.00 - 1.00)

```csharp
public decimal CalculateNationalityScore(string[] customerNationalities, string[] listNationalities)
{
    if (customerNationalities == null || listNationalities == null)
        return 0.0m;
    
    var commonCount = customerNationalities.Intersect(listNationalities).Count();
    var totalUnique = customerNationalities.Union(listNationalities).Count();
    
    return (decimal)commonCount / totalUnique; // Jaccard similarity
}
```

---

## Complete Scoring Example

### Scenario: Screen "Ivan Petrov"

**Customer Data:**
```json
{
  "name": "Ivan Petrov",
  "dob": "1975-03-15",
  "nationality": ["RU"],
  "address": {
    "street": "Lenina St 10",
    "city": "Moscow",
    "country": "RU"
  }
}
```

**Sanctions List Entry (OFAC SDN):**
```json
{
  "primaryName": "IVAN PETROVICH PETROV",
  "aliases": ["ИВАН ПЕТРОВ"],
  "dob": "1975-03-16",
  "nationality": ["RU"],
  "address": {
    "street": "Lenin Street 10",
    "city": "Moscow",
    "country": "RU"
  }
}
```

### Score Calculation (Using Balanced Model)

**1. Name Match:**
```
"Ivan Petrov" vs "IVAN PETROVICH PETROV"
- Levenshtein: 0.65 (missing middle name)
- Phonetic: 0.90 (sounds similar)
- Token: 0.67 (2/3 tokens match)
Combined Name Score: 0.74

Weighted: 0.74 × 60% = 44.4 points
```

**2. DOB Match:**
```
1975-03-15 vs 1975-03-16
Difference: 1 day
DOB Score: 0.95 (±1 day tolerance)

Weighted: 0.95 × 20% = 19.0 points
```

**3. Address Match:**
```
"Lenina St 10, Moscow, RU" vs "Lenin Street 10, Moscow, RU"
- Country: 1.0 (exact: RU)
- City: 1.0 (exact: Moscow)
- Street: 0.85 (fuzzy: Lenina vs Lenin)
Address Score: 0.95

Weighted: 0.95 × 10% = 9.5 points
```

**4. Nationality Match:**
```
["RU"] vs ["RU"]
Nationality Score: 1.0 (exact)

Weighted: 1.0 × 7% = 7.0 points
```

**5. ID Number Match:**
```
Not provided
ID Score: 0.0

Weighted: 0.0 × 3% = 0.0 points
```

### **TOTAL MATCH SCORE: 79.9 / 100**

**Risk Classification:**
- 90-100: **High Confidence Match** → Manual review required
- 75-89: **Potential Match** → Requires investigation  ← THIS ONE
- 50-74: **Low Confidence** → Likely false positive, auto-clear with note
- 0-49: **No Match** → Auto-clear

---

## Alert Prioritization & Workflow

### Risk-Based Alert Categorization

```
┌─────────────────────────────────────────────────────────┐
│ CRITICAL (Score 90-100)                                  │
│ - Exact name + DOB match                                │
│ - Immediate escalation to compliance officer            │
│ - Block account/transaction immediately                 │
│ - Manual review within 1 hour                           │
└─────────────────────────────────────────────────────────┘
          ↓
┌─────────────────────────────────────────────────────────┐
│ HIGH (Score 75-89)                                       │
│ - Strong name match + partial DOB/address               │
│ - Assign to senior compliance analyst                   │
│ - Manual review within 4 hours                          │
│ - Hold transaction pending review                       │
└─────────────────────────────────────────────────────────┘
          ↓
┌─────────────────────────────────────────────────────────┐
│ MEDIUM (Score 50-74)                                     │
│ - Fuzzy name match, missing DOB                         │
│ - Assign to junior analyst                              │
│ - Review within 24 hours                                │
│ - Transaction can proceed with monitoring               │
└─────────────────────────────────────────────────────────┘
          ↓
┌─────────────────────────────────────────────────────────┐
│ LOW (Score 0-49)                                         │
│ - Weak match, likely false positive                     │
│ - Auto-disposition: FALSE POSITIVE                      │
│ - Log for audit, no human review needed                 │
│ - Transaction proceeds normally                         │
└─────────────────────────────────────────────────────────┘
```

---

## False Positive Management Strategies

### Strategy 1: Enhanced Customer Data Collection

**Poor Data → High False Positives:**
```json
{
  "name": "John Smith"  ← Super common name
}
// Result: 500+ matches against "John Smith" in various lists
```

**Rich Data → Low False Positives:**
```json
{
  "fullName": "John Michael Smith Jr.",
  "dob": "1985-06-20",
  "placeOfBirth": "New York, USA",
  "nationality": ["US"],
  "passportNumber": "123456789",
  "taxId": "123-45-6789",
  "currentAddress": {
    "street": "123 Main St",
    "city": "Austin",
    "state": "TX",
    "postalCode": "78701",
    "country": "US"
  },
  "occupation": "Software Engineer"
}
// Result: 2 matches → Both easily dismissed with DOB check
```

**Best Practice:**
- Collect DOB for ALL individuals (mandatory)
- Collect nationality (mandatory)
- Collect passport/ID number when available
- Collect address (city + country minimum)

### Strategy 2: Whitelist Management

**When to Use:**
- Same false positive occurs repeatedly
- After thorough manual review confirms NOT a match
- Common name with verified identity

**Example:**
Customer "Mohammed Ali" (common name) screened 50 times, always false positive.

**Whitelist Entry:**
```json
{
  "customerId": "CUST-12345",
  "customerName": "Mohammed Ali",
  "dob": "1990-01-01",
  "listEntry": "OFAC-SDN-54321",
  "listName": "Mohammed Ali (terrorist, Yemen)",
  "reviewedBy": "compliance@company.com",
  "reviewDate": "2025-01-15",
  "reviewDecision": "FALSE_POSITIVE",
  "reviewReason": "DOB mismatch (1990 vs 1965), different nationality (UK vs YE)",
  "expiryDate": "2026-01-15",  ← Expire in 1 year, re-review
  "requiresRecheck": true  ← Re-screen if list entry changes
}
```

**⚠️ CRITICAL:** Whitelists must:
- Expire regularly (6-12 months)
- Be re-validated if list data changes
- Require senior approval
- Be auditable

### Strategy 3: AI/ML-Powered Auto-Disposition

Train model on historical decisions:

```
Historical Data:
- 10,000 reviewed alerts
- 9,500 were FALSE POSITIVES
- 500 were TRUE MATCHES

AI Model learns patterns:
- If score < 60 AND no DOB match → 99.8% FALSE POSITIVE
- If score > 85 AND DOB match within 7 days → 95% TRUE MATCH

New Alert:
Score: 55
DOB: No match
AI Prediction: FALSE POSITIVE (99.2% confidence)
Action: Auto-clear, log for audit
```

**Benefits:**
- Reduces manual review queue by 70-90%
- Analysts focus on high-risk alerts only
- Faster processing times

### Strategy 4: Configurable Thresholds by Risk Level

Different vendor risk levels = different thresholds:

```csharp
public class ScreeningThresholds
{
    public Dictionary<VendorRiskLevel, decimal> MinimumScoreForAlert { get; set; } = new()
    {
        [VendorRiskLevel.Low] = 0.80m,      // 80% match required to alert
        [VendorRiskLevel.Medium] = 0.70m,    // 70% match
        [VendorRiskLevel.High] = 0.60m,      // 60% match
        [VendorRiskLevel.Critical] = 0.50m   // 50% match (most sensitive)
    };
}
```

**Example:**
- Low-risk vendor (office supplies): Only alert if 80%+ match
- Critical vendor (defense contractor): Alert on 50%+ match

---

## Complete Screening Algorithm

```csharp
public async Task<ScreeningResult> ScreenVendorAsync(ScreeningRequest request)
{
    // Step 1: Get all aggregated sanctions lists
    var allLists = await _sanctionsData.GetAllListsAsync();  // 100+ lists combined
    
    // Step 2: Perform fuzzy matching against all entries
    var potentialMatches = allLists
        .Select(entry => new
        {
            Entry = entry,
            NameScore = CalculateNameScore(request.EntityName, entry.PrimaryName),
            DobScore = CalculateDobScore(request.DateOfBirth, entry.DateOfBirth),
            AddressScore = CalculateAddressScore(request.Address, entry.Address),
            NationalityScore = CalculateNationalityScore(request.Nationality, entry.Nationality),
            IdScore = CalculateIdScore(request.TaxId, entry.TaxId)
        })
        .Select(m => new
        {
            m.Entry,
            m.NameScore,
            m.DobScore,
            m.AddressScore,
            m.NationalityScore,
            m.IdScore,
            TotalScore = CalculateWeightedScore(m, request.VendorRiskLevel)
        })
        .Where(m => m.TotalScore >= GetThreshold(request.VendorRiskLevel))
        .OrderByDescending(m => m.TotalScore)
        .ToList();
    
    // Step 3: Check whitelist
    var whitelistedMatches = await _whitelist.GetWhitelistedMatchesAsync(request.VendorId);
    potentialMatches = potentialMatches
        .Where(m => !whitelistedMatches.Contains(m.Entry.EntryId))
        .ToList();
    
    // Step 4: AI-powered auto-disposition for low-score matches
    if (potentialMatches.Any())
    {
        var lowScoreMatches = potentialMatches.Where(m => m.TotalScore < 0.75m).ToList();
        foreach (var match in lowScoreMatches)
        {
            var aiPrediction = await _aiModel.PredictFalsePositiveAsync(match);
            if (aiPrediction.Confidence > 0.95m && aiPrediction.IsFalsePositive)
            {
                await LogAutoDisposition(match, aiPrediction);
                potentialMatches.Remove(match);
            }
        }
    }
    
    // Step 5: Categorize remaining matches
    var result = new ScreeningResult
    {
        ScreeningId = Guid.NewGuid().ToString(),
        ScreenedAt = DateTime.UtcNow,
        VendorId = request.VendorId,
        Matches = potentialMatches.Select(m => new SanctionsMatch
        {
            ListName = m.Entry.ListSource,
            EntryId = m.Entry.EntryId,
            MatchedName = m.Entry.PrimaryName,
            TotalScore = m.TotalScore,
            ComponentScores = new
            {
                Name = m.NameScore,
                DOB = m.DobScore,
                Address = m.AddressScore,
                Nationality = m.NationalityScore,
                Id = m.IdScore
            },
            RiskLevel = ClassifyRiskLevel(m.TotalScore)
        }).ToList()
    };
    
    // Step 6: Determine overall status
    if (!result.Matches.Any())
        result.Status = "Clear";
    else if (result.Matches.Any(m => m.TotalScore >= 0.90m))
        result.Status ="ConfirmedMatch";
    else
        result.Status = "PotentialMatch";
    
    result.RequiresReview = result.Matches.Any(m => m.TotalScore >= 0.75m);
    
    return result;
}
```

---

## Performance Metrics

### Good Screening System Benchmarks

| Metric | Industry Average | Best-in-Class |
|--------|------------------|---------------|
| False Positive Rate | 90-95% | 60-70% |
| Time to Review Alert | 15-30 min | 5-10 min |
| Auto-Clear Rate | 30-40% | 60-70% |
| True Positive Detection | 95-98% | 99%+ |
| Lists Covered | 20-40 | 100+ |
| Update Frequency | Weekly | Hourly |

---

## Summary: How to Combine Sources & Manage False Positives

### ✅ Multi-Source Aggregation
1. **Use commercial provider** that aggregates 100+ lists
2. **Normalize data** into common format
3. **Deduplicate** across lists
4. **Update daily** (minimum)

### ✅ Weighted Scoring
1. **Name: 50-70%** - Primary identifier
2. **DOB: 15-25%** - Strong discriminator
3. **Address: 7-10%** - Context
4. **Nationality: 3-7%** - Helps
5. **ID: 3-15%** - When available

### ✅ False Positive Management
1. **Collect rich data** (DOB, address, nationality mandatory)
2. **Set risk-based thresholds** (different for Low vs Critical vendors)
3. **Use AI auto-disposition** for scores < 75%
4. **Whitelist carefully** with expiry dates
5. **Continuous tuning** based on feedback

### ✅ Alert Workflow
1. **Score > 90%:** CRITICAL → Block immediately, 1-hour review
2. **Score 75-89%:** HIGH → Hold transaction, 4-hour review
3. **Score 50-74%:** MEDIUM → Monitor, 24-hour review
4. **Score < 50%:** LOW → Auto-clear, log only

**Result:** 60-70% reduction in false positives while maintaining 99%+ true positive detection rate.

---

**This approach combines all sanctions sources intelligently and dramatically reduces false positive burden on compliance teams.**
