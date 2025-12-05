# 🎯 Run Locally → Connect to Azure Resources

## Quick Start

### Terminal 1: Backend (with Azure connection)
```powershell
cd backend/VendorMdm.Api

# Set environment variable for Azure connection
$env:ASPNETCORE_ENVIRONMENT="Development"

dotnet run
```

### Terminal 2: Frontend
```powershell
cd frontend
npm run dev
```

### Open Browser
Navigate to: `http://localhost:5173/admin/invite-vendor`

---

## 🔑 Get Azure Connection Strings

### Option A: From Azure Portal

**SQL Database:**
1. Azure Portal → SQL Database → "vendormdm-db"
2. Settings → Connection strings
3. Copy **ADO.NET** connection string

**Cosmos DB:**
1. Azure Portal → Cosmos DB Account
2. Settings → Keys
3. Copy **Primary Connection String**

**Service Bus:**
1. Azure Portal → Service Bus Namespace
2. Settings → Shared access policies → RootManageSharedAccessKey
3. Copy **Primary Connection String**

### Option B: Using Azure CLI
```powershell
# Login
az login

# Get SQL connection string
az sql db show-connection-string --client ado.net `
  --name vendormdm-db `
  --server your-sql-server

# Get Cosmos connection string
az cosmosdb keys list --type connection-strings `
  --resource-group your-rg `
  --name your-cosmos-account

# Get Service Bus connection string
az servicebus namespace authorization-rule keys list `
  --resource-group your-rg `
  --namespace-name your-servicebus `
  --name RootManageSharedAccessKey
```

---

## 📝 Update appsettings.Development.json

Edit: `backend/VendorMdm.Api/appsettings.Development.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "Sql": "Server=tcp:YOUR-SERVER.database.windows.net,1433;Initial Catalog=vendormdm-db;Persist Security Info=False;User ID=YOUR-USER;Password=YOUR-PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;",
    "Cosmos": "AccountEndpoint=https://YOUR-COSMOS.documents.azure.com:443/;AccountKey=YOUR-KEY==;",
    "ServiceBus": "Endpoint=sb://YOUR-SERVICEBUS.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YOUR-KEY="
  },
  "SapEnvironmentCode": "D01"
}
```

**Replace:**
- `YOUR-SERVER` - Your SQL server name
- `YOUR-USER` / `YOUR-PASSWORD` - SQL credentials
- `YOUR-COSMOS` - Cosmos account name
- `YOUR-SERVICEBUS` - Service Bus namespace

---

## ⚡ Even Easier: Use User Secrets (Recommended)

**Don't commit connection strings to git!**

```powershell
cd backend/VendorMdm.Api

# Initialize user secrets
dotnet user-secrets init

# Add connection strings
dotnet user-secrets set "ConnectionStrings:Sql" "YOUR-SQL-CONNECTION-STRING"
dotnet user-secrets set "ConnectionStrings:Cosmos" "YOUR-COSMOS-CONNECTION-STRING"
dotnet user-secrets set "ConnectionStrings:ServiceBus" "YOUR-SERVICEBUS-CONNECTION-STRING"
```

Then `appsettings.Development.json` stays clean - secrets are stored securely.

---

## ✅ What Will Work Locally

### With Azure SQL Connected:
- ✅ Create invitations (stored in Azure SQL)
- ✅ Validate invitation tokens
- ✅ List invitations
- ✅ Complete invitations

### With Cosmos DB Connected:
- ✅ Store invitation artifacts (audit trail)
- ✅ Emit domain events

### With Service Bus Connected:
- ✅ Queue email notifications

### Without Cosmos/Service Bus:
- ✅ Invitations still work!
- ⚠️ Warnings logged (non-blocking)
- ✅ SQL database keeps working

---

## 🚀 Start Testing (2 Commands)

```powershell
# Terminal 1
cd backend/VendorMdm.Api
dotnet run

# Terminal 2
cd frontend
npm run dev
```

**Then:** Open `http://localhost:5173/admin/invite-vendor`

---

## 🐛 If You See Errors

### "Login failed for user"
→ SQL connection string wrong or firewall blocking

**Fix:** Azure Portal → SQL Server → Firewalls and virtual networks
→ Add your IP address

### "Container InvitationArtifacts not found"
→ Cosmos container doesn't exist yet (OK - it's non-blocking)

**Fix:** Create container in Cosmos or ignore (invitation still works)

### "Service Bus error"
→ Service Bus connection wrong (OK - it's non-blocking)

**Fix:** Check connection string or ignore (invitation still works)

---

## 🎯 Expected Flow

```
1. Start backend → Connects to Azure SQL ✅
2. Start frontend → Points to localhost:5000 ✅
3.Create invitation → Saved to Azure SQL ✅
4. Cosmos warning → Logged but continues ⚠️
5. Service Bus warning → Logged but continues ⚠️
6. Invitation created successfully! ✅
```

**The key:** SQL database connection is what matters most. Cosmos/Service Bus are optional.

---

## Ready?

1. Get your Azure SQL connection string
2. Update `appsettings.Development.json` or use user secrets
3. Run `dotnet run` in backend
4. Run `npm run dev` in frontend
5. Test at `http://localhost:5173`

**Want me to help you get the connection strings from Azure?** 
Just let me know and I'll guide you! 🚀
