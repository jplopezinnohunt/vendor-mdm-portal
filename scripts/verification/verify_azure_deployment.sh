#!/bin/bash

# Azure Deployment Verification Script
# Verifies the health of the deployed Azure resources (Backend API and Frontend SWA)

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[0;33m'
NC='\033[0m' # No Color

# URLs
BACKEND_URL="https://app-vendor-mdm-api-dev.azurewebsites.net/api/system/data-sources"
FRONTEND_URL="https://thankful-field-0258f8110.3.azurestaticapps.net"

echo "═══════════════════════════════════════════════════════════"
echo "✅ AZURE DEPLOYMENT VERIFICATION"
echo "═══════════════════════════════════════════════════════════"
echo -e "${YELLOW}Wait: Ensuring Backend Wake-up (Cold Start)...${NC}"

# 1. Check Backend
echo ""
echo "🔍 Checking Backend API Health ($BACKEND_URL)..."
echo "   (Timeout set to 45s to accommodate Azure Cold Start)"

BACKEND_RESPONSE=$(curl -s -m 45 "$BACKEND_URL")
CURL_EXIT_CODE=$?

if [ $CURL_EXIT_CODE -eq 0 ]; then
    echo -e "${GREEN}✓ Backend Operational${NC}"
    
    # Parse status using rudimentary string matching (since jq might not be available everywhere, but ideally usage of jq)
    if echo "$BACKEND_RESPONSE" | grep -q '"database":{"type":"SQL Server","mode":"Connected"'; then
        echo -e "${GREEN}✓ Database: Connected (SQL Server)${NC}"
    else
        echo -e "${RED}✗ Database: Issue Detected${NC}"
        echo "Response excerpt: $(echo "$BACKEND_RESPONSE" | cut -c 1-100)..."
    fi

    if echo "$BACKEND_RESPONSE" | grep -q '"email":{"mode":"Azure Communication"'; then
        echo -e "${GREEN}✓ Email: Azure Communication Services${NC}"
    elif echo "$BACKEND_RESPONSE" | grep -q '"email":{"mode":"SMTP"'; then
        echo -e "${YELLOW}✓ Email: SMTP Fallback${NC}"
    else
        echo -e "${YELLOW}! Email: Not Fully Configured (Check Logs)${NC}"
    fi

else
    echo -e "${RED}✗ Backend Verification Failed (Exit Code: $CURL_EXIT_CODE)${NC}"
    if [ $CURL_EXIT_CODE -eq 28 ]; then
        echo "   Reason: Connection Timed Out (Possible Cold Start or Crash)"
    else
        echo "   Reason: Connection Failed"
    fi
fi

# 2. Check Frontend
echo ""
echo "🔍 Checking Frontend Availability ($FRONTEND_URL)..."
FRONTEND_HTTP_CODE=$(curl -o /dev/null -s -w "%{http_code}\n" "$FRONTEND_URL")

if [ "$FRONTEND_HTTP_CODE" == "200" ]; then
    echo -e "${GREEN}✓ Frontend Operational (HTTP 200)${NC}"
else
    echo -e "${RED}✗ Frontend Verification Failed (HTTP $FRONTEND_HTTP_CODE)${NC}"
fi

echo ""
echo "═══════════════════════════════════════════════════════════"
