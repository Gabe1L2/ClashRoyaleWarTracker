using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClashRoyaleWarTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceWarTablesWithPlayerWarHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RawWarData");

            migrationBuilder.DropTable(
                name: "WarData");

            migrationBuilder.CreateTable(
                name: "PlayerWarHistories",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerID = table.Column<int>(type: "int", nullable: false),
                    ClanHistoryID = table.Column<int>(type: "int", nullable: false),
                    Fame = table.Column<int>(type: "int", nullable: false),
                    DecksUsed = table.Column<int>(type: "int", nullable: false),
                    BoatAttacks = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsModified = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerWarHistories", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PlayerWarHistories_ClanHistories_ClanHistoryID",
                        column: x => x.ClanHistoryID,
                        principalTable: "ClanHistories",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerWarHistories_Players_PlayerID",
                        column: x => x.PlayerID,
                        principalTable: "Players",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerWarHistories_ClanHistoryID",
                table: "PlayerWarHistories",
                column: "ClanHistoryID");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerWarHistories_PlayerID_ClanHistoryID",
                table: "PlayerWarHistories",
                columns: new[] { "PlayerID", "ClanHistoryID" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerWarHistories");

            migrationBuilder.CreateTable(
                name: "RawWarData",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BoatAttacks = table.Column<int>(type: "int", nullable: false),
                    ClanHistoryID = table.Column<int>(type: "int", nullable: false),
                    DecksUsed = table.Column<int>(type: "int", nullable: false),
                    Fame = table.Column<int>(type: "int", nullable: false),
                    InsertDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlayerID = table.Column<int>(type: "int", nullable: false),
                    RepairPoints = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawWarData", x => x.ID);
                    table.ForeignKey(
                        name: "FK_RawWarData_ClanHistories_ClanHistoryID",
                        column: x => x.ClanHistoryID,
                        principalTable: "ClanHistories",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RawWarData_Players_PlayerID",
                        column: x => x.PlayerID,
                        principalTable: "Players",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WarData",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClanHistoryID = table.Column<int>(type: "int", nullable: false),
                    DecksUsed = table.Column<int>(type: "int", nullable: false),
                    Fame = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlayerID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarData", x => x.ID);
                    table.ForeignKey(
                        name: "FK_WarData_ClanHistories_ClanHistoryID",
                        column: x => x.ClanHistoryID,
                        principalTable: "ClanHistories",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WarData_Players_PlayerID",
                        column: x => x.PlayerID,
                        principalTable: "Players",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RawWarData_ClanHistoryID",
                table: "RawWarData",
                column: "ClanHistoryID");

            migrationBuilder.CreateIndex(
                name: "IX_RawWarData_PlayerID_ClanHistoryID",
                table: "RawWarData",
                columns: new[] { "PlayerID", "ClanHistoryID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarData_ClanHistoryID",
                table: "WarData",
                column: "ClanHistoryID");

            migrationBuilder.CreateIndex(
                name: "IX_WarData_PlayerID_ClanHistoryID",
                table: "WarData",
                columns: new[] { "PlayerID", "ClanHistoryID" },
                unique: true);
        }
    }
}
