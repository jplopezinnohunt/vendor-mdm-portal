# Functional Flow: Approver

**Role**: Vendor Unit / Approver
**Access Level**: Invitation & approval workflows
**Last Updated**: 2026-02-05

---

## Responsibilities

- Create vendor invitations (direct or event-based)
- Create vendors directly (bypassing invitation)
- Review vendor onboarding applications
- Approve/reject change requests
- Manage own invitations

---

## Primary Flows

### 1. Worklist (Main Dashboard)

```
Login → /approver/worklist
              │
              ├── KPI Cards
              │   (Total Pending, New Onboarding, Open Invitations, High-Risk)
              │
              └── Tabs
                  ├── Invitations
                  ├── Onboarding Applications
                  └── Change Requests
```

### 2. Invite Vendor (Multi-Step Wizard)

```
Worklist → "Invite Vendor"
              │
              ▼
       Step 1: Selection
       (Search existing or new)
              │
              ▼
       Step 2: Main Data
       (Company, Contact, Expiry)
              │
              ▼
       Step 3: Review & Submit
              │
              ▼
       Duplicate Detection
              │
              ▼
       Sanctions Screening
              │
              ▼
       Invitation Created → Email Sent
```

### 3. Create Vendor Directly (5-Step Wizard)

```
Worklist → "Create Vendor"
              │
              ▼
       Step 1: Definition
       (Vendor Type, Account Group)
              │
              ▼
       Step 2: Main Data
       (Company, Contact, Tax)
              │
              ▼
       Step 3: Profile
       (Address, Industry)
              │
              ▼
       Step 4: Financial
       (Bank Info, Payment Terms)
              │
              ▼
       Step 5: Review & Submit
```

### 4. Review Onboarding Application

```
Worklist → Select Application → /approver/onboarding/{id}
              │
              ├── View submitted data
              ├── View attachments
              ├── Sanctions status
              │
              ▼
        ┌─────┴─────┐
        ▼           ▼
    Approve      Reject
        │           │
        ▼           ▼
   Enrich data   Add reason
        │           │
        ▼           ▼
   SAP Queue    Notify Vendor
```

### 5. Review Change Request

```
Worklist → Select Request → /approver/requests/{id}
              │
              ├── View changes (diff)
              ├── Workflow tracker
              │
              ▼
        ┌─────┴─────┐
        ▼           ▼
    Approve      Reject
        │           │
        ▼           ▼
   SAP Sync     Notify Requester
```

---

## Actual Routes (from code)

| Action | Path | Component | Description |
|--------|------|-----------|-------------|
| Worklist | `/approver/worklist` | ApproverDashboard.tsx | Main dashboard with tabs |
| History | `/approver/history` | ApproverDashboard.tsx | Past decisions |
| Review Request | `/approver/requests/:id` | RequestReview.tsx | Change request review |
| Review Onboarding | `/approver/onboarding/:id` | OnboardingReview.tsx | Application review |
| Invite Vendor | `/approver/invite-vendor` | InviteVendorForm.tsx | 3-step invitation wizard |
| Create Vendor | `/approver/create-vendor` | CreateVendorForm.tsx | 5-step creation wizard |
| Select Vendor | `/approver/select-vendor` | VendorSelectionList.tsx | Search/select vendor |
| Update Vendor | `/approver/update-vendor/:vendorId` | ChangeRequestForm.tsx | Submit change request |
| View Vendor | `/view-vendor` | ViewVendor.tsx | View from duplicate flow |

**Root Redirect**: `/` → `/approver/worklist`

---

## API Endpoints Used

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/invitation/create` | POST | Create invitation |
| `/api/invitation/list` | GET | List invitations |
| `/api/invitation/resend/{id}` | POST | Resend invitation |
| `/api/invitation/cancel/{id}` | POST | Cancel invitation |
| `/api/review/pending` | GET | Pending applications |
| `/api/review/{id}` | GET | Application details |
| `/api/review/{id}/approve` | POST | Approve application |
| `/api/review/{id}/reject` | POST | Reject application |
| `/api/ChangeRequest` | POST | Create change request |
| `/api/ChangeRequest/{id}/approve` | POST | Approve change |
| `/api/vendor` | POST | Create vendor directly |
| `/api/sap/vendor/search` | POST | Duplicate detection |
| `/api/sanctions/screen` | POST | Sanctions screening |

---

## Permissions

- ✅ Create vendor invitations
- ✅ Create vendors directly
- ✅ Manage own invitations
- ✅ Approve/reject applications
- ✅ Approve/reject change requests
- ✅ View sanctions screening results
- ❌ Cannot manage users
- ❌ Cannot access system config

---

## Shared With Other Roles

The `/approver/*` routes are also accessible by:
- **Requestor** - Can submit change requests
- **VendorUnit** - Full approver access
- **BFM** - Budget/Finance approval

---

## Related Processes

| Process | Role in Process |
|---------|-----------------|
| [Direct Invitation](../processes/direct-invitation.md) | Initiator |
| [Event Invitation](../processes/event-invitation.md) | Initiator |
| [Vendor Self-Modification](../processes/vendor-self-modification.md) | Approver |
| [MD Team Modification](../processes/md-team-modification.md) | Approver |
