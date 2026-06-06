using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EaseClub.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class TransitionClubAssetsToNavigationProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverImageUrl",
                table: "Clubs");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Clubs");

            migrationBuilder.AddColumn<Guid>(
                name: "CoverImageId",
                table: "Clubs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LogoId",
                table: "Clubs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clubs_CoverImageId",
                table: "Clubs",
                column: "CoverImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Clubs_LogoId",
                table: "Clubs",
                column: "LogoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clubs_FileResources_CoverImageId",
                table: "Clubs",
                column: "CoverImageId",
                principalTable: "FileResources",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Clubs_FileResources_LogoId",
                table: "Clubs",
                column: "LogoId",
                principalTable: "FileResources",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clubs_FileResources_CoverImageId",
                table: "Clubs");

            migrationBuilder.DropForeignKey(
                name: "FK_Clubs_FileResources_LogoId",
                table: "Clubs");

            migrationBuilder.DropIndex(
                name: "IX_Clubs_CoverImageId",
                table: "Clubs");

            migrationBuilder.DropIndex(
                name: "IX_Clubs_LogoId",
                table: "Clubs");

            migrationBuilder.DropColumn(
                name: "CoverImageId",
                table: "Clubs");

            migrationBuilder.DropColumn(
                name: "LogoId",
                table: "Clubs");

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
        }
    }
}
