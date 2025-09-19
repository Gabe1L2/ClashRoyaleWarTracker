using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClashRoyaleWarTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceIsActiveWithStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add the new Status column with default value
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Players",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Active");

            // Step 2: Update Status based on existing IsActive values
            migrationBuilder.Sql(@"
                UPDATE Players 
                SET Status = CASE 
                    WHEN IsActive = 1 THEN 'Active' 
                    ELSE 'Inactive' 
                END");

            // Step 3: Drop the old IsActive column
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Players");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add back the IsActive column
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Players",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Step 2: Update IsActive based on Status values
            migrationBuilder.Sql(@"
                UPDATE Players 
                SET IsActive = CASE 
                    WHEN Status = 'Active' THEN 1 
                    ELSE 0 
                END");

            // Step 3: Drop the Status column
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Players");
        }
    }
}