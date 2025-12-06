# 🚀 Setup and Run Project Locally with Azure Resources

## Prerequisites Check

Before running the project, ensure you have:

### Required Tools
- [ ] **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- [ ] **Node.js 18+** - [Download](https://nodejs.org/)
- [ ] **npm** (comes with Node.js)

### Verify Installation
```bash
dotnet --version    # Should show 8.x.x
node --version      # Should show 18.x.x or higher
npm --version       # Should show 9.x.x or higher
```

---

## Step 1: Get Azure Connection Strings

Your Azure resources are already created:
- **SQL**: `mdmportal-sql-12031241-dev.database.windows.net`
- **Cosmos**: `mdmportal-cosmos-dev.documents.azure.com`
- **Service Bus**: `mdmportal-sb-dev.servicebus.windows.net`

### Get Connection Strings from Azure Portal

#### SQL Database
1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to: **SQL databases** → `mdmportal-sqldb-dev`
3. Click **Connection strings** → **ADO.NET**
4. Copy the connection string
5. Replace `{your_username}` and `{your_password}` with actual credentials

#### Cosmos DB
1. Go to **Azure Cosmos DB** → `mdmportal-cosmos-dev`
2. Click **Keys** → Copy **Primary Connection String**

#### Service Bus
1. Go to **Service Bus namespaces** → `mdmportal-sb-dev`
2. Click **Shared access policies** → **RootManageSharedAccessKey**
3. Copy **Primary Connection String**

---

## Step 2: Configure Connection Strings

### Option A: User Secrets (Recommended)

```bash
cd backend/VendorMdm.Api

# Initialize user secrets
dotnet user-secrets init

# Add connection strings
dotnet user-secrets set "ConnectionStrings:Sql" "YOUR_SQL_CONNECTION_STRING"
dotnet user-secrets set "ConnectionStrings:Cosmos" "YOUR_COSMOS_CONNECTION_STRING"
dotnet user-secrets set "ConnectionStrings:ServiceBus" "YOUR_SERVICEBUS_CONNECTION_STRING"
```

### Option B: Update appsettings.Development.json

Edit `backend/VendorMdm.Api/appsettings.Development.json`:

Replace the placeholders:
- `YOUR_SQL_USERNAME` → Your SQL admin username
- `YOUR_SQL_PASSWORD` → Your SQL admin password
- `YOUR_COSMOS_KEY` → Cosmos DB primary key
- `YOUR_SERVICE_BUS_KEY` → Service Bus shared access key

Set `UseLocalEmulators` to `false`:
```json
{
  "UseLocalEmulators": false
}
```

---

## Step 3: Configure Azure Firewall Rules

### SQL Database Firewall
1. Azure Portal → SQL Server → **Networking**
2. Add your current IP address
3. Or enable "Allow Azure services and resources"

### Cosmos DB Firewall
1. Azure Portal → Cosmos DB → **Networking**
2. Add your IP or allow all Azure IPs

### Service Bus Firewall
1. Azure Portal → Service Bus → **Networking**
2. Add your IP or allow all Azure IPs

---

## Step 4: Run Backend API

```bash
cd backend/VendorMdm.Api

# Restore dependencies
dotnet restore

# Run the API
dotnet run
```

**Expected Output:**
```
☁️ Using Azure Resources.
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
      Now listening on: https://localhost:5001
```

**If you see:**
```
⚠️ Azure connection strings contain placeholders. Falling back to Local Emulators.
🔧 Using Local Emulators for development.
```
This means connection strings still have placeholders. Update them using Option A or B above.

**Access Swagger UI:** http://localhost:5000/swagger

---

## Step 5: Run Frontend

Open a **new terminal**:

```bash
cd frontend

# Install dependencies (first time only)
npm install

# Run development server
npm run dev
```

**Expected Output:**
```
VITE v6.x.x  ready in xxx ms

➜  Local:   http://localhost:5173/
```

**Access Frontend:** http://localhost:5173

---

## Step 6: Test the Application

### Test Backend
1. Open http://localhost:5000/swagger
2. Try the `/api/invitation/create` endpoint
3. Verify responses

### Test Frontend
1. Open http://localhost:5173
2. Navigate to login page
3. Test the invitation flow

---

## Troubleshooting

### Backend Issues

#### "Cannot open server" (SQL)
- **Solution**: Check firewall rules in Azure Portal
- Add your IP to SQL Server firewall

#### "Unauthorized" (Cosmos/Service Bus)
- **Solution**: Verify connection strings are correct
- Regenerate keys in Azure Portal if needed

#### "Connection timeout"
- **Solution**: Check internet connection
- Verify resource names are correct

### Frontend Issues

#### "Cannot connect to API"
- **Solution**: Ensure backend is running on port 5000
- Check CORS settings in `Program.cs`

#### "Module not found"
- **Solution**: Run `npm install` in frontend directory

---

## Quick Reference

### Resource Names
| Resource | Name |
|----------|------|
| SQL Server | `mdmportal-sql-12031241-dev` |
| SQL Database | `mdmportal-sqldb-dev` |
| Cosmos DB | `mdmportal-cosmos-dev` |
| Service Bus | `mdmportal-sb-dev` |

### Ports
- **Backend API**: http://localhost:5000
- **Backend HTTPS**: https://localhost:5001
- **Frontend**: http://localhost:5173

### Important Files
- `backend/VendorMdm.Api/appsettings.Development.json` - Local config
- `backend/VendorMdm.Api/Program.cs` - Connection logic
- `frontend/vite.config.ts` - Frontend config

---

## Next Steps

Once running:
1. ✅ Test invitation creation
2. ✅ Verify data in Azure SQL
3. ✅ Check Service Bus messages
4. ✅ Verify Cosmos DB documents

---

## Security Notes

⚠️ **Important:**
- Never commit connection strings to git
- Use User Secrets for local development
- Rotate keys regularly in production
- The `.gitignore` already excludes `appsettings.Development.json`

---

*For detailed Azure connection setup, see: [AZURE_LOCAL_CONNECTION_SETUP.md](./AZURE_LOCAL_CONNECTION_SETUP.md)*

