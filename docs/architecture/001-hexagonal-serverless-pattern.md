# Architectural Rule 001: Hexagonal Serverless Pattern

**Status**: MANDATORY
**Context**: Vendor Master Data Management Platform
**Date**: 2025-12-18

## 1. Core Principle
The platform follows a strict **Hexagonal Architecture (Ports & Adapters)** pattern, hosted on a **Serverless** infrastructure. This ensures the core domain logic remains isolated from external systems and infrastructure concerns.

## 2. Layer Definitions

### 2.1 Core Domain (The "Golden" Kernel)
- **Location**: `backend/VendorMdm.Shared/`
- **Responsibility**: Contains PURE business entities, logic, and JSON schema definitions.
- **Constraints**:
    -   MUST NOT reference SAP, Salesforce, SQL, or HTTP libraries.
    -   MUST NOT contain external system IDs (e.g., `LIFNR`).
    -   MUST be compile-able independently of the API or Database.

### 2.2 Inbound Port (API Layer)
- **Location**: `backend/VendorMdm.Api/Controllers/`
- **Responsibility**: Contract-first REST APIs.
- **Role**:
    -   Accepts HTTP requests.
    -   Validates payloads against strict JSON Schemas (`VendorMdm.Shared.Schemas`).
    -   Assigns Correlation IDs.
    -   Delegates processing to the Service Layer.

### 2.3 Persistence Port (Hybrid SQL/JSONB)
- **Location**: `backend/VendorMdm.Api/Data/` & `backend/VendorMdm.Api/Services/`
- **Responsibility**: State management and Audit logging.
- **Components**:
    -   **State Store (SQL)**: PostgreSQL/SQL Server.
        -   Uses **Standard SQL Columns** for identity and indexed fields (e.g., `Id`, `TaxId`, `Email`).
        -   Uses **JSONB (Attributes)** for all other data.
    -   **Functional Log (Cosmos DB)**: Immutable ledger recording every state change as an artifact.

### 2.4 Outbound Port (Event Bus)
- **Location**: `backend/VendorMdm.Api/Services/` (Event Emission)
- **Responsibility**: Publishing domain events.
- **Mechanism**:
    -   Services emit events (e.g., `VendorCreated`) to **Cosmos DB** (Event Store).
    -   Cosmos Change Feed triggers downstream Serverless Functions.

### 2.5 Anti-Corruption Layer (ACL) Adapters
- **Location**: `MigrationRunner` (or separate Function Apps)
- **Responsibility**: Translation between Canonical and External formats.
- **Pattern**: **Hub-and-Spoke**.
    -   `Canonical Entity` (Hub) <-> `ExternalSystemMapping` <-> `Adapter` <-> `External System` (Spoke).
    -   **ExternalSystemMapping** table is the ONLY place where internal GUIDs meet external IDs (e.g., SAP, DUO, UROLES).

## 3. Serverless First
- All new compute logic MUST be implemented as **Azure Functions** or **Container Apps** (Serverless Containers).
- No long-running VMs or stateful services.

## 4. Compliance Checklist
- [ ] Entity defined in `Shared/Models`?
- [ ] JSON Schema created in `Shared/Schemas`?
- [ ] Service implements `Sql -> Cosmos -> Event` flow?
- [ ] No external dependencies in Core?
- [ ] All external IDs managed via `ExternalSystemMapping`?
