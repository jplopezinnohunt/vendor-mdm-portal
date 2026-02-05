# Functional Flow: Admin

**Role**: System Administrator
**Access Level**: Full system access

---

## Responsibilities

- User management
- System configuration
- Monitoring & troubleshooting
- Security oversight

---

## Primary Flows

### 1. User Management

```
Login → Admin Dashboard → Users
                           │
        ┌──────────────────┼──────────────────┐
        ▼                  ▼                  ▼
   Create User       Edit User          Deactivate
        │                  │                  │
        ▼                  ▼                  ▼
   Assign Role      Change Role         Audit Log
```

### 2. Invitation Oversight

```
Dashboard → All Invitations
              │
              ├── View statistics
              ├── Cancel invitations
              └── Resend on behalf of approver
```

### 3. System Monitoring

```
Dashboard → System Status
              │
              ├── Service health
              ├── Error logs
              └── Performance metrics
```

---

## Available Actions

| Action | Path | Description |
|--------|------|-------------|
| View Users | `/admin/users` | List all users |
| Create User | `/admin/users/new` | Add new user |
| System Health | `/admin/system` | View service status |
| All Invitations | `/admin/invitations` | Manage all invitations |
| Audit Trail | `/admin/audit` | View system events |

---

## Permissions

- ✅ All Approver permissions
- ✅ User CRUD operations
- ✅ System configuration
- ✅ View audit logs
- ✅ Cancel any invitation

---

## Related Processes

| Process | Role in Process |
|---------|-----------------|
| [Direct Invitation](../processes/direct-invitation.md) | Oversight |
| [Event Invitation](../processes/event-invitation.md) | Oversight |
| [MD Team Creation](../processes/md-team-creation.md) | Oversight |
| [All Modification Processes](../processes/INDEX.md) | Oversight |
