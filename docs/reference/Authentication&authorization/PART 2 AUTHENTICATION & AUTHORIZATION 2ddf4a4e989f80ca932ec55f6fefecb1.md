# PART 2: AUTHENTICATION & AUTHORIZATION

### 2.1 UNESCO Single Sign-On (SSO) Architecture

**Authentication Method:** UNESCO Enterprise Single Sign-On (SSO)
**Identity Provider:** Microsoft Azure Active Directory (Azure AD)
**Protocol:** SAML 2.0 (primary) or OpenID Connect/OAuth 2.0
**Integration Type:** Federated authentication with UNESCO corporate directory

**Key Evidence:**

1. User display: “Connected as Julio Pablo Francisco Lopez” (full name from directory)
2. UNESCO logo links to: `https://unesco.sharepoint.com/sites/intranet`
3. Domain structure: `mouv-qas.hq.int.unesco.org` (.int.unesco.org = internal network)
4. “My Profile” dropdown suggests centralized profile management

### 2.2 Azure Active Directory Integration

**Azure AD Tenant:**

```
Tenant Name: UNESCO
Tenant Domain: unesco.onmicrosoft.com (or custom: unesco.org)
Tenant ID:
Tenant: unesco.onmicrosoft.com or unesco.org
Directory: UNESCO Global Directory
Sync: Azure AD Connect (on-prem AD → Azure AD)
User Attributes:
  - displayName: Full name
  - userPrincipalName: email@unesco.org
  - objectId: GUID
  - groups: AD group memberships
  - department, jobTitle, officeLocation
```

### 2.3 Authentication Flow

**Complete Flow:**

```
1. User → https://mouv-qas.hq.int.unesco.org/
2. Check authentication cookie
3. If not authenticated → Redirect to Azure AD
   https://login.microsoftonline.com/{tenant}/saml2
4. User enters credentials
5. Azure AD validates (Active Directory)
6. MFA prompt (if required by conditional access)
7. Azure AD issues SAML assertion
8. Redirect to MoUV with token
9. MoUV validates token signature
10. Create session, set cookie
11. Display: "Connected as [User Name]"
```

**SAML Configuration (Inferred):**

```xml
<EntityDescriptor entityID="https://mouv-qas.hq.int.unesco.org">
  <SPSSODescriptor>
    <AssertionConsumerService
      Binding="urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST"
      Location="https://mouv-qas.hq.int.unesco.org/auth/callback"/>
    <SingleLogoutService
      Binding="urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect"
      Location="https://mouv-qas.hq.int.unesco.org/auth/logout"/>
  </SPSSODescriptor>
</EntityDescriptor>
```

### 2.4 Session Management

**Cookie Configuration:**

```
Name: .AspNetCore.Cookies or ASP.NET_SessionId
Domain: .unesco.org or mouv-qas.hq.int.unesco.org
Path: /
Secure: true (HTTPS only)
HttpOnly: true
SameSite: Lax
Max-Age: 28800 seconds (8 hours)
```

**Session Store:**

- SQL Server or Redis for session state
- Session ID in cookie
- User claims stored server-side
- Timeout: 8 hours or end of business day

### 2.5 Role-Based Access Control (RBAC)

**User Roles:**

| Role | Permissions | Azure AD Group | Frontend Routes |
| --- | --- | --- | --- |
| **Vendor** | View/edit own profile, submit change requests | UNESCO-MoUV-Vendors | `/profile`, `/requests` |
| **Requestor** | Create/edit vendor requests, view own worklist | UNESCO-MoUV-Requestors | `/approver/worklist`, `/approver/history` |
| **Vendor Unit** | Approve vendor requests, view all submissions | UNESCO-MoUV-VendorUnit | `/approver/*` |
| **BFM** | High-value approvals, override rejections | UNESCO-MoUV-BFM | `/approver/*` |
| **Approver** | General approval authority | UNESCO-MoUV-Approvers | `/approver/*` |
| **Administrator** | Full system access, user management | UNESCO-MoUV-Admins | `/admin/*`, `/approver/*` |

> [!NOTE]
> **Frontend Implementation**: See [Frontend Authentication](./frontend-authentication.md) for details on how these roles are enforced in the React application using the `ProtectedRoute` component.

> [!NOTE]
> **Development/Testing**: Mock authentication is available for all roles. See [Frontend Authentication - Mock Login](./frontend-authentication.md#mock-authentication-development) for details.

**Claims-Based Authorization Example:**

```csharp
[Authorize(Roles = "Requestor,VendorUnit,BFM,Approver,Administrator")]
public class VendorController : Controller
{
    [Authorize(Roles = "VendorUnit,BFM,Approver,Administrator")]
    public IActionResult ApproveRequest(int id)
    {
        // Only approvers can access
    }
}
```

### 2.6 Multi-Factor Authentication (MFA)

**Azure AD Conditional Access Policies:**

**Policy 1: Location-Based**

```
IF location = Outside UNESCO network
THEN Require MFA
ELSE Allow (internal users)
```

**Policy 2: Device Compliance**

```
IF device NOT UNESCO-managed
THEN Require MFA + device registration
```

**Policy 3: Risk-Based**

```
IF anomalous sign-in detected
THEN Require MFA + admin notification
```

**MFA Methods:**

- Microsoft Authenticator
- SMS code
- Phone call
- Hardware token (YubiKey)

### 2.7 Security Features

**HTTPS Enforcement:**

```
TLS: 1.2 or 1.3
HSTS: Strict-Transport-Security: max-age=31536000
All HTTP → HTTPS redirect
```

**Session Security:**

- Inactivity timeout: 480 minutes (8 hours)
- Session fixation prevention
- New session ID after authentication
- Concurrent session handling

### 2.8 Authentication API Endpoints

**Login Initiation:**

```
GET /auth/login
→ 302 Redirect
Location: https://login.microsoftonline.com/{tenant}/oauth2/authorize?
  client_id={id}&
  redirect_uri=https://mouv-qas.hq.int.unesco.org/auth/callback&
  response_type=code&
  scope=openid profile email
```

**Callback:**

```
GET /auth/callback?code={auth-code}&state={token}

Process:
1. Validate state (CSRF protection)
2. Exchange code for access token
3. Validate token signature
4. Extract claims
5. Create session
6. Set cookie
7. Redirect to home
```

**Profile Retrieval:**

```
GET /api/auth/profile

Response:
{
  "userId": "guid",
  "displayName": "Julio Pablo Francisco Lopez",
  "email": "julio.lopez@unesco.org",
  "roles": ["Requestor", "VendorUnit"],
  "permissions": ["vendor.create", "vendor.approve"]
}
```

**Logout:**

```
POST /auth/logout

Process:
1. Clear session
2. Delete cookie
3. Redirect to Azure AD logout
   https://login.microsoftonline.com/{tenant}/oauth2/logout
```

### 2.9 SAP User Mapping

**Option 1: Technical User (Most Likely)**

```
MoUV User: julio.lopez@unesco.org
  ↓
SAP Technical User: MOUV_INTEGRATION
SAP Auth: FK01, XK01 (vendor creation)
```

**Option 2: Individual Mapping**

```
Azure AD: julio.lopez@unesco.org
  ↓
SAP User: JLOPEZ
SAP Roles: Z_VENDOR_CREATE, Z_VENDOR_MODIFY
```

**SAP Authority Check (ABAP):**

```abap
AUTHORITY-CHECK OBJECT 'F_LFA1_BUK'
  ID 'BUKRS' FIELD 'UNES'
  ID 'ACTVT' FIELD '01'.

IF sy-subrc <> 0.
  RAISE EXCEPTION TYPE cx_auth_failed.
ENDIF.
```

---