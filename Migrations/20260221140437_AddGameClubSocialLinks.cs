using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BacklogBasement.Migrations
{
    /// <inheritdoc />
    public partial class AddGameClubSocialLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DiscordLink",
                table: "GameClubs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RedditLink",
                table: "GameClubs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsAppLink",
                table: "GameClubs",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscordLink",
                table: "GameClubs");

            migrationBuilder.DropColumn(
                name: "RedditLink",
                table: "GameClubs");

            migrationBuilder.DropColumn(
                name: "WhatsAppLink",
                table: "GameClubs");
        }
    }
}
