# Functional Flow: Admin

**Role**: System Administrator
**Access Level**: Full system access
**Last Updated**: 2026-02-05

---

## Responsibilities

- User management (invite, roles, block)
- System configuration and monitoring
- Audit log access
- Developer tools (role impersonation)
- Event management oversight

---

## Primary Flows

### 1. User Management

```
Login → Admin Dashboard → User Management Tab
                           │
        ┌──────────────────┼──────────────────┐
        ▼                  ▼                  ▼
   Invite User        Edit Roles         Block/Unblock
        │                  │                  │
        ▼                  ▼                  ▼
   Email sent        Update user         Toggle status
```

### 2. System Monitoring

```
Dashboard → System Tab
              │
              ├── Data Sources Status
              │   (SAP, Logic Apps, Rules)
              │
              ├── Audit Log Stream
              │
              └── Service Health
```

### 3. Developer Tools

```
Dashboard → Developer Tools
              │
              ├── Role Impersonation
              │   (Test as different role)
              │
              └── System Configuration
```

---

## Actual Routes (from code)

| Action | Path | Component | Description |
|--------|------|-----------|-------------|
| Dashboard | `/admin/dashboard` | AdminDashboard.tsx | System overview + User management tabs |
| Rules Config | `/admin/rules` | AdminDashboard.tsx | Workflow rules JSON editor |
| System Status | `/admin/system-status` | SystemStatus.tsx | Data source configurations |
| Branch Strategy | `/admin/strategy` | BranchingStrategy.tsx | Branching configuration |
| User Management | `/admin/users` | UserManagement.tsx | Invite, roles, block users |

**Root Redirect**: `/` → `/admin/dashboard`

---

## API Endpoints Used

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/user` | GET | List all users |
| `/api/user` | POST | Create user |
| `/api/user/{id}/roles` | PUT | Update roles |
| `/api/user/{id}/block` | PUT | Block/unblock |
| `/api/auth/invite` | POST | Invite new user |
| `/api/auth/resend-invite` | POST | Resend invitation |
| `/api/auditlog/user/{userId}` | GET | User audit logs |
| `/api/system/data-sources` | GET | System status |
| `/api/system/services` | GET | Mock vs Real status |
| `/api/health` | GET | Health check |

---

## Permissions

- ✅ All Approver permissions
- ✅ User CRUD operations
- ✅ Role assignment (Admin, Approver, Requestor, Vendor, VendorUnit, BFM)
- ✅ Block/unblock users
- ✅ System configuration access
- ✅ View all audit logs
- ✅ Developer tools (impersonation)
- ✅ Event management

---

## Related Processes

| Process | Role in Process |
|---------|-----------------|
| [User Onboarding](../processes/user-onboarding.md) | Initiator |
| [Direct Invitation](../processes/direct-invitation.md) | Oversight |
| [Event Invitation](../processes/event-invitation.md) | Oversight |
| [All Modification Processes](../processes/INDEX.md) | Oversight |
