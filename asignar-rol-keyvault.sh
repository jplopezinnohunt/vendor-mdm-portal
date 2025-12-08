#!/bin/bash

# Script para asignar el rol "Key Vault Secrets Officer" al usuario actual
# Uso: ./asignar-rol-keyvault.sh

echo "🔐 Asignando rol 'Key Vault Secrets Officer' en Key Vault..."
echo ""

# Verificar que Azure CLI está instalado
if ! command -v az &> /dev/null; then
    echo "❌ Azure CLI no está instalado"
    echo "Instala desde: https://aka.ms/InstallAzureCLIMacOS"
    exit 1
fi

# Verificar que estás logueado
echo "Verificando sesión de Azure..."
az account show &> /dev/null
if [ $? -ne 0 ]; then
    echo "Iniciando sesión en Azure..."
    az login --use-device-code
fi

# Configuración
KEY_VAULT_NAME="vendormdm-kv-dev"
RESOURCE_GROUP="rg-mdmportal-dev"
SUBSCRIPTION_ID="8c89e199-98bc-4cfd-9ad7-f8e97238f5c6"
USER_EMAIL="jplopez1172@hotmail.com"
ROLE_NAME="Key Vault Secrets Officer"

# Scope del Key Vault
SCOPE="/subscriptions/${SUBSCRIPTION_ID}/resourceGroups/${RESOURCE_GROUP}/providers/Microsoft.KeyVault/vaults/${KEY_VAULT_NAME}"

echo "📋 Configuración:"
echo "   Key Vault: ${KEY_VAULT_NAME}"
echo "   Resource Group: ${RESOURCE_GROUP}"
echo "   Usuario: ${USER_EMAIL}"
echo "   Rol: ${ROLE_NAME}"
echo ""

# Asignar rol
echo "Asignando rol..."
az role assignment create \
  --role "${ROLE_NAME}" \
  --assignee "${USER_EMAIL}" \
  --scope "${SCOPE}" \
  --output table

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ Rol asignado exitosamente!"
    echo ""
    echo "⏳ Espera 2-5 minutos para que los permisos se propaguen."
    echo "Luego intenta crear un secreto en el Key Vault."
else
    echo ""
    echo "❌ Error al asignar el rol."
    echo "Verifica que tienes permisos de administrador."
    exit 1
fi


