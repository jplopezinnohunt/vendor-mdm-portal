#!/bin/bash

# End-to-End Verification Script for Azure Integration
# Tests SQL Database, Cosmos DB, and Service Bus

set -e

RESOURCE_GROUP="rg-vendor-mdm-dev-v3"
SQL_SERVER="sql-vendor-mdm-dev-fmkijlt6yfyeq"
SQL_DATABASE="VendorMdmDb"
COSMOS_ACCOUNT="cosmos-vendor-mdm-dev-fmkijlt6yfyeq"
API_URL="https://app-vendor-mdm-api-dev-fmkijlt6yfyeq.azurewebsites.net"

echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "🧪 END-TO-END AZURE INTEGRATION VERIFICATION"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""

# 1. Check Azure SQL Database Tables
echo "1️⃣  Checking Azure SQL Database Tables..."
echo ""

if command -v sqlcmd &> /dev/null; then
    echo "   Querying table list..."
    sqlcmd -S "${SQL_SERVER}.database.windows.net" -d "$SQL_DATABASE" -G -Q \
        "SELECT TABLE_NAME, (SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME)) AS ColumnCount FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' ORDER BY TABLE_NAME" \
        -o /tmp/sql-tables.txt 2>&1 || echo "   ⚠️ SQL query may have failed"
    
    if [ -f /tmp/sql-tables.txt ]; then
        echo "   Tables found:"
        cat /tmp/sql-tables.txt | grep -v "^-" | grep -v "^$" | head -20
    fi
    
    echo ""
    echo "   Querying VendorInvitations count..."
    sqlcmd -S "${SQL_SERVER}.database.windows.net" -d "$SQL_DATABASE" -G -Q \
        "SELECT COUNT(*) AS InvitationCount FROM VendorInvitations" 2>&1 || echo "   ⚠️ Table may not exist yet"
else
    echo "   ⚠️ sqlcmd not installed - skipping SQL verification"
fi

echo ""

# 2. Check Cosmos DB Containers
echo "2️⃣  Checking Cosmos DB Containers..."
echo ""

echo "   Listing containers..."
az cosmosdb sql container list \
    --resource-group "$RESOURCE_GROUP" \
    --account-name "$COSMOS_ACCOUNT" \
    --database-name "VendorMdm" \
    --query "[].{Name:id}" -o table

echo ""

# 3. Test API Endpoints
echo "3️⃣  Testing API Endpoints..."
echo ""

echo "   Testing GET /api/invitation/list..."
RESPONSE=$(curl -s -w "\n%{http_code}" "${API_URL}/api/invitation/list")
HTTP_CODE=$(echo "$RESPONSE" | tail -n 1)
BODY=$(echo "$RESPONSE" | sed '$d')

if [ "$HTTP_CODE" = "200" ]; then
    echo "   ✅ API is responding (HTTP 200)"
    echo "   Response preview:"
    echo "$BODY" | jq -r '.invitations | length' | xargs printf "      Found %s invitations\n" 2>/dev/null || echo "      $BODY" | head -c 200
else
    echo "   ❌ API returned HTTP $HTTP_CODE"
    echo "   $BODY" | head -c 500
fi

echo ""

# 4. Summary
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "📋 VERIFICATION SUMMARY"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""
echo "✅ Next Steps:"
echo "   1. Create a test invitation via the deployed UI"
echo "   2. Rerun this script to verify data persistence"
echo "   3. Check Cosmos DB Data Explorer in Azure Portal for artifacts"
echo ""
echo "   Deployed App URL: https://purple-glacier-0b338720f.3.azurestaticapps.net"
echo ""
