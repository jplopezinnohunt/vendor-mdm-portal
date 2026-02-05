# Solution Spec: Core

**Focus**: Architecture, Entities, Tech Stack

---

## Tech Stack

| Layer | Technology |
|-------|------------|
| Frontend | React 19 + TypeScript 5.8 + Vite |
| Backend | ASP.NET Core 8 |
| DB (Relational) | Azure SQL Database |
| DB (Document) | Azure Cosmos DB Serverless |
| Messaging | Azure Service Bus |
| Functions | Azure Functions (Isolated) |
| Hosting | Azure Static Web Apps + App Service |

---

## Core Entities

```
VendorInvitation ──creates──► VendorApplication
       │                            │
       ▼                            ▼
  [Attachments]              [ChangeRequest]
                                    │
                                    ▼
                              [Attachments]
```

| Entity | Purpose | Storage |
|--------|---------|---------|
| VendorInvitation | Pre-auth onboarding | SQL + Cosmos |
| VendorApplication | Registration data | SQL + Cosmos |
| ChangeRequest | Data modifications | SQL + Cosmos |
| Attachment | Document metadata | SQL (blob in Storage) |
| UserAndRole | Auth & RBAC | SQL |

---

## Hybrid Data Pattern

**Every entity change**:
1. SQL: Metadata + searchable fields + `Attributes` JSON
2. Cosmos: Full artifact payload
3. Cosmos: Domain event (audit trail)

---

## Project Structure

```
vendor-mdm-portal/
├── frontend/           # React SPA
│   └── src/
│       ├── components/ # UI components
│       ├── pages/      # Route pages
│       ├── services/   # API layer
│       └── context/    # React context
├── backend/
│   ├── VendorMdm.Api/          # REST API
│   ├── VendorMdm.Artifacts/    # Azure Functions
│   ├── VendorMdm.Shared/       # Shared models
│   └── VendorMdm.Core.Framework/ # Core lib
├── infrastructure/     # Bicep IaC
├── specs/              # Specifications
└── docs/               # Documentation
```

---

## Key Patterns

| Pattern | Implementation |
|---------|----------------|
| Hexagonal | Domain ↔ Ports ↔ Adapters |
| Result | No exceptions for business failures |
| State Machine | Defined transitions |
| Repository | DB abstraction |
| Event-Driven | Outbox + Service Bus |
