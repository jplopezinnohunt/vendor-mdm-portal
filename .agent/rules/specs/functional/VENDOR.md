# Functional Flow: Vendor

**Role**: External Vendor
**Access Level**: Self-service registration and status
**Last Updated**: 2026-02-05

---

## Responsibilities

- Complete registration via invitation
- Upload required documents
- Track application status
- Submit change requests for own data

---

## Primary Flows

### 1. Accept Invitation & Register (Multi-Stage)

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
        MFA Trigger       Error Page
              │           "Contact Admin"
              ▼
        Enter 6-digit code
              │
              ▼
        MFA Verified
              │
              ▼
        Initial Info Form:
        - Contact Person
        - Tax ID
        - Address
              │
              ▼
        Enrichment Form:
        - Banking Info
        - Certifications
        - Additional Documents
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

### 2. Vendor Dashboard

```
Login → /dashboard
              │
              ├── Request statistics
              │   (Pending, Approved counts)
              │
              ├── Recent requests table
              │
              └── Quick links
                  ├── View Profile
                  └── New Request
```

### 3. View Profile

```
Dashboard → /profile
              │
              ├── Current SAP master data (read-only)
              │
              └── "Request Change" button
                  → /requests/new
```

### 4. Submit Change Request

```
Profile → "Request Change" → /requests/new
              │
              ▼
        Select fields to change
              │
              ▼
        Enter new values
              │
              ▼
        Upload supporting docs
              │
              ▼
           Submit
              │
              ▼
        Track in /requests
```

### 5. View Request History

```
Dashboard → /requests
              │
              ├── Filter by status
              │   (Approved, Rejected, Applied, Error)
              │
              └── View request details
```

---

## Actual Routes (from code)

| Action | Path | Component | Description |
|--------|------|-----------|-------------|
| Dashboard | `/dashboard` | Dashboard.tsx | Home with stats and recent requests |
| Profile | `/profile` | VendorProfile.tsx | View SAP master data (read-only) |
| Request History | `/requests` | RequestHistory.tsx | Historical requests |
| New Request | `/requests/new` | ChangeRequestForm.tsx | Submit change request |

**Public Routes (No Auth)**:
| Action | Path | Component | Description |
|--------|------|-----------|-------------|
| Registration | `/invitation/register/:token` | InvitationRegistration.tsx | Token-based registration |
| Self Register | `/register` | VendorRegistration.tsx | Self-service registration |
| Accept Invite | `/accept-invite` | InvitationPage.tsx | Legacy invitation page |

**Root Redirect**: `/` → `/profile`

---

## API Endpoints Used

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/invitation/validate/{token}` | GET | Validate token |
| `/api/invitation/trigger-mfa/{token}` | POST | Send MFA code |
| `/api/invitation/verify-mfa/{token}` | POST | Verify MFA code |
| `/api/invitation/submit-initial/{token}` | POST | Submit initial info |
| `/api/invitation/submit-enrichment/{token}` | POST | Submit enrichment |
| `/api/invitation/complete/{token}` | POST | Complete registration |
| `/api/invitation/save-draft/{token}` | POST | Save as draft |
| `/api/ChangeRequest` | POST | Create change request |
| `/api/attachment/request-upload` | POST | Get upload SAS token |
| `/api/attachment/confirm-upload` | POST | Confirm upload |

---

## Permissions

- ✅ Complete registration via invitation
- ✅ Upload documents
- ✅ View own application status
- ✅ Submit change requests for own data
- ✅ View own request history
- ❌ Cannot view other vendors
- ❌ Cannot access internal system
- ❌ No approval capabilities

---

## Registration Stages

| Stage | Field | Description |
|-------|-------|-------------|
| InvitationSent | CurrentStage | Initial email sent |
| MfaVerified | CurrentStage | MFA code verified |
| InitialInfoCompleted | CurrentStage | Basic info submitted |
| Enriched | CurrentStage | All data complete |

---

## Related Processes

| Process | Role in Process |
|---------|-----------------|
| [Direct Invitation](../processes/direct-invitation.md) | Recipient |
| [Event Invitation](../processes/event-invitation.md) | Recipient |
| [Vendor Self-Modification](../processes/vendor-self-modification.md) | Initiator |
