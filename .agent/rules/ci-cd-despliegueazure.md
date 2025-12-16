---
trigger: always_on
---

## CI/CD y Despliegue en Azure
1. **Método de Despliegue Obligatorio:**
   - Frontend (Static Web App): GitHub Actions ÚNICAMENTE
   - Backend API, Functions, Infraestructura: Azure CLI ÚNICAMENTE
   - NUNCA usar GitHub Actions para backend/functions/infra
2. **Source of Truth:**
   - Azure (producción) es la fuente de verdad
   - Código local debe sincronizarse CON Azure, no modificar Azure desde local sin proceso controlado
3. **Nombres de Recursos:**
   - Seguir patrón: `<tipo>-vendor-mdm-<ambiente>`
   - Mantener `main.bicep` alineado con nombres reales en Azure
   - Verificar connection strings en `appsettings.json` apuntan a recursos existentes
4. **Documentación Obligatoria de Cambios Azure:**
   - TODO plan de implementación que modifique infraestructura Azure DEBE incluir tabla explícita de "Cambios en Recursos Azure"
   - Especificar: recurso afectado, tipo de cambio, detalle, impacto (costo/tiempo/dependencias)
   - Incluir método de aplicación (comando Azure CLI específico)
5. **Documentación de Costos (OBLIGATORIO):**
   - Al modificar SKUs/tiers en [main.bicep](cci:7://file:///Users/jplopez/projects/vendor-mdm-portal/infrastructure/main.bicep:0:0-0:0), ACTUALIZAR [docs/azure/azure-resources-costs.md](cci:7://file:///Users/jplopez/projects/vendor-mdm-portal/docs/azure/azure-resources-costs.md:0:0-0:0)
   - Mantener tabla de costos Dev vs Prod actualizada
   - Incluir razones para cada cambio de SKU
   - Actualizar estimados de costo mensual
6. **Validación Pre-Push:**
   - Build local exitoso antes de push
   - Tests locales exitosos (unit tests mínimo)
   - Workflows de GitHub deben pasar o estar deshabilitados
   - Código NO es production-ready hasta que CI/CD pase (build + tests)
