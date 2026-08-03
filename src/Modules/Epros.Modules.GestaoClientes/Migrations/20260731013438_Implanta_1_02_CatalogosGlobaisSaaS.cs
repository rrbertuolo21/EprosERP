using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GestaoClientes.Migrations
{
    /// <inheritdoc />
    public partial class Implanta_1_02_CatalogosGlobaisSaaS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "nome",
                schema: "plataforma",
                table: "moedas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "nome",
                schema: "plataforma",
                table: "cupons",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_paises_nome",
                schema: "plataforma",
                table: "paises",
                column: "nome",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_paises_nome",
                schema: "plataforma",
                table: "paises");

            migrationBuilder.DropColumn(
                name: "nome",
                schema: "plataforma",
                table: "moedas");

            migrationBuilder.DropColumn(
                name: "nome",
                schema: "plataforma",
                table: "cupons");
        }
    }
}
