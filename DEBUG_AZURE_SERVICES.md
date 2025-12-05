# 🔍 Azure Services Debug - Why Invitation Creation Failed

## Step 1: Check if API Endpoint Exists

Open browser console (F12) on the invitation page and look for the API call:

**Expected call:**
```
POST https://YOUR-API-URL/api/invitation/create
```

**What do you see?**
- ❌ `Failed to fetch` → API not deployed or not accessible
- ❌ `404 Not Found` → Endpoint doesn't exist  
- ❌ `500 Internal Server Error` → Backend error (table missing?)
- ❌ `CORS error` → Cross-origin blocked

## Step 2: Find Your API URL

Check where your frontend is calling:

```javascript
// In frontend/src/services/api.ts
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';
```

**In production (Azure Static Web Apps):**
- Calls should go to `/api` which routes to your backend
- Or check SWA configuration for API backend

## Step 3: Test API Directly

Try this URL in browser:
```
https://black-flower-0aee6900f.3.azurestaticapps.net/api/healthcheck
```

**Does it work?**
- ✅ Yes → Backend is deployed
- ❌ No → Backend not deployed or not linked

## Step 4: Check GitHub Actions

Go to: https://github.com/jplopezinnohunt/vendor-mdm-portal/actions

Look for latest "Deploy Backend Artifacts" workflow:
- ✅ Green checkmark → Deployed successfully
- ❌ Red X → Deployment failed
- ⚠️ Yellow → Still running

## Step 5: Check Browser Console Error

**On the invitation page:**
1. Press F12
2. Go to Console tab
3. Click "Create Invitation"
4. **Copy the exact error message**

That will tell us exactly what's broken!

---

## Most Likely Issues

### Issue 1: API Not Deployed
**Symptom:** `Failed to fetch` or `404`  
**Cause:** Backend not deployed or not linked to Static Web App  
**Check:** GitHub Actions logs

### Issue 2: Database Table Missing  
**Symptom:** `500 Internal Server Error`  
**Error:** "Invalid object name 'VendorInvitations'"  
**Fix:** Run SQL migration in Azure SQL

### Issue 3: Cosmos Container Missing
**Symptom:** 500 error with Cosmos message  
**Cause:** InvitationArtifacts container doesn't exist  
**Impact:** Should be non-blocking (logged but continues)

---

## Quick Test

**Open browser console and run:**
```javascript
// Test if API is accessible
fetch('/api/invitation/validate/test-token')
  .then(r => r.json())
  .then(console.log)
  .catch(console.error);
```

**What happens?**
- Response with error → API works, but token invalid (expected)
- Network error → API not accessible
- CORS error → Configuration issue

---

## What I Need to Help Debug

Please share:
1. **Browser console error** (exact message)
2. **Network tab** - screenshot of failed request
3. **GitHub Actions** - did deployment succeed?

Then I can tell you exactly what's missing!
