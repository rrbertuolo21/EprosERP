using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Aplicativo.Migrations
{
    /// <inheritdoc />
    public partial class AddApiKeyPoliciesAndRateLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "api_key_created",
                schema: "aplicativo",
                table: "usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "api_key_expiration",
                schema: "aplicativo",
                table: "usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "api_key_last_used",
                schema: "aplicativo",
                table: "usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "api_key_rate_limit",
                schema: "aplicativo",
                table: "usuarios",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "api_key_created",
                schema: "aplicativo",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "api_key_expiration",
                schema: "aplicativo",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "api_key_last_used",
                schema: "aplicativo",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "api_key_rate_limit",
                schema: "aplicativo",
                table: "usuarios");
        }
    }
}
