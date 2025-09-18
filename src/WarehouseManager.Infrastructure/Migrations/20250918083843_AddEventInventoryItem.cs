using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEventInventoryItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_Events_EventId",
                table: "InventoryItems");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_EventId",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "InventoryItems");

            migrationBuilder.CreateTable(
                name: "EventInventoryItems",
                columns: table => new
                {
                    EventId = table.Column<int>(type: "INTEGER", nullable: false),
                    InventoryItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventInventoryItems", x => new { x.EventId, x.InventoryItemId });
                    table.ForeignKey(
                        name: "FK_EventInventoryItems_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventInventoryItems_InventoryItems_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalTable: "InventoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventInventoryItems_InventoryItemId",
                table: "EventInventoryItems",
                column: "InventoryItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventInventoryItems");

            migrationBuilder.AddColumn<int>(
                name: "EventId",
                table: "InventoryItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_EventId",
                table: "InventoryItems",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_Events_EventId",
                table: "InventoryItems",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id");
        }
    }
}
