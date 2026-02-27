using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BacklogBasement.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyPoll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyPolls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PollDate = table.Column<string>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyPolls", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DailyPollGames",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PollId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GameId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyPollGames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyPollGames_DailyPolls_PollId",
                        column: x => x.PollId,
                        principalTable: "DailyPolls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DailyPollGames_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DailyPollVotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PollId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VotedGameId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyPollVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyPollVotes_DailyPolls_PollId",
                        column: x => x.PollId,
                        principalTable: "DailyPolls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DailyPollVotes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyPollGames_GameId",
                table: "DailyPollGames",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyPollGames_PollId",
                table: "DailyPollGames",
                column: "PollId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyPolls_PollDate",
                table: "DailyPolls",
                column: "PollDate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyPollVotes_PollId_UserId",
                table: "DailyPollVotes",
                columns: new[] { "PollId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyPollVotes_UserId",
                table: "DailyPollVotes",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyPollGames");

            migrationBuilder.DropTable(
                name: "DailyPollVotes");

            migrationBuilder.DropTable(
                name: "DailyPolls");
        }
    }
}
