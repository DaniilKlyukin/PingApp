using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PingApp.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Nickname",
                table: "UserDevice",
                newName: "DeviceNickname");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeviceNickname",
                table: "UserDevice",
                newName: "Nickname");
        }
    }
}
