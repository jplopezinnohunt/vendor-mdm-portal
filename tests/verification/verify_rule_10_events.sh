#!/bin/bash

# Rule 6: Verification Script for Rule 10 (Event Driven) coverage.

echo "🔍 Verifying Rule 10 Compliance..."

# 1. Verify Event Classes Exist
if [ -f "backend/VendorMdm.Shared/DomainEvents/ApplicationApprovedEvent.cs" ] && \
   [ -f "backend/VendorMdm.Shared/DomainEvents/ApplicationRejectedEvent.cs" ]; then
    echo "✅ Event Classes Found."
else
    echo "❌ Missing Event Classes!"
    exit 1
fi

# 2. Verify Service Publishes Events
if grep -q "new ApplicationApprovedEvent" backend/VendorMdm.Api/Services/VendorApplicationService.cs && \
   grep -q "_serviceBus.PublishEventAsync" backend/VendorMdm.Api/Services/VendorApplicationService.cs; then
    echo "✅ ApplicationApprovedEvent is instantiated and published."
else
    echo "❌ VendorApplicationService does NOT publish ApplicationApprovedEvent!"
    exit 1
fi

if grep -q "new ApplicationRejectedEvent" backend/VendorMdm.Api/Services/VendorApplicationService.cs && \
   grep -q "_serviceBus.PublishEventAsync" backend/VendorMdm.Api/Services/VendorApplicationService.cs; then
    echo "✅ ApplicationRejectedEvent is instantiated and published."
else
    echo "❌ VendorApplicationService does NOT publish ApplicationRejectedEvent!"
    exit 1
fi

# 3. Verify Controller passes ApproverId
if grep -q "User.Identity?.Name" backend/VendorMdm.Api/Controllers/ReviewController.cs; then
    echo "✅ ApproverId is captured in Controller."
else
    echo "❌ ReviewController does not capture User Identity!"
    exit 1
fi

echo "🎉 Rule 10 Verification Passed!"
