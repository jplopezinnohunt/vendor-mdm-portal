# 🔧 Azure Local Development Setup Guide

This guide helps you configure your local development environment to connect to existing Azure resources.

## Prerequisites

1. **Azure CLI** installed and configured
2. **Access** to the Azure subscription with the resources
3. **SQL Server Admin credentials** (for SQL authentication)

## Step 1: Login to Azure

```bash
az login
```

This will open a browser for authentication. After login, verify:

```bash
az account show
```

## Step 2: Get Azure Resource Information

From your `appsettings.json`, I can see these resources exist:
- **SQL Server**: `mdmportal-sql-12031241-dev.database.windows.net`
- **SQL Database**: `mdmportal-sqldb-dev`
- **Cosmos DB**: `mdmportal-cosmos-dev.documents.azure.com`
- **Service Bus**: `mdmportal-sb-dev.servicebus.windows.net`

Let's get the connection strings:

### Option A: Get Connection Strings via Azure CLI

```bash
# Set your resource group (update if different)
RESOURCE_GROUP="your-resource-group-name"

# Get SQL Connection String (requires SQL auth credentials)
# Note: You'll need to provide SQL username and password
az sql db show-connection-string \
  --server mdmportal-sql-12031241-dev \
  --name mdmportal-sqldb-dev \
  --client ado.net

# Get Cosmos DB Connection String
az cosmosdb keys list \
  --resource-group $RESOURCE_GROUP \
  --name mdmportal-cosmos-dev \
  --type connection-strings \
  --query "connectionStrings[0].connectionString" -o tsv

# Get Service Bus Connection String
az servicebus namespace authorization-rule keys list \
  --resource-group $RESOURCE_GROUP \
  --namespace-name mdmportal-sb-dev \
  --name RootManageSharedAccessKey \
  --query "primaryConnectionString" -o tsv
```

### Option B: Get from Azure Portal

1. **SQL Database Connection String:**
   - Go to Azure Portal → SQL Database → `mdmportal-sqldb-dev`
   - Click "Connection strings" in left menu
   - Copy the **ADO.NET** connection string
   - **Important**: Replace `{your_username}` and `{your_password}` with actual SQL admin credentials

2. **Cosmos DB Connection String:**
   - Go to Azure Portal → Cosmos DB Account → `mdmportal-cosmos-dev`
   - Click "Keys" in left menu
   - Copy the **Primary Connection String**

3. **Service Bus Connection String:**
   - Go to Azure Portal → Service Bus Namespace → `mdmportal-sb-dev`
   - Click "Shared access policies" → `RootManageSharedAccessKey`
   - Copy the **Primary Connection String**

## Step 3: Configure Local Development

### Option A: Use User Secrets (Recommended - Secure)

User secrets are stored outside the project and won't be committed to git:

```bash
cd backend/VendorMdm.Api

# Initialize user secrets (if not already done)
dotnet user-secrets init

# Add SQL connection string (replace with your actual connection string)
dotnet user-secrets set "ConnectionStrings:Sql" "Server=tcp:mdmportal-sql-12031241-dev.database.windows.net,1433;Initial Catalog=mdmportal-sqldb-dev;Persist Security Info=False;User ID=YOUR_SQL_USERNAME;Password=YOUR_SQL_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# Add Cosmos connection string
dotnet user-secrets set "ConnectionStrings:Cosmos" "AccountEndpoint=https://mdmportal-cosmos-dev.documents.azure.com:443/;AccountKey=YOUR_COSMOS_KEY;"

# Add Service Bus connection string
dotnet user-secrets set "ConnectionStrings:ServiceBus" "Endpoint=sb://mdmportal-sb-dev.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YOUR_SERVICE_BUS_KEY;"
```

### Option B: Update appsettings.Development.json

**⚠️ Warning**: This file is gitignored, but be careful not to commit secrets.

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
    "Sql": "Server=tcp:mdmportal-sql-12031241-dev.database.windows.net,1433;Initial Catalog=mdmportal-sqldb-dev;Persist Security Info=False;User ID=YOUR_SQL_USERNAME;Password=YOUR_SQL_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;",
    "Cosmos": "AccountEndpoint=https://mdmportal-cosmos-dev.documents.azure.com:443/;AccountKey=YOUR_COSMOS_KEY;",
    "ServiceBus": "Endpoint=sb://mdmportal-sb-dev.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YOUR_SERVICE_BUS_KEY;"
  },
  "SapEnvironmentCode": "D01",
  "UseLocalEmulators": false
}
```

**Replace:**
- `YOUR_SQL_USERNAME` - SQL Server admin username
- `YOUR_SQL_PASSWORD` - SQL Server admin password
- `YOUR_COSMOS_KEY` - Cosmos DB primary key
- `YOUR_SERVICE_BUS_KEY` - Service Bus shared access key

## Step 4: Configure SQL Server Firewall

Your local IP needs to be allowed to connect to Azure SQL:

### Option A: Add Your IP via Azure CLI

```bash
# Get your public IP
MY_IP=$(curl -s https://api.ipify.org)

# Add firewall rule (update resource group and server name)
az sql server firewall-rule create \
  --resource-group YOUR_RESOURCE_GROUP \
  --server mdmportal-sql-12031241-dev \
  --name "LocalDev-$(date +%s)" \
  --start-ip-address $MY_IP \
  --end-ip-address $MY_IP
```

### Option B: Add via Azure Portal

1. Go to Azure Portal → SQL Server → `mdmportal-sql-12031241-dev`
2. Click "Networking" in left menu
3. Under "Public network access", click "Add your client IPv4 address"
4. Click "Save"

## Step 5: Verify Configuration

### Test SQL Connection

```bash
cd backend/VendorMdm.Api
dotnet ef database update --dry-run
```

If this works, your SQL connection is configured correctly.

### Test Azure CLI Authentication (for Managed Identity fallback)

```bash
az account show
az account list --output table
```

## Step 6: Run the Application

### Terminal 1: Backend API

```bash
cd backend/VendorMdm.Api
dotnet restore
dotnet run
```

**Expected output:**
```
☁️ Using Azure Resources.
Now listening on: http://localhost:5000
Now listening on: https://localhost:5001
Application started. Press Ctrl+C to shut down.
```

### Terminal 2: Frontend

```bash
cd frontend
npm install  # If not already done
npm run dev
```

**Expected output:**
```
VITE v6.x.x  ready in xxx ms
➜  Local:   http://localhost:5173/
```

## Troubleshooting

### "Login failed for user"
- **Cause**: SQL authentication credentials are wrong
- **Fix**: Verify SQL username and password in connection string

### "Cannot open server 'mdmportal-sql-12031241-dev'"
- **Cause**: Firewall blocking your IP
- **Fix**: Add your IP to SQL Server firewall rules (see Step 4)

### "The remote certificate is invalid"
- **Cause**: SSL certificate validation issue
- **Fix**: Ensure connection string has `Encrypt=True;TrustServerCertificate=False;`

### "Container not found" (Cosmos DB)
- **Cause**: Container doesn't exist yet
- **Fix**: This is non-blocking. The application will create containers on first use, or you can create them manually in Azure Portal

### "Service Bus connection failed"
- **Cause**: Connection string or key is wrong
- **Fix**: Verify Service Bus connection string and shared access key
- **Note**: This is non-blocking - invitations will still work without Service Bus

## Authentication Methods

The application supports two authentication methods:

1. **Connection String with Keys** (Recommended for local dev)
   - SQL: Username/Password in connection string
   - Cosmos: AccountKey in connection string
   - Service Bus: SharedAccessKey in connection string

2. **Azure Managed Identity** (For deployed apps)
   - Requires `az login` to be active
   - Uses `DefaultAzureCredential()` from Azure.Identity
   - Works automatically when deployed to Azure

## Next Steps

Once connected:
1. ✅ Test API endpoints at `http://localhost:5000/swagger`
2. ✅ Test frontend at `http://localhost:5173`
3. ✅ Create an invitation via `/admin/invite-vendor`
4. ✅ Verify data appears in Azure SQL Database

---

**Need Help?** Check the logs in the backend terminal for detailed error messages.

