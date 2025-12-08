# 🔐 Azure Key Vault - Setup y Configuración

## ¿Por qué Azure Key Vault?

En una arquitectura segura, **nunca** debemos guardar contraseñas o credenciales en archivos de configuración que se suban a Git. Azure Key Vault es el servicio de Azure diseñado para almacenar secretos de forma segura.

---

## ✅ Lo que se Implementó

1. **Integración con Azure Key Vault** en `Program.cs`
2. **Módulo Bicep** para crear Key Vault (`infrastructure/modules/keyvault.bicep`)
3. **EmailService actualizado** para leer desde Key Vault con fallback a configuración local
4. **Soporte para desarrollo local** (User Secrets o appsettings.Development.json)

---

## 🏗️ Arquitectura

### Desarrollo Local
```
appsettings.Development.json → EmailService
User Secrets → EmailService
```

### Producción/Staging
```
Azure Key Vault → Program.cs → EmailService
```

---

## 📋 Crear Key Vault

### Opción A: Usando Bicep (Recomendado)

```bash
cd infrastructure
az deployment group create \
  --resource-group vendormdm-rg-dev \
  --template-file main.bicep \
  --parameters environmentName=dev
```

### Opción B: Azure Portal

1. Azure Portal → **Create a resource** → **Key Vault**
2. **Name**: `vendormdm-kv-dev` (o `vendormdm-kv-prod`)
3. **Region**: Misma región que tus otros recursos
4. **Pricing tier**: Standard
5. **Enable soft delete**: ✅ Sí
6. **Enable purge protection**: ✅ Sí (para producción)
7. **Access configuration**: Selecciona el modelo de permisos (RBAC recomendado)
8. Click **Create**

---

## 🔧 Configuración en el Código

El código ya está configurado para usar Key Vault automáticamente en producción. Revisa `backend/VendorMdm.Api/Program.cs`:

```csharp
// Add Azure Key Vault for secrets in production
if (!builder.Environment.IsDevelopment())
{
    var keyVaultUrl = builder.Configuration["KeyVault:VaultUrl"];
    if (!string.IsNullOrEmpty(keyVaultUrl))
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUrl),
            new DefaultAzureCredential());
        Console.WriteLine($"🔑 Azure Key Vault configured: {keyVaultUrl}");
    }
}
```

Esto significa:
- **Development (local)**: Usa `appsettings.Development.json` o User Secrets
- **Production (Azure)**: Usa Key Vault automáticamente si `KeyVault:VaultUrl` está configurado

---

## 🔄 Cómo Funciona

### En Desarrollo Local:
1. Lee de `appsettings.Development.json` o User Secrets
2. Si no encuentra, usa valores por defecto
3. **No** intenta conectar a Key Vault

### En Producción/Staging:
1. Lee `KeyVault:VaultUrl` de configuración
2. Se conecta a Key Vault usando **Managed Identity**
3. Carga todos los secretos automáticamente
4. Los secretos están disponibles como configuración normal

---

## 📝 Nombres de Secretos en Key Vault

Key Vault usa `--` (doble guión) para separar niveles:

| Configuración Local | Key Vault Secret Name |
|---------------------|----------------------|
| `EmailService:Smtp:Host` | `EmailService--Smtp--Host` |
| `EmailService:Smtp:Username` | `EmailService--Smtp--Username` |
| `EmailService:Smtp:Password` | `EmailService--Smtp--Password` |
| `EmailService:Smtp:FromEmail` | `EmailService--Smtp--FromEmail` |

---

## 🔒 Seguridad

### ✅ Buenas Prácticas Implementadas:

1. **Key Vault con soft delete** - Los secretos no se eliminan inmediatamente
2. **Purge protection** - Previene eliminación permanente
3. **Managed Identity** - No se usan claves de acceso
4. **Acceso basado en roles** - Solo servicios autorizados pueden leer
5. **Separación por ambiente** - Key Vault diferente para dev/staging/prod

### ⚠️ Importante:

- **Nunca** subas contraseñas a Git
- **Siempre** usa Key Vault en producción
- **Rota** las contraseñas periódicamente
- **Audita** el acceso a Key Vault

---

## ✅ Verificación

### En Desarrollo:
```bash
# Verificar que User Secrets funciona
dotnet user-secrets list
```

### En Producción:
1. Ve a **App Service** → **Log stream**
2. Deberías ver: `✅ Azure Key Vault configured: https://...`
3. Si hay error: `⚠️ Failed to connect to Key Vault`

---

## 🔗 Próximos Pasos

1. **Configurar RBAC**: Consulta [Configuración RBAC](./rbac-configuration.md)
2. **Crear Secretos**: Consulta [Crear y Gestionar Secretos](./secrets-management.md)
3. **Configurar App Service**: Consulta [Configuración RBAC → App Service](./rbac-configuration.md#configurar-app-service-con-rbac)

---

## 📚 Referencias

- [Azure Key Vault Documentation](https://learn.microsoft.com/azure/key-vault/)
- [Managed Identity](https://learn.microsoft.com/azure/active-directory/managed-identities-azure-resources/)
- [Key Vault Secret Manager](https://learn.microsoft.com/aspnet/core/security/key-vault-configuration)

---

## 🎯 Resumen

✅ **Key Vault creado** con Bicep  
✅ **Integración en Program.cs** - Carga secretos automáticamente  
✅ **EmailService actualizado** - Lee desde Key Vault o configuración local  
✅ **Desarrollo local** - Usa User Secrets o appsettings.Development.json  
✅ **Producción** - Usa Key Vault con Managed Identity  

**Las credenciales ahora están seguras en Azure Key Vault! 🔐**

