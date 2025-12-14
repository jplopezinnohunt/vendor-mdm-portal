# Vendor MDM Portal - Architecture & Schema Documentation

## Table of Contents
1. [System Architecture Overview](#system-architecture-overview)
2. [Frontend-API Communication](#frontend-api-communication)
3. [Managed Identity Authentication](#managed-identity-authentication)
4. [API-Database Integration](#api-database-integration)
5. [SQL Database Schema](#sql-database-schema)
6. [Cosmos DB Schema](#cosmos-db-schema)
7. [Hybrid Database Model](#hybrid-database-model)

---

## 1. System Architecture Overview

### High-Level Component Diagram

```mermaid
graph TB
    subgraph "Azure Cloud"
        subgraph "Frontend - Static Web App"
            SWA[Static Web App<br/>React + TypeScript]
        end
        
        subgraph "Backend - App Service"
            API[ASP.NET Core API<br/>Port 443/HTTPS]
            MI[Managed Identity<br/>System-Assigned]
        end
        
        subgraph "Data Layer"
            SQL[(Azure SQL Database<br/>Operational Data)]
            COSMOS[(Cosmos DB<br/>Audit Trail)]
            SB[Service Bus<br/>Event Streaming]
        end
        
        subgraph "Security"
            AAD[Azure AD<br/>Authentication]
            KV[Key Vault<br/>Secrets]
        end
    end
    
    User([User Browser]) -->|HTTPS| SWA
    SWA -->|HTTPS/JSON| API
    API -->|Managed Identity| SQL
    API -->|Managed Identity| COSMOS
    API -->|Connection String| SB
    API -.->|Optional| KV
    AAD -.->|Auth Tokens| SWA
    AAD -.->|Auth Tokens| API
    MI -->|RBAC| SQL
    MI -->|RBAC| COSMOS
    
    style SWA fill:#0078d4,color:#fff
    style API fill:#68217a,color:#fff
    style SQL fill:#e81123,color:#fff
    style COSMOS fill:#00bcf2,color:#fff
    style SB fill:#59b4d9,color:#fff
    style MI fill:#ffb900,color:#000
```

### Resource Topology

| Component | Azure Service | SKU | Purpose |
|-----------|--------------|-----|---------|
| **Frontend** | Static Web App | Free | React UI hosting |
| **Backend API** | App Service | F1 (Free) | REST API (.NET 8) |
| **Operational DB** | Azure SQL | Basic (5 DTU) | Transactional data |
| **Audit Store** | Cosmos DB | Serverless | Event sourcing & audit |
| **Message Queue** | Service Bus | Basic | Async messaging |
| **Secrets** | Key Vault | Standard | Connection strings |

---

## 2. Frontend-API Communication

### Request/Response Flow

```mermaid
sequenceDiagram
    participant User
    participant Browser
    participant SWA as Static Web App<br/>(Frontend)
    participant API as App Service<br/>(Backend API)
    participant SQL as Azure SQL
    participant Cosmos as Cosmos DB
    participant SB as Service Bus
    
    User->>Browser: Navigate to /admin/invitations
    Browser->>SWA: Load React App
    SWA-->>Browser: HTML + JavaScript
    
    Note over Browser,SWA: User fills invitation form
    
    Browser->>SWA: Click "Create Invitation"
    SWA->>API: POST /api/invitation<br/>{vendor data}
    
    activate API
    API->>API: Validate request
    API->>API: Generate invitation token
    
    par Database Operations
        API->>SQL: INSERT INTO VendorInvitations
        SQL-->>API: Success (Record ID)
    and Audit Trail
        API->>Cosmos: Save InvitationArtifact
        Cosmos-->>API: Success (Document ID)
    and Event Publishing  
        API->>SB: Publish InvitationCreated event
        SB-->>API: Success
    end
    
    API-->>SWA: 201 Created<br/>{invitation details}
    deactivate API
    
    SWA->>API: GET /api/invitation/list
    activate API
    API->>SQL: SELECT * FROM VendorInvitations
    SQL-->>API: Result set
    API-->>SWA: 200 OK<br/>{invitations: [...]}
    deactivate API
    
    SWA-->>Browser: Update UI with invitation list
    Browser-->>User: Display invitations
```

### API Endpoints

#### Invitation Endpoints
```
POST   /api/invitation              Create new invitation
GET    /api/invitation/list         List all invitations
GET    /api/invitation/{token}      Get invitation by token
POST   /api/invitation/validate     Validate invitation token
POST   /api/invitation/resend       Resend invitation email
```

#### Request/Response Examples

**Create Invitation Request:**
```json
POST /api/invitation
Content-Type: application/json

{
  "vendorLegalName": "Acme Corporation",
  "primaryContactEmail": "contact@acme.com",
  "invitedBy": "admin@company.com",
  "notes": "Preferred supplier"
}
```

**Create Invitation Response:**
```json
HTTP/1.1 201 Created
Content-Type: application/json

{
  "id": "66df29ee-bcdd-4daa-b3cc-3e4c267ef1bb",
  "invitationToken": "abc123xyz789...",
  "vendorLegalName": "Acme Corporation",
  "primaryContactEmail": "contact@acme.com",
  "status": "Pending",
  "createdAt": "2025-12-13T06:00:47Z",
  "expiresAt": "2025-12-27T06:00:47Z",
  "invitationLink": "https://portal.company.com/register?token=abc123..."
}
```

### CORS Configuration

```mermaid
graph LR
    subgraph "Allowed Origins"
        LOCAL1[localhost:5173<br/>Vite Dev]
        LOCAL2[localhost:3000<br/>Alternative]
        AZURE[*.azurestaticapps.net<br/>Production]
    end
    
    subgraph "API Middleware"
        CORS[CORS Policy<br/>AllowFrontend]
    end
    
    subgraph "Allowed Methods"
        GET[GET]
        POST[POST]
        PUT[PUT]
        DELETE[DELETE]
    end
    
    LOCAL1 --> CORS
    LOCAL2 --> CORS
    AZURE --> CORS
    
    CORS --> GET
    CORS --> POST
    CORS --> PUT
    CORS --> DELETE
    
    style CORS fill:#ff6b6b,color:#fff
```

---

## 3. Managed Identity Authentication

### Authentication Architecture

```mermaid
graph TB
    subgraph "App Service"
        API[Backend API<br/.NET 8/]
        MI[System-Assigned<br/>Managed Identity]
    end
    
    subgraph "Azure AD"
        AAD[Azure Active Directory]
        TOKENS[OAuth 2.0 Tokens]
    end
    
    subgraph "Azure Resources"
        SQL[(SQL Database)]
        COSMOS[(Cosmos DB)]
        KV[Key Vault]
    end
    
    subgraph "RBAC Permissions"
        SQL_ROLES[db_datareader<br/>db_datawriter<br/>db_ddladmin]
        COSMOS_ROLES[Cosmos DB Reader<br/>Document Contributor]
        KV_ROLES[Key Vault Secrets<br/>Get, List]
    end
    
    API -->|Has| MI
    MI -->|Registers with| AAD
    AAD -->|Issues| TOKENS
    
    MI -->|Requests Access| SQL
    MI -->|Requests Access| COSMOS
    MI -->|Requests Access| KV
    
    SQL_ROLES -.->|Grants| SQL
    COSMOS_ROLES -.->|Grants| COSMOS
    KV_ROLES -.->|Grants| KV
    
    TOKENS -.->|Authorizes| SQL
    TOKENS -.->|Authorizes| COSMOS
    TOKENS -.->|Authorizes| KV
    
    style MI fill:#ffb900,color:#000
    style AAD fill:#0078d4,color:#fff
    style TOKENS fill:#00bcf2,color:#fff
```

### Authentication Flow Sequence

```mermaid
sequenceDiagram
    participant API as App Service API
    participant MI as Managed Identity
    participant AAD as Azure AD
    participant SQL as SQL Database
    participant Cosmos as Cosmos DB
    
    Note over API,SQL: Application Startup
    
    API->>MI: Get Managed Identity
    MI->>AAD: Request Access Token<br/>(Scope: https://database.windows.net)
    AAD->>AAD: Validate MI Principal
    AAD-->>MI: Access Token (JWT)
    MI-->>API: Token for SQL
    
    Note over API,SQL: SQL Database Connection
    
    API->>SQL: Connect with Access Token<br/>(Active Directory Auth)
    SQL->>SQL: Validate token signature
    SQL->>SQL: Check RBAC permissions
    SQL-->>API: Connection Established
    
    Note over API,Cosmos: Cosmos DB Connection
    
    API->>MI: Get Managed Identity
    MI->>AAD: Request Access Token<br/>(Scope: https://cosmos.azure.com)
    AAD-->>MI: Access Token
    MI-->>API: Token for Cosmos
    
    API->>Cosmos: Connect with Managed Identity
    Cosmos->>Cosmos: Validate MI permissions
    Cosmos-->>API: Connection Established
    
    Note over API,Cosmos: Application Ready
```

### Connection Strings (No Secrets!)

**SQL Database Connection:**
```
Server=tcp:sql-vendor-mdm-dev.database.windows.net,1433;
Initial Catalog=VendorMdmDb;
Authentication=Active Directory Managed Identity;
Encrypt=True;
TrustServerCertificate=False;
```
✅ **No username or password** - uses Managed Identity

**Cosmos DB Connection:**
```
https://cosmos-vendor-mdm-dev.documents.azure.com:443/
```
✅ **No access key** - uses Managed Identity via DefaultAzureCredential

**Service Bus Connection:**
```
Endpoint=sb://sb-vendor-mdm-dev.servicebus.windows.net/;
SharedAccessKeyName=RootManageSharedAccessKey;
SharedAccessKey=[from Key Vault or App Settings]
```

---

## 4. API-Database Integration

### Data Access Pattern

```mermaid
graph TB
    subgraph "API Layer"
        CTRL[Controllers<br/>InvitationController]
        SVC[Services<br/>InvitationService]
        REPO[Repositories<br/>ChangeRequestRepository]
    end
    
    subgraph "Data Access"
        EF[Entity Framework Core<br/>DbContext]
        COSMOS_CLIENT[Cosmos Client<br/>CosmosRepository]
    end
    
    subgraph "Database Layer"
        SQL[(SQL Database<br/>Operational)]
        COSMOS[(Cosmos DB<br/>Audit/Events)]
    end
    
    CTRL -->|Uses| SVC
    SVC -->|Uses| REPO
    SVC -->|Direct Access| EF
    SVC -->|Direct Access| COSMOS_CLIENT
    
    EF -->|LINQ Queries| SQL
    COSMOS_CLIENT -->|Document Queries| COSMOS
    
    style CTRL fill:#68217a,color:#fff
    style SVC fill:#68217a,color:#fff
    style EF fill:#512bd4,color:#fff
    style COSMOS_CLIENT fill:#00bcf2,color:#fff
```

### Entity Framework Configuration

**DbContext Setup:**
```csharp
public class SqlDbContext : DbContext
{
    public DbSet<VendorInvitation> VendorInvitations { get; set; }
    public DbSet<ChangeRequest> ChangeRequests { get; set; }
    public DbSet<VendorApplication> VendorApplications { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<WorkflowState> WorkflowStates { get; set; }
    public DbSet<SapEnvironment> SapEnvironments { get; set; }
    public DbSet<UserRole> UsersAndRoles { get; set; }
}
```

**Dependency Injection:**
```csharp
// SQL Database with EF Core
builder.Services.AddDbContext<SqlDbContext>(options =>
{
    if (sqlConnection.Contains("Data Source="))
        options.UseSqlite(sqlConnection);  // Local dev
    else
        options.UseSqlServer(sqlConnection);  // Azure
});

// Cosmos DB Client
builder.Services.AddSingleton<CosmosClient>(sp =>
{
    return new CosmosClient(
        cosmosConnection, 
        new DefaultAzureCredential()  // Managed Identity
    );
});
```

---

## 5. SQL Database Schema

### Entity Relationship Diagram

```mermaid
erDiagram
    VendorInvitations ||--o| VendorApplications : "completes to"
    VendorApplications ||--o{ ChangeRequests : "has"
    VendorApplications ||--o{ Attachments : "has"
    ChangeRequests ||--|| WorkflowStates : "current state"
    VendorApplications ||--|| WorkflowStates : "current state"
    VendorApplications ||--|| SapEnvironments : "targets"
    UsersAndRoles ||--o{ VendorInvitations : "creates"
    UsersAndRoles ||--o{ ChangeRequests : "approves"
    
    VendorInvitations {
        uuid Id PK
        string InvitationToken UK
        string VendorLegalName
        string PrimaryContactEmail
        string InvitedBy
        string InvitedByName
        datetime CreatedAt
        datetime ExpiresAt
        string Status
        datetime CompletedAt
        uuid VendorApplicationId FK
        string Notes
    }
    
    VendorApplications {
        uuid Id PK
        string CompanyLegalName
        string TaxId
        string ContactEmail
        string ContactPhone
        string Country
        string City
        string StateName
        datetime SubmittedAt
        string CurrentState FK
        string TargetSapEnvironment FK
    }
    
    ChangeRequests {
        uuid Id PK
        uuid VendorApplicationId FK
        string FieldName
        string OldValue
        string NewValue
        string CurrentState FK
        datetime RequestedAt
    }
    
    Attachments {
        uuid Id PK
        uuid VendorApplicationId FK
        string FileName
        string BlobUrl
        datetime UploadedAt
    }
    
    WorkflowStates {
        string StateName PK
        string Description
    }
    
    SapEnvironments {
        string EnvironmentCode PK
        string Description
    }
    
    UsersAndRoles {
        string UserId PK
        string Email
        string Role
    }
```

### Table Details

#### VendorInvitations Table
**Purpose:** Track invitation tokens sent to potential vendors

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | uniqueidentifier | PK | Unique invitation ID |
| InvitationToken | nvarchar(256) | UNIQUE, NOT NULL | Secure random token |
| VendorLegalName | nvarchar(500) | NOT NULL | Company name |
| PrimaryContactEmail | nvarchar(320) | NOT NULL | Contact email |
| InvitedBy | nvarchar(256) | | User who created invitation |
| InvitedByName | nvarchar(256) | | Display name of inviter |
| CreatedAt | datetime2 | NOT NULL | When invitation was created |
| ExpiresAt | datetime2 | NOT NULL | Expiration date (14 days) |
| Status | nvarchar(50) | NOT NULL | Pending/Completed/Expired |
| CompletedAt | datetime2 | NULL | When vendor completed registration |
| VendorApplicationId | uniqueidentifier | FK, NULL | Link to completed application |
| Notes | nvarchar(max) | NULL | Internal notes |

**Indexes:**
- PRIMARY KEY on `Id`
- UNIQUE INDEX on `InvitationToken`
- INDEX on `Status, CreatedAt`

#### VendorApplications Table
**Purpose:** Store vendor master data submissions

| Column | Type | Description |
|--------|------|-------------|
| Id | uniqueidentifier | Primary key |
| CompanyLegalName | nvarchar(500) | Legal entity name |
| TaxId | nvarchar(50) | Tax identification number |
| ContactEmail | nvarchar(320) | Primary contact email |
| ContactPhone | nvarchar(50) | Phone number |
| Country | nvarchar(100) | Country of operation |
| City | nvarchar(100) | City |
| StateName | nvarchar(100) | State/Province |
| SubmittedAt | datetime2 | Submission timestamp |
| CurrentState | nvarchar(50) | FK to WorkflowStates |
| TargetSapEnvironment | nvarchar(10) | FK to SapEnvironments |

#### WorkflowStates (Seed Data)
| StateName | Description |
|-----------|-------------|
| Draft | Initial draft |
| Submitted | Submitted for approval |
| Approved | Approved by admin |
| Integrated | Synced to SAP |

#### SapEnvironments (Seed Data)
| EnvironmentCode | Description |
|-----------------|-------------|
| D01 | Development |
| Q01 | Quality Assurance |
| P01 | Production |

---

## 6. Cosmos DB Schema

### Container Structure

```mermaid
graph TB
    subgraph "Cosmos DB Account"
        DB{VendorMdm Database}
    end
    
    subgraph "Containers"
        ARTIFACTS[InvitationArtifacts<br/>Partition: /InvitationId]
        EVENTS[DomainEvents<br/>Partition: /EventType]
    end
    
    subgraph "Document Types"
        INV_DOC[Invitation Artifact]
        EVENT_DOC[Domain Event]
    end
    
    DB --> ARTIFACTS
    DB --> EVENTS
    
    ARTIFACTS -.->|Stores| INV_DOC
    EVENTS -.->|Stores| EVENT_DOC
    
    style DB fill:#00bcf2,color:#fff
    style ARTIFACTS fill:#0078d4,color:#fff
    style EVENTS fill:#0078d4,color:#fff
```

### InvitationArtifacts Container

**Purpose:** Complete immutable audit trail of every invitation

**Document Schema:**
```json
{
  "id": "66df29ee-bcdd-4daa-b3cc-3e4c267ef1bb",
  "InvitationId": "66df29ee-bcdd-4daa-b3cc-3e4c267ef1bb",
  "Type": "InvitationArtifact",
  "VendorLegalName": "Acme Corporation",
  "PrimaryContactEmail": "contact@acme.com",
  "InvitationToken": "abc123xyz789...",
  "InvitedBy": "admin@company.com",
  "InvitedByName": "John Admin",
  "CreatedAt": "2025-12-13T06:00:47.564946Z",
  "ExpiresAt": "2025-12-27T06:00:47.564324Z",
  "Status": "Pending",
  "Notes": "Preferred supplier",
  "EmailSent": true,
  "EmailSentAt": "2025-12-13T06:00:48.123456Z",
  "InvitationLink": "https://portal.company.com/register?token=abc123...",
  "_ts": 1702450847
}
```

**Partition Key:** `/InvitationId`  
**TTL:** None (permanent audit record)

### DomainEvents Container

**Purpose:** Event sourcing for all domain events

**Document Schema:**
```json
{
  "id": "event-uuid-here",
  "EventType": "InvitationCreated",
  "EventVersion": "1.0",
  "AggregateId": "66df29ee-bcdd-4daa-b3cc-3e4c267ef1bb",
  "AggregateType": "VendorInvitation",
  "EventData": {
    "vendorLegalName": "Acme Corporation",
    "contactEmail": "contact@acme.com",
    "invitedBy": "admin@company.com"
  },
  "Metadata": {
    "userId": "admin@company.com",
    "ipAddress": "94.204.171.89",
    "userAgent": "Mozilla/5.0..."
  },
  "Timestamp": "2025-12-13T06:00:47.564946Z",
  "_ts": 1702450847
}
```

**Partition Key:** `/EventType`  
**TTL:** Configurable (e.g., 90 days for non-critical events)

### Query Patterns

**Get Invitation Artifact:**
```sql
SELECT * FROM c 
WHERE c.InvitationId = '66df29ee-bcdd-4daa-b3cc-3e4c267ef1bb'
```

**Get All Events for an Aggregate:**
```sql
SELECT * FROM c 
WHERE c.EventType = 'InvitationCreated'
ORDER BY c.Timestamp DESC
```

---

## 7. Hybrid Database Model

### SQL vs Cosmos DB - Data Distribution

```mermaid
graph TB
    subgraph "Application Layer"
        APP[ASP.NET Core API]
    end
    
    subgraph "Operational Data - SQL"
        SQL[(Azure SQL Database)]
        SQL_DATA["`**Current State**
        - Active invitations
        - Vendor applications
        - Change requests
        - Attachments metadata
        
        **Purpose:**
        - ACID transactions
        - Relational queries
        - Business logic
        - SAP integration source`"]
    end
    
    subgraph "Audit Trail - Cosmos DB"
        COSMOS[(Cosmos DB)]  
        COSMOS_DATA["`**Historical Record**
        - Invitation artifacts
        - Domain events
        - Event sourcing
        - Complete audit log
        
        **Purpose:**
        - Immutable history
        - Event replay
        - Compliance
        - Analytics`"]
    end
    
    APP -->|Write/Read| SQL
    APP -->|Write Only| COSMOS
    SQL -.->|Point-in-time| COSMOS
    
    SQL --> SQL_DATA
    COSMOS --> COSMOS_DATA
    
    style SQL fill:#e81123,color:#fff
    style COSMOS fill:#00bcf2,color:#fff
    style APP fill:#68217a,color:#fff
```

### Data Flow: Creating an Invitation

```mermaid
sequenceDiagram
    participant User
    participant API
    participant SQL
    participant Cosmos
    participant ServiceBus
    
    User->>API: POST /api/invitation
    
    rect rgb(200, 220, 240)
        Note over API: Transaction Boundary
        
        API->>API: Generate invitation token
        API->>API: Create VendorInvitation entity
        
        API->>SQL: BEGIN TRANSACTION
        API->>SQL: INSERT VendorInvitations
        SQL-->>API: Row inserted (Id)
        API->>SQL: COMMIT TRANSACTION
    end
    
    rect rgb(220, 240, 200)
        Note over API,Cosmos: Audit Trail (Async)
        
        API->>API: Create InvitationArtifact document
        API->>Cosmos: Save to InvitationArtifacts
        Cosmos-->>API: Document created
        
        API->>API: Create InvitationCreated event
        API->>Cosmos: Save to DomainEvents
        Cosmos-->>API: Event stored
    end
    
    rect rgb(240, 220, 200)
        Note over API,ServiceBus: Event Publishing (Async)
        
        API->>ServiceBus: Publish InvitationCreated
        ServiceBus-->>API: Acknowledged
    end
    
    API-->>User: 201 Created
```

### When to Use SQL vs Cosmos DB

| Data Type | Storage | Reason |
|-----------|---------|--------|
| **Active invitations** | SQL | Need to query by status, email, date |
| **Vendor applications** | SQL | Relational integrity, joins with changes |
| **Change requests** | SQL | Complex queries, approval workflows |
| **Workflow states** | SQL | Lookup tables, foreign keys |
| **Invitation history** | Cosmos | Immutable audit trail |
| **Domain events** | Cosmos | Event sourcing, time-series |
| **Email logs** | Cosmos | Append-only, no updates needed |
| **Analytics snapshots** | Cosmos | Large documents, flexible schema |

### Data Consistency Model

**SQL Database: Strong Consistency**
- ACID transactions
- Immediate consistency
- Used for operational queries

**Cosmos DB: Eventual Consistency**
- Session consistency (default)
- Async writes
- Used for audit and history

**Pattern: Dual Write**
```csharp
public async Task<VendorInvitation> CreateInvitationAsync(CreateInvitationDto dto)
{
    // 1. Write to SQL (transactional, synchronous)
    var invitation = new VendorInvitation { /* ... */ };
    _context.VendorInvitations.Add(invitation);
    await _context.SaveChangesAsync();  // ✅ COMMITTED
    
    // 2. Write to Cosmos (audit, asynchronous, best-effort)
    try 
    {
        await SaveInvitationArtifactAsync(invitation);  // ⏳ Fire and forget
        await EmitDomainEventAsync("InvitationCreated", invitation);
    }
    catch (Exception ex) 
    {
        _logger.LogWarning("Cosmos write failed: {Error}", ex.Message);
        // ⚠️ Don't fail the request - audit is supplementary
    }
    
    return invitation;
}
```

### Database Size Estimates

**SQL Database (Operational):**
- VendorInvitations: ~1KB per row × 10,000 invitations = 10 MB
- VendorApplications: ~2KB per row × 5,000 applications = 10 MB
- ChangeRequests: ~500B per row × 20,000 changes = 10 MB
- **Total: < 100 MB** (fits in Basic tier)

**Cosmos DB (Audit):**
- InvitationArtifacts: ~2KB per document × 10,000 = 20 MB
- DomainEvents: ~1KB per event × 50,000 events = 50 MB
- **Total: ~70 MB** (minimal RU consumption on Serverless)

---

## Summary

This architecture provides:

✅ **Scalability** - Static Web App CDN + App Service auto-scale  
✅ **Security** - Managed Identity (no credentials in code)  
✅ **Performance** - SQL for queries, Cosmos for history  
✅ **Compliance** - Complete immutable audit trail  
✅ **Cost-Effective** - Free/Basic tiers for dev  
✅ **Maintainable** - Clear separation of concerns  

**Key Design Decisions:**
1. **Hybrid DB** - SQL for operational data, Cosmos for audit
2. **Managed Identity** - No secrets in connection strings
3. **Event-Driven** - Service Bus for async processing
4. **Dual Write** - Sync to SQL, async to Cosmos
5. **API-First** - Frontend is decoupled from backend
