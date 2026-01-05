#!/bin/bash
set -e

# Configuration
RESOURCE_GROUP="rg-vendor-mdm-dev"
APP_NAME="app-vendor-mdm-api-dev"
PROJECT_PATH="./backend/VendorMdm.Api"
PUBLISH_OUTPUT="./backend/VendorMdm.Api/publish"

echo "========================================================"
echo "Backend Deployment Script (Manual / Azure CLI)"
echo "Target: $APP_NAME ($RESOURCE_GROUP)"
echo "Protocol: Azure CLI Only (No GitHub Actions)"
echo "========================================================"

# 1. Clean and Build
echo "--> Cleaning previous build..."
rm -rf "$PUBLISH_OUTPUT"

echo "--> Restoring dependencies..."
dotnet restore "$PROJECT_PATH"

echo "--> Building Release configuration..."
dotnet build "$PROJECT_PATH" --configuration Release --no-restore

# 2. Publish
echo "--> Publishing to $PUBLISH_OUTPUT..."
dotnet publish "$PROJECT_PATH" --configuration Release --no-build --output "$PUBLISH_OUTPUT"

# 3. Deploy via Azure CLI
# Compressing format is handled automatically by az webapp deploy in some versions, 
# but dotnet publish produces a folder. 'az webapp deploy' accepts --src-path as folder or zip.
# We will zip it to be safe and standard.
echo "--> Zipping publish package..."
cd "$PUBLISH_OUTPUT"
zip -r ../publish.zip .
cd - > /dev/null

echo "--> Deploying to Azure App Service..."
echo "Command: az webapp deploy --resource-group $RESOURCE_GROUP --name $APP_NAME --src-path $PROJECT_PATH/publish.zip"

# Check if logged in (simple check)
if ! az account show > /dev/null 2>&1; then
    echo "ERROR: You are not logged into Azure CLI. Please run 'az login' first."
    exit 1
fi

az webapp deploy --resource-group "$RESOURCE_GROUP" --name "$APP_NAME" --src-path "$PROJECT_PATH/publish.zip"

echo "========================================================"
echo "Deployment Complete!"
echo "========================================================"
