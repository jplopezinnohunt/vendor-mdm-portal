# 🔐 Configurar Key Vault con RBAC

Guía completa para configurar Azure Key Vault con Role-Based Access Control (RBAC).

## 🎯 Objetivo

Configurar tu Key Vault para usar **RBAC (Role-Based Access Control)** y asignar los permisos necesarios para crear secretos y dar acceso a servicios.

---

## 📋 Paso 1: Verificar que el Key Vault Usa RBAC

1. Ve a tu Key Vault: `vendormdm-kv-dev`
2. En el **menú izquierdo**, busca: **"Settings"**
3. Click en **"Settings"**
4. Busca **"Access configuration"** o **"Permission model"**
5. Debe estar seleccionado: **"Azure role-based access control (RBAC)"**
   - Si no, cámbialo a RBAC y guarda

---

## 📋 Paso 2: Asignar Rol a Tu Usuario

### Opción A: Desde Azure Portal (Recomendado)

1. En tu Key Vault, ve a **"Access control (IAM)"** (menú izquierdo)
2. Click en **"+ Add"** → **"Add role assignment"**

3. En el panel que se abre:
   - **Role**: Busca y selecciona **"Key Vault Secrets Officer"**
   - Click en **"Next"**

4. En **"Assign access to"**:
   - Selecciona: **"User, group, or service principal"**
   - Click en **"Select members"**
   - Busca tu email de usuario (ej: `jplopez1172@hotmail.com`)
   - Selecciona tu usuario
   - Click en **"Select"**
   - Click en **"Next"**

5. En **"Review + assign"**:
   - Revisa la información
   - Click en **"Review + assign"**
   - Click en **"Review + assign"** de nuevo (confirmación)

6. **Espera 5-10 minutos** para que los permisos se propaguen

### Opción B: Usar Azure CLI

Si tienes Azure CLI instalado:

```bash
# Login
az login

# Asignar rol
az role assignment create \
  --role "Key Vault Secrets Officer" \
  --assignee "tu-email@dominio.com" \
  --scope "/subscriptions/SUBSCRIPTION-ID/resourceGroups/RESOURCE-GROUP/providers/Microsoft.KeyVault/vaults/KEYVAULT-NAME"
```

### Opción C: Usar Script

Si existe el script `asignar-rol-rbac-keyvault.sh`:

```bash
./asignar-rol-rbac-keyvault.sh
```

---

## 📋 Paso 3: Verificar que el Rol Está Asignado

1. Ve a **"Access control (IAM)"** en tu Key Vault
2. Click en la pestaña **"Role assignments"**
3. Busca tu usuario
4. Deberías ver: **"Key Vault Secrets Officer"** en la columna "Role"

Si no aparece:
- Espera 5-10 minutos más
- Refresca la página
- Verifica que asignaste el rol correctamente

---

## 📋 Paso 4: Verificar Permisos de Suscripción

Para asignar roles RBAC, necesitas uno de estos roles a nivel de **Suscripción**:

- **Owner**
- **User Access Administrator**
- **Key Vault Administrator**

Para verificar:

1. Ve a tu **Suscripción** en Azure Portal
2. Click en **"Access control (IAM)"**
3. Click en **"Role assignments"**
4. Busca tu usuario
5. Verifica que tienes uno de los roles mencionados

Si no tienes ninguno:
- Necesitas que alguien con permisos de Owner te asigne el rol
- O pide que te asignen **"User Access Administrator"** temporalmente

---

## 📋 Paso 5: Probar Crear un Secreto

Después de esperar 5-10 minutos:

1. Ve a **"Secrets"** en tu Key Vault
2. Click en **"+ Generate/Import"**
3. Intenta crear un secreto de prueba:
   - **Name**: `test-secret`
   - **Value**: `test-value`
   - Click en **"Create"**

Si funciona, ¡perfecto! Si aún da error:
- Espera 5 minutos más
- Cierra y abre el navegador
- Intenta de nuevo

---

## 📋 Paso 6: Configurar App Service con RBAC

### 6.1: Habilitar Managed Identity

1. Ve a tu **App Service** (`vendormdm-api-dev`)
2. **"Identity"** → **"System assigned"** → **"On"** → **"Save"**
3. Copia el **Object (principal) ID** que aparece

### 6.2: Asignar Rol a Managed Identity

1. Ve a tu **Key Vault**
2. **"Access control (IAM)"** → **"+ Add"** → **"Add role assignment"**
3. **Role**: `Key Vault Secrets User`
4. **Assign access to**: `Managed identity`
5. **Select members**: 
   - Click en **"Select members"**
   - Busca tu App Service por nombre
   - O pega el **Object ID** que copiaste
   - Selecciona
6. Click **"Review + assign"** dos veces

### 6.3: Configurar App Service

1. **App Service** → **"Configuration"** → **"Application settings"**
2. Agrega:
   - **Name**: `KeyVault:VaultUrl`
   - **Value**: `https://vendormdm-kv-dev.vault.azure.net/`
3. **"Save"**

### 6.4: Reiniciar App Service

1. **App Service** → **"Overview"** → **"Restart"**

---

## ⚠️ Troubleshooting

### Error: "You do not have permission to assign roles"

**Solución**: Necesitas el rol **"User Access Administrator"** o **"Owner"** a nivel de suscripción.

1. Ve a tu **Suscripción**
2. **"Access control (IAM)"** → Verifica tus roles
3. Si no tienes permisos, pide a un administrador que te asigne el rol

### Error: "Role assignment already exists"

**Solución**: El rol ya está asignado. Espera 5-10 minutos y prueba de nuevo.

### Error: "The operation is not allowed by RBAC" (después de asignar)

**Solución**: 
- Espera 10-15 minutos (puede tardar)
- Cierra completamente el navegador
- Abre Azure Portal de nuevo
- Intenta crear el secreto otra vez

### Los permisos no se propagan después de 15 minutos

**Solución**:
- Verifica que el rol está asignado correctamente en IAM
- Verifica que tienes permisos de suscripción
- Considera cambiar temporalmente a Access Policies (más rápido pero menos seguro)

---

## ✅ Checklist Final

- [ ] Key Vault configurado con RBAC
- [ ] Rol "Key Vault Secrets Officer" asignado a tu usuario
- [ ] Esperaste 5-10 minutos después de asignar
- [ ] Puedes crear secretos en Key Vault
- [ ] Managed Identity habilitada en App Service
- [ ] Rol "Key Vault Secrets User" asignado a Managed Identity
- [ ] `KeyVault:VaultUrl` configurado en App Service
- [ ] App Service reiniciado
- [ ] Logs muestran "Azure Key Vault configured"

---

## 📝 Nota sobre Propagación de Permisos

Los permisos RBAC pueden tardar **5-15 minutos** en propagarse. Si después de 15 minutos aún no funciona:
- Verifica que el rol está asignado correctamente
- Verifica que tienes permisos de suscripción
- Considera usar Access Policies como alternativa más rápida

---

## 🔗 Próximos Pasos

Una vez configurado RBAC:

1. **Crear Secretos**: Consulta [Crear y Gestionar Secretos](./secrets-management.md)
2. **Verificar Configuración**: Revisa los logs del App Service para confirmar que Key Vault está configurado

---

## 📚 Roles Comunes

| Rol | Descripción | Cuándo Usar |
|-----|-------------|-------------|
| **Key Vault Secrets Officer** | Puede crear, leer, actualizar y eliminar secretos | Desarrolladores que necesitan gestionar secretos |
| **Key Vault Secrets User** | Puede leer secretos | Servicios (App Service, Functions) que solo necesitan leer |
| **Key Vault Administrator** | Administración completa del Key Vault | Administradores de infraestructura |

---

*¿Problemas? Consulta la sección de Troubleshooting o revisa [Setup y Configuración](./setup.md) para más detalles.*

