using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClashRoyaleWarTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Players",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Players");
        }
    }
}
