using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateEventEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "To",
                table: "Events",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "From",
                table: "Events",
                newName: "EndDate");

            migrationBuilder.AddColumn<int>(
                name: "eventStatus",
                table: "Events",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "eventStatus",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Events",
                newName: "To");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "Events",
                newName: "From");
        }
    }
}
