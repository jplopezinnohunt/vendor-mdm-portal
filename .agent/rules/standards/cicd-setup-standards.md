# Complete GitHub Actions Deployment Guide

## Overview

All deployment types are now automated via GitHub Actions with **manual triggers** for critical operations.

---

## Workflows Summary

| Workflow | Type | Trigger | Status |
|----------|------|---------|--------|
| `deploy-backend-api.yml` | Backend API | Auto (push to main) | ✅ Active |
| `azure-static-web-apps.yml` | Frontend | Auto (push to main) | ✅ Active |
| `deploy-database-migrations.yml` | Database | Manual | ✅ Active |
| `deploy-infrastructure.yml` | Infrastructure | Manual | ✅ Active |
| `azure-functions.yml` | Azure Functions | Manual | ✅ Active |

---

## Required GitHub Secrets

Add these at: https://github.com/jplopezinnohunt/vendor-mdm-portal/settings/secrets/actions

### 1. `AZURE_CREDENTIALS` (for all Azure deployments)
```bash
# Create service principal
az ad sp create-for-rbac \
  --name "github-actions-vendor-mdm" \
  --role contributor \
  --scopes /subscriptions/<SUBSCRIPTION_ID>/resourceGroups/rg-vendor-mdm-dev-v3 \
  --sdk-auth

# Copy the JSON output and add as secret
```

### 2. `AZURE_APP_SERVICE_PUBLISH_PROFILE` (for Backend API)
```bash
az webapp deployment list-publishing-profiles \
  --resource-group rg-vendor-mdm-dev-v3 \
  --name app-vendor-mdm-api-dev \
  --xml
```

### 3. `AZURE_STATIC_WEB_APPS_API_TOKEN` (for Frontend)
- Get from Azure Portal → Static Web Apps → Configuration → Deployment token

---

## Usage Instructions

### 1. Backend API (Automatic)
**File**: `deploy-backend-api.yml`  
**Trigger**: Push to `main` with backend code changes

```bash
# Just commit and push - deployment is automatic
git add backend/
git commit -m "feat: add new feature"
git push origin main

# Monitor at: https://github.com/jplopezinnohunt/vendor-mdm-portal/actions
```

---

### 2. Frontend (Automatic)
**File**: `azure-static-web-apps.yml`  
**Trigger**: Push to `main` with frontend code changes

```bash
# Just commit and push - deployment is automatic
git add frontend/
git commit -m "feat: update UI"
git push origin main

# Monitor at: https://github.com/jplopezinnohunt/vendor-mdm-portal/actions
```

---

### 3. Database Migrations (Manual Trigger)
**File**: `deploy-database-migrations.yml`  
**Trigger**: Manual via GitHub Actions UI

**Steps**:
1. Go to: https://github.com/jplopezinnohunt/vendor-mdm-portal/actions
2. Click "Deploy Database Migrations"
3. Click "Run workflow"
4. Select environment (dev/prod)
5. Click "Run workflow" button

**What it does**:
- Generates migration script from EF Core
- Patches for SQL Server (`TEXT` → `nvarchar(max)`)
- Applies to Azure SQL Database
- Shows preview before applying

---

### 4. Infrastructure (Manual Trigger)
**File**: `deploy-infrastructure.yml`  
**Trigger**: Manual via GitHub Actions UI

**Steps**:
1. Go to: https://github.com/jplopezinnohunt/vendor-mdm-portal/actions
2. Click "Deploy Azure Infrastructure"
3. Click "Run workflow"
4. Select environment (dev/prod)
5. Optionally specify Bicep file (default: main.bicep)
6. Click "Run workflow" button

**What it does**:
- Validates Bicep template
- Deploys to Azure Resource Group
- Shows deployment outputs
- Lists all resources created/updated

---

### 5. Azure Functions (Manual Trigger)
**File**: `azure-functions.yml`  
**Trigger**: Manual via GitHub Actions UI

**Steps**:
1. Go to: https://github.com/jplopezinnohunt/vendor-mdm-portal/actions
2. Click "Deploy Backend Artifacts"
3. Click "Run workflow"
4. Enter Function App name (e.g., `mdmportal-func-dev`)
5. Select environment (dev/prod)
6. Click "Run workflow" button

**What it does**:
- Builds Azure Functions project
- Publishes to specified Function App
- Deploys artifacts

---

## Quick Commands Reference

### Get Azure Credentials (One-time Setup)
```bash
# 1. Get your subscription ID
az account show --query id -o tsv

# 2. Create service principal
az ad sp create-for-rbac \
  --name "github-actions-vendor-mdm" \
  --role contributor \
  --scopes /subscriptions/<SUBSCRIPTION_ID>/resourceGroups/rg-vendor-mdm-dev-v3 \
  --sdk-auth

# 3. Copy JSON output → GitHub Secret: AZURE_CREDENTIALS
```

### Get App Service Publish Profile
```bash
az webapp deployment list-publishing-profiles \
  --resource-group rg-vendor-mdm-dev-v3 \
  --name app-vendor-mdm-api-dev \
  --xml

# Copy XML output → GitHub Secret: AZURE_APP_SERVICE_PUBLISH_PROFILE
```

### View Workflow Runs
```bash
# Open in browser
open https://github.com/jplopezinnohunt/vendor-mdm-portal/actions

# Or use GitHub CLI
gh run list --limit 10
gh run view <run-id>
```

---

## Deployment Decision Tree

```
┌─ Code Change?
│
├─ Backend Code → Push to main → Auto-deploys
│
├─ Frontend Code → Push to main → Auto-deploys
│
├─ Database Schema → Manual workflow → Trigger "Deploy Database Migrations"
│
├─ Infrastructure (Bicep) → Manual workflow → Trigger "Deploy Azure Infrastructure"
│
└─ Azure Functions → Manual workflow → Trigger "Deploy Backend Artifacts"
```

---

## Safety Features

### All Workflows:
- ✅ Use Azure service principal (least privilege)
- ✅ Require GitHub secrets (encrypted)
- ✅ Show deployment logs
- ✅ Support rollback (revert commit)

### Manual Workflows (DB/Infra/Functions):
- ✅ Require explicit trigger (no auto-deploy)
- ✅ Environment selection (dev/prod)
- ✅ Validation before deployment
- ✅ Deployment preview/verification

---

## Monitoring

### View All Deployments
https://github.com/jplopezinnohunt/vendor-mdm-portal/actions

### Workflow Status
- ✅ Green = Success
- ❌ Red = Failed
- 🟡 Yellow = In progress
- ⚪ Gray = Queued

### Get Email Notifications
GitHub → Settings → Notifications → Actions → Enable notifications for failed workflows

---

## Rollback Procedures

### Backend/Frontend Code
```bash
# Revert the commit
git revert <commit-sha>
git push origin main

# GitHub Actions will auto-deploy the reverted version
```

### Database Migration
```bash
# Manually rollback via Azure Portal SQL Query Editor
# Or trigger workflow with previous migration
```

### Infrastructure
```bash
# Revert Bicep changes
git revert <commit-sha>

# Manually trigger "Deploy Azure Infrastructure" workflow
```

---

## 7. Post-Deployment Verification (MANDATORY)

**CRITICAL**: Every deployment MUST be verified before considering it complete.

### Verification Steps

**1. Wait for Deployment** (5-10 minutes)
- Monitor GitHub Actions workflow
- Wait for "Success" status
- Check deployment logs for errors

**2. Run Verification Script**:
```bash
./scripts/verify-deployment.sh
# Expected: ✓ DEPLOYMENT VERIFIED
```

**3. Manual Checks**:
- [ ] Backend Swagger accessible
- [ ] Frontend loads
- [ ] Login works
- [ ] Critical features functional
- [ ] No console errors

**4. Smoke Tests**:
```bash
# Backend health
curl https://app-vendor-mdm-api-dev.azurewebsites.net/swagger

# Frontend
curl https://thankful-field-0258f8110.3.azurestaticapps.net/

# Critical endpoint
curl -H "Authorization: Bearer $TOKEN" \
  https://app-vendor-mdm-api-dev.azurewebsites.net/api/vendors
```

### Rollback Plan

**If Verification Fails**:
1. **Immediate**: Revert commit
   ```bash
   git revert <commit-sha>
   git push origin main
   ```

2. **Investigate**: Check logs
   - GitHub Actions logs
   - Azure App Service logs
   - Browser console errors

3. **Fix**: Address issue locally
   - Test thoroughly
   - Verify alignment
   - Redeploy

4. **Document**: Create issue
   - What failed
   - Why it failed
   - How it was fixed

### Agent Behavior

**After Merge to Main**:
1. ✅ Wait for deployment (monitor GitHub Actions)
2. ✅ Run `verify-deployment.sh`
3. ✅ Report deployment status to user
4. ✅ Suggest rollback if verification fails
5. ✅ Document any issues found

**Verification Checklist**:
- [ ] GitHub Actions shows "Success"
- [ ] Backend API responds (200 OK)
- [ ] Frontend loads (200 OK)
- [ ] Swagger UI accessible
- [ ] No 500 errors in logs
- [ ] Critical features work

**Failure Response**:
- Agent MUST alert user immediately
- Agent MUST suggest rollback
- Agent MUST provide error details
- Agent MUST NOT proceed with other work until resolved

---

## 8. Environment Alignment (MANDATORY)

**CRITICAL**: Every deployment MUST maintain alignment between frontend origins, backend CORS, and environment variables. See [deployment-environment-standard.md](deployment-environment-standard.md) for the full standard.

### Key Rules

1. **ASPNETCORE_ENVIRONMENT**: Must be set explicitly in every App Service deployment via `azure/appservice-settings@v1` (never rely on Azure defaults)
2. **CORS Origins**: The Origin Registry in [deployment-environment-standard.md](deployment-environment-standard.md) is the source of truth. Every SWA URL MUST be in the backend CORS config.
3. **Frontend Env Vars**: `.env.production` MUST set both `VITE_API_BASE_URL` (with `/api`) and `VITE_API_URL` (without `/api`)
4. **Post-Deployment Verification**: Every backend deployment MUST include CORS preflight + SignalR negotiate checks

### When Adding New Environments

1. Update Origin Registry in `deployment-environment-standard.md`
2. Add CORS origin in `Program.cs` → `GetAllowedOrigins()`
3. Add App Settings step in `deploy-backend-api.yml`
4. Create/update `.env.[environment]` in frontend

---

## Next Steps

1. ✅ Add all required secrets to GitHub
2. ✅ Test Backend API deployment (push small change)
3. ✅ Test Frontend deployment (push small change)
4. ✅ Test manual workflows (Database/Infra/Functions)
5. ✅ Enable branch protection rules (require CI to pass)

---

**All workflows created! Ready for automated deployments.**
