# Process: Vendor Self-Modification

**Trigger**: Vendor needs to update their own data
**Actors**: Vendor, Approver
**Result**: Change Request approved and synced

---

## Flow Diagram

```
┌─────────┐                              ┌──────────┐
│ VENDOR  │                              │ APPROVER │
└────┬────┘                              └────┬─────┘
     │                                        │
     │ 1. Login to Vendor Portal              │
     │                                        │
     │ 2. Navigate to "Update My Info"        │
     │                                        │
     │ 3. Select fields to change:            │
     │    - Contact info                      │
     │    - Banking details                   │
     │    - Tax information                   │
     │    - Address                           │
     │                                        │
     │ 4. Enter new values                    │
     │    Upload supporting docs              │
     │                                        │
     │ 5. Submit change request               │
     │                                        │
     ▼                                        │
┌─────────────┐                               │
│  SYSTEM     │                               │
│  - Create ChangeRequest                     │
│  - Store old vs new values                  │
│  - Attach documents                         │
│  - Notify Approver ────────────────────────►│
└─────────────┘                               │
                                              │
                                              │ 6. Review changes
                                              │    (see diff view)
                                              │
                                              │ 7. Decision
                                              │
                              ┌───────────────┴───────────────┐
                              ▼                               ▼
                         [APPROVE]                       [REJECT]
                              │                               │
                              ▼                               ▼
                    ┌─────────────┐                  Notify Vendor
                    │  SYSTEM     │                  with reason
                    │  - Sync to SAP                       │
                    │  - Update master                     │
                    │  - Notify Vendor                     │
                    │    "Changes approved"                │
                    └─────────────┘                        │
                              │                            │
                              ▼                            ▼
                    [Vendor Data Updated]         [No Changes Made]
```

---

## Change Categories

| Category | Requires Approval | SAP Sync |
|----------|-------------------|----------|
| Contact info | ✅ Yes | ✅ Yes |
| Banking details | ✅ Yes (enhanced) | ✅ Yes |
| Tax information | ✅ Yes (enhanced) | ✅ Yes |
| Address | ✅ Yes | ✅ Yes |
| Certifications | ✅ Yes | ⚠️ Depends |

**Enhanced Review**: Banking and Tax changes require additional verification.

---

## Change Request States

```
[DRAFT] → [SUBMITTED] → [UNDER_REVIEW]
                              │
                    ┌─────────┴─────────┐
                    ▼                   ▼
              [APPROVED]           [REJECTED]
                    │
                    ▼
              [INTEGRATED] (SAP synced)
```

---

## Data Captured

```json
{
  "requestId": "cr-12345",
  "vendorId": "v-67890",
  "requestType": "SelfModification",
  "changes": [
    {
      "field": "contactEmail",
      "oldValue": "old@vendor.com",
      "newValue": "new@vendor.com"
    }
  ],
  "attachments": ["bank_letter.pdf"],
  "submittedAt": "2026-02-05T10:00:00Z",
  "submittedBy": "vendor-user-id"
}
```

---

## Referenced From

- [functional/VENDOR.md](../functional/VENDOR.md) - "Update My Info"
- [functional/APPROVER.md](../functional/APPROVER.md) - "Review Change Requests"
