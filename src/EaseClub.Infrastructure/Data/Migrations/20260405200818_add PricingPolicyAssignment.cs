using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EaseClub.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class addPricingPolicyAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Priority",
                table: "PricingPolicies");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "PricingPolicies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PricingPolicyAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClubId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    TargetType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ApplicationTemplateDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingPolicyAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PricingPolicyAssignments_ApplicationTemplateDefinitions_ApplicationTemplateDefinitionId",
                        column: x => x.ApplicationTemplateDefinitionId,
                        principalTable: "ApplicationTemplateDefinitions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PricingPolicyAssignments_PricingPolicies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "PricingPolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PricingPolicyAssignments_ApplicationTemplateDefinitionId",
                table: "PricingPolicyAssignments",
                column: "ApplicationTemplateDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_PricingPolicyAssignments_PolicyId",
                table: "PricingPolicyAssignments",
                column: "PolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_PricingPolicyAssignments_TargetId_PolicyId_TargetType",
                table: "PricingPolicyAssignments",
                columns: new[] { "TargetId", "PolicyId", "TargetType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PricingPolicyAssignments");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "PricingPolicies");

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "PricingPolicies",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
