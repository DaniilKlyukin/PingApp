using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PingApp.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class PingSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsAllowedToPing",
                table: "Devices",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<bool>(
                name: "IsVisibleToUsers",
                table: "Devices",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVisibleToUsers",
                table: "Devices");

            migrationBuilder.AlterColumn<bool>(
                name: "IsAllowedToPing",
                table: "Devices",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);
        }
    }
}
