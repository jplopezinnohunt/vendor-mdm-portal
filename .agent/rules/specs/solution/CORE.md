# Solution Spec: Core

**Focus**: Architecture, Entities, Tech Stack
**Last Updated**: 2026-02-05 | **Entities**: 34 | **Patterns**: 8

---

## Tech Stack

| Layer | Technology | Version |
|-------|------------|---------|
| Frontend | React + TypeScript + Vite | 19 / 5.8 |
| Backend | ASP.NET Core | 8.0 |
| DB (Relational) | Azure SQL Database | - |
| DB (Document) | Azure Cosmos DB Serverless | - |
| Messaging | Azure Service Bus | - |
| Functions | Azure Functions (Isolated) | - |
| Storage | Azure Blob Storage | - |
| Auth | Azure AD + Local + MagicLink | Multi-strategy |
| Hosting | Azure Static Web Apps + App Service | - |

---

## Project Structure

```
vendor-mdm-portal/
├── frontend/                    # React SPA
│   └── src/
│       ├── components/          # UI components
│       ├── pages/               # Route pages (24 routes)
│       ├── services/            # API layer
│       └── context/             # React context (Auth, SignalR)
├── backend/
│   ├── VendorMdm.Api/           # REST API (22 controllers, 94+ endpoints)
│   ├── VendorMdm.Artifacts/     # Azure Functions
│   ├── VendorMdm.Shared/        # Shared models (34 entities)
│   └── VendorMdm.Core.Framework/ # Core library
├── infrastructure/              # Bicep IaC
├── specs/                       # Feature specifications
└── docs/                        # Documentation
```

---

## Entity Architecture (34 Entities)

### Entity Categories Overview

| Category | Count | Storage | Purpose |
|----------|-------|---------|---------|
| SQL Entities | 9 | SQL Server | Operational data |
| Canonical Entities | 10 | SQL Server | Domain models with versioning |
| Workflow Entities | 5 | SQL Server | Dynamic workflow engine |
| Audit/Event Entities | 4 | SQL + Cosmos | Audit trail & events |
| Cosmos Entities | 6 | Cosmos DB | Artifacts & reference data |

---

## Category 1: SQL Entities (9)

### 1.1 VendorInvitation
**Purpose**: Pre-auth vendor onboarding invitations
**File**: `VendorMdm.Shared/Models/SqlEntities.cs`

| Property | Type | Description |
|----------|------|-------------|
| Id | Guid | Primary Key |
| InvitationToken | string(100) | Unique token for invite link |
| VendorLegalName | string(200) | Company name |
| PrimaryContactEmail | string(255) | Contact email |
| InvitedBy | Guid | User ID of inviter |
| InvitedByName | string(200) | Inviter display name |
| ExpiresAt | DateTime | Invitation expiration |
| Status | string(20) | Pending, Draft, Accepted, Expired, Completed, PendingReview, Approved, Rejected, Cancelled |
| EventId | Guid? | Link to Event (for event invitations) |
| Tier | string(20) | Tier_1, Tier_2, Tier_3 |
| VendorType | string(50) | Vendor classification |
| AccountGroup | string(10) | SAP account group |
| SanctionsStatus | string(20) | NotScreened, Screened, Sanctioned |
| SanctionsScore | decimal(5,2) | Risk score |
| ReviewStatus | string(20) | NotRequired, Pending, Approved, Rejected |
| CurrentStage | string(50) | InvitationSent, MfaVerified, InitialInfoCompleted, Enriched |
| Attributes | JSON | Notes, customFields, mfaCode, metadata |

### 1.2 VendorApplication
**Purpose**: Vendor registration submissions

| Property | Type | Description |
|----------|------|-------------|
| Id | Guid | Primary Key |
| CompanyName | string(200) | Required |
| TaxId | string(100) | Tax identifier |
| ContactName | string(200) | Required |
| ContactEmail | string | Required, validated |
| Status | string | Pending (default) |
| RegistrationType | string(20) | SelfRegistration, Invitation, InternalCreation |
| InvitationId | Guid? | Link to VendorInvitation |
| Attributes | JSON | Address, contactInfo, attachments, certifications |

### 1.3 ChangeRequest
**Purpose**: Vendor data modification requests

| Property | Type | Description |
|----------|------|-------------|
| Id | Guid | Primary Key |
| Status | string(20) | Draft, Submitted, Approved, Integrated, Rejected |
| SapVendorId | string? | Null for new vendors |
| RequesterId | Guid | User making request |
| Attributes | JSON | approvalHistory, rejectionReason, changeImpactAssessment |

### 1.4 Attachment
**Purpose**: Document metadata (files in Blob Storage)

| Property | Type | Description |
|----------|------|-------------|
| Id | Guid | Primary Key |
| LinkedEntityId | Guid | FK to ChangeRequest or VendorApplication |
| FileName | string | Original filename |
| BlobUrl | string | Azure Blob Storage URL |
| UploadedAt | DateTime | UTC timestamp |
| Attributes | JSON | fileSizeBytes, mimeType, uploadedByName, virusScanResult |

### 1.5 UserRole (Legacy)
**Purpose**: Simple user/role mapping (being replaced by canonical User)

| Property | Type | Description |
|----------|------|-------------|
| Id | Guid | Primary Key |
| Username | string | User identifier |
| Role | string | Admin, Requestor, Approver (default: User) |
| Attributes | JSON | fullName, email, department, uiPreferences |

### 1.6 Event
**Purpose**: Event/conference management for vendor invitations

| Property | Type | Description |
|----------|------|-------------|
| Id | Guid | Primary Key |
| EventCode | string(50) | Unique code |
| Title | string | Event title (required) |
| EventType | string(20) | Event, Conference |
| StartDate | DateTime | Event start |
| EndDate | DateTime | Event end |
| CreatedBy | string(100) | Creator |
| Attributes | JSON | sector, field_office, location, financial_coding (wbs, io, sap_vendor_id) |

### 1.7 EventParticipant
**Purpose**: Participant tracking with tier system

| Property | Type | Description |
|----------|------|-------------|
| Id | Guid | Primary Key |
| EventId | Guid | FK to Event |
| Email | string(255) | Required |
| FullName | string(255) | Required |
| Tier | string(20) | Tier_1, Tier_2, Tier_3 |
| Status | string(20) | Pending, Invited, Confirmed, SapCreated |
| VendorInviteId | Guid? | Link to VendorInvitation |
| Attributes | JSON | organization, job_title, notes |

### 1.8 WorkflowState
**Purpose**: State machine reference data

| Property | Type | Description |
|----------|------|-------------|
| StateName | string(20) | Primary Key |
| Description | string | State description |
| Attributes | JSON | displayOrder, colorCode, iconName, transitionsAllowed |

### 1.9 SapEnvironment
**Purpose**: SAP environment configuration

| Property | Type | Description |
|----------|------|-------------|
| EnvironmentCode | string(3) | Primary Key (D01, Q01, P01) |
| Description | string | Environment description |

---

## Category 2: Canonical Entities (10)

All inherit from `CanonicalEntityBase` with these common fields:
- `Id` (Guid) - Immutable global UUID
- `EntityVersion` (int) - Optimistic concurrency
- `Status` (string) - Pending, Active, Suspended, Archived
- `SourceSystem` (string) - Portal, SAP, API, Migration, Batch
- `Data` (JSON) - Semi-structured attributes
- `SchemaVersion` (string) - Data schema version
- `IsDeleted` (bool) - Soft delete flag
- `DeletedAt`, `DeletedBy` - Soft delete metadata
- `CreatedAt`, `UpdatedAt` - Timestamps

### 2.1 Vendor (Canonical)
**Purpose**: Canonical vendor master record
**File**: `VendorMdm.Shared/Models/CanonicalEntities.cs`

| Property | Type | Description |
|----------|------|-------------|
| LegalName | string(200) | Company name (required) |
| TaxId | string(100) | Tax identifier |
| PrimaryContactEmail | string(255) | Required |
| TenantId | Guid? | Multi-tenancy support |
| DataResidencyRegion | string(20) | EU, US, APAC, GLOBAL |

### 2.2 User (Canonical)
**Purpose**: Full user with multi-auth support

| Property | Type | Description |
|----------|------|-------------|
| Username | string(100) | Unique identifier |
| Email | string(255) | Required |
| Roles | List | Admin, Requestor, Approver, Viewer, VendorUnit, BFM |
| AzureAdObjectId | string(50) | Azure AD link |
| AuthProvider | string(20) | Local, AzureAd |
| AuthMethod | string | MagicLink, AzureAd, LocalStrong |
| PasswordHash | string? | For local auth |
| TwoFactorEnabled | bool | 2FA status |
| TwoFactorSecret | string(200) | TOTP secret |
| RecoveryCodes | JSON | Backup codes |
| MagicLinkToken | string(100) | Magic link token |
| MagicLinkExpiresAt | DateTime? | Token expiry |
| InvitationToken | string(100) | User invitation |
| IsBlocked | bool | Block status |
| LastLogonAt | DateTime? | Last login |

### 2.3 ExternalSystemMapping
**Purpose**: Anti-corruption layer for SAP/external systems

| Property | Type | Description |
|----------|------|-------------|
| Id | Guid | Primary Key |
| CanonicalEntityId | Guid | Reference to canonical entity |
| EntityType | string(50) | Vendor, Customer, Employee |
| ExternalSystemId | string(100) | SAP LIFNR, Salesforce ID |
| SystemName | string(50) | SAP, Salesforce, SuccessFactors |
| SystemEnvironment | string(50) | D01, Production, Sandbox |

### 2.4 Employee (Canonical)
**Purpose**: Employee master data

| Property | Type | Description |
|----------|------|-------------|
| GivenName | string(100) | First name |
| Surname | string(100) | Last name |
| Email | string(255) | Email |
| EmployeeId | string(50) | Employee number |

### 2.5 Project (Canonical)
**Purpose**: Project master data

| Property | Type | Description |
|----------|------|-------------|
| ProjectCode | string(50) | Unique code |
| Name | string(200) | Project name |
| StartDate | DateTime? | Start |
| EndDate | DateTime? | End |

### 2.6 Fund (Canonical)
**Purpose**: Fund master data

| Property | Type | Description |
|----------|------|-------------|
| FundCode | string(50) | Unique code |
| Name | string(200) | Fund name |
| FiscalYear | string(4) | Fiscal year |

### 2.7 Customer (Canonical)
**Purpose**: Customer master data

| Property | Type | Description |
|----------|------|-------------|
| Name | string(200) | Customer name |
| TaxId | string(100) | Tax ID |
| PrimaryContactEmail | string(255) | Contact |

### 2.8 DocumentRegistry (Canonical)
**Purpose**: Enterprise document management (banking-grade)

| Property | Type | Description |
|----------|------|-------------|
| EntityType | string(50) | Vendor, Employee, Transaction |
| EntityRef | string(100) | Entity UUID reference |
| Category | string(50) | Legal, Identity, Finance, Tax, Banking |
| DocType | string(50) | TradeLicense, Passport, BankCertificate |
| SecurityLevel | int | 1=Public, 2=Internal, 3=Confidential, 4=PII |
| StoragePath | string | Azure Blob path |
| MimeType | string(100) | File MIME type |
| FileSizeBytes | long | File size |
| DocumentStatus | string(20) | Pending, Verified, Rejected, Archived |
| ExpiryDate | DateTime? | Document expiry (indexed) |
| UploadedBy | string(255) | Uploader |

### 2.9 VendorInvitationCanonical
**Purpose**: Canonical invitation with full audit

### 2.10 ChangeRequestCanonical
**Purpose**: Canonical change request with history

---

## Category 3: Workflow Entities (5)

### 3.1 WorkflowDefinition
**Purpose**: Configurable workflow templates

| Property | Type | Description |
|----------|------|-------------|
| Name | string(100) | e.g., "Vendor Onboarding" |
| Domain | string(50) | Vendor, HR, Finance |
| Version | string(20) | Semantic versioning |
| IsActive | bool | Active flag |

### 3.2 WorkflowStep
**Purpose**: Individual workflow steps

| Property | Type | Description |
|----------|------|-------------|
| WorkflowDefinitionId | Guid | Parent workflow |
| StepName | string(100) | e.g., "LegalReview" |
| OrderIndex | int | Step order |
| StepType | string(50) | Task, Approval, Automated, Decision |
| IsFinal | bool | Terminal step flag |

### 3.3 WorkflowAction
**Purpose**: Allowed actions per step

| Property | Type | Description |
|----------|------|-------------|
| WorkflowStepId | Guid | Parent step |
| ActionName | string(50) | e.g., "Approve" |
| TargetStepName | string(100) | Target step |
| ButtonLabel | string(50) | UI label |

### 3.4 WorkflowRoleBinding
**Purpose**: Role permissions per step

### 3.5 WorkflowFieldDefinition
**Purpose**: Dynamic field configuration per step

---

## Category 4: Audit & Event Entities (4)

### 4.1 AuditLog
**Purpose**: Complete audit trail for all entity changes

| Property | Type | Description |
|----------|------|-------------|
| Id | Guid | Primary Key |
| EntityType | string | Vendor, Event, User |
| EntityId | Guid | Entity reference |
| Action | string | Created, Updated, Deleted, Approved, Rejected |
| ChangedBy | string | User email |
| OldValues | JSON | Before snapshot |
| NewValues | JSON | After snapshot |
| Reason | string? | Justification |
| IpAddress | string? | Client IP |
| TenantId | Guid? | Multi-tenancy |

### 4.2 OutboxEvent
**Purpose**: Outbox pattern for guaranteed event delivery

| Property | Type | Description |
|----------|------|-------------|
| Id | Guid | Primary Key |
| EventType | string(200) | VendorCreatedEvent, etc. |
| Payload | JSON | Event data |
| Status | string(50) | Pending, Processing, Completed, Failed, DeadLettered |
| CorrelationId | string(100) | Trace correlation |
| RetryCount | int | Retry attempts |
| NextRetryAt | DateTime? | Next retry time |

### 4.3 EnhancedDomainEvent (Cosmos)
**Purpose**: Enhanced events with correlation tracking

### 4.4 DomainEvent (Cosmos)
**Purpose**: Basic domain events

---

## Category 5: Cosmos Entities (6)

### 5.1 ChangeRequestData
**Purpose**: Change request artifacts (Cosmos)

### 5.2 InvitationArtifact
**Purpose**: Invitation audit trail (Cosmos)

### 5.3 InvitationCompletionArtifact
**Purpose**: Completed registration records (Cosmos)

### 5.4 ReferenceDataItem
**Purpose**: Master data for dropdowns/lookups (Cosmos)

| Property | Type | Description |
|----------|------|-------------|
| Id | string | e.g., "COUNTRY_US" |
| Category | string | Partition key: Country, Currency, VendorType |
| Code | string | e.g., "US", "USD" |
| Description | string | Display text |
| SapCode | string? | SAP mapping |
| IsActive | bool | Active flag |

### 5.5 ValidationRule
**Purpose**: Dynamic validation rules (Cosmos)

### 5.6 RefreshToken
**Purpose**: Authentication token storage

---

## Key Patterns

| Pattern | Implementation | Purpose |
|---------|----------------|---------|
| Hexagonal | Domain ↔ Ports ↔ Adapters | Clean architecture |
| Result | No exceptions for business failures | Explicit error handling |
| State Machine | Defined transitions | Workflow control |
| Repository | DB abstraction | Data access |
| Event-Driven | Outbox + Service Bus + SignalR | Async processing |
| Canonical Entity | CanonicalEntityBase | Versioned domain models |
| Anti-Corruption Layer | ExternalSystemMapping | SAP independence |
| Dynamic Workflow | WorkflowDefinition engine | Configurable processes |

---

## Entity Relationships

```
┌─────────────────────────────────────────────────────────────────────┐
│                        ENTITY RELATIONSHIPS                          │
│                                                                      │
│  Event ──────────────► EventParticipant ──────► VendorInvitation    │
│                              │                        │              │
│                              └── Tier System          │              │
│                                                       ▼              │
│                                              VendorApplication       │
│                                                       │              │
│  User ──────► creates ──────────────────────► ChangeRequest         │
│    │                                                  │              │
│    │                                                  ▼              │
│    └── has ──► Roles                            Attachment          │
│                                                                      │
│  Vendor (Canonical) ◄──────── ExternalSystemMapping ──► SAP         │
│         │                                                            │
│         └──► DocumentRegistry                                        │
│                                                                      │
│  WorkflowDefinition ──► WorkflowStep ──► WorkflowAction             │
│                              │                                       │
│                              └──► WorkflowRoleBinding               │
│                              └──► WorkflowFieldDefinition           │
│                                                                      │
│  All Changes ──► AuditLog + OutboxEvent + DomainEvent               │
└─────────────────────────────────────────────────────────────────────┘
```

---

## Hybrid Data Pattern

**Every entity change triggers**:
1. **SQL**: Metadata + searchable fields + `Attributes` JSON
2. **Cosmos**: Full artifact payload (immutable snapshot)
3. **Cosmos**: Domain event (audit trail with correlation)
4. **Outbox**: Guaranteed delivery for external systems
