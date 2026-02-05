# Functional Flow: Requester

**Role**: Internal Staff (Change Requester)
**Access Level**: Submit and track change requests

---

## Responsibilities

- Submit vendor data change requests
- Track request status
- Respond to reviewer feedback

---

## Primary Flows

### 1. Create Change Request

```
Login → Dashboard → "New Change Request"
                         │
                         ▼
              Search Vendor (SAP ID or Name)
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

### 2. Track Requests

```
Dashboard → "My Requests"
              │
              ├── Filter by status
              │   (Draft, Submitted, Approved, Rejected)
              │
              ├── View request details
              │
              └── Continue draft
```

### 3. Respond to Feedback

```
Notification: "Changes Requested"
              │
              ▼
        View reviewer comments
              │
              ▼
        Update request
              │
              ▼
        Resubmit
```

---

## Available Actions

| Action | Path | Description |
|--------|------|-------------|
| New Request | `/requester/new` | Create change request |
| My Requests | `/requester/requests` | View all my requests |
| Drafts | `/requester/drafts` | Continue saved drafts |

---

## Permissions

- ✅ Create change requests
- ✅ View own requests
- ✅ Upload attachments
- ✅ Edit drafts
- ❌ Cannot approve requests
- ❌ Cannot create invitations
- ❌ Cannot view other users' requests

---

## Related Processes

| Process | Role in Process |
|---------|-----------------|
| [MD Team Modification](../processes/md-team-modification.md) | Initiator |
