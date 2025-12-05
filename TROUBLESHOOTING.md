# Troubleshooting: Cannot Create Invitation

## Issue
Unable to create invitation - button click not working

## Debugging Steps

### Step 1: Check Browser Console
1. Open browser DevTools (F12)
2. Go to Console tab
3. Click "Create Invitation" button
4. Look for error messages

**Common errors:**
- `Network error` - API not running
- `404 Not Found` - Endpoint doesn't exist
- `500 Internal Server Error` - Backend error
- `CORS error` - Cross-origin issue

### Step 2: Check Network Tab
1. Open DevTools → Network tab
2. Click "Create Invitation"
3. Look for request to `/api/invitation/create`
4. Check response status and error message

### Step 3: Verify Backend is Running

**Check if API is running:**
```powershell
# Check if backend is running on port 7071
Test-NetConnection -ComputerName localhost -Port 7071
```

**If not running, start it:**
```powershell
cd backend/VendorMdm.Api
dotnet run
```

### Step 4: Check Database

The VendorInvitations table might not exist yet. Run migration:

```powershell
cd backend/VendorMdm.Api

# Create migration
dotnet ef migrations add AddVendorInvitations --context SqlDbContext

# Apply migration
dotnet ef database update --context SqlDbContext
```

### Step 5: Check API Endpoint

Test the endpoint directly:

```powershell
# Test if endpoint exists
curl -X POST http://localhost:7071/api/invitation/create `
  -H "Content-Type: application/json" `
  -d '{
    "vendorLegalName": "Test Company",
    "primaryContactEmail": "test@example.com",
    "expirationDays": 14,
    "notes": "Test"
  }'
```

## Quick Fix Checklist

- [ ] Backend API running (port 7071 or configured port)
- [ ] Database migration applied (VendorInvitations table exists)
- [ ] Cosmos DB containers created (optional for now)
- [ ] CORS configured for frontend origin
- [ ] No errors in browser console

## Most Likely Issues

### 1. Backend Not Running
**Symptom:** Network error in console  
**Fix:** Start backend with `dotnet run`

### 2. Table Doesn't Exist
**Symptom:** SQL error "Invalid object name 'VendorInvitations'"  
**Fix:** Run `dotnet ef database update`

### 3. Cosmos Containers Don't Exist
**Symptom:** "Container InvitationArtifacts not found"  
**Fix:** Service will log warning but invitation will work (Cosmos is optional for basic flow)

### 4. CORS Error
**Symptom:** "CORS policy" error in console  
**Fix:** Check Program.cs CORS configuration allows your frontend origin

## Expected Response

**Success:**
```json
{
  "invitationId": "guid",
  "invitationToken": "secure-token",
  "invitationLink": "/invitation/register/token",
  "expiresAt": "2025-12-12T..."
}
```

**Error:**
```json
{
  "error": "Error message here"
}
```

## Next Steps

Please check

:
1. Browser console errors (Screenshot if possible)
2. Is backend running? (`dotnet run` in VendorMdm.Api)
3. Any error messages?

I'll help debug based on what you find!
