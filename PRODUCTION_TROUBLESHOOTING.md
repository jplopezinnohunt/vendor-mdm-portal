# 🔧 Production Deployment Issue - Invitation Not Working

## 🎯 You're Testing On: Azure Production
**URL:** `https://black-flower-0aee6900f.3.azurestaticapps.net/admin/invite-vendor`

## ❌ Why It's Not Working

The invitation feature code is deployed to the **frontend**, but the **backend** needs:

1. ✅ Frontend deployed (Static Web App) - **DONE**
2. ❌ Backend API deployed - **NEEDS CHECK**
3. ❌ Database migration run - **NEEDS TO BE RUN**
4. ❌ Cosmos container created - **NEEDS TO BE CREATED**

---

## 🔍 Step 1: Check Backend Deployment Status

### Check GitHub Actions
```
1. Go to: https://github.com/jplopezinnohunt/vendor-mdm-portal/actions
2. Look for "Deploy Backend Artifacts" workflow
3. Check if it completed successfully
4. If failed, check the logs
```

**Expected:** Green checkmark ✅

---

## 🗄️ Step 2: Run Database Migration (CRITICAL)

The `VendorInvitations` table doesn't exist yet in Azure SQL.

### Option A: Via Azure Cloud Shell
```bash
# Connect to your Azure subscription
az login

# Set variables
RESOURCE_GROUP="your-resource-group"
SQL_SERVER="your-sql-server"
DATABASE="your-database"

# Run migrations (if using EF Core migrations)
# You'll need to do this from a machine with dotnet CLI and access to Azure SQL
```

### Option B: Via SQL Server Management Studio / Azure Portal
```sql
-- Run this in Azure SQL Database Query Editor

-- Create VendorInvitations table
CREATE TABLE VendorInvitations (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    InvitationToken NVARCHAR(100) NOT NULL UNIQUE,
    VendorLegalName NVARCHAR(200) NOT NULL,
    PrimaryContactEmail NVARCHAR(255) NOT NULL,
    InvitedBy UNIQUEIDENTIFIER NOT NULL,
    InvitedByName NVARCHAR(200) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ExpiresAt DATETIME2 NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    CompletedAt DATETIME2 NULL,
    VendorApplicationId UNIQUEIDENTIFIER NULL,
    Notes NVARCHAR(1000) NULL
);

-- Create indexes
CREATE INDEX IX_VendorInvitations_Token ON VendorInvitations(InvitationToken);
CREATE INDEX IX_VendorInvitations_Email ON VendorInvitations(PrimaryContactEmail);
CREATE INDEX IX_VendorInvitations_Status ON VendorInvitations(Status);
CREATE INDEX IX_VendorInvitations_ExpiresAt ON VendorInvitations(ExpiresAt);

-- Update VendorApplications table (if not already updated)
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID('VendorApplications') 
               AND name = 'TaxId')
BEGIN
    ALTER TABLE VendorApplications ADD 
        TaxId NVARCHAR(100) NULL,
        ContactName NVARCHAR(200) NULL,
        RegistrationType NVARCHAR(20) NOT NULL DEFAULT 'SelfRegistration',
        InvitationId UNIQUEIDENTIFIER NULL,
        UpdatedAt DATETIME2 NULL;
END
GO
```

---

## 🌐 Step 3: Create Cosmos Container

### Via Azure Portal
```
1. Go to Azure Portal
2. Navigate to your Cosmos DB account
3. Go to Data Explorer
4. Select database "VendorMdm"
5. Click "New Container"
6. Container ID: InvitationArtifacts
7. Partition key: /invitationId
8. Throughput: 400 RU/s (Manual)
9. Click OK
```

**Note:** Cosmos is optional for basic functionality - the service will log errors but continue working.

---

## 🔌 Step 4: Check API Endpoint

### Test the API
Open browser and try:
```
https://YOUR-API-URL/api/invitation/create
```

**Where is YOUR-API-URL?**
- Check your GitHub workflow outputs
- Or check Azure Portal → Function App → Overview → URL

### Example Test
```powershell
# Replace with your actual API URL
$apiUrl = "https://your-function-app.azurewebsites.net"

# Test invitation creation
Invoke-RestMethod -Uri "$apiUrl/api/invitation/create" `
  -Method POST `
  -ContentType "application/json" `
  -Body '{
    "vendorLegalName": "Test Company",
    "primaryContactEmail": "test@example.com",
    "expirationDays": 14,
    "notes": "Test"
  }'
```

---

## 🐛 Step 5: Check Browser Console

1. Open site: `https://black-flower-0aee6900f.3.azurestaticapps.net/admin/invite-vendor`
2. Press F12 (DevTools)
3. Go to Console tab
4. Click "Create Invitation"
5. Look for errors

**Common errors:**
- `Failed to fetch` - API not running/accessible
- `404` - Endpoint doesn't exist
- `500` - Backend error (check logs)
- `CORS` - Cross-origin issue

---

## 🔧 Step 6: Check Azure Function Logs

### Via Azure Portal
```
1. Go to Azure Portal
2. Navigate to your Function App
3. Go to "Monitor" → "Log Stream"
4. Try creating invitation
5. Watch for error messages
```

### Via Azure CLI
```bash
az functionapp log tail \
  --name your-function-app-name \
  --resource-group your-resource-group
```

---

## ✅ Quick Fix (Most Likely Issue)

**The VendorInvitations table doesn't exist in Azure SQL!**

### Fastest Fix:
1. **Go to Azure Portal**
2. **Navigate to SQL Database**
3. **Click "Query editor"**
4. **Login**
5. **Paste and run the SQL script from Step 2**
6. **Try again!**

---

## 📊 Deployment Checklist

- [ ] Frontend deployed (Static Web App) ✅ **DONE**
- [ ] Backend deployed (Function App) - **CHECK GITHUB ACTIONS**
- [ ] SQL Database table created - **RUN MIGRATION**
- [ ] Cosmos container created - **OPTIONAL**
- [ ] API responding to requests - **TEST ENDPOINT**
- [ ] CORS configured - **CHECK FUNCTION APP SETTINGS**

---

## 🚨 Most Likely Problem

**Database Migration Not Run!**

The `VendorInvitations` table doesn't exist in your Azure SQL database yet.

**Quick Solution:**
1. Go to Azure Portal → SQL Database
2. Open Query Editor
3. Run the CREATE TABLE script above
4. Try creating invitation again

---

## 📞 Next Steps

1. **Check GitHub Actions** - Did backend deploy?
2. **Run SQL Script** - Create VendorInvitations table
3. **Test Again** - Try creating invitation
4. **Check Logs** - If still failing, check Function App logs

Let me know what you find and I'll help debug further!
