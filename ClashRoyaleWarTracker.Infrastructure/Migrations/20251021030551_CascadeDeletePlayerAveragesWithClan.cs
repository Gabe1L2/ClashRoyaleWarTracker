using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClashRoyaleWarTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDeletePlayerAveragesWithClan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerAverages_Clans_ClanID",
                table: "PlayerAverages");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerAverages_Clans_ClanID",
                table: "PlayerAverages",
                column: "ClanID",
                principalTable: "Clans",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerAverages_Clans_ClanID",
                table: "PlayerAverages");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerAverages_Clans_ClanID",
                table: "PlayerAverages",
                column: "ClanID",
                principalTable: "Clans",
                principalColumn: "ID",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
