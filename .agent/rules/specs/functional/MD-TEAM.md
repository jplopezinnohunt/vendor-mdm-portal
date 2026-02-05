# Functional Flow: MD Team (Master Data Team)

**Role**: Master Data Specialist
**Access Level**: Vendor data creation and modification

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
Login → Dashboard → "Create Vendor"
                         │
                         ▼
              Enter all vendor data:
              - General info
              - Tax information
              - Banking details
              - Payment terms
                         │
                         ▼
              Upload documents
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
Dashboard → "Modify Vendor"
              │
              ▼
        Search vendor (SAP ID or Name)
              │
              ▼
        View current data
              │
              ▼
        Select fields to change
              │
              ▼
        Enter new values + justification
              │
              ▼
        Attach supporting docs
              │
              ▼
        Submit for approval
```

### 3. Handle Data Exceptions

```
Dashboard → "Data Quality Queue"
              │
              ├── Duplicate resolution
              │
              ├── Missing data completion
              │
              └── Data correction requests
```

---

## Available Actions

| Action | Path | Description |
|--------|------|-------------|
| Create Vendor | `/md-team/create` | Direct vendor creation |
| Modify Vendor | `/md-team/modify` | Request data changes |
| Data Queue | `/md-team/queue` | Handle exceptions |
| My Requests | `/md-team/requests` | Track submitted requests |

---

## Permissions

- ✅ Create vendors directly
- ✅ Submit modification requests
- ✅ Access vendor master data
- ✅ Upload documents
- ✅ View SAP sync status
- ❌ Cannot approve own requests
- ❌ Cannot manage users
- ❌ Cannot access system config

---

## Related Processes

| Process | Role in Process |
|---------|-----------------|
| [MD Team Creation](../processes/md-team-creation.md) | Initiator |
| [MD Team Modification](../processes/md-team-modification.md) | Initiator |
