# ⚠️ CRITICAL REMINDER: Re-enable Authentication

**Date Disabled:** 2025-12-11
**Reason:** Temporary disable for API testing without JWT tokens
**Status:** 🔴 AUTHENTICATION CURRENTLY DISABLED IN PRODUCTION

---

## Current Authentication Status

| Component | Status | Notes |
|-----------|--------|-------|
| Frontend MSAL | ✅ Implemented | Fully configured in `authConfig.ts` |
| Backend JWT Service | ✅ Implemented | `JwtAuthenticationService.cs` |
| Azure AD in Dev | ✅ Configured | Settings in `appsettings.Development.json` |
| Azure AD in Prod | ❌ DISABLED | App settings removed from Azure |
| Mock Auth (Dev) | ✅ Working | `MockAuthMiddleware.cs` |
| Impersonation | ✅ Working | `ImpersonationMiddleware.cs` |

---

## What Was Changed

Azure AD authentication was temporarily disabled by removing these app settings:
- `AzureAd__ClientId`: `2f2020ec-264d-4de5-bea4-f4dfc545c5d8`
- `AzureAd__TenantId`: `a93513e2-d327-4301-80ed-d703eb03f6cb`

**Resource:** `app-vendor-mdm-api-dev`
**Resource Group:** `rg-vendor-mdm-dev-v3`

---

## Security Impact

⚠️ **All protected API endpoints are now accessible without authentication!**

Affected endpoints (any with `[Authorize]` attribute):
- `POST /api/invitation/create` (AdminOrApprover)
- `GET /api/invitation/list` (AdminOrApprover)
- `POST /api/invitation/resend/{id}` (AdminOrApprover)
- `GET /api/vendors/*` (Various policies)
- `POST /api/vendors/*` (Various policies)

---

## Steps to Re-enable Authentication

### Step 1: Restore Azure AD App Settings

```bash
# For Development environment
az webapp config appsettings set \
  --resource-group rg-vendor-mdm-dev-v3 \
  --name app-vendor-mdm-api-dev \
  --settings \
    AzureAd__ClientId="2f2020ec-264d-4de5-bea4-f4dfc545c5d8" \
    AzureAd__TenantId="a93513e2-d327-4301-80ed-d703eb03f6cb" \
    AzureAd__Instance="https://login.microsoftonline.com/" \
    AzureAd__Domain="unesco.onmicrosoft.com"
```

```bash
# For Production environment (when ready)
az webapp config appsettings set \
  --resource-group rg-vendor-mdm-prod \
  --name app-vendor-mdm-api-prod \
  --settings \
    AzureAd__ClientId="<PROD_CLIENT_ID>" \
    AzureAd__TenantId="<PROD_TENANT_ID>" \
    AzureAd__Instance="https://login.microsoftonline.com/" \
    AzureAd__Domain="<PROD_DOMAIN>.onmicrosoft.com"
```

### Step 2: Configure JWT Secret (if using local JWT)

```bash
az webapp config appsettings set \
  --resource-group rg-vendor-mdm-dev-v3 \
  --name app-vendor-mdm-api-dev \
  --settings \
    Jwt__SecretKey="<GENERATE_256_BIT_SECRET>" \
    Jwt__Issuer="VendorMDM" \
    Jwt__Audience="VendorMDM"
```

### Step 3: Verify Frontend Configuration

Frontend auth config is already set up in:
- [authConfig.ts](../frontend/src/authConfig.ts) - MSAL configuration
- [AuthContext.tsx](../frontend/src/context/AuthContext.tsx) - Auth context with token acquisition
- [main.tsx](../frontend/src/main.tsx) - MSAL initialization

No frontend changes needed - the code is ready.

### Step 4: Test Authentication Flow

```bash
# Test that protected endpoints now require auth
curl -I https://app-vendor-mdm-api-dev.azurewebsites.net/api/vendors
# Expected: 401 Unauthorized

# Test with valid token
curl -H "Authorization: Bearer <valid-token>" \
  https://app-vendor-mdm-api-dev.azurewebsites.net/api/vendors
# Expected: 200 OK with data
```

### Step 5: Verification Checklist

- [ ] Azure AD settings restored via `az webapp config`
- [ ] User can log in via Azure AD (frontend)
- [ ] Token is acquired successfully (check browser DevTools)
- [ ] API calls include `Authorization: Bearer <token>` header
- [ ] Protected endpoints return data (not 401)
- [ ] Role-based access works:
  - [ ] Admin can access all endpoints
  - [ ] Approver can access approval endpoints
  - [ ] Requestor has limited access
- [ ] Impersonation still works for testing
- [ ] SignalR connection authenticated

### Step 6: Remove This File

Once authentication is re-enabled and verified, delete this reminder file:
```bash
rm docs/PENDING-AUTH-REENABLE.md
git add -A && git commit -m "chore: remove auth reminder - authentication re-enabled"
```

---

## Known Issues to Address

1. **Missing `/api/user/me` endpoint**: Frontend attempts to fetch user profile after login
   - Location: `AuthContext.tsx:151`
   - Fix: Add `GET /api/user/me` endpoint returning current user claims

2. **Microsoft Graph validation**: Token signature validated locally but not against Azure AD
   - Location: `JwtAuthenticationService.cs:275`
   - Risk: Medium - should validate tokens against Azure AD for production

3. **Password validation**: LocalStrong auth method uses TODO placeholder
   - Location: `UserRepository.cs:80-86`
   - Note: Only affects local auth, not Azure AD SSO

---

## Reference Files

| File | Purpose |
|------|---------|
| [Program.cs](../backend/VendorMdm.Api/Program.cs) | Auth middleware setup |
| [AuthController.cs](../backend/VendorMdm.Api/Controllers/AuthController.cs) | Auth endpoints |
| [JwtAuthenticationService.cs](../backend/VendorMdm.Core.Framework/Security/Authentication/JwtAuthenticationService.cs) | JWT implementation |
| [MockAuthMiddleware.cs](../backend/VendorMdm.Api/Middleware/MockAuthMiddleware.cs) | Development auth bypass |
| [authConfig.ts](../frontend/src/authConfig.ts) | Frontend MSAL config |
| [AuthContext.tsx](../frontend/src/context/AuthContext.tsx) | Frontend auth context |
