using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BacklogBasement.Migrations
{
    /// <inheritdoc />
    public partial class AddIgdbIdChecked : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IgdbIdChecked",
                table: "Games",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "DailyQuizzes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuizDate = table.Column<string>(type: "TEXT", nullable: false),
                    QuestionType = table.Column<string>(type: "TEXT", nullable: false),
                    QuestionText = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyQuizzes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DailyQuizAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuizId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SelectedOptionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsCorrect = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyQuizAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyQuizAnswers_DailyQuizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "DailyQuizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DailyQuizAnswers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyQuizOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuizId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Text = table.Column<string>(type: "TEXT", nullable: false),
                    IsCorrect = table.Column<bool>(type: "INTEGER", nullable: false),
                    CoverUrl = table.Column<string>(type: "TEXT", nullable: true),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyQuizOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyQuizOptions_DailyQuizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "DailyQuizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyQuizAnswers_QuizId_UserId",
                table: "DailyQuizAnswers",
                columns: new[] { "QuizId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyQuizAnswers_UserId",
                table: "DailyQuizAnswers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyQuizOptions_QuizId",
                table: "DailyQuizOptions",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyQuizzes_QuizDate",
                table: "DailyQuizzes",
                column: "QuizDate",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyQuizAnswers");

            migrationBuilder.DropTable(
                name: "DailyQuizOptions");

            migrationBuilder.DropTable(
                name: "DailyQuizzes");

            migrationBuilder.DropColumn(
                name: "IgdbIdChecked",
                table: "Games");
        }
    }
}
