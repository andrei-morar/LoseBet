using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LoseBet.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateV1Slots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SlotSymbol",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ImageName = table.Column<string>(type: "text", nullable: false),
                    Multiplier3 = table.Column<decimal>(type: "numeric", nullable: false),
                    Multiplier4 = table.Column<decimal>(type: "numeric", nullable: false),
                    Multiplier5 = table.Column<decimal>(type: "numeric", nullable: false),
                    IsWild = table.Column<bool>(type: "boolean", nullable: false),
                    SlotGameId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlotSymbol", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SlotSymbol_SlotGames_SlotGameId",
                        column: x => x.SlotGameId,
                        principalTable: "SlotGames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SlotSymbol_SlotGameId",
                table: "SlotSymbol",
                column: "SlotGameId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SlotSymbol");
        }
    }
}
