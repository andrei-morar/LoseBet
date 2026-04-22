using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoseBet.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMinesAndBlackjack : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlackjackSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UserHand = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DealerHand = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlackjackSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MinesSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BombCount = table.Column<int>(type: "int", nullable: false),
                    BombLocations = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RevealedIndices = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsGameOver = table.Column<bool>(type: "bit", nullable: false),
                    CurrentMultiplier = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MinesSessions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlackjackSessions");

            migrationBuilder.DropTable(
                name: "MinesSessions");
        }
    }
}
