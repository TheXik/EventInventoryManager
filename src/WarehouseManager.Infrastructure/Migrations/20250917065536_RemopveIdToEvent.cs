using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemopveIdToEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResourceId",
                table: "Events");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResourceId",
                table: "Events",
                type: "TEXT",
                nullable: true);
        }
    }
}
