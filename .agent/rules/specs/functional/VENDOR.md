# Functional Flow: Vendor

**Role**: External Vendor
**Access Level**: Self-service registration and status

---

## Responsibilities

- Complete registration via invitation
- Upload required documents
- Track application status

---

## Primary Flows

### 1. Accept Invitation & Register

```
Receive Email → Click Invitation Link
                      │
                      ▼
              Token Validation
              ┌───────┴───────┐
              ▼               ▼
           Valid          Invalid/Expired
              │               │
              ▼               ▼
        Pre-filled Form   Error Page
        (Company, Email)  "Contact Admin"
              │
              ▼
        Complete Form:
        - Tax ID
        - Contact Person
        - Address
        - Banking Info
              │
              ▼
        Upload Documents:
        - Tax certificate
        - Bank verification
        - Compliance docs
              │
              ▼
           Submit
              │
              ▼
        Confirmation Page
        "Application Submitted"
```

### 2. Track Status (After Registration)

```
Login (via magic link or email)
              │
              ▼
        Vendor Portal
              │
              ├── Application status
              │   (Submitted, Under Review, Approved)
              │
              ├── View submitted data
              │
              └── Upload additional docs
                  (if requested)
```

### 3. Respond to Document Request

```
Receive Email: "Additional Documents Required"
              │
              ▼
        Click link → Vendor Portal
              │
              ▼
        View request details
              │
              ▼
        Upload requested documents
              │
              ▼
        Submit for review
```

---

## Available Paths

| Path | Access | Description |
|------|--------|-------------|
| `/invitation/register/:token` | Public | Registration form |
| `/vendor/status` | Authenticated | Track application |
| `/vendor/documents` | Authenticated | Manage documents |

---

## Permissions

- ✅ Complete registration
- ✅ Upload documents
- ✅ View own application status
- ❌ Cannot view other vendors
- ❌ Cannot access internal system
- ❌ No system administration

---

## Related Processes

| Process | Role in Process |
|---------|-----------------|
| [Direct Invitation](../processes/direct-invitation.md) | Recipient |
| [Event Invitation](../processes/event-invitation.md) | Recipient |
| [Vendor Self-Modification](../processes/vendor-self-modification.md) | Initiator |
