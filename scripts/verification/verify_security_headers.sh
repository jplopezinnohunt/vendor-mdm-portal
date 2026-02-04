#!/bin/bash
# Verification Script: Security Headers & Input Sanitization
# Spec: specs/spec_security_headers_middleware.md
# Date: 2026-02-03

set -e

echo "🔍 Security Headers & Input Sanitization Verification"
echo "======================================================"
echo ""

# Color codes for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

PASS_COUNT=0
FAIL_COUNT=0

# Function to check if API is running
check_api_running() {
    echo "🔧 Checking if API is running..."
    if curl -s http://localhost:5001/api/health > /dev/null 2>&1; then
        echo -e "${GREEN}✅ API is running on http://localhost:5001${NC}"
        return 0
    else
        echo -e "${RED}❌ API is not running. Please start the API first:${NC}"
        echo "   cd backend/VendorMdm.Api && dotnet run"
        exit 1
    fi
}

# Function to pass test
pass_test() {
    echo -e "${GREEN}✅ $1${NC}"
    ((PASS_COUNT++))
}

# Function to fail test
fail_test() {
    echo -e "${RED}❌ $1${NC}"
    ((FAIL_COUNT++))
}

# Function to warn test
warn_test() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

# Test 1: Security Headers Present
test_security_headers() {
    echo ""
    echo "Test 1: Verifying security headers..."
    echo "--------------------------------------"

    RESPONSE=$(curl -sI http://localhost:5001/api/health)

    # X-Frame-Options
    if echo "$RESPONSE" | grep -qi "X-Frame-Options: DENY"; then
        pass_test "X-Frame-Options: DENY header present"
    else
        fail_test "X-Frame-Options header missing or incorrect"
    fi

    # X-Content-Type-Options
    if echo "$RESPONSE" | grep -qi "X-Content-Type-Options: nosniff"; then
        pass_test "X-Content-Type-Options: nosniff header present"
    else
        fail_test "X-Content-Type-Options header missing or incorrect"
    fi

    # Referrer-Policy
    if echo "$RESPONSE" | grep -qi "Referrer-Policy"; then
        pass_test "Referrer-Policy header present"
    else
        fail_test "Referrer-Policy header missing"
    fi

    # Content-Security-Policy
    if echo "$RESPONSE" | grep -qi "Content-Security-Policy"; then
        pass_test "Content-Security-Policy header present"
    else
        fail_test "Content-Security-Policy header missing"
    fi

    # X-XSS-Protection (optional but recommended)
    if echo "$RESPONSE" | grep -qi "X-XSS-Protection"; then
        pass_test "X-XSS-Protection header present"
    else
        warn_test "X-XSS-Protection header missing (optional)"
    fi

    # HSTS (only in HTTPS/Production)
    if echo "$RESPONSE" | grep -qi "Strict-Transport-Security"; then
        pass_test "Strict-Transport-Security (HSTS) header present"
    else
        warn_test "HSTS header missing (expected in Development/HTTP)"
    fi
}

# Test 2: CORS Configuration
test_cors_config() {
    echo ""
    echo "Test 2: Verifying CORS configuration..."
    echo "----------------------------------------"

    # Test CORS preflight
    CORS_RESPONSE=$(curl -s -I -H "Origin: http://localhost:3000" \
        -H "Access-Control-Request-Method: GET" \
        -X OPTIONS http://localhost:5001/api/health)

    if echo "$CORS_RESPONSE" | grep -qi "Access-Control-Allow-Origin"; then
        pass_test "CORS configured (Development mode allows localhost)"
    else
        warn_test "CORS check inconclusive (verify environment configuration)"
    fi

    # Check for environment-based CORS in code
    if grep -q "GetAllowedOrigins" backend/VendorMdm.Api/Program.cs; then
        pass_test "Environment-based CORS function found in Program.cs"
    else
        fail_test "Environment-based CORS not implemented"
    fi
}

# Test 3: IInputSanitizer Registration
test_input_sanitizer() {
    echo ""
    echo "Test 3: Verifying IInputSanitizer implementation..."
    echo "----------------------------------------------------"

    # Check if IInputSanitizer exists in Core.Framework
    if [ -f "backend/VendorMdm.Core.Framework/Security/IInputSanitizer.cs" ] || \
       grep -rq "interface IInputSanitizer" backend/VendorMdm.Core.Framework/; then
        pass_test "IInputSanitizer interface exists in Core.Framework"
    else
        fail_test "IInputSanitizer interface not found"
    fi

    # Check if IInputSanitizer is registered in DI
    if grep -q "IInputSanitizer" backend/VendorMdm.Api/Program.cs; then
        pass_test "IInputSanitizer registered in Program.cs"
    else
        fail_test "IInputSanitizer not registered in DI"
    fi

    # Check if InputSanitizationActionFilter exists
    if [ -f "backend/VendorMdm.Api/Filters/InputSanitizationActionFilter.cs" ] || \
       grep -rq "InputSanitizationActionFilter" backend/VendorMdm.Api/; then
        pass_test "InputSanitizationActionFilter implemented"
    else
        fail_test "InputSanitizationActionFilter not found"
    fi
}

# Test 4: SecurityHeadersMiddleware Registration
test_middleware_registration() {
    echo ""
    echo "Test 4: Verifying SecurityHeadersMiddleware..."
    echo "-----------------------------------------------"

    # Check if middleware file exists
    if [ -f "backend/VendorMdm.Api/Middleware/SecurityHeadersMiddleware.cs" ]; then
        pass_test "SecurityHeadersMiddleware.cs file exists"
    else
        fail_test "SecurityHeadersMiddleware.cs file not found"
    fi

    # Check if middleware is registered in Program.cs
    if grep -q "SecurityHeadersMiddleware" backend/VendorMdm.Api/Program.cs || \
       grep -q "UseSecurityHeaders" backend/VendorMdm.Api/Program.cs; then
        pass_test "SecurityHeadersMiddleware registered in Program.cs"
    else
        fail_test "SecurityHeadersMiddleware not registered"
    fi
}

# Test 5: Configuration Files
test_configuration() {
    echo ""
    echo "Test 5: Verifying security configuration..."
    echo "--------------------------------------------"

    # Check for Security section in appsettings
    if grep -q "\"Security\"" backend/VendorMdm.Api/appsettings.json; then
        pass_test "Security configuration section found in appsettings.json"
    else
        warn_test "Security configuration section not found (may be optional)"
    fi

    # Check for App:BaseUrl configuration
    if grep -q "\"App\"" backend/VendorMdm.Api/appsettings.json && \
       grep -q "\"BaseUrl\"" backend/VendorMdm.Api/appsettings.json; then
        pass_test "App:BaseUrl configuration found"
    else
        fail_test "App:BaseUrl configuration missing"
    fi
}

# Test 6: Build Verification
test_build() {
    echo ""
    echo "Test 6: Verifying build succeeds..."
    echo "------------------------------------"

    cd backend/VendorMdm.Api

    if dotnet build --configuration Release --no-restore > /dev/null 2>&1; then
        pass_test "Backend builds successfully (Release configuration)"
    else
        fail_test "Backend build failed"
        dotnet build --configuration Release --no-restore
    fi

    cd ../..
}

# Main execution
main() {
    check_api_running
    test_security_headers
    test_cors_config
    test_input_sanitizer
    test_middleware_registration
    test_configuration
    test_build

    # Summary
    echo ""
    echo "======================================================"
    echo "📊 Test Summary"
    echo "======================================================"
    echo -e "Passed: ${GREEN}${PASS_COUNT}${NC}"
    echo -e "Failed: ${RED}${FAIL_COUNT}${NC}"
    echo ""

    if [ $FAIL_COUNT -eq 0 ]; then
        echo -e "${GREEN}✅ ALL SECURITY CHECKS PASSED${NC}"
        echo "======================================================"
        exit 0
    else
        echo -e "${RED}❌ SOME CHECKS FAILED${NC}"
        echo "Please fix the failing tests before proceeding."
        echo "======================================================"
        exit 1
    fi
}

main
