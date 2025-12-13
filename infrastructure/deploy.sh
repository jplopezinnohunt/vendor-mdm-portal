#!/bin/bash
# Automated Azure Deployment Script for Vendor MDM Portal
# This script deploys all infrastructure and applications to Azure

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Helper functions
print_header() {
    echo -e "\n${BLUE}════════════════════════════════════════════════════════${NC}"
    echo -e "${BLUE}  $1${NC}"
    echo -e "${BLUE}════════════════════════════════════════════════════════${NC}\n"
}

print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

print_info() {
    echo -e "${BLUE}ℹ $1${NC}"
}

# Check prerequisites
print_header "Checking Prerequisites"

if ! command -v az &> /dev/null; then
    print_error "Azure CLI not found. Please install it first:"
    echo "  https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
    exit 1
fi
print_success "Azure CLI found"

if ! command -v dotnet &> /dev/null; then
    print_error ".NET SDK not found. Please install .NET 8.0:"
    echo "  https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
fi
print_success ".NET SDK found"

if ! command -v npm &> /dev/null; then
    print_error "npm not found. Please install Node.js:"
    echo "  https://nodejs.org/"
    exit 1
fi
print_success "npm found"

# Configuration
print_header "Deployment Configuration"

read -p "Environment (dev/test/prod) [dev]: " ENVIRONMENT
ENVIRONMENT=${ENVIRONMENT:-dev}

read -p "Azure Region [eastus]: " LOCATION
LOCATION=${LOCATION:-eastus}

read -p "Resource Group Name [rg-vendor-mdm-$ENVIRONMENT]: " RESOURCE_GROUP
RESOURCE_GROUP=${RESOURCE_GROUP:-rg-vendor-mdm-$ENVIRONMENT}

read -p "SQL Admin Username [sqladmin]: " SQL_ADMIN_USER
SQL_ADMIN_USER=${SQL_ADMIN_USER:-sqladmin}

read -s -p "SQL Admin Password: " SQL_ADMIN_PASSWORD
echo ""

if [ -z "$SQL_ADMIN_PASSWORD" ]; then
    print_error "SQL Admin Password is required"
    exit 1
fi

read -p "Azure AD Tenant ID: " AZURE_AD_TENANT_ID
if [ -z "$AZURE_AD_TENANT_ID" ]; then
    print_error "Azure AD Tenant ID is required"
    exit 1
fi

read -p "Azure AD Client ID (App Registration): " AZURE_AD_CLIENT_ID
if [ -z "$AZURE_AD_CLIENT_ID" ]; then
    print_error "Azure AD Client ID is required"
    exit 1
fi

read -p "Company Name [Your Company]: " COMPANY_NAME
COMPANY_NAME=${COMPANY_NAME:-"Your Company"}

print_info "Configuration:"
echo "  Environment: $ENVIRONMENT"
echo "  Region: $LOCATION"
echo "  Resource Group: $RESOURCE_GROUP"
echo "  SQL Admin User: $SQL_ADMIN_USER"
echo "  Azure AD Tenant: $AZURE_AD_TENANT_ID"
echo "  Azure AD Client: $AZURE_AD_CLIENT_ID"
echo "  Company Name: $COMPANY_NAME"

read -p "Continue with deployment? (yes/no): " CONFIRM
if [ "$CONFIRM" != "yes" ]; then
    print_warning "Deployment cancelled"
    exit 0
fi

# Login to Azure
print_header "Azure Login"
az account show &> /dev/null || az login
print_success "Logged in to Azure"

# Create Resource Group
print_header "Creating Resource Group"
az group create \
    --name "$RESOURCE_GROUP" \
    --location "$LOCATION" \
    --output none
print_success "Resource group created: $RESOURCE_GROUP"

# Deploy Infrastructure
print_header "Deploying Infrastructure (this may take 10-15 minutes)"
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
    --output json)

print_success "Infrastructure deployed successfully"

# Extract resource names from deployment output
APP_SERVICE_NAME=$(echo $DEPLOYMENT_OUTPUT | jq -r '.properties.outputs.appServiceName.value')
APP_SERVICE_URL=$(echo $DEPLOYMENT_OUTPUT | jq -r '.properties.outputs.appServiceUrl.value')
STATIC_WEB_APP_NAME=$(echo $DEPLOYMENT_OUTPUT | jq -r '.properties.outputs.staticWebAppName.value')
STATIC_WEB_APP_URL=$(echo $DEPLOYMENT_OUTPUT | jq -r '.properties.outputs.staticWebAppUrl.value')
DEPLOYMENT_TOKEN=$(echo $DEPLOYMENT_OUTPUT | jq -r '.properties.outputs.staticWebAppDeploymentToken.value')

print_info "Backend API: $APP_SERVICE_URL"
print_info "Frontend: $STATIC_WEB_APP_URL"

# Deploy Backend API
print_header "Deploying Backend API"
cd ../backend/VendorMdm.Api

print_info "Building backend..."
dotnet publish -c Release -o ./publish

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

# Cleanup
rm -rf publish deploy.zip

print_success "Backend deployed to $APP_SERVICE_URL"

# Deploy Frontend
print_header "Deploying Frontend"
cd ../../frontend

print_info "Installing dependencies..."
npm install --silent

print_info "Building frontend..."
npm run build

print_info "Deploying to Static Web App..."
# Install SWA CLI if not present
if ! command -v swa &> /dev/null; then
    npm install -g @azure/static-web-apps-cli
fi

swa deploy ./dist \
    --deployment-token "$DEPLOYMENT_TOKEN" \
    --env production

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
print_info "Testing backend health endpoint..."
HEALTH_RESPONSE=$(curl -s "$APP_SERVICE_URL/api/health" || echo "failed")

if [[ $HEALTH_RESPONSE == *"healthy"* ]]; then
    print_success "Backend is healthy"
else
    print_warning "Backend health check returned: $HEALTH_RESPONSE"
    print_warning "This might be normal if the app is still starting up"
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
echo "  2. Test the login flow with Azure AD"
echo "  3. Assign users to Admin/Approver roles in Azure AD"
echo "  4. Configure custom domain (optional)"
echo ""
print_warning "Important: Remember to:"
echo "  - Assign proper Azure AD roles to users"
echo "  - Review and adjust Azure SQL firewall rules"
echo "  - Configure backup policies"
echo "  - Set up monitoring alerts"
echo ""
