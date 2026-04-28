using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EaseClub.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RefactorApplicationStructureToJsonStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationAnswers");

            migrationBuilder.DropTable(
                name: "ApplicationFieldDefinitions");

            migrationBuilder.DropTable(
                name: "ApplicationSectionDefinitions");

            migrationBuilder.DropTable(
                name: "ApplicationStepDefinitions");

            migrationBuilder.AddColumn<string>(
                name: "Answers",
                table: "MembershipApplications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Steps",
                table: "ApplicationTemplateDefinitions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "ApplicationReviews",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Answers",
                table: "MembershipApplications");

            migrationBuilder.DropColumn(
                name: "Steps",
                table: "ApplicationTemplateDefinitions");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "ApplicationReviews",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ApplicationAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FieldDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FieldKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FieldType = table.Column<int>(type: "int", nullable: false),
                    InstanceIndex = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationAnswers_MembershipApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "MembershipApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationStepDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationStepDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationStepDefinitions_ApplicationTemplateDefinitions_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "ApplicationTemplateDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationSectionDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StepId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Intent = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Repeat_Mode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumberOfRepeats = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationSectionDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationSectionDefinitions_ApplicationStepDefinitions_StepId",
                        column: x => x.StepId,
                        principalTable: "ApplicationStepDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationFieldDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AllowedValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsSystemField = table.Column<bool>(type: "bit", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    PersistToMembership = table.Column<bool>(type: "bit", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ValidationRules = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VisibilityCondition = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationFieldDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationFieldDefinitions_ApplicationSectionDefinitions_SectionId",
                        column: x => x.SectionId,
                        principalTable: "ApplicationSectionDefinitions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationFieldDefinitions_ApplicationTemplateDefinitions_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "ApplicationTemplateDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationAnswers_ApplicationId",
                table: "ApplicationAnswers",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationAnswers_ApplicationId_FieldDefinitionId_InstanceIndex",
                table: "ApplicationAnswers",
                columns: new[] { "ApplicationId", "FieldDefinitionId", "InstanceIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationFieldDefinitions_SectionId_Order",
                table: "ApplicationFieldDefinitions",
                columns: new[] { "SectionId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationFieldDefinitions_TemplateId_Key",
                table: "ApplicationFieldDefinitions",
                columns: new[] { "TemplateId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationSectionDefinitions_StepId_Order",
                table: "ApplicationSectionDefinitions",
                columns: new[] { "StepId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationStepDefinitions_TemplateId_Order",
                table: "ApplicationStepDefinitions",
                columns: new[] { "TemplateId", "Order" });
        }
    }
}
