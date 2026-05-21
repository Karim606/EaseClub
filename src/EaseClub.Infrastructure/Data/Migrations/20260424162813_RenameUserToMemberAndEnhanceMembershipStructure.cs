using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EaseClub.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameUserToMemberAndEnhanceMembershipStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_MemberUsers_UserId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_MembershipApplications_MemberUsers_UserId",
                table: "MembershipApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_MembershipCycle_Memberships_MembershipId",
                table: "MembershipCycle");

            migrationBuilder.DropForeignKey(
                name: "FK_MembershipInstallments_MembershipCycle_MembershipCycleId",
                table: "MembershipInstallments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MembershipCycle",
                table: "MembershipCycle");

            migrationBuilder.RenameTable(
                name: "MembershipCycle",
                newName: "MembershipCycles");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Memberships",
                newName: "MemberId");

            migrationBuilder.RenameIndex(
                name: "IX_Memberships_MembershipTypeId_ClubId_UserId",
                table: "Memberships",
                newName: "IX_Memberships_MembershipTypeId_ClubId_MemberId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "MembershipApplications",
                newName: "MemberId");

            migrationBuilder.RenameIndex(
                name: "IX_MembershipApplications_UserId",
                table: "MembershipApplications",
                newName: "IX_MembershipApplications_MemberId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Invoices",
                newName: "MemberId");

            migrationBuilder.RenameIndex(
                name: "IX_Invoices_UserId",
                table: "Invoices",
                newName: "IX_Invoices_MemberId");

            migrationBuilder.RenameIndex(
                name: "IX_MembershipCycle_MembershipId",
                table: "MembershipCycles",
                newName: "IX_MembershipCycles_MembershipId");

            migrationBuilder.CreateSequence<int>(
                name: "MembershipSequence");

            migrationBuilder.AddColumn<string>(
                name: "MembershipNumber",
                table: "Memberships",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "MembershipId",
                table: "MembershipInstallments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Clubs",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Branches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MembershipCycles",
                table: "MembershipCycles",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_MemberId",
                table: "Memberships",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_MembershipNumber",
                table: "Memberships",
                column: "MembershipNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clubs_Code",
                table: "Clubs",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_MemberUsers_MemberId",
                table: "Invoices",
                column: "MemberId",
                principalTable: "MemberUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipApplications_Clubs_ClubId",
                table: "MembershipApplications",
                column: "ClubId",
                principalTable: "Clubs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipApplications_MemberUsers_MemberId",
                table: "MembershipApplications",
                column: "MemberId",
                principalTable: "MemberUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipCycles_Memberships_MembershipId",
                table: "MembershipCycles",
                column: "MembershipId",
                principalTable: "Memberships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipInstallments_MembershipCycles_MembershipCycleId",
                table: "MembershipInstallments",
                column: "MembershipCycleId",
                principalTable: "MembershipCycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_MemberUsers_MemberId",
                table: "Memberships",
                column: "MemberId",
                principalTable: "MemberUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_MemberUsers_MemberId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_MembershipApplications_Clubs_ClubId",
                table: "MembershipApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_MembershipApplications_MemberUsers_MemberId",
                table: "MembershipApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_MembershipCycles_Memberships_MembershipId",
                table: "MembershipCycles");

            migrationBuilder.DropForeignKey(
                name: "FK_MembershipInstallments_MembershipCycles_MembershipCycleId",
                table: "MembershipInstallments");

            migrationBuilder.DropForeignKey(
                name: "FK_Memberships_MemberUsers_MemberId",
                table: "Memberships");

            migrationBuilder.DropIndex(
                name: "IX_Memberships_MemberId",
                table: "Memberships");

            migrationBuilder.DropIndex(
                name: "IX_Memberships_MembershipNumber",
                table: "Memberships");

            migrationBuilder.DropIndex(
                name: "IX_Clubs_Code",
                table: "Clubs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MembershipCycles",
                table: "MembershipCycles");

            migrationBuilder.DropColumn(
                name: "MembershipNumber",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "MembershipId",
                table: "MembershipInstallments");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Clubs");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Branches");

            migrationBuilder.DropSequence(
                name: "MembershipSequence");

            migrationBuilder.RenameTable(
                name: "MembershipCycles",
                newName: "MembershipCycle");

            migrationBuilder.RenameColumn(
                name: "MemberId",
                table: "Memberships",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Memberships_MembershipTypeId_ClubId_MemberId",
                table: "Memberships",
                newName: "IX_Memberships_MembershipTypeId_ClubId_UserId");

            migrationBuilder.RenameColumn(
                name: "MemberId",
                table: "MembershipApplications",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_MembershipApplications_MemberId",
                table: "MembershipApplications",
                newName: "IX_MembershipApplications_UserId");

            migrationBuilder.RenameColumn(
                name: "MemberId",
                table: "Invoices",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Invoices_MemberId",
                table: "Invoices",
                newName: "IX_Invoices_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_MembershipCycles_MembershipId",
                table: "MembershipCycle",
                newName: "IX_MembershipCycle_MembershipId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MembershipCycle",
                table: "MembershipCycle",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_MemberUsers_UserId",
                table: "Invoices",
                column: "UserId",
                principalTable: "MemberUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipApplications_MemberUsers_UserId",
                table: "MembershipApplications",
                column: "UserId",
                principalTable: "MemberUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipCycle_Memberships_MembershipId",
                table: "MembershipCycle",
                column: "MembershipId",
                principalTable: "Memberships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipInstallments_MembershipCycle_MembershipCycleId",
                table: "MembershipInstallments",
                column: "MembershipCycleId",
                principalTable: "MembershipCycle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
