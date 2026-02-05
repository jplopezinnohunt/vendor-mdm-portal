# Solution Specification Index

**Version**: 1.0.0 | **Last Updated**: 2026-02-05

> **Purpose**: Define CURRENT state of the solution. Update when system changes.

---

## Quick Reference

| Topic | Document | Focus |
|-------|----------|-------|
| **Core** | [CORE.md](CORE.md) | Architecture, entities, tech stack |
| **Flows** | [FLOWS.md](FLOWS.md) | Workflows, state machines |
| **Integrations** | [INTEGRATIONS.md](INTEGRATIONS.md) | External systems, APIs |
| **Entity-Process Map** | [ENTITY-PROCESS-MAP.md](ENTITY-PROCESS-MAP.md) | How entities connect to processes |

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
[Frontend]     →  [API]      →  [Domain]     →  [Storage]
React/TS          ASP.NET       Hexagonal       SQL + Cosmos
                  Core 8                        + Service Bus
```

**Patterns**: Hexagonal | Event-Driven | Hybrid Database | Result Pattern

---

## Status Dashboard

| Component | Status | Notes |
|-----------|--------|-------|
| Frontend | ✅ Active | React 19 + Vite |
| Backend API | ✅ Active | .NET 8 |
| SQL Database | ✅ Active | Structured data |
| Cosmos DB | ✅ Active | Artifacts + Events |
| Service Bus | ⚠️ Partial | Queue exists, workers pending |
| Azure Functions | ⚠️ Partial | Email only |
| Azure AD Auth | ❌ Disabled | Re-enable required |

---

## Living Document Rules

1. **Update on Change**: When system changes, update relevant spec
2. **Feature Specs First**: New features get their own `specs/spec_*.md`
3. **Solution Spec Second**: After implementation, update Solution Spec
4. **Keep Concise**: Max 100 lines per module

---

## Related Documents

- **Brain**: [moderngoldenrules.md](../../moderngoldenrules.md)
- **Backlog**: [docs/BACKLOG.md](../../../../docs/BACKLOG.md)
- **Standards**: [standards/](../../standards/)
- **Feature Specs**: [specs/](../../../../specs/) (root level - what to build)
