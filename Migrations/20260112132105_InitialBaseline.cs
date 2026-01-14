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
            // No-op: Database already has these changes from manual setup.
            // This migration exists to establish a baseline for future migrations.
            // The actual changes were:
            // - Added SteamId column to Users table
            // - Added SteamAppId column to Games table
            // - Created IX_Users_SteamId unique filtered index
            // - Created IX_Games_SteamAppId unique filtered index
            // - Updated IX_Games_IgdbId to be filtered
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
