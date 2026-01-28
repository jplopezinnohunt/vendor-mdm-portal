# Specification: Authentication Re-implementation & Database-Driven RBAC

## 1. Overview
This specification monitors the transition from hardcoded Group-to-Role mappings to a Database-Driven Role Based Access Control (RBAC) system. It also formalizes the User Management API to allow creating users and assigning roles dynamically.

**Goal**: Enable dynamic user management and role assignment without code changes, while maintaining testing impersonation capabilities.

## 2. Architecture & Standards Compliance
- **Standard**: Hybrid Relational-Document Model ([data-model-standards.md](../.agent/rules/standards/data-model-standards.md))
- **Entities**: 
  - `User` (Canonical Entity): Stores Identity + Role.
  - **Schema**:
    - `Id` (GUID, PK)
    - `Email` (Index, Unique)
    - `Role` (String, Structured Column) - See "Role Definitions" below.
    - `AzureAdObjectId` (String, Index, Nullable) - Links to Azure AD.
    - `AuthProvider` (String) - "Local", "AzureId".
    - `Data` (JSONB) - UI Preferences, Notifications, etc.

## 3. Role Definitions (Functional)
Based on Vendor MDM best practices:
1. **`Admin`**: Platform Administrator. Full access to System, Users, and Settings.
2. **`VendorAdmin`**: Data Steward. Manages Vendor Master Data, Cleaning, and Merging.
3. **`Requestor`**: Internal Business User. Can initiate Vendor Requests.
4. **`Approver`**: Finance/Compliance Officer. Can approve/reject workflow steps.
5. **`Viewer`**: Auditor. Read-only access to all data.

## 4. Authentication Flow (Hybrid Group Sync)
1.  **Login**: User logs in via Azure AD.
2.  **Token Analysis**: `DbClaimsTransformationService` inspects `groups` or `roles` claims.
3.  **Group Sync (Priority)**:
    - **Configuration**: Map Azure Group IDs to App Roles (e.g., `Values:AzureAd:Groups:Admin` -> `Admin`).
    - **Logic**: 
        - If User is in `Group_Admin` -> Force Role = `Admin`.
        - If User is in `Group_Approver` -> Force Role = `Approver`.
        - *Note*: If in multiple, Admin takes precedence.
4.  **Database Sync (Link-on-Login)**:
    - **Step A: Lookup**: Find User by `OID` or `Email`.
    - **Step B: Sync**:
        - If Group Match found: UPDATE `User.Role` = Mapped Role.
        - If NO Group Match: Keep existing `User.Role` (allows manual overrides).
        - Update `AzureAdObjectId` and `AuthProvider`.
    - **Step C**: Return Principal with the Final Role.

## 5. Impersonation
- `ImpersonationMiddleware` runs *after* Authentication.
- Checks for `X-Impersonate-User` cookie (Admin-set).
- Overrides Principal with impersonated claims.

## 4. Proposed Changes

### A. Data Model (`VendorMdm.Shared.Models.User`)
Ensure the `User` entity matches the standard.
- `Role` field is the source of truth for authorization.

### B. Service Layer (`DbClaimsTransformationService`)
Replace (or refactor) `ClaimsTransformationService.cs`.
- **Dependency**: `IServiceScopeFactory` (to resolve `SqlDbContext` scoped service inside singleton/transient transformer).
- **Logic**:
  ```csharp
  // Pseudo-code
  var email = principal.FindFirst(ClaimTypes.Email)?.Value;
  using var scope = _scopeFactory.CreateScope();
  var db = scope.ServiceProvider.GetRequiredService<SqlDbContext>();
  var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
  if (user != null) {
      identity.AddClaim(new Claim(ClaimTypes.Role, user.Role));
  }
  ```

### C. API Endpoints (`UserController`)
Enhance `UserController` to support Admin operations.
- `POST /api/users`
    - Creates user with specific Role.
    - Body: `{ "email": "...", "username": "...", "role": "Approver" }`
    - Protection: `[Authorize(Roles = "Admin")]`
- `PUT /api/users/{id}/role`
    - Updates user role.
    - Body: `{ "role": "Admin" }`
    - Protection: `[Authorize(Roles = "Admin")]`

## 5. Security & Verification
- **Testing Impersonation**: Must remain functional. `X-Mock-User` header (Dev) and `X-Impersonate-User` cookie (Admin) must still work.
- **Verification Plan**:
    1.  **Integration Test**: Create User via API -> Login with Mock Token (matching email) -> Verify Role is applied.
    2.  **Impersonation Test**: Use `Impersonate` endpoint -> Verify `GetProfile` returns impersonated role.

## 6. Migration
- Existing `UsersAndRoles` table in `SqlDbContext` should be deprecated or synchronized with `Users` canonical table. *Decision: Use `Users` Canonical Table exclusively.*
