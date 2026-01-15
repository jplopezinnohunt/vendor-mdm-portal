#!/bin/bash

# Restore SMTP Configuration Helper Script
# Usage: ./restore_smtp.sh [environment]
# Example: ./restore_smtp.sh dev

ENV_NAME=${1:-dev}
PREFIX="vendor-mdm"
KV_NAME="kv-${PREFIX}-${ENV_NAME}"

echo "═══════════════════════════════════════════════════════════"
echo "🔐  SMTP Secret Restoration Helper"
echo "═══════════════════════════════════════════════════════════"
echo "Target Environment: $ENV_NAME"
echo "Target Key Vault:   $KV_NAME"
echo ""

# Check if logged in
if ! az account show > /dev/null 2>&1; then
    echo "⚠️  You are not logged in to Azure CLI."
    echo "   Please run 'az login' first."
    exit 1
fi

echo "Checking if Key Vault '$KV_NAME' exists..."
if ! az keyvault show --name "$KV_NAME" > /dev/null 2>&1; then
    echo "❌ Key Vault '$KV_NAME' not found!"
    echo "   Please verify the environment name and try again."
    echo "   Available Key Vaults:"
    az keyvault list --query "[].name" -o tsv
    exit 1
fi

echo "✅ Key Vault found."
echo ""
echo "Please enter the SMTP details to save securely:"

# Helper function to set secret
set_secret() {
    local secret_name=$1
    local prompt_text=$2
    local is_password=$3

    read -p "$prompt_text: " secret_value
    
    if [[ -z "$secret_value" ]]; then
        echo "   Skipping..."
        return
    fi
    
    echo "   Saving '$secret_name'..."
    az keyvault secret set --vault-name "$KV_NAME" --name "$secret_name" --value "$secret_value" > /dev/null
    echo "   ✅ Saved."
}

# 1. Enabled
echo "1. Enable SMTP?"
read -p "   Enable (true/false) [default: true]: " enabled_val
enabled_val=${enabled_val:-true}
az keyvault secret set --vault-name "$KV_NAME" --name "EmailService--Smtp--Enabled" --value "$enabled_val" > /dev/null
echo "   ✅ Saved."

# 2. Host
set_secret "EmailService--Smtp--Host" "2. SMTP Host (e.g., smtp.sendgrid.net)"

# 3. Port
echo "3. SMTP Port"
read -p "   Port [default: 587]: " port_val
port_val=${port_val:-587}
az keyvault secret set --vault-name "$KV_NAME" --name "EmailService--Smtp--Port" --value "$port_val" > /dev/null
echo "   ✅ Saved."

# 4. Username
set_secret "EmailService--Smtp--Username" "4. SMTP Username (apikey for SendGrid)"

# 5. Password
read -s -p "5. SMTP Password: " pass_val
echo ""
if [[ -n "$pass_val" ]]; then
    az keyvault secret set --vault-name "$KV_NAME" --name "EmailService--Smtp--Password" --value "$pass_val" > /dev/null
    echo "   ✅ Saved."
else
    echo "   Skipping..."
fi

# 6. From Email
set_secret "EmailService--Smtp--FromEmail" "6. From Email Address"

echo ""
echo "═══════════════════════════════════════════════════════════"
echo "✅ Configuration Complete!"
echo "   The application should pick up these changes automatically"
echo "   within a few minutes (Key Vault refresh)."
echo "   You may need to restart the App Service to force an update."
echo "═══════════════════════════════════════════════════════════"
