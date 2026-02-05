# Deployment Processes - MANDATORY RULES (Updated 2024-12-18)

## PRIMARY RULE: Hybrid GitHub Actions + Azure CLI Strategy

**New Strategy**: Use GitHub Actions for code deployments (automated), Azure CLI for critical operations (manual).

---

## Deployment Methods by Component

| Component | Method | Trigger | Rationale |
|-----------|--------|---------|-----------|
| **Backend API Code** | GitHub Actions | Auto (push to main) | Code changes benefit from automation |
| **Frontend (SWA)** | ✅ Automated | GitHub Actions | Push to `main` |
| **Backend API** | ✅ Automated | GitHub Actions | Push to `main` |
| **Database (SQL)** | ✅ **Automated** | GitHub Actions (`dotnet ef`) | Push to `main` |
| **Infrastructure** | ⚠️ Manual | Azure CLI (`bicep`) | User Trigger |
| **Azure Functions** | ⚠️ Manual | Azure CLI (`func check`) | User Trigger |

---

## GitHub Actions Workflows

### 1. Backend API Deployment (Automatic)
**File**: `.github/workflows/deploy-backend-api.yml`  
**Status**: ✅ **ACTIVE**  
**Trigger**: Push to `main` with changes in `backend/VendorMdm.Api/**` or `backend/VendorMdm.Shared/**`

**What it does**:
1. Builds .NET application (Release)
2. Publishes to `./publish`
3. Deploys to Azure App Service using publish profile

**Secret required**: `AZURE_APP_SERVICE_PUBLISH_PROFILE`

```bash
# Get secret value:
az webapp deployment list-publishing-profiles \
  --resource-group rg-vendor-mdm-dev-v3 \
  --name app-vendor-mdm-api-dev \
  --xml
```

---

### 2. Frontend (Static Web App) Deployment (Automatic)
**File**: `.github/workflows/azure-static-web-apps.yml`  
**Status**: ✅ **ACTIVE**  
**Trigger**: Push to `main` with changes in `frontend/**`

**What it does**:
1. Builds React/Vite application
2. Deploys to Azure Static Web Apps

**Secret required**: `AZURE_STATIC_WEB_APPS_API_TOKEN`

---

### 3. Database Migrations (Automated - Environment Safe)
**File**: `.github/workflows/deploy-database-migrations.yml`  
**Status**: ✅ **ACTIVE**  
**Trigger**: Push to `main` OR Manual Dispatch

**What it does**:
1. Generates migration script from EF Core
2. Patches for SQL Server (`TEXT` → `nvarchar(max)`)
3. **Automated**: Defaults to `dev` environment on push
4. **Manual**: Allows targeting `prod` with explicit approval
5. Applies to Azure SQL Database safely

**Secret required**: `AZURE_CREDENTIALS`

**Triggers**:
- **Auto**: Push changes to `backend/VendorMdm.Api/Migrations/**` to `main`
- **Manual**: Go to Actions → Deploy Database Migrations → Run workflow

---

### 4. Infrastructure Deployment (Manual Trigger)
**File**: `.github/workflows/deploy-infrastructure.yml`  
**Status**: ✅ **ACTIVE**  
**Trigger**: Manual via GitHub Actions UI

**What it does**:
1. Validates Bicep templates
2. Deploys to Azure Resource Group
3. Shows deployment outputs

**Secret required**: `AZURE_CREDENTIALS`

**How to trigger**:
1. Go to: https://github.com/jplopezinnohunt/vendor-mdm-portal/actions
2. Click "Deploy Azure Infrastructure"
3. Click "Run workflow" → Select environment + Bicep file

---

### 5. Azure Functions Deployment (Manual Trigger)
**File**: `.github/workflows/azure-functions.yml`  
**Status**: ✅ **ACTIVE**  
**Trigger**: Manual via GitHub Actions UI

**What it does**:
1. Builds Azure Functions project
2. Deploys to specified Function App

**Secret required**: `AZURE_CREDENTIALS`

**How to trigger**:
1. Go to: https://github.com/jplopezinnohunt/vendor-mdm-portal/actions
2. Click "Deploy Backend Artifacts"
3. Click "Run workflow" → Enter Function App name + environment

---

## Required GitHub Secrets

Add at: https://github.com/jplopezinnohunt/vendor-mdm-portal/settings/secrets/actions

### `AZURE_CREDENTIALS`
**Used by**: Database migrations, Infrastructure, Azure Functions

```bash
az ad sp create-for-rbac \
  --name "github-actions-vendor-mdm" \
  --role contributor \
  --scopes /subscriptions/<SUBSCRIPTION_ID>/resourceGroups/rg-vendor-mdm-dev-v3 \
  --sdk-auth
```

### `AZURE_APP_SERVICE_PUBLISH_PROFILE`
**Used by**: Backend API deployment

```bash
az webapp deployment list-publishing-profiles \
  --resource-group rg-vendor-mdm-dev-v3 \
  --name app-vendor-mdm-api-dev \
  --xml
```

### `AZURE_STATIC_WEB_APPS_API_TOKEN`
**Used by**: Frontend deployment

Get from: Azure Portal → Static Web Apps → Configuration → Deployment token

---

## Deployment Workflow

### For Code Changes (Automatic)
```bash
# 1. Make changes
git add backend/VendorMdm.Api  # or frontend/

# 2. Commit
git commit -m "feat: your feature"

# 3. Push to main
git push origin main

# 4. GitHub Actions auto-deploys
# Monitor: https://github.com/jplopezinnohunt/vendor-mdm-portal/actions
```

### For Database Migrations (Manual)
1. Ensure code with migrations is pushed to `main`
2. Go to GitHub Actions UI
3. Trigger "Deploy Database Migrations" workflow
4. Select environment (dev/prod)
5. Monitor execution
6. Verify in Azure Portal SQL Query Editor

### For Infrastructure Changes (Manual)
1. Update Bicep files and push to `main`
2. Go to GitHub Actions UI
3. Trigger "Deploy Azure Infrastructure" workflow
4. Select environment and Bicep file
5. Monitor execution
6. Verify resources in Azure Portal

---

## Summary Table

| Component | Deployment Method | GitHub Actions? | Manual Approval? |
|-----------|-------------------|-----------------|------------------|
| **Backend Code** | GitHub Actions | ✅ Auto | ❌ No |
| **Frontend Code** | GitHub Actions | ✅ Auto | ❌ No |
| **Database Schema** | GitHub Actions | ✅ Auto | ❌ No |
| **Infrastructure** | GitHub Actions | ✅ Manual trigger | ✅ Yes |
| **Azure Functions** | GitHub Actions | ✅ Manual trigger | ✅ Yes |

---

## Enforcement

**Before ANY Deployment**:
1. ✅ Code pushed to GitHub (`main` branch)
2. ✅ Local build passes (0 errors)
3. ✅ All required GitHub secrets configured
4. ✅ For manual workflows: Trigger via GitHub Actions UI
5. ✅ Monitor workflow execution
6. ✅ Verify in Azure Portal after deployment

**After Deployment**:
1. ✅ Workflow shows green checkmark
2. ✅ Verify resources in Azure Portal
3. ✅ Run validation tests
4. ✅ Check Application Insights for errors

**Never**:
- ❌ Deploy manually via Azure CLI for code (use GitHub Actions)
- ❌ Skip workflow monitoring
- ❌ Deploy to production without testing in dev first
- ❌ Ignore failed workflows

---

## Rollback Procedures

### Code Deployments (Backend/Frontend)
```bash
# Revert commit
git revert <commit-sha>
git push origin main

# GitHub Actions auto-deploys reverted version
```

### Database Migrations
```bash
# Option 1: Trigger workflow with previous migration
# Option 2: Manual rollback via Azure Portal SQL Query Editor
```

### Infrastructure
```bash
# Revert Bicep changes
git revert <commit-sha>

# Manually trigger "Deploy Azure Infrastructure" workflow
```

---

**Updated**: 2024-12-18  
**Reason**: Implemented comprehensive GitHub Actions strategy for all deployment types  
**Old Method**: Fully manual Azure CLI  
**New Method**: Hybrid (GitHub Actions for automation, manual triggers for critical operations)
