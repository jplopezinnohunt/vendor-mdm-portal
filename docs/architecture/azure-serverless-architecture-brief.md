# Azure Architecture Brief: Vendor MDM Portal

**Date**: 2026-02-25 | **Status**: Validated Against Live Azure Resources | **Environment**: Development (centralus)

---

## 1. Executive Summary

The Vendor Master Data Management (MDM) Portal is a cloud-native application deployed on **Microsoft Azure** following a **Hexagonal (Ports & Adapters) Architecture** with a **Serverless-first** design principle. The system manages vendor onboarding, change requests, approval workflows, sanctions screening, and SAP integration for master data governance.

This brief documents every Azure component **validated against the real deployed infrastructure** in resource group `rg-vendor-mdm-dev`.

---

## 2. Deployed Azure Resources (Verified)

| Resource Name | Azure Service | SKU/Tier | Location | Status |
|---|---|---|---|---|
| `cosmos-vendor-mdm-dev` | Cosmos DB (SQL API) | Serverless | centralus | Online |
| `sql-vendor-mdm-dev` | Azure SQL Server | - | centralus | Online |
| `VendorMdmDb` | Azure SQL Database | Basic (5 DTU) | centralus | Online |
| `sb-vendor-mdm-dev` | Service Bus Namespace | Basic | centralus | Active |
| `kv-vendor-mdm-dev` | Key Vault | Standard | centralus | Active |
| `app-vendor-mdm-api-dev` | App Service (Linux) | F1 Free | centralus | Running |
| `asp-vendor-mdm-dev` | App Service Plan | F1 Free | centralus | Active |
| `swa-vendor-mdm-dev` | Static Web App | Free | centralus | Active |
| `stvendormdmdev` | Storage Account | Standard_LRS | centralus | Active |

**Resource Group**: `rg-vendor-mdm-dev`
**Subscription**: `8c89e199-98bc-4cfd-9ad7-f8e97238f5c6`
**Tenant**: `a93513e2-d327-4301-80ed-d703eb03f6cb`

---

## 3. Architecture Diagram (Logical)

```
                          ┌──────────────────────────────────────────────────────┐
                          │                   AZURE CLOUD                        │
                          │              (rg-vendor-mdm-dev)                     │
                          │                                                      │
  ┌─────────────┐        │   ┌──────────────────┐     ┌─────────────────────┐   │
  │   Browser    │────────┼──>│  Static Web App   │     │    Key Vault        │   │
  │  (React 19)  │        │   │  swa-vendor-mdm   │     │  kv-vendor-mdm-dev  │   │
  │              │        │   │  Free Tier         │     │  10 Secrets         │   │
  └──────┬───────┘        │   └──────────────────┘     └────────┬────────────┘   │
         │ /api/*         │                                      │ Secrets       │
         │                │   ┌──────────────────┐               │               │
         └────────────────┼──>│   App Service     │───────────────┘               │
                          │   │  app-vendor-mdm   │                               │
                          │   │  .NET 8.0 Linux   │                               │
                          │   │  F1 Free Tier     │                               │
                          │   └──┬───┬───┬───┬───┘                               │
                          │      │   │   │   │                                    │
                    ┌─────┼──────┘   │   │   └────────────┐                      │
                    │     │          │   │                 │                      │
              ┌─────▼──┐  │   ┌──────▼───▼──┐    ┌────────▼─────────┐            │
              │SQL DB   │  │   │  Cosmos DB   │    │  Service Bus     │            │
              │Basic    │  │   │  Serverless  │    │  Basic Tier      │            │
              │5 DTU    │  │   │  Session     │    │  1 Queue         │            │
              │2 GB Max │  │   │  2 Containers│    │                  │            │
              └─────────┘  │   └─────────────┘    └────────┬─────────┘            │
                           │                               │                      │
                           │   ┌──────────────────┐        │                      │
                           │   │  Storage Account  │   ┌───▼──────────────┐       │
                           │   │  stvendormdmdev   │   │  Azure Functions  │       │
                           │   │  Standard_LRS     │   │  (Planned)        │       │
                           │   │  1 Container      │   │  Email/Async Jobs │       │
                           │   └──────────────────┘   └──────────────────┘       │
                           │                                                      │
                           └──────────────────────────────────────────────────────┘
```

---

## 4. Component Deep Dive

### 4.1 Frontend — Azure Static Web App

| Property | Value |
|---|---|
| **Resource** | `swa-vendor-mdm-dev` |
| **Hostname** | `thankful-field-0258f8110.3.azurestaticapps.net` |
| **SKU** | Free |
| **Deploy Branch** | `develop` |
| **Framework** | React 19.2 + TypeScript 5.8 + Vite 6.2 |
| **Auth Library** | MSAL Browser 4.27 (Azure AD) |
| **Real-time** | SignalR 10.0 (WebSocket) |

**Why Static Web App**: Zero-server frontend hosting with built-in CI/CD from GitHub, global CDN, custom domains, and free SSL. The SPA routing is configured in `staticwebapp.config.json` with security headers (HSTS, CSP, X-Frame-Options: deny).

**Serverless Fit**: No compute cost for static assets. Scales automatically. API proxy routes `/api/*` calls to the backend App Service.

---

### 4.2 Backend API — Azure App Service

| Property | Value |
|---|---|
| **Resource** | `app-vendor-mdm-api-dev` |
| **Hostname** | `app-vendor-mdm-api-dev.azurewebsites.net` |
| **Runtime** | DOTNETCORE 8.0 (Linux) |
| **SKU** | F1 Free Tier |
| **HTTPS Only** | Yes |
| **DataSourceMode** | Connected (live Azure services) |

**Stack**: ASP.NET Core 8.0 with 22 REST controllers covering vendors, change requests, invitations, workflows, sanctions screening, audit logs, GDPR compliance, and SAP integration.

**Key Patterns**:
- API Versioning (URL + header-based, default v1.0)
- Rate Limiting (5 req/min anonymous)
- Input Sanitization Filter (XSS prevention)
- JWT Bearer + Cookie authentication
- OpenTelemetry distributed tracing
- Swagger/OpenAPI documentation

**Serverless Evolution Path**: The App Service is the current compute host. Per the Hexagonal Serverless architecture rule (ADR-001), new compute should be implemented as **Azure Functions** or **Container Apps**. The App Service can be migrated to Azure Container Apps for serverless container scaling with per-request billing.

---

### 4.3 Relational Data — Azure SQL Database

| Property | Value |
|---|---|
| **Server** | `sql-vendor-mdm-dev` |
| **Database** | `VendorMdmDb` |
| **Edition** | Basic |
| **Service Objective** | Basic (5 DTU) |
| **Max Size** | 2 GB |
| **Status** | Online |

**Purpose**: Structured relational data for entities requiring ACID transactions, referential integrity, and indexed querying.

**Core Tables**:
| Table Category | Tables |
|---|---|
| **Vendor Lifecycle** | VendorApplication, ChangeRequest, VendorInvitation, Attachment |
| **Canonical Entities** | Vendor, Employee, Project, Fund, Customer, User |
| **Workflow** | WorkflowDefinition, WorkflowStep, WorkflowAction, WorkflowState, WorkflowRoleBinding, WorkflowFieldDefinition |
| **Security** | UserRole, RefreshToken, AuditLog |
| **Integration** | ExternalSystemMapping, SapEnvironment, OutboxEvent |
| **Events** | Event, EventParticipant, VendorDocument |

**Hybrid JSON Pattern**: Several tables use a `Attributes` NVARCHAR(MAX) column storing JSON for semi-structured data, enabling schema evolution without migrations while keeping indexed fields as standard SQL columns.

**Serverless Fit**: Azure SQL supports a **Serverless compute tier** that auto-pauses and auto-scales. The current Basic (5 DTU) tier is fixed; migrating to the Serverless tier would enable pay-per-second billing and auto-pause during inactivity.

---

### 4.4 Document Store — Azure Cosmos DB

| Property | Value |
|---|---|
| **Account** | `cosmos-vendor-mdm-dev` |
| **API** | SQL (Core) — GlobalDocumentDB |
| **Consistency** | Session |
| **Endpoint** | `https://cosmos-vendor-mdm-dev.documents.azure.com:443/` |
| **Database** | VendorMdm |

**Deployed Containers** (verified):

| Container | Partition Key | Purpose |
|---|---|---|
| `DomainEvents` | `/EventType` | Immutable event sourcing log |
| `InvitationArtifacts` | `/InvitationId` | Invitation lifecycle documents |

**Bicep-Defined Containers** (available for deployment):

| Container | Partition Key | Purpose |
|---|---|---|
| `ChangeRequestData` | `/RequestId` | Complex JSON payloads for change requests |
| `ReferenceData` | `/Category` | Master reference data |
| `ValidationRules` | `/EntityType` | Dynamic validation schemas |
| `CanonicalArtifacts` | `/EntityType` | Canonical entity snapshots |
| `AuditLogs` | `/EntityId` | Detailed audit trail documents |
| `IntegrationEvents` | `/SourceSystem` | Cross-system integration events |
| `Configuration` | `/Scope` | Runtime configuration documents |

**Serverless Fit**: Cosmos DB is already operating in **Serverless capacity mode** — pay-per-RU with no provisioned throughput. This is ideal for the development/low-traffic workloads. For production, the throughput model can be switched to autoscale provisioned if consistent traffic patterns emerge.

---

### 4.5 Messaging — Azure Service Bus

| Property | Value |
|---|---|
| **Namespace** | `sb-vendor-mdm-dev` |
| **SKU** | Basic |
| **Queues** | `invitation-created` (Active) |
| **Topics** | None deployed (Basic tier) |

**Purpose**: Asynchronous message delivery for decoupled event processing. The `invitation-created` queue triggers downstream processing (email sending, artifact creation).

**Bicep-Defined (Standard Tier)**:
- **Topic**: `vendor-changes` with `sap-integration` subscription
- **Queue**: `invitation-emails` with 14-day TTL, dead-letter queue, and duplicate detection

**Serverless Fit**: Service Bus is inherently serverless in consumption — you pay per operation. Upgrading to Standard tier would unlock Topics & Subscriptions for pub/sub fan-out patterns, enabling event-driven serverless functions.

---

### 4.6 Secrets Management — Azure Key Vault

| Property | Value |
|---|---|
| **Vault** | `kv-vendor-mdm-dev` |
| **SKU** | Standard |
| **Authorization** | RBAC |

**Stored Secrets** (10 verified):

| Secret | Purpose |
|---|---|
| `ConnectionStrings--Sql` | Azure SQL connection string |
| `ConnectionStrings--Cosmos` | Cosmos DB connection string |
| `ConnectionStrings--ServiceBus` | Service Bus connection string |
| `ConnectionStrings--BlobStorage` | Storage Account connection string |
| `EmailService--Smtp--Host` | SMTP server hostname |
| `EmailService--Smtp--Username` | SMTP authentication user |
| `EmailService--Smtp--Password` | SMTP authentication password |
| `EmailService--Smtp--FromEmail` | Sender email address |
| `EmailService--Smtp--FromName` | Sender display name |
| `EmailService--Smtp--Enabled` | SMTP feature toggle |

**Access Pattern**: The App Service uses `DefaultAzureCredential` (Managed Identity) to access Key Vault secrets at startup via `Azure.Extensions.AspNetCore.Configuration.Secrets`, injecting them into the ASP.NET Core configuration pipeline.

---

### 4.7 File Storage — Azure Storage Account

| Property | Value |
|---|---|
| **Account** | `stvendormdmdev` |
| **SKU** | Standard_LRS (Locally Redundant) |
| **Containers** | `vendor-attachments` |
| **Public Access** | Disabled |
| **Soft Delete** | 30-day retention |

**Purpose**: Secure blob storage for vendor document attachments (contracts, certificates, compliance docs). CORS configured for the Static Web App and local dev origins.

---

## 5. Serverless Architecture Design

### 5.1 Current State vs. Target Serverless State

```
   CURRENT STATE                          TARGET SERVERLESS STATE
   ─────────────                          ──────────────────────

   ┌─────────────────┐                    ┌─────────────────────┐
   │ Static Web App  │ ── Already ───────>│  Static Web App     │
   │ (Serverless)    │    Serverless      │  (No change)        │
   └─────────────────┘                    └─────────────────────┘

   ┌─────────────────┐                    ┌─────────────────────┐
   │  App Service    │                    │  Container Apps      │
   │  F1 (Always On) │ ── Migrate ──────>│  (Scale to Zero)     │
   │  Fixed Compute  │                    │  Per-request billing │
   └─────────────────┘                    └─────────────────────┘

   ┌─────────────────┐                    ┌─────────────────────┐
   │  SQL Database   │                    │  SQL Serverless      │
   │  Basic (5 DTU)  │ ── Upgrade ──────>│  Auto-pause          │
   │  Fixed Cost     │                    │  vCore-based billing │
   └─────────────────┘                    └─────────────────────┘

   ┌─────────────────┐                    ┌─────────────────────┐
   │  Cosmos DB      │ ── Already ───────>│  Cosmos DB           │
   │  (Serverless)   │    Serverless      │  (No change)         │
   └─────────────────┘                    └─────────────────────┘

   ┌─────────────────┐                    ┌─────────────────────┐
   │  Service Bus    │ ── Already ───────>│  Service Bus         │
   │  (Per-message)  │    Serverless      │  (Upgrade to Std)    │
   └─────────────────┘                    └─────────────────────┘

   (Not deployed)                         ┌─────────────────────┐
                       ── Deploy ────────>│  Azure Functions     │
                                          │  Consumption Plan    │
                                          │  Email, Async Jobs   │
                                          └─────────────────────┘
```

### 5.2 Serverless Components Breakdown

| Component | Serverless Model | Billing | Current Status |
|---|---|---|---|
| **Static Web App** | Fully serverless | Free (bandwidth-based at scale) | Deployed |
| **Cosmos DB** | Serverless capacity | Pay-per-RU consumed | Deployed |
| **Service Bus** | Per-operation | Pay-per-message | Deployed |
| **Key Vault** | Per-operation | Pay-per-transaction | Deployed |
| **Storage Account** | Per-operation | Pay-per-GB + transactions | Deployed |
| **App Service** | Fixed compute (F1) | Free tier (upgrade path below) | Deployed |
| **SQL Database** | Fixed DTU (Basic) | Fixed monthly (upgrade path below) | Deployed |
| **Azure Functions** | Consumption plan | Pay-per-execution | Planned (Bicep ready) |

### 5.3 Event-Driven Serverless Flow

The architecture implements an **Event Sourcing + Outbox Pattern** designed for serverless:

```
 1. User Action (Browser)
    │
    ▼
 2. Static Web App ──> App Service API (REST)
    │
    ▼
 3. Service Layer (Business Logic)
    │
    ├──> SQL Database (Transactional State + OutboxEvent)
    │
    ├──> Cosmos DB (Immutable Domain Events)
    │
    └──> Outbox Processor (Hosted Service)
         │
         ├──> Service Bus Queue (invitation-created)
         │    │
         │    └──> Azure Function (Email Sender) [PLANNED]
         │
         └──> SignalR Hub (Real-time UI Updates)
              │
              └──> Browser (WebSocket notification)
```

### 5.4 Hexagonal Architecture Mapping to Azure

```
                    ┌──────────────────────────────────────────────┐
                    │          HEXAGONAL ARCHITECTURE               │
                    │                                               │
  INBOUND PORTS     │         CORE DOMAIN              OUTBOUND     │
  ──────────────    │         ───────────              PORTS         │
                    │                                  ──────────    │
  Static Web App ──>│  ┌────────────────────────┐                   │
  (React SPA)       │  │                        │──> Azure SQL      │
                    │  │   VendorMdm.Shared      │    (State Store)  │
  App Service ─────>│  │   (Pure Domain Logic)   │                   │
  (REST API)        │  │                        │──> Cosmos DB      │
                    │  │   - Entities            │    (Event Store)  │
  Azure Functions ─>│  │   - DTOs                │                   │
  (Async Triggers)  │  │   - Domain Events       │──> Service Bus   │
                    │  │   - Business Rules       │    (Event Bus)   │
                    │  │                        │                   │
                    │  └────────────────────────┘──> Blob Storage   │
                    │                                 (Documents)    │
                    │                                               │
                    │       ANTI-CORRUPTION LAYER (ACL)              │
                    │       ──────────────────────────               │
                    │       ExternalSystemMapping Table              │
                    │       Canonical ID <──> SAP LIFNR             │
                    │       Canonical ID <──> DUO/UROLES            │
                    │                                               │
                    └──────────────────────────────────────────────┘
```

---

## 6. Security Architecture

### 6.1 Identity & Access

| Layer | Mechanism |
|---|---|
| **User Auth** | Azure AD (MSAL) with JWT Bearer tokens |
| **API Auth** | ASP.NET Core `[Authorize]` + Role-based policies |
| **Service-to-Azure** | Managed Identity (`DefaultAzureCredential`) |
| **Secrets** | Key Vault with RBAC authorization |
| **Roles** | Requestor, Approver, MDMAdmin, ITAdmin |

### 6.2 Network & Transport

| Control | Implementation |
|---|---|
| HTTPS Enforced | App Service `httpsOnly: true` |
| HSTS | 1 year (31536000s) |
| CSP | `default-src 'self'` (no unsafe-eval) |
| X-Frame-Options | `deny` |
| CORS | Whitelisted origins only |
| Rate Limiting | 5 req/min for anonymous endpoints |
| Input Sanitization | Global action filter (XSS prevention) |

---

## 7. Observability

| Capability | Azure Service | Status |
|---|---|---|
| **Distributed Tracing** | OpenTelemetry + OTLP | Configured |
| **Structured Logging** | Microsoft.Extensions.Logging | Active |
| **Application Insights** | App Insights (integration ready) | Bicep-defined |
| **Health Checks** | `/api/health` (EF Core probe) | Active |
| **Audit Trail** | AuditLog table + Cosmos DomainEvents | Active |
| **Real-time Monitoring** | SignalR event stream | Active |

---

## 8. Gap Analysis: Bicep Definitions vs. Deployed Resources

| Component | Bicep Defined | Actually Deployed | Gap |
|---|---|---|---|
| Cosmos Containers | 10 containers | 2 containers | 8 containers not yet deployed |
| Service Bus Topics | `vendor-changes` topic | None | Topic requires Standard tier upgrade |
| Service Bus Queues | `invitation-emails` | `invitation-created` | Different queue name in deployment |
| Azure Functions | Consumption plan + Function App | Not deployed | Email/async processing not live |
| App Insights | Defined in main.bicep | Extension not confirmed | Needs verification |
| SQL Server deployment | Commented out in main.bicep | Deployed separately | Manual or prior deployment |

---

## 9. Cost Optimization (Serverless Benefits)

### Current Dev Environment Monthly Estimate

| Resource | Current Tier | Est. Monthly Cost |
|---|---|---|
| App Service F1 | Free | $0 |
| Static Web App Free | Free | $0 |
| SQL Basic (5 DTU) | Basic | ~$5 |
| Cosmos DB Serverless | Pay-per-RU | ~$0-2 (low traffic) |
| Service Bus Basic | Per-message | ~$0-1 |
| Key Vault | Per-transaction | ~$0-1 |
| Storage Standard_LRS | Per-GB | ~$0-1 |
| **Total Dev** | | **~$5-10/month** |

### Production Serverless Projection

| Resource | Serverless Tier | Est. Monthly Cost |
|---|---|---|
| Container Apps | Scale-to-zero | ~$15-50 (usage-based) |
| Static Web App Standard | Standard | ~$9 |
| SQL Serverless | Auto-pause | ~$5-30 (usage-based) |
| Cosmos DB Serverless | Pay-per-RU | ~$5-25 |
| Service Bus Standard | Per-message + topics | ~$10 |
| Functions Consumption | Per-execution | ~$0-5 |
| Key Vault | Per-transaction | ~$1 |
| Storage Standard_GRS | Per-GB | ~$2-5 |
| **Total Prod** | | **~$47-135/month** |

---

## 10. Recommendations

1. **Deploy Remaining Cosmos Containers**: 8 of 10 defined containers are not yet provisioned. Deploy via `infrastructure/modules/cosmos.bicep` to enable ChangeRequestData, AuditLogs, and other planned features.

2. **Upgrade Service Bus to Standard**: Current Basic tier does not support Topics. The `vendor-changes` topic with SAP subscription requires Standard tier (~$10/month).

3. **Deploy Azure Functions**: The `VendorMdm.Artifacts` project and `invitation-infrastructure.bicep` define a Consumption-plan Function App for email processing. Deploy to enable serverless async workflows.

4. **SQL Serverless Tier**: Migrate from Basic (5 DTU) to General Purpose Serverless for auto-pause during inactivity and vCore-based scaling.

5. **Container Apps Migration**: When outgrowing the F1 App Service, migrate to Azure Container Apps for true serverless container hosting with scale-to-zero and per-second billing.

6. **Enable Application Insights**: Verify App Insights deployment and ensure the connection string is configured in the App Service for full APM.

---

*This brief was generated on 2026-02-25 by validating against live Azure resources in subscription `8c89e199-...` using `az resource list` and service-specific Azure CLI queries.*
