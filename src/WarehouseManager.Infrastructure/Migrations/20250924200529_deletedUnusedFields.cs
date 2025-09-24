using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class deletedUnusedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "RecurrenceRule",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "eventStatus",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Categories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Events",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecurrenceRule",
                table: "Events",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "eventStatus",
                table: "Events",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Categories",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: null);
        }
    }
}
