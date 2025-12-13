#!/bin/bash

# Query Azure SQL Database for invitation data
# This script uses Azure AD authentication

set -e

RESOURCE_GROUP="rg-vendor-mdm-dev-v3"
SQL_SERVER="sql-vendor-mdm-dev-fmkijlt6yfyeq"
SQL_DB="VendorMdmDb"

echo "🔍 Querying Azure SQL Database for Invitation Data..."
echo ""
echo "Server: $SQL_SERVER"
echo "Database: $SQL_DB"
echo ""

# Get access token for SQL
echo "🔑 Getting Azure AD access token..."
ACCESS_TOKEN=$(az account get-access-token --resource https://database.windows.net --query accessToken -o tsv)

if [ -z "$ACCESS_TOKEN" ]; then
    echo "❌ Failed to get access token"
    exit 1
fi

echo "✅ Access token obtained"
echo ""

# Query using sqlcmd if available, otherwise provide instructions
if command -v sqlcmd &> /dev/null; then
    echo "📊 Querying VendorInvitations table..."
    echo ""
    
    sqlcmd -S "$SQL_SERVER.database.windows.net" -d "$SQL_DB" -G -P "$ACCESS_TOKEN" -Q "SELECT TOP 10 Id, VendorId, ContactEmail, Status, CreatedAt, ExpiresAt, InvitedBy FROM VendorInvitations ORDER BY CreatedAt DESC"
else
    echo "⚠️  sqlcmd not found. Install using:"
    echo "   brew install sqlcmd"
    echo ""
    echo "Or use Azure Portal Query Editor:"
    echo "   https://portal.azure.com/#@$(az account show --query tenantId -o tsv)/resource/subscriptions/$(az account show --query id -o tsv)/resourceGroups/$RESOURCE_GROUP/providers/Microsoft.Sql/servers/$SQL_SERVER/databases/$SQL_DB/queryEditor"
    echo ""
    echo "Query to run:"
    echo "   SELECT TOP 10 Id, VendorId, ContactEmail, Status, CreatedAt, ExpiresAt, InvitedBy"
    echo "   FROM VendorInvitations"
    echo "   ORDER BY CreatedAt DESC"
fi
