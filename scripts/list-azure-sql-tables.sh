#!/bin/bash

# List tables in Azure SQL Database
# This script uses Azure AD authentication

set -e

RESOURCE_GROUP="rg-vendor-mdm-dev-v3"
SQL_SERVER="sql-vendor-mdm-dev-fmkijlt6yfyeq"
SQL_DB="VendorMdmDb"

echo "🔍 Listing tables in Azure SQL Database..."
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

# Query using sqlcmd
if command -v sqlcmd &> /dev/null; then
    echo "📊 Querying table list..."
    echo ""
    
    sqlcmd -S "$SQL_SERVER.database.windows.net" -d "$SQL_DB" -G -P "$ACCESS_TOKEN" -Q "SELECT TABLE_SCHEMA, TABLE_NAME, TABLE_TYPE FROM INFORMATION_SCHEMA.TABLES ORDER BY TABLE_SCHEMA, TABLE_NAME"
else
    echo "⚠️  sqlcmd not found"
    exit 1
fi
