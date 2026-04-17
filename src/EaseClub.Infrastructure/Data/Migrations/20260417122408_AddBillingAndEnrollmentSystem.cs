using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EaseClub.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBillingAndEnrollmentSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileResources_Clubs_ClubId",
                table: "FileResources");

            migrationBuilder.DropForeignKey(
                name: "FK_FileResources_MembershipApplications_ApplicationId",
                table: "FileResources");

            migrationBuilder.DropForeignKey(
                name: "FK_MembershipInstallments_Memberships_MembershipId",
                table: "MembershipInstallments");

            migrationBuilder.DropIndex(
                name: "IX_MembershipInstallments_MembershipId_Order",
                table: "MembershipInstallments");

            migrationBuilder.DropIndex(
                name: "IX_MembershipInstallments_MembershipId_Status",
                table: "MembershipInstallments");

            migrationBuilder.DropIndex(
                name: "IX_FileResources_ApplicationId",
                table: "FileResources");

            migrationBuilder.DropIndex(
                name: "IX_FileResources_ClubId",
                table: "FileResources");

            migrationBuilder.DropColumn(
                name: "Period_EndDate",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "Period_StartDate",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "ApplicationId",
                table: "FileResources");

            migrationBuilder.DropColumn(
                name: "ClubId",
                table: "FileResources");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "FileResources",
                newName: "Purpose");

            migrationBuilder.AlterColumn<string>(
                name: "EnrollmentMode",
                table: "MembershipPlans",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "InstallmentsAllowdInRenewal",
                table: "MembershipPlans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMode",
                table: "MembershipPlans",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "RenewPrice",
                table: "MembershipPlans",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "MembershipCycleId",
                table: "MembershipInstallments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "ReadableId",
                table: "MembershipInstallments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "FileResources",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "OwnerType",
                table: "FileResources",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "About",
                table: "Clubs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Amenities",
                table: "Clubs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "Clubs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "Clubs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CoverImageUrl",
                table: "Clubs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Clubs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkSchedules",
                table: "Clubs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "FieldType",
                table: "ApplicationAnswers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BillingItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BillingItemReadableId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillingItemType = table.Column<int>(type: "int", nullable: false),
                    ClubId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReadableId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Clubs_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_MemberUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "MemberUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MembershipCycle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MembershipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipCycle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MembershipCycle_Memberships_MembershipId",
                        column: x => x.MembershipId,
                        principalTable: "Memberships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PendingEnrollments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClubId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MembershipTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MembershipPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MembershipApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InstallmentTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubscriptionValidityInYears = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReadableId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ExistingMembershipId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FirstInvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InstallmentsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingEnrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PendingEnrollments_Clubs_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PendingEnrollments_MemberUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "MemberUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExternalRef = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Gateway = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Method = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MembershipInstallments_MembershipCycleId",
                table: "MembershipInstallments",
                column: "MembershipCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipInstallments_MembershipCycleId_Order",
                table: "MembershipInstallments",
                columns: new[] { "MembershipCycleId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ClubId",
                table: "Invoices",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_UserId",
                table: "Invoices",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipCycle_MembershipId",
                table: "MembershipCycle",
                column: "MembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_InvoiceId_ExternalRef",
                table: "PaymentTransactions",
                columns: new[] { "InvoiceId", "ExternalRef" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PendingEnrollments_ClubId",
                table: "PendingEnrollments",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingEnrollments_FirstInvoiceId",
                table: "PendingEnrollments",
                column: "FirstInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingEnrollments_MembershipApplicationId",
                table: "PendingEnrollments",
                column: "MembershipApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingEnrollments_UserId",
                table: "PendingEnrollments",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipInstallments_MembershipCycle_MembershipCycleId",
                table: "MembershipInstallments",
                column: "MembershipCycleId",
                principalTable: "MembershipCycle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipInstallments_MembershipCycle_MembershipId",
                table: "MembershipInstallments",
                column: "MembershipId",
                principalTable: "MembershipCycle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MembershipInstallments_MembershipCycle_MembershipCycleId",
                table: "MembershipInstallments");

            migrationBuilder.DropForeignKey(
                name: "FK_MembershipInstallments_MembershipCycle_MembershipId",
                table: "MembershipInstallments");

            migrationBuilder.DropTable(
                name: "MembershipCycle");

            migrationBuilder.DropTable(
                name: "PaymentTransactions");

            migrationBuilder.DropTable(
                name: "PendingEnrollments");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_MembershipInstallments_MembershipCycleId",
                table: "MembershipInstallments");

            migrationBuilder.DropIndex(
                name: "IX_MembershipInstallments_MembershipCycleId_Order",
                table: "MembershipInstallments");

            migrationBuilder.DropColumn(
                name: "InstallmentsAllowdInRenewal",
                table: "MembershipPlans");

            migrationBuilder.DropColumn(
                name: "PaymentMode",
                table: "MembershipPlans");

            migrationBuilder.DropColumn(
                name: "RenewPrice",
                table: "MembershipPlans");

            migrationBuilder.DropColumn(
                name: "MembershipCycleId",
                table: "MembershipInstallments");

            migrationBuilder.DropColumn(
                name: "ReadableId",
                table: "MembershipInstallments");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "FileResources");

            migrationBuilder.DropColumn(
                name: "OwnerType",
                table: "FileResources");

            migrationBuilder.DropColumn(
                name: "About",
                table: "Clubs");

            migrationBuilder.DropColumn(
                name: "Amenities",
                table: "Clubs");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "Clubs");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "Clubs");

            migrationBuilder.DropColumn(
                name: "CoverImageUrl",
                table: "Clubs");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Clubs");

            migrationBuilder.DropColumn(
                name: "WorkSchedules",
                table: "Clubs");

            migrationBuilder.DropColumn(
                name: "FieldType",
                table: "ApplicationAnswers");

            migrationBuilder.RenameColumn(
                name: "Purpose",
                table: "FileResources",
                newName: "Category");

            migrationBuilder.AddColumn<DateTime>(
                name: "Period_EndDate",
                table: "Memberships",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "Period_StartDate",
                table: "Memberships",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<int>(
                name: "EnrollmentMode",
                table: "MembershipPlans",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationId",
                table: "FileResources",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClubId",
                table: "FileResources",
                type: "uniqueidentifier",
                nullable: true);

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
                name: "IX_FileResources_ApplicationId",
                table: "FileResources",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_FileResources_ClubId",
                table: "FileResources",
                column: "ClubId");

            migrationBuilder.AddForeignKey(
                name: "FK_FileResources_Clubs_ClubId",
                table: "FileResources",
                column: "ClubId",
                principalTable: "Clubs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_FileResources_MembershipApplications_ApplicationId",
                table: "FileResources",
                column: "ApplicationId",
                principalTable: "MembershipApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipInstallments_Memberships_MembershipId",
                table: "MembershipInstallments",
                column: "MembershipId",
                principalTable: "Memberships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
