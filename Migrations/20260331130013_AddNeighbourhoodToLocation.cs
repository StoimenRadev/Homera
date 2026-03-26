using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Homera.Migrations
{
    /// <inheritdoc />
    public partial class AddNeighbourhoodToLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Neighbourhood",
                table: "Locations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Neighbourhood",
                table: "Locations");
        }
    }
}
