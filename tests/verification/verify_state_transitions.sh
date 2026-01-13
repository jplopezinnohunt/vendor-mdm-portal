#!/bin/bash

# Configuration
API_URL="http://localhost:5001/api"
SEED_ENDPOINT="$API_URL/test/seed-application"
APPROVE_ENDPOINT="$API_URL/review"

echo "============================================"
echo "NON-REGRESSION TEST: State Transitions"
echo "============================================"

# 1. Seed Application (PendingReview)
echo "1. Seeding Application..."
SEED_RESPONSE=$(curl -s -X POST "$SEED_ENDPOINT" \
  -H "Content-Type: application/json" \
  -d '{"CompanyName": "Golden State Vendor", "TaxId": "GOLD-999", "ContactEmail": "gold@state.com"}')

APP_ID=$(echo "$SEED_RESPONSE" | grep -o '"applicationId":"[^"]*"' | cut -d'"' -f4)

if [ -z "$APP_ID" ]; then
    echo "❌ FAILED: Could not seed application. Response:"
    echo "$SEED_RESPONSE"
    exit 1
fi

echo "✅ Application Seeded with ID: $APP_ID"

# 2. Approve Application
echo "2. Approving Application..."
APPROVE_RESPONSE=$(curl -s -X POST "$APPROVE_ENDPOINT/$APP_ID/approve" \
  -H "Content-Type: application/json" \
  -H "X-Mock-User: Approver" \
  -d '{"Comments": "Approving via Verify Script", "EnrichedAttributes": {"script_verified": true}, "ForceSanctionsOverride": true}')

# Check for success status in response (simple grep check)
if echo "$APPROVE_RESPONSE" | grep -q "Approved"; then
    echo "✅ Approval Successful. Response:"
    echo "$APPROVE_RESPONSE"
else
    echo "❌ FAILED: Approval failed. Response:"
    echo "$APPROVE_RESPONSE"
    exit 1
fi

echo "============================================"
echo "✅ TRANSITION TEST PASSED"
echo "============================================"
