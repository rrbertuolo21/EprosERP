using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GestaoClientes.Migrations
{
    /// <inheritdoc />
    public partial class Implanta_1_10_MenuPorCapacidade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "capacidade_requerida",
                schema: "plataforma",
                table: "menus_itens_nivel2",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "capacidade_requerida",
                schema: "plataforma",
                table: "menus_itens_nivel1",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "capacidade_requerida",
                schema: "plataforma",
                table: "menus",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "capacidade_requerida",
                schema: "plataforma",
                table: "menus_itens_nivel2");

            migrationBuilder.DropColumn(
                name: "capacidade_requerida",
                schema: "plataforma",
                table: "menus_itens_nivel1");

            migrationBuilder.DropColumn(
                name: "capacidade_requerida",
                schema: "plataforma",
                table: "menus");
        }
    }
}
