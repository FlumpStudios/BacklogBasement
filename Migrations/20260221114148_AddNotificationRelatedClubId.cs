using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BacklogBasement.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationRelatedClubId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RelatedClubId",
                table: "Notifications",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RelatedClubId",
                table: "Notifications");
        }
    }
}
