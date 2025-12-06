# 🔍 Azure Connection Configuration Review

## Summary

I've reviewed your project structure and Azure connection configuration. Here's what I found and what needs to be done to connect locally to your existing Azure resources.

---

## ✅ What I've Done

1. **Reviewed Azure Infrastructure**
   - Identified existing Azure resources:
     - SQL: `mdmportal-sql-12031241-dev.database.windows.net`
     - Cosmos: `mdmportal-cosmos-dev.documents.azure.com`
     - Service Bus: `mdmportal-sb-dev.servicebus.windows.net`

2. **Analyzed Connection Logic**
   - Reviewed `Program.cs` - has smart fallback logic
   - Detects placeholders and falls back to local emulators
   - Supports both connection strings and Managed Identity

3. **Created Documentation**
   - `AZURE_LOCAL_CONNECTION_SETUP.md` - Detailed connection guide
   - `SETUP_AND_RUN_LOCAL.md` - Step-by-step setup instructions
   - `setup-azure-connections.sh` - Helper script for User Secrets

4. **Improved Code**
   - Enhanced placeholder detection in `Program.cs`
   - Better error messages for connection issues

---

## 📋 Current Configuration Status

### appsettings.json (Production)
✅ **Correctly configured** for Azure deployment:
- Uses Managed Identity for SQL
- Uses endpoint URLs for Cosmos and Service Bus
- Ready for Azure App Service deployment

### appsettings.Development.json (Local)
⚠️ **Has placeholders** that need to be replaced:
```json
{
  "ConnectionStrings": {
    "Sql": "...User ID=YOUR_SQL_USERNAME;Password=YOUR_SQL_PASSWORD;...",
    "Cosmos": "...AccountKey=YOUR_COSMOS_KEY;",
    "ServiceBus": "...SharedAccessKey=YOUR_SERVICE_BUS_KEY;"
  },
  "UseLocalEmulators": false
}
```

**Current Behavior:**
- Program.cs detects placeholders (`YOUR_*`)
- Automatically falls back to local emulators
- Shows warning message

---

## 🔧 What You Need to Do

### Step 1: Get Connection Strings from Azure Portal

#### SQL Database
1. Azure Portal → SQL databases → `mdmportal-sqldb-dev`
2. Connection strings → ADO.NET
3. Copy and replace `{your_username}` and `{your_password}`

**Format:**
```
Server=tcp:mdmportal-sql-12031241-dev.database.windows.net,1433;
Initial Catalog=mdmportal-sqldb-dev;
Persist Security Info=False;
User ID=YOUR_USERNAME;
Password=YOUR_PASSWORD;
MultipleActiveResultSets=False;
Encrypt=True;
TrustServerCertificate=False;
Connection Timeout=30;
```

#### Cosmos DB
1. Azure Portal → Cosmos DB → `mdmportal-cosmos-dev`
2. Keys → Primary Connection String

**Format:**
```
AccountEndpoint=https://mdmportal-cosmos-dev.documents.azure.com:443/;
AccountKey=YOUR_PRIMARY_KEY==;
```

#### Service Bus
1. Azure Portal → Service Bus → `mdmportal-sb-dev`
2. Shared access policies → RootManageSharedAccessKey
3. Primary Connection String

**Format:**
```
Endpoint=sb://mdmportal-sb-dev.servicebus.windows.net/;
SharedAccessKeyName=RootManageSharedAccessKey;
SharedAccessKey=YOUR_SHARED_ACCESS_KEY==;
```

### Step 2: Configure Connection Strings

**Option A: User Secrets (Recommended)**
```bash
cd backend/VendorMdm.Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Sql" "YOUR_SQL_CONNECTION_STRING"
dotnet user-secrets set "ConnectionStrings:Cosmos" "YOUR_COSMOS_CONNECTION_STRING"
dotnet user-secrets set "ConnectionStrings:ServiceBus" "YOUR_SERVICEBUS_CONNECTION_STRING"
```

**Option B: Update appsettings.Development.json**
Replace all `YOUR_*` placeholders with actual values.

### Step 3: Configure Firewall Rules

**SQL Database:**
- Azure Portal → SQL Server → Networking
- Add your current IP address

**Cosmos DB:**
- Azure Portal → Cosmos DB → Networking
- Add your IP or allow Azure IPs

**Service Bus:**
- Azure Portal → Service Bus → Networking
- Add your IP or allow Azure IPs

### Step 4: Run the Project

**Backend:**
```bash
cd backend/VendorMdm.Api
dotnet restore
dotnet run
```

**Frontend (new terminal):**
```bash
cd frontend
npm install
npm run dev
```

---

## 🔍 Connection Logic Flow

```
1. Check UseLocalEmulators flag
   ├─ true → Use LocalConnectionStrings
   └─ false → Continue to step 2

2. Read ConnectionStrings from config
   ├─ Contains "YOUR_*" → Fallback to local emulators
   └─ Valid connection strings → Use Azure resources

3. Connect to Azure
   ├─ SQL: Use connection string directly
   ├─ Cosmos: Use connection string (if AccountKey present)
   │          OR DefaultAzureCredential (if endpoint only)
   └─ Service Bus: Use connection string (if SharedAccessKey present)
                   OR DefaultAzureCredential
```

---

## ✅ Verification Checklist

Before running, verify:

- [ ] .NET 8 SDK installed (`dotnet --version`)
- [ ] Node.js 18+ installed (`node --version`)
- [ ] Connection strings retrieved from Azure Portal
- [ ] Connection strings configured (User Secrets or appsettings.Development.json)
- [ ] Firewall rules configured for your IP
- [ ] `UseLocalEmulators: false` in appsettings.Development.json

---

## 🚨 Common Issues

### Issue: "Cannot open server"
**Cause:** SQL firewall blocking your IP  
**Solution:** Add your IP in Azure Portal → SQL Server → Networking

### Issue: "Unauthorized" error
**Cause:** Invalid or expired connection string  
**Solution:** Regenerate keys in Azure Portal and update connection strings

### Issue: Falls back to local emulators
**Cause:** Placeholders still in connection strings  
**Solution:** Replace all `YOUR_*` values with actual credentials

### Issue: "Connection timeout"
**Cause:** Network/firewall issue  
**Solution:** Check internet connection and firewall rules

---

## 📚 Documentation Created

1. **AZURE_LOCAL_CONNECTION_SETUP.md**
   - Detailed guide for getting connection strings
   - Step-by-step Azure Portal instructions
   - Troubleshooting section

2. **SETUP_AND_RUN_LOCAL.md**
   - Complete setup guide
   - Prerequisites checklist
   - Quick reference for ports and resources

3. **setup-azure-connections.sh**
   - Helper script for setting User Secrets
   - Interactive prompt for connection strings

---

## 🎯 Next Steps

1. **Get Connection Strings** from Azure Portal (see Step 1 above)
2. **Configure** using User Secrets or appsettings.Development.json
3. **Set Firewall Rules** for your IP address
4. **Run Backend** - should show "☁️ Using Azure Resources."
5. **Run Frontend** - should connect to backend API
6. **Test** - Create invitation, verify data in Azure SQL

---

## 💡 Tips

- **User Secrets** are stored outside the project (not in git)
- **appsettings.Development.json** is gitignored (safe to edit)
- **Managed Identity** works in Azure, but locally you need connection strings
- **Firewall rules** may need to be updated if your IP changes

---

*Review completed: 2025-01-27*

