# Deployment Processes - MANDATORY RULES

## Azure Functions Deployment

**RULE**: Azure Functions are deployed using **Azure CLI**, NOT GitHub Actions.

### Correct Deployment Process

```bash
# 1. Navigate to the Function project
cd backend/VendorMdm.Artifacts  # or other function project

# 2. Build the project
dotnet build --configuration Release

# 3. Deploy using Azure Functions Core Tools
func azure functionapp publish <your-function-app-name>

# Example:
# func azure functionapp publish mdmportal-func-dev
```

### What NOT to Do
❌ Don't rely on `.github/workflows/azure-functions.yml`  
❌ Don't deploy via GitHub Actions  
❌ Don't use CI/CD for Azure Functions

### Why
- Azure Functions deployment requires specific runtime configuration
- Direct CLI deployment ensures proper settings
- GitHub Actions workflow adds unnecessary complexity

---

## Frontend (Static Web App) Deployment

**RULE**: Frontend deploys via **GitHub Actions** when Azure Static Web App is configured.

### Workflow File
`.github/workflows/azure-static-web-apps.yml`

### Status
🟡 Currently **disabled** - waiting for Azure Static Web App deployment

### When to Enable
1. Deploy Azure Static Web App via Bicep/Portal
2. Get deployment token from Azure
3. Add `AZURE_STATIC_WEB_APPS_API_TOKEN` secret to GitHub
4. Enable workflow (change `if: false` to `if: true`)

---

## API Backend Deployment

**RULE**: API backend (VendorMdm.Api) deploys to **Azure App Service** or **Azure Container Apps**.

### Deployment Options

**Option 1: Azure App Service (Simplest)**
```bash
# Publish to Azure App Service
cd backend/VendorMdm.Api
dotnet publish -c Release
az webapp deployment source config-zip --resource-group <rg> --name <app-service-name> --src ./bin/Release/net8.0/publish.zip
```

**Option 2: GitHub Actions** (not yet configured)
- Requires Azure App Service deployment credentials
- Workflow TBD

---

## Summary: Deployment Methods by Component

| Component | Method | Status |
|-----------|--------|--------|
| **Azure Functions** | ✅ Azure CLI (`func publish`) | Ready |
| **Frontend (SWA)** | GitHub Actions | 🟡 Disabled (waiting for Azure SWA) |
| **API Backend** | Azure CLI or GitHub Actions | TBD |
| **Database** | Azure Portal SQL Editor | ✅ Ready (`azure-sql-safe-migration.sql`) |

---

## Enforcement

**Before Deployment**:
1. ✅ Code pushed to GitHub (`main` branch)
2. ✅ Local build passes (0 errors)
3. ✅ Use correct deployment method per component
4. ✅ Verify deployment success in Azure Portal

**Never**:
- ❌ Deploy Azure Functions via GitHub Actions
- ❌ Deploy without testing locally first
- ❌ Deploy with failing builds
