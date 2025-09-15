using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRemainingItemFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_ItemCategory_CategoryId",
                table: "InventoryItems");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_CategoryId",
                table: "InventoryItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemCategory",
                table: "ItemCategory");

            migrationBuilder.RenameTable(
                name: "ItemCategory",
                newName: "Categories");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "InventoryItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AvailabilityStatus",
                table: "InventoryItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Condition",
                table: "InventoryItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConditionDescription",
                table: "InventoryItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "InventoryItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "RentalDate",
                table: "InventoryItems",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "RentalDescription",
                table: "InventoryItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RentalPrice",
                table: "InventoryItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RentalStatus",
                table: "InventoryItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TruckLoadingPriority",
                table: "InventoryItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Weight",
                table: "InventoryItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Width",
                table: "InventoryItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Categories",
                table: "Categories",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Categories",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "AvailabilityStatus",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "Condition",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "ConditionDescription",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "RentalDate",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "RentalDescription",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "RentalPrice",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "RentalStatus",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "TruckLoadingPriority",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "InventoryItems");

            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "ItemCategory");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "InventoryItems",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemCategory",
                table: "ItemCategory",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_CategoryId",
                table: "InventoryItems",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_ItemCategory_CategoryId",
                table: "InventoryItems",
                column: "CategoryId",
                principalTable: "ItemCategory",
                principalColumn: "Id");
        }
    }
}
