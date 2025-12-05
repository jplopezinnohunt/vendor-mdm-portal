# 🚀 Local Testing Setup - Invitation Feature

## Quick Start (5 minutes)

### Step 1: Start Backend API

```powershell
# Navigate to API project
cd backend/VendorMdm.Api

# Restore dependencies
dotnet restore

# Run the API
dotnet run
```

**Expected output:**
```
Now listening on: http://localhost:5000
Now listening on: https://localhost:5001
Application started. Press Ctrl+C to shut down.
```

**Keep this terminal open!**

---

### Step 2: Start Frontend

```powershell
# Open NEW terminal
cd frontend

# Install dependencies (if not already)
npm install

# Start dev server
npm run dev
```

**Expected output:**
```
VITE v6.x.x  ready in xxx ms

➜  Local:   http://localhost:5173/
➜  Network: use --host to expose
```

---

### Step 3: Open Browser

Navigate to: `http://localhost:5173`

Login with mock credentials (if using mock auth)

---

## 🗄️ Database Setup (Local)

### Option A: Use In-Memory Database (Fastest)

Update `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "Sql": "InMemory",
    "ServiceBus": "UseDevelopmentStorage=true",
    "Cosmos": "AccountEndpoint=https://localhost:8081/"
  }
}
```

Modify `Program.cs` to use in-memory:

```csharp
if (builder.Configuration.GetConnectionString("Sql") == "InMemory")
{
    builder.Services.AddDbContext<SqlDbContext>(options =>
        options.UseInMemoryDatabase("VendorMdmDev"));
}
else
{
    builder.Services.AddDbContext<SqlDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("Sql")));
}
```

### Option B: Use LocalDB (SQL Server)

```powershell
# Create database
sqllocaldb create MSSQLLocalDB
sqllocaldb start MSSQLLocalDB

# Connection string in appsettings.Development.json:
{
  "ConnectionStrings": {
    "Sql": "Server=(localdb)\\MSSQLLocalDB;Database=VendorMdm;Trusted_Connection=True;"
  }
}
```

Then run migrations:
```powershell
cd backend/VendorMdm.Api
dotnet ef migrations add AddVendorInvitations
dotnet ef database update
```

### Option C: Use Azure SQL (Remote)

Use existing Azure SQL connection string.

---

## 🌐 Cosmos DB (Optional for Now)

### Option A: Skip Cosmos (Fastest)

Cosmos calls will fail gracefully (logged but not blocking).

### Option B: Cosmos DB Emulator

```powershell
# Download and install Cosmos DB Emulator
# https://aka.ms/cosmosdb-emulator

# Connection string:
{
  "ConnectionStrings": {
    "Cosmos": "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="
  }
}
```

Create container manually in emulator.

### Option C: Use Azure Cosmos (Remote)

Use existing Azure Cosmos connection string.

---

## 🚌 Service Bus (Optional)

### Mock Service Bus (Recommended for Local)

Create a mock implementation:

```csharp
// In Program.cs - for development only
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<ServiceBusService, MockServiceBusService>();
}
else
{
    builder.Services.AddScoped<ServiceBusService>();
}

// MockServiceBusService.cs
public class MockServiceBusService : ServiceBusService
{
    private readonly ILogger<MockServiceBusService> _logger;

    public MockServiceBusService(ILogger<MockServiceBusService> logger)
    {
        _logger = logger;
    }

    public override Task PublishEventAsync(string eventType, object data, string? queueName = null)
    {
        _logger.LogInformation("MOCK: Would publish event {EventType} to queue {Queue}", 
            eventType, queueName ?? "default");
        _logger.LogInformation("MOCK: Data: {Data}", JsonSerializer.Serialize(data));
        return Task.CompletedTask;
    }
}
```

---

## ✅ Minimal Setup (Start in 2 minutes)

**Just to test invitation UI:**

1. **Backend with In-Memory DB:**
```powershell
cd backend/VendorMdm.Api

# Edit Program.cs - add this before builder.Build():
builder.Services.AddDbContext<SqlDbContext>(options =>
    options.UseInMemoryDatabase("VendorMdmDev"));

dotnet run
```

2. **Frontend:**
```powershell
cd frontend
npm run dev
```

3. **Open:** `http://localhost:5173/admin/invite-vendor`

**Cosmos/Service Bus errors are OK - they'll be logged but won't break the flow.**

---

## 🐛 Testing the Flow

### 1. Create Invitation
- Navigate to `http://localhost:5173/admin/invite-vendor`
- Fill form and submit
- Check backend terminal for logs

### 2. Check Response
```
POST /api/invitation/create
Status: 200 OK

Response:
{
  "invitationId": "...",
  "invitationToken": "...",
  "invitationLink": "/invitation/register/abc123xyz",
  "expiresAt": "2025-12-19T..."
}
```

### 3. Test Invitation Link
- Copy the token from response
- Navigate to: `http://localhost:5173/invitation/register/{token}`
- Should show registration form with pre-filled data

---

## 🔍 Debugging Tips

### Check Backend Logs
Watch the terminal where `dotnet run` is running for:
```
Invitation created: {InvitationId} for {Email} by {InvitedBy}
Invitation artifact stored in Cosmos for {InvitationId}  <-- May fail (OK)
Domain event InvitationCreated emitted for {InvitationId}  <-- May fail (OK)
Invitation email queued for {Email}  <-- May fail (OK)
```

### Check Frontend Network Tab
1. Open DevTools (F12)
2. Network tab
3. Click "Create Invitation"
4. Look for `/api/invitation/create` request
5. Check response body

### Check Browser Console
Look for any JavaScript errors.

---

## 🎯 Expected Behavior

**Success Flow:**
```
1. Fill form
2. Click "Create Invitation"
3. ✅ Success page shows
4. ✅ Invitation link displayed
5. ✅ Can copy link
6. ✅ Link opens registration page
```

**With Cosmos/Service Bus Not Configured:**
```
1. Fill form
2. Click "Create Invitation"
3. ✅ Success page shows (invitation created in SQL)
4. ⚠️ Backend logs Cosmos errors (non-blocking)
5. ⚠️ Backend logs Service Bus errors (non-blocking)
6. ✅ Link still works
```

---

## 📝 Configuration Files

### `appsettings.Development.json` (Minimal)
```json
{
  "ConnectionStrings": {
    "Sql": "InMemory"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

### `frontend/.env.local`
```
VITE_API_BASE_URL=http://localhost:5000/api
```

---

## Ready to Test?

Run these 2 commands in separate terminals:

```powershell
# Terminal 1
cd backend/VendorMdm.Api
dotnet run

# Terminal 2  
cd frontend
npm run dev
```

Then open `http://localhost:5173` and try creating an invitation!

Let me know if you see any errors and I'll help debug! 🚀
