#!/bin/bash
# Verification script for Infrastructure Health reporting (Rule 14)

API_URL=${1:-"http://localhost:5001/api"}

echo "🔍 Verifying Infrastructure Health reporting at $API_URL/system/data-sources..."

RESPONSE=$(curl -s "$API_URL/system/data-sources")

if [ $? -ne 0 ]; then
  echo "❌ Error: Could not reach API at $API_URL"
  exit 1
fi

# Check for all required service sections
SERVICES=("sap" "fileStorage" "sanctions" "serviceBus" "email")
ALL_PASSED=true

for service in "${SERVICES[@]}"; do
  # Check if service exists in response and has isConnected property
  IS_PRESENT=$(echo "$RESPONSE" | grep -o "$service")
  if [ -z "$IS_PRESENT" ]; then
    echo "❌ Error: Service '$service' missing from health response"
    ALL_PASSED=false
    continue
  fi

  IS_CONNECTED=$(echo "$RESPONSE" | grep -o "\"$service\":{[^}]*\"isConnected\":[^,}]*" | grep -o "true\|false")
  MODE=$(echo "$RESPONSE" | grep -o "\"$service\":{[^}]*\"mode\":\"[^\"]*\"" | cut -d'"' -f6)

  if [ -n "$IS_CONNECTED" ]; then
    echo "✅ Service '$service' found (Mode: $MODE, Connected: $IS_CONNECTED)"
  else
    echo "❌ Error: Service '$service' found but 'isConnected' status missing"
    ALL_PASSED=false
  fi
done

if [ "$ALL_PASSED" = true ]; then
  echo "✨ Infrastructure health verification PASSED"
  exit 0
else
  echo "⚠️ Infrastructure health verification FAILED"
  exit 1
fi
