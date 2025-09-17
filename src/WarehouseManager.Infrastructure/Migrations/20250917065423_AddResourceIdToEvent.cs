using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResourceIdToEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResourceId",
                table: "Events",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResourceId",
                table: "Events");
        }
    }
}
