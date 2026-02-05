# Solution Spec: Integrations

**Focus**: External Systems, APIs, Third-Party Services

---

## Integration Map

```
                    ┌─────────────────┐
                    │  Vendor Portal  │
                    └────────┬────────┘
                             │
     ┌───────────────────────┼───────────────────────┐
     │                       │                       │
     ▼                       ▼                       ▼
┌─────────┐           ┌─────────────┐         ┌──────────┐
│   SAP   │           │    Email    │         │ Sanctions│
│  (BAPI) │           │   Service   │         │ Screening│
└─────────┘           └─────────────┘         └──────────┘
  ⏸️ Mock              ✅ Active                📋 Planned
```

---

## 1. SAP Integration

| Attribute | Value |
|-----------|-------|
| **Status** | ⏸️ Mock Available |
| **Protocol** | BAPI (RFC) |
| **Environment** | D01 (pending access) |
| **Pattern** | Mock/Real swap via config |

**Operations**:
- Create Vendor
- Update Vendor
- Get Vendor Details

---

## 2. Email Service

| Attribute | Value |
|-----------|-------|
| **Status** | ✅ Active |
| **Provider** | Azure Communication Services |
| **Pattern** | Queue → Function → Send |

**Triggers**:
- Invitation created
- Application status change
- Reminder (planned)

---

## 3. Sanctions Screening

| Attribute | Value |
|-----------|-------|
| **Status** | 📋 Planned |
| **Phase 1** | Mock service |
| **Phase 2** | OFAC API (free) |
| **Phase 3** | Commercial (Refinitiv/Dow Jones) |

---

## 4. Azure Services

| Service | Purpose | Status |
|---------|---------|--------|
| SQL Database | Relational data | ✅ Active |
| Cosmos DB | Documents + Events | ✅ Active |
| Service Bus | Async messaging | ⚠️ Partial |
| Key Vault | Secrets | ✅ Active |
| Blob Storage | File storage | ✅ Active |
| App Insights | Monitoring | ✅ Active |

---

## Config Pattern

```json
{
  "Services": {
    "Sap": { "UseMock": true },
    "Email": { "UseMock": false },
    "Sanctions": { "UseMock": true }
  }
}
```
