#!/bin/bash

# Script to configure Azure App Service connection strings directly
# This bypasses Key Vault to ensure the API connects to databases

set -e

RESOURCE_GROUP="rg-vendor-mdm-dev-v3"
APP_SERVICE="app-vendor-mdm-api-dev-fmkijlt6yfyeq"
SQL_SERVER="sql-vendor-mdm-dev-fmkijlt6yfyeq"
SQL_DATABASE="VendorMdmDb"
COSMOS_ACCOUNT="cosmos-vendor-mdm-dev-fmkijlt6yfyeq"
SERVICE_BUS="sb-vendor-mdm-dev-fmkijlt6yfyeq"

echo "🔧 Configuring Azure App Service Connection Strings..."
echo ""

# Get SQL connection string with Managed Identity
SQL_CONNECTION="Server=tcp:${SQL_SERVER}.database.windows.net,1433;Initial Catalog=${SQL_DATABASE};Authentication=Active Directory Managed Identity;Encrypt=True;TrustServerCertificate=False;"

# Get Cosmos endpoint (will use Managed Identity)
COSMOS_ENDPOINT="https://${COSMOS_ACCOUNT}.documents.azure.com:443/"

# Get Service Bus namespace
SERVICE_BUS_ENDPOINT="${SERVICE_BUS}.servicebus.windows.net"

echo "1️⃣  Setting SQL Connection String..."
az webapp config connection-string set \
  --resource-group "$RESOURCE_GROUP" \
  --name "$APP_SERVICE" \
  --connection-string-type "SQLAzure" \
  --settings Sql="$SQL_CONNECTION"

echo "✅ SQL connection string set"
echo ""

echo "2️⃣  Setting Cosmos DB endpoint as App Setting..."
az webapp config appsettings set \
  --resource-group "$RESOURCE_GROUP" \
  --name "$APP_SERVICE" \
  --settings "ConnectionStrings__Cosmos=$COSMOS_ENDPOINT"

echo "✅ Cosmos endpoint set"
echo ""

echo "3️⃣  Setting Service Bus endpoint..."
az webapp config appsettings set \
  --resource-group "$RESOURCE_GROUP" \
  --name "$APP_SERVICE" \
  --settings "ConnectionStrings__ServiceBus=$SERVICE_BUS_ENDPOINT"

echo "✅ Service Bus endpoint set"
echo ""

echo "4️⃣  Setting DataSourceMode to Connected..."
az webapp config appsettings set \
  --resource-group "$RESOURCE_GROUP" \
  --name "$APP_SERVICE" \
  --settings "DataSourceMode=Connected"

echo "✅ DataSourceMode set to Connected"
echo ""

echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "✅ Configuration Complete!"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""
echo "⚠️  Important: You need to grant the App Service Managed Identity access to:"
echo "   1. SQL Database (db_datareader, db_datawriter roles)"
echo "   2. Cosmos DB (Read/Write permissions)"
echo "   3. Service Bus (Send/Listen)"
echo ""
echo "Run the companion script: scripts/grant-app-service-permissions.sh"
echo ""
