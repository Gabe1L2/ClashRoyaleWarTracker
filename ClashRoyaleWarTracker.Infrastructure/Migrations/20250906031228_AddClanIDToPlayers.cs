using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClashRoyaleWarTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClanIDToPlayers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClanID",
                table: "Players",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_ClanID",
                table: "Players",
                column: "ClanID");

            migrationBuilder.AddForeignKey(
                name: "FK_Players_Clans_ClanID",
                table: "Players",
                column: "ClanID",
                principalTable: "Clans",
                principalColumn: "ID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Players_Clans_ClanID",
                table: "Players");

            migrationBuilder.DropIndex(
                name: "IX_Players_ClanID",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "ClanID",
                table: "Players");
        }
    }
}
