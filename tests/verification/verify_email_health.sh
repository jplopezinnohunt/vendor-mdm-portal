#!/bin/bash

# Configuration
API_URL="http://localhost:5001/api"
HEALTH_ENDPOINT="${API_URL}/health/email-service"
SYSTEM_ENDPOINT="${API_URL}/system/data-sources"

echo "--------------------------------------------------"
echo "📧 VERIFYING EMAIL HEALTH SYSTEM"
echo "--------------------------------------------------"

# 1. Check Health Controller
echo -n "Checking Health Controller... "
HEALTH_RESPONSE=$(curl -s $HEALTH_ENDPOINT)
if echo "$HEALTH_RESPONSE" | grep -q "\"connected\":"; then
    CONNECTED=$(echo "$HEALTH_RESPONSE" | grep -o '"connected":[a-z]*' | cut -d: -f2)
    ENV=$(echo "$HEALTH_RESPONSE" | grep -o '"environment":"[^"]*"' | cut -d: -f2 | tr -d '"')
    echo "✅ [Mode: $ENV, Connected: $CONNECTED]"
else
    echo "❌ Health Controller response invalid"
    echo "$HEALTH_RESPONSE"
    exit 1
fi

# 2. Check System Controller
echo -n "Checking System Controller... "
SYSTEM_RESPONSE=$(curl -s $SYSTEM_ENDPOINT)
if echo "$SYSTEM_RESPONSE" | grep -q "\"email\":"; then
    IS_CONFIGURED=$(echo "$SYSTEM_RESPONSE" | grep -A 10 '"email":' | grep -o '"isConfigured":[a-z]*' | cut -d: -f2)
    IS_CONNECTED=$(echo "$SYSTEM_RESPONSE" | grep -A 10 '"email":' | grep -o '"isConnected":[a-z]*' | cut -d: -f2)
    echo "✅ [Configured: $IS_CONFIGURED, Connected: $IS_CONNECTED]"
else
    echo "❌ System Controller response invalid"
    exit 1
fi

echo "--------------------------------------------------"
echo "✅ VERIFICATION COMPLETE"
echo "--------------------------------------------------"
