using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoseBet.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSlotSymbolsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SlotSymbol_SlotGames_SlotGameId",
                table: "SlotSymbol");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SlotSymbol",
                table: "SlotSymbol");

            migrationBuilder.RenameTable(
                name: "SlotSymbol",
                newName: "SlotSymbols");

            migrationBuilder.RenameIndex(
                name: "IX_SlotSymbol_SlotGameId",
                table: "SlotSymbols",
                newName: "IX_SlotSymbols_SlotGameId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SlotSymbols",
                table: "SlotSymbols",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SlotSymbols_SlotGames_SlotGameId",
                table: "SlotSymbols",
                column: "SlotGameId",
                principalTable: "SlotGames",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SlotSymbols_SlotGames_SlotGameId",
                table: "SlotSymbols");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SlotSymbols",
                table: "SlotSymbols");

            migrationBuilder.RenameTable(
                name: "SlotSymbols",
                newName: "SlotSymbol");

            migrationBuilder.RenameIndex(
                name: "IX_SlotSymbols_SlotGameId",
                table: "SlotSymbol",
                newName: "IX_SlotSymbol_SlotGameId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SlotSymbol",
                table: "SlotSymbol",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SlotSymbol_SlotGames_SlotGameId",
                table: "SlotSymbol",
                column: "SlotGameId",
                principalTable: "SlotGames",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
