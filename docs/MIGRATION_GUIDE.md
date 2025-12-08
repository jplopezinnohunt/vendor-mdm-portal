# 📚 Guía de Migración - Reorganización de Documentación

Este documento explica la reorganización de la documentación y dónde encontrar la información que antes estaba en múltiples archivos.

---

## 🎯 Objetivo de la Reorganización

La documentación tenía **67 archivos .md** en la raíz del proyecto, muchos con contenido duplicado o relacionado. La nueva estructura:

- ✅ Organiza la documentación por temas en carpetas
- ✅ Consolida archivos duplicados
- ✅ Mantiene toda la información importante
- ✅ Facilita la navegación

---

## 📁 Nueva Estructura

```
docs/
├── README.md                          # Índice principal
├── getting-started/                   # Guías de inicio
│   ├── README.md
│   ├── installation.md                # Consolida: INSTALL_DOTNET, INSTALL_DOTNET_QUICK, etc.
│   └── local-development.md           # Consolida: QUICK_START, SETUP_GUIDE, RUNNING_LOCALLY, etc.
├── azure/                             # Todo sobre Azure
│   ├── README.md
│   ├── infrastructure.md              # Consolida: AZURE_INFRASTRUCTURE, AZURE_COMPONENTS_SUMMARY
│   ├── deployment.md                  # Consolida: AZURE_DEPLOYMENT_GUIDE, DEPLOY, etc.
│   ├── local-azure-setup.md          # Consolida: AZURE_LOCAL_SETUP, AZURE_LOCAL_CONNECTION_SETUP, RUN_LOCAL_WITH_AZURE
│   └── key-vault/                     # Todo sobre Key Vault
│       ├── README.md
│       ├── setup.md                   # Consolida: AZURE_KEY_VAULT_SETUP
│       ├── rbac-configuration.md      # Consolida: CONFIGURAR_RBAC_KEYVAULT, PASOS_RAPIDOS_RBAC, etc.
│       └── secrets-management.md      # Consolida: CREAR_SECRETOS_EN_KEYVAULT, CREAR_KEYVAULT_Y_SECRETOS, etc.
├── features/                          # Características del proyecto
│   ├── README.md
│   ├── invitations.md                 # Consolida: INVITATION_*, etc.
│   └── email-configuration.md         # Consolida: EMAIL_CONFIGURATION, SETUP_EMAIL_SENDING, etc.
├── architecture/                      # Arquitectura
│   ├── README.md
│   ├── principles.md                  # ARCHITECTURAL_PRINCIPLES
│   └── project-structure.md           # PROJECT_STRUCTURE_EVALUATION
├── troubleshooting/                   # Solución de problemas
│   ├── README.md
│   └── common-issues.md               # Consolida: TROUBLESHOOTING, TROUBLESHOOTING_*, etc.
└── guides/                            # Guías especializadas
    ├── README.md
    ├── testing.md                     # Consolida: TESTING_GUIDE, LOCAL_TESTING_GUIDE
    └── service-calls.md               # SERVICE_CALLS_DOCUMENTATION
```

---

## 📋 Mapeo de Archivos Antiguos → Nuevos

### Getting Started

| Archivo Antiguo | Ubicación Nueva |
|----------------|-----------------|
| `INSTALL_DOTNET.md` | `docs/getting-started/installation.md` |
| `INSTALL_DOTNET_QUICK.md` | `docs/getting-started/installation.md` |
| `INSTALL_AZURE_CLI.md` | `docs/getting-started/installation.md` |
| `INSTALAR_AZURE_CLI.md` | `docs/getting-started/installation.md` |
| `QUICK_START.md` | `docs/getting-started/local-development.md` |
| `SETUP_GUIDE.md` | `docs/getting-started/local-development.md` |
| `SETUP_AND_RUN_LOCAL.md` | `docs/getting-started/local-development.md` |
| `RUNNING_LOCALLY.md` | `docs/getting-started/local-development.md` |

### Azure - Key Vault

| Archivo Antiguo | Ubicación Nueva |
|----------------|-----------------|
| `AZURE_KEY_VAULT_SETUP.md` | `docs/azure/key-vault/setup.md` |
| `CONFIGURAR_RBAC_KEYVAULT.md` | `docs/azure/key-vault/rbac-configuration.md` |
| `PASOS_RAPIDOS_RBAC.md` | `docs/azure/key-vault/rbac-configuration.md` |
| `RESUMEN_CONFIGURACION_RBAC.md` | `docs/azure/key-vault/rbac-configuration.md` |
| `SOLUCION_RBAC_KEYVAULT.md` | `docs/azure/key-vault/rbac-configuration.md` |
| `ASIGNAR_ROL_KEYVAULT_PASO_A_PASO.md` | `docs/azure/key-vault/rbac-configuration.md` |
| `PASOS_RAPIDOS_PERMISOS.md` | `docs/azure/key-vault/rbac-configuration.md` |
| `SOLUCIONAR_PERMISOS_KEYVAULT.md` | `docs/azure/key-vault/rbac-configuration.md` |
| `CREAR_SECRETOS_EN_KEYVAULT.md` | `docs/azure/key-vault/secrets-management.md` |
| `CREAR_KEYVAULT_Y_SECRETOS.md` | `docs/azure/key-vault/secrets-management.md` |
| `GUIA_KEYVAULT_PORTAL.md` | `docs/azure/key-vault/secrets-management.md` |
| `ACTUALIZAR_KEY_VAULT_PORTAL.md` | `docs/azure/key-vault/secrets-management.md` |
| `ACTUALIZAR_CREDENCIALES_PORTAL.md` | `docs/azure/key-vault/secrets-management.md` |
| `ACTUALIZAR_CREDENCIALES_AZURE.md` | `docs/azure/key-vault/secrets-management.md` |
| `QUICK_UPDATE_KEYVAULT.md` | `docs/azure/key-vault/secrets-management.md` |
| `QUICK_UPDATE_CREDENTIALS.md` | `docs/azure/key-vault/secrets-management.md` |
| `VERIFICAR_CONFIGURACION_KEYVAULT.md` | `docs/azure/key-vault/setup.md` |
| `PASOS_DESPUES_DE_CONFIGURAR_KEYVAULT.md` | `docs/azure/key-vault/rbac-configuration.md` |
| `RESUMEN_PASOS_SIGUIENTES.md` | `docs/azure/key-vault/rbac-configuration.md` |
| `CAMBIAR_A_ACCESS_POLICIES.md` | `docs/azure/key-vault/rbac-configuration.md` |

### Azure - General

| Archivo Antiguo | Ubicación Nueva |
|----------------|-----------------|
| `AZURE_INFRASTRUCTURE.md` | `docs/azure/infrastructure.md` |
| `AZURE_COMPONENTS_SUMMARY.md` | `docs/azure/infrastructure.md` |
| `AZURE_DEPLOYMENT_GUIDE.md` | `docs/azure/deployment/README.md` |
| `DEPLOY.md` | `docs/azure/deployment/README.md` |
| `DEPLOY_NOW.md` | `docs/azure/deployment/README.md` |
| `AZURE_LOCAL_SETUP.md` | `docs/azure/local-azure-setup.md` |
| `AZURE_LOCAL_CONNECTION_SETUP.md` | `docs/azure/local-azure-setup.md` |
| `RUN_LOCAL_WITH_AZURE.md` | `docs/azure/local-azure-setup.md` |
| `AZURE_CONNECTION_REVIEW.md` | `docs/azure/local-azure-setup.md` |

### Features

| Archivo Antiguo | Ubicación Nueva |
|----------------|-----------------|
| `EMAIL_CONFIGURATION.md` | `docs/features/email-configuration.md` |
| `SETUP_EMAIL_SENDING.md` | `docs/features/email-configuration.md` |
| `QUICK_EMAIL_SETUP.md` | `docs/features/email-configuration.md` |
| `CONFIGURAR_EMAIL_LOCAL.md` | `docs/features/email-configuration.md` |
| `AUTOMATIC_EMAIL_SENDING.md` | `docs/features/email-configuration.md` |
| `EMAIL_CONFIGURED.md` | `docs/features/email-configuration.md` |
| `INVITATION_IMPLEMENTATION_SUMMARY.md` | `docs/features/invitations.md` ✨ **Mejorado con diagramas visuales** |
| `INVITATION_QUICK_START.md` | `docs/features/invitations.md` |
| `INVITATION_FIXES.md` | `docs/features/invitations.md` |

**Nota**: El documento `docs/features/invitations.md` ahora incluye:
- ✅ Todo el contenido de los archivos originales
- ✅ Diagramas visuales profesionales con iconos de Azure
- ✅ Flujos de proceso completos
- ✅ Diagramas de secuencia y arquitectura

### Architecture

| Archivo Antiguo | Ubicación Nueva |
|----------------|-----------------|
| `ARCHITECTURAL_PRINCIPLES.md` | `docs/architecture/principles.md` |
| `PROJECT_STRUCTURE_EVALUATION.md` | `docs/architecture/project-structure.md` |
| `HYBRID_ARCHITECTURE_COMPLETE.md` | `docs/architecture/principles.md` |
| `HYBRID_ARCHITECTURE_COMPLIANCE.md` | `docs/architecture/principles.md` |

### Troubleshooting

| Archivo Antiguo | Ubicación Nueva |
|----------------|-----------------|
| `TROUBLESHOOTING.md` | `docs/troubleshooting/common-issues.md` |
| `TROUBLESHOOTING_INVITATION_ERROR.md` | `docs/troubleshooting/common-issues.md` |
| `PRODUCTION_TROUBLESHOOTING.md` | `docs/troubleshooting/common-issues.md` |
| `DEBUG_AZURE_SERVICES.md` | `docs/troubleshooting/debug-azure-services.md` |

### Guides

| Archivo Antiguo | Ubicación Nueva |
|----------------|-----------------|
| `TESTING_GUIDE.md` | `docs/guides/testing.md` |
| `LOCAL_TESTING_GUIDE.md` | `docs/guides/testing.md` |
| `SERVICE_CALLS_DOCUMENTATION.md` | `docs/guides/service-calls.md` |

### Archivos de Estado (Se pueden eliminar)

Estos archivos eran temporales o de estado y no contienen información permanente:

- `ACTION_REQUIRED.md`
- `BACKEND_STARTED.md`
- `CURRENT_STATUS.md`
- `DEPLOYMENT_IN_PROGRESS.md`
- `EMAIL_AND_ICON_FIXES.md`
- `REINICIAR_BACKEND.md`
- `RUNNING_STATUS.md`
- `START_BACKEND_NOW.md`
- `UI_DESIGN_REVIEW.md`

---

## 🔍 Cómo Buscar Información Ahora

### Por Tema

- **Instalación**: `docs/getting-started/installation.md`
- **Setup Local**: `docs/getting-started/local-development.md`
- **Key Vault**: `docs/azure/key-vault/README.md`
- **Deployment**: `docs/azure/deployment/README.md`
- **Email**: `docs/features/email-configuration.md`
- **Troubleshooting**: `docs/troubleshooting/README.md`

### Por Palabra Clave

Usa el índice principal en `docs/README.md` que tiene enlaces organizados por tema.

---

## ✅ Ventajas de la Nueva Estructura

1. **Organización Clara**: Cada tema tiene su carpeta
2. **Sin Duplicación**: Información consolidada en un solo lugar
3. **Navegación Fácil**: Índices en cada sección
4. **Mantenible**: Más fácil de actualizar y mantener
5. **Búsqueda Simple**: Estructura lógica facilita encontrar información

---

## 📝 Notas Importantes

- ✅ **Toda la información se mantiene**: Nada se perdió en la consolidación
- ✅ **Enlaces actualizados**: Los nuevos documentos tienen referencias cruzadas
- ✅ **README principal**: El `README.md` en la raíz del proyecto sigue siendo válido y ahora referencia la nueva estructura

---

## 🚀 Próximos Pasos

1. **Explora la nueva estructura**: Empieza con `docs/README.md`
2. **Actualiza enlaces**: Si tienes enlaces a documentos antiguos, actualízalos
3. **Elimina archivos antiguos**: Después de verificar que toda la información está en la nueva estructura, puedes eliminar los archivos antiguos

---

*¿Preguntas sobre la reorganización? Revisa `docs/README.md` para el índice completo.*

