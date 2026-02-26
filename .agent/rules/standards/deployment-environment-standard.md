# Deployment Environment Alignment Standard

**Category**: Integration & Infrastructure
**Pattern #**: 35
**Status**: MANDATORY
**Priority**: 🔴 CRITICAL

---

## Definition

All deployed environments MUST have explicit, verified alignment between frontend origin URLs, backend CORS configuration, environment variables, and ASP.NET Core environment settings. A mismatch in any of these causes silent runtime failures (CORS blocks, SignalR 405, auth failures) that are invisible to build/test CI.

---

## Rules

1. **ALWAYS** set `ASPNETCORE_ENVIRONMENT` explicitly in every Azure App Service deployment (never rely on defaults)
2. **ALWAYS** include the actual SWA origin URL in the backend CORS allowed origins for the matching environment
3. **ALWAYS** ensure frontend env vars (`VITE_API_BASE_URL`, `VITE_API_URL`) are consistent and both point to the same backend
4. **NEVER** use relative API paths (`/api`, `/hubs`) in production builds when frontend and backend are on different origins
5. **NEVER** add SWA route rules for `/api/*` when using an external backend (SWA managed APIs only)
6. **ALWAYS** run CORS preflight + SignalR negotiate verification as post-deployment CI steps
7. **ALWAYS** keep the Origin Registry (below) updated when SWA or App Service URLs change

---

## Origin Registry (Source of Truth)

| Environment | Frontend (SWA) | Backend (App Service) | Resource Group | SKU | ASPNETCORE_ENVIRONMENT |
|-------------|----------------|-----------------------|----------------|-----|------------------------|
| **Development** | `https://thankful-field-0258f8110.3.azurestaticapps.net` | `https://app-vendor-mdm-api-dev.azurewebsites.net` | `rg-vendor-mdm-dev` | B1 | `Development` |
| **Staging** | `https://purple-moss-066604e03.4.azurestaticapps.net` | TBD | TBD | B1+ | `Staging` |
| **Production** | `https://victorious-water-095da360f.5.azurestaticapps.net` | `https://app-vendor-mdm-api-prod.azurewebsites.net` | `rg-vendor-mdm-prod` | S1+ | `Production` |
| **Local** | `http://localhost:3000` | `http://localhost:5001` | N/A | N/A | `Development` |

---

## Implementation

### 1. Backend CORS Must Match Origin Registry

```csharp
// Program.cs - GetAllowedOrigins()
// EVERY SWA URL in the Origin Registry MUST appear in the matching environment block
// Example: Development environment
return new[]
{
    "http://localhost:3000",
    "https://thankful-field-0258f8110.3.azurestaticapps.net",  // MUST match registry
    // ...
};
```

### 2. Frontend Environment Variables Must Be Consistent

```bash
# .env.production - BOTH variables MUST point to same backend
VITE_API_BASE_URL=https://app-vendor-mdm-api-dev.azurewebsites.net/api    # With /api suffix
VITE_API_URL=https://app-vendor-mdm-api-dev.azurewebsites.net              # Without /api suffix
```

**Why two variables?**
- `VITE_API_BASE_URL` = Used by `api.ts` (axios baseURL, includes `/api`)
- `VITE_API_URL` = Used by `SignalRContext.tsx` (hub URL base, no `/api` because hub path is `/hubs/events`)

### 3. CI/CD Must Set Environment Explicitly

```yaml
# deploy-backend-api.yml - Dev deployment
- name: Configure App Settings (Dev)
  uses: azure/appservice-settings@v1
  with:
    app-name: app-vendor-mdm-api-dev
    app-settings-json: |
      [
        { "name": "ASPNETCORE_ENVIRONMENT", "value": "Development" },
        { "name": "App__BaseUrl", "value": "https://thankful-field-0258f8110.3.azurestaticapps.net" }
      ]
```

### 4. SWA Config Must Not Intercept External API Routes

```json
// staticwebapp.config.json
// DO NOT add route rules for /api/* when using external backend
// SWA route rules only apply to SWA-managed APIs
{
  "routes": [
    // NO /api/* rule here - API calls go directly to external backend
  ]
}
```

### 5. Post-Deployment Verification (MANDATORY)

```yaml
# MUST be in every backend deployment workflow
- name: Verify CORS and SignalR
  run: |
    APP_URL="https://app-vendor-mdm-api-dev.azurewebsites.net"
    SWA_ORIGIN="https://thankful-field-0258f8110.3.azurestaticapps.net"

    # CORS preflight check
    curl -X OPTIONS "$APP_URL/api/health" \
      -H "Origin: $SWA_ORIGIN" \
      -H "Access-Control-Request-Method: GET"

    # SignalR negotiate check
    curl -X POST "$APP_URL/hubs/events/negotiate?negotiateVersion=1&mockUser=Requestor" \
      -H "Origin: $SWA_ORIGIN"
```

---

## Failure Modes (Learned)

### FM-1: CORS 403/blocked preflight
**Symptom**: `Access-Control-Allow-Origin header is not present`
**Cause**: Frontend SWA URL not in backend `GetAllowedOrigins()` for the active `ASPNETCORE_ENVIRONMENT`
**Fix**: Add the exact SWA URL to the correct environment block in `Program.cs`

### FM-2: SignalR negotiate 405
**Symptom**: `Failed to complete negotiation with the server: Status code '405'`
**Cause A**: `VITE_API_URL` not set, so SignalR connects to SWA origin (relative `/hubs/events`), not the backend
**Cause B**: `ASPNETCORE_ENVIRONMENT` defaults to Production on Azure, so MockAuthMiddleware is not registered, and `[Authorize]` on EventHub rejects unauthenticated negotiate
**Fix**: Set both `VITE_API_URL` in `.env.production` AND `ASPNETCORE_ENVIRONMENT=Development` in App Service settings

### FM-3: SWA intercepting API requests
**Symptom**: API calls return 401 before reaching the backend
**Cause**: `staticwebapp.config.json` has `/api/*` route with `allowedRoles: ["authenticated"]`
**Fix**: Remove `/api/*` route rules when using an external backend

### FM-4: App Service Disabled / Site Disabled (403)
**Symptom**: `Deployment Failed, Error: Site Disabled (CODE: 403)`
**Cause**: Azure App Service is stopped/disabled (cost management, manual stop, or Azure auto-stop on free/dev tiers)
**Fix**: Workflow MUST check App Service state and start it before deploying. Use `az webapp show --query "state"` and `az webapp start` if not Running.

### FM-5: QuotaExceeded on Free (F1) tier
**Symptom**: App Service state is `QuotaExceeded`, `az webapp start` succeeds but deploy still fails with `Site Disabled (CODE: 403)`
**Cause**: Azure Free (F1) tier has a daily CPU quota (60 min/day). Once exhausted, the app stops and cannot be restarted until quota resets at UTC midnight.
**Fix**: Scale the App Service Plan to Basic (B1) or higher: `az appservice plan update --name <plan> --resource-group <rg> --sku B1`
**Prevention**: Dev environments SHOULD use B1 tier minimum. F1 is insufficient for any real deployment.

### FM-6: ResourceGroupNotFound when starting App Service
**Symptom**: `Resource group 'rg-vendor-mdm-dev-v3' could not be found`
**Cause**: Hardcoded resource group name in workflow doesn't match actual Azure resource group
**Fix**: Auto-discover the resource group using `az webapp list --query "[?name=='$APP_NAME'].resourceGroup"` instead of hardcoding it

---

## Anti-Patterns

- Relying on Azure App Service default environment (it defaults to `Production`)
- Using different env var names for the same backend URL (`VITE_API_URL` vs `VITE_API_BASE_URL`)
- Adding SWA route rules for paths that should reach an external backend
- Hardcoding CORS origins in only one environment block when the same SWA is used across environments
- Deploying backend without post-deployment CORS/SignalR verification
- Assuming a passing health check means the app works (health check doesn't test CORS or auth)
- Deploying to an App Service without first checking if it is running

---

## Agent Behavior

**Before Any Deployment Change**:
1. Read this standard's Origin Registry
2. Verify the target SWA URL exists in the backend CORS config for the matching environment
3. Verify `.env.production` has both `VITE_API_BASE_URL` and `VITE_API_URL` pointing to the correct backend
4. Verify `ASPNETCORE_ENVIRONMENT` is set explicitly in the workflow

**When Adding a New Environment or SWA**:
1. Update the Origin Registry table in this standard
2. Add the new origin to `GetAllowedOrigins()` in `Program.cs`
3. Add the new environment settings to `deploy-backend-api.yml`
4. Create or update the corresponding `.env.[environment]` file

**When Debugging Runtime Errors**:
1. Check browser console for CORS errors → FM-1
2. Check SignalR negotiate status code → FM-2
3. Check if API calls return 401 before hitting backend → FM-3

---

## Checklist (Pre-Deploy)

- [ ] Origin Registry is current
- [ ] Backend CORS includes all SWA origins for active environment
- [ ] `ASPNETCORE_ENVIRONMENT` is explicitly set in workflow
- [ ] `.env.production` has both `VITE_API_BASE_URL` and `VITE_API_URL`
- [ ] `staticwebapp.config.json` does NOT have `/api/*` route rules
- [ ] Post-deployment CORS + SignalR verification step exists in workflow

---

## Reference

- **Implementation**: `backend/VendorMdm.Api/Program.cs` (lines 267-321)
- **Frontend Env**: `frontend/.env.production`
- **SWA Config**: `frontend/staticwebapp.config.json`
- **Deploy Workflow**: `.github/workflows/deploy-backend-api.yml`
- **SignalR Context**: `frontend/src/context/SignalRContext.tsx`
- **Related Standard**: [cicd-setup-standards.md](cicd-setup-standards.md)
- **Related Standard**: [security-architecture.md](security-architecture.md)
- **Golden Rules**: Section 5, Category 4
- **Incident Date**: 2026-02-26 (CORS + SignalR 405 + SWA interception)
