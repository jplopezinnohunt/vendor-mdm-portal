#!/bin/bash
# Non-interactive Azure Deployment Script
#
# USAGE:
#   1. Set required environment variables (see below)
#   2. Run: ./deploy-auto.sh
#
# REQUIRED ENVIRONMENT VARIABLES:
#   DEPLOY_SQL_ADMIN_PASSWORD    - SQL Server admin password (strong password)
#   DEPLOY_AZURE_AD_TENANT_ID    - Azure AD Tenant ID
#   DEPLOY_AZURE_AD_CLIENT_ID    - Azure AD Application Client ID
#   DEPLOY_AZURE_SUBSCRIPTION_ID - Azure Subscription ID
#
# OPTIONAL ENVIRONMENT VARIABLES (have defaults):
#   DEPLOY_ENVIRONMENT           - Environment name (default: dev)
#   DEPLOY_LOCATION              - Azure region (default: centralus)
#   DEPLOY_RESOURCE_GROUP        - Resource group name (default: rg-vendor-mdm-dev-v3)
#   DEPLOY_SQL_ADMIN_USER        - SQL admin username (default: sqladmin)
#   DEPLOY_COMPANY_NAME          - Company name (default: VendorMDM)

set -e

# ============================================================================
# CONFIGURATION - FROM ENVIRONMENT VARIABLES
# ============================================================================

# Required variables - must be set
if [ -z "$DEPLOY_SQL_ADMIN_PASSWORD" ]; then
    echo "ERROR: DEPLOY_SQL_ADMIN_PASSWORD environment variable is required"
    echo "Set it with: export DEPLOY_SQL_ADMIN_PASSWORD='your-secure-password'"
    exit 1
fi

if [ -z "$DEPLOY_AZURE_AD_TENANT_ID" ]; then
    echo "ERROR: DEPLOY_AZURE_AD_TENANT_ID environment variable is required"
    exit 1
fi

if [ -z "$DEPLOY_AZURE_AD_CLIENT_ID" ]; then
    echo "ERROR: DEPLOY_AZURE_AD_CLIENT_ID environment variable is required"
    exit 1
fi

if [ -z "$DEPLOY_AZURE_SUBSCRIPTION_ID" ]; then
    echo "ERROR: DEPLOY_AZURE_SUBSCRIPTION_ID environment variable is required"
    exit 1
fi

# Optional variables with defaults
ENVIRONMENT="${DEPLOY_ENVIRONMENT:-dev}"
LOCATION="${DEPLOY_LOCATION:-centralus}"
RESOURCE_GROUP="${DEPLOY_RESOURCE_GROUP:-rg-vendor-mdm-dev-v3}"
SQL_ADMIN_USER="${DEPLOY_SQL_ADMIN_USER:-sqladmin}"
SQL_ADMIN_PASSWORD="$DEPLOY_SQL_ADMIN_PASSWORD"
AZURE_AD_TENANT_ID="$DEPLOY_AZURE_AD_TENANT_ID"
AZURE_AD_CLIENT_ID="$DEPLOY_AZURE_AD_CLIENT_ID"
AZURE_SUBSCRIPTION_ID="$DEPLOY_AZURE_SUBSCRIPTION_ID"
COMPANY_NAME="${DEPLOY_COMPANY_NAME:-VendorMDM}"

# ============================================================================
# DO NOT EDIT BELOW THIS LINE
# ============================================================================

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

print_header() {
    echo -e "\n${BLUE}════════════════════════════════════════════════════════${NC}"
    echo -e "${BLUE}  $1${NC}"
    echo -e "${BLUE}════════════════════════════════════════════════════════${NC}\n"
}

print_success() { echo -e "${GREEN}✓ $1${NC}"; }
print_error() { echo -e "${RED}✗ $1${NC}"; }
print_warning() { echo -e "${YELLOW}⚠ $1${NC}"; }
print_info() { echo -e "${BLUE}ℹ $1${NC}"; }

# Check prerequisites
print_header "Checking Prerequisites"

if ! command -v az &> /dev/null; then
    print_error "Azure CLI not found"
    exit 1
fi
print_success "Azure CLI found"

if ! command -v dotnet &> /dev/null; then
    print_error ".NET SDK not found"
    exit 1
fi
print_success ".NET SDK found"

if ! command -v npm &> /dev/null; then
    print_error "npm not found"
    exit 1
fi
print_success "npm found"

# Display configuration
print_header "Deployment Configuration"
echo "  Environment: $ENVIRONMENT"
echo "  Region: $LOCATION"
echo "  Resource Group: $RESOURCE_GROUP"
echo "  SQL Admin User: $SQL_ADMIN_USER"
echo "  Azure AD Tenant: $AZURE_AD_TENANT_ID"
echo "  Azure AD Client: $AZURE_AD_CLIENT_ID"
echo "  Company Name: $COMPANY_NAME"
echo ""

# Login to Azure
print_header "Azure Login"

# Try to find existing subscription
SUBSCRIPTION_ID=$(az account show --query id -o tsv 2>/dev/null || echo "")

if [ -z "$SUBSCRIPTION_ID" ]; then
    print_warning "No active session found. Please login..."
    # Login allowing no subscriptions initially
    az login --allow-no-subscriptions --use-device-code

    # Use subscription ID from environment variable
    SUBSCRIPTION_ID="$AZURE_SUBSCRIPTION_ID"

    print_info "Setting subscription to $SUBSCRIPTION_ID..."
    az account set --subscription "$SUBSCRIPTION_ID"
else
    print_success "Already logged in"
fi

CURRENT_SUB=$(az account show --query id -o tsv)
print_success "Using Subscription: $CURRENT_SUB"

# Create Resource Group
print_header "Creating Resource Group"
az group create \
    --name "$RESOURCE_GROUP" \
    --location "$LOCATION" \
    --output none
print_success "Resource group created: $RESOURCE_GROUP"

# Deploy Infrastructure
print_header "Deploying Infrastructure"
print_info "This will take 10-15 minutes..."

DEPLOYMENT_OUTPUT=$(az deployment group create \
    --resource-group "$RESOURCE_GROUP" \
    --template-file ./main.bicep \
    --parameters \
        environment="$ENVIRONMENT" \
        location="$LOCATION" \
        sqlAdminUsername="$SQL_ADMIN_USER" \
        sqlAdminPassword="$SQL_ADMIN_PASSWORD" \
        azureAdTenantId="$AZURE_AD_TENANT_ID" \
        azureAdClientId="$AZURE_AD_CLIENT_ID" \
        companyName="$COMPANY_NAME" \
    --output json 2>&1)

if [ $? -ne 0 ]; then
    print_error "Infrastructure deployment failed"
    echo "$DEPLOYMENT_OUTPUT"
    exit 1
fi

print_success "Infrastructure deployed successfully"

# Extract outputs
APP_SERVICE_NAME=$(echo $DEPLOYMENT_OUTPUT | jq -r '.properties.outputs.appServiceName.value')
APP_SERVICE_URL=$(echo $DEPLOYMENT_OUTPUT | jq -r '.properties.outputs.appServiceUrl.value')
STATIC_WEB_APP_NAME=$(echo $DEPLOYMENT_OUTPUT | jq -r '.properties.outputs.staticWebAppName.value')
STATIC_WEB_APP_URL=$(echo $DEPLOYMENT_OUTPUT | jq -r '.properties.outputs.staticWebAppUrl.value')
DEPLOYMENT_TOKEN=$(echo $DEPLOYMENT_OUTPUT | jq -r '.properties.outputs.staticWebAppDeploymentToken.value')

print_info "Backend API: $APP_SERVICE_URL"
print_info "Frontend: $STATIC_WEB_APP_URL"

# Deploy Backend
print_header "Deploying Backend API"
cd ../backend/VendorMdm.Api

print_info "Building backend..."
dotnet publish -c Release -o ./publish > /dev/null 2>&1

print_info "Creating deployment package..."
cd publish
zip -r -q ../deploy.zip .
cd ..

print_info "Deploying to Azure App Service..."
az webapp deployment source config-zip \
    --resource-group "$RESOURCE_GROUP" \
    --name "$APP_SERVICE_NAME" \
    --src deploy.zip \
    --output none

rm -rf publish deploy.zip
print_success "Backend deployed to $APP_SERVICE_URL"

# Deploy Frontend
print_header "Deploying Frontend"
cd ../../frontend

print_info "Installing dependencies..."
npm install --silent

print_info "Building frontend..."
npm run build > /dev/null 2>&1

print_info "Deploying to Static Web App..."
if ! command -v swa &> /dev/null; then
    print_info "Installing SWA CLI..."
    npm install -g @azure/static-web-apps-cli > /dev/null 2>&1
fi

swa deploy ./dist \
    --deployment-token "$DEPLOYMENT_TOKEN" \
    --env production > /dev/null 2>&1

print_success "Frontend deployed to $STATIC_WEB_APP_URL"

# Configure CORS
print_header "Configuring CORS"
az webapp cors add \
    --resource-group "$RESOURCE_GROUP" \
    --name "$APP_SERVICE_NAME" \
    --allowed-origins "$STATIC_WEB_APP_URL" \
    --output none

print_success "CORS configured"

# Test deployment
print_header "Testing Deployment"
print_info "Waiting for services to start (30 seconds)..."
sleep 30

print_info "Testing backend health endpoint..."
HEALTH_RESPONSE=$(curl -s "$APP_SERVICE_URL/api/health" || echo "failed")

if [[ $HEALTH_RESPONSE == *"healthy"* ]]; then
    print_success "Backend is healthy"
else
    print_warning "Backend might still be starting up"
fi

# Final Summary
print_header "Deployment Complete!"
echo ""
echo "📦 Resource Group: $RESOURCE_GROUP"
echo "🌐 Frontend URL:   $STATIC_WEB_APP_URL"
echo "🔧 Backend API:    $APP_SERVICE_URL"
echo "📊 Health Check:   $APP_SERVICE_URL/api/health"
echo "📚 Swagger:        $APP_SERVICE_URL/swagger"
echo ""
print_success "Your Vendor MDM Portal is now deployed!"
echo ""
print_info "Next Steps:"
echo "  1. Navigate to $STATIC_WEB_APP_URL"
echo "  2. Configure app roles in Azure AD (Admin, Approver)"
echo "  3. Assign users to roles"
echo "  4. Test the invitation flow"
echo ""
