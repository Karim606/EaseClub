using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EaseClub.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMembershipSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationTemplateDefinitionId",
                table: "MembershipTypes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubscriptionValidityInYears",
                table: "MembershipPlans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "InstallmentTemplateId",
                table: "MembershipInstallments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "MembershipPlanId",
                table: "MembershipInstallments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "MembershipTypeId",
                table: "MembershipInstallments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "InstallmentTemplateId",
                table: "MembershipApplications",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "ApplicationReviews",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Memberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClubId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MembershipTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MembershipPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Period_StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Period_EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MembershipApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExtraDataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Memberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Memberships_Clubs_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Memberships_MembershipPlans_MembershipPlanId",
                        column: x => x.MembershipPlanId,
                        principalTable: "MembershipPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Memberships_MembershipTypes_MembershipTypeId",
                        column: x => x.MembershipTypeId,
                        principalTable: "MembershipTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MembershipTypes_ApplicationTemplateDefinitionId",
                table: "MembershipTypes",
                column: "ApplicationTemplateDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipInstallments_DueDate",
                table: "MembershipInstallments",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipInstallments_MembershipId_Order",
                table: "MembershipInstallments",
                columns: new[] { "MembershipId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipInstallments_MembershipId_Status",
                table: "MembershipInstallments",
                columns: new[] { "MembershipId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_MembershipApplications_MembershipPlanId",
                table: "MembershipApplications",
                column: "MembershipPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipApplications_MembershipTypeId",
                table: "MembershipApplications",
                column: "MembershipTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_ClubId",
                table: "Memberships",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_MembershipPlanId",
                table: "Memberships",
                column: "MembershipPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_MembershipTypeId_ClubId_UserId",
                table: "Memberships",
                columns: new[] { "MembershipTypeId", "ClubId", "UserId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipApplications_MembershipPlans_MembershipPlanId",
                table: "MembershipApplications",
                column: "MembershipPlanId",
                principalTable: "MembershipPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipApplications_MembershipTypes_MembershipTypeId",
                table: "MembershipApplications",
                column: "MembershipTypeId",
                principalTable: "MembershipTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipInstallments_Memberships_MembershipId",
                table: "MembershipInstallments",
                column: "MembershipId",
                principalTable: "Memberships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipTypes_ApplicationTemplateDefinitions_ApplicationTemplateDefinitionId",
                table: "MembershipTypes",
                column: "ApplicationTemplateDefinitionId",
                principalTable: "ApplicationTemplateDefinitions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MembershipApplications_MembershipPlans_MembershipPlanId",
                table: "MembershipApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_MembershipApplications_MembershipTypes_MembershipTypeId",
                table: "MembershipApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_MembershipInstallments_Memberships_MembershipId",
                table: "MembershipInstallments");

            migrationBuilder.DropForeignKey(
                name: "FK_MembershipTypes_ApplicationTemplateDefinitions_ApplicationTemplateDefinitionId",
                table: "MembershipTypes");

            migrationBuilder.DropTable(
                name: "Memberships");

            migrationBuilder.DropIndex(
                name: "IX_MembershipTypes_ApplicationTemplateDefinitionId",
                table: "MembershipTypes");

            migrationBuilder.DropIndex(
                name: "IX_MembershipInstallments_DueDate",
                table: "MembershipInstallments");

            migrationBuilder.DropIndex(
                name: "IX_MembershipInstallments_MembershipId_Order",
                table: "MembershipInstallments");

            migrationBuilder.DropIndex(
                name: "IX_MembershipInstallments_MembershipId_Status",
                table: "MembershipInstallments");

            migrationBuilder.DropIndex(
                name: "IX_MembershipApplications_MembershipPlanId",
                table: "MembershipApplications");

            migrationBuilder.DropIndex(
                name: "IX_MembershipApplications_MembershipTypeId",
                table: "MembershipApplications");

            migrationBuilder.DropColumn(
                name: "ApplicationTemplateDefinitionId",
                table: "MembershipTypes");

            migrationBuilder.DropColumn(
                name: "SubscriptionValidityInYears",
                table: "MembershipPlans");

            migrationBuilder.DropColumn(
                name: "InstallmentTemplateId",
                table: "MembershipInstallments");

            migrationBuilder.DropColumn(
                name: "MembershipPlanId",
                table: "MembershipInstallments");

            migrationBuilder.DropColumn(
                name: "MembershipTypeId",
                table: "MembershipInstallments");

            migrationBuilder.DropColumn(
                name: "InstallmentTemplateId",
                table: "MembershipApplications");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "ApplicationReviews");
        }
    }
}
