# 3-Way Cross-Validation Report: Brief vs. Azure vs. Code vs. Golden Rules

**Date**: 2026-02-25 | **Auditor**: Automated (Claude Agent) | **Sources**: 4

| Source | Description |
|--------|-------------|
| **BRIEF** | `docs/architecture/azure-serverless-architecture-brief.md` |
| **AZURE** | Live `az` CLI queries against `rg-vendor-mdm-dev` |
| **CODE** | Actual `.cs`, `.tsx`, `.csproj`, `Program.cs`, `appsettings.json` |
| **RULES** | `moderngoldenrules.md`, `001-hexagonal-serverless-pattern.md`, `hexagonal-architecture-standards.md`, `principles.md` |

---

## LEGEND

| Symbol | Meaning |
|--------|---------|
| :white_check_mark: | All sources agree — no discrepancy |
| :warning: | Mismatch between sources — needs attention |
| :x: | Critical discrepancy — rule violation or missing component |
| :bulb: | Observation — not a violation but noteworthy |

---

## 1. COSMOS DB

### 1.1 Database Name

| Source | Value | Match |
|--------|-------|-------|
| **AZURE** | `VendorMdm` (1 database deployed) | - |
| **CODE** (InvitationService) | `VendorMdm` | :white_check_mark: |
| **CODE** (CosmosRepository) | `MdmCore` | :x: |
| **BRIEF** | `VendorMdm` only | :warning: |
| **RULES** (principles.md) | `VendorMdm` | :white_check_mark: |

> :x: **DISCREPANCY**: Code references **TWO** Cosmos databases — `VendorMdm` (InvitationService) and `MdmCore` (CosmosRepository used by Employee, Project, Fund, Customer, User, Vendor, ChangeRequest services). Only `VendorMdm` exists in Azure. The `MdmCore` database is **not deployed**, meaning CosmosRepository calls will fail in Connected mode.

### 1.2 Containers

| Container | AZURE Deployed | CODE Referenced | Bicep main.bicep | Bicep cosmos module | RULES (principles.md) |
|-----------|---------------|-----------------|-------------------|--------------------|-----------------------|
| `DomainEvents` | :white_check_mark: Yes | :white_check_mark: Yes (both DBs) | :white_check_mark: Yes | :white_check_mark: Yes | :white_check_mark: Yes |
| `InvitationArtifacts` | :white_check_mark: Yes | :white_check_mark: Yes | :white_check_mark: Yes | :white_check_mark: Yes | :white_check_mark: Yes |
| `ChangeRequestData` | :x: No | :white_check_mark: Yes (MdmCore) | :x: No | :white_check_mark: Yes | :white_check_mark: Yes |
| `AuditLogs` | :x: No | :white_check_mark: Yes (MdmCore) | :x: No | :white_check_mark: Yes | :x: No |
| `IntegrationEvents` | :x: No | :white_check_mark: Yes (MdmCore) | :x: No | :white_check_mark: Yes | :x: No |
| `Configuration` | :x: No | :white_check_mark: Yes (MdmCore) | :x: No | :white_check_mark: Yes | :x: No |
| `ReferenceData` | :x: No | :x: No | :x: No | :white_check_mark: Yes | :x: No |
| `ValidationRules` | :x: No | :x: No | :x: No | :white_check_mark: Yes | :x: No |
| `CanonicalArtifacts` | :x: No | :x: No | :x: No | :white_check_mark: Yes | :x: No |
| `VendorChangeArtifacts` | :x: No | :x: No | :x: No | :x: No | :white_check_mark: Yes |

> :x: **DISCREPANCY**:
> - **8 containers** defined in `cosmos.bicep` module but only **2 deployed** in Azure.
> - **`main.bicep`** only deploys 2 containers (InvitationArtifacts, DomainEvents) — it does NOT use the cosmos module.
> - Code references 5 containers under a **different database** (`MdmCore`) that doesn't exist in Azure.
> - Golden Rules (principles.md) mandate `VendorChangeArtifacts` — not defined anywhere in Bicep or code.

### 1.3 Partition Keys

| Container | AZURE | CODE | Bicep Module |
|-----------|-------|------|-------------|
| `DomainEvents` | `/EventType` | `/EventType` (lowercase `eventType` in InvitationService) | `/eventType` |
| `InvitationArtifacts` | `/InvitationId` | `/InvitationId` (lowercase `invitationId` in code) | `/invitationId` |

> :bulb: **Case sensitivity**: Cosmos DB partition keys are case-sensitive. Code uses camelCase (`eventType`) while Azure shows PascalCase (`/EventType`). This could cause partition mismatches depending on which deployment created the containers.

### 1.4 Throughput Model

| Source | Value |
|--------|-------|
| **AZURE** | Serverless (pay-per-RU, no provisioned throughput) |
| **Bicep main.bicep** | Serverless (`enableServerless` capability) |
| **Bicep cosmos module** | **400 RU/s provisioned per container** |
| **BRIEF** | "Serverless capacity mode" |

> :warning: **DISCREPANCY**: The `cosmos.bicep` module defines **400 RU/s provisioned throughput** per container (would cost ~$23/month each), but `main.bicep` deploys Cosmos as **Serverless**. The module and main template are incompatible — deploying the module as-is would fail or create a provisioned account.

---

## 2. SQL DATABASE

### 2.1 Server & Database

| Property | AZURE | CODE (appsettings) | Bicep main.bicep | Bicep sql module |
|----------|-------|---------------------|-------------------|-----------------|
| Server | `sql-vendor-mdm-dev` | `sql-vendor-mdm-dev` | **Commented out** | `mdmportal-sql-12031241-{env}` |
| Database | `VendorMdmDb` | `VendorMdmDb` | **Commented out** | `mdmportal-sqldb-{env}` |
| Tier | Basic (5 DTU) | - | - | Basic (5 DTU) |

> :warning: **DISCREPANCY**:
> - SQL deployment is **commented out** in `main.bicep` — the deployed SQL was created outside IaC or via an earlier deployment.
> - The `sql.bicep` module uses a different naming convention (`mdmportal-sql-12031241-{env}`) than what's deployed (`sql-vendor-mdm-dev`).

### 2.2 DbSet Count (SQL Tables)

| Source | Count | Notes |
|--------|-------|-------|
| **CODE** (SqlDbContext) | **27 DbSets** | Full list verified |
| **BRIEF** | Mentions ~20 tables | Undercount |

> :warning: **BRIEF INACCURACY**: Brief lists fewer tables than actual DbSets. Missing from brief: `VendorInvitationsCanonical`, `ChangeRequestsCanonical`, `WorkflowFieldDefinitions`, `WorkflowRoleBindings`, `WorkflowActions`, `WorkflowSteps`, `WorkflowDefinitions`.

### 2.3 Connection Strategy

| Source | Pattern |
|--------|---------|
| **AZURE** (App Service setting) | `DataSourceMode = Connected` |
| **CODE** (appsettings.json) | `DataSourceMode = Auto` |
| **CODE** (appsettings.Development.json) | `UseLocalEmulators = true` (SQLite) |

> :bulb: App Service overrides config to `Connected` mode, while the codebase defaults to `Auto` which detects based on connection strings.

---

## 3. SERVICE BUS

### 3.1 Queues

| Queue Name | AZURE | CODE | Bicep main.bicep | Bicep servicebus module | Bicep invitation-infra |
|------------|-------|------|-------------------|-----------------------|----------------------|
| `invitation-created` | :white_check_mark: Yes | :x: No | :white_check_mark: Yes | :x: No | :x: No |
| `invitation-emails` | :x: No | :white_check_mark: Yes | :x: No | :white_check_mark: Yes | :white_check_mark: Yes |
| `vendor-applications` | :x: No | :white_check_mark: Yes | :x: No | :x: No | :x: No |
| `vendor-status` | :x: No | :white_check_mark: Yes | :x: No | :x: No | :x: No |
| `approvals` | :x: No | :white_check_mark: Yes | :x: No | :x: No | :x: No |
| `vendor-changes` | :x: No | :white_check_mark: Yes (default) | :x: No | :x: Topic (not queue) | :white_check_mark: Queue |

> :x: **CRITICAL DISCREPANCY**:
> - Azure has **1 queue** (`invitation-created`), but code sends to **5 different queues** dynamically.
> - The deployed queue name (`invitation-created`) doesn't match what code references (`invitation-emails`).
> - Service Bus is **Basic tier** in Azure — Topics are NOT supported. The servicebus module defines Standard tier with a `vendor-changes` topic.
> - Code creates `ServiceBusSender` dynamically per queue name — these queues don't exist in Azure and messages will fail.

### 3.2 Topics

| Topic | AZURE | CODE | Bicep servicebus module |
|-------|-------|------|------------------------|
| `vendor-changes` | :x: No (Basic tier) | :x: No topic usage | :white_check_mark: Yes (Standard) |

> :warning: Topics require Standard tier. Current Basic tier deployment cannot support topics.

### 3.3 Azure Functions as Consumers

| Function | CODE (VendorMdm.Artifacts) | AZURE | Bicep |
|----------|---------------------------|-------|-------|
| `InvitationEmailFunction` | :white_check_mark: ServiceBusTrigger on `invitation-emails` | :x: Not deployed | :white_check_mark: Defined |
| `VendorArtifactFunction` | :white_check_mark: HTTP triggers | :x: Not deployed | :white_check_mark: Defined |
| `MetadataFunction` | :white_check_mark: HTTP triggers | :x: Not deployed | :white_check_mark: Defined |

> :x: **Azure Functions NOT DEPLOYED**: 3 functions exist in code, Bicep templates are ready, but nothing is deployed in Azure. The `InvitationEmailFunction` depends on the `invitation-emails` queue which also doesn't exist.

---

## 4. KEY VAULT

### 4.1 Secrets

| Secret | AZURE | CODE References |
|--------|-------|----------------|
| `ConnectionStrings--Sql` | :white_check_mark: | :white_check_mark: |
| `ConnectionStrings--Cosmos` | :white_check_mark: | :white_check_mark: |
| `ConnectionStrings--ServiceBus` | :white_check_mark: | :white_check_mark: |
| `ConnectionStrings--BlobStorage` | :white_check_mark: | :white_check_mark: |
| `EmailService--Smtp--Host` | :white_check_mark: | :white_check_mark: |
| `EmailService--Smtp--Username` | :white_check_mark: | :white_check_mark: |
| `EmailService--Smtp--Password` | :white_check_mark: | :white_check_mark: |
| `EmailService--Smtp--FromEmail` | :white_check_mark: | :white_check_mark: |
| `EmailService--Smtp--FromName` | :white_check_mark: | :white_check_mark: |
| `EmailService--Smtp--Enabled` | :white_check_mark: | :white_check_mark: |

> :white_check_mark: **FULLY ALIGNED** — All 10 secrets match between Azure and code references.

### 4.2 RBAC Configuration

| Source | RBAC Enabled |
|--------|-------------|
| **AZURE** | Not verified (Standard SKU confirmed) |
| **Bicep main.bicep** | `enableRbacAuthorization: false` |
| **Bicep keyvault module** | `enableRbacAuthorization: true` |

> :warning: **DISCREPANCY**: `main.bicep` disables RBAC, but `keyvault.bicep` module enables it. Different templates produce different configurations.

---

## 5. STORAGE ACCOUNT

| Property | AZURE | CODE | Bicep |
|----------|-------|------|-------|
| Account | `stvendormdmdev` | BlobServiceClient injected | :white_check_mark: `st{prefix}{env}` |
| Container: `vendor-attachments` | :white_check_mark: | :white_check_mark: | :white_check_mark: |
| Container: `deleted-blobs` | :x: Not found | :white_check_mark: Referenced in code | :x: Not in Bicep |
| Public Access | Disabled | - | Disabled |

> :warning: **DISCREPANCY**: Code references a `deleted-blobs` container for soft-deleted blob archival, but it doesn't exist in Azure or Bicep.

---

## 6. APP SERVICE

| Property | AZURE | CODE (appsettings) | Bicep |
|----------|-------|---------------------|-------|
| Runtime | DOTNETCORE 8.0 | .NET 8.0 | DOTNETCORE 8.0 |
| Tier | F1 Free | - | F1 Free |
| HTTPS Only | True | HSTS configured | True |
| Always On | False (F1 limit) | - | False |
| Managed Identity | SystemAssigned | DefaultAzureCredential | SystemAssigned |
| Health endpoint | `/api/health` (CI/CD) | `/health/live`, `/health/ready`, `/health/startup` | - |

> :warning: **HEALTH ENDPOINT MISMATCH**:
> - CI/CD workflows test `/api/health`
> - Code maps health checks to `/health/live`, `/health/ready`, `/health/startup` (no `/api` prefix)
> - Golden Rules standard expects `/health`, `/health/ready`, `/health/live`
> - There is NO `/api/health` endpoint in the code

---

## 7. STATIC WEB APP

| Property | AZURE | CODE | Bicep |
|----------|-------|------|-------|
| Hostname | `thankful-field-0258f8110.3.azurestaticapps.net` | - | - |
| SKU | Free | - | Free |
| Branch | `develop` | - | - |
| Framework | - | React 19.2 + Vite 6.2 | - |
| MSAL Client ID | - | `2f2020ec-264d-4de5-bea4-f4dfc545c5d8` | Same |
| MSAL Tenant ID | - | `a93513e2-d327-4301-80ed-d703eb03f6cb` | Same |

> :white_check_mark: **ALIGNED** — Frontend deployment matches configuration.

---

## 8. AUTHENTICATION

| Property | CODE Backend | CODE Frontend | AZURE | RULES |
|----------|-------------|---------------|-------|-------|
| Azure AD | Configured (conditional) | MSAL 4.27 fully wired | Auth disabled for testing | Required |
| JWT Bearer | Registered in Program.cs | Token acquisition via MSAL | - | Required |
| Mock Auth | MockAuthMiddleware (dev) | mockLogin() method | - | Dev only |
| Cookie Auth | `VendorMdmAuth` (60 min) | Session storage | - | - |

> :bulb: **Auth is DISABLED** in production per `PENDING-AUTH-REENABLE.md`. Frontend has full MSAL integration ready. Backend conditionally enables Azure AD based on `AzureAd:ClientId` config presence.

### 8.1 Roles Alignment

| Role | Backend (Policies) | Frontend (AuthContext) |
|------|-------------------|----------------------|
| Requestor | :white_check_mark: | :white_check_mark: |
| Approver | :white_check_mark: | :white_check_mark: |
| MDMAdmin | :white_check_mark: | :white_check_mark: (as Admin) |
| ITAdmin | :white_check_mark: | :x: Not listed |
| Vendor | :x: Not a policy role | :white_check_mark: |
| VendorUnit | :x: Not in backend | :white_check_mark: |
| BFM | :x: Not in backend | :white_check_mark: |
| Viewer | :x: Not in backend | :white_check_mark: |

> :warning: **ROLE MISMATCH**: Frontend defines 7 roles (Vendor, Requestor, VendorUnit, BFM, Approver, Admin, Viewer) but backend only has 3 authorization policies (ApproverOnly, RequestorOnly, AdminOnly) covering 4 roles (Requestor, Approver, MDMAdmin, ITAdmin). Frontend roles `Vendor`, `VendorUnit`, `BFM`, `Viewer` have no backend policy enforcement.

---

## 9. HEXAGONAL ARCHITECTURE COMPLIANCE (Golden Rules)

### 9.1 Core Domain Isolation (Rule 2.1)

| Requirement | Status | Evidence |
|------------|--------|----------|
| `VendorMdm.Shared` has NO SAP/SQL/HTTP refs | :warning: Needs verification | Shared contains DTOs, Models, Domain Events, Mapping Extensions |
| `VendorMdm.Shared` compiles independently | :warning: Needs verification | Part of solution but separate project |
| External IDs (LIFNR) NOT in Core | :warning: | Frontend `types.ts` defines `LIFNR` in `VendorMasterData` interface |

> :warning: **POTENTIAL VIOLATION**: Frontend `types.ts` contains SAP-specific field names (`LIFNR`, `NAME1`, `LFBK`) in `VendorMasterData` interface. The golden rule states core domain MUST NOT contain external system IDs.

### 9.2 Domain Ontology (Rule 2.1.2)

| Requirement | Status | Evidence |
|------------|--------|----------|
| `backend/VendorMdm.Shared/Ontology/` exists | :x: Not verified | Not found in code audit |
| `IOntologyConcept` implemented | :x: Not found | No references in codebase |
| Origin Contexts defined | :x: Not found | No enum for Direct/Event/Migration |

> :x: **RULE VIOLATION**: The Hexagonal Architecture Standard mandates a Domain Ontology layer with `IOntologyConcept`, but this does not appear to be implemented in the codebase.

### 9.3 Mandatory Hybrid Flow: SQL -> Cosmos Artifact -> Cosmos Event -> Service Bus

| Service | SQL | Cosmos Artifact | Cosmos Event | Service Bus | Compliant |
|---------|-----|-----------------|--------------|-------------|-----------|
| InvitationService | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: |
| VendorService | :white_check_mark: | :white_check_mark: | :white_check_mark: | :warning: Via Outbox | :white_check_mark: |
| ChangeRequestRepository | :white_check_mark: | :white_check_mark: | :white_check_mark: | :warning: Via Outbox | :white_check_mark: |
| EmployeeService | :white_check_mark: | :white_check_mark: (MdmCore) | :warning: | :x: No | :warning: |
| ProjectService | :white_check_mark: | :white_check_mark: (MdmCore) | :warning: | :x: No | :warning: |
| FundService | :white_check_mark: | :white_check_mark: (MdmCore) | :warning: | :x: No | :warning: |
| CustomerService | :white_check_mark: | :white_check_mark: (MdmCore) | :warning: | :x: No | :warning: |
| UserService | :white_check_mark: | :white_check_mark: (MdmCore) | :warning: | :x: No | :warning: |

> :warning: **PARTIAL COMPLIANCE**: Core services (Invitation, Vendor, ChangeRequest) follow the hybrid pattern. Entity services (Employee, Project, Fund, Customer, User) write to Cosmos via `CosmosRepository` using the **MdmCore** database which doesn't exist in Azure. These services will fail in Connected mode.

### 9.4 Serverless First (Rule 3)

| Requirement | Status | Evidence |
|------------|--------|----------|
| New compute as Azure Functions or Container Apps | :warning: | App Service (F1) is the primary compute — NOT serverless |
| No long-running VMs or stateful services | :warning: | `OutboxProcessor` is a **hosted background service** — long-running |
| Azure Functions deployed | :x: | 3 functions in code, 0 deployed |

> :x: **RULE VIOLATION**: The golden rule states "All new compute logic MUST be implemented as Azure Functions or Container Apps." The primary API runs on App Service (not serverless), and `OutboxProcessor` is a long-running hosted service, contradicting the serverless-first principle.

### 9.5 Anti-Corruption Layer (Rule 2.5)

| Requirement | Status | Evidence |
|------------|--------|----------|
| `ExternalSystemMapping` table exists | :white_check_mark: | DbSet in SqlDbContext |
| `ExternalSystemMappingService` exists | :white_check_mark: | In Services directory |
| External IDs ONLY in ACL | :warning: | Frontend has SAP IDs (`LIFNR`) |
| Hub-and-Spoke pattern | :white_check_mark: | `SapMapperService` exists |

> :white_check_mark: Backend ACL is properly implemented. Frontend leaks SAP field names but this is a display concern.

### 9.6 Schema Evolution (Rule 6)

| Requirement | Status | Evidence |
|------------|--------|----------|
| `SchemaVersion` property on entities | :x: Not found | No SchemaVersion in DbSets |
| JSON Schema repository in `Shared/Schemas/` | :x: Not verified | Not found in audit |
| DomainEvents include SchemaVersion | :x: Not found | Domain events lack versioning |

> :x: **RULE VIOLATION**: The Hexagonal Architecture Standard mandates semantic versioning for JSON schemas and `SchemaVersion` on all canonical entities. This is not implemented.

### 9.7 Health Endpoints (Rule 7)

| Requirement | CODE | CI/CD | Standard |
|-------------|------|-------|----------|
| `/health` | :x: Not mapped | `/api/health` in workflows | :white_check_mark: Required |
| `/health/ready` | :white_check_mark: Mapped | :x: Not tested | :white_check_mark: Required |
| `/health/live` | :white_check_mark: Mapped | :x: Not tested | :white_check_mark: Required |
| `/health/startup` | :white_check_mark: Mapped | :x: Not tested | :x: Not in standard |

> :warning: **MISMATCH**: Code maps to `/health/*` but CI/CD tests `/api/health`. The standard requires `/health`, `/health/ready`, `/health/live`. Code adds `/health/startup` (extra). CI/CD uses `/api/health` which doesn't exist.

---

## 10. BRIEF ACCURACY ASSESSMENT

### 10.1 Statements Verified as Correct

| Brief Claim | Verified |
|-------------|----------|
| 22 Controllers | :white_check_mark: Exact match |
| React 19.2 + TypeScript 5.8 + Vite 6.2 | :white_check_mark: |
| MSAL Browser 4.27 + SignalR 10.0 | :white_check_mark: |
| .NET 8.0 on Linux App Service | :white_check_mark: |
| Key Vault with 10 secrets | :white_check_mark: |
| Cosmos DB Serverless, Session consistency | :white_check_mark: |
| SQL Basic 5 DTU, 2GB | :white_check_mark: |
| Storage Standard_LRS with vendor-attachments | :white_check_mark: |
| Outbox Pattern implemented | :white_check_mark: |
| Domain Events with SignalR handler | :white_check_mark: |
| OpenTelemetry + App Insights configured | :white_check_mark: |
| Rate limiting 5 req/min | :white_check_mark: |

### 10.2 Statements Requiring Correction

| Brief Claim | Reality |
|-------------|---------|
| "27 DbSets" mentioned as tables | Brief said ~20, actual is **27** |
| "Cosmos Change Feed triggers Functions" | Code uses **Outbox Pattern + Service Bus**, not Change Feed |
| "5 queues in Service Bus" | Only **1 queue** deployed; code references 5 but they don't exist |
| Health endpoint `/api/health` | Code maps `/health/live`, `/health/ready`, `/health/startup` |
| Cosmos has 1 database | Code references **2 databases** (`VendorMdm` + `MdmCore`) |
| "47 service files" from audit | Brief didn't specify — should be documented |
| Service Bus "inherently serverless" | Basic tier is fixed-cost, not pay-per-message |

---

## 11. CRITICAL ISSUES SUMMARY (Ranked by Severity)

### :x: P0 — Will Fail in Production

| # | Issue | Impact |
|---|-------|--------|
| 1 | **MdmCore Cosmos database doesn't exist** in Azure. CosmosRepository targets `MdmCore` but only `VendorMdm` is deployed. | Entity services (Employee, Project, Fund, Customer, User) will throw `CosmosException` in Connected mode. |
| 2 | **4 of 5 Service Bus queues don't exist**. Code sends to `invitation-emails`, `vendor-applications`, `vendor-status`, `approvals`, `vendor-changes` — only `invitation-created` exists. | Service Bus publishing will fail silently or throw. |
| 3 | **CI/CD tests `/api/health`** but code maps health checks to `/health/*`. | Post-deployment health verification will always fail with 404. |

### :warning: P1 — Architectural Gaps

| # | Issue | Impact |
|---|-------|--------|
| 4 | **Azure Functions not deployed** — email sending via Function URL will fail. | Invitation emails won't be sent via the Function path. |
| 5 | **Cosmos partition key case mismatch** — Azure has `/EventType` (PascalCase), code may write `/eventType` (camelCase). | Documents may land in wrong partitions. |
| 6 | **Bicep naming inconsistency** — modules use `mdmportal-*` prefix, main.bicep and actual resources use `vendor-mdm-*`. | Running module templates would create duplicate resources. |
| 7 | **Frontend defines 7 roles**, backend only enforces 4. | Roles `Vendor`, `VendorUnit`, `BFM`, `Viewer` have no backend authorization. |
| 8 | **`deleted-blobs` container** referenced in code but doesn't exist in Azure or Bicep. | Blob soft-delete archival will fail. |

### :x: P2 — Golden Rule Violations

| # | Rule | Violation |
|---|------|-----------|
| 9 | **Serverless First** (Rule 3) | Primary compute is App Service F1 (not serverless). `OutboxProcessor` is a long-running hosted service. |
| 10 | **Domain Ontology** (Rule 2.1.2) | `IOntologyConcept` and Ontology layer not implemented. |
| 11 | **Schema Evolution** (Rule 6) | No `SchemaVersion` property on entities or domain events. No JSON Schema repository. |
| 12 | **Mandatory Testing Gate** (Rule 4) | Schema validation tests (`VendorMdm.SchemaTest`) exist as a project but compliance is unverified. |

---

## 12. RECOMMENDED ACTIONS

### Immediate (P0)

1. **Fix Cosmos Database alignment**: Either:
   - Update `CosmosRepository` to use `VendorMdm` database (match Azure), OR
   - Deploy `MdmCore` database with required containers via Bicep

2. **Deploy missing Service Bus queues** or update code to use `invitation-created` (the queue that exists):
   - `invitation-emails`, `vendor-applications`, `vendor-status`, `approvals`, `vendor-changes`

3. **Fix health endpoint path** — either:
   - Add `/api/health` mapping in code, OR
   - Update CI/CD workflows to test `/health/live`

### Short-term (P1)

4. Deploy Azure Functions from `VendorMdm.Artifacts`
5. Verify Cosmos partition key casing consistency
6. Unify Bicep naming convention (choose `vendor-mdm-*` or `mdmportal-*`)
7. Add backend authorization policies for missing frontend roles
8. Add `deleted-blobs` container to Bicep and deploy

### Strategic (P2 — Golden Rules)

9. Plan App Service -> Container Apps migration (serverless-first compliance)
10. Implement Domain Ontology layer (`IOntologyConcept`)
11. Add `SchemaVersion` to canonical entities and domain events
12. Create JSON Schema repository in `VendorMdm.Shared/Schemas/`

---

*Generated 2026-02-25 by 3-way cross-validation against live Azure subscription `8c89e199-...`, codebase at commit `ef99f25`, and Golden Rules v1.7.0.*
