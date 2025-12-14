# Azure SQL Migration - Canonical Model

## Pre-Migration Checklist

- [ ] Backup existing Azure SQL database
- [ ] Verify connection string in Key Vault or appsettings
- [ ] Review migration SQL

## Migration Commands

### 1. Review Migration
```bash
cd backend/VendorMdm.Api

# Review the migration SQL (optional)
export PATH="$PATH:/Users/jplopez/.dotnet/tools"
dotnet ef migrations script --context SqlDbContext --output migration.sql

# Review migration.sql file
```

### 2. Apply Migration to Azure SQL

**Option A: Direct Migration** (if Azure SQL connection string configured)
```bash
# Ensure appsettings has Azure SQL connection string
dotnet ef database update --context SqlDbContext
```

**Option B: Manual SQL Script** (safer for production)
```bash
# Generate script
dotnet ef migrations script --context SqlDbContext --idempotent --output azure-migration.sql

# Then apply manually in Azure Portal Query Editor or SSMS
```

## What Gets Created

### New Tables
1. **Vendors** - Canonical vendor master
   - Id (UNIQUEIDENTIFIER, PK)
   - Legal Name, TaxId, PrimaryContactEmail (indexed)
   - Status, SourceSystem, EntityVersion
   - Data (nvarchar(max), JSONB equivalent)
   - CreatedAt, UpdatedAt, SchemaVersion

2. **VendorInvitationsCanonical** - Canonical invitations
   - Id, InvitationToken (unique index)
   - PrimaryContactEmail, ExpiresAt
   - Status, SourceSystem, EntityVersion
   - Data (JSONB)

3. **ChangeRequestsCanonical** - Canonical change requests
   - Id, VendorId (FK to Vendors)
   - Status, RequesterId
   - Data (JSONB)

4. **ExternalSystemMappings** - Multi-system ACL
   - Id (PK)
   - CanonicalEntityId, EntityType
   - ExternalSystemId, SystemName, SystemEnvironment
   - Unique index: (EntityType, ExternalSystemId, SystemName, SystemEnvironment)
   - Index: (CanonicalEntityId, EntityType, SystemName)

### Indexes Created
- Primary keys on all Id columns
- Unique index on InvitationToken
- Composite indexes for external system lookups
- Performance indexes on Status, SourceSystem

## Post-Migration Verification

```sql
-- Verify tables created
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('Vendors', 'VendorInvitationsCanonical', 'ChangeRequestsCanonical', 'ExternalSystemMappings');

-- Check indexes
SELECT 
    t.name AS TableName,
    i.name AS IndexName,
    i.is_unique AS IsUnique
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE t.name IN ('Vendors', 'VendorInvitationsCanonical', 'ChangeRequestsCanonical', 'ExternalSystemMappings')
ORDER BY t.name, i.name;
```

## Rollback Plan

If migration fails:
```bash
# Remove the migration
dotnet ef migrations remove --context SqlDbContext

# Or rollback to previous migration
dotnet ef database update PreviousMigrationName --context SqlDbContext
```

## Notes

- Migration is **additive** - existing tables (VendorInvitations, VendorApplications) are NOT modified
- Dual-write pattern allows gradual migration
- Old entities can be deprecated later after full migration
