# Invitation Flow Feature

## Overview
The Invitation Flow allows administrators to invite vendors to the platform. It uses a **Hybrid Architecture** combining SQL Server for state management and Cosmos DB for event sourcing and audit trails.

## Architecture

### 1. Data Flow
1.  **Creation**: Admin sends invitation via API.
2.  **State (SQL)**: `VendorInvitation` record created in SQL (Status: Pending).
3.  **Audit (Cosmos)**: `InvitationArtifact` stored in Cosmos `InvitationArtifacts` container (Full payload).
4.  **Event (Cosmos)**: `DomainEvent` emitted to Cosmos `DomainEvents` container.
5.  **Notification (Service Bus)**: message published to `invitation-created` topic.
6.  **Email**: `EmailService` consumes message (or triggered via Azure Function) and sends email.

### 2. Components
-   **InvitationService**: Core business logic.
-   **ServiceBusService**: Handles messaging integration.
-   **InvitationController**: Secure API endpoints (Role: AdminOrApprover).
-   **CosmosRepository**: Handles artifact and event storage.

## Security
-   **Authentication**: Azure AD B2C / Entra ID via `Microsoft.Identity.Web`.
-   **Authorization**: `AdminOrApprover` policy required for sensitive operations.
-   **Validation**: Tokens are validated for expiration and usage.

## Testing
-   **Unit Tests**: `VendorMdm.Api.Tests` covers validation and service logic.
-   **Integration Tests**: Verifies flow A->B->C->D using mocked infrastructure.

## Configuration
Required app settings:
-   `ConnectionStrings:Sql`
-   `ConnectionStrings:Cosmos`
-   `ConnectionStrings:ServiceBus`
-   `AzureAd:ClientId`, `TenantId`, `Instance`
