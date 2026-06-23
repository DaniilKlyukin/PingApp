using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PingApp.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTelegramSubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "telegram");

            migrationBuilder.CreateTable(
                name: "TelegramSubscriptions",
                schema: "telegram",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChatId = table.Column<long>(type: "bigint", nullable: false),
                    DeviceAddress = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SubscribedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TelegramSubscriptions_ChatId_DeviceAddress",
                schema: "telegram",
                table: "TelegramSubscriptions",
                columns: new[] { "ChatId", "DeviceAddress" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TelegramSubscriptions",
                schema: "telegram");
        }
    }
}
