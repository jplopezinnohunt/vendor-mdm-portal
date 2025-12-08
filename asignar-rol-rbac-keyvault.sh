#!/bin/bash

# Script para asignar rol RBAC "Key Vault Secrets Officer" al usuario actual
# Uso: ./asignar-rol-rbac-keyvault.sh

echo "🔐 Asignando rol RBAC 'Key Vault Secrets Officer' en Key Vault..."
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

# Mostrar cuenta actual
CURRENT_ACCOUNT=$(az account show --query user.name -o tsv)
echo "✅ Sesión activa como: ${CURRENT_ACCOUNT}"
echo ""

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

# Verificar que el Key Vault existe
echo "Verificando que el Key Vault existe..."
az keyvault show --name "${KEY_VAULT_NAME}" --resource-group "${RESOURCE_GROUP}" &> /dev/null
if [ $? -ne 0 ]; then
    echo "❌ Key Vault '${KEY_VAULT_NAME}' no encontrado en el grupo de recursos '${RESOURCE_GROUP}'."
    exit 1
fi
echo "✅ Key Vault encontrado"
echo ""

# Verificar que el Key Vault usa RBAC
echo "Verificando que el Key Vault usa RBAC..."
RBAC_ENABLED=$(az keyvault show --name "${KEY_VAULT_NAME}" --resource-group "${RESOURCE_GROUP}" --query "properties.enableRbacAuthorization" -o tsv)
if [ "$RBAC_ENABLED" != "true" ]; then
    echo "⚠️  El Key Vault NO está configurado con RBAC."
    echo "   Configurando RBAC..."
    az keyvault update --name "${KEY_VAULT_NAME}" --resource-group "${RESOURCE_GROUP}" --enable-rbac-authorization true
    echo "✅ RBAC habilitado"
    echo ""
fi

# Verificar si el rol ya está asignado
echo "Verificando asignaciones de rol existentes..."
EXISTING_ASSIGNMENT=$(az role assignment list \
  --scope "${SCOPE}" \
  --assignee "${USER_EMAIL}" \
  --role "${ROLE_NAME}" \
  --query "[].id" -o tsv 2>/dev/null)

if [ ! -z "$EXISTING_ASSIGNMENT" ]; then
    echo "⚠️  El rol '${ROLE_NAME}' ya está asignado a '${USER_EMAIL}'"
    echo "   Assignment ID: ${EXISTING_ASSIGNMENT}"
    echo ""
    echo "✅ No se necesita acción adicional. Espera 5-10 minutos para que los permisos se propaguen."
    exit 0
fi

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
    echo "⏳ IMPORTANTE: Espera 5-10 minutos para que los permisos se propaguen."
    echo "   Luego intenta crear un secreto en el Key Vault."
    echo ""
    echo "📋 Próximos pasos:"
    echo "   1. Espera 5-10 minutos"
    echo "   2. Cierra y abre Azure Portal de nuevo"
    echo "   3. Intenta crear un secreto en Key Vault"
    echo "   4. Si aún da error, espera 5 minutos más"
else
    echo ""
    echo "❌ Error al asignar el rol."
    echo ""
    echo "💡 Posibles causas:"
    echo "   - No tienes permisos de 'User Access Administrator' o 'Owner'"
    echo "   - El usuario '${USER_EMAIL}' no existe en el directorio"
    echo "   - El Key Vault no está configurado con RBAC"
    echo ""
    echo "🔍 Verifica tus permisos:"
    echo "   az role assignment list --assignee '${USER_EMAIL}' --scope '/subscriptions/${SUBSCRIPTION_ID}' --output table"
    exit 1
fi


