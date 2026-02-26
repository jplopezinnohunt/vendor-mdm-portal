# Specifications Index

**Location**: `.agent/rules/specs/`

---

## Folder Structure

| Folder | Purpose | Lifecycle |
|--------|---------|-----------|
| **solution/** | Current system state | Permanent, updated on changes |
| **functional/** | Role-based user flows (WHO) | Permanent, per-role |
| **processes/** | Reusable business flows (HOW) | Permanent, cross-role |
| **features/** | In-progress feature work | Temporal → merges to solution |

---

## Solution Specs (What IS)

Current state of the system.

- [INDEX.md](solution/INDEX.md) - Overview & status
- [CORE.md](solution/CORE.md) - Architecture, entities
- [FLOWS.md](solution/FLOWS.md) - State machines
- [INTEGRATIONS.md](solution/INTEGRATIONS.md) - External systems
- [ENTITY-PROCESS-MAP.md](solution/ENTITY-PROCESS-MAP.md) - Entity-process relationships

---

## Functional Flows (Per Role)

WHO does what - role-based capabilities.

- [ADMIN.md](functional/ADMIN.md) - System administration
- [APPROVER.md](functional/APPROVER.md) - Vendor Unit / approval tasks
- [REQUESTER.md](functional/REQUESTER.md) - Change request submissions
- [VENDOR.md](functional/VENDOR.md) - External vendor actions
- [MD-TEAM.md](functional/MD-TEAM.md) - Master Data specialists

---

## Business Processes (Cross-Role)

HOW things happen - reusable subflows referenced by roles.

- [INDEX.md](processes/INDEX.md) - Process catalog & matrix

**Onboarding Processes**:
- [direct-invitation.md](processes/direct-invitation.md) - Approver invites known vendor
- [event-invitation.md](processes/event-invitation.md) - Invitation from meeting/event
- [md-team-creation.md](processes/md-team-creation.md) - Internal vendor creation

**Modification Processes**:
- [vendor-self-modification.md](processes/vendor-self-modification.md) - Vendor updates own data
- [md-team-modification.md](processes/md-team-modification.md) - Internal team updates vendor

---

## Features (In Progress)

Active feature branches. **Temporal** - after completion, content moves to Solution Specs.

| Feature | Branch | Status |
|---------|--------|--------|
| *None active* | - | - |

**Workflow**:
1. Create feature branch: `feature/topic-name`
2. Create spec: `features/topic-name.md`
3. Implement per spec
4. After merge: Update Solution Specs, archive feature spec

---

## Quick Reference

```
Need to understand current system? → solution/
Need to understand user workflow?  → functional/
Need to understand a process?      → processes/
Building a new feature?            → features/ + feature branch
```

---

## Relationship Model

```
┌─────────────────────────────────────────────────────────┐
│                    SPECIFICATIONS                        │
│                                                          │
│   functional/          processes/          solution/     │
│   ───────────          ──────────          ─────────     │
│   WHO does what        HOW it happens      WHAT exists   │
│                                                          │
│   ADMIN.md ─────────►  direct-invitation   CORE.md      │
│   APPROVER.md ──────►  event-invitation    FLOWS.md     │
│   REQUESTER.md ─────►  md-team-mod         INTEGRATIONS │
│   VENDOR.md ────────►  vendor-self-mod     ENTITY-MAP   │
│   MD-TEAM.md ───────►  md-team-creation                 │
│                                                          │
│        │                    │                 ▲          │
│        └────────────────────┴─────────────────┘          │
│                    references                            │
└─────────────────────────────────────────────────────────┘
```
