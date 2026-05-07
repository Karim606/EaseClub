using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EaseClub.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class EventModuleRefactoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "TicketTypes");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "TicketTypes");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "Audience",
                table: "Events",
                newName: "AccessType");

            migrationBuilder.AddColumn<bool>(
                name: "IsRegistrantAttending",
                table: "EventRegistrations",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRegistrantAttending",
                table: "EventRegistrations");

            migrationBuilder.RenameColumn(
                name: "AccessType",
                table: "Events",
                newName: "Audience");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "TicketTypes",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "TicketTypes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "Events",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
