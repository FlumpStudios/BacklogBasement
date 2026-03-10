using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BacklogBasement.Migrations
{
    /// <inheritdoc />
    public partial class AddGamePasswords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GamePasswords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GameId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: false),
                    Label = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GamePasswords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GamePasswords_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GamePasswords_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GamePasswords_GameId",
                table: "GamePasswords",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_GamePasswords_UserId_GameId",
                table: "GamePasswords",
                columns: new[] { "UserId", "GameId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GamePasswords");
        }
    }
}
