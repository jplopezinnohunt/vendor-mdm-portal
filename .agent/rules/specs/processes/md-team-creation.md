# Process: MD Team Vendor Creation

**Trigger**: Internal need to create vendor (no invitation)
**Actors**: MD Team, Approver
**Result**: New vendor created in system and SAP

---

## When to Use

| Scenario | Use This Process |
|----------|------------------|
| Vendor can't receive email | ✅ Yes |
| Urgent vendor setup | ✅ Yes |
| Migration from legacy | ✅ Yes |
| Data entry on behalf | ✅ Yes |
| Standard onboarding | ❌ Use Invitation instead |

---

## Flow Diagram

```
┌───────────────┐                        ┌──────────┐
│   MD TEAM     │                        │ APPROVER │
└──────┬────────┘                        └────┬─────┘
       │                                      │
       │ 1. Navigate to "Create Vendor"       │
       │                                      │
       │ 2. Enter all vendor data:            │
       │    - Legal name                      │
       │    - Tax ID                          │
       │    - Address                         │
       │    - Contact info                    │
       │    - Banking details                 │
       │    - Payment terms                   │
       │                                      │
       │ 3. Upload required documents         │
       │    - Tax certificate                 │
       │    - Bank verification               │
       │    - Compliance docs                 │
       │                                      │
       │ 4. Sanctions screening (auto)        │
       │    ┌─────────────────────┐          │
       │    │ Screening Result    │          │
       │    │ ✅ Clear: Continue  │          │
       │    │ ⚠️ Match: Review    │          │
       │    │ ❌ Block: Stop      │          │
       │    └─────────────────────┘          │
       │                                      │
       │ 5. Submit for approval               │
       │                                      │
       ▼                                      │
┌─────────────┐                               │
│  SYSTEM     │                               │
│  - Create VendorApplication                 │
│  - Mark as "InternalCreation"               │
│  - Run sanctions check                      │
│  - Route to Approver ──────────────────────►│
└─────────────┘                               │
                                              │
                                              │ 6. Review submission
                                              │    - Verify data
                                              │    - Check documents
                                              │    - Review sanctions
                                              │
                                              │ 7. Decision
                                              │
                              ┌───────────────┴───────────────┐
                              ▼                               ▼
                         [APPROVE]                       [REJECT]
                              │                               │
                              ▼                               │
                    ┌─────────────┐                           │
                    │  SYSTEM     │                           │
                    │  - Create in SAP                        │
                    │  - Generate SAP ID                      │
                    │  - Activate vendor                      │
                    │  - Notify MD Team                       │
                    └──────┬──────┘                           │
                           │                                  │
                           ▼                                  ▼
                    [VENDOR ACTIVE]                   [Request Closed]
```

---

## Difference from Invitation Process

| Aspect | Invitation | MD Team Creation |
|--------|------------|------------------|
| Data entry by | Vendor | MD Team |
| Sanctions check | After submission | During creation |
| Verification | Trust vendor | MD Team validates |
| Use case | Standard | Exception |
| Email required | Yes | No |

---

## Required Data

| Section | Fields | Mandatory |
|---------|--------|-----------|
| **General** | Legal name, Trade name | ✅ |
| **Tax** | Tax ID, VAT number | ✅ |
| **Address** | Street, City, Country | ✅ |
| **Contact** | Name, Email, Phone | ✅ |
| **Banking** | Account, IBAN, BIC | ✅ |
| **Payment** | Terms, Currency | ✅ |
| **Compliance** | Certifications | ⚠️ Per type |

---

## Application States

```
[DRAFT] → [SCREENING] → [PENDING_APPROVAL]
                              │
                    ┌─────────┴─────────┐
                    ▼                   ▼
              [APPROVED]           [REJECTED]
                    │
                    ▼
              [SAP_CREATING]
                    │
                    ▼
              [ACTIVE]
```

---

## Registration Type Flag

```csharp
public enum RegistrationType
{
    SelfRegistration,    // Vendor completed form
    Invitation,          // Via invitation link
    InternalCreation     // MD Team created ← This process
}
```

---

## Referenced From

- [functional/APPROVER.md](../functional/APPROVER.md) - "Review Applications"
- [solution/INTEGRATIONS.md](../solution/INTEGRATIONS.md) - "SAP Integration"
