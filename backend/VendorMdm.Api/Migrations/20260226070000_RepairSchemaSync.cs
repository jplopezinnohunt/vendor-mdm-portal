using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VendorMdm.Api.Migrations
{
    /// <summary>
    /// REPAIR MIGRATION: Sync Azure SQL schema with EF model.
    ///
    /// Root cause: Migrations 20260128183157, 20260203075258, 20260204000000
    /// were generated against SQLite but applied to SQL Server via sed patch.
    /// The sed patch (TEXT→nvarchar) couldn't handle type conversions like
    /// uniqueidentifier→TEXT, causing silent failures. This left the DB
    /// missing columns and tables that the C# model expects.
    ///
    /// This migration is hand-crafted SQL to safely add ONLY what's missing.
    /// All statements use IF NOT EXISTS to be idempotent.
    ///
    /// Missing from DB:
    /// 1. Soft delete columns (IsDeleted, DeletedAt, DeletedBy) on ALL canonical entities
    /// 2. Vendor-specific columns (TenantId, DataResidencyRegion)
    /// 3. VendorInvitations columns (EventId, Tier) from AddEventIdToInvitation
    /// 4. AuditLogs table from AddAuditLogTable
    /// 5. JSON computed columns from AddJsonComputedColumnsAndIndexes
    /// 6. Missing __EFMigrationsHistory entries for skipped migrations
    /// </summary>
    public partial class RepairSchemaSync : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =================================================================
            // STEP 1: Record the 3 skipped migrations so EF won't re-run them
            // =================================================================
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE MigrationId = '20260128183157_AddEventIdToInvitation')
                    INSERT INTO [__EFMigrationsHistory] (MigrationId, ProductVersion) VALUES ('20260128183157_AddEventIdToInvitation', '8.0.11');

                IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE MigrationId = '20260203075258_AddAuditLogTable')
                    INSERT INTO [__EFMigrationsHistory] (MigrationId, ProductVersion) VALUES ('20260203075258_AddAuditLogTable', '8.0.11');

                IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE MigrationId = '20260204000000_AddJsonComputedColumnsAndIndexes')
                    INSERT INTO [__EFMigrationsHistory] (MigrationId, ProductVersion) VALUES ('20260204000000_AddJsonComputedColumnsAndIndexes', '8.0.11');
            ");

            // =================================================================
            // STEP 2: Add missing columns from AddEventIdToInvitation
            // =================================================================
            migrationBuilder.Sql(@"
                IF COL_LENGTH('VendorInvitations', 'EventId') IS NULL
                    ALTER TABLE VendorInvitations ADD EventId uniqueidentifier NULL;

                IF COL_LENGTH('VendorInvitations', 'Tier') IS NULL
                    ALTER TABLE VendorInvitations ADD Tier nvarchar(20) NULL;
            ");

            // =================================================================
            // STEP 3: Create AuditLogs table from AddAuditLogTable
            // =================================================================
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AuditLogs')
                BEGIN
                    CREATE TABLE AuditLogs (
                        Id uniqueidentifier NOT NULL,
                        EntityType nvarchar(100) NOT NULL,
                        EntityId uniqueidentifier NOT NULL,
                        Action nvarchar(50) NOT NULL,
                        ChangedBy nvarchar(256) NOT NULL,
                        ChangedByUserId uniqueidentifier NOT NULL,
                        ChangedAt datetime2 NOT NULL,
                        OldValues nvarchar(max) NULL,
                        NewValues nvarchar(max) NULL,
                        Reason nvarchar(500) NULL,
                        IpAddress nvarchar(45) NULL,
                        UserAgent nvarchar(500) NULL,
                        TenantId uniqueidentifier NULL,
                        CONSTRAINT PK_AuditLogs PRIMARY KEY (Id)
                    );

                    CREATE INDEX IX_AuditLog_Entity ON AuditLogs (EntityType, EntityId, ChangedAt);
                    CREATE INDEX IX_AuditLog_User ON AuditLogs (ChangedByUserId, ChangedAt);
                    CREATE INDEX IX_AuditLog_Timestamp ON AuditLogs (ChangedAt);
                    CREATE INDEX IX_AuditLog_Action ON AuditLogs (Action, ChangedAt);
                    CREATE INDEX IX_AuditLog_Tenant ON AuditLogs (TenantId, ChangedAt);
                END
            ");

            // =================================================================
            // STEP 4: Add JSON computed columns from AddJsonComputedColumnsAndIndexes
            // =================================================================
            migrationBuilder.Sql(@"
                IF COL_LENGTH('VendorApplications', 'VendorType_Computed') IS NULL
                BEGIN
                    ALTER TABLE VendorApplications
                    ADD VendorType_Computed AS CAST(JSON_VALUE(Attributes, '$.VendorType') AS NVARCHAR(50)) PERSISTED;

                    CREATE NONCLUSTERED INDEX IX_VendorApplications_VendorType
                    ON VendorApplications(VendorType_Computed)
                    WHERE VendorType_Computed IS NOT NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('VendorApplications', 'AccountGroup_Computed') IS NULL
                BEGIN
                    ALTER TABLE VendorApplications
                    ADD AccountGroup_Computed AS CAST(JSON_VALUE(Attributes, '$.AccountGroup') AS NVARCHAR(50)) PERSISTED;

                    CREATE NONCLUSTERED INDEX IX_VendorApplications_AccountGroup
                    ON VendorApplications(AccountGroup_Computed)
                    WHERE AccountGroup_Computed IS NOT NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('VendorInvitations', 'Currency_Computed') IS NULL
                BEGIN
                    ALTER TABLE VendorInvitations
                    ADD Currency_Computed AS CAST(JSON_VALUE(Attributes, '$.Metadata.Currency') AS NVARCHAR(10)) PERSISTED;

                    CREATE NONCLUSTERED INDEX IX_VendorInvitations_Currency
                    ON VendorInvitations(Currency_Computed)
                    WHERE Currency_Computed IS NOT NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('VendorInvitations', 'CompanyCode_Computed') IS NULL
                BEGIN
                    ALTER TABLE VendorInvitations
                    ADD CompanyCode_Computed AS CAST(JSON_VALUE(Attributes, '$.CompanyCode') AS NVARCHAR(10)) PERSISTED;

                    CREATE NONCLUSTERED INDEX IX_VendorInvitations_CompanyCode
                    ON VendorInvitations(CompanyCode_Computed)
                    WHERE CompanyCode_Computed IS NOT NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('ChangeRequests', 'ChangeType_Computed') IS NULL
                BEGIN
                    ALTER TABLE ChangeRequests
                    ADD ChangeType_Computed AS CAST(JSON_VALUE(Attributes, '$.ChangeType') AS NVARCHAR(50)) PERSISTED;

                    CREATE NONCLUSTERED INDEX IX_ChangeRequests_ChangeType
                    ON ChangeRequests(ChangeType_Computed)
                    WHERE ChangeType_Computed IS NOT NULL;
                END
            ");

            // =================================================================
            // STEP 5: Add soft delete columns to ALL canonical entities
            // Pattern 11: Soft Delete (CanonicalEntityBase)
            // =================================================================

            // --- Vendors ---
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Vendors', 'IsDeleted') IS NULL
                    ALTER TABLE Vendors ADD IsDeleted bit NOT NULL DEFAULT 0;
                IF COL_LENGTH('Vendors', 'DeletedAt') IS NULL
                    ALTER TABLE Vendors ADD DeletedAt datetime2 NULL;
                IF COL_LENGTH('Vendors', 'DeletedBy') IS NULL
                    ALTER TABLE Vendors ADD DeletedBy nvarchar(256) NULL;
            ");

            // --- Vendor-specific columns ---
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Vendors', 'TenantId') IS NULL
                    ALTER TABLE Vendors ADD TenantId uniqueidentifier NULL;
                IF COL_LENGTH('Vendors', 'DataResidencyRegion') IS NULL
                    ALTER TABLE Vendors ADD DataResidencyRegion nvarchar(20) NOT NULL DEFAULT 'GLOBAL';
            ");

            // --- VendorInvitationsCanonical ---
            migrationBuilder.Sql(@"
                IF COL_LENGTH('VendorInvitationsCanonical', 'IsDeleted') IS NULL
                    ALTER TABLE VendorInvitationsCanonical ADD IsDeleted bit NOT NULL DEFAULT 0;
                IF COL_LENGTH('VendorInvitationsCanonical', 'DeletedAt') IS NULL
                    ALTER TABLE VendorInvitationsCanonical ADD DeletedAt datetime2 NULL;
                IF COL_LENGTH('VendorInvitationsCanonical', 'DeletedBy') IS NULL
                    ALTER TABLE VendorInvitationsCanonical ADD DeletedBy nvarchar(256) NULL;
            ");

            // --- ChangeRequestsCanonical ---
            migrationBuilder.Sql(@"
                IF COL_LENGTH('ChangeRequestsCanonical', 'IsDeleted') IS NULL
                    ALTER TABLE ChangeRequestsCanonical ADD IsDeleted bit NOT NULL DEFAULT 0;
                IF COL_LENGTH('ChangeRequestsCanonical', 'DeletedAt') IS NULL
                    ALTER TABLE ChangeRequestsCanonical ADD DeletedAt datetime2 NULL;
                IF COL_LENGTH('ChangeRequestsCanonical', 'DeletedBy') IS NULL
                    ALTER TABLE ChangeRequestsCanonical ADD DeletedBy nvarchar(256) NULL;
            ");

            // --- Employees ---
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Employees', 'IsDeleted') IS NULL
                    ALTER TABLE Employees ADD IsDeleted bit NOT NULL DEFAULT 0;
                IF COL_LENGTH('Employees', 'DeletedAt') IS NULL
                    ALTER TABLE Employees ADD DeletedAt datetime2 NULL;
                IF COL_LENGTH('Employees', 'DeletedBy') IS NULL
                    ALTER TABLE Employees ADD DeletedBy nvarchar(256) NULL;
            ");

            // --- Projects ---
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Projects', 'IsDeleted') IS NULL
                    ALTER TABLE Projects ADD IsDeleted bit NOT NULL DEFAULT 0;
                IF COL_LENGTH('Projects', 'DeletedAt') IS NULL
                    ALTER TABLE Projects ADD DeletedAt datetime2 NULL;
                IF COL_LENGTH('Projects', 'DeletedBy') IS NULL
                    ALTER TABLE Projects ADD DeletedBy nvarchar(256) NULL;
            ");

            // --- Funds ---
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Funds', 'IsDeleted') IS NULL
                    ALTER TABLE Funds ADD IsDeleted bit NOT NULL DEFAULT 0;
                IF COL_LENGTH('Funds', 'DeletedAt') IS NULL
                    ALTER TABLE Funds ADD DeletedAt datetime2 NULL;
                IF COL_LENGTH('Funds', 'DeletedBy') IS NULL
                    ALTER TABLE Funds ADD DeletedBy nvarchar(256) NULL;
            ");

            // --- Customers ---
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Customers', 'IsDeleted') IS NULL
                    ALTER TABLE Customers ADD IsDeleted bit NOT NULL DEFAULT 0;
                IF COL_LENGTH('Customers', 'DeletedAt') IS NULL
                    ALTER TABLE Customers ADD DeletedAt datetime2 NULL;
                IF COL_LENGTH('Customers', 'DeletedBy') IS NULL
                    ALTER TABLE Customers ADD DeletedBy nvarchar(256) NULL;
            ");

            // --- Users ---
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Users', 'IsDeleted') IS NULL
                    ALTER TABLE Users ADD IsDeleted bit NOT NULL DEFAULT 0;
                IF COL_LENGTH('Users', 'DeletedAt') IS NULL
                    ALTER TABLE Users ADD DeletedAt datetime2 NULL;
                IF COL_LENGTH('Users', 'DeletedBy') IS NULL
                    ALTER TABLE Users ADD DeletedBy nvarchar(256) NULL;
            ");

            // --- WorkflowSteps (also inherits CanonicalEntityBase) ---
            migrationBuilder.Sql(@"
                IF COL_LENGTH('WorkflowSteps', 'IsDeleted') IS NULL
                    ALTER TABLE WorkflowSteps ADD IsDeleted bit NOT NULL DEFAULT 0;
                IF COL_LENGTH('WorkflowSteps', 'DeletedAt') IS NULL
                    ALTER TABLE WorkflowSteps ADD DeletedAt datetime2 NULL;
                IF COL_LENGTH('WorkflowSteps', 'DeletedBy') IS NULL
                    ALTER TABLE WorkflowSteps ADD DeletedBy nvarchar(256) NULL;
            ");

            // =================================================================
            // STEP 6: Create indexes for soft delete (Pattern 11)
            // =================================================================
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Vendor_IsDeleted')
                    CREATE INDEX IX_Vendor_IsDeleted ON Vendors(IsDeleted);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_VendorInvitationCanonical_IsDeleted')
                    CREATE INDEX IX_VendorInvitationCanonical_IsDeleted ON VendorInvitationsCanonical(IsDeleted);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChangeRequestCanonical_IsDeleted')
                    CREATE INDEX IX_ChangeRequestCanonical_IsDeleted ON ChangeRequestsCanonical(IsDeleted);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Employee_IsDeleted')
                    CREATE INDEX IX_Employee_IsDeleted ON Employees(IsDeleted);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Project_IsDeleted')
                    CREATE INDEX IX_Project_IsDeleted ON Projects(IsDeleted);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Fund_IsDeleted')
                    CREATE INDEX IX_Fund_IsDeleted ON Funds(IsDeleted);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Customer_IsDeleted')
                    CREATE INDEX IX_Customer_IsDeleted ON Customers(IsDeleted);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_User_IsDeleted')
                    CREATE INDEX IX_User_IsDeleted ON Users(IsDeleted);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Down migration: Remove soft delete columns and undo repairs
            // WARNING: This would drop data. Only use for full rollback.
            migrationBuilder.Sql(@"
                -- Remove soft delete indexes
                DROP INDEX IF EXISTS IX_Vendor_IsDeleted ON Vendors;
                DROP INDEX IF EXISTS IX_VendorInvitationCanonical_IsDeleted ON VendorInvitationsCanonical;
                DROP INDEX IF EXISTS IX_ChangeRequestCanonical_IsDeleted ON ChangeRequestsCanonical;
                DROP INDEX IF EXISTS IX_Employee_IsDeleted ON Employees;
                DROP INDEX IF EXISTS IX_Project_IsDeleted ON Projects;
                DROP INDEX IF EXISTS IX_Fund_IsDeleted ON Funds;
                DROP INDEX IF EXISTS IX_Customer_IsDeleted ON Customers;
                DROP INDEX IF EXISTS IX_User_IsDeleted ON Users;
            ");

            // Note: We do NOT drop the AuditLogs table or remove the soft delete columns
            // in Down() because that would violate Zero Data Loss policy.
            // To fully reverse, create a separate intentional migration.
        }
    }
}
