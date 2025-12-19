using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VendorMdm.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowCanonicalEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkflowDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Domain = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    EntityVersion = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SourceSystem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SchemaVersion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowFieldDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkflowStepId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FieldName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FieldType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsReadOnly = table.Column<bool>(type: "bit", nullable: false),
                    IsHidden = table.Column<bool>(type: "bit", nullable: false),
                    ValidationRegex = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityVersion = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SourceSystem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SchemaVersion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowFieldDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkflowDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StepName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    StepType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsFinal = table.Column<bool>(type: "bit", nullable: false),
                    EntityVersion = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SourceSystem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SchemaVersion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowSteps_WorkflowDefinitions_WorkflowDefinitionId",
                        column: x => x.WorkflowDefinitionId,
                        principalTable: "WorkflowDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkflowStepId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActionName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TargetStepName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ButtonLabel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ButtonStyle = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EntityVersion = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SourceSystem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SchemaVersion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowActions_WorkflowSteps_WorkflowStepId",
                        column: x => x.WorkflowStepId,
                        principalTable: "WorkflowSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowRoleBindings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkflowStepId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkflowRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EntityVersion = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SourceSystem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SchemaVersion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowRoleBindings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowRoleBindings_WorkflowSteps_WorkflowStepId",
                        column: x => x.WorkflowStepId,
                        principalTable: "WorkflowSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowActions_WorkflowStepId",
                table: "WorkflowActions",
                column: "WorkflowStepId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowFieldDefinitions_WorkflowStepId",
                table: "WorkflowFieldDefinitions",
                column: "WorkflowStepId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowRoleBindings_WorkflowStepId",
                table: "WorkflowRoleBindings",
                column: "WorkflowStepId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSteps_WorkflowDefinitionId",
                table: "WorkflowSteps",
                column: "WorkflowDefinitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkflowActions");

            migrationBuilder.DropTable(
                name: "WorkflowFieldDefinitions");

            migrationBuilder.DropTable(
                name: "WorkflowRoleBindings");

            migrationBuilder.DropTable(
                name: "WorkflowSteps");

            migrationBuilder.DropTable(
                name: "WorkflowDefinitions");
        }
    }
}
