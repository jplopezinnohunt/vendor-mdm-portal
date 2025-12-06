# 🔧 Troubleshooting "Failed to Create Invitation" Error

## Issue
When clicking "Create Invitation" in the Invite Vendor form, you see:
```
Error: Failed to create invitation
```

## Root Cause
The backend API is not running or not accessible.

## ✅ Solution

### Step 1: Check if Backend is Running

The backend API should be running on **http://localhost:5000**

**Check in terminal:**
```bash
curl http://localhost:5000/api/invitation/list
```

If you get a connection error, the backend is not running.

### Step 2: Start the Backend API

**Prerequisites:**
- .NET 8 SDK must be installed
- Check: `dotnet --version` (should show 8.0.x)

**Start the backend:**
```bash
cd backend/VendorMdm.Api
dotnet restore
dotnet run
```

**Expected output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
      Now listening on: https://localhost:5001
```

### Step 3: Verify API is Accessible

Once backend is running, test the endpoint:
```bash
curl http://localhost:5000/swagger
```

Or open in browser: http://localhost:5000/swagger

### Step 4: Try Creating Invitation Again

1. Go to: http://localhost:5173/admin/invite-vendor
2. Fill in the form
3. Click "Create Invitation"
4. Should now work! ✅

---

## Common Issues

### Issue 1: "Cannot connect to backend API"
**Cause:** Backend not running  
**Solution:** Start backend with `dotnet run` in `backend/VendorMdm.Api`

### Issue 2: "ECONNREFUSED" error
**Cause:** Backend not running on port 5000  
**Solution:** 
- Check if port 5000 is in use: `lsof -i :5000`
- Start backend on correct port
- Or update `VITE_API_BASE_URL` in frontend `.env` file

### Issue 3: "404 Not Found"
**Cause:** API endpoint doesn't exist  
**Solution:** 
- Check backend is running
- Verify route: `/api/invitation/create`
- Check Swagger UI: http://localhost:5000/swagger

### Issue 4: "500 Internal Server Error"
**Cause:** Backend error (database, service, etc.)  
**Solution:**
- Check backend console for error messages
- Verify database connection
- Check backend logs

---

## API Configuration

### Frontend API Base URL
The frontend is configured to use:
- **Local Dev**: `http://localhost:5000/api`
- **Production**: `/api` (relative, uses Azure Static Web Apps routing)

### Backend API Endpoints
- **Create Invitation**: `POST /api/invitation/create`
- **List Invitations**: `GET /api/invitation/list`
- **Validate Invitation**: `GET /api/invitation/validate/{token}`
- **Complete Invitation**: `POST /api/invitation/complete/{token}`

---

## Quick Test

### Test Backend is Running
```bash
# Should return Swagger UI HTML
curl http://localhost:5000/swagger

# Should return API response or error (not connection refused)
curl http://localhost:5000/api/invitation/list
```

### Test Frontend Can Reach Backend
1. Open browser DevTools (F12)
2. Go to Network tab
3. Try creating invitation
4. Check the request:
   - **URL**: Should be `http://localhost:5000/api/invitation/create`
   - **Status**: Should be 200 (success) or 4xx/5xx (error, but connected)

---

## Updated Error Messages

The frontend now shows more helpful error messages:
- ✅ "Cannot connect to backend API. Please ensure the backend is running on http://localhost:5000"
- ✅ Shows specific error details from backend
- ✅ Provides troubleshooting steps

---

## Next Steps After Fix

Once backend is running:
1. ✅ Create invitation should work
2. ✅ Check backend console for logs
3. ✅ Verify data in database (if connected)
4. ✅ Test invitation link generation

---

*Last updated: 2025-01-27*

