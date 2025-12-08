# 🔧 Troubleshooting

Solución de problemas comunes y guías de debugging.

## 📋 Índice

1. [Problemas Comunes](./common-issues.md) - Errores frecuentes y soluciones
2. [Debug Azure Services](./debug-azure-services.md) - Troubleshooting de servicios Azure

---

## 🚨 Problemas Frecuentes

### Backend no inicia

**Síntoma**: Error al ejecutar `dotnet run`

**Soluciones comunes**:
- Verificar que .NET 8 SDK está instalado
- Revisar connection strings en `appsettings.Development.json`
- Verificar que los puertos no están en uso

👉 Consulta: [Problemas Comunes → Backend](./common-issues.md#backend-no-inicia)

### Frontend no conecta al backend

**Síntoma**: Errores CORS o "Cannot connect to API"

**Soluciones comunes**:
- Verificar que el backend está corriendo
- Revisar configuración de puertos
- Verificar CORS en `Program.cs`

👉 Consulta: [Problemas Comunes → Frontend](./common-issues.md#frontend-no-conecta)

### Problemas con Key Vault

**Síntoma**: "No tienes permisos" o "Secret not found"

**Soluciones comunes**:
- Verificar configuración RBAC
- Revisar que los nombres de secretos usan `--`
- Verificar Managed Identity

👉 Consulta: [Azure Key Vault](../azure/key-vault/README.md#problemas-comunes)

---

## 🔍 Buscar por Error

| Error | Solución |
|-------|----------|
| "dotnet: command not found" | [Instalación](../getting-started/installation.md#net-8-sdk) |
| "Port already in use" | [Problemas Comunes](./common-issues.md#puertos) |
| "Cannot connect to Azure" | [Debug Azure](./debug-azure-services.md) |
| "Key Vault permission denied" | [Key Vault RBAC](../azure/key-vault/rbac-configuration.md) |
| "Email not sending" | [Email Configuration](../features/email-configuration.md) |

---

## 📚 Documentos de Troubleshooting

### [Problemas Comunes](./common-issues.md)

Soluciones para:
- Problemas de instalación
- Errores de conexión
- Problemas de puertos
- Errores de base de datos

### [Debug Azure Services](./debug-azure-services.md)

Guías para:
- Debugging de conexiones Azure
- Verificación de recursos
- Problemas de firewall
- Logs y diagnóstico

---

## 🆘 ¿No Encuentras la Solución?

1. **Revisa los logs**: Backend console, browser console, Azure logs
2. **Consulta documentación específica**: Cada feature tiene su sección de troubleshooting
3. **Verifica configuración**: Connection strings, variables de entorno, permisos

---

## 🔗 Enlaces Rápidos

- [Getting Started](../getting-started/README.md) - Setup inicial
- [Azure Documentation](../azure/README.md) - Problemas de Azure
- [Key Vault](../azure/key-vault/README.md) - Problemas de Key Vault

---

*¿Problemas persistentes? Revisa los documentos específicos o los logs detallados.*

