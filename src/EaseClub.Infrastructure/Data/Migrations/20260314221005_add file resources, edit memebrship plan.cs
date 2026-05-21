using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EaseClub.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class addfileresourceseditmemebrshipplan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MembershipTypes_ApplicationTemplateDefinitions_ApplicationTemplateDefinitionId",
                table: "MembershipTypes");

            migrationBuilder.DropIndex(
                name: "IX_MembershipTypes_ApplicationTemplateDefinitionId",
                table: "MembershipTypes");

            migrationBuilder.DropColumn(
                name: "ApplicationTemplateDefinitionId",
                table: "MembershipTypes");

            migrationBuilder.DropColumn(
                name: "FamilyAllowed",
                table: "MembershipTypes");

            migrationBuilder.DropColumn(
                name: "MaxFamilyMembers",
                table: "MembershipTypes");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "ApplicationStepDefinitions");

            migrationBuilder.DropColumn(
                name: "Repeat_DependsOnFieldKey",
                table: "ApplicationSectionDefinitions");

            migrationBuilder.RenameColumn(
                name: "DurationInDays",
                table: "MembershipPlans",
                newName: "MaxPaymentPeriodInDays");

            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationTemplateDefinitionId",
                table: "MembershipPlans",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxFamilyMembers",
                table: "MembershipPlans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Intent",
                table: "ApplicationSectionDefinitions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfRepeats",
                table: "ApplicationSectionDefinitions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSystemField",
                table: "ApplicationFieldDefinitions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "FamilyMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MembershipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Relationship = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    AdditionalData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyMembers_Memberships_MembershipId",
                        column: x => x.MembershipId,
                        principalTable: "Memberships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FileResources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    IsPrivate = table.Column<bool>(type: "bit", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    IsTemporary = table.Column<bool>(type: "bit", nullable: false),
                    ClubId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileResources_Clubs_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FileResources_MembershipApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "MembershipApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MembershipPlans_ApplicationTemplateDefinitionId",
                table: "MembershipPlans",
                column: "ApplicationTemplateDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyMembers_MembershipId",
                table: "FamilyMembers",
                column: "MembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_FileResources_ApplicationId",
                table: "FileResources",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_FileResources_ClubId",
                table: "FileResources",
                column: "ClubId");

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipPlans_ApplicationTemplateDefinitions_ApplicationTemplateDefinitionId",
                table: "MembershipPlans",
                column: "ApplicationTemplateDefinitionId",
                principalTable: "ApplicationTemplateDefinitions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MembershipPlans_ApplicationTemplateDefinitions_ApplicationTemplateDefinitionId",
                table: "MembershipPlans");

            migrationBuilder.DropTable(
                name: "FamilyMembers");

            migrationBuilder.DropTable(
                name: "FileResources");

            migrationBuilder.DropIndex(
                name: "IX_MembershipPlans_ApplicationTemplateDefinitionId",
                table: "MembershipPlans");

            migrationBuilder.DropColumn(
                name: "ApplicationTemplateDefinitionId",
                table: "MembershipPlans");

            migrationBuilder.DropColumn(
                name: "MaxFamilyMembers",
                table: "MembershipPlans");

            migrationBuilder.DropColumn(
                name: "Intent",
                table: "ApplicationSectionDefinitions");

            migrationBuilder.DropColumn(
                name: "NumberOfRepeats",
                table: "ApplicationSectionDefinitions");

            migrationBuilder.DropColumn(
                name: "IsSystemField",
                table: "ApplicationFieldDefinitions");

            migrationBuilder.RenameColumn(
                name: "MaxPaymentPeriodInDays",
                table: "MembershipPlans",
                newName: "DurationInDays");

            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationTemplateDefinitionId",
                table: "MembershipTypes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FamilyAllowed",
                table: "MembershipTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxFamilyMembers",
                table: "MembershipTypes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "ApplicationStepDefinitions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Repeat_DependsOnFieldKey",
                table: "ApplicationSectionDefinitions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipTypes_ApplicationTemplateDefinitionId",
                table: "MembershipTypes",
                column: "ApplicationTemplateDefinitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipTypes_ApplicationTemplateDefinitions_ApplicationTemplateDefinitionId",
                table: "MembershipTypes",
                column: "ApplicationTemplateDefinitionId",
                principalTable: "ApplicationTemplateDefinitions",
                principalColumn: "Id");
        }
    }
}
