# 🚀 Quick Start Guide

## Current Status

✅ **Frontend**: Running on http://localhost:5173  
❌ **Backend**: Needs .NET 8 SDK installed

---

## Step 1: Install .NET 8 SDK

### Quick Install (5 minutes)

1. **Download**: https://dotnet.microsoft.com/download/dotnet/8.0
2. **Click**: "Download .NET SDK 8.0.x" for macOS
3. **Install**: Open the `.pkg` file and follow the wizard
4. **Verify**: Open a **new terminal** and run:
   ```bash
   dotnet --version
   ```
   Should show: `8.0.x`

📖 **Detailed instructions**: See `INSTALL_DOTNET_QUICK.md`

---

## Step 2: Start Backend API

Once .NET is installed, you have two options:

### Option A: Use the startup script
```bash
./start-backend.sh
```

### Option B: Manual start
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

---

## Step 3: Test the Application

1. **Frontend**: http://localhost:5173
2. **Login as Admin**: Click "Log in as Administrator"
3. **Go to**: Admin Dashboard → Invite Vendor
4. **Fill form** and click "Create Invitation"
5. **Should work!** ✅

---

## Troubleshooting

### "dotnet: command not found"
- **Solution**: Open a **new terminal** after installation
- Or add to `~/.zshrc`:
  ```bash
  export PATH="$PATH:/usr/local/share/dotnet"
  ```

### "Port 5001 already in use"
- **Solution**: Kill the process using port 5001:
  ```bash
  lsof -ti:5001 | xargs kill
  ```

### "Cannot connect to backend API"
- **Check**: Is backend running? Look for "Now listening on: http://localhost:5001"
- **Check**: Browser console (F12) for detailed error

---

## Port Configuration

- **Frontend**: http://localhost:5173
- **Backend API**: http://localhost:5001
- **Swagger UI**: http://localhost:5001/swagger

*(Port 5000 is used by Apple AirPlay on macOS, so we use 5001)*

---

## Next Steps After Backend Starts

1. ✅ Backend will use local emulators (since Azure connection strings have placeholders)
2. ✅ You can create invitations
3. ✅ Data will be stored locally (or in Azure if you configure connection strings)

---

**Ready to install .NET?** Follow Step 1 above, then run `./start-backend.sh`!

