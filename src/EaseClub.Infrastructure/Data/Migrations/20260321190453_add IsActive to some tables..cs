using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EaseClub.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class addIsActivetosometables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "InstallmentTemplates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipApplications_MemberUsers_UserId",
                table: "MembershipApplications",
                column: "UserId",
                principalTable: "MemberUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MembershipApplications_MemberUsers_UserId",
                table: "MembershipApplications");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "InstallmentTemplates");
        }
    }
}
