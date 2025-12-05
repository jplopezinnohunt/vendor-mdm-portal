# Azure Infrastructure Deployment Guide

## Prerequisites

1. **Azure CLI** installed and configured
   ```bash
   az --version  # Should be 2.40 or higher
   az login
   az account set --subscription "<your-subscription-id>"
   ```

2. **Azure Subscription** with appropriate permissions:
   - Contributor role (minimum)
   - Owner role (for RBAC assignments)

3. **Resource Group** created:
   ```bash
   az group create --name vendormdm-rg --location eastus
   ```

## Deployment Steps

### Step 1: Deploy Infrastructure with Bicep

```bash
# Navigate to infrastructure directory
cd infrastructure

# Deploy for Development environment
az deployment group create \
  --resource-group vendormdm-rg \
  --template-file invitation-infrastructure.bicep \
  --parameters environment=dev \
  --parameters baseName=vendormdm

# Save outputs for later use
az deployment group show \
  --resource-group vendormdm-rg \
  --name invitation-infrastructure \
  --query properties.outputs
```

**Expected Resources Created:**
- ✅ Service Bus Namespace with 2 queues
- ✅ Storage Account for Functions
- ✅ Application Insights
- ✅ App Service Plan
- ✅ Function App
- ✅ Metric Alert

### Step 2: Deploy Azure Function Code

```bash
# Navigate to Functions project
cd ../backend/VendorMdm.Artifacts

# Build and publish
dotnet publish --configuration Release --output ./publish

# Create deployment package
cd publish
Compress-Archive -Path * -DestinationPath ../deploy.zip -Force
cd ..

# Deploy to Azure
az functionapp deployment source config-zip \
  --resource-group vendormdm-rg \
  --name vendormdm-func-dev \
  --src deploy.zip
```

### Step 3: Verify Function Deployment

```bash
# Check function app status
az functionapp show \
  --resource-group vendormdm-rg \
  --name vendormdm-func-dev \
  --query state

# List functions
az functionapp function list \
  --resource-group vendormdm-rg \
  --name vendormdm-func-dev

# Expected functions:
# - SendInvitationEmail
# - SendInvitationEmailHttp
# - SubmitVendorChange
# - GetPendingOnboardingRequests
```

### Step 4: Configure Email Service

#### Option A: Azure Communication Services

```bash
# Create Communication Service resource
az communication create \
  --name vendormdm-communication \
  --resource-group vendormdm-rg \
  --location global \
  --data-location UnitedStates

# Get connection string
az communication list-key \
  --name vendormdm-communication \
  --resource-group vendormdm-rg

# Add to Function App settings
az functionapp config appsettings set \
  --resource-group vendormdm-rg \
  --name vendormdm-func-dev \
  --settings EmailServiceConnection="<connection-string>"
```

#### Option B: SendGrid (Alternative)

```bash
# Create SendGrid account (via Portal or CLI)
# Get API key from SendGrid dashboard

# Add to Function App settings
az functionapp config appsettings set \
  --resource-group vendormdm-rg \
  --name vendormdm-func-dev \
  --settings SendGridApiKey="<api-key>"
```

### Step 5: Deploy API Application

```bash
# Create App Service (if not exists)
az webapp create \
  --resource-group vendormdm-rg \
  --plan vendormdm-asp-dev \
  --name vendormdm-api-dev \
  --runtime "DOTNET|8.0"

# Get Service Bus connection string
SERVICE_BUS_CONN=$(az servicebus namespace authorization-rule keys list \
  --resource-group vendormdm-rg \
  --namespace-name vendormdm-sb-dev \
  --name RootManageSharedAccessKey \
  --query primaryConnectionString -o tsv)

# Configure App Settings
az webapp config appsettings set \
  --resource-group vendormdm-rg \
  --name vendormdm-api-dev \
  --settings \
    ASPNETCORE_ENVIRONMENT=Development \
    ConnectionStrings__ServiceBus="$SERVICE_BUS_CONN" \
    SapEnvironmentCode=D01

# Deploy API code
cd ../VendorMdm.Api
dotnet publish --configuration Release
cd bin/Release/net8.0/publish

# Create zip and deploy
Compress-Archive -Path * -DestinationPath ../api-deploy.zip -Force
az webapp deployment source config-zip \
  --resource-group vendormdm-rg \
  --name vendormdm-api-dev \
  --src ../api-deploy.zip
```

### Step 6: Create and Configure SQL Database

```bash
# Create SQL Server
az sql server create \
  --resource-group vendormdm-rg \
  --name vendormdm-sql-dev \
  --location eastus \
  --admin-user sqladmin \
  --admin-password "<strong-password>"

# Enable Azure services access
az sql server firewall-rule create \
  --resource-group vendormdm-rg \
  --server vendormdm-sql-dev \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Create database
az sql db create \
  --resource-group vendormdm-rg \
  --server vendormdm-sql-dev \
  --name vendormdm-db \
  --service-objective S1 \
  --backup-storage-redundancy Local

# Get connection string
az sql db show-connection-string \
  --server vendormdm-sql-dev \
  --name vendormdm-db \
  --client ado.net
```

### Step 7: Run Database Migrations

```bash
# Update connection string in appsettings
# Run migrations from development machine or cloud shell

cd backend/VendorMdm.Api

# Add migration
dotnet ef migrations add AddVendorInvitations \
  --context SqlDbContext

# Update database
dotnet ef database update \
  --connection "<connection-string>"
```

### Step 8: Test the Deployment

#### Test 1: Service Bus Queue
```bash
# Send test message to queue
az servicebus queue message send \
  --resource-group vendormdm-rg \
  --namespace-name vendormdm-sb-dev \
  --queue-name invitation-emails \
  --body '{
    "EventType": "invitation-created",
    "Data": {
      "InvitationId": "test-123",
      "VendorName": "Test Vendor",
      "Email": "test@example.com",
      "Token": "test-token",
      "ExpiresAt": "2025-12-31T23:59:59Z",
      "InvitedByName": "Admin User"
    }
  }'

# Check function logs
az functionapp log tail \
  --resource-group vendormdm-rg \
  --name vendormdm-func-dev
```

#### Test 2: API Endpoint
```bash
# Test invitation creation (requires API key/auth)
curl -X POST https://vendormdm-api-dev.azurewebsites.net/api/invitation/create \
  -H "Content-Type: application/json" \
  -d '{
    "vendorLegalName": "Test Company",
    "primaryContactEmail": "contact@testcompany.com",
    "expirationDays": 14,
    "notes": "Test invitation"
  }'
```

#### Test 3: Email Function (HTTP Trigger)
```bash
# Get function key
FUNCTION_KEY=$(az functionapp function keys list \
  --resource-group vendormdm-rg \
  --name vendormdm-func-dev \
  --function-name SendInvitationEmailHttp \
  --query default -o tsv)

# Send test email
curl -X POST \
  "https://vendormdm-func-dev.azurewebsites.net/api/invitation/send-email?code=$FUNCTION_KEY" \
  -H "Content-Type: application/json" \
  -d '{
    "invitationId": "test-123",
    "vendorName": "Test Vendor",
    "email": "test@example.com",
    "token": "abc123xyz",
    "expiresAt": "2025-12-31T23:59:59Z",
    "invitedByName": "Admin User"
  }'
```

### Step 9: Configure Monitoring

```bash
# Create action group for alerts
az monitor action-group create \
  --resource-group vendormdm-rg \
  --name vendormdm-alerts \
  --short-name vmdmalert \
  --email-receiver name=AdminEmail address=admin@yourcompany.com

# Update metric alert to use action group
az monitor metrics alert update \
  --resource-group vendormdm-rg \
  --name vendormdm-email-failure-alert-dev \
  --add-action vendormdm-alerts
```

### Step 10: Enable Diagnostic Logs

```bash
# Create Log Analytics Workspace
az monitor log-analytics workspace create \
  --resource-group vendormdm-rg \
  --workspace-name vendormdm-logs

# Get workspace ID
WORKSPACE_ID=$(az monitor log-analytics workspace show \
  --resource-group vendormdm-rg \
  --workspace-name vendormdm-logs \
  --query id -o tsv)

# Enable diagnostics for Function App
az monitor diagnostic-settings create \
  --resource $(az functionapp show --resource-group vendormdm-rg --name vendormdm-func-dev --query id -o tsv) \
  --name function-diagnostics \
  --workspace $WORKSPACE_ID \
  --logs '[{"category": "FunctionAppLogs", "enabled": true}]' \
  --metrics '[{"category": "AllMetrics", "enabled": true}]'

# Enable diagnostics for Service Bus
az monitor diagnostic-settings create \
  --resource $(az servicebus namespace show --resource-group vendormdm-rg --name vendormdm-sb-dev --query id -o tsv) \
  --name servicebus-diagnostics \
  --workspace $WORKSPACE_ID \
  --logs '[{"category": "OperationalLogs", "enabled": true}]' \
  --metrics '[{"category": "AllMetrics", "enabled": true}]'
```

## Post-Deployment Configuration

### 1. Configure CORS (API)
```bash
az webapp cors add \
  --resource-group vendormdm-rg \
  --name vendormdm-api-dev \
  --allowed-origins "https://vendormdm-webapp.azurestaticapps.net"
```

### 2. Configure Custom Domain (Optional)
```bash
# For Static Web App
az staticwebapp hostname set \
  --resource-group vendormdm-rg \
  --name vendormdm-webapp \
  --hostname vendor-portal.yourcompany.com

# For API
az webapp config hostname add \
  --resource-group vendormdm-rg \
  --webapp-name vendormdm-api-dev \
  --hostname api.vendor-portal.yourcompany.com
```

### 3. Enable Application Insights Profiler
```bash
az monitor app-insights component update \
  --resource-group vendormdm-rg \
  --app vendormdm-ai-dev \
  --query-access Enabled
```

## Troubleshooting

### Function App Not Receiving Messages
```bash
# Check queue for messages
az servicebus queue show \
  --resource-group vendormdm-rg \
  --namespace-name vendormdm-sb-dev \
  --name invitation-emails \
  --query messageCount

# Check dead-letter queue
az servicebus queue show \
  --resource-group vendormdm-rg \
  --namespace-name vendormdm-sb-dev \
  --name invitation-emails/$deadletterqueue \
  --query messageCount

# View function errors
az functionapp log tail \
  --resource-group vendormdm-rg \
  --name vendormdm-func-dev \
  --filter Error
```

### Database Connection Issues
```bash
# Test connectivity
az sql db show \
  --resource-group vendormdm-rg \
  --server vendormdm-sql-dev \
  --name vendormdm-db

# Check firewall rules
az sql server firewall-rule list \
  --resource-group vendormdm-rg \
  --server vendormdm-sql-dev
```

### Email Not Sending
```bash
# Check Function App logs
az functionapp log tail \
  --resource-group vendormdm-rg \
  --name vendormdm-func-dev \
  --filter "SendInvitationEmail"

# Verify email service configuration
az functionapp config appsettings list \
  --resource-group vendormdm-rg \
  --name vendormdm-func-dev \
  --query "[?name=='EmailServiceConnection']"
```

## Clean Up (Development Only)

```bash
# Delete entire resource group (CAREFUL!)
az group delete --name vendormdm-rg --yes --no-wait
```

## Production Deployment Checklist

- [ ] Use Premium SKUs for production workloads
- [ ] Enable VNET integration for Functions
- [ ] Configure Private Endpoints for SQL
- [ ] Set up Key Vault for secrets
- [ ] Enable Advanced Threat Protection
- [ ] Configure backup and disaster recovery
- [ ] Set up CI/CD pipeline (GitHub Actions / Azure DevOps)
- [ ] Configure custom domain with SSL
- [ ] Enable Application Insights Live Metrics
- [ ] Set up dashboards and alerts
- [ ] Document runbooks for incidents
- [ ] Perform load testing

## Cost Monitoring

```bash
# View cost analysis
az consumption usage list \
  --start-date 2025-12-01 \
  --end-date 2025-12-31 \
  --query "[?contains(instanceName, 'vendormdm')]"
```

## Next Steps

1. Review Application Insights for any errors
2. Test end-to-end invitation flow
3. Configure production environment
4. Set up CI/CD pipeline
5. Document operational procedures
