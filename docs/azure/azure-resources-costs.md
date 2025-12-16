# Azure Resources - Costs & Configuration

**Last Updated:** 2025-12-16  
**Environment:** Development (`rg-vendor-mdm-dev-v3`)  
**Region:** Central US

---

## Resource Configuration Summary

| Recurso | SKU/Tier Dev | Razón Dev | Costo Dev/mes | SKU/Tier Prod | Razón Prod | Costo Prod/mes |
|---------|--------------|-----------|---------------|---------------|------------|----------------|
| **SQL Database** | Basic (5 DTU) | Free no disponible, mínimo tier | ~$5 | S0 Standard (10 DTU) | Más DTU, backups automáticos | ~$15 |
| **Cosmos DB** | Serverless | Pay-per-request, ideal dev/test | $1-5 | Serverless | Escala según uso real | $5-20 |
| **Service Bus** | Basic | Suficiente para Queues | ~$0.05 | Standard | Requiere Topics/Subscriptions | ~$10 |
| **App Service Plan** | F1 (Free) | Gratis, suficiente para dev | $0 | B1 Basic | AlwaysOn, sin sleep | ~$13 |
| **Backend API** | F1 (Free) | Incluido en plan | $0 | B1 Basic | Incluido en plan | - |
| **Static Web App** | Free | Gratis, custom domains OK | $0 | Standard | Staging environments | ~$9 |
| **Key Vault** | Standard | No hay tier free | ~$0.50 | Standard | Mismo tier | ~$1 |
| **App Insights** | Pay-as-you-go | 5GB/mes gratis | $0-2 | Pay-as-you-go | Más telemetría | $5-10 |
| **TOTAL** | | | **$7-13/mes** | | | **$58-78/mes** |

---

## Detailed Resource Configuration

### 1. SQL Server & Database
**Name:** `sql-vendor-mdm-dev` / `VendorMdmDb`  
**Region:** Central US

**Development (Actual):**
```bicep
sku: {
  name: 'Basic'
  tier: 'Basic'
  capacity: 5  // 5 DTUs
}
properties: {
  maxSizeBytes: 2147483648  // 2GB
}
```
- **Costo:** ~$5/mes
- **Límites:** 5 DTU, 2GB storage

**Production Path:**
```bicep
sku: {
  name: 'S0'
  tier: 'Standard'
  capacity: 10  // 10 DTUs
}
properties: {
  maxSizeBytes: 268435456000  // 250GB
}
```
- **Costo:** ~$15/mes
- **Beneficios:** 10 DTU, 250GB storage, backups automáticos

---

### 2. Cosmos DB
**Name:** `cosmos-vendor-mdm-dev`  
**Region:** Central US

**Development & Production:**
```bicep
capabilities: [
  { name: 'EnableServerless' }
]
```
- **Costo Dev:** $1-5/mes (bajo uso)
- **Costo Prod:** $5-20/mes (según tráfico)
- **Pricing:** $0.25 por millón de RU + $0.25/GB storage/mes

---

### 3. Service Bus
**Name:** `sb-vendor-mdm-dev`  
**Region:** Central US

**Development (Actual):**
```bicep
sku: {
  name: 'Basic'
  tier: 'Basic'
}
```
- **Costo:** ~$0.05/mes
- **Límites:** Solo Queues (no Topics)

**Production Path:**
```bicep
sku: {
  name: 'Standard'
  tier: 'Standard'
}
```
- **Costo:** ~$10/mes
- **Beneficios:** Topics + Subscriptions, auto-forwarding

---

### 4. App Service Plan & Backend API
**Name:** `asp-vendor-mdm-dev` / `app-vendor-mdm-api-dev`  
**Region:** Central US

**Development (Actual):**
```bicep
sku: {
  name: 'F1'
  tier: 'Free'
}
properties: {
  siteConfig: {
    alwaysOn: false  // No disponible en F1
  }
}
```
- **Costo:** $0/mes
- **Límites:** 1GB RAM, 60 CPU min/día, app duerme tras inactividad

**Production Path:**
```bicep
sku: {
  name: 'B1'
  tier: 'Basic'
}
properties: {
  siteConfig: {
    alwaysOn: true  // ✅ App siempre activa
  }
}
```
- **Costo:** ~$13/mes
- **Beneficios:** 1.75GB RAM, AlwaysOn, custom domains, SSL

---

### 5. Static Web App
**Name:** `swa-vendor-mdm-dev`  
**Region:** Central US

**Development (Actual):**
```bicep
sku: {
  name: 'Free'
  tier: 'Free'
}
```
- **Costo:** $0/mes
- **Límites:** 100GB bandwidth/mes, no staging environments

**Production Path:**
```bicep
sku: {
  name: 'Standard'
  tier: 'Standard'
}
```
- **Costo:** ~$9/mes
- **Beneficios:** Custom domains, staging environments, 100GB bandwidth incluido

---

### 6. Key Vault
**Name:** `kv-vendor-mdm-dev`  
**Region:** Central US

**Development & Production:**
```bicep
sku: {
  family: 'A'
  name: 'standard'
}
```
- **Costo Dev:** ~$0.50/mes
- **Costo Prod:** ~$1/mes
- **Pricing:** $0.03 por 10,000 operaciones

---

### 7. Application Insights
**Name:** `ai-vendor-mdm-dev`  
**Region:** Central US

**Development & Production:**
```bicep
kind: 'web'
properties: {
  Application_Type: 'web'
}
```
- **Costo Dev:** $0-2/mes (dentro de 5GB free tier)
- **Costo Prod:** $5-10/mes
- **Free tier:** 5GB ingestion/mes incluido

---

## Quota Management

### Problemas Históricos Resueltos

1. **SQL Free Tier No Disponible**
   - Error: `Free tier not available for SQL Database`
   - Solución: Migrado a `Basic` tier (mínimo disponible)
   - Status: ✅ Resuelto

2. **App Service Quota Limits**
   - Riesgo: Límite de 1 App Service Free (F1) por subscription
   - Error posible: `SubscriptionIsOverQuotaForSku`
   - Solución: Upgrade a `B1` Basic si ocurre
   - Status: ✅ F1 funcionando actualmente

3. **Región Consistency**
   - Decisión: Todo en `Central US`
   - Razón: Mejor disponibilidad de quotas, menor latencia
   - Status: ✅ Todos los recursos en centralus

---

## Upgrade Commands

### SQL Database: Basic → Standard
```bash
az sql db update \
  --resource-group rg-vendor-mdm-dev-v3 \
  --server sql-vendor-mdm-dev \
  --name VendorMdmDb \
  --edition Standard \
  --service-objective S0
```

### App Service Plan: F1 → B1
```bash
az appservice plan update \
  --resource-group rg-vendor-mdm-dev-v3 \
  --name asp-vendor-mdm-dev \
  --sku B1
```

### Service Bus: Basic → Standard
```bash
az servicebus namespace update \
  --resource-group rg-vendor-mdm-dev-v3 \
  --name sb-vendor-mdm-dev \
  --sku Standard
```

### Static Web App: Free → Standard
```bash
az staticwebapp update \
  --resource-group rg-vendor-mdm-dev-v3 \
  --name swa-vendor-mdm-dev \
  --sku Standard
```

---

## Cost Monitoring

### View Current Month Costs
```bash
az consumption usage list \
  --start-date $(date -u -d "1 month ago" '+%Y-%m-%d') \
  --end-date $(date -u '+%Y-%m-%d') \
  --query "[?contains(instanceName, 'vendor-mdm')].{Resource:instanceName, Cost:pretaxCost}" \
  --output table
```

### Set Budget Alert
```bash
az consumption budget create \
  --resource-group rg-vendor-mdm-dev-v3 \
  --budget-name vendor-mdm-dev-budget \
  --amount 20 \
  --time-grain Monthly \
  --start-date $(date -u '+%Y-%m-01') \
  --end-date 2026-12-31
```

---

## Notes

- **CRITICAL:** Always update this document when modifying SKUs in `main.bicep`
- **Region:** All resources MUST remain in `Central US` unless quota issues arise
- **Cost estimates:** Based on December 2025 pricing, subject to change
- **Production upgrade:** Plan for ~$60-80/mes when moving to production

---

**Maintained by:** Infrastructure Team  
**Review Frequency:** Monthly or after SKU changes
