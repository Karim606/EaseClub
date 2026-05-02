using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EaseClub.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistrationPricingAndAttendeeDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppliedPolicies",
                table: "EventRegistrations",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "EventRegistrations",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalTotal",
                table: "EventRegistrations",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "EventAttendees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "EventAttendees",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppliedPolicies",
                table: "EventRegistrations");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "EventRegistrations");

            migrationBuilder.DropColumn(
                name: "FinalTotal",
                table: "EventRegistrations");

            migrationBuilder.DropColumn(
                name: "Age",
                table: "EventAttendees");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "EventAttendees");
        }
    }
}
