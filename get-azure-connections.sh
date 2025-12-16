#!/bin/bash

# Script to get Azure connection strings for local development
# Prerequisites: Azure CLI installed and logged in

set -e

echo "🔍 Getting Azure Connection Strings..."
echo ""

# Check if logged in
if ! az account show &>/dev/null; then
    echo "❌ Not logged in to Azure. Please run: az login"
    exit 1
fi

echo "✅ Logged in to Azure"
echo ""

# Get current subscription
SUBSCRIPTION=$(az account show --query name -o tsv)
echo "📋 Subscription: $SUBSCRIPTION"
echo ""

# Resource names (from current Azure deployment)
SQL_SERVER="sql-vendor-mdm-dev"
SQL_DB="VendorMdmDb"
COSMOS_ACCOUNT="cosmos-vendor-mdm-dev"
SERVICE_BUS="sb-vendor-mdm-dev"

# Get resource group (try to find it)
echo "🔍 Finding resource group..."
RESOURCE_GROUP=$(az sql server list --query "[?name=='$SQL_SERVER'].resourceGroup" -o tsv | head -1)

if [ -z "$RESOURCE_GROUP" ]; then
    echo "⚠️  Could not auto-detect resource group."
    echo "Please provide your resource group name:"
    read -r RESOURCE_GROUP
else
    echo "✅ Found resource group: $RESOURCE_GROUP"
fi

echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "📝 Connection Strings"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""

# SQL Connection String
echo "1️⃣  SQL Database Connection String:"
echo "   (You'll need to replace YOUR_USERNAME and YOUR_PASSWORD with actual SQL credentials)"
SQL_CONNECTION=$(az sql db show-connection-string \
    --server "$SQL_SERVER" \
    --name "$SQL_DB" \
    --client ado.net \
    -o tsv)
echo "   $SQL_CONNECTION"
echo ""

# Cosmos DB Connection String
echo "2️⃣  Cosmos DB Connection String:"
COSMOS_CONNECTION=$(az cosmosdb keys list \
    --resource-group "$RESOURCE_GROUP" \
    --name "$COSMOS_ACCOUNT" \
    --type connection-strings \
    --query "connectionStrings[0].connectionString" -o tsv)
echo "   $COSMOS_CONNECTION"
echo ""

# Service Bus Connection String
echo "3️⃣  Service Bus Connection String:"
SERVICE_BUS_CONNECTION=$(az servicebus namespace authorization-rule keys list \
    --resource-group "$RESOURCE_GROUP" \
    --namespace-name "$SERVICE_BUS" \
    --name RootManageSharedAccessKey \
    --query "primaryConnectionString" -o tsv)
echo "   $SERVICE_BUS_CONNECTION"
echo ""

echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "📋 Next Steps:"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""
echo "Option 1: Use User Secrets (Recommended)"
echo "  cd backend/VendorMdm.Api"
echo "  dotnet user-secrets set \"ConnectionStrings:Sql\" \"<SQL_CONNECTION_STRING>\""
echo "  dotnet user-secrets set \"ConnectionStrings:Cosmos\" \"<COSMOS_CONNECTION_STRING>\""
echo "  dotnet user-secrets set \"ConnectionStrings:ServiceBus\" \"<SERVICE_BUS_CONNECTION_STRING>\""
echo ""
echo "Option 2: Update appsettings.Development.json"
echo "  Edit: backend/VendorMdm.Api/appsettings.Development.json"
echo "  Replace YOUR_SQL_USERNAME, YOUR_SQL_PASSWORD, YOUR_COSMOS_KEY, YOUR_SERVICE_BUS_KEY"
echo ""
echo "⚠️  Don't forget to add your IP to SQL Server firewall!"
echo "   Run: az sql server firewall-rule create --resource-group $RESOURCE_GROUP --server $SQL_SERVER --name LocalDev --start-ip-address $(curl -s https://api.ipify.org) --end-ip-address $(curl -s https://api.ipify.org)"
echo ""

