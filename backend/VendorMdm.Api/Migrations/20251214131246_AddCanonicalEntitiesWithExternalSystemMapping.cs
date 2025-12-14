using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VendorMdm.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCanonicalEntitiesWithExternalSystemMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SapIdMappings");

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

            migrationBuilder.CreateIndex(
                name: "IX_ExternalSystemMapping_Canonical",
                table: "ExternalSystemMappings",
                columns: new[] { "CanonicalEntityId", "EntityType", "SystemName" });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalSystemMapping_Unique",
                table: "ExternalSystemMappings",
                columns: new[] { "EntityType", "ExternalSystemId", "SystemName", "SystemEnvironment" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalSystemMappings");

            migrationBuilder.CreateTable(
                name: "SapIdMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CanonicalEntityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EntityType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SapEnvironment = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    SapId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SapIdMappings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SapIdMappings_CanonicalEntityId_EntityType",
                table: "SapIdMappings",
                columns: new[] { "CanonicalEntityId", "EntityType" });

            migrationBuilder.CreateIndex(
                name: "IX_SapIdMappings_EntityType_SapId_SapEnvironment",
                table: "SapIdMappings",
                columns: new[] { "EntityType", "SapId", "SapEnvironment" },
                unique: true);
        }
    }
}
