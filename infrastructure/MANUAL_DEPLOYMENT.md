# Manual Azure Portal Deployment Guide

## Step 1: Prepare Parameters

Edit `main.parameters.json` and update:
- `sqlAdminPassword`: Choose a strong password (min 8 chars, uppercase, lowercase, number, special char)
- `azureAdTenantId`: Your Azure AD Tenant ID (from portal.azure.com → Azure Active Directory → Overview)
- `azureAdClientId`: Your App Registration Client ID (from portal.azure.com → Azure Active Directory → App registrations)

## Step 2: Deploy via Azure Portal

1. **Go to Azure Portal**: https://portal.azure.com

2. **Create Resource Group**:
   - Search for "Resource groups"
   - Click "+ Create"
   - Name: `rg-vendor-mdm-dev`
   - Region: `East US` (or your preferred region)
   - Click "Review + create" → "Create"

3. **Deploy Custom Template**:
   - Search for "Deploy a custom template"
   - Click "Build your own template in the editor"
   - Click "Load file" and select: `/Users/jplopez/projects/vendor-mdm-portal/infrastructure/main.bicep`
   - Click "Save"

4. **Configure Parameters**:
   - Click "Edit parameters"
   - Click "Load file" and select: `/Users/jplopez/projects/vendor-mdm-portal/infrastructure/main.parameters.json`
   - Click "Save"
   - Verify all parameters are correct

5. **Deploy**:
   - Select Resource group: `rg-vendor-mdm-dev`
   - Review configuration
   - Click "Review + create"
   - Click "Create"

6. **Wait for Deployment** (10-15 minutes):
   - Monitor progress in the deployment blade
   - Once complete, go to "Outputs" tab
   - **Save these values** (you'll need them):
     - `appServiceUrl`
     - `staticWebAppUrl`
     - `staticWebAppDeploymentToken`

## Step 3: Deploy Backend API

### Option A: Via VS Code Azure Extension
1. Install "Azure App Service" extension in VS Code
2. Right-click on `backend/VendorMdm.Api` → "Deploy to Web App"
3. Select your subscription and app service

### Option B: Via Azure CLI (after installing)
```bash
cd backend/VendorMdm.Api
dotnet publish -c Release -o ./publish
cd publish
zip -r ../deploy.zip .
cd ..
az webapp deployment source config-zip \
  --resource-group rg-vendor-mdm-dev \
  --name <YOUR_APP_SERVICE_NAME> \
  --src deploy.zip
```

## Step 4: Deploy Frontend

### Via npm (requires SWA CLI)
```bash
cd frontend
npm install
npm run build
npm install -g @azure/static-web-apps-cli
swa deploy ./dist \
  --deployment-token <YOUR_DEPLOYMENT_TOKEN> \
  --env production
```

## Step 5: Configure CORS

In Azure Portal:
1. Go to your App Service → API → CORS
2. Add Allowed Origin: `https://<your-static-web-app-name>.azurestaticapps.net`
3. Save

## Step 6: Test

1. Navigate to your Static Web App URL
2. Test login with Azure AD
3. Test creating an invitation

---

## Troubleshooting

- **404 errors**: Wait 5-10 minutes after deployment for apps to fully start
- **Authentication errors**: Verify Azure AD Client ID and Tenant ID are correct
- **Database errors**: Check Key Vault access policy for App Service managed identity
