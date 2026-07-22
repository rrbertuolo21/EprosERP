using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Aplicativo.Migrations
{
    /// <inheritdoc />
    public partial class AddCommandTypeAndLogToExecucaoMassa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "command_type",
                schema: "aplicativo",
                table: "execucoes_massa_global",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "resultado_log",
                schema: "aplicativo",
                table: "execucoes_massa_global",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "command_type",
                schema: "aplicativo",
                table: "execucoes_massa_global");

            migrationBuilder.DropColumn(
                name: "resultado_log",
                schema: "aplicativo",
                table: "execucoes_massa_global");
        }
    }
}
