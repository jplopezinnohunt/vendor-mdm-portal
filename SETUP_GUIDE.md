# 🚀 Setup Guide - Vendor MDM Portal

## Current Status & Missing Components

### ✅ What's Already Configured
- Frontend React app with TypeScript, Vite, and TailwindCSS
- Backend ASP.NET Core 8 API with controllers and services
- Infrastructure as Code (Bicep files for Azure resources)
- All npm dependencies installed
- Routing and authentication context configured

### ❌ What's Missing to Run the App

## 1. Frontend Setup

### Navigation Issue - FIXED ✓
The app uses `HashRouter`, so URLs should include `#`:
- ✅ Correct: `http://localhost:5173/#/login`
- ❌ Wrong: `http://localhost:5173/login`

### To Run Frontend:
```bash
cd c:\Users\jp_lopez\.gemini\antigravity\scratch\vendor-mdm-portal
npm run dev
```

The app will start at: **http://localhost:5173**
Login page: **http://localhost:5173/#/login**

### Optional: Create .env.local (gitignored)
Create manually if needed:
```env
VITE_API_BASE_URL=http://localhost:5000
VITE_ENVIRONMENT=development
```

---

## 2. Backend Setup

### Prerequisites for Local Development

#### Option A: Full Local Development (Recommended for Testing)
You need to install:

1. **SQL Server LocalDB** (for database)
   - Included with Visual Studio
   - Or install: [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)

2. **Azure Cosmos DB Emulator** (for document storage)
   - Download: [Cosmos DB Emulator](https://aka.ms/cosmosdb-emulator)
   - Default endpoint: `https://localhost:8081/`

3. **Azure Service Bus Emulator** (optional - can mock)
   - Or use in-memory queue for local testing

#### Option B: Connect to Azure Resources
Use the existing `appsettings.json` configuration with:
- Azure SQL Database
- Azure Cosmos DB
- Azure Service Bus

You'll need to authenticate with:
```bash
az login
```

### Configuration Files

✅ **Created**: `appsettings.Development.json` with local emulator settings

### To Run Backend:
```bash
cd c:\Users\jp_lopez\.gemini\antigravity\scratch\vendor-mdm-portal\backend\VendorMdm.Api
dotnet restore
dotnet ef database update  # Create/update database
dotnet run
```

The API will start at: **http://localhost:5000** (or https://localhost:5001)

---

## 3. Database Setup

### Create Initial Database
```bash
cd c:\Users\jp_lopez\.gemini\antigravity\scratch\vendor-mdm-portal\backend\VendorMdm.Api

# Create migration (if not exists)
dotnet ef migrations add InitialCreate

# Apply migration to database
dotnet ef database update
```

---

## 4. Quick Start (Frontend Only - No Backend)

The frontend can run **standalone** for UI development:

```bash
npm run dev
```

Features that work without backend:
- ✅ Login page navigation
- ✅ Mock authentication (uses localStorage)
- ✅ All UI components and routing
- ✅ Role-based navigation (Vendor, Approver, Admin)
- ❌ API calls will fail (but UI will still render)

---

## 5. Full Stack Development

### Terminal 1 - Backend:
```bash
cd backend\VendorMdm.Api
dotnet run
```

### Terminal 2 - Frontend:
```bash
npm run dev
```

### Access the App:
- Frontend: http://localhost:5173/#/login
- Backend API: http://localhost:5000/swagger (Swagger UI)

---

## 6. Testing the App

### Login Options (Mock Authentication):
1. **Vendor User**: Click "Access Portal" → redirects to `/profile`
2. **Approver User**: Click "Log in as Approver" → redirects to `/approver/worklist`
3. **Admin User**: Click "Log in as Administrator" → redirects to `/admin/dashboard`

### Available Routes:
- `/#/login` - Login page
- `/#/register` - Vendor registration
- `/#/profile` - Vendor profile (Vendor role)
- `/#/dashboard` - Vendor dashboard (Vendor role)
- `/#/requests` - Request history (Vendor role)
- `/#/approver/worklist` - Approver worklist (Approver/Admin role)
- `/#/admin/dashboard` - Admin dashboard (Admin role)

---

## 7. Common Issues & Solutions

### Issue: "Cannot find module" errors
**Solution**: 
```bash
npm install
```

### Issue: Backend won't start - Database connection error
**Solution**: 
- Install SQL Server LocalDB
- Or update `appsettings.Development.json` with your SQL connection string

### Issue: Navigation doesn't work / blank page
**Solution**: 
- Make sure you're using the `#` in URLs: `http://localhost:5173/#/login`
- Check browser console for errors
- Clear localStorage: `localStorage.clear()`

### Issue: CORS errors when calling API
**Solution**: 
- Backend `Program.cs` already has CORS configured for `http://localhost:5173`
- Make sure backend is running

---

## 8. Deployment to Azure

### Prerequisites:
- Azure subscription
- Azure CLI installed: `az --version`

### Deploy Infrastructure:
```bash
cd infrastructure
az login
az deployment sub create --location westeurope --template-file main.bicep --parameters environmentName=dev
```

### Deploy Frontend (Azure Static Web Apps):
- Push to `main` branch
- GitHub Actions will automatically deploy

### Deploy Backend (Azure Container Apps):
```bash
cd backend/VendorMdm.Api
az containerapp up --name vendormdm-api --resource-group rg-vendormdm-dev
```

---

## 9. Next Steps

### To make the app fully functional:
1. ✅ Run frontend: `npm run dev`
2. ⚠️ Install SQL LocalDB or Cosmos Emulator (for backend)
3. ⚠️ Run backend: `dotnet run` (optional for UI testing)
4. ⚠️ Create database: `dotnet ef database update`
5. ✅ Access app: http://localhost:5173/#/login

### For production deployment:
1. ⚠️ Deploy Azure infrastructure (Bicep files)
2. ⚠️ Configure GitHub secrets for CI/CD
3. ⚠️ Push to main branch to trigger deployment

---

## Summary

**To run the app RIGHT NOW (Frontend only):**
```bash
npm run dev
```
Then open: **http://localhost:5173/#/login**

**To run full stack:**
1. Install SQL LocalDB or Cosmos Emulator
2. Run backend: `cd backend\VendorMdm.Api && dotnet run`
3. Run frontend: `npm run dev`
4. Open: **http://localhost:5173/#/login**

---

Built with ❤️ for vendor master data management
