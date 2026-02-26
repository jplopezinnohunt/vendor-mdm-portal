# Azure Infrastructure: POC vs Production Cost Comparison

**Version**: 1.0
**Date**: 2026-02-26
**Purpose**: Document the cost implications of current POC/Free-tier architecture versus a production-ready deployment, aligned with the SAP landscape mirroring strategy (DEV/QA/PROD).

---

## Executive Summary

The Vendor MDM Portal currently runs on Azure Free/Dev tiers optimized for POC validation. Moving to production requires upgrading services across **three environments** (DEV, QA, PROD) that mirror the SAP landscape (D01, Q01, P01). This document outlines what changes, why, and the estimated monthly costs.

---

## Environment Strategy (SAP Landscape Mirroring)

Per the [SAP Environment Strategy](integration/sap-environment-strategy.md), environments are **NOT** 1:1 with SAP but follow a flexible configuration model:

| Platform Environment | Default SAP Target | Azure Resource Group | Purpose |
|----------------------|-------------------|---------------------|---------|
| **DEV** | SAP D01 | `rg-vendor-mdm-dev-v3` | Development, integration testing, debugging |
| **QA** (Staging) | SAP Q01 | `rg-vendor-mdm-qa` | Pre-production validation, UAT, performance testing |
| **PROD** | SAP P01 | `rg-vendor-mdm-prod` | Live operations, end users |

**Key Principle**: Each environment is a full, independent Azure deployment. DEV stays on free/low tiers. QA mirrors production config at smaller scale. PROD runs on production-grade SKUs.

---

## Current Architecture (POC Mode)

All services deployed in a single environment (DEV) using free/lowest tiers:

| # | Azure Service | Resource Name | Current Tier | Monthly Cost |
|---|--------------|---------------|-------------|-------------|
| 1 | App Service Plan | `asp-vendor-mdm-dev` | F1 Free (Linux) | $0 |
| 2 | App Service (API) | `app-vendor-mdm-api-dev` | F1 Free (.NET 8.0) | $0 |
| 3 | Static Web App | `swa-vendor-mdm-dev` | Free | $0 |
| 4 | Cosmos DB | `cosmos-vendor-mdm-dev` | Serverless | ~$1-5 |
| 5 | SQL Database | `sql-vendor-mdm-dev` | Basic (5 DTU, 2GB) | ~$5 |
| 6 | Service Bus | `sb-vendor-mdm-dev` | Basic | ~$0.05/1M ops |
| 7 | Storage Account | `stvendormdmdev` | Standard_LRS, Hot | ~$0.50 |
| 8 | Key Vault | `kv-vendor-mdm-dev` | Standard | ~$0.03/10K ops |
| 9 | Application Insights | `ai-vendor-mdm-dev` | Pay-as-you-go (5GB free) | $0 |
| 10 | Function App | *(not deployed)* | Y1 Consumption | $0 |

**POC Total: ~$5-15/month** (single environment)

---

## Production Architecture (3-Environment Model)

### Per-Service Tier Comparison

| Service | DEV Tier | QA Tier | PROD Tier | Key Changes for PROD |
|---------|----------|---------|-----------|---------------------|
| **App Service Plan** | F1 Free | B1 Basic | S1 Standard | AlwaysOn, auto-scale, staging slots, SLA |
| **Static Web App** | Free | Free | Standard | SLA, custom auth providers, 100GB bandwidth |
| **Cosmos DB** | Serverless | Serverless | Serverless or Provisioned (400 RU/s) | Multi-region option, backup policies |
| **SQL Database** | Basic (5 DTU) | Standard S0 (10 DTU) | Standard S1 (20 DTU) | More DTUs, 250GB, geo-replication |
| **Service Bus** | Basic | Standard | Standard | Topics/Subscriptions support (required for SAP integration events) |
| **Storage Account** | Standard_LRS | Standard_LRS | Standard_GRS | Geo-redundancy for disaster recovery |
| **Key Vault** | Standard | Standard | Standard + RBAC | Purge protection, RBAC, 90-day soft delete |
| **App Insights** | Pay-as-you-go | Pay-as-you-go | Commitment Tier | Daily cap, alerting, availability tests |
| **Function App** | Y1 Consumption | Y1 Consumption | EP1 Elastic Premium | Always warm, VNET integration |

### Monthly Cost Estimates by Environment

#### DEV Environment (~$5-15/month)
*Keep free/low tiers for development. Connects to SAP D01.*

| Service | Tier | Est. Cost |
|---------|------|-----------|
| App Service | F1 Free | $0 |
| Static Web App | Free | $0 |
| Cosmos DB | Serverless | $1-5 |
| SQL Database | Basic (5 DTU) | $5 |
| Service Bus | Basic | <$1 |
| Storage | Standard_LRS | <$1 |
| Key Vault | Standard | <$1 |
| App Insights | Free (5GB) | $0 |
| Function App | Consumption | $0 |
| **DEV Total** | | **~$10-15** |

#### QA Environment (~$80-120/month)
*Mid-tier for pre-production validation. Connects to SAP Q01.*

| Service | Tier | Est. Cost |
|---------|------|-----------|
| App Service | B1 Basic (Linux) | $13 |
| Static Web App | Free | $0 |
| Cosmos DB | Serverless | $5-15 |
| SQL Database | Standard S0 (10 DTU) | $15 |
| Service Bus | Standard | $10 |
| Storage | Standard_LRS | $1-2 |
| Key Vault | Standard | <$1 |
| App Insights | Pay-as-you-go | $5-10 |
| Function App | Consumption (Y1) | $0-5 |
| **QA Total** | | **~$50-75** |

#### PROD Environment (~$200-450/month)
*Production-grade with SLA. Connects ONLY to SAP P01.*

| Service | Tier | Est. Cost |
|---------|------|-----------|
| App Service | S1 Standard (Linux) | $70 |
| Static Web App | Standard | $9 |
| Cosmos DB | Serverless or Provisioned 400 RU/s | $24-50 |
| SQL Database | Standard S1 (20 DTU) | $30 |
| Service Bus | Standard | $10 |
| Storage | Standard_GRS | $3-5 |
| Key Vault | Standard | <$1 |
| App Insights | Commitment Tier or Pay-as-you-go | $20-50 |
| Function App | EP1 Elastic Premium | $220 |
| **PROD Total** | | **~$390-450** |

> **Note**: Function App EP1 is the biggest cost driver. Consider staying on Consumption (Y1) if cold starts are acceptable, reducing PROD to ~$170-230/month.

### Total Monthly Cost Summary

| Scenario | DEV | QA | PROD | Total |
|----------|-----|-----|------|-------|
| **Current (POC)** | $10-15 | - | - | **$10-15** |
| **Production Lite** (Y1 Functions) | $10-15 | $50-75 | $170-230 | **$230-320** |
| **Production Standard** (EP1 Functions) | $10-15 | $50-75 | $390-450 | **$450-540** |
| **Production HA** (P1v3 + Multi-region) | $10-15 | $80-120 | $800-1,200 | **$890-1,335** |

---

## Critical Bicep Changes Required

### 1. App Service Plan (F1 -> S1)

```bicep
// infrastructure/main.bicep
resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: environmentName == 'prod' ? 'S1' : environmentName == 'qa' ? 'B1' : 'F1'
    tier: environmentName == 'prod' ? 'Standard' : environmentName == 'qa' ? 'Basic' : 'Free'
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}
```

### 2. AlwaysOn (requires paid tier)

```bicep
alwaysOn: environmentName != 'dev'  // Enable for QA and PROD
```

### 3. Service Bus (Basic -> Standard for Topics)

```bicep
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  sku: {
    name: environmentName == 'dev' ? 'Basic' : 'Standard'
    tier: environmentName == 'dev' ? 'Basic' : 'Standard'
  }
}
```

### 4. Static Web App (Free -> Standard for PROD)

```bicep
resource staticWebApp 'Microsoft.Web/staticSites@2023-01-01' = {
  sku: {
    name: environmentName == 'prod' ? 'Standard' : 'Free'
    tier: environmentName == 'prod' ? 'Standard' : 'Free'
  }
}
```

### 5. Storage Redundancy (LRS -> GRS for PROD)

```bicep
sku: {
  name: environmentName == 'prod' ? 'Standard_GRS' : 'Standard_LRS'
}
```

### 6. Key Vault Security (RBAC + Purge Protection for PROD)

```bicep
enableRbacAuthorization: environmentName == 'prod'
enablePurgeProtection: environmentName == 'prod'
softDeleteRetentionInDays: environmentName == 'prod' ? 90 : 7
```

---

## Security Upgrades for Production

| Area | POC (Current) | Production (Required) |
|------|--------------|----------------------|
| Authentication | Disabled (testing) | Azure AD enforced |
| Key Vault | Access Policies, no purge protection | RBAC, purge protection, 90-day retention |
| Network | Public endpoints | Private endpoints + VNET integration |
| Storage | Public blob access disabled | + Private endpoints |
| SQL | Public network access | + Firewall rules, VNET service endpoints |
| TLS | 1.2 minimum | 1.2 minimum (already set) |

---

## Free Tier Limitations That Block Production

| Limitation | Free Tier Behavior | Production Impact |
|-----------|-------------------|------------------|
| No AlwaysOn | App idles after 20min, cold starts | Users experience 10-30s delays |
| No SLA | No uptime guarantee | Cannot promise availability to business |
| No auto-scale | Single instance only | Cannot handle concurrent users |
| No staging slots | Downtime during deploys | Zero-downtime deployment impossible |
| No Topics (SB Basic) | Queue-only messaging | Cannot implement pub/sub for SAP events |
| 5 DTU / 2GB (SQL Basic) | Very limited performance | Concurrent queries will timeout |
| No custom auth (SWA Free) | Limited auth providers | Cannot enforce corporate SSO |
| Single region | No disaster recovery | Single point of failure |

---

## Recommended Migration Path

### Phase 1: Minimum Viable Production (~$230-320/month total)
1. Upgrade App Service DEV to keep F1, add QA as B1, PROD as S1
2. Upgrade Service Bus to Standard (QA + PROD) for Topics support
3. Re-enable Azure AD authentication
4. Enable Key Vault RBAC and purge protection (PROD)
5. Keep Cosmos DB Serverless (good for variable traffic)
6. Keep SWA Free (upgrade to Standard later)
7. Deploy Function App on Consumption (Y1) plan

### Phase 2: Production Hardened (~$450-540/month total)
1. Upgrade SWA to Standard (PROD) for SLA
2. Upgrade Function App to EP1 (PROD) for always-warm execution
3. Upgrade SQL to S1 (20 DTU) for PROD
4. Enable GRS on Storage (PROD)
5. Add Application Insights alerting and availability tests

### Phase 3: High Availability (if needed, ~$890-1,335/month)
1. Upgrade App Service to P1v3 with auto-scale
2. Add Cosmos DB multi-region replication
3. Enable SQL geo-replication
4. Add Premium Service Bus tier
5. Implement full VNET isolation with private endpoints

---

## References

- [Azure Pricing Calculator](https://azure.microsoft.com/en-us/pricing/calculator/)
- [App Service Pricing](https://azure.microsoft.com/en-us/pricing/details/app-service/linux/)
- [Cosmos DB Pricing](https://azure.microsoft.com/en-us/pricing/details/cosmos-db/autoscale-provisioned/)
- [Service Bus Pricing](https://azure.microsoft.com/en-us/pricing/details/service-bus/)
- [Static Web Apps Pricing](https://azure.microsoft.com/en-us/pricing/details/app-service/static/)
- [SQL Database Pricing](https://azure.microsoft.com/en-us/pricing/details/azure-sql-database/single/)
- [SAP Environment Strategy](integration/sap-environment-strategy.md)
- [Infrastructure Bicep](../infrastructure/main.bicep)

---

**Note**: All prices are approximate estimates based on US Central region (February 2026). Use the [Azure Pricing Calculator](https://azure.microsoft.com/en-us/pricing/calculator/) for exact figures. Prices may vary by region and are subject to change.
