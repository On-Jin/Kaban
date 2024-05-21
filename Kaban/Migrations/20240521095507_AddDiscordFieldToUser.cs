using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kaban.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscordFieldToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DiscordAvatar",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiscordUsername",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscordAvatar",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DiscordUsername",
                table: "Users");
        }
    }
}
