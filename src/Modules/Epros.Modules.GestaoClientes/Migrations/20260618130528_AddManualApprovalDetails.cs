using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GestaoClientes.Migrations
{
    /// <inheritdoc />
    public partial class AddManualApprovalDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "justificativa_aprovacao",
                schema: "plataforma",
                table: "contratos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "operador_aprovacao",
                schema: "plataforma",
                table: "contratos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "justificativa_aprovacao",
                schema: "plataforma",
                table: "assinaturas_clientes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "operador_aprovacao",
                schema: "plataforma",
                table: "assinaturas_clientes",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "justificativa_aprovacao",
                schema: "plataforma",
                table: "contratos");

            migrationBuilder.DropColumn(
                name: "operador_aprovacao",
                schema: "plataforma",
                table: "contratos");

            migrationBuilder.DropColumn(
                name: "justificativa_aprovacao",
                schema: "plataforma",
                table: "assinaturas_clientes");

            migrationBuilder.DropColumn(
                name: "operador_aprovacao",
                schema: "plataforma",
                table: "assinaturas_clientes");
        }
    }
}
