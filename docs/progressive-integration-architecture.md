# Progressive Integration Architecture

## Overview

All external system integrations follow a **3-phase progressive rollout** strategy, allowing the system to deploy to production with mock services and gradually activate real integrations as they become available.

## Architecture Principle

**All integrations go through the Canonical Model API**

```
┌─────────────────────────────────────────────────────────────────┐
│                         Frontend (SWA)                           │
│                     Single Static Web App                        │
└────────────────────────────┬────────────────────────────────────┘
                             │
                    Always calls same API
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Canonical Model API                           │
│              /api/sap/vendor/{id}                                │
│              /api/rbac/check-permission                          │
│              /api/master-data/countries                          │
└────────────────────────────┬────────────────────────────────────┘
                             │
              Configuration-based routing
                             │
        ┌────────────────────┼────────────────────┐
        │                    │                    │
        ▼                    ▼                    ▼
  ┌──────────┐         ┌──────────┐        ┌──────────┐
  │   MOCK   │         │   MOCK   │        │   REAL   │
  │  Local   │         │  Azure   │        │  Azure   │
  └──────────┘         └──────────┘        └──────────┘
  Development          Production          Production
  (Local PC)          (No real systems)    (Connected)
```

## 3-Phase Deployment Strategy

### Phase 1: Local Development (Mock on Local Machine)
**Environment:** Developer's local machine  
**Configuration:** `Services:{Service}:UseMock = true`  
**Data:** In-memory mock data  
**Purpose:** Fast development without network dependencies

```json
{
  "Services": {
    "SAP": { "UseMock": true },
    "RBAC": { "UseMock": true },
    "MasterData": { "UseMock": true }
  }
}
```

**Characteristics:**
- ✓ No external connections needed
- ✓ Instant responses
- ✓ Predictable data for testing
- ✓ Works offline
- ✓ Full CRUD operations simulated

---

### Phase 2: Azure Deployment with Mock (Production-Ready Simulation)
**Environment:** Azure App Service / Container Apps  
**Configuration:** `Services:{Service}:UseMock = true`  
**Data:** In-memory mock data OR Azure SQL mock tables  
**Purpose:** Deploy to production while real systems are being configured

```json
{
  "Services": {
    "SAP": { "UseMock": true },  // SAP connection not ready yet
    "RBAC": { "UseMock": true }, // Azure AD not configured yet
    "MasterData": { "UseMock": false } // Using real SQL database
  }
}
```

**Characteristics:**
- ✓ **Production deployment** with simulated backends
- ✓ Frontend works normally
- ✓ Users can test full workflows
- ✓ Data persists in Azure SQL (not in-memory)
- ✓ Can demonstrate system to stakeholders
- ✓ No dependency on SAP/AD availability
- ✓ **Gradually activate real services**

**Progressive Activation:**
1. **Week 1:** Deploy with all Mock ✓
2. **Week 2:** Connect Master Data (SQL) → `UseMock: false` ✓
3. **Week 3:** Connect Azure AD (RBAC) → `UseMock: false` ✓
4. **Week 4:** Connect SAP or MoUV → `UseMock: false` ✓

---

### Phase 3: Azure Deployment with Real Systems (Fully Integrated)
**Environment:** Azure App Service / Container Apps  
**Configuration:** `Services:{Service}:UseMock = false`  
**Data:** Real SAP, Azure AD, SQL Server  
**Purpose:** Production with full integration

```json
{
  "Services": {
    "SAP": { 
      "UseMock": false,
      "RealProvider": "MoUV"  // or "SapNco"
    },
    "RBAC": { 
      "UseMock": false,
      "RealProvider": "AzureAd"
    },
    "MasterData": { 
      "UseMock": false 
    }
  }
}
```

**Characteristics:**
- ✓ All real-time data from live systems
- ✓ Production transactions
- ✓ Audit trails in real systems
- ✓ SSO with Azure AD
- ✓ SAP vendor numbers assigned
- ✓ **Can still fall back to Mock if systems go down**

---

## Service Integration Options

### SAP Vendor Service

**3 Implementation Options:**

```csharp
public interface ISapVendorService { }

// Option 1: Simulation (Local + Azure Mock)
public class SapVendorSimulationService : ISapVendorService
{
    // In-memory mock vendor data
    // Levenshtein fuzzy search
    // Instant responses
}

// Option 2: MoUV Proxy (Azure Real - Future)
public class SapVendorMouvProxyService : ISapVendorService
{
    // HTTP calls to MoUV API
    // MoUV handles SAP connection
    // Reuses UNESCO's proven integration
}

// Option 3: Direct SAP (Azure Real - Future)
public class SapVendorRfcService : ISapVendorService
 {
    // SAP NCo connector
    // Direct BAPI calls
    // Requires SAP NetWeaver RFC SDK
}
```

**Configuration:**
```json
{
  "Services": {
    "SAP": {
      "UseMock": false,
      "RealProvider": "MoUV",  // or "SapNco"
      "MockSettings": { ... },
      "MoUVSettings": {
        "BaseUrl": "https://mouv.hq.int.unesco.org/api",
        "ApiKey": "from-keyvault"
      },
      "SapNcoSettings": {
        "Host": "sap-server.company.com",
        "Client": "100",
        "SystemNumber": "00"
      }
    }
  }
}
```

---

### RBAC/Authorization Service

**3 Implementation Options:**

```csharp
public interface IAuthorizationService { }

// Option 1: Simulation
public class AuthorizationSimulationService : IAuthorizationService
{
    // Hardcoded users in config
    // Static role assignments
}

// Option 2: Azure AD (Real)
public class AzureAdAuthorizationService : IAuthorizationService
{
    // Microsoft Graph API
    // AD group membership
    // Claims-based authorization
}

// Option 3: Custom DB (Alternative Real)
public class DatabaseAuthorizationService : IAuthorizationService
{
    // User roles in SQL
    // Custom RBAC implementation
}
```

---

### Master Data Service

**3 Implementation Options:**

```csharp
public interface IMasterDataService { }

// Option 1: Simulation
public class MasterDataSimulationService : IMasterDataService
{
    // Hardcoded 195 countries
    // Hardcoded currencies, account groups
}

// Option 2: Database (Real)
public class DatabaseMasterDataService : IMasterDataService
{
    // Azure SQL master data tables
    // Cached with Redis
    // Admin UI for updates
}

// Option 3: SAP Master Data (Alternative Real)
public class SapMasterDataService : IMasterDataService
{
    // Pull from SAP T001 (companies)
    // Pull from T005 (countries)
    // Always in sync with SAP
}
```

---

## Deployment Workflow

### Step 1: Initial Deployment (All Mock)

```bash
# Deploy to Azure with all mock services
az webapp create ...
az webapp config appsettings set \
  --settings Services__SAP__UseMock=true \
             Services__RBAC__UseMock=true \
             Services__MasterData__UseMock=true
```

**Result:** ✓ System is live and functional with simulated data

---

### Step 2: Activate Database Master Data

```bash
# Switch Master Data to real SQL
az webapp config appsettings set \
  --settings Services__MasterData__UseMock=false
```

**Result:** ✓ Countries, currencies from real database

---

### Step 3: Activate Azure AD

```bash
# Configure Azure AD
az webapp config appsettings set \
  --settings Services__RBAC__UseMock=false \
             Services__RBAC__RealSettings__TenantId=xxx \
             Services__RBAC__RealSettings__ClientId=yyy
```

**Result:** ✓ SSO and real user roles active

---

### Step 4: Activate SAP (via MoUV)

```bash
# Connect to MoUV API
az webapp config appsettings set \
  --settings Services__SAP__UseMock=false \
             Services__SAP__RealProvider=MoUV \
             Services__SAP__MoUVSettings__BaseUrl=https://mouv.hq.int.unesco.org/api \
             Services__SAP__MoUVSettings__ApiKey=@Microsoft.KeyVault(...)
```

**Result:** ✓ Real SAP vendor operations via MoUV

---

### Step 5: Switch to Direct SAP (Optional - Future)

```bash
# Switch from MoUV to direct SAP
az webapp config appsettings set \
  --settings Services__SAP__RealProvider=SapNco \
             Services__SAP__SapNcoSettings__Host=sap.company.com \
             Services__SAP__SapNcoSettings__Client=100
```

**Result:** ✓ Direct SAP integration

---

## Code Architecture

### Service Registration (Program.cs)

```csharp
// SAP Service - Select implementation based on config
var sapProvider = builder.Configuration["Services:SAP:RealProvider"];
var useSapMock = builder.Configuration.GetValue<bool>("Services:SAP:UseMock", true);

if (useSapMock)
{
    builder.Services.AddScoped<ISapVendorService, SapVendorSimulationService>();
}
else if (sapProvider == "MoUV")
{
    builder.Services.AddScoped<ISapVendorService, SapVendorMouvProxyService>();
}
else if (sapProvider == "SapNco")
{
    builder.Services.AddScoped<ISapVendorService, SapVendorRfcService>();
}
```

### Frontend - No Changes Required

```typescript
// Frontend always calls same API
const vendor = await fetch('/api/sap/vendor/10189999');

// Backend decides: Mock, MoUV, or Direct SAP
// Frontend doesn't know or care
```

---

## Benefits

### ✅ Deploy Early, Integrate Later
- Deploy to production **before** SAP connection is ready
- Demonstrate system to stakeholders with real UI/UX
- Gather feedback on workflows

### ✅ Zero Downtime Migration
- Switch from Mock → Real without code changes
- Just update configuration
- Instant rollback if issues

### ✅ Independent Service Activation
- SAP can go live while RBAC is still Mock
- Master Data can go real while SAP is Mock
- Each service progresses independently

### ✅ Disaster Recovery
- If SAP goes down, flip back to Mock temporarily
- System stays operational
- Users see graceful degradation message

### ✅ Testing Flexibility
- Test with Mock in Staging
- Test with Real in Production
- A/B test different implementations

---

## Service Implementation Matrix

| Service | Mock (Phase 1/2) | Real Option 1 (Phase 3) | Real Option 2 (Future) |
|---------|------------------|-------------------------|------------------------|
| **SAP** | In-Memory Simulation | MoUV API Proxy | SAP NCo Direct |
| **RBAC** | Static Config | Azure AD Graph | Custom DB |
| **Master Data** | Hardcoded Lists | Azure SQL | SAP Master Data |
| **Email** | Console Logging | SendGrid | Azure Communication |
| **File Storage** | Temp Filesystem | Azure Blob | SharePoint |
| **Workflow** | In-Memory State | Azure SQL + Service Bus | Power Automate |

---

## Current Status

### ✅ Implemented (Ready to Test)
- SAP Simulation Service
- SAP RFC Service (skeleton)
- SAP Controller with Swagger
- Configuration system
- Service registration with toggles

### 🚧 Next Steps
- RBAC Simulation Service
- RBAC Azure AD Service
- Master Data Simulation Service
- Master Data Database Service
- MoUV Proxy Service (Future)

---

## Testing Instructions

### Test SAP Mock Service (Now)

1. **Start API:**
```bash
cd backend/VendorMdm.Api
dotnet run
```

2. **Open Swagger:**
```
https://localhost:5001/swagger
```

3. **Test Endpoints:**
- `POST /api/sap/vendor/search` - Fuzzy vendor search
- `GET /api/sap/vendor/10189999` - Get vendor details
- `POST /api/sap/validate/name` - Name validation
- `POST /api/sap/validate/bank` - IBAN/SWIFT validation

### Toggle to Real SAP (When Ready)

**Option A: appsettings.json**
```json
{
  "Services": {
    "SAP": {
      "UseMock": false,
      "RealProvider": "MoUV"
    }
  }
}
```

**Option B: Azure App Settings**
```bash
az webapp config appsettings set \
  --name app-vendor-mdm-dev \
  --resource-group rg-vendor-mdm-dev \
  --settings Services__SAP__UseMock=false \
             Services__SAP__RealProvider=MoUV
```

**Result:** Same API, different backend. Frontend unchanged.

---

## Summary

🎯 **Key Principle:** All integrations through Canonical API  
📦 **3 Deployment Phases:** Local Mock → Azure Mock → Azure Real  
🔧 **Configuration-Driven:** Toggle via `appsettings.json`  
🚀 **Progressive Rollout:** Activate services one at a time  
♻️ **Zero Code Changes:** Frontend never changes  
🔄 **Instant Rollback:** Flip config if issues arise  

This architecture allows you to **ship to production immediately** with mock services, then **progressively activate real integrations** as they become available, all while **maintaining a single codebase** and **zero frontend changes**.
