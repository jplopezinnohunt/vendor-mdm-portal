# 🚀 Start Backend - Quick Steps

## Current Status
- ✅ Frontend: Running on http://localhost:5173
- ✅ Configuration: Updated to port 5001
- ❌ Backend: **NOT RUNNING** (needs .NET SDK)

---

## Step 1: Install .NET 8 SDK

### Quick Install (5 minutes)

1. **Open this link**: https://dotnet.microsoft.com/download/dotnet/8.0
2. **Click**: "Download .NET SDK 8.0.x" button (for macOS)
3. **Install**: 
   - Open the downloaded `.pkg` file
   - Click through the installation wizard
   - Wait for installation to complete
4. **Verify**: Open a **NEW terminal window** and run:
   ```bash
   dotnet --version
   ```
   Should show: `8.0.x` or similar

---

## Step 2: Start Backend

Once .NET is installed, run this command:

```bash
cd /Users/jplopez/projects/vendor-mdm-portal
./start-backend.sh
```

**OR manually:**
```bash
cd backend/VendorMdm.Api
dotnet restore
dotnet run
```

**Expected output:**
```
Now listening on: http://localhost:5001
Swagger UI: http://localhost:5001/swagger
```

**Keep this terminal open!** The backend needs to keep running.

---

## Step 3: Test the Application

1. **Refresh browser**: Hard refresh with `Cmd + Shift + R`
2. **Go to**: http://localhost:5173/admin/invite-vendor
3. **Fill the form** and click "Create Invitation"
4. **Should work!** ✅

---

## Troubleshooting

### "dotnet: command not found" after installation
- **Solution**: Open a **NEW terminal window** (to refresh PATH)
- Or add to `~/.zshrc`:
  ```bash
  export PATH="$PATH:/usr/local/share/dotnet"
  export DOTNET_ROOT="/usr/local/share/dotnet"
  ```
- Then run: `source ~/.zshrc`

### "Port 5001 already in use"
- **Solution**: Kill the process:
  ```bash
  lsof -ti:5001 | xargs kill
  ```

### Backend starts but shows errors
- Check the terminal output for specific error messages
- Common issues: Database connection, missing dependencies
- The app will use local emulators if Azure connection strings have placeholders

---

## What Happens After Backend Starts

1. ✅ Backend API will be available on http://localhost:5001
2. ✅ Swagger UI will be available on http://localhost:5001/swagger
3. ✅ Frontend can connect and create invitations
4. ✅ Data will be stored (locally or in Azure, depending on configuration)

---

## Quick Reference

- **Frontend**: http://localhost:5173
- **Backend API**: http://localhost:5001
- **Swagger UI**: http://localhost:5001/swagger
- **API Endpoint**: http://localhost:5001/api/invitation/create

---

**Once .NET is installed, run `./start-backend.sh` and you're good to go!** 🚀

