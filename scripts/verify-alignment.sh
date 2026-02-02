#!/bin/bash

# Vendor MDM Portal - Full Stack Alignment Verification
# This script verifies that Backend, Frontend, and Database are all aligned

set -e

echo "════════════════════════════════════════════════════════════"
echo "🔍 VENDOR MDM PORTAL - ALIGNMENT VERIFICATION"
echo "════════════════════════════════════════════════════════════"
echo ""

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

ERRORS=0
WARNINGS=0

# Function to print status
print_status() {
    if [ $1 -eq 0 ]; then
        echo -e "${GREEN}✓${NC} $2"
    else
        echo -e "${RED}✗${NC} $2"
        ((ERRORS++))
    fi
}

print_warning() {
    echo -e "${YELLOW}⚠${NC} $1"
    ((WARNINGS++))
}

print_info() {
    echo -e "${BLUE}ℹ${NC} $1"
}

# ═══════════════════════════════════════════════════════════════
# 1. BACKEND VERIFICATION
# ═══════════════════════════════════════════════════════════════
echo "═══════════════════════════════════════════════════════════════"
echo "1️⃣  BACKEND VERIFICATION"
echo "═══════════════════════════════════════════════════════════════"

cd backend/VendorMdm.Api

# Check if backend builds
echo ""
print_info "Building backend..."
if dotnet build --configuration Release > /dev/null 2>&1; then
    print_status 0 "Backend builds successfully"
else
    print_status 1 "Backend build FAILED"
fi

# Check for compilation warnings
WARNINGS_COUNT=$(dotnet build 2>&1 | grep -c "warning" || true)
if [ $WARNINGS_COUNT -gt 0 ]; then
    print_warning "Backend has $WARNINGS_COUNT compilation warnings"
fi

# Check migrations exist
echo ""
print_info "Checking database migrations..."
MIGRATION_COUNT=$(find Migrations -name "*.cs" ! -name "*Designer.cs" ! -name "*Snapshot.cs" | wc -l | tr -d ' ')
if [ $MIGRATION_COUNT -gt 0 ]; then
    print_status 0 "Found $MIGRATION_COUNT migrations"
else
    print_status 1 "No migrations found"
fi

# Check for massive migrations (>1000 lines)
echo ""
print_info "Checking for oversized migrations..."
LARGE_MIGRATIONS=$(find Migrations -name "*.cs" ! -name "*Designer.cs" ! -name "*Snapshot.cs" -exec wc -l {} \; | awk '$1 > 1000 {print $2}')
if [ -z "$LARGE_MIGRATIONS" ]; then
    print_status 0 "No oversized migrations (all < 1000 lines)"
else
    print_warning "Found large migrations:\n$LARGE_MIGRATIONS"
fi

# Check DbContext configuration
echo ""
print_info "Checking DbContext configuration..."
if grep -q "SqlDbContext" Data/SqlDbContext.cs; then
    print_status 0 "SqlDbContext found"
else
    print_status 1 "SqlDbContext NOT found"
fi

cd ../..

# ═══════════════════════════════════════════════════════════════
# 2. FRONTEND VERIFICATION
# ═══════════════════════════════════════════════════════════════
echo ""
echo "═══════════════════════════════════════════════════════════════"
echo "2️⃣  FRONTEND VERIFICATION"
echo "═══════════════════════════════════════════════════════════════"

cd frontend

# Check if node_modules exists
echo ""
print_info "Checking dependencies..."
if [ -d "node_modules" ]; then
    print_status 0 "node_modules exists"
else
    print_warning "node_modules not found - run 'npm install'"
fi

# Check if frontend builds
echo ""
print_info "Building frontend..."
if npm run build > /dev/null 2>&1; then
    print_status 0 "Frontend builds successfully"
else
    print_status 1 "Frontend build FAILED"
fi

# Check TypeScript compilation
echo ""
print_info "Checking TypeScript..."
if npx tsc --noEmit > /dev/null 2>&1; then
    print_status 0 "TypeScript compiles without errors"
else
    print_warning "TypeScript has compilation errors"
fi

# Check for console.log statements (code smell)
echo ""
print_info "Checking for debug statements..."
CONSOLE_LOGS=$(grep -r "console.log" src --include="*.tsx" --include="*.ts" | wc -l | tr -d ' ')
if [ $CONSOLE_LOGS -lt 5 ]; then
    print_status 0 "Minimal console.log usage ($CONSOLE_LOGS found)"
else
    print_warning "Found $CONSOLE_LOGS console.log statements (consider removing)"
fi

cd ..

# ═══════════════════════════════════════════════════════════════
# 3. API CONTRACT VERIFICATION
# ═══════════════════════════════════════════════════════════════
echo ""
echo "═══════════════════════════════════════════════════════════════"
echo "3️⃣  API CONTRACT VERIFICATION"
echo "═══════════════════════════════════════════════════════════════"

# Check backend controllers
echo ""
print_info "Checking backend API endpoints..."
BACKEND_CONTROLLERS=$(find backend/VendorMdm.Api/Controllers -name "*Controller.cs" | wc -l | tr -d ' ')
print_status 0 "Found $BACKEND_CONTROLLERS backend controllers"

# Check frontend services
echo ""
print_info "Checking frontend API services..."
FRONTEND_SERVICES=$(find frontend/src/services -name "*Service.ts" 2>/dev/null | wc -l | tr -d ' ')
if [ $FRONTEND_SERVICES -gt 0 ]; then
    print_status 0 "Found $FRONTEND_SERVICES frontend services"
else
    print_warning "No frontend services found"
fi

# Check for API base URL configuration
echo ""
print_info "Checking API configuration..."
if grep -q "VITE_API_URL" frontend/.env.example 2>/dev/null || grep -q "apiClient" frontend/src/lib/apiClient.ts 2>/dev/null; then
    print_status 0 "API client configuration found"
else
    print_warning "API client configuration may be missing"
fi

# ═══════════════════════════════════════════════════════════════
# 4. AUTHENTICATION ALIGNMENT
# ═══════════════════════════════════════════════════════════════
echo ""
echo "═══════════════════════════════════════════════════════════════"
echo "4️⃣  AUTHENTICATION ALIGNMENT"
echo "═══════════════════════════════════════════════════════════════"

# Check backend JWT configuration
echo ""
print_info "Checking backend JWT configuration..."
if grep -q "JwtBearer" backend/VendorMdm.Api/Program.cs; then
    print_status 0 "JWT authentication configured in backend"
else
    print_warning "JWT authentication may not be configured"
fi

# Check frontend auth context
echo ""
print_info "Checking frontend auth context..."
if [ -f "frontend/src/context/AuthContext.tsx" ]; then
    print_status 0 "AuthContext found in frontend"
else
    print_warning "AuthContext not found in frontend"
fi

# Check for auth token handling
echo ""
print_info "Checking auth token handling..."
if grep -q "authToken" frontend/src/lib/apiClient.ts 2>/dev/null || grep -q "Authorization" frontend/src/lib/apiClient.ts 2>/dev/null; then
    print_status 0 "Auth token handling found in API client"
else
    print_warning "Auth token handling may be missing"
fi

# ═══════════════════════════════════════════════════════════════
# 5. DATABASE SCHEMA VERIFICATION
# ═══════════════════════════════════════════════════════════════
echo ""
echo "═══════════════════════════════════════════════════════════════"
echo "5️⃣  DATABASE SCHEMA VERIFICATION"
echo "═══════════════════════════════════════════════════════════════"

# Check for DbContext entities
echo ""
print_info "Checking entity models..."
ENTITY_COUNT=$(grep -c "public DbSet<" backend/VendorMdm.Api/Data/SqlDbContext.cs 2>/dev/null || echo "0")
if [ $ENTITY_COUNT -gt 0 ]; then
    print_status 0 "Found $ENTITY_COUNT entity models in DbContext"
else
    print_warning "No entity models found in DbContext"
fi

# Check for migration snapshot
echo ""
print_info "Checking migration snapshot..."
if [ -f "backend/VendorMdm.Api/Migrations/SqlDbContextModelSnapshot.cs" ]; then
    print_status 0 "Migration snapshot exists"
    SNAPSHOT_SIZE=$(wc -l < backend/VendorMdm.Api/Migrations/SqlDbContextModelSnapshot.cs | tr -d ' ')
    print_info "Snapshot size: $SNAPSHOT_SIZE lines"
else
    print_status 1 "Migration snapshot NOT found"
fi

# ═══════════════════════════════════════════════════════════════
# 6. CONFIGURATION ALIGNMENT
# ═══════════════════════════════════════════════════════════════
echo ""
echo "═══════════════════════════════════════════════════════════════"
echo "6️⃣  CONFIGURATION ALIGNMENT"
echo "═══════════════════════════════════════════════════════════════"

# Check backend appsettings
echo ""
print_info "Checking backend configuration..."
if [ -f "backend/VendorMdm.Api/appsettings.json" ]; then
    print_status 0 "appsettings.json exists"
else
    print_status 1 "appsettings.json NOT found"
fi

if [ -f "backend/VendorMdm.Api/appsettings.Development.json" ]; then
    print_status 0 "appsettings.Development.json exists"
else
    print_warning "appsettings.Development.json NOT found"
fi

# Check frontend environment files
echo ""
print_info "Checking frontend configuration..."
if [ -f "frontend/.env.example" ]; then
    print_status 0 ".env.example exists"
else
    print_warning ".env.example NOT found"
fi

# Check vite config
if [ -f "frontend/vite.config.ts" ]; then
    print_status 0 "vite.config.ts exists"
    
    # Check proxy configuration
    if grep -q "proxy" frontend/vite.config.ts; then
        print_status 0 "Proxy configuration found"
    else
        print_warning "Proxy configuration may be missing"
    fi
else
    print_status 1 "vite.config.ts NOT found"
fi

# ═══════════════════════════════════════════════════════════════
# 7. DEPLOYMENT READINESS
# ═══════════════════════════════════════════════════════════════
echo ""
echo "═══════════════════════════════════════════════════════════════"
echo "7️⃣  DEPLOYMENT READINESS"
echo "═══════════════════════════════════════════════════════════════"

# Check GitHub workflows
echo ""
print_info "Checking CI/CD workflows..."
WORKFLOW_COUNT=$(find .github/workflows -name "*.yml" 2>/dev/null | wc -l | tr -d ' ')
if [ $WORKFLOW_COUNT -gt 0 ]; then
    print_status 0 "Found $WORKFLOW_COUNT GitHub workflows"
else
    print_warning "No GitHub workflows found"
fi

# Check for deployment scripts
echo ""
print_info "Checking deployment scripts..."
if [ -d "scripts" ]; then
    SCRIPT_COUNT=$(find scripts -name "*.sh" | wc -l | tr -d ' ')
    print_status 0 "Found $SCRIPT_COUNT deployment scripts"
else
    print_warning "No scripts directory found"
fi

# ═══════════════════════════════════════════════════════════════
# 8. FINAL SUMMARY
# ═══════════════════════════════════════════════════════════════
echo ""
echo "═══════════════════════════════════════════════════════════════"
echo "📊 VERIFICATION SUMMARY"
echo "═══════════════════════════════════════════════════════════════"
echo ""

if [ $ERRORS -eq 0 ] && [ $WARNINGS -eq 0 ]; then
    echo -e "${GREEN}✓ ALL CHECKS PASSED${NC}"
    echo ""
    echo "🎉 Backend, Frontend, and Database are fully aligned!"
    exit 0
elif [ $ERRORS -eq 0 ]; then
    echo -e "${YELLOW}⚠ PASSED WITH WARNINGS${NC}"
    echo ""
    echo "Errors: $ERRORS"
    echo "Warnings: $WARNINGS"
    echo ""
    echo "✓ No critical issues found"
    echo "⚠ Please review warnings above"
    exit 0
else
    echo -e "${RED}✗ VERIFICATION FAILED${NC}"
    echo ""
    echo "Errors: $ERRORS"
    echo "Warnings: $WARNINGS"
    echo ""
    echo "❌ Critical issues found - please fix errors above"
    exit 1
fi
