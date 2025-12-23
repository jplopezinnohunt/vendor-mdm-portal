using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VendorMdm.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorInvitationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountGroup",
                table: "VendorInvitations",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReviewStatus",
                table: "VendorInvitations",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "SanctionsScore",
                table: "VendorInvitations",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SanctionsStatus",
                table: "VendorInvitations",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VendorType",
                table: "VendorInvitations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountGroup",
                table: "VendorInvitations");

            migrationBuilder.DropColumn(
                name: "ReviewStatus",
                table: "VendorInvitations");

            migrationBuilder.DropColumn(
                name: "SanctionsScore",
                table: "VendorInvitations");

            migrationBuilder.DropColumn(
                name: "SanctionsStatus",
                table: "VendorInvitations");

            migrationBuilder.DropColumn(
                name: "VendorType",
                table: "VendorInvitations");
        }
    }
}
