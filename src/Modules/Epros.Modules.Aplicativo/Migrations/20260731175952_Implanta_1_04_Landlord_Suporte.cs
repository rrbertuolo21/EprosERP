using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Aplicativo.Migrations
{
    /// <inheritdoc />
    public partial class Implanta_1_04_Landlord_Suporte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "perfil_suporte",
                schema: "aplicativo",
                table: "usuarios_internos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "tenant_alvo",
                schema: "aplicativo",
                table: "sessoes_impersonacao",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "tipo_acesso",
                schema: "aplicativo",
                table: "sessoes_impersonacao",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "perfil_suporte",
                schema: "aplicativo",
                table: "usuarios_internos");

            migrationBuilder.DropColumn(
                name: "tenant_alvo",
                schema: "aplicativo",
                table: "sessoes_impersonacao");

            migrationBuilder.DropColumn(
                name: "tipo_acesso",
                schema: "aplicativo",
                table: "sessoes_impersonacao");
        }
    }
}
