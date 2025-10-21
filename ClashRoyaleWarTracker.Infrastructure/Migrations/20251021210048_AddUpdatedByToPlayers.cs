using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClashRoyaleWarTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdatedByToPlayers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Players",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Players");
        }
    }
}
