# Entity-Process-Role Map

**Purpose**: Show how entities, processes, and roles connect.

---

## Entity → Process Mapping

### VendorInvitation

| Source Process | Context | Same Entity |
|----------------|---------|-------------|
| Direct Invitation | Known vendor | ✅ VendorInvitation |
| Event Invitation | Trade show, meeting | ✅ VendorInvitation |

**Difference**: Metadata (event name, campaign, source tracking)

### VendorApplication

| Source Process | Registration Type | Same Entity |
|----------------|-------------------|-------------|
| Direct Invitation | `Invitation` | ✅ VendorApplication |
| Event Invitation | `Invitation` | ✅ VendorApplication |
| MD Team Creation | `InternalCreation` | ✅ VendorApplication |

**Difference**: `RegistrationType` field

### ChangeRequest

| Source Process | Requester | Same Entity |
|----------------|-----------|-------------|
| Vendor Self-Modification | Vendor | ✅ ChangeRequest |
| MD Team Modification | Internal Staff | ✅ ChangeRequest |

**Difference**: `RequesterId` (vendor vs internal user)

---

## Visual Map

```
┌─────────────────────────────────────────────────────────────────┐
│                     ENTITY-PROCESS-ROLE MAP                      │
│                                                                  │
│  PROCESSES              ENTITIES              ROLES              │
│  ──────────             ────────              ─────              │
│                                                                  │
│  Direct Invitation ───┐                                          │
│                       ├──► VendorInvitation ◄── Approver         │
│  Event Invitation ────┘         │                 │              │
│                                 │                 │              │
│                                 ▼                 │              │
│  (via invitation) ────┐                           │              │
│                       ├──► VendorApplication ◄───┤              │
│  MD Team Creation ────┘         │                 │              │
│                                 │              Vendor            │
│                                 ▼                 │              │
│  Vendor Self-Mod ─────┐                           │              │
│                       ├──► ChangeRequest ◄───────┤              │
│  MD Team Mod ─────────┘         │              Requester         │
│                                 │                                │
│                                 ▼                                │
│                            Attachments ◄── All Roles             │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## Key Insight

**Same entities, different processes** = The data model is unified, but the business context varies:

| Entity | Distinguishing Field | Values |
|--------|---------------------|--------|
| VendorInvitation | `Attributes.metadata.source` | "direct", "trade-show", "meeting" |
| VendorApplication | `RegistrationType` | Invitation, InternalCreation |
| ChangeRequest | `RequesterId` + `RequestType` | Vendor vs Internal |

This means:
- 1 entity schema, multiple business contexts
- Reporting can slice by source/type
- Same approval workflow, different triggers
