using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EaseClub.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEnrollmentModetoplanAddSupportFamilyconfigurerelationbetweenapplicationtemplatesandmembershipplans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MembershipPlans_ApplicationTemplateDefinitions_ApplicationTemplateDefinitionId",
                table: "MembershipPlans");

            migrationBuilder.RenameColumn(
                name: "ApplicationTemplateDefinitionId",
                table: "MembershipPlans",
                newName: "ApplicationTemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_MembershipPlans_ApplicationTemplateDefinitionId",
                table: "MembershipPlans",
                newName: "IX_MembershipPlans_ApplicationTemplateId");

            migrationBuilder.AddColumn<int>(
                name: "EnrollmentMode",
                table: "MembershipPlans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "InstallmentTemplateId",
                table: "MembershipInstallments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "InstallmentTemplateId",
                table: "MembershipApplications",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<bool>(
                name: "SupportsFamilyPlans",
                table: "ApplicationTemplateDefinitions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipPlans_ApplicationTemplateDefinitions_ApplicationTemplateId",
                table: "MembershipPlans",
                column: "ApplicationTemplateId",
                principalTable: "ApplicationTemplateDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MembershipPlans_ApplicationTemplateDefinitions_ApplicationTemplateId",
                table: "MembershipPlans");

            migrationBuilder.DropColumn(
                name: "EnrollmentMode",
                table: "MembershipPlans");

            migrationBuilder.DropColumn(
                name: "SupportsFamilyPlans",
                table: "ApplicationTemplateDefinitions");

            migrationBuilder.RenameColumn(
                name: "ApplicationTemplateId",
                table: "MembershipPlans",
                newName: "ApplicationTemplateDefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_MembershipPlans_ApplicationTemplateId",
                table: "MembershipPlans",
                newName: "IX_MembershipPlans_ApplicationTemplateDefinitionId");

            migrationBuilder.AlterColumn<Guid>(
                name: "InstallmentTemplateId",
                table: "MembershipInstallments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "InstallmentTemplateId",
                table: "MembershipApplications",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipPlans_ApplicationTemplateDefinitions_ApplicationTemplateDefinitionId",
                table: "MembershipPlans",
                column: "ApplicationTemplateDefinitionId",
                principalTable: "ApplicationTemplateDefinitions",
                principalColumn: "Id");
        }
    }
}
