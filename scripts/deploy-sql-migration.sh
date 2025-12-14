#!/bin/bash

# Deploy Database Migration to Azure SQL
# Uses Azure AD authentication to execute migration script

set -e

RESOURCE_GROUP="rg-vendor-mdm-dev-v3"
SQL_SERVER="sql-vendor-mdm-dev"
SQL_DB="mdmportal-sqldb-dev"
MIGRATION_SCRIPT="docs/azure-sql-safe-migration.sql"

echo "=== Azure SQL Database Migration ==="
echo ""
echo "Server: $SQL_SERVER"
echo "Database: $SQL_DB"
echo "Migration Script: $MIGRATION_SCRIPT"
echo ""

# Check if migration script exists
if [ ! -f "$MIGRATION_SCRIPT" ]; then
    echo "❌ Migration script not found: $MIGRATION_SCRIPT"
    exit 1
fi

# Get Azure AD access token
echo "🔑 Getting Azure AD access token..."
ACCESS_TOKEN=$(az account get-access-token --resource https://database.windows.net --query accessToken -o tsv)

if [ -z "$ACCESS_TOKEN" ]; then
    echo "❌ Failed to get access token"
    exit 1
fi

echo "✅ Access token obtained"
echo ""

# Check if sqlcmd is available
if command -v sqlcmd &> /dev/null; then
    echo "📊 Executing migration script..."
    echo ""
    
    sqlcmd -S "$SQL_SERVER.database.windows.net" \
           -d "$SQL_DB" \
           -G \
           -P "$ACCESS_TOKEN" \
           -i "$MIGRATION_SCRIPT"
    
    echo ""
    echo "✅ Migration executed successfully!"
    echo ""
    echo "📋 Verifying tables..."
    sqlcmd -S "$SQL_SERVER.database.windows.net" \
           -d "$SQL_DB" \
           -G \
           -P "$ACCESS_TOKEN" \
           -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME IN ('Vendors', 'VendorInvitationsCanonical', 'ChangeRequestsCanonical', 'ExternalSystemMappings') ORDER BY TABLE_NAME"
else
    echo "⚠️  sqlcmd not installed"
    echo ""
    echo "Option 1: Install sqlcmd"
    echo "   brew install microsoft/mssql-release/mssql-tools18"
    echo ""
    echo "Option 2: Use Azure Portal Query Editor"
    echo "   1. Go to: https://portal.azure.com"
    echo "   2. Navigate to SQL Database: $SQL_DB"
    echo "   3. Open Query Editor"
    echo "   4. Copy and paste contents of: $MIGRATION_SCRIPT"
    echo "   5. Click Run"
    echo ""
    echo "Option 3: Use Azure CLI (alternative)"
    echo "   az sql db show-connection-string --client sqlcmd --name $SQL_DB"
    exit 1
fi
