# Specification: Event Management Module

**Version:** 1.0
**Status:** DRAFT
**Owner:** Vendor MDM Team
**Standards Compliance:**
- [UI Design Standards](file:///Users/jplopez/projects/vendor-mdm-portal/.agent/rules/standards/ui-design-standards.md)
- [Data Model: Hybrid Relational-Document](file:///Users/jplopez/projects/vendor-mdm-portal/.agent/rules/standards/data-model-standards.md)
- [Hexagonal Architecture](file:///Users/jplopez/projects/vendor-mdm-portal/.agent/rules/standards/hexagonal-architecture-standards.md)

---

## 1. Overview
The Event Management module manages the lifecycle of official UN events, focusing on the recruitment and financial onboarding of "Assistance" personnel (Tier 3). It integrates with the existing Vendor Invitation flow to ensure seamless SAP Vendor creation for payable participants.

## 2. Terminology & Roles
- **Event**: A structured gathering (Conference, Workshop, Mission) with a specific budget and dates.
- **Participant**: An individual associated with an event.
    - **Tier 1 (Official/Delegate)**: No payment required.
    - **Tier 2 (UN Staff)**: Exists in payroll (skipped for vendor creation).
    - **Tier 3 (Assistance)**: Consultants/Support requiring payments. **Must go through Vendor Invitation flow.**

---

## 3. Data Model (Hybrid Approach)

### 3.1 `Events` Table
*Storage: PostgreSQL Relation*

| Column | Type | Nullable | Description | Standard |
| :--- | :--- | :--- | :--- | :--- |
| `Id` | Guid | No | PK | Relational |
| `EventCode` | string(50) | No | Unique Code (e.g., EVT-2024-001) | Relational |
| `Title` | string | No | Event Title | Relational |
| `EventType` | string(20) | No | Enum: `EVENT`, `CONFERENCE` | Relational |
| `StartDate` | DateTime | No | Start | Relational |
| `EndDate` | DateTime | No | End | Relational |
| `CreatedAt` | DateTime | No | Audit | Relational |
| `CreatedBy` | string(100)| No | User Email/ID | Relational |
| `Attributes` | JSONB | No | Semi-structured data | **JSONB** |

**JSONB Schema (`Attributes`):**
```json
{
  "sector": "Climate Change",
  "field_office": "Nairobi",
  "location": "Nairobi HQ",
  "financial_coding": {
    "wbs_element": "S-12345.01",
    "internal_order": "IO-9999",
    "sap_vendor_id": "V-5555"
  }
}
```

### 3.2 `EventParticipants` Table
*Storage: PostgreSQL Relation*

| Column | Type | Nullable | Description | Standard |
| :--- | :--- | :--- | :--- | :--- |
| `Id` | Guid | No | PK | Relational |
| `EventId` | Guid | No | FK to Events | Relational |
| `Email` | string(255)| No | Unique identifier for invite | Relational |
| `FullName` | string(255)| No | Display Name | Relational |
| `Tier` | string(20) | No | `TIER_1`, `TIER_2`, `TIER_3` | Relational |
| `Status` | string(20) | No | `PENDING`, `INVITED`, `CONFIRMED`, `SAP_CREATED` | Relational |
| `VendorInviteId`| Guid | Yes | FK to `VendorInvitations` (Tier 3 only) | Relational |
| `Attributes` | JSONB | No | Metadata | **JSONB** |

**JSONB Schema (`Attributes`):**
```json
{
  "organization": "Green Peace",
  "job_title": "Senior Advisor",
  "notes": "VIP handling"
}
```

---

## 4. API Specification

### 4.1 Event Lifecycle
- `POST /api/events` - Create new event.
- `GET /api/events` - List events (filtered by date, office).
- `GET /api/events/{id}` - Get details + KPI summary.

### 4.2 Participant Management
- `POST /api/events/{id}/participants` - Bulk add participants (JSON payload).
- `POST /api/events/{id}/invite-tier3` - Trigger Vendor Invitation flow for pending Tier 3 participants.
    - *Action*: Creates `VendorInvitation` records for selected participants and links them via `VendorInviteId`.
- `GET /api/events/{id}/participants` - List with status.

### 4.3 Analytics (KPIs)
- `GET /api/events/{id}/kpi`
    - Returns `invitation_rate`, `confirmation_rate`, `sap_conversion_rate`.

---

## 5. User Interface Design

### 5.1 Event Dashboard (New Feature)
- **List View**: Card grid of active events using `AppCard` standard.
- **KPI Banner**: Top of Event Detail page showing color-coded stats.

### 5.2 Create Event Modal
- **Steps**:
    1. **Basic Info**: Title, Code, Type, Dates.
    2. **Context**: Sector, Office, Pillar.
    3. **Finance**: WBS, Internal Order (Validated inputs).

### 5.3 Participant Management Tab
- **Grid**: `DataGrid` showing Name, Tier, Status, SAP Status.
- **Actions**:
    - "Download CSV Template" (New Requirement).
    - "Add Participants" (Manual or CSV upload).
    - "Invite Selected" (Triggers Tier 3 flows).
    - **Participant Actions** (Full Invitation Control):
        - "Review" (Check details before sending).
        - "Send" (Trigger invite).
        - "Resend" (If expired or missed).

---

## 6. Verification Plan
- **Automated**: `scripts/verification/verify_event_management.sh`
    - Creates Event.
    - Adds Tier 3 Participant.
    - Simulates Invitation.
    - Verifies Status update.
- **Manual**: UI Walkthrough of Dashboard and CSV Upload.
