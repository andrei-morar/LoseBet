using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LoseBet.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSportsBetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BetTickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Stake = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalOdds = table.Column<decimal>(type: "numeric", nullable: false),
                    PotentialWin = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BetTickets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SportsMatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HomeTeam = table.Column<string>(type: "text", nullable: false),
                    AwayTeam = table.Column<string>(type: "text", nullable: false),
                    Odds1 = table.Column<decimal>(type: "numeric", nullable: false),
                    OddsX = table.Column<decimal>(type: "numeric", nullable: false),
                    Odds2 = table.Column<decimal>(type: "numeric", nullable: false),
                    IsFinished = table.Column<bool>(type: "boolean", nullable: false),
                    HomeScore = table.Column<int>(type: "integer", nullable: false),
                    AwayScore = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SportsMatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BetSelections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BetTicketId = table.Column<int>(type: "integer", nullable: false),
                    SportsMatchId = table.Column<int>(type: "integer", nullable: false),
                    Pick = table.Column<string>(type: "text", nullable: false),
                    Odds = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BetSelections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BetSelections_BetTickets_BetTicketId",
                        column: x => x.BetTicketId,
                        principalTable: "BetTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BetSelections_SportsMatches_SportsMatchId",
                        column: x => x.SportsMatchId,
                        principalTable: "SportsMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BetSelections_BetTicketId",
                table: "BetSelections",
                column: "BetTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_BetSelections_SportsMatchId",
                table: "BetSelections",
                column: "SportsMatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BetSelections");

            migrationBuilder.DropTable(
                name: "BetTickets");

            migrationBuilder.DropTable(
                name: "SportsMatches");
        }
    }
}
