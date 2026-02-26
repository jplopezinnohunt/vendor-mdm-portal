using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VendorMdm.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentRegistryAndGlobalTaxonomy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VendorDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VendorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CategoryCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BlobName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SecurityLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    UploadedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    VerifiedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CountryCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    ContentHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExtractedData = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}"),
                    Attributes = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorDocuments", x => x.Id);
                });

            // Index for vendor document lookups
            migrationBuilder.CreateIndex(
                name: "IX_VendorDocument_VendorCategory",
                table: "VendorDocuments",
                columns: new[] { "VendorId", "CategoryCode", "Status" });

            // Index for document type queries
            migrationBuilder.CreateIndex(
                name: "IX_VendorDocument_Type",
                table: "VendorDocuments",
                columns: new[] { "DocumentType", "Status" });

            // Index for expiry tracking (compliance monitoring)
            migrationBuilder.CreateIndex(
                name: "IX_VendorDocument_Expiry",
                table: "VendorDocuments",
                columns: new[] { "ExpiryDate", "Status" });

            // Index for country-specific document queries
            migrationBuilder.CreateIndex(
                name: "IX_VendorDocument_Country",
                table: "VendorDocuments",
                columns: new[] { "CountryCode", "DocumentType" });

            // Index for soft delete (Pattern 11)
            migrationBuilder.CreateIndex(
                name: "IX_VendorDocument_IsDeleted",
                table: "VendorDocuments",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VendorDocuments");
        }
    }
}
