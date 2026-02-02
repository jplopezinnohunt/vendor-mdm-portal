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

### 2.1.2 Domain Ontology (The "World Model")
- **Location**: `backend/VendorMdm.Shared/Ontology/`
- **Responsibility**: The executable definition of what exists (Concepts) and how it behaves.
- **Role**:
    -   **Concepts**: Classes implementing `IOntologyConcept` (e.g., `Vendor`, `Contract`).
    -   **Rules**: Logic that holds true regardless of persistence (e.g., `IsEligibleForInvitation`).
    -   **Origin Contexts**: Enums defining how an entity came to be (`Direct`, `Event`, `Migration`).
-   **Usage**: Services MUST delegate decision-making to this layer. `Service` -> `Ontology` -> `Result`.

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

## 4. Mandatory Testing (Deployment Gate)
- **Policy**: No code may be deployed to any environment without verified automated tests.
- **Requirements**:
    -   **Unit Tests**: All Service Layer logic must have covering unit tests.
    -   **Integration Tests**: Canonical Entity flows (Create/Read/Update) must be verified against the running API (or integration test suite) before merge.
    -   **Schema Validation**: All JSON payloads must be validated against the JSON Schema `v1.0.0` or higher.

## 5. Error Handling Standard
- **Policy**: APIs MUST return standardized HTTP status codes and strict error messages.
- **Rules**:
    -   **400 Bad Request**: Validation failures (Schema, Business Rules).
    -   **404 Not Found**: Entity does not exist.
    -   **500 Internal Server Error**: Unhandled exceptions (Database, System).
    -   **Format**: Response Body MUST include `message` and optionally `details`.
    -   **Implementation**: Use `try/catch` in Controllers to wrap Service calls and return `StatusCode(500, ex.Message)`.

## 6. Schema Evolution Strategy (Forward Compatibility)
- **Problem**: Canonical entities evolve over time (e.g., adding `LegalEntityIdentifier`).
- **Standard**: **Semantic Versioning** for JSON Schemas.
- **Rules**:
    1.  **Additive Only**: New fields MUST be optional or have default values. Never remove or rename existing fields in the SAME major version.
    2.  **Schema Versioning**: All Canonical Entities MUST have a `SchemaVersion` property (e.g., "1.0.0").
    3.  **Immutable History**: Old artifacts in Cosmos DB retain their original `SchemaVersion` and structure. Do NOT back-fill old events.
    4.  **Major Breaking Changes**: If a breaking change is required (e.g., renaming a core field), increment the Major Version (e.g., `v2.0.0`) and create a NEW Schema file.
    5.  **JSON Schema Repository**: Iterate schemas in `VendorMdm.Shared/Schemas/` (e.g., `vendor-v1.0.0.json`, `vendor-v1.1.0.json`).
    6.  **Event Propagation**: `DomainEvent` objects MUST include `SchemaVersion` to inform downstream consumers of the payload structure.
