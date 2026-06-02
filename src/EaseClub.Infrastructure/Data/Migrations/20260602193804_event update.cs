using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EaseClub.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class eventupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GenderRestriction",
                table: "TicketTypes");

            migrationBuilder.DropColumn(
                name: "MaxAge",
                table: "TicketTypes");

            migrationBuilder.DropColumn(
                name: "MinAge",
                table: "TicketTypes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GenderRestriction",
                table: "TicketTypes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxAge",
                table: "TicketTypes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinAge",
                table: "TicketTypes",
                type: "int",
                nullable: true);
        }
    }
}
