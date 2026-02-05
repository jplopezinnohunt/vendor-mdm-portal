# Process: Event/Meeting Invitation

**Trigger**: Vendor met at trade show, conference, or meeting
**Actors**: Approver, Vendor
**Result**: New Vendor Application with event context

---

## Flow Diagram

```
┌──────────────────┐
│  EXTERNAL EVENT  │
│  (Trade show,    │
│   Conference,    │
│   Meeting)       │
└────────┬─────────┘
         │
         │ Collect vendor info
         ▼
┌─────────────────┐
│    APPROVER     │
│  1. Create Invitation
│     - Vendor Name & Email
│     - Event reference (metadata)
│     - Campaign/Source tracking
└────────┬────────┘
         │
         ▼
┌─────────────────────────────────────┐
│  SYSTEM                              │
│  - Generate token                    │
│  - Store with event metadata:        │
│    {                                 │
│      "source": "trade-show",         │
│      "campaignId": "Q1-2026",        │
│      "eventName": "TechExpo Berlin"  │
│    }                                 │
│  - Queue personalized email          │
└────────┬────────────────────────────┘
         │
         │ Email with event context
         ▼
┌─────────────────┐
│    VENDOR       │
│  2. Receive personalized email       │
│     "We met at TechExpo..."          │
│  3. Click link                       │
│  4. Complete registration            │
└────────┬────────┘
         │
         ▼
    [Standard Invitation Flow]
    (Same as Direct Invitation)
```

---

## Difference from Direct Invitation

| Aspect | Direct | Event-Based |
|--------|--------|-------------|
| Context | Known vendor | Met at event |
| Metadata | Minimal | Event, campaign, source |
| Email | Generic | Personalized with event |
| Tracking | Basic | Campaign attribution |
| Urgency | Normal | Often time-sensitive |

---

## Event Metadata Schema

```json
{
  "notes": "Met at TechExpo Berlin booth #42",
  "metadata": {
    "source": "trade-show",
    "campaignId": "Q1-2026-EMEA",
    "eventName": "TechExpo Berlin 2026",
    "tags": {
      "region": "EMEA",
      "priority": "high",
      "followUpBy": "2026-02-15"
    }
  }
}
```

---

## Bulk Event Invitations (Future)

```
Upload CSV from event
        │
        ▼
   Validate data
        │
        ▼
   Create batch invitations
        │
        ▼
   Queue all emails
        │
        ▼
   Track campaign metrics
```

**Status**: 📋 Planned (see Backlog)

---

## Referenced From

- [functional/APPROVER.md](../functional/APPROVER.md) - "Create Invitation"
- [solution/FLOWS.md](../solution/FLOWS.md) - "Invitation Flow"
