using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BacklogBasement.Migrations
{
    /// <inheritdoc />
    public partial class InitialBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add SteamId column to Users table
            migrationBuilder.AddColumn<string>(
                name: "SteamId",
                table: "Users",
                type: "TEXT",
                nullable: true);

            // Add SteamAppId column to Games table
            migrationBuilder.AddColumn<long>(
                name: "SteamAppId",
                table: "Games",
                type: "INTEGER",
                nullable: true);

            // Create unique index on SteamId (filtered for non-null values)
            migrationBuilder.CreateIndex(
                name: "IX_Users_SteamId",
                table: "Users",
                column: "SteamId",
                unique: true,
                filter: "[SteamId] IS NOT NULL");

            // Create unique index on SteamAppId (filtered for non-null values)
            migrationBuilder.CreateIndex(
                name: "IX_Games_SteamAppId",
                table: "Games",
                column: "SteamAppId",
                unique: true,
                filter: "[SteamAppId] IS NOT NULL");

            // Update IgdbId index to be filtered (allow null values)
            migrationBuilder.DropIndex(
                name: "IX_Games_IgdbId",
                table: "Games");

            migrationBuilder.CreateIndex(
                name: "IX_Games_IgdbId",
                table: "Games",
                column: "IgdbId",
                unique: true,
                filter: "[IgdbId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Games_IgdbId",
                table: "Games");

            migrationBuilder.CreateIndex(
                name: "IX_Games_IgdbId",
                table: "Games",
                column: "IgdbId",
                unique: true);

            migrationBuilder.DropIndex(
                name: "IX_Games_SteamAppId",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_Users_SteamId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SteamAppId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "SteamId",
                table: "Users");
        }
    }
}
