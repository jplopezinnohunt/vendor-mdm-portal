# Deployment Plan - Personal Details & Document Registry

## ⚠️ CRITICAL: Deployment Order & Safety

This feature includes **infrastructure changes** and **database schema changes**. Incorrect deployment order can cause production downtime.

---

## Pre-Deployment Checklist

### 1. Backup Current State
```bash
# Backup Azure SQL Database
az sql db export \
  --resource-group rg-vendor-mdm-dev \
  --server sql-vendor-mdm-dev \
  --name VendorMdmDb \
  --storage-key-type StorageAccessKey \
  --storage-key $STORAGE_KEY \
  --storage-uri "https://stvmdmdev.blob.core.windows.net/backups/pre-deployment-$(date +%Y%m%d).bacpac"

# Document current infrastructure state
az resource list --resource-group rg-vendor-mdm-dev > infrastructure-snapshot-$(date +%Y%m%d).json
```

### 2. Verify Feature Branch
```bash
git checkout feature/personal-details-enhancements
git pull origin feature/personal-details-enhancements
git log --oneline -10  # Review commits
```

**Expected Commits**:
- `b017a15` - Infrastructure and data models
- `691f585` - Documentation
- `f1a2054` - Backend and frontend core
- `d30d5aa` - Document Registry entity
- `49d4b58` - Global document taxonomy
- `ecdaa2a` - Multi-country implementation

### 3. Local Testing
```bash
# Backend build test
cd backend
dotnet build --configuration Release
dotnet test

# Frontend build test  
cd frontend
npm run build
```

---

## Deployment Steps (MUST FOLLOW IN ORDER)

### Phase 1: Infrastructure (Azure Storage)

**Risk Level**: 🟡 Medium (new resource, no breaking changes)

**Step 1.1**: Deploy Storage Account
```bash
cd infrastructure

# DRY RUN first (validate template)
az deployment group validate \
  --resource-group rg-vendor-mdm-dev \
  --template-file main.bicep \
  --parameters environmentName=dev

# ACTUAL DEPLOYMENT
az deployment group create \
  --resource-group rg-vendor-mdm-dev \
  --template-file main.bicep \
  --parameters environmentName=dev \
  --mode Incremental  # CRITICAL: Use Incremental, not Complete
```

**Step 1.2**: Verify Storage Account Created
```bash
# Check storage account exists
az storage account show \
  --name stvendormdmdev \
  --resource-group rg-vendor-mdm-dev

# Check container created
az storage container list \
  --account-name stvendormdmdev \
  --auth-mode login

# Expected containers:
# - vendor-attachments (private)
# - deleted-blobs (private)
```

**Step 1.3**: Verify Key Vault Secret
```bash
az keyvault secret show \
  --vault-name kv-vendor-mdm-dev \
  --name ConnectionStrings--BlobStorage

# Should return connection string (value will be hidden)
```

**Step 1.4**: Verify RBAC Role Assignment
```bash
# Check App Service has Storage Blob Data Contributor role
az role assignment list \
  --assignee $(az webapp identity show --resource-group rg-vendor-mdm-dev --name app-vendor-mdm-api-dev --query principalId -o tsv) \
  --scope /subscriptions/$SUBSCRIPTION_ID/resourceGroups/rg-vendor-mdm-dev/providers/Microsoft.Storage/storageAccounts/stvendormdmdev
```

**Rollback Plan (Phase 1)**:
```bash
# If issues, delete storage account (data will be lost)
az storage account delete \
  --name stvendormdmdev \
  --resource-group rg-vendor-mdm-dev \
  --yes
```

---

### Phase 2: Database Migration (DocumentRegistry)

**Risk Level**: 🔴 HIGH (schema changes, cannot rollback easily)

**Step 2.1**: Create EF Migration (Local)
```bash
cd backend

# Create migration
dotnet ef migrations add AddDocumentRegistryAndGlobalTaxonomy \
  --project VendorMdm.Api \
  --startup-project VendorMdm.Api \
  --context SqlDbContext

# Review migration SQL
dotnet ef migrations script \
  --project VendorMdm.Api \
  --context SqlDbContext \
  --output migration-script.sql

# CRITICAL: Review migration-script.sql BEFORE applying
```

**Step 2.2**: Review Migration Script

**Expected Changes**:
- ✅ CREATE TABLE `DocumentRegistry` with all columns
- ✅ CREATE INDEX `idx_docreg_entity` ON `DocumentRegistry(EntityRef, EntityType)`
- ✅ CREATE INDEX `idx_docreg_expiry` ON `DocumentRegistry(ExpiryDate)` WHERE `DocumentStatus = 'Verified'`
- ✅ CREATE INDEX `idx_docreg_extracted` ON `DocumentRegistry` USING GIN (`Data`)
- ✅ NO DROP statements (unless expected)
- ✅ NO ALTER on existing tables (unless expected)

**Step 2.3**: Apply Migration to DEV Database
```bash
# Option A: Via dotnet ef (recommended for dev)
dotnet ef database update \
  --project VendorMdm.Api \
  --context SqlDbContext \
  --connection "Server=tcp:sql-vendor-mdm-dev.database.windows.net,1433;Database=VendorMdmDb;User ID=sqladmin;Password=$SQL_PASSWORD;Encrypt=True;"

# Option B: Via Azure SQL (more control)
az sql db query \
  --resource-group rg-vendor-mdm-dev \
  --server sql-vendor-mdm-dev \
  --name VendorMdmDb \
  --username sqladmin \
  --password $SQL_PASSWORD \
  --file migration-script.sql
```

**Step 2.4**: Verify Migration Applied
```bash
# Check table exists
az sql db query \
  --resource-group rg-vendor-mdm-dev \
  --server sql-vendor-mdm-dev \
  --name VendorMdmDb \
  --username sqladmin \
  --password $SQL_PASSWORD \
  --query "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'DocumentRegistry'"

# Check indexes created
az sql db query \
  --resource-group rg-vendor-mdm-dev \
  --server sql-vendor-mdm-dev \
  --name VendorMdmDb \
  --username sqladmin \
  --password $SQL_PASSWORD \
  --query "SELECT name FROM sys.indexes WHERE object_id = OBJECT_ID('DocumentRegistry')"
```

**Rollback Plan (Phase 2)**:
```bash
# CRITICAL: Rollback is complex. Options:
# 1. Downgrade migration (if no data written yet)
dotnet ef database update PreviousMigrationName \
  --project VendorMdm.Api \
  --context SqlDbContext

# 2. Manual DROP (only if empty table)
az sql db query \
  --resource-group rg-vendor-mdm-dev \
  --server sql-vendor-mdm-dev \
  --name VendorMdmDb \
  --username sqladmin \
  --password $SQL_PASSWORD \
  --query "DROP TABLE DocumentRegistry"

# 3. Restore from backup (nuclear option)
az sql db restore \
  --resource-group rg-vendor-mdm-dev \
  --server sql-vendor-mdm-dev \
  --name VendorMdmDb-rollback \
  --source-database VendorMdmDb \
  --restore-point <BACKUP_TIMESTAMP>
```

---

### Phase 3: Backend Deployment

**Risk Level**: 🟡 Medium (new code, backward compatible)

**Step 3.1**: Install NuGet Packages
```bash
cd backend/VendorMdm.Api
dotnet add package Azure.Storage.Blobs --version 12.19.1
```

**Step 3.2**: Build and Publish
```bash
cd backend/VendorMdm.Api
dotnet publish -c Release -o ./publish

# Create zip
cd publish
zip -r ../publish.zip .
cd ..
```

**Step 3.3**: Deploy to Azure App Service
```bash
# Deploy via Azure CLI (as per CI/CD rules)
az webapp deployment source config-zip \
  --resource-group rg-vendor-mdm-dev \
  --name app-vendor-mdm-api-dev \
  --src publish.zip

# Wait for deployment to complete
az webapp log tail \
  --resource-group rg-vendor-mdm-dev \
  --name app-vendor-mdm-api-dev
```

**Step 3.4**: Verify Backend Health
```bash
# Health check
curl https://app-vendor-mdm-api-dev.azurewebsites.net/health

# Check new endpoints exist
curl https://app-vendor-mdm-api-dev.azurewebsites.net/api/attachments/request-upload \
  -X POST \
  -H "Content-Type: application/json" \
  -d '{"fileName":"test.pdf","contentType":"application/pdf","category":"DOC_LEG_REG","sizeBytes":1000,"vendorId":"test"}'
```

**Rollback Plan (Phase 3)**:
```bash
# Redeploy previous version from Git
git checkout main  # or previous stable tag
cd backend/VendorMdm.Api
dotnet publish -c Release -o ./publish
cd publish
zip -r ../publish.zip .
cd ..

az webapp deployment source config-zip \
  --resource-group rg-vendor-mdm-dev \
  --name app-vendor-mdm-api-dev \
  --src publish.zip
```

---

### Phase 4: Frontend Deployment

**Risk Level**: 🟢 Low (Static Web App, automated rollback)

**Step 4.1**: Merge to Develop Branch
```bash
# Local merge
git checkout develop
git merge feature/personal-details-enhancements

# Push to trigger GitHub Actions
git push origin develop
```

**Step 4.2**: Monitor GitHub Actions
- Go to: https://github.com/jplopezinnohunt/vendor-mdm-portal/actions
- Wait for "Azure Static Web Apps CI/CD" workflow to complete
- Expected duration: 3-5 minutes

**Step 4.3**: Verify SWA Deployment
```bash
# Check SWA URL
curl https://swa-vendor-mdm-dev.azurestaticapps.net

# Test new components exist (browser dev tools)
# Should see: CollapsibleSection, FileUpload components loaded
```

**Rollback Plan (Phase 4)**:
```bash
# GitHub Actions has automatic rollback
# OR manually rollback to previous commit
git revert HEAD
git push origin develop
```

---

## Post-Deployment Validation

### Integration Tests

**Test 1**: Storage Account Connection
```bash
# From backend app, test blob upload
curl -X POST https://app-vendor-mdm-api-dev.azurewebsites.net/api/attachments/request-upload \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "fileName": "test-deployment.pdf",
    "contentType": "application/pdf",
    "category": "DOC_LEG_REG",
    "sizeBytes": 5000,
    "vendorId": "test-vendor-001"
  }'

# Expected: Returns SAS URL with 5-min expiry
```

**Test 2**: DocumentRegistry CRUD
```bash
# Create document registry record (via Swagger or Postman)
POST /api/documents/register
{
  "entityType": "Vendor",
  "entityRef": "test-vendor-001",
  "category": "DOC_LEG_REG",
  "docType": "DOCTYPE_VAT_CERT",
  "securityLevel": 2,
  "storagePath": "dev/vendors/test-vendor-001/DOC_LEG_REG/DOCTYPE_VAT_CERT/20260103_abc123_en-US_test.pdf"
}

# Expected: 201 Created with document ID
```

**Test 3**: Country-to-Document Mapping
```bash
# Test DocumentRequirementsService
# Should return correct docs for country code
```

### Smoke Tests (Manual)

1. **File Upload Flow**:
   - ✅ Navigate to vendor registration form
   - ✅ Click "Add file..." under Identification
   - ✅ Select a PDF file
   - ✅ Upload completes with progress bar
   - ✅ File appears in list
   - ✅ Download works
   - ✅ Delete works

2. **Collapsible Sections**:
   - ✅ Sections expand/collapse smoothly
   - ✅ Chevron icon rotates correctly

3. **Extended Fields**:
   - ✅ Street Name 2-4 fields appear
   - ✅ Payment Email field appears
   - ✅ Fax field appears (not for Participant type)

---

## Monitoring Post-Deployment

### Azure Monitor Queries

**Check for Errors (Last 1 hour)**:
```kusto
traces
| where timestamp > ago(1h)
| where severityLevel >= 3  // Error or higher
| where message contains "DocumentRegistry" or message contains "BlobStorage"
| project timestamp, severityLevel, message, operation_Name
```

**Check Storage Operations**:
```kusto
dependencies
| where timestamp > ago(1h)
| where target contains "stvendormdmdev"
| summarize count() by target, resultCode
```

---

## Rollback Decision Matrix

| Issue | Severity | Rollback Phase | Action |
|-------|----------|----------------|--------|
| Storage account failed to create | 🔴 Critical | Phase 1 | Delete storage, fix Bicep, redeploy |
| Migration failed | 🔴 Critical | Phase 2 | Restore DB backup, investigate |
| Backend 500 errors | 🟡 High | Phase 3 | Redeploy previous version |
| Frontend not loading | 🟢 Medium | Phase 4 | GitHub Actions auto-rollback |
| File upload fails | 🟡 High | Config | Check RBAC permissions, Key Vault |

---

## Success Criteria

✅ Storage account `stvendormdmdev` exists with 2 containers  
✅ Key Vault secret `ConnectionStrings--BlobStorage` populated  
✅ DocumentRegistry table exists with 3 indexes  
✅ Backend health check returns 200 OK  
✅ File upload returns SAS URL  
✅ Frontend loads without console errors  
✅ No Azure Monitor alerts triggered  

---

## Deployment Approval

**Deployment Ready When**:
- [ ] All pre-deployment checks pass
- [ ] Database backup completed
- [ ] Infrastructure snapshot saved
- [ ] Migration script reviewed
- [ ] Rollback plan understood

**Approved By**: ___________________  
**Date**: ___________________
