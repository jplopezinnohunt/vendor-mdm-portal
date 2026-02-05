# Functional Flow: Approver

**Role**: Vendor Unit / Approver
**Access Level**: Invitation & approval workflows

---

## Responsibilities

- Create vendor invitations
- Review vendor applications
- Approve/reject change requests
- Manage own invitations

---

## Primary Flows

### 1. Create Invitation

```
Login → Dashboard → "Invite Vendor"
                         │
                         ▼
              Fill Form:
              - Vendor Legal Name
              - Contact Email
              - Expiration (7/14/30 days)
              - Notes (optional)
                         │
                         ▼
                    Submit
                         │
                         ▼
              Success Page
              - Copy invitation link
              - Send to vendor
```

### 2. Manage Invitations

```
Dashboard → "My Invitations"
              │
              ├── Filter by status
              │   (Pending, Accepted, Completed, Expired)
              │
              ├── View details
              │
              ├── Resend invitation
              │   (generates new token)
              │
              └── Cancel invitation
```

### 3. Review Applications

```
Dashboard → "Pending Applications"
              │
              ▼
        Select Application
              │
              ├── View vendor data
              ├── View attachments
              ├── Check sanctions status
              │
              ▼
        ┌─────┴─────┐
        ▼           ▼
    Approve      Reject
        │           │
        ▼           ▼
   SAP Queue    Notify Vendor
```

### 4. Review Change Requests

```
Dashboard → "Change Requests"
              │
              ▼
        Select Request
              │
              ├── View changes (diff)
              ├── Review impact
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

## Available Actions

| Action | Path | Description |
|--------|------|-------------|
| Invite Vendor | `/approver/invite` | Create new invitation |
| My Invitations | `/approver/invitations` | Manage invitations |
| Pending Apps | `/approver/applications` | Review applications |
| Change Requests | `/approver/requests` | Review change requests |

---

## Permissions

- ✅ Create invitations
- ✅ Manage own invitations
- ✅ Approve/reject applications
- ✅ Approve/reject change requests
- ❌ Cannot manage users
- ❌ Cannot access system config

---

## Related Processes

| Process | Role in Process |
|---------|-----------------|
| [Direct Invitation](../processes/direct-invitation.md) | Initiator |
| [Event Invitation](../processes/event-invitation.md) | Initiator |
| [Vendor Self-Modification](../processes/vendor-self-modification.md) | Approver |
| [MD Team Modification](../processes/md-team-modification.md) | Approver |
