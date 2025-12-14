-- SAFE Azure SQL Migration for Canonical Model
-- This script creates ONLY the new canonical entity tables
-- Assumes existing tables (Attachments, VendorInvitations, etc.) already exist

-- 1. Create Vendors (Canonical Master)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Vendors')
BEGIN
    CREATE TABLE [Vendors] (
        [Id] uniqueidentifier NOT NULL PRIMARY KEY,
        [Legal Name] nvarchar(200) NOT NULL,
        [TaxId] nvarchar(100) NULL,
        [PrimaryContactEmail] nvarchar(255) NOT NULL,
        [EntityVersion] int NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [SourceSystem] nvarchar(50) NOT NULL,
        [Data] nvarchar(max) NOT NULL DEFAULT '{}',
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [SchemaVersion] nvarchar(20) NOT NULL
    );

    CREATE INDEX [IX_Vendors_LegalName] ON [Vendors]([LegalName]);
    CREATE INDEX [IX_Vendors_PrimaryContactEmail] ON [Vendors]([PrimaryContactEmail]);
    CREATE INDEX [IX_Vendors_SourceSystem] ON [Vendors]([SourceSystem]);
    CREATE INDEX [IX_Vendors_Status] ON [Vendors]([Status]);
    
    PRINT '✅ Vendors table created';
END
ELSE
    PRINT '⚠️ Vendors table already exists - skipping';

-- 2. Create VendorInvitationsCanonical
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'VendorInvitationsCanonical')
BEGIN
    CREATE TABLE [VendorInvitationsCanonical] (
        [Id] uniqueidentifier NOT NULL PRIMARY KEY,
        [InvitationToken] nvarchar(100) NOT NULL,
        [VendorLegalName] nvarchar(200) NOT NULL,
        [PrimaryContactEmail] nvarchar(255) NOT NULL,
        [InvitedBy] uniqueidentifier NOT NULL,
        [InvitedByName] nvarchar(200) NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [CompletedAt] datetime2 NULL,
        [VendorId] uniqueidentifier NULL,
        [EntityVersion] int NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [SourceSystem] nvarchar(50) NOT NULL,
        [Data] nvarchar(max) NOT NULL DEFAULT '{}',
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [SchemaVersion] nvarchar(20) NOT NULL
    );

    CREATE UNIQUE INDEX [IX_VendorInvitationsCanonical_InvitationToken] 
        ON [VendorInvitationsCanonical]([InvitationToken]);
    CREATE INDEX [IX_VendorInvitationsCanonical_PrimaryContactEmail] 
        ON [VendorInvitationsCanonical]([PrimaryContactEmail]);
    CREATE INDEX [IX_VendorInvitationsCanonical_Status] 
        ON [VendorInvitationsCanonical]([Status]);
    CREATE INDEX [IX_VendorInvitationsCanonical_ExpiresAt] 
        ON [VendorInvitationsCanonical]([ExpiresAt]);
    
    PRINT '✅ VendorInvitationsCanonical table created';
END
ELSE
    PRINT '⚠️ VendorInvitationsCanonical table already exists - skipping';

-- 3. Create ChangeRequestsCanonical
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ChangeRequestsCanonical')
BEGIN
    CREATE TABLE [ChangeRequestsCanonical] (
        [Id] uniqueidentifier NOT NULL PRIMARY KEY,
        [VendorId] uniqueidentifier NOT NULL,
        [RequesterId] uniqueidentifier NOT NULL,
        [EntityVersion] int NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [SourceSystem] nvarchar(50) NOT NULL,
        [Data] nvarchar(max) NOT NULL DEFAULT '{}',
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [SchemaVersion] nvarchar(20) NOT NULL
    );

    CREATE INDEX [IX_ChangeRequestsCanonical_VendorId] ON [ChangeRequestsCanonical]([VendorId]);
    CREATE INDEX [IX_ChangeRequestsCanonical_RequesterId] ON [ChangeRequestsCanonical]([RequesterId]);
    CREATE INDEX [IX_ChangeRequestsCanonical_Status] ON [ChangeRequestsCanonical]([Status]);
    
    PRINT '✅ ChangeRequestsCanonical table created';
END
ELSE
    PRINT '⚠️ ChangeRequestsCanonical table already exists - skipping';

-- 4. Create ExternalSystemMappings (Multi-System ACL)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ExternalSystemMappings')
BEGIN
    CREATE TABLE [ExternalSystemMappings] (
        [Id] uniqueidentifier NOT NULL PRIMARY KEY,
        [CanonicalEntityId] uniqueidentifier NOT NULL,
        [EntityType] nvarchar(50) NOT NULL,
        [ExternalSystemId] nvarchar(100) NOT NULL,
        [SystemName] nvarchar(50) NOT NULL,
        [SystemEnvironment] nvarchar(50) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL
    );

    -- Unique constraint: one external system ID per canonical entity per system+environment
    CREATE UNIQUE INDEX [IX_ExternalSystemMapping_Unique] 
        ON [ExternalSystemMappings]([EntityType], [ExternalSystemId], [SystemName], [SystemEnvironment]);
    
    -- Index for canonical ID lookups
    CREATE INDEX [IX_ExternalSystemMapping_Canonical] 
        ON [ExternalSystemMappings]([CanonicalEntityId], [EntityType], [SystemName]);
    
    PRINT '✅ ExternalSystemMappings table created';
END
ELSE
    PRINT '⚠️ ExternalSystemMappings table already exists - skipping';

-- Verification Query
SELECT 
    'Vendors' AS TableName,
    CASE WHEN EXISTS (SELECT * FROM sys.tables WHERE name = 'Vendors') 
        THEN '✅ EXISTS' ELSE '❌ MISSING' END AS Status
UNION ALL
SELECT 
    'VendorInvitationsCanonical',
    CASE WHEN EXISTS (SELECT * FROM sys.tables WHERE name = 'VendorInvitationsCanonical') 
        THEN '✅ EXISTS' ELSE '❌ MISSING' END
UNION ALL
SELECT 
    'ChangeRequestsCanonical',
    CASE WHEN EXISTS (SELECT * FROM sys.tables WHERE name = 'ChangeRequestsCanonical') 
        THEN '✅ EXISTS' ELSE '❌ MISSING' END
UNION ALL
SELECT 
    'ExternalSystemMappings',
    CASE WHEN EXISTS (SELECT * FROM sys.tables WHERE name = 'ExternalSystemMappings') 
        THEN '✅ EXISTS' ELSE '❌ MISSING' END;

PRINT '';
PRINT '✅ Canonical Model Migration Complete!';
PRINT 'Created: Vendors, VendorInvitationsCanonical, ChangeRequestsCanonical, ExternalSystemMappings';
