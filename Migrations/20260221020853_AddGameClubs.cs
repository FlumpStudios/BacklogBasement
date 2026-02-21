using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BacklogBasement.Migrations
{
    /// <inheritdoc />
    public partial class AddGameClubs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameClubs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IsPublic = table.Column<bool>(type: "INTEGER", nullable: false),
                    OwnerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameClubs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameClubs_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GameClubInvites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClubId = table.Column<Guid>(type: "TEXT", nullable: false),
                    InvitedByUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    InviteeUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameClubInvites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameClubInvites_GameClubs_ClubId",
                        column: x => x.ClubId,
                        principalTable: "GameClubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameClubInvites_Users_InvitedByUserId",
                        column: x => x.InvitedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameClubInvites_Users_InviteeUserId",
                        column: x => x.InviteeUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GameClubMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClubId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameClubMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameClubMembers_GameClubs_ClubId",
                        column: x => x.ClubId,
                        principalTable: "GameClubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameClubMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameClubRounds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClubId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoundNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    GameId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    NominatingDeadline = table.Column<DateTime>(type: "TEXT", nullable: true),
                    VotingDeadline = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PlayingDeadline = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReviewingDeadline = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameClubRounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameClubRounds_GameClubs_ClubId",
                        column: x => x.ClubId,
                        principalTable: "GameClubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameClubRounds_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "GameClubNominations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    NominatedByUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GameId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameClubNominations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameClubNominations_GameClubRounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "GameClubRounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameClubNominations_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameClubNominations_Users_NominatedByUserId",
                        column: x => x.NominatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GameClubReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameClubReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameClubReviews_GameClubRounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "GameClubRounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameClubReviews_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GameClubVotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    NominationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameClubVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameClubVotes_GameClubNominations_NominationId",
                        column: x => x.NominationId,
                        principalTable: "GameClubNominations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameClubVotes_GameClubRounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "GameClubRounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameClubVotes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameClubInvites_ClubId",
                table: "GameClubInvites",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_GameClubInvites_InvitedByUserId",
                table: "GameClubInvites",
                column: "InvitedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GameClubInvites_InviteeUserId",
                table: "GameClubInvites",
                column: "InviteeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GameClubMembers_ClubId_UserId",
                table: "GameClubMembers",
                columns: new[] { "ClubId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameClubMembers_UserId",
                table: "GameClubMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GameClubNominations_GameId",
                table: "GameClubNominations",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_GameClubNominations_NominatedByUserId",
                table: "GameClubNominations",
                column: "NominatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GameClubNominations_RoundId_GameId",
                table: "GameClubNominations",
                columns: new[] { "RoundId", "GameId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameClubReviews_RoundId_UserId",
                table: "GameClubReviews",
                columns: new[] { "RoundId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameClubReviews_UserId",
                table: "GameClubReviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GameClubRounds_ClubId",
                table: "GameClubRounds",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_GameClubRounds_GameId",
                table: "GameClubRounds",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_GameClubs_OwnerId",
                table: "GameClubs",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_GameClubVotes_NominationId",
                table: "GameClubVotes",
                column: "NominationId");

            migrationBuilder.CreateIndex(
                name: "IX_GameClubVotes_RoundId_UserId",
                table: "GameClubVotes",
                columns: new[] { "RoundId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameClubVotes_UserId",
                table: "GameClubVotes",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameClubInvites");

            migrationBuilder.DropTable(
                name: "GameClubMembers");

            migrationBuilder.DropTable(
                name: "GameClubReviews");

            migrationBuilder.DropTable(
                name: "GameClubVotes");

            migrationBuilder.DropTable(
                name: "GameClubNominations");

            migrationBuilder.DropTable(
                name: "GameClubRounds");

            migrationBuilder.DropTable(
                name: "GameClubs");
        }
    }
}
