# ⚠️ CRITICAL REMINDER: Re-enable Authentication

**Date Disabled:** 2025-12-11  
**Reason:** Temporary disable for API testing without JWT tokens  
**Status:** 🔴 AUTHENTICATION CURRENTLY DISABLED IN AZURE

## What Was Changed

Azure AD authentication was temporarily disabled by removing these app settings:
- `AzureAd__ClientId`: `2f2020ec-264d-4de5-bea4-f4dfc545c5d8`
- `AzureAd__TenantId`: `a93513e2-d327-4301-80ed-d703eb03f6cb`

**Resource:** `app-vendor-mdm-api-dev-fmkijlt6yfyeq`  
**Resource Group:** `rg-vendor-mdm-dev-v3`

## Security Impact

⚠️ **All protected API endpoints are now accessible without authentication!**

Affected endpoints:
- `POST /api/invitation/create` (AdminOrApprover)
- `GET /api/invitation/list` (AdminOrApprover)
- `POST /api/invitation/resend/{id}` (AdminOrApprover)
- Any other endpoints with `[Authorize]` attributes

## Steps to Re-enable Authentication

### 1. Restore Azure AD Configuration

```bash
az webapp config appsettings set \
  --resource-group rg-vendor-mdm-dev-v3 \
  --name app-vendor-mdm-api-dev-fmkijlt6yfyeq \
  --settings \
    AzureAd__ClientId="2f2020ec-264d-4de5-bea4-f4dfc545c5d8" \
    AzureAd__TenantId="a93513e2-d327-4301-80ed-d703eb03f6cb" \
    AzureAd__Instance="https://login.microsoftonline.com/" \
    AzureAd__Domain="<your-domain>.onmicrosoft.com"
```

### 2. Configure Frontend Authentication

Update frontend to:
- Initialize MSAL (Microsoft Authentication Library)
- Acquire access tokens for API calls
- Include token in Authorization header: `Bearer <token>`

### 3. Test Authentication Flow

- [ ] User can log in via Azure AD
- [ ] Token is acquired successfully
- [ ] API calls include Authorization header
- [ ] Protected endpoints return data (not 401)
- [ ] Role-based access works (Admin/Approver)

### 4. Remove This File

Once authentication is re-enabled and tested, delete this reminder file.

## Reference

- **Code:** [`Program.cs`](backend/VendorMdm.Api/Program.cs) lines 46-76
- **Controllers:** All controllers in `backend/VendorMdm.Api/Controllers/`
- **Auth Policy:** `AdminOrApprover` requires roles: Admin, Approver
