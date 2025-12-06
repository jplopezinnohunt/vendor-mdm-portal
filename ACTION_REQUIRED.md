# ⚠️ ACTION REQUIRED: Install .NET SDK

## The Problem
Your backend API cannot start because **.NET 8 SDK is not installed** on your Mac.

## The Solution (5 minutes)

### Step 1: Download .NET SDK
👉 **Click this link**: https://dotnet.microsoft.com/download/dotnet/8.0

### Step 2: Install
1. Click the **"Download .NET SDK 8.0.x"** button (macOS version)
2. Wait for download to complete
3. Open the downloaded `.pkg` file
4. Follow the installation wizard (click "Continue", "Install", etc.)
5. Enter your password when prompted
6. Wait for installation to complete

### Step 3: Verify Installation
Open a **NEW terminal window** (important!) and run:
```bash
dotnet --version
```

**Expected output**: `8.0.xxx` (some version number)

If you see "command not found", you may need to:
- Close and reopen your terminal
- Or add to `~/.zshrc`:
  ```bash
  export PATH="$PATH:/usr/local/share/dotnet"
  ```

### Step 4: Start Backend
Once `.NET` is installed, run:
```bash
cd /Users/jplopez/projects/vendor-mdm-portal
./start-backend.sh
```

You should see:
```
✅ .NET SDK found: 8.0.xxx
📦 Restoring dependencies...
🌐 Starting API server on http://localhost:5001
Now listening on: http://localhost:5001
```

### Step 5: Test
1. Keep the backend terminal running
2. Refresh your browser (`Cmd + Shift + R`)
3. Try creating an invitation again
4. Should work! ✅

---

## Why This Is Needed

- Your **frontend** (React) is already running ✅
- Your **backend** (ASP.NET Core) needs .NET SDK to run ❌
- Without .NET SDK, the backend cannot start
- Without the backend, API calls will fail

---

## Quick Checklist

- [ ] Downloaded .NET SDK from https://dotnet.microsoft.com/download/dotnet/8.0
- [ ] Installed the .pkg file
- [ ] Verified with `dotnet --version` in a new terminal
- [ ] Ran `./start-backend.sh`
- [ ] Backend shows "Now listening on: http://localhost:5001"
- [ ] Refreshed browser and tested invitation creation

---

**Once .NET is installed, everything will work!** 🚀

