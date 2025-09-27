using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClashRoyaleWarTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRosterAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RosterAssignments",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeasonID = table.Column<int>(type: "int", nullable: false),
                    WeekIndex = table.Column<int>(type: "int", nullable: false),
                    PlayerID = table.Column<int>(type: "int", nullable: false),
                    ClanID = table.Column<int>(type: "int", nullable: true),
                    IsInClan = table.Column<bool>(type: "bit", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RosterAssignments", x => x.ID);
                    table.ForeignKey(
                        name: "FK_RosterAssignments_Clans_ClanID",
                        column: x => x.ClanID,
                        principalTable: "Clans",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RosterAssignments_Players_PlayerID",
                        column: x => x.PlayerID,
                        principalTable: "Players",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RosterAssignments_ClanID_SeasonID_WeekIndex",
                table: "RosterAssignments",
                columns: new[] { "ClanID", "SeasonID", "WeekIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_RosterAssignments_PlayerID",
                table: "RosterAssignments",
                column: "PlayerID");

            migrationBuilder.CreateIndex(
                name: "IX_RosterAssignments_SeasonID_WeekIndex",
                table: "RosterAssignments",
                columns: new[] { "SeasonID", "WeekIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_RosterAssignments_SeasonID_WeekIndex_PlayerID",
                table: "RosterAssignments",
                columns: new[] { "SeasonID", "WeekIndex", "PlayerID" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RosterAssignments");
        }
    }
}
