# 🔐 Crear y Gestionar Secretos en Key Vault

Guía completa para crear, actualizar y gestionar secretos en Azure Key Vault.

---

## 📋 Secretos Requeridos

El proyecto requiere estos 5 secretos en Key Vault:

| Nombre del Secreto | Valor de Ejemplo | Descripción |
|-------------------|------------------|-------------|
| `EmailService--Smtp--Host` | `smtp.gmail.com` | Servidor SMTP |
| `EmailService--Smtp--Username` | `JPLOPEZ@INNOHUNT.IO` | Usuario SMTP |
| `EmailService--Smtp--Password` | `mxobzmcgiggvrwqb` | Contraseña SMTP (App Password) |
| `EmailService--Smtp--FromEmail` | `jplopez+MDMPORTAL@innohunt.io` | Email remitente |
| `EmailService--Smtp--FromName` | `Vendor Management` | Nombre remitente |

**⚠️ Importante**: Key Vault usa `--` (doble guión) para separar niveles, no `:` ni `-` simple.

---

## 📋 Crear Secretos

### Desde Azure Portal

1. **Abrir Key Vault**:
   - Azure Portal → Busca "Key Vaults"
   - Selecciona tu Key Vault (ej: `vendormdm-kv-dev`)

2. **Ir a Secrets**:
   - Menú izquierdo → **"Secrets"**

3. **Crear Secreto**:
   - Click en **"+ Generate/Import"**
   - **Upload options**: "Manual"
   - **Name**: Ingresa el nombre exacto (ej: `EmailService--Smtp--Host`)
   - **Value**: Ingresa el valor
   - **Content type**: Dejar vacío (opcional)
   - **Enabled**: ✅ Sí
   - Click **"Create"**

4. **Repetir para cada secreto** (5 veces en total)

### Formato Correcto de Nombres

✅ **Correcto**:
- `EmailService--Smtp--Host`
- `EmailService--Smtp--Username`
- `EmailService--Smtp--Password`

❌ **Incorrecto**:
- `EmailService:Smtp:Host` (usa `:`)
- `EmailService-Smtp-Host` (guión simple)
- `EmailService_Smtp_Host` (guión bajo)

---

## ✅ Verificar Secretos Creados

Después de crear los secretos, verifica en la lista:

1. Ve a **"Secrets"** en tu Key Vault
2. Deberías ver los 5 secretos:
   - ✅ `EmailService--Smtp--Host`
   - ✅ `EmailService--Smtp--Username`
   - ✅ `EmailService--Smtp--Password`
   - ✅ `EmailService--Smtp--FromEmail`
   - ✅ `EmailService--Smtp--FromName`

---

## 🔄 Actualizar Secretos

Para actualizar un secreto existente:

1. Ve a **"Secrets"** en tu Key Vault
2. Click en el secreto que quieres actualizar
3. Click en **"New version"** (o **"Create new version"**)
4. Ingresa el nuevo valor
5. Click en **"Create"**

**Nota**: El App Service cargará automáticamente la nueva versión después de unos minutos. No necesitas reiniciar el servicio.

---

## 🔍 Ver Valor de un Secreto

1. Ve a **"Secrets"** en tu Key Vault
2. Click en el secreto que quieres ver
3. Click en la versión más reciente
4. Click en **"Show Secret Value"** para ver el valor

**Nota**: Solo usuarios con permisos de lectura pueden ver los valores.

---

## 🗑️ Eliminar Secretos

⚠️ **Advertencia**: Eliminar secretos puede causar que la aplicación falle. Solo elimina si estás seguro.

1. Ve a **"Secrets"** en tu Key Vault
2. Click en el secreto que quieres eliminar
3. Click en **"Delete"**
4. Confirma la eliminación

**Nota**: Con soft delete habilitado, el secreto se mantiene por un período antes de ser eliminado permanentemente.

---

## 📝 Mapeo de Configuración

Key Vault convierte automáticamente los nombres:

| Key Vault Secret | Configuración en Código |
|-----------------|------------------------|
| `EmailService--Smtp--Host` | `EmailService:Smtp:Host` |
| `EmailService--Smtp--Username` | `EmailService:Smtp:Username` |
| `EmailService--Smtp--Password` | `EmailService:Smtp:Password` |
| `EmailService--Smtp--FromEmail` | `EmailService:Smtp:FromEmail` |
| `EmailService--Smtp--FromName` | `EmailService:Smtp:FromName` |

El código puede acceder a estos valores como configuración normal:

```csharp
var host = configuration["EmailService:Smtp:Host"];
var username = configuration["EmailService:Smtp:Username"];
```

---

## 🔐 Seguridad de Secretos

### ✅ Mejores Prácticas

1. **Usa App Passwords** para servicios como Gmail (no contraseñas regulares)
2. **Rota contraseñas** periódicamente
3. **No compartas** valores de secretos por email o chat
4. **Usa diferentes secretos** para dev/staging/prod
5. **Habilita soft delete** para recuperación

### ⚠️ Importante

- **Nunca** commitees valores de secretos a Git
- **Nunca** compartas secretos en logs o mensajes de error
- **Rota** contraseñas si sospechas que fueron comprometidas

---

## 🎯 Ejemplo Visual

```
Key Vault: vendormdm-kv-dev
├── Secret: EmailService--Smtp--Host = smtp.gmail.com
├── Secret: EmailService--Smtp--Username = JPLOPEZ@INNOHUNT.IO
├── Secret: EmailService--Smtp--Password = mxobzmcgiggvrwqb
├── Secret: EmailService--Smtp--FromEmail = jplopez+MDMPORTAL@innohunt.io
└── Secret: EmailService--Smtp--FromName = Vendor Management
```

---

## 🔗 Próximos Pasos

Después de crear los secretos:

1. **Configurar App Service**: Consulta [Configuración RBAC → App Service](./rbac-configuration.md#configurar-app-service-con-rbac)
2. **Verificar Configuración**: Revisa los logs del App Service
3. **Probar Email**: Crea una invitación y verifica que el email se envía

---

## 📚 Referencias

- [Azure Key Vault Secrets](https://learn.microsoft.com/azure/key-vault/secrets/)
- [Key Vault Naming Conventions](https://learn.microsoft.com/azure/key-vault/general/about-keys-secrets-certificates)

---

*¿Problemas creando secretos? Consulta [Configuración RBAC](./rbac-configuration.md) para verificar permisos.*

