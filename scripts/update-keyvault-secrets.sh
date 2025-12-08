#!/bin/bash
# Script para actualizar secretos de email en Azure Key Vault
# Uso: ./update-keyvault-secrets.sh <environment> <keyvault-name>

set -e

ENVIRONMENT=${1:-dev}
KEY_VAULT_NAME=${2:-vendormdm-kv-${ENVIRONMENT}}

echo "🔐 Actualizando secretos en Azure Key Vault: ${KEY_VAULT_NAME}"
echo ""

# Verificar que Azure CLI está instalado
if ! command -v az &> /dev/null; then
    echo "❌ Azure CLI no está instalado"
    echo "Instala desde: https://aka.ms/InstallAzureCLI"
    exit 1
fi

# Verificar que está logueado
if ! az account show &> /dev/null; then
    echo "⚠️ No estás logueado en Azure CLI"
    echo "Ejecuta: az login"
    exit 1
fi

echo "✅ Conectado a Azure"
echo ""

# Solicitar credenciales
read -p "📧 SMTP Host (ej: smtp.gmail.com): " SMTP_HOST
read -p "👤 SMTP Username (ej: user@example.com): " SMTP_USERNAME
read -sp "🔑 SMTP Password (app password): " SMTP_PASSWORD
echo ""
read -p "📨 From Email (ej: noreply@example.com): " FROM_EMAIL
read -p "📝 From Name (ej: Vendor Management): " FROM_NAME
FROM_NAME=${FROM_NAME:-"Vendor Management"}

echo ""
echo "🔄 Actualizando secretos en Key Vault..."

# Actualizar secretos
az keyvault secret set \
  --vault-name "${KEY_VAULT_NAME}" \
  --name "EmailService--Smtp--Host" \
  --value "${SMTP_HOST}" \
  --output none

az keyvault secret set \
  --vault-name "${KEY_VAULT_NAME}" \
  --name "EmailService--Smtp--Username" \
  --value "${SMTP_USERNAME}" \
  --output none

az keyvault secret set \
  --vault-name "${KEY_VAULT_NAME}" \
  --name "EmailService--Smtp--Password" \
  --value "${SMTP_PASSWORD}" \
  --output none

az keyvault secret set \
  --vault-name "${KEY_VAULT_NAME}" \
  --name "EmailService--Smtp--FromEmail" \
  --value "${FROM_EMAIL}" \
  --output none

az keyvault secret set \
  --vault-name "${KEY_VAULT_NAME}" \
  --name "EmailService--Smtp--FromName" \
  --value "${FROM_NAME}" \
  --output none

echo ""
echo "✅ Secretos actualizados exitosamente en ${KEY_VAULT_NAME}"
echo ""
echo "📋 Secretos configurados:"
echo "   - EmailService--Smtp--Host"
echo "   - EmailService--Smtp--Username"
echo "   - EmailService--Smtp--Password"
echo "   - EmailService--Smtp--FromEmail"
echo "   - EmailService--Smtp--FromName"
echo ""
echo "💡 Reinicia tu App Service para que los cambios surtan efecto"

