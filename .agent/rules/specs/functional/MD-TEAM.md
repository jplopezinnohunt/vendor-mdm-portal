# Functional Flow: MD Team (Master Data Team)

**Role**: Master Data Specialist
**Access Level**: Vendor data creation and modification
**Last Updated**: 2026-02-05

---

## Important: Route Sharing

**The MD Team role functionality is integrated into the Approver routes** (`/approver/*`).

There are **no separate `/md-team/*` routes** in the current implementation. MD Team members use the Approver interface with equivalent permissions.

---

## Responsibilities

- Create vendors directly (without invitation)
- Modify vendor master data
- Handle data quality issues
- Support vendor onboarding exceptions

---

## Primary Flows

### 1. Create Vendor (Direct)

```
Login → /approver/worklist → "Create Vendor"
              │
              ▼
        /approver/create-vendor
              │
              ▼
        5-Step Wizard:
        1. Definition (Type, Account Group)
        2. Main Data (Company, Contact, Tax)
        3. Profile (Address, Industry)
        4. Financial (Bank, Payment Terms)
        5. Review & Submit
              │
              ▼
        Sanctions screening (auto)
              │
              ▼
        Submit for approval
              │
              ▼
        [Pending Approval]
```

### 2. Modify Vendor Data

```
Worklist → "Update Vendor" or Select Vendor
              │
              ▼
        /approver/select-vendor
              │
              ▼
        Search vendor (SAP ID or Name)
              │
              ▼
        View current data
              │
              ▼
        /approver/update-vendor/{vendorId}
              │
              ▼
        Select fields to change
        Enter new values + justification
        Attach supporting docs
              │
              ▼
        Submit for approval
```

### 3. Review Queue (if also Approver)

```
Worklist → Onboarding Tab
              │
              ├── Pending applications
              │
              ├── Enrich/validate data
              │
              └── Approve or Reject
```

---

## Actual Routes Used (from code)

| Action | Path | Component | Notes |
|--------|------|-----------|-------|
| Worklist | `/approver/worklist` | ApproverDashboard.tsx | Shared route |
| Create Vendor | `/approver/create-vendor` | CreateVendorForm.tsx | 5-step wizard |
| Select Vendor | `/approver/select-vendor` | VendorSelectionList.tsx | Search/select |
| Update Vendor | `/approver/update-vendor/:vendorId` | ChangeRequestForm.tsx | Change request |
| View Request | `/approver/requests/:id` | RequestReview.tsx | Track submissions |

**Root Redirect**: `/` → `/approver/worklist`

**Note**: The documented `/md-team/*` routes do NOT exist in the codebase.

---

## API Endpoints Used

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/vendor` | POST | Create vendor directly |
| `/api/vendor/{id}` | PUT | Update vendor |
| `/api/vendor/search` | GET | Search vendors |
| `/api/sap/vendor/search` | POST | Duplicate detection |
| `/api/ChangeRequest` | POST | Submit change request |
| `/api/sanctions/screen` | POST | Screen vendor |
| `/api/bank/validate-iban` | POST | Validate bank |

---

## Permissions

- ✅ Create vendors directly
- ✅ Submit modification requests
- ✅ Access vendor master data
- ✅ Upload documents
- ✅ View SAP sync status
- ✅ Run sanctions screening
- ❌ Cannot approve own requests
- ❌ Cannot manage users
- ❌ Cannot access system config

---

## Role Equivalence

In the current implementation, MD Team has equivalent capabilities to:
- **VendorUnit** role
- **Approver** role (without approval of own requests)

The distinction is semantic/organizational rather than technical.

---

## Future Consideration

A dedicated `/md-team/*` route structure could be added if:
- MD Team needs different UI layout
- Specific data quality workflows are required
- Separation of concerns becomes necessary

---

## Related Processes

| Process | Role in Process |
|---------|-----------------|
| [MD Team Creation](../processes/md-team-creation.md) | Initiator |
| [MD Team Modification](../processes/md-team-modification.md) | Initiator |
