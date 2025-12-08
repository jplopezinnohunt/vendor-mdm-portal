# 🔐 Azure Key Vault

Documentación completa sobre configuración y uso de Azure Key Vault en el proyecto.

## 📋 Índice

1. [Setup y Configuración](./setup.md) - Guía completa de configuración inicial
2. [Configuración RBAC](./rbac-configuration.md) - Configurar Key Vault con RBAC
3. [Crear y Gestionar Secretos](./secrets-management.md) - Guía para crear y actualizar secretos

---

## 🎯 Inicio Rápido

### Para Nuevos Usuarios

1. Lee [Setup y Configuración](./setup.md) para entender qué es Key Vault y por qué lo usamos
2. Configura RBAC siguiendo [Configuración RBAC](./rbac-configuration.md)
3. Crea los secretos necesarios usando [Crear y Gestionar Secretos](./secrets-management.md)

---

## 🔑 Secretos Requeridos

El proyecto requiere estos secretos en Key Vault:

| Nombre del Secreto | Descripción | Ejemplo |
|-------------------|-------------|---------|
| `EmailService--Smtp--Host` | Servidor SMTP | `smtp.gmail.com` |
| `EmailService--Smtp--Username` | Usuario SMTP | `your-email@domain.com` |
| `EmailService--Smtp--Password` | Contraseña SMTP | `your-app-password` |
| `EmailService--Smtp--FromEmail` | Email remitente | `noreply@yourcompany.com` |
| `EmailService--Smtp--FromName` | Nombre remitente | `Vendor Management` |

**⚠️ Importante**: Key Vault usa `--` (doble guión) para separar niveles, no `:`.

---

## 📚 Documentos Principales

### [Setup y Configuración](./setup.md)
- ¿Por qué usar Key Vault?
- Crear Key Vault
- Arquitectura (desarrollo vs producción)
- Integración en el código

### [Configuración RBAC](./rbac-configuration.md)
- Configurar RBAC en Key Vault
- Asignar roles a usuarios
- Asignar roles a Managed Identity
- Troubleshooting de permisos

### [Crear y Gestionar Secretos](./secrets-management.md)
- Crear secretos desde Azure Portal
- Actualizar secretos existentes
- Formato correcto de nombres
- Verificar secretos creados

---

## 🆘 Problemas Comunes

### "No tienes permisos para crear secretos"
→ Consulta [Configuración RBAC → Asignar Rol a Usuario](./rbac-configuration.md#asignar-rol-a-tu-usuario)

### "El secreto no se encuentra"
→ Verifica que el nombre use `--` (doble guión), no `:` o `-`

### "Managed Identity no puede leer secretos"
→ Consulta [Configuración RBAC → Configurar App Service](./rbac-configuration.md#configurar-app-service-con-rbac)

---

## 🔗 Enlaces Relacionados

- [Configuración de Email](../../features/email-configuration.md) - Usa los secretos de Key Vault
- [Azure Deployment](../deployment/README.md) - Incluye configuración de Key Vault
- [Troubleshooting](../../troubleshooting/README.md) - Problemas comunes

---

## 📝 Notas Importantes

- **Nunca** commitees contraseñas o secretos a Git
- **Siempre** usa Key Vault en producción
- Los permisos RBAC pueden tardar **5-15 minutos** en propagarse
- Key Vault usa formato `--` para separar niveles de configuración

---

*¿Necesitas ayuda? Consulta los documentos individuales o la sección de [Troubleshooting](../../troubleshooting/README.md).*

