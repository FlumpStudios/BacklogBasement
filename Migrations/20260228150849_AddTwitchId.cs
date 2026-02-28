using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BacklogBasement.Migrations
{
    /// <inheritdoc />
    public partial class AddTwitchId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TwitchId",
                table: "Users",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TwitchId",
                table: "Users");
        }
    }
}
