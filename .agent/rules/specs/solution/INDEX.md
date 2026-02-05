# Solution Specification Index

**Version**: 2.0.0 | **Last Updated**: 2026-02-05

> **Purpose**: Define CURRENT state of the solution. Update when system changes.

---

## Quick Reference

| Topic | Document | Focus | Stats |
|-------|----------|-------|-------|
| **Core** | [CORE.md](CORE.md) | Architecture, entities, tech stack | 34 entities, 8 patterns |
| **Flows** | [FLOWS.md](FLOWS.md) | Workflows, state machines | 15 flows |
| **Integrations** | [INTEGRATIONS.md](INTEGRATIONS.md) | External systems, APIs | 10 integrations |
| **Entity-Process Map** | [ENTITY-PROCESS-MAP.md](ENTITY-PROCESS-MAP.md) | How entities connect to processes | - |

---

## System Identity

| Attribute | Value |
|-----------|-------|
| **Name** | Vendor MDM Portal |
| **Type** | Cloud-Native SaaS |
| **Platform** | Azure PaaS |
| **Domain** | Vendor Master Data Management |

---

## Architecture Summary

```
[Frontend]     →  [API]           →  [Domain]     →  [Storage]
React 19/TS       ASP.NET Core 8     Hexagonal       SQL + Cosmos
24 routes         22 controllers      34 entities     + Service Bus
                  94+ endpoints       8 patterns      + Blob Storage
```

**Core Patterns**:
- Hexagonal Architecture
- Event-Driven (Outbox + SignalR)
- Canonical Entity Pattern
- Anti-Corruption Layer
- Result Pattern
- Dynamic Workflow Engine

---

## Status Dashboard

| Component | Status | Details |
|-----------|--------|---------|
| Frontend | ✅ Active | React 19 + Vite + TypeScript 5.8 |
| Backend API | ✅ Active | .NET 8 (22 controllers, 94+ endpoints) |
| SQL Database | ✅ Active | 34 entities |
| Cosmos DB | ✅ Active | Artifacts + Events + Reference Data |
| Service Bus | ✅ Active | Outbox pattern |
| Azure Functions | ⚠️ Partial | Email only |
| Azure AD Auth | ⚠️ Configurable | Multi-auth: Local + MagicLink + AzureAD |
| Blob Storage | ✅ Active | Document storage |
| Sanctions | ✅ Active | Screening service |
| GDPR | ✅ Active | 6 rights implemented |

---

## Key Capabilities (from code)

| Capability | Status | Controller |
|------------|--------|------------|
| Vendor Invitations | ✅ | InvitationController |
| Vendor Creation | ✅ | VendorController |
| Change Requests | ✅ | ChangeRequestController |
| User Management | ✅ | UserController, AuthController |
| Sanctions Screening | ✅ | SanctionsController |
| Bank Validation | ✅ | BankController |
| File Storage | ✅ | AttachmentController, FilesController |
| GDPR Compliance | ✅ | GdprController |
| Event Management | ✅ | EventController |
| Audit Logs | ✅ | AuditLogController |

---

## Roles (from code)

| Role | Routes | Capabilities |
|------|--------|--------------|
| Admin | `/admin/*` | Full system access, user management |
| Approver | `/approver/*` | Invitations, approvals, vendor creation |
| VendorUnit | `/approver/*` | Same as Approver |
| BFM | `/approver/*` | Budget/Finance approval |
| Requestor | `/approver/*` | Submit change requests (view only) |
| Vendor | `/`, `/profile`, `/requests/*` | Self-service, change requests |

---

## Living Document Rules

1. **Update on Change**: When system changes, update relevant spec
2. **Code is Truth**: Specs must reflect actual implementation
3. **Feature Specs First**: New features get their own `specs/spec_*.md`
4. **Solution Spec Second**: After implementation, update Solution Spec

---

## Related Documents

- **Brain**: [moderngoldenrules.md](../../moderngoldenrules.md)
- **Backlog**: [docs/BACKLOG.md](../../../../docs/BACKLOG.md)
- **Standards**: [standards/](../../standards/)
- **Feature Specs**: [specs/](../../../../specs/) (root level - what to build)
- **Functional Flows**: [functional/](../functional/) (per-role capabilities)
- **Processes**: [processes/](../processes/) (business processes)
