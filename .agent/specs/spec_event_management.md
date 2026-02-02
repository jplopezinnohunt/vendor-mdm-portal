---
description: Event Management Specification
---

# Specification: Event Management

## Compliance Sidebar
- **UI Design**: [ui-design-standards.md](file:///Users/jplopez/projects/vendor-mdm-portal/.agent/rules/standards/ui-design-standards.md) - Fluent UI v9
- **Data Model**: [data-model-standards.md](file:///Users/jplopez/projects/vendor-mdm-portal/.agent/rules/standards/data-model-standards.md) - Hybrid Model
- **Architecture**: Ontology Pattern (`EventConcept`)

## Overview
Event Management allows administrators to create and manage events (conferences, meetings) and invite participants.

## Business Rules
1. **Date Validation**: Start Date < End Date (enforced by `EventConcept.ValidateState()`)
2. **Status Lifecycle**: Draft → Published → Completed → Archived
3. **Participant Tiers**: Tier 1 (VIP), Tier 2 (Standard), Tier 3 (Observer)

## Data Model
- **Structured Columns**: `Title`, `StartDate`, `EndDate`, `EventType`, `Status`
- **JSONB Attributes**: `Location`, `Agenda`, `ParticipantList`

## UI Requirements
- Doherty Threshold: Event list loads in <400ms
- Loading states for all async operations
- Fluent UI components (Button, Card, Input)

## API Endpoints
- `POST /api/event/create` - Create event
- `GET /api/event/list` - List events
- `POST /api/event/{id}/invite` - Invite participant
