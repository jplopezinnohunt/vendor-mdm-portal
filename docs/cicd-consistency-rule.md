# CI/CD Pipeline Consistency - MANDATORY RULE

## Core Principle
**All code pushed to `main` must result in passing CI/CD pipelines.**

## Critical Deployment Rule: Azure Functions

**Azure Functions are deployed via Azure CLI, NOT GitHub Actions.**

### Deployment Process:
```bash
# Azure Functions deployment
cd backend/VendorMdm.Artifacts  # or relevant function project
func azure functionapp publish <function-app-name>
```

**GitHub Workflow**: `.github/workflows/azure-functions.yml` is **DISABLED** - not used for deployment.

---

## The Problem
GitHub workflows were failing because:
- ❌ `azure-functions.yml` tried to deploy Functions (wrong - use Azure CLI instead)
- ❌ `azure-static-web-apps.yml` requires Azure Static Web App (not deployed yet)

## Fix Applied
✅ Both workflows **disabled** until needed  
✅ Azure Functions deployment: Use Azure CLI manually  
✅ Frontend deployment: Will configure when Azure Static Web App is ready

---

## Rule for Future Development

### Before Pushing to Main:
1. ✅ Local build succeeds (`dotnet build` = 0 errors)
2. ✅ GitHub Actions configured or disabled
3. ✅ After push, check https://github.com/jplopezinnohunt/vendor-mdm-portal/actions
4. ✅ All workflows must be green (or intentionally disabled)

### If Workflow Fails After Push:
1. Investigate immediately
2. Fix or disable the workflow
3. Push fix within 1 hour
4. Code is NOT production-ready until CI/CD passes

## Long-term: Enable Branch Protection
- Require pull requests
- Require status checks to pass
- No direct pushes to `main`
