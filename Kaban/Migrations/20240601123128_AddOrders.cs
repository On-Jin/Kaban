using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kaban.Migrations
{
    /// <inheritdoc />
    public partial class AddOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "MainTasks",
                newName: "Order");

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "SubTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Columns",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "SubTasks");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Columns");

            migrationBuilder.RenameColumn(
                name: "Order",
                table: "MainTasks",
                newName: "Status");
        }
    }
}
