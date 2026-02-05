# Process: MD Team Vendor Modification

**Trigger**: Internal need to update vendor data
**Actors**: Requester/MD Team, Approver
**Result**: Change Request approved and synced to SAP

---

## Flow Diagram

```
┌───────────────┐                        ┌──────────┐
│ REQUESTER/    │                        │ APPROVER │
│ MD TEAM       │                        └────┬─────┘
└──────┬────────┘                             │
       │                                      │
       │ 1. Search vendor (SAP ID or name)    │
       │                                      │
       │ 2. View current vendor data          │
       │    (pulled from SAP/master)          │
       │                                      │
       │ 3. Select fields to modify:          │
       │    - General info                    │
       │    - Payment terms                   │
       │    - Banking                         │
       │    - Tax                             │
       │    - Purchasing data                 │
       │                                      │
       │ 4. Enter new values                  │
       │    Provide justification             │
       │    Attach supporting docs            │
       │                                      │
       │ 5. Submit for approval               │
       │                                      │
       ▼                                      │
┌─────────────┐                               │
│  SYSTEM     │                               │
│  - Create ChangeRequest                     │
│  - Link to SAP Vendor ID                    │
│  - Capture requester info                   │
│  - Route to Approver ──────────────────────►│
└─────────────┘                               │
                                              │
                                              │ 6. Review request
                                              │    - View diff
                                              │    - Check justification
                                              │    - Verify docs
                                              │
                                              │ 7. Decision
                                              │
                              ┌───────────────┴───────────────┐
                              ▼                               ▼
                         [APPROVE]                     [REQUEST CHANGES]
                              │                               │
                              ▼                               ▼
                    ┌─────────────┐                  Return to Requester
                    │  SYSTEM     │                  with feedback
                    │  - Queue SAP sync              │
                    │  - Update local master         │
                    │  - Notify Requester            │
                    └──────┬──────┘                  │
                           │                         │
                           ▼                         ▼
                    [SAP INTEGRATION]         [Requester Revises]
                           │                         │
                           ▼                         ▼
                    [INTEGRATED]              [Resubmit]
```

---

## Difference from Vendor Self-Modification

| Aspect | Vendor Self-Mod | MD Team Mod |
|--------|-----------------|-------------|
| Initiator | Vendor | Internal staff |
| Justification | Optional | Required |
| Scope | Own data only | Any vendor |
| Access to SAP fields | Limited | Full |
| Audit trail | Vendor-centric | Business-centric |

---

## Request Types

| Type | Description | Approval Level |
|------|-------------|----------------|
| **General Info** | Name, address, contacts | Standard |
| **Banking** | Payment accounts | Enhanced (2-level) |
| **Tax** | Tax IDs, classifications | Enhanced |
| **Payment Terms** | Credit, payment conditions | Standard |
| **Blocking** | Block/unblock vendor | Manager approval |

---

## States

```
[DRAFT] ─save─► [DRAFT]
    │
    │ submit
    ▼
[SUBMITTED] ──► [UNDER_REVIEW]
                     │
        ┌────────────┼────────────┐
        ▼            ▼            ▼
   [APPROVED]  [CHANGES_REQ]  [REJECTED]
        │            │
        │            └──► [DRAFT] (revised)
        ▼
   [INTEGRATED] (SAP synced)
```

---

## Referenced From

- [functional/REQUESTER.md](../functional/REQUESTER.md) - "Create Change Request"
- [functional/APPROVER.md](../functional/APPROVER.md) - "Review Change Requests"
