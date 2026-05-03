using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EaseClub.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTicketTypeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "TicketTypes",
                newName: "TotalQuantity");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "TicketTypes",
                newName: "BasePrice");

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

            migrationBuilder.AddColumn<bool>(
                name: "RequiresMembership",
                table: "TicketTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "RequiresMembership",
                table: "TicketTypes");

            migrationBuilder.RenameColumn(
                name: "TotalQuantity",
                table: "TicketTypes",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "BasePrice",
                table: "TicketTypes",
                newName: "Price");
        }
    }
}
