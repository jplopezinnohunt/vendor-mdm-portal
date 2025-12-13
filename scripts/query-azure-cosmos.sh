#!/bin/bash

# Query Cosmos DB for invitation data
# This script retrieves invitation-related documents from Cosmos DB

set -e

RESOURCE_GROUP="rg-vendor-mdm-dev-v3"
COSMOS_ACCOUNT="cosmos-vendor-mdm-dev-fmkijlt6yfyeq"
DATABASE_NAME="VendorMdm"
CONTAINER_NAME="InvitationArtifacts"  # Based on the hybrid model architecture

echo "🔍 Querying Cosmos DB for Invitation Data..."
echo ""
echo "Account: $COSMOS_ACCOUNT"
echo "Database: $DATABASE_NAME"
echo "Container: $CONTAINER_NAME"
echo ""

# Get Cosmos DB key
echo "🔑 Getting Cosmos DB access key..."
COSMOS_KEY=$(az cosmosdb keys list \
    --resource-group "$RESOURCE_GROUP" \
    --name "$COSMOS_ACCOUNT" \
    --type keys \
    --query "primaryMasterKey" -o tsv)

if [ -z "$COSMOS_KEY" ]; then
    echo "❌ Failed to get Cosmos DB key"
    exit 1
fi

echo "✅ Access key obtained"
echo ""

# Get Cosmos DB endpoint
COSMOS_ENDPOINT=$(az cosmosdb show \
    --resource-group "$RESOURCE_GROUP" \
    --name "$COSMOS_ACCOUNT" \
    --query "documentEndpoint" -o tsv)

echo "📊 Cosmos DB Endpoint: $COSMOS_ENDPOINT"
echo ""

# List databases
echo "📦 Listing Cosmos DB databases..."
az cosmosdb sql database list \
    --resource-group "$RESOURCE_GROUP" \
    --account-name "$COSMOS_ACCOUNT" \
    --query "[].id" -o table

echo ""

# Try to list containers in the VendorMdm database if it exists
echo "📦 Attempting to list containers in '$DATABASE_NAME' database..."
az cosmosdb sql container list \
    --resource-group "$RESOURCE_GROUP" \
    --account-name "$COSMOS_ACCOUNT" \
    --database-name "$DATABASE_NAME" \
    --query "[].id" -o table 2>/dev/null || {
        echo "⚠️  Database '$DATABASE_NAME' may not exist yet"
        echo ""
        echo "📦 Trying alternative names..."
        
        # List all databases to see what exists
        echo "Available databases:"
        az cosmosdb sql database list \
            --resource-group "$RESOURCE_GROUP" \
            --account-name "$COSMOS_ACCOUNT" \
            --query "[].id" -o tsv
    }

echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "💡 To query documents, use:"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""
echo "Option 1: Azure Portal Data Explorer"
echo "   https://portal.azure.com"
echo ""
echo "Option  2: Azure CLI (if container exists)"
echo "   az cosmosdb sql container query \\"
echo "     --resource-group $RESOURCE_GROUP \\"
echo "     --account-name $COSMOS_ACCOUNT \\"
echo "     --database-name $DATABASE_NAME \\"
echo "     --name $CONTAINER_NAME \\"
echo "     --query-text 'SELECT * FROM c ORDER BY c._ts DESC OFFSET 0 LIMIT 10'"
echo ""
