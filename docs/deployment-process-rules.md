# Deployment Processes - MANDATORY RULES

## PRIMARY RULE: All Azure Resources Deploy via Azure CLI

**MANDATORY**: All Azure resources are deployed using **Azure CLI**, NOT GitHub Actions.

### Why This Rule Exists
- ✅ Consistent deployment methodology
- ✅ Direct control over Azure resources
- ✅ No dependency on GitHub Actions secrets
- ✅ Simpler debugging and troubleshooting
- ✅ Works from any environment (local, CI/CD, Azure Portal)

### What This Means
- ❌ No GitHub Actions for Azure resource deployment
- ❌ No automated CI/CD deployments to Azure
- ✅ Use `az` CLI commands for all Azure operations
- ✅ Use `func` CLI for Azure Functions
- ✅ Manual, controlled deployments

---

## Deployment Methods by Resource Type

### 1. Azure Functions
**Method**: Azure Functions Core Tools CLI

```bash
cd backend/VendorMdm.Artifacts  # or other function project
dotnet build --configuration Release
func azure functionapp publish <function-app-name>

# Example:
# func azure functionapp publish mdmportal-func-dev
```


### 2. API Backend (App Service / Container Apps)
**Method**: Azure CLI

```bash
# Build and publish
cd backend/VendorMdm.Api
dotnet publish -c Release -o ./publish

# Deploy to Azure App Service
az webapp deployment source config-zip \
  --resource-group <resource-group> \
  --name <app-service-name> \
  --src ./publish.zip

# OR deploy to Azure Container Apps (if using containers)
az containerapp update \
  --name <container-app-name> \
  --resource-group <resource-group> \
  --image <your-image>
```

### 3. Static Web App (Frontend)
**Method**: Azure CLI (Static Web Apps CLI)

```bash
cd frontend
npm run build

# Deploy using Static Web Apps CLI
swa deploy ./dist \
  --app-name <swa-name> \
  --resource-group <resource-group>
```

**Note**: GitHub Actions workflow (`.github/workflows/azure-static-web-apps.yml`) is **disabled** - not used.

### 4. Azure SQL Database
**Method**: Azure Portal SQL Query Editor OR Azure CLI

```bash
# Apply migration script
az sql db query \
  --server <server-name> \
  --database <database-name> \
  --auth-mode ActiveDirectoryIntegrated \
  --input-file ./docs/azure-sql-safe-migration.sql
```

**OR** use Azure Portal Query Editor (easier for SQL scripts).

### 5. Infrastructure (Bicep/ARM)
**Method**: Azure CLI

```bash
# Deploy infrastructure templates
az deployment group create \
  --resource-group <resource-group> \
  --template-file ./infrastructure/main.bicep \
  --parameters environmentName=dev
```

---

## GitHub Actions: What They're Used For

**NOT for Azure deployment** - Only for:
- ✅ Code quality checks (future)
- ✅ Running tests (future)
- ✅ Linting (future)

**Currently**: All GitHub Actions workflows for Azure deployment are **disabled**.

---

## Summary Table

| Component | Deployment Method | GitHub Actions? |
|-----------|-------------------|-----------------|
| **Azure Functions** | `func publish` | ❌ Disabled |
| **API Backend** | `az webapp deployment` | ❌ Not used |
| **Frontend (SWA)** | `swa deploy` OR Azure CLI | ❌ Disabled |
| **Database** | Azure Portal OR `az sql db query` | ❌ Not applicable |
| **Infrastructure** | `az deployment group create` | ❌ Not used |

---

## Enforcement

**Before ANY Azure Deployment**:
1. ✅ Code pushed to GitHub (`main` branch)
2. ✅ Local build passes (0 errors)
3. ✅ Use Azure CLI commands (per table above)
4. ✅ Verify in Azure Portal after deployment

**Never**:
- ❌ Deploy via GitHub Actions
- ❌ Rely on CI/CD for Azure resources
- ❌ Deploy without local build verification

