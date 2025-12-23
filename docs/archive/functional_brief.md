# Vendor MDM Portal (VEN) - Functional Brief

## 1. Overview
The Vendor MDM Portal (VEN) is a centralized platform for managing vendor master data. It facilitates the onboarding of new vendors, maintenance of existing vendor profiles, and ensures data integrity through approval workflows and integration with SAP.

## 2. Roles & Permissions
The application serves three distinct user roles, each with specific capabilities:

### **Vendor**
- **Purpose**: External entity providing goods/services.
- **Capabilities**:
    - Complete registration (Onboarding).
    - View own profile and status.
    - Request changes to master data (e.g., bank details, address).
    - Upload required documents (Attachments).
    - View request history.

### **Approver**
- **Purpose**: Internal business user responsible for validating vendor data.
- **Capabilities**:
    - **Send invitations to new vendors.**
    - **Manage all invitations (Resend/Revoke).**
    - View "Worklist" of pending tasks (Onboarding & Change Requests).
    - Review vendor applications in detail.
    - Approve or Reject requests with comments.
    - View history of past approvals.

### **Admin**
- **Purpose**: System administrator and process overseer.
- **Capabilities**:
    - Monitor system status and integrations.
    - Manage users and roles (implied).
    - Configure system rules and strategies (e.g., Branching Strategy view).

## 3. Core Functional Flows

### A. Vendor Onboarding (Invitation-Based)
1.  **Invitation**: **Approver** creates an invitation via `InviteVendorForm`.
    - Input: Vendor Name, Email, Notes.
    - System: Generates a unique, secure token and sends an email.
2.  **Registration**: Vendor clicks link (`/invitation/register/:token`).
    - System: Validates token expiry and status.
    - Vendor: Fills `VendorRegistration` form (Company Info, Tax ID, Contacts).
    - **Draft Mode**: Vendor can **"Save for Later"**.
        - State: `Draft`.
        - Data is persisted but validation is lenient. Vendor can return via token link.
3.  **Submission**: Vendor submits application.
    - System: Performs strict validation.
    - State: `Submitted`.
    - Notification: Approvers notified (via Email/System).
4.  **Approval**: Approver reviews via `OnboardingReview`.
    - Outcome: `Approved` or `Rejected`.
5.  **Integration**: On `Approved`, system syncs data to SAP.
    - Success: Vendor created in SAP. Application State -> `Integrated`.

### B. Change Management (Vendor-Initiated)
1.  **Request**: Vendor navigates to `ChangeRequestForm`.
    - **Screen**: `/requests/new`
    - **Steps**: Select category (Address/Bank/Contact) -> Enter changes -> Upload proof (if needed).
2.  **Submission**: Request saved.
3.  **Review**: Approver sees item in `RequestReview`.
    - **Screen**: `/approver/requests/:id`
    - **View**: Side-by-side comparison (Old vs New).
4.  **Decision**: Approver approves or rejects.

### C. Approver Operations
- **Invitation Management**: View list of all generated invitations.
    - **Screen**: `/approver/invitations`
    - **Columns**: Vendor Name, Email, Status, Expires At, Action (Resend/Revoke).

### D. System Administration
- **System Status**: View health of downstream services.
    - **Screen**: `/admin/system-status`

### E. Dashboards & Portals (Key Screens)

#### 1. Vendor Dashboard (`/dashboard`)
*   **Target User**: Vendor
*   **Purpose**: Overview of status and tasks.
*   **Current Elements**:
    *   **Profile Status Card**: Shows "Incomplete", "Submitted", or "Integrated".
    *   **Recent Requests**: List of last 5 change requests.
    *   **Notifications**: "You have pending tasks".

#### 2. Approver Dashboard (`/approver/worklist`)
*   **Target User**: Approver
*   **Purpose**: Triage pending items.
*   **Current Elements**:
    *   **Pending Onboardings**: Count of new vendor applications waiting Review.
    *   **Pending Changes**: Count of profile updates waiting Review.
    *   **Worklist Table**: Combined list sorted by SLA Due Date.

#### 3. Admin Dashboard (`/admin/dashboard`)
*   **Target User**: Admin
*   **Purpose**: High-level system oversight.
*   **Current Elements**:
    *   **Total Vendors**: Count of active vendors.
    *   **System Health**: Red/Green indicators for SAP & Email.

## 4. Data Model (Hybrid Architecure)
The system uses a **Hybrid Relational-Document Model** on Azure SQL and Cosmos DB.

### Key Entities (Azure SQL)
- **VendorInvitations**: Tracks the lifecycle of an invite (`Token`, `Status`, `ExpiresAt`).
- **VendorApplications**: The core vendor profile (`CompanyName`, `TaxId`, `Status`).
- **ChangeRequests**: Granular changes requested (`FieldName`, `OldValue`, `NewValue`).
- **UsersAndRoles**: Identity management (`Username`, `Role`).
- **Attachments**: Metadata for uploaded files stored in Blob Storage.
- **WorkflowStates**: Reference data for process states (`Draft`, `Submitted`, `Approved`).

### Semi-Structured Data (JSON Attributes)
All SQL tables include an `Attributes` (JSON) column to handle flexible/volatile data without schema migrations, such as:
- UI Preferences (Themes, Language).
- Campaign Metadata.
- Dynamic Form Fields.
- Formatting rules.

### Audit & Events (Cosmos DB)
- **DomainEvents**: Event sourcing store (e.g., `InvitationCreated`, `VendorApproved`).
- **InvitationArtifacts**: Immutable history of invitation payloads.

## 5. Services & Architecture

### Frontend (Static Web App)
- **Tech**: React + TypeScript + Vite.
- **Routing**: Role-based protected routes (`<ProtectedRoute>`).
- **Services**:
    - `authService`: Managed Identity / Mock Auth.
    - `vendorService`: CRUD for implementation.
    - `statusService`: Health checks.

### Backend (Azure App Service)
- **Tech**: .NET 8 API.
- **Key Services**:
    - **InvitationService**: Manages token generation (crypto), validation, and email dispatch.
    - **VendorService**: Core domain logic for applications and profile management.
    - **SapMapperService**: Critical component mapping VEN domain entities to strict SAP BAPI structures.
    - **ExternalSystemMappingService**: Handles canonical entity references and lookup codes.

### Infrastructure
- **Azure SQL**: Primary operational store (strong consistency).
- **Cosmos DB**: Audit trail (eventual consistency).
- **Service Bus**: Async event messaging (decoupling UI from SAP sync).
- **Key Vault**: Secret management (though Managed Identity is preferred).

## 6. Integration Points

### SAP Integration
- **Direction**: Outbound (Portal -> SAP).
- **Mechanism**: BAPI calls / IDOCs via Azure Functions/Logic Apps (implied by `ISapMapper`).
- **Trigger**: Vendor Approval, Change Request Approval.
- **Data**: Vendor Master Data (LFA1), Bank Details (LFBK), Contact Persons.

### Azure Services
- **Blob Storage**: Stores vendor artifacts (Certifications, Tax Docs).
- **Service Bus**: Decouples UI from heavy background processing (Email sending, SAP Sync).
- **Email Service**: SMTP / SendGrid for notifications.

## 7. Logs & Telemetry
- **Application Insights**: Distributed tracing and performance monitoring.
- **Cosmos DB (DomainEvents)**: Business-level audit logs for compliance.

## 8. Resource Impact & Dependency Framework

This section maps the relationships between entities, services, and external resources. **Use this matrix to assess the impact of any proposed change.**

### A. Entity Usage Matrix
| Entity | Primary API Service | Frontend Pages | Database Table | Impacted Roles | Downstream Impact |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **VendorInvitation** | `InvitationService` | `InviteVendorForm`<br>`InvitationRegistration` | `VendorInvitations` | **Approver**, Vendor | Email Service |
| **VendorApplication** | `VendorService` | `VendorRegistration`<br>`VendorProfile`<br>`OnboardingReview` | `VendorApplications` | Vendor, Approver | **SAP (Creation)**<br>Cosmos (Audit) |
| **ChangeRequest** | `VendorService` | `ChangeRequestForm`<br>`RequestReview` | `ChangeRequests` | Vendor, Approver | **SAP (Updates)** |
| **Attachment** | `BlobService`* | `VendorRegistration` | `Attachments` | Vendor | Blob Storage |
| **UserRole** | `UserService` | Global (Auth Context) | `UsersAndRoles` | All | Access Control |

### B. Critical Integration Dependencies
**Modifying these areas requires strict regression testing of the specific integration:**

1.  **Schema Changes in `VendorApplications`**:
    *   **High Risk**: Changing `TaxId`, `CompanyName`, or Address fields.
    *   **Impact**: Breaks `SapMapperService`. SAP sync will fail if BAPI constraints are violated.
    *   **Action**: Must update `SapMapperService` and verify SAP field length/format definitions.

2.  **Role Changes (`UsersAndRoles`)**:
    *   **High Risk**: Renaming roles (e.g., `Approver` -> `Reviewer`).
    *   **Impact**: Breaks `<ProtectedRoute>` in `App.tsx` and specific generic API authorization policies.
    *   **Action**: Grep codebase for string literal role names and update Enum definitions.

3.  **API Response Structure**:
    *   **Medium Risk**: Changing DTOs in `VendorService`.
    *   **Impact**: Breaks TypeScript interfaces in `frontend/src/types.ts`.
    *   **Action**: Regeneration of frontend API client or manual update of Types.

### C. Change Protocol
When proposing a modification, explicitly state:
1.  **Entities Touched**: (e.g., "Adding 'IBAN' to VendorApplication")
2.  **Schema Impact**: (e.g., "New column or JSON attribute?")
3.  **Service Impact**: (e.g., "Updates `VendorService`? Need to update `SapMapperService`?")
4.  **Role Impact**: (e.g., "Does this change who can see/approve this?")

## 9. Status Lifecycle Logic

This section defines the valid status transitions for core entities.

### A. Vendor Invitation (`VendorInvitations`)
*   **Pending**: Initial state on creation.
*   **Accepted**: Vendor has clicked the link and started/completed registration.
*   **Expired**: Token passed expiry date (14 days).
*   **Revoked**: Manually cancelled by Approver/Admin.

> **Transition Rule**: `Pending` -> (`Accepted` | `Expired` | `Revoked`)

### B. Vendor Application (`VendorApplications`)
1.  **Draft**: Vendor saved progress but has not submitted. (Can be edited indefinitely).
2.  **Submitted**: Vendor completed form. Locked for Vendor. Visible to Approver.
3.  **Under Review**: Approver has opened the application (optional state).
4.  **Rejected**: Sent back to Vendor for correction (or terminal rejection).
5.  **Approved**: Business approval granted. Queued for SAP Sync.
6.  **Integrated**: Successfully created in SAP. (Terminal State).

> **Transition Rule**: 
> `Draft` -> `Submitted`
> `Submitted` -> `Rejected` | `Approved`
> `Approved` -> `Integrated`

### C. Change Request (`ChangeRequests`)
Current state of a specific field change.
1.  **Draft**: Vendor is preparing the request.
2.  **Submitted**: Sent to Approver.
3.  **Approved**: Accepted by Approver.
4.  **Rejected**: Denied by Approver.
5.  **Failed**: Integration to SAP failed (requires retry).
6.  **Completed**: Synced to SAP.

> **Transition Rule**: `Draft` -> `Submitted` -> (`Approved` | `Rejected`) -> `Completed`
