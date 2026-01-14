#!/bin/bash
# verify_cicd_alignment.sh - Proves the pipeline matches project rules

echo "Checking for PR Validation Gate..."
if [ -f .github/workflows/verify-pr.yml ]; then
  echo "✅ verify-pr.yml exists."
else
  echo "❌ verify-pr.yml missing."
  exit 1
fi

echo "Checking Branch Triggers in Backend Deploy..."
if grep -q "develop" .github/workflows/deploy-backend-api.yml; then
  echo "✅ Backend deploy supports develop branch."
else
  echo "❌ Backend deploy missing develop branch trigger."
  exit 1
fi

echo "Checking Branch Triggers in SWA Deploy..."
if grep -q "develop" .github/workflows/azure-static-web-apps.yml; then
  echo "✅ SWA deploy supports develop branch."
else
  echo "❌ SWA deploy missing develop branch trigger."
  exit 1
fi

echo "Checking for multi-environment logic in Backend Deploy..."
if grep -q "app-name: \${{ github.ref == 'refs/heads/main'" .github/workflows/deploy-backend-api.yml; then
  echo "✅ Backend deploy has environment logic."
else
  echo "❌ Backend deploy lacks dynamic environment logic."
  exit 1
fi

echo "CI/CD Governance Verification Passed!"
exit 0
