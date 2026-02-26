# Process: Direct Invitation

**Trigger**: Approver knows a specific vendor to onboard
**Actors**: Approver, Vendor
**Result**: New Vendor Application created

---

## Flow Diagram

```
┌─────────┐                              ┌─────────┐
│APPROVER │                              │ VENDOR  │
└────┬────┘                              └────┬────┘
     │                                        │
     │ 1. Create Invitation                   │
     │    (Name, Email, Expiry)               │
     │                                        │
     ▼                                        │
┌─────────────┐                               │
│  SYSTEM     │                               │
│  - Generate token                           │
│  - Store in SQL                             │
│  - Store artifact (Cosmos)                  │
│  - Queue email                              │
└──────┬──────┘                               │
       │                                      │
       │ 2. Send Email ──────────────────────►│
       │                                      │
       │                                      │ 3. Click Link
       │                                      │
       │◄─────────────────────────────────────│
       │    4. Validate Token                 │
       │                                      │
       │ 5. Return Pre-filled Form ──────────►│
       │                                      │
       │                                      │ 6. Complete Form
       │                                      │    Upload Docs
       │                                      │
       │◄─────────────────────────────────────│
       │    7. Submit Application             │
       │                                      │
┌──────▼──────┐                               │
│  SYSTEM     │                               │
│  - Create VendorApplication                 │
│  - Mark invitation Complete                 │
│  - Emit events                              │
│  - Notify Approver                          │
└─────────────┘                               │
                                              ▼
                              [Application Under Review]
```

---

## States & Transitions

| State | Trigger | Next State |
|-------|---------|------------|
| - | Approver creates | **Pending** |
| Pending | Vendor clicks link | **Accepted** |
| Pending | Time exceeds | **Expired** |
| Accepted | Vendor submits | **Completed** |
| Expired | Approver resends | **Pending** |

---

## Data Created

| Entity | Storage | Content |
|--------|---------|---------|
| VendorInvitation | SQL + Cosmos | Token, expiry, inviter |
| VendorApplication | SQL + Cosmos | Registration data |
| Attachments | SQL + Blob | Uploaded documents |
| DomainEvents | Cosmos | Audit trail |

---

## Roles Involved

| Role | Actions |
|------|---------|
| **Approver** | Create invitation, monitor status, resend |
| **Vendor** | Receive email, complete form, upload docs |
| **Admin** | Oversight, cancel any invitation |

---

## Referenced From

- [functional/APPROVER.md](../functional/APPROVER.md) - "Create Invitation"
- [functional/VENDOR.md](../functional/VENDOR.md) - "Accept Invitation"
