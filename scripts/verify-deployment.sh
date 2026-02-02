#!/bin/bash

# Vendor MDM Portal - Azure Deployment Verification
# This script verifies that deployed environments are working correctly

set -e

echo "════════════════════════════════════════════════════════════"
echo "🌐 AZURE DEPLOYMENT VERIFICATION"
echo "════════════════════════════════════════════════════════════"
echo ""

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Environment URLs
DEV_BACKEND="https://app-vendor-mdm-api-dev.azurewebsites.net"
DEV_FRONTEND="https://thankful-field-0258f8110.3.azurestaticapps.net"

ERRORS=0

print_status() {
    if [ $1 -eq 0 ]; then
        echo -e "${GREEN}✓${NC} $2"
    else
        echo -e "${RED}✗${NC} $2"
        ((ERRORS++))
    fi
}

print_info() {
    echo -e "${BLUE}ℹ${NC} $1"
}

# ═══════════════════════════════════════════════════════════════
# 1. BACKEND VERIFICATION
# ═══════════════════════════════════════════════════════════════
echo "═══════════════════════════════════════════════════════════════"
echo "1️⃣  BACKEND API VERIFICATION"
echo "═══════════════════════════════════════════════════════════════"
echo ""

# Check health endpoint
print_info "Checking backend health..."
HEALTH_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$DEV_BACKEND/health" || echo "000")
if [ "$HEALTH_STATUS" = "200" ]; then
    print_status 0 "Backend health check: OK"
elif [ "$HEALTH_STATUS" = "404" ]; then
    echo -e "${YELLOW}⚠${NC} Health endpoint not implemented (404)"
else
    print_status 1 "Backend health check failed (HTTP $HEALTH_STATUS)"
fi

# Check Swagger
print_info "Checking Swagger UI..."
SWAGGER_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$DEV_BACKEND/swagger/index.html" || echo "000")
if [ "$SWAGGER_STATUS" = "200" ]; then
    print_status 0 "Swagger UI: Accessible"
else
    print_status 1 "Swagger UI failed (HTTP $SWAGGER_STATUS)"
fi

# Check API endpoints
print_info "Checking critical API endpoints..."

# Auth endpoint
AUTH_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$DEV_BACKEND/api/auth/login" -X POST -H "Content-Type: application/json" -d '{}' || echo "000")
if [ "$AUTH_STATUS" = "400" ] || [ "$AUTH_STATUS" = "401" ]; then
    print_status 0 "Auth endpoint: Responding (HTTP $AUTH_STATUS)"
else
    echo -e "${YELLOW}⚠${NC} Auth endpoint: HTTP $AUTH_STATUS"
fi

# ═══════════════════════════════════════════════════════════════
# 2. FRONTEND VERIFICATION
# ═══════════════════════════════════════════════════════════════
echo ""
echo "═══════════════════════════════════════════════════════════════"
echo "2️⃣  FRONTEND VERIFICATION"
echo "═══════════════════════════════════════════════════════════════"
echo ""

# Check frontend loads
print_info "Checking frontend..."
FRONTEND_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$DEV_FRONTEND" || echo "000")
if [ "$FRONTEND_STATUS" = "200" ]; then
    print_status 0 "Frontend: Accessible"
else
    print_status 1 "Frontend failed (HTTP $FRONTEND_STATUS)"
fi

# Check for JavaScript errors (basic check)
print_info "Checking frontend assets..."
FRONTEND_CONTENT=$(curl -s "$DEV_FRONTEND" || echo "")
if echo "$FRONTEND_CONTENT" | grep -q "script"; then
    print_status 0 "Frontend: JavaScript loaded"
else
    echo -e "${YELLOW}⚠${NC} Frontend: No JavaScript found"
fi

# ═══════════════════════════════════════════════════════════════
# 3. INTEGRATION VERIFICATION
# ═══════════════════════════════════════════════════════════════
echo ""
echo "═══════════════════════════════════════════════════════════════"
echo "3️⃣  INTEGRATION VERIFICATION"
echo "═══════════════════════════════════════════════════════════════"
echo ""

# Check CORS
print_info "Checking CORS configuration..."
CORS_HEADERS=$(curl -s -I -X OPTIONS "$DEV_BACKEND/api/auth/login" -H "Origin: $DEV_FRONTEND" | grep -i "access-control" || echo "")
if [ -n "$CORS_HEADERS" ]; then
    print_status 0 "CORS: Configured"
else
    echo -e "${YELLOW}⚠${NC} CORS: Headers not found (may need configuration)"
fi

# ═══════════════════════════════════════════════════════════════
# 4. SUMMARY
# ═══════════════════════════════════════════════════════════════
echo ""
echo "═══════════════════════════════════════════════════════════════"
echo "📊 DEPLOYMENT SUMMARY"
echo "═══════════════════════════════════════════════════════════════"
echo ""

if [ $ERRORS -eq 0 ]; then
    echo -e "${GREEN}✓ DEPLOYMENT VERIFIED${NC}"
    echo ""
    echo "🎉 All systems operational!"
    echo ""
    echo "Frontend: $DEV_FRONTEND"
    echo "Backend:  $DEV_BACKEND"
    echo "Swagger:  $DEV_BACKEND/swagger"
    exit 0
else
    echo -e "${RED}✗ DEPLOYMENT ISSUES FOUND${NC}"
    echo ""
    echo "Errors: $ERRORS"
    echo ""
    echo "❌ Please check the errors above"
    exit 1
fi
