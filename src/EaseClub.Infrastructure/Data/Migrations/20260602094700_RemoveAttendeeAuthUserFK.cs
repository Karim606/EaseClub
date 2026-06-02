using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EaseClub.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAttendeeAuthUserFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventAttendees_AuthUsers_AttendeeId",
                table: "EventAttendees");

            migrationBuilder.DropIndex(
                name: "IX_EventAttendees_AttendeeId",
                table: "EventAttendees");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_EventAttendees_AttendeeId",
                table: "EventAttendees",
                column: "AttendeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventAttendees_AuthUsers_AttendeeId",
                table: "EventAttendees",
                column: "AttendeeId",
                principalTable: "AuthUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
