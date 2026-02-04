using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VendorMdm.Api.Migrations
{
    /// <summary>
    /// Migration: Add Computed Columns for JSON Field Performance
    ///
    /// Brain v1.2.0 Compliance: Pattern 3 - Performance Generated Columns
    ///
    /// This migration addresses the performance gap where JSONB searches
    /// require full table scans. By creating persisted computed columns
    /// that extract frequently-queried JSON values, we enable:
    ///
    /// 1. Index scans instead of table scans for JSON queries
    /// 2. Statistics-based query optimization
    /// 3. Faster filtering on VendorType, AccountGroup, and Currency
    ///
    /// Performance Impact:
    /// - VendorType queries: O(n) -> O(log n) with index
    /// - AccountGroup queries: O(n) -> O(log n) with index
    /// - Currency queries: O(n) -> O(log n) with index
    /// </summary>
    public partial class AddJsonComputedColumnsAndIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =============================================================
            // VendorApplications: Add VendorType computed column + index
            // =============================================================
            // Extracts VendorType from Attributes JSON for fast filtering
            // Used in: Application list filters, reporting, dashboards
            migrationBuilder.Sql(@"
                ALTER TABLE VendorApplications
                ADD VendorType_Computed AS CAST(JSON_VALUE(Attributes, '$.VendorType') AS NVARCHAR(50)) PERSISTED;
            ");

            migrationBuilder.Sql(@"
                CREATE NONCLUSTERED INDEX IX_VendorApplications_VendorType
                ON VendorApplications(VendorType_Computed)
                WHERE VendorType_Computed IS NOT NULL;
            ");

            // =============================================================
            // VendorApplications: Add AccountGroup computed column + index
            // =============================================================
            // Extracts AccountGroup from Attributes JSON for fast filtering
            // Used in: SAP integration, compliance reporting, bulk operations
            migrationBuilder.Sql(@"
                ALTER TABLE VendorApplications
                ADD AccountGroup_Computed AS CAST(JSON_VALUE(Attributes, '$.AccountGroup') AS NVARCHAR(50)) PERSISTED;
            ");

            migrationBuilder.Sql(@"
                CREATE NONCLUSTERED INDEX IX_VendorApplications_AccountGroup
                ON VendorApplications(AccountGroup_Computed)
                WHERE AccountGroup_Computed IS NOT NULL;
            ");

            // =============================================================
            // VendorInvitations: Add Currency computed column + index
            // =============================================================
            // Extracts Currency from Attributes.Metadata.Currency for filtering
            // Used in: Financial reporting, regional dashboards
            migrationBuilder.Sql(@"
                ALTER TABLE VendorInvitations
                ADD Currency_Computed AS CAST(JSON_VALUE(Attributes, '$.Metadata.Currency') AS NVARCHAR(10)) PERSISTED;
            ");

            migrationBuilder.Sql(@"
                CREATE NONCLUSTERED INDEX IX_VendorInvitations_Currency
                ON VendorInvitations(Currency_Computed)
                WHERE Currency_Computed IS NOT NULL;
            ");

            // =============================================================
            // VendorInvitations: Add CompanyCode computed column + index
            // =============================================================
            // Extracts CompanyCode from Attributes for SAP mapping
            // Used in: SAP integration, company-specific reporting
            migrationBuilder.Sql(@"
                ALTER TABLE VendorInvitations
                ADD CompanyCode_Computed AS CAST(JSON_VALUE(Attributes, '$.CompanyCode') AS NVARCHAR(10)) PERSISTED;
            ");

            migrationBuilder.Sql(@"
                CREATE NONCLUSTERED INDEX IX_VendorInvitations_CompanyCode
                ON VendorInvitations(CompanyCode_Computed)
                WHERE CompanyCode_Computed IS NOT NULL;
            ");

            // =============================================================
            // ChangeRequests: Add ChangeType computed column + index
            // =============================================================
            // Extracts ChangeType from Attributes for change categorization
            // Used in: Change management dashboards, audit reports
            migrationBuilder.Sql(@"
                ALTER TABLE ChangeRequests
                ADD ChangeType_Computed AS CAST(JSON_VALUE(Attributes, '$.ChangeType') AS NVARCHAR(50)) PERSISTED;
            ");

            migrationBuilder.Sql(@"
                CREATE NONCLUSTERED INDEX IX_ChangeRequests_ChangeType
                ON ChangeRequests(ChangeType_Computed)
                WHERE ChangeType_Computed IS NOT NULL;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove indexes first (required before dropping columns)
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS IX_ChangeRequests_ChangeType ON ChangeRequests;
                DROP INDEX IF EXISTS IX_VendorInvitations_CompanyCode ON VendorInvitations;
                DROP INDEX IF EXISTS IX_VendorInvitations_Currency ON VendorInvitations;
                DROP INDEX IF EXISTS IX_VendorApplications_AccountGroup ON VendorApplications;
                DROP INDEX IF EXISTS IX_VendorApplications_VendorType ON VendorApplications;
            ");

            // Remove computed columns
            migrationBuilder.Sql(@"
                ALTER TABLE ChangeRequests DROP COLUMN IF EXISTS ChangeType_Computed;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE VendorInvitations DROP COLUMN IF EXISTS CompanyCode_Computed;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE VendorInvitations DROP COLUMN IF EXISTS Currency_Computed;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE VendorApplications DROP COLUMN IF EXISTS AccountGroup_Computed;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE VendorApplications DROP COLUMN IF EXISTS VendorType_Computed;
            ");
        }
    }
}
