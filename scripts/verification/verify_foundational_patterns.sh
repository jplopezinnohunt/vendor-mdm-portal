#!/bin/bash
# Verification Script: Foundational Patterns Hardening
# Spec: specs/spec_foundational_patterns_hardening.md
# Date: 2026-02-04

set -e

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

PASS_COUNT=0
FAIL_COUNT=0
WARN_COUNT=0

pass() {
    echo -e "${GREEN}✓ PASS${NC}: $1"
    ((PASS_COUNT++))
}

fail() {
    echo -e "${RED}✗ FAIL${NC}: $1"
    ((FAIL_COUNT++))
}

warn() {
    echo -e "${YELLOW}⚠ WARN${NC}: $1"
    ((WARN_COUNT++))
}

echo "============================================"
echo "Foundational Patterns Verification"
echo "============================================"
echo ""

# Navigate to project root
cd "$(dirname "$0")/../.."

# ===========================================
# PHASE 1: Soft Delete Verification
# ===========================================
echo "--- Phase 1: Soft Delete Infrastructure ---"

# Check ISoftDeletable interface exists
if [ -f "backend/VendorMdm.Shared/Ontology/Interfaces/ISoftDeletable.cs" ]; then
    pass "ISoftDeletable interface exists"
else
    fail "ISoftDeletable interface missing"
fi

# Check CanonicalEntityBase has soft delete fields
if grep -q "IsDeleted" "backend/VendorMdm.Shared/Models/CanonicalEntityBase.cs" 2>/dev/null; then
    pass "CanonicalEntityBase has IsDeleted field"
else
    fail "CanonicalEntityBase missing IsDeleted field"
fi

if grep -q "DeletedAt" "backend/VendorMdm.Shared/Models/CanonicalEntityBase.cs" 2>/dev/null; then
    pass "CanonicalEntityBase has DeletedAt field"
else
    fail "CanonicalEntityBase missing DeletedAt field"
fi

if grep -q "DeletedBy" "backend/VendorMdm.Shared/Models/CanonicalEntityBase.cs" 2>/dev/null; then
    pass "CanonicalEntityBase has DeletedBy field"
else
    fail "CanonicalEntityBase missing DeletedBy field"
fi

# Check query filters in DbContext
if grep -q "HasQueryFilter.*IsDeleted" "backend/VendorMdm.Api/Data/SqlDbContext.cs" 2>/dev/null; then
    pass "SqlDbContext has soft delete query filters"
else
    fail "SqlDbContext missing soft delete query filters"
fi

# ===========================================
# PHASE 2: Status Constants Verification
# ===========================================
echo ""
echo "--- Phase 2: Status Constants ---"

# Check each status constants file
# Note: DocumentStatus is in DocumentConstants.cs, not a separate file
for file in ApplicationStatus UserStatus GdprStatus InvitationStatus VendorTypes AuditActions; do
    if [ -f "backend/VendorMdm.Shared/Constants/${file}.cs" ]; then
        pass "${file}.cs exists"
    else
        fail "${file}.cs missing"
    fi
done

# Check DocumentStatus exists in DocumentConstants.cs
if grep -q "public static class DocumentStatus" backend/VendorMdm.Shared/Constants/DocumentConstants.cs 2>/dev/null; then
    pass "DocumentStatus exists in DocumentConstants.cs"
else
    fail "DocumentStatus missing from DocumentConstants.cs"
fi

# Check no hardcoded status strings in controllers
echo ""
echo "Checking for hardcoded status strings..."

HARDCODED_COUNT=0

# Check specific patterns that should be replaced
if grep -n '"Submitted"' backend/VendorMdm.Api/Controllers/*.cs 2>/dev/null | grep -v "ApplicationStatus.Submitted" | grep -v "//"; then
    warn "Hardcoded 'Submitted' found in controllers"
    ((HARDCODED_COUNT++))
fi

if grep -n '"PendingReview"' backend/VendorMdm.Api/Controllers/*.cs 2>/dev/null | grep -v "ApplicationStatus.PendingReview" | grep -v "//"; then
    warn "Hardcoded 'PendingReview' found in controllers"
    ((HARDCODED_COUNT++))
fi

if grep -n '"Draft"' backend/VendorMdm.Api/Controllers/*.cs 2>/dev/null | grep -v "ApplicationStatus.Draft" | grep -v "//"; then
    warn "Hardcoded 'Draft' found in controllers"
    ((HARDCODED_COUNT++))
fi

if [ $HARDCODED_COUNT -eq 0 ]; then
    pass "No hardcoded status strings in controllers"
fi

# ===========================================
# PHASE 3: DTO Pattern Verification
# ===========================================
echo ""
echo "--- Phase 3: DTO Pattern ---"

# Check each DTO file
for dto in EmployeeDto ProjectDto FundDto UserDto CustomerDto; do
    if [ -f "backend/VendorMdm.Shared/Contracts/Dtos/${dto}.cs" ]; then
        pass "${dto}.cs exists"
    else
        fail "${dto}.cs missing"
    fi
done

# Check mapping extensions
if [ -f "backend/VendorMdm.Shared/Contracts/Mappings/EntityMappingExtensions.cs" ]; then
    pass "EntityMappingExtensions.cs exists"
else
    fail "EntityMappingExtensions.cs missing"
fi

# Check controllers return DTOs not entities
echo ""
echo "Checking controller return types..."

ENTITY_LEAK_COUNT=0

# Check VendorController
if grep -q "ActionResult<Vendor>" backend/VendorMdm.Api/Controllers/VendorController.cs 2>/dev/null; then
    fail "VendorController returns Vendor entity (should be VendorDto)"
    ((ENTITY_LEAK_COUNT++))
else
    pass "VendorController returns DTOs"
fi

# Check UserController
if grep -q "ActionResult<User>" backend/VendorMdm.Api/Controllers/UserController.cs 2>/dev/null; then
    fail "UserController returns User entity (should be UserDto)"
    ((ENTITY_LEAK_COUNT++))
else
    pass "UserController returns DTOs"
fi

# Check EmployeeController
if grep -q "ActionResult<Employee>" backend/VendorMdm.Api/Controllers/EmployeeController.cs 2>/dev/null; then
    fail "EmployeeController returns Employee entity (should be EmployeeDto)"
    ((ENTITY_LEAK_COUNT++))
else
    pass "EmployeeController returns DTOs"
fi

# ===========================================
# PHASE 4: Evolution Infrastructure
# ===========================================
echo ""
echo "--- Phase 4: Evolution Infrastructure ---"

if [ -f "backend/VendorMdm.Api/Middleware/ApiVersionHeaderMiddleware.cs" ]; then
    pass "ApiVersionHeaderMiddleware exists"
else
    fail "ApiVersionHeaderMiddleware missing"
fi

# ===========================================
# BUILD VERIFICATION
# ===========================================
echo ""
echo "--- Build Verification ---"

echo "Building backend..."
cd backend/VendorMdm.Api
if dotnet build --configuration Release --nologo -v q 2>&1 | tail -5; then
    pass "Backend build succeeded"
else
    fail "Backend build failed"
fi
cd ../..

echo ""
echo "Building frontend..."
cd frontend
if npm run build --silent 2>&1 | tail -3; then
    pass "Frontend build succeeded"
else
    fail "Frontend build failed"
fi
cd ..

# ===========================================
# SUMMARY
# ===========================================
echo ""
echo "============================================"
echo "VERIFICATION SUMMARY"
echo "============================================"
echo -e "Passed: ${GREEN}${PASS_COUNT}${NC}"
echo -e "Failed: ${RED}${FAIL_COUNT}${NC}"
echo -e "Warnings: ${YELLOW}${WARN_COUNT}${NC}"
echo ""

if [ $FAIL_COUNT -gt 0 ]; then
    echo -e "${RED}VERIFICATION FAILED${NC}"
    echo "Please fix the failing checks before committing."
    exit 1
else
    echo -e "${GREEN}VERIFICATION PASSED${NC}"
    exit 0
fi
