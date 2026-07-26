using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GestaoClientes.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantCompleteMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_demo",
                schema: "plataforma",
                table: "clientes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "nome_contato",
                schema: "plataforma",
                table: "clientes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "telefone",
                schema: "plataforma",
                table: "clientes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "token_acesso",
                schema: "plataforma",
                table: "clientes",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_demo",
                schema: "plataforma",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "nome_contato",
                schema: "plataforma",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "telefone",
                schema: "plataforma",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "token_acesso",
                schema: "plataforma",
                table: "clientes");
        }
    }
}
