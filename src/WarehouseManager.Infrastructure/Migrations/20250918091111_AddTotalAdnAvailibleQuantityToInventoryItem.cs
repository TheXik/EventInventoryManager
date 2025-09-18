using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTotalAdnAvailibleQuantityToInventoryItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "InventoryItems",
                newName: "TotalQuantity");

            migrationBuilder.AddColumn<int>(
                name: "AvailableQuantity",
                table: "InventoryItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableQuantity",
                table: "InventoryItems");

            migrationBuilder.RenameColumn(
                name: "TotalQuantity",
                table: "InventoryItems",
                newName: "Quantity");
        }
    }
}
