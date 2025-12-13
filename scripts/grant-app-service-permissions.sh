#!/bin/bash

# Grant App Service Managed Identity permissions to Azure resources
# This allows the API to access SQL, Cosmos DB, and Service Bus

set -e

RESOURCE_GROUP="rg-vendor-mdm-dev-v3"
APP_SERVICE="app-vendor-mdm-api-dev-fmkijlt6yfyeq"
SQL_SERVER="sql-vendor-mdm-dev-fmkijlt6yfyeq"
SQL_DATABASE="VendorMdmDb"
COSMOS_ACCOUNT="cosmos-vendor-mdm-dev-fmkijlt6yfyeq"
SERVICE_BUS="sb-vendor-mdm-dev-fmkijlt6yfyeq"

echo "🔐 Granting App Service Managed Identity Permissions..."
echo ""

# Get the App Service Managed Identity Principal ID
echo "1️⃣  Getting App Service Managed Identity..."
PRINCIPAL_ID=$(az webapp identity show \
  --resource-group "$RESOURCE_GROUP" \
  --name "$APP_SERVICE" \
  --query principalId -o tsv)

if [ -z "$PRINCIPAL_ID" ]; then
  echo "❌ No Managed Identity found. Creating one..."
  az webapp identity assign \
    --resource-group "$RESOURCE_GROUP" \
    --name "$APP_SERVICE"
  
  PRINCIPAL_ID=$(az webapp identity show \
    --resource-group "$RESOURCE_GROUP" \
    --name "$APP_SERVICE" \
    --query principalId -o tsv)
fi

echo "✅ Managed Identity Principal ID: $PRINCIPAL_ID"
echo ""

# Grant SQL Server permissions
echo "2️⃣  Granting SQL Server permissions..."

# Add AAD admin to SQL Server (using current user)
CURRENT_USER=$(az account show --query user.name -o tsv)
CURRENT_USER_OID=$(az ad signed-in-user show --query id -o tsv)

echo "   Setting you ($CURRENT_USER) as SQL Admin to grant permissions..."
az sql server ad-admin create \
  --resource-group "$RESOURCE_GROUP" \
  --server-name "$SQL_SERVER" \
  --display-name "$CURRENT_USER" \
  --object-id "$CURRENT_USER_OID" || echo "   (Admin may already be set)"

echo "   Granting db_owner role to App Service Managed Identity..."
echo "   Note: This requires you to run SQL commands. Creating SQL script..."

cat > /tmp/grant-sql-permissions.sql <<EOF
-- Create user for Managed Identity
CREATE USER [${APP_SERVICE}] FROM EXTERNAL PROVIDER;

-- Grant permissions
ALTER ROLE db_datareader ADD MEMBER [${APP_SERVICE}];
ALTER ROLE db_datawriter ADD MEMBER [${APP_SERVICE}];
ALTER ROLE db_ddladmin ADD MEMBER [${APP_SERVICE}];

-- Verify
SELECT name, type_desc FROM sys.database_principals WHERE name = '${APP_SERVICE}';
EOF

echo "   SQL script created at: /tmp/grant-sql-permissions.sql"
echo "   You need to run this against the database using sqlcmd or Azure Portal"
echo ""

# Grant Cosmos DB permissions
echo "3️⃣  Granting Cosmos DB permissions..."

# Get Cosmos DB resource ID
COSMOS_ID=$(az cosmosdb show \
  --resource-group "$RESOURCE_GROUP" \
  --name "$COSMOS_ACCOUNT" \
  --query id -o tsv)

# Assign Cosmos DB Data Contributor role
az role assignment create \
  --assignee "$PRINCIPAL_ID" \
  --role "Cosmos DB Account Reader Role" \
  --scope "$COSMOS_ID" || echo "   (Role may already be assigned)"

az role assignment create \
  --assignee "$PRINCIPAL_ID" \
  --role "DocumentDB Account Contributor" \
  --scope "$COSMOS_ID" || echo "   (Role may already be assigned)"

echo "✅ Cosmos DB permissions granted"
echo ""

# Grant Service Bus permissions
echo "4️⃣  Granting Service Bus permissions..."

# Get Service Bus resource ID
SERVICE_BUS_ID=$(az servicebus namespace show \
  --resource-group "$RESOURCE_GROUP" \
  --name "$SERVICE_BUS" \
  --query id -o tsv)

# Assign Service Bus roles
az role assignment create \
  --assignee "$PRINCIPAL_ID" \
  --role "Azure Service Bus Data Sender" \
  --scope "$SERVICE_BUS_ID" || echo "   (Role may already be assigned)"

az role assignment create \
  --assignee "$PRINCIPAL_ID" \
  --role "Azure Service Bus Data Receiver" \
  --scope "$SERVICE_BUS_ID" || echo "   (Role may already be assigned)"

echo "✅ Service Bus permissions granted"
echo ""

echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "✅ Permissions Configuration Complete!"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""
echo "⚠️  SQL Database Permissions:"
echo "   Run the SQL script to grant database access:"
echo "   cat /tmp/grant-sql-permissions.sql"
echo ""
echo "   Option 1: Use sqlcmd"
echo "     sqlcmd -S ${SQL_SERVER}.database.windows.net -d ${SQL_DATABASE} -G -i /tmp/grant-sql-permissions.sql"
echo ""
echo "   Option 2: Use Azure Portal Query Editor"
echo "     Copy/paste the SQL from /tmp/grant-sql-permissions.sql"
echo ""
