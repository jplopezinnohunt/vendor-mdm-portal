# 🚀 Deploy Application to Azure - Quick Guide

## Current Status

You have uncommitted changes that need to be deployed:
- ✅ Backend API configuration updates (port 5001, connection logic)
- ✅ Frontend API configuration (port 5001)
- ✅ Error handling improvements
- ✅ Documentation files

---

## Step 1: Review Changes

```bash
git status
```

**Modified Files:**
- `backend/VendorMdm.Api/Program.cs` - Connection logic improvements
- `backend/VendorMdm.Api/Properties/launchSettings.json` - Port changed to 5001
- `frontend/src/services/api.ts` - API base URL updated to port 5001
- `frontend/src/pages/admin/InviteVendorForm.tsx` - Error handling improved

**New Documentation:**
- Various setup and troubleshooting guides (optional to commit)

---

## Step 2: Commit Changes

```bash
# Add all changes
git add .

# Commit with descriptive message
git commit -m "fix: Update API configuration for local development

- Changed backend port from 5000 to 5001 (avoid AirPlay conflict on macOS)
- Updated frontend API base URL to use port 5001
- Improved error handling and user feedback messages
- Enhanced connection string detection logic
- Added comprehensive documentation for local setup

Fixes:
- Connection errors when backend not running
- Port conflicts on macOS
- Better error messages for troubleshooting"

# Push to trigger deployment
git push origin main
```

---

## Step 3: Monitor Deployment

### GitHub Actions Workflows

Once you push, two workflows will run automatically:

#### 1. Frontend Deployment (Azure Static Web Apps)
- **Workflow**: `.github/workflows/azure-static-web-apps.yml`
- **Trigger**: Push to `main` branch
- **Duration**: ~3-5 minutes
- **Watch**: https://github.com/YOUR_USERNAME/vendor-mdm-portal/actions

#### 2. Backend Deployment (Azure Functions)
- **Workflow**: `.github/workflows/azure-functions.yml`
- **Trigger**: Push to `main` with changes in `backend/` or `infrastructure/`
- **Duration**: ~5-10 minutes
- **Watch**: https://github.com/YOUR_USERNAME/vendor-mdm-portal/actions

---

## Step 4: Verify Deployment

### Check Frontend
1. Visit your Azure Static Web App URL
2. Verify the app loads correctly
3. Test the invitation feature

### Check Backend
1. Azure Portal → Function App
2. Verify functions are deployed
3. Check logs for any errors

### Check Infrastructure
1. Azure Portal → Service Bus
2. Verify queues exist
3. Check resource group for all resources

---

## Required GitHub Secrets

Make sure these are configured in:
**GitHub Repository → Settings → Secrets and variables → Actions**

- `AZURE_CREDENTIALS` - Service principal JSON
- `AZURE_RESOURCE_GROUP` - Your resource group name
- `AZURE_STATIC_WEB_APPS_API_TOKEN` - Static Web Apps deployment token

---

## Quick Deploy Command

```bash
# One command to deploy everything
git add . && \
git commit -m "fix: Update API configuration for local development" && \
git push origin main
```

Then monitor: https://github.com/YOUR_USERNAME/vendor-mdm-portal/actions

---

## Troubleshooting

### Deployment Fails
- Check GitHub Actions logs
- Verify secrets are configured
- Ensure Azure resources exist

### Frontend Not Updating
- Check Static Web Apps deployment status
- Verify build completed successfully
- Clear browser cache

### Backend Not Deploying
- Check if changes are in `backend/` directory
- Verify Function App exists in Azure
- Check deployment logs

---

**Ready to deploy? Run the commit and push commands above!** 🚀

