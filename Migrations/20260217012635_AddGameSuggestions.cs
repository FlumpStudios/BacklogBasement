using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BacklogBasement.Migrations
{
    /// <inheritdoc />
    public partial class AddGameSuggestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameSuggestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SenderUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RecipientUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GameId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: true),
                    IsDismissed = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSuggestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameSuggestions_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameSuggestions_Users_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameSuggestions_Users_SenderUserId",
                        column: x => x.SenderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameSuggestions_GameId",
                table: "GameSuggestions",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_GameSuggestions_RecipientUserId_IsDismissed",
                table: "GameSuggestions",
                columns: new[] { "RecipientUserId", "IsDismissed" });

            migrationBuilder.CreateIndex(
                name: "IX_GameSuggestions_SenderUserId",
                table: "GameSuggestions",
                column: "SenderUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameSuggestions");
        }
    }
}
