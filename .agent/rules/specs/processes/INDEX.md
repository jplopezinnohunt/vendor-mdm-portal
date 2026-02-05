# Business Processes Index

**Purpose**: Reusable business flows referenced by multiple roles.

---

## Process Catalog

### Vendor Onboarding Processes

| Process | Trigger | Primary Actor | Document |
|---------|---------|---------------|----------|
| Direct Invitation | Approver knows vendor | Approver → Vendor | [direct-invitation.md](direct-invitation.md) |
| Event Invitation | Meeting/Trade show | Approver → Vendor | [event-invitation.md](event-invitation.md) |
| MD Team Creation | Internal need | MD Team | [md-team-creation.md](md-team-creation.md) |

### Vendor Modification Processes

| Process | Trigger | Primary Actor | Document |
|---------|---------|---------------|----------|
| Vendor Self-Modification | Vendor request | Vendor | [vendor-self-modification.md](vendor-self-modification.md) |
| MD Team Modification | Internal request | MD Team / Requester | [md-team-modification.md](md-team-modification.md) |

---

## Process vs Functional

| Concept | Focus | Example |
|---------|-------|---------|
| **Process** | HOW things happen | "Direct Invitation Flow" |
| **Functional** | WHO does what | "Approver can create invitations" |

Processes are **referenced** from Functional docs, not duplicated.

---

## Role-Process Matrix

| Process | Admin | Approver | Requester | Vendor | MD Team |
|---------|-------|----------|-----------|--------|---------|
| Direct Invitation | Oversight | Initiator | - | Recipient | - |
| Event Invitation | Oversight | Initiator | - | Recipient | - |
| MD Team Creation | Oversight | Approver | - | - | Initiator |
| Vendor Self-Modification | - | Approver | - | Initiator | - |
| MD Team Modification | Oversight | Approver | Initiator | - | Initiator |

**Legend**: Initiator = starts process | Approver = reviews/approves | Recipient = receives | Oversight = monitor/cancel

---

## Process States Overview

```
┌─────────────────────────────────────────────────────────┐
│                VENDOR LIFECYCLE                          │
│                                                          │
│  ┌──────────────┐    ┌──────────────┐                   │
│  │   ONBOARD    │    │   MODIFY     │                   │
│  ├──────────────┤    ├──────────────┤                   │
│  │ • Direct Inv │    │ • Self-Mod   │                   │
│  │ • Event Inv  │    │ • MD-Mod     │                   │
│  │ • MD Create  │    │              │                   │
│  └──────┬───────┘    └──────┬───────┘                   │
│         │                   │                            │
│         ▼                   ▼                            │
│     [ACTIVE VENDOR] ◄───────┘                           │
│                                                          │
└─────────────────────────────────────────────────────────┘
```
