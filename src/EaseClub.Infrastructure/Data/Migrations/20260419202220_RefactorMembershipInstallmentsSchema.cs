using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EaseClub.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RefactorMembershipInstallmentsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MembershipInstallments_MembershipCycle_MembershipId",
                table: "MembershipInstallments");

            migrationBuilder.DropIndex(
                name: "IX_MembershipInstallments_MembershipId",
                table: "MembershipInstallments");

            migrationBuilder.DropColumn(
                name: "MembershipId",
                table: "MembershipInstallments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MembershipId",
                table: "MembershipInstallments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_MembershipInstallments_MembershipId",
                table: "MembershipInstallments",
                column: "MembershipId");

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipInstallments_MembershipCycle_MembershipId",
                table: "MembershipInstallments",
                column: "MembershipId",
                principalTable: "MembershipCycle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
