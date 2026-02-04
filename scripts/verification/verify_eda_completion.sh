#!/bin/bash
#
# Verification Script: Event-Driven Architecture Completion
# Spec Reference: specs/spec_event_driven_architecture.md
# Date: 2026-02-04
#

set -u  # Exit on undefined variables
# NOTE: NOT using set -e to accumulate failures

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

FAIL_COUNT=0
PASS_COUNT=0

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "=============================================="
echo "  EDA Completion Verification Script"
echo "  Date: $(date)"
echo "=============================================="
echo ""

# Function to check and report
check() {
    local name="$1"
    local result="$2"

    if [ "$result" -eq 0 ]; then
        echo -e "${GREEN}✓ PASS${NC}: $name"
        ((PASS_COUNT++))
    else
        echo -e "${RED}✗ FAIL${NC}: $name"
        ((FAIL_COUNT++))
    fi
}

# ==============================================
# PHASE 1: Backend Build
# ==============================================
echo ""
echo "--- Phase 1: Backend Build ---"

cd "$PROJECT_ROOT/backend/VendorMdm.Api"
dotnet build --configuration Release --verbosity quiet 2>&1
check "Backend build succeeds" $?

# ==============================================
# PHASE 2: Core Framework Files Exist
# ==============================================
echo ""
echo "--- Phase 2: Core Framework Files ---"

if [ -f "$PROJECT_ROOT/backend/VendorMdm.Core.Framework/Events/IDomainEventDispatcher.cs" ]; then
    check "IDomainEventDispatcher.cs exists" 0
else
    check "IDomainEventDispatcher.cs exists" 1
fi

if [ -f "$PROJECT_ROOT/backend/VendorMdm.Core.Framework/Events/IEventHandler.cs" ]; then
    check "IEventHandler.cs exists" 0
else
    check "IEventHandler.cs exists" 1
fi

# ==============================================
# PHASE 3: Implementation Files Exist
# ==============================================
echo ""
echo "--- Phase 3: Implementation Files ---"

if [ -f "$PROJECT_ROOT/backend/VendorMdm.Api/Services/Events/DomainEventDispatcher.cs" ]; then
    check "DomainEventDispatcher.cs exists" 0
else
    check "DomainEventDispatcher.cs exists" 1
fi

if [ -f "$PROJECT_ROOT/backend/VendorMdm.Api/Hubs/EventHub.cs" ]; then
    check "EventHub.cs exists" 0
else
    check "EventHub.cs exists" 1
fi

if [ -f "$PROJECT_ROOT/backend/VendorMdm.Api/Services/Events/SignalREventHandler.cs" ]; then
    check "SignalREventHandler.cs exists" 0
else
    check "SignalREventHandler.cs exists" 1
fi

if [ -f "$PROJECT_ROOT/backend/VendorMdm.Shared/Models/OutboxEvent.cs" ]; then
    check "OutboxEvent.cs exists" 0
else
    check "OutboxEvent.cs exists" 1
fi

# ==============================================
# PHASE 4: DI Registration
# ==============================================
echo ""
echo "--- Phase 4: DI Registration ---"

if grep -q "IDomainEventDispatcher" "$PROJECT_ROOT/backend/VendorMdm.Api/Program.cs"; then
    check "IDomainEventDispatcher registered in DI" 0
else
    check "IDomainEventDispatcher registered in DI" 1
fi

if grep -q "MapHub<EventHub>" "$PROJECT_ROOT/backend/VendorMdm.Api/Program.cs"; then
    check "SignalR Hub mapped in Program.cs" 0
else
    check "SignalR Hub mapped in Program.cs" 1
fi

# ==============================================
# PHASE 5: Frontend Build
# ==============================================
echo ""
echo "--- Phase 5: Frontend Build ---"

cd "$PROJECT_ROOT/frontend"
npm run build --silent 2>&1
check "Frontend build succeeds" $?

# ==============================================
# PHASE 6: Frontend SignalR Files
# ==============================================
echo ""
echo "--- Phase 6: Frontend SignalR Files ---"

if [ -f "$PROJECT_ROOT/frontend/src/context/SignalRContext.tsx" ]; then
    check "SignalRContext.tsx exists" 0
else
    check "SignalRContext.tsx exists" 1
fi

if [ -f "$PROJECT_ROOT/frontend/src/hooks/useSignalR.ts" ]; then
    check "useSignalR.ts exists" 0
else
    check "useSignalR.ts exists" 1
fi

if grep -q "@microsoft/signalr" "$PROJECT_ROOT/frontend/package.json"; then
    check "@microsoft/signalr package installed" 0
else
    check "@microsoft/signalr package installed" 1
fi

# ==============================================
# PHASE 7: Brain Rules Update
# ==============================================
echo ""
echo "--- Phase 7: Brain Rules ---"

if grep -q "EDA.*Mandatory\|Event-Driven.*Mandatory\|MANDATORY.*EDA" "$PROJECT_ROOT/.agent/rules/moderngoldenrules.md"; then
    check "EDA mandatory evaluation rule added to brain" 0
else
    check "EDA mandatory evaluation rule added to brain" 1
fi

# ==============================================
# SUMMARY
# ==============================================
echo ""
echo "=============================================="
echo "  VERIFICATION SUMMARY"
echo "=============================================="
echo -e "  ${GREEN}PASSED${NC}: $PASS_COUNT"
echo -e "  ${RED}FAILED${NC}: $FAIL_COUNT"
echo "=============================================="

if [ $FAIL_COUNT -gt 0 ]; then
    echo -e "${RED}VERIFICATION FAILED${NC}: $FAIL_COUNT checks did not pass"
    exit 1
else
    echo -e "${GREEN}ALL CHECKS PASSED${NC}"
    exit 0
fi
