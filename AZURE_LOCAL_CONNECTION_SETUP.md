# 🔗 Connect Local Development to Azure Resources

## Overview
This guide helps you configure your local development environment to connect to existing Azure resources instead of using local emulators.

## Azure Resources (Already Created)

Based on your configuration, these resources exist:
- **SQL Database**: `mdmportal-sql-12031241-dev.database.windows.net` / `mdmportal-sqldb-dev`
- **Cosmos DB**: `mdmportal-cosmos-dev.documents.azure.com`
- **Service Bus**: `mdmportal-sb-dev.servicebus.windows.net`

---

## Step 1: Get Connection Strings from Azure Portal

### SQL Database Connection String

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to: **SQL databases** → `mdmportal-sqldb-dev`
3. Click **Connection strings** in the left menu
4. Select **ADO.NET** tab
5. Copy the connection string
6. Replace placeholders:
   - `{your_username}` → Your SQL admin username
   - `{your_password}` → Your SQL admin password

**Example format:**
```
Server=tcp:mdmportal-sql-12031241-dev.database.windows.net,1433;Initial Catalog=mdmportal-sqldb-dev;Persist Security Info=False;User ID=YOUR_USERNAME;Password=YOUR_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

**Alternative: Use SQL Authentication with Active Directory**
If you have Azure AD authentication set up:
```
Server=tcp:mdmportal-sql-12031241-dev.database.windows.net,1433;Initial Catalog=mdmportal-sqldb-dev;Authentication=Active Directory Password;User ID=your-email@domain.com;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=False;
```

### Cosmos DB Connection String

1. Go to **Azure Cosmos DB** → `mdmportal-cosmos-dev`
2. Click **Keys** in the left menu
3. Copy the **Primary Connection String**

**Example format:**
```
AccountEndpoint=https://mdmportal-cosmos-dev.documents.azure.com:443/;AccountKey=YOUR_PRIMARY_KEY==;
```

### Service Bus Connection String

1. Go to **Service Bus namespaces** → `mdmportal-sb-dev`
2. Click **Shared access policies** in the left menu
3. Click **RootManageSharedAccessKey** (or create a new policy)
4. Copy the **Primary Connection String**

**Example format:**
```
Endpoint=sb://mdmportal-sb-dev.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YOUR_SHARED_ACCESS_KEY==;
```

---

## Step 2: Update Configuration Files

### Option A: Use User Secrets (Recommended - Secure)

User secrets are stored outside the project and won't be committed to git.

```bash
cd backend/VendorMdm.Api

# Initialize user secrets (if not already done)
dotnet user-secrets init

# Add connection strings
dotnet user-secrets set "ConnectionStrings:Sql" "YOUR_SQL_CONNECTION_STRING"
dotnet user-secrets set "ConnectionStrings:Cosmos" "YOUR_COSMOS_CONNECTION_STRING"
dotnet user-secrets set "ConnectionStrings:ServiceBus" "YOUR_SERVICEBUS_CONNECTION_STRING"
```

Then set `UseLocalEmulators` to `false` in `appsettings.Development.json`:
```json
{
  "UseLocalEmulators": false
}
```

### Option B: Update appsettings.Development.json Directly

⚠️ **Warning**: This file is gitignored, but be careful not to commit secrets.

Edit `backend/VendorMdm.Api/appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "Sql": "Server=tcp:mdmportal-sql-12031241-dev.database.windows.net,1433;Initial Catalog=mdmportal-sqldb-dev;Persist Security Info=False;User ID=YOUR_USERNAME;Password=YOUR_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;",
    "Cosmos": "AccountEndpoint=https://mdmportal-cosmos-dev.documents.azure.com:443/;AccountKey=YOUR_COSMOS_KEY==;",
    "ServiceBus": "Endpoint=sb://mdmportal-sb-dev.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YOUR_SERVICEBUS_KEY==;"
  },
  "SapEnvironmentCode": "D01",
  "UseLocalEmulators": false
}
```

Replace:
- `YOUR_USERNAME` → SQL admin username
- `YOUR_PASSWORD` → SQL admin password
- `YOUR_COSMOS_KEY==` → Cosmos DB primary key
- `YOUR_SERVICEBUS_KEY==` → Service Bus shared access key

---

## Step 3: Verify Azure Resource Access

### Check SQL Database Access

1. **Firewall Rules**: Ensure your IP is allowed
   - Azure Portal → SQL Server → **Networking**
   - Add your current IP address to firewall rules
   - Or enable "Allow Azure services and resources to access this server"

2. **Authentication**: Verify you have the correct credentials

### Check Cosmos DB Access

1. **Firewall**: Cosmos DB should allow your IP
   - Azure Portal → Cosmos DB → **Networking**
   - Add your IP or allow all Azure IPs

### Check Service Bus Access

1. **Firewall**: Service Bus should allow your IP
   - Azure Portal → Service Bus → **Networking**
   - Add your IP or allow all Azure IPs

---

## Step 4: Test the Connection

### Test Backend Connection

```bash
cd backend/VendorMdm.Api
dotnet restore
dotnet run
```

**Expected Output:**
```
☁️ Using Azure Resources.
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
      Now listening on: https://localhost:5001
```

**If you see errors:**
- **SQL Connection Error**: Check firewall rules and credentials
- **Cosmos Connection Error**: Verify the connection string format and firewall
- **Service Bus Error**: Check connection string and firewall

---

## Step 5: Run Full Stack Locally

### Terminal 1: Backend API
```bash
cd backend/VendorMdm.Api
dotnet run
```

### Terminal 2: Frontend
```bash
cd frontend
npm install  # If not already done
npm run dev
```

### Access the Application
- **Frontend**: http://localhost:5173
- **Backend API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger

---

## Troubleshooting

### Issue: "Cannot open server 'mdmportal-sql-...' requested by the login"

**Solution**: 
1. Check SQL Server firewall rules in Azure Portal
2. Add your current IP address
3. Verify SQL authentication credentials

### Issue: "The remote name could not be resolved"

**Solution**:
1. Check internet connection
2. Verify resource names are correct
3. Check if resources exist in Azure Portal

### Issue: "Unauthorized" or "Forbidden" errors

**Solution**:
1. Verify connection strings are correct
2. Check if keys/credentials have expired
3. Regenerate keys in Azure Portal if needed

### Issue: Cosmos DB "Unauthorized" error

**Solution**:
1. Verify the AccountKey in connection string
2. Check if the key was regenerated (regenerate if needed)
3. Ensure connection string format is correct

---

## Security Best Practices

1. ✅ **Use User Secrets** for local development (not committed to git)
2. ✅ **Never commit** connection strings to git
3. ✅ **Rotate keys** regularly in production
4. ✅ **Use Managed Identity** in Azure (already configured in appsettings.json)
5. ✅ **Limit firewall rules** to specific IPs when possible

---

## Next Steps

Once connected:
1. Test API endpoints via Swagger UI
2. Test invitation creation flow
3. Verify data is being stored in Azure SQL
4. Check Service Bus messages are being queued
5. Verify Cosmos DB documents are being created

---

## Quick Reference: Resource Names

| Resource Type | Resource Name |
|--------------|---------------|
| SQL Server | `mdmportal-sql-12031241-dev` |
| SQL Database | `mdmportal-sqldb-dev` |
| Cosmos DB Account | `mdmportal-cosmos-dev` |
| Service Bus Namespace | `mdmportal-sb-dev` |

---

*Last Updated: 2025-01-27*

