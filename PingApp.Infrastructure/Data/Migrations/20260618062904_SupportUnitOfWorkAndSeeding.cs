using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PingApp.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SupportUnitOfWorkAndSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StatusRecord_DeviceId",
                table: "StatusRecord");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "LastKnownStatus",
                table: "Devices",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastStatusChangedUtc",
                table: "Devices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatusRecord_DeviceId_DateTime_Desc",
                table: "StatusRecord",
                columns: new[] { "DeviceId", "DateTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StatusRecord_DeviceId_DateTime_Desc",
                table: "StatusRecord");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastKnownStatus",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "LastStatusChangedUtc",
                table: "Devices");

            migrationBuilder.CreateIndex(
                name: "IX_StatusRecord_DeviceId",
                table: "StatusRecord",
                column: "DeviceId");
        }
    }
}
