using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VendorMdm.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCanonicalModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LinkedEntityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    BlobUrl = table.Column<string>(type: "TEXT", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Attributes = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChangeRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SapVendorId = table.Column<string>(type: "TEXT", nullable: true),
                    RequesterId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Attributes = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChangeRequestsCanonical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    VendorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RequesterId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EntityVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SourceSystem = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}"),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SchemaVersion = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeRequestsCanonical", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExternalSystemMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CanonicalEntityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EntityType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ExternalSystemId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SystemName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SystemEnvironment = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalSystemMappings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SapEnvironments",
                columns: table => new
                {
                    EnvironmentCode = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SapEnvironments", x => x.EnvironmentCode);
                });

            migrationBuilder.CreateTable(
                name: "UsersAndRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: false),
                    Attributes = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersAndRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VendorApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CompanyName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TaxId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ContactName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ContactEmail = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    RegistrationType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    InvitationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Attributes = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorApplications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VendorInvitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    InvitationToken = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    VendorLegalName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    PrimaryContactEmail = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    InvitedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    InvitedByName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    VendorApplicationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Attributes = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorInvitations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VendorInvitationsCanonical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    InvitationToken = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    VendorLegalName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    PrimaryContactEmail = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    InvitedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    InvitedByName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    VendorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    EntityVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SourceSystem = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}"),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SchemaVersion = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorInvitationsCanonical", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vendors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LegalName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TaxId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    PrimaryContactEmail = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    EntityVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SourceSystem = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}"),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SchemaVersion = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowStates",
                columns: table => new
                {
                    StateName = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Attributes = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowStates", x => x.StateName);
                });

            migrationBuilder.InsertData(
                table: "SapEnvironments",
                columns: new[] { "EnvironmentCode", "Description" },
                values: new object[,]
                {
                    { "D01", "Development" },
                    { "P01", "Production" },
                    { "Q01", "Quality Assurance" }
                });

            migrationBuilder.InsertData(
                table: "WorkflowStates",
                columns: new[] { "StateName", "Attributes", "Description" },
                values: new object[,]
                {
                    { "Approved", "{}", "Approved by admin" },
                    { "Draft", "{}", "Initial draft" },
                    { "Integrated", "{}", "Synced to SAP" },
                    { "Submitted", "{}", "Submitted for approval" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChangeRequestsCanonical_RequesterId",
                table: "ChangeRequestsCanonical",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeRequestsCanonical_Status",
                table: "ChangeRequestsCanonical",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeRequestsCanonical_VendorId",
                table: "ChangeRequestsCanonical",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalSystemMapping_Canonical",
                table: "ExternalSystemMappings",
                columns: new[] { "CanonicalEntityId", "EntityType", "SystemName" });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalSystemMapping_Unique",
                table: "ExternalSystemMappings",
                columns: new[] { "EntityType", "ExternalSystemId", "SystemName", "SystemEnvironment" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendorInvitationsCanonical_ExpiresAt",
                table: "VendorInvitationsCanonical",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_VendorInvitationsCanonical_InvitationToken",
                table: "VendorInvitationsCanonical",
                column: "InvitationToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendorInvitationsCanonical_PrimaryContactEmail",
                table: "VendorInvitationsCanonical",
                column: "PrimaryContactEmail");

            migrationBuilder.CreateIndex(
                name: "IX_VendorInvitationsCanonical_Status",
                table: "VendorInvitationsCanonical",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_LegalName",
                table: "Vendors",
                column: "LegalName");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_PrimaryContactEmail",
                table: "Vendors",
                column: "PrimaryContactEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_SourceSystem",
                table: "Vendors",
                column: "SourceSystem");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_Status",
                table: "Vendors",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropTable(
                name: "ChangeRequests");

            migrationBuilder.DropTable(
                name: "ChangeRequestsCanonical");

            migrationBuilder.DropTable(
                name: "ExternalSystemMappings");

            migrationBuilder.DropTable(
                name: "SapEnvironments");

            migrationBuilder.DropTable(
                name: "UsersAndRoles");

            migrationBuilder.DropTable(
                name: "VendorApplications");

            migrationBuilder.DropTable(
                name: "VendorInvitations");

            migrationBuilder.DropTable(
                name: "VendorInvitationsCanonical");

            migrationBuilder.DropTable(
                name: "Vendors");

            migrationBuilder.DropTable(
                name: "WorkflowStates");
        }
    }
}
