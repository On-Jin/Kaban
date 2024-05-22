using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kaban.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnTableIntoDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Column_Boards_BoardId",
                table: "Column");

            migrationBuilder.DropForeignKey(
                name: "FK_MainTask_Column_ColumnId",
                table: "MainTask");

            migrationBuilder.DropForeignKey(
                name: "FK_SubTask_MainTask_MainTaskId",
                table: "SubTask");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SubTask",
                table: "SubTask");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MainTask",
                table: "MainTask");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Column",
                table: "Column");

            migrationBuilder.RenameTable(
                name: "SubTask",
                newName: "SubTasks");

            migrationBuilder.RenameTable(
                name: "MainTask",
                newName: "MainTasks");

            migrationBuilder.RenameTable(
                name: "Column",
                newName: "Columns");

            migrationBuilder.RenameIndex(
                name: "IX_SubTask_MainTaskId",
                table: "SubTasks",
                newName: "IX_SubTasks_MainTaskId");

            migrationBuilder.RenameIndex(
                name: "IX_MainTask_ColumnId",
                table: "MainTasks",
                newName: "IX_MainTasks_ColumnId");

            migrationBuilder.RenameIndex(
                name: "IX_Column_BoardId",
                table: "Columns",
                newName: "IX_Columns_BoardId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubTasks",
                table: "SubTasks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MainTasks",
                table: "MainTasks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Columns",
                table: "Columns",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Columns_Boards_BoardId",
                table: "Columns",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MainTasks_Columns_ColumnId",
                table: "MainTasks",
                column: "ColumnId",
                principalTable: "Columns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubTasks_MainTasks_MainTaskId",
                table: "SubTasks",
                column: "MainTaskId",
                principalTable: "MainTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Columns_Boards_BoardId",
                table: "Columns");

            migrationBuilder.DropForeignKey(
                name: "FK_MainTasks_Columns_ColumnId",
                table: "MainTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_SubTasks_MainTasks_MainTaskId",
                table: "SubTasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SubTasks",
                table: "SubTasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MainTasks",
                table: "MainTasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Columns",
                table: "Columns");

            migrationBuilder.RenameTable(
                name: "SubTasks",
                newName: "SubTask");

            migrationBuilder.RenameTable(
                name: "MainTasks",
                newName: "MainTask");

            migrationBuilder.RenameTable(
                name: "Columns",
                newName: "Column");

            migrationBuilder.RenameIndex(
                name: "IX_SubTasks_MainTaskId",
                table: "SubTask",
                newName: "IX_SubTask_MainTaskId");

            migrationBuilder.RenameIndex(
                name: "IX_MainTasks_ColumnId",
                table: "MainTask",
                newName: "IX_MainTask_ColumnId");

            migrationBuilder.RenameIndex(
                name: "IX_Columns_BoardId",
                table: "Column",
                newName: "IX_Column_BoardId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubTask",
                table: "SubTask",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MainTask",
                table: "MainTask",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Column",
                table: "Column",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Column_Boards_BoardId",
                table: "Column",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MainTask_Column_ColumnId",
                table: "MainTask",
                column: "ColumnId",
                principalTable: "Column",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubTask_MainTask_MainTaskId",
                table: "SubTask",
                column: "MainTaskId",
                principalTable: "MainTask",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
