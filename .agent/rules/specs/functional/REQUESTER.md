# Functional Flow: Requester

**Role**: Internal Staff (Change Requester)
**Access Level**: Submit and track change requests
**Last Updated**: 2026-02-05

---

## Important: Route Sharing

**The Requester role uses the same routes as Approver** (`/approver/*`).

The UI shows/hides features based on role permissions, but the URL structure is shared.

---

## Responsibilities

- Submit vendor data change requests
- Track request status
- Respond to reviewer feedback
- Search and select vendors to update

---

## Primary Flows

### 1. Access Worklist

```
Login → /approver/worklist
              │
              └── Change Requests Tab
                  (filtered to own submissions)
```

### 2. Create Change Request

```
Worklist → Select Vendor → /approver/select-vendor
              │
              ▼
        Search Vendor (SAP ID or Name)
              │
              ▼
        Select Vendor
              │
              ▼
        /approver/update-vendor/{vendorId}
              │
              ▼
        Select fields to change:
        - General info
        - Banking details
        - Contact info
        - Tax information
              │
              ▼
        Enter new values
        Upload supporting docs
              │
              ▼
        Submit for approval
```

### 3. Track Requests

```
Worklist → My Submissions
              │
              ├── Filter by status
              │   (Draft, Submitted, Approved, Rejected)
              │
              ├── View request details
              │
              └── Continue draft
```

---

## Actual Routes (from code)

| Action | Path | Component | Description |
|--------|------|-----------|-------------|
| Worklist | `/approver/worklist` | ApproverDashboard.tsx | Shared with Approver |
| History | `/approver/history` | ApproverDashboard.tsx | Past submissions |
| Select Vendor | `/approver/select-vendor` | VendorSelectionList.tsx | Search vendors |
| Update Vendor | `/approver/update-vendor/:vendorId` | ChangeRequestForm.tsx | Submit change |
| View Request | `/approver/requests/:id` | RequestReview.tsx | View only (no approve) |

**Root Redirect**: `/` → `/approver/worklist`

---

## API Endpoints Used

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/sap/vendor/search` | POST | Search vendors |
| `/api/sap/vendor/{vendorNumber}` | GET | Get vendor details |
| `/api/ChangeRequest` | POST | Create change request |
| `/api/attachment/request-upload` | POST | Get upload SAS token |
| `/api/attachment/confirm-upload` | POST | Confirm upload |

---

## Permissions

- ✅ Create change requests
- ✅ View own requests
- ✅ Upload attachments
- ✅ Edit drafts
- ✅ Search vendors
- ❌ Cannot approve requests (view only)
- ❌ Cannot create invitations
- ❌ Cannot view other users' requests

---

## UI Differences from Approver

When logged in as Requester:
- Worklist shows only "My Submissions" tab
- Request detail page has no Approve/Reject buttons
- No "Invite Vendor" or "Create Vendor" options
- Cannot access Onboarding Applications

---

## Related Processes

| Process | Role in Process |
|---------|-----------------|
| [MD Team Modification](../processes/md-team-modification.md) | Initiator |
