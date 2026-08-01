using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GestaoClientes.Migrations
{
    /// <inheritdoc />
    public partial class AddSplitComissaoFatura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "percentual_comissao_revenda",
                schema: "plataforma",
                table: "faturas",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "percentual_comissao_vendedor",
                schema: "plataforma",
                table: "faturas",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_comissao_revenda",
                schema: "plataforma",
                table: "faturas",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_comissao_vendedor",
                schema: "plataforma",
                table: "faturas",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "percentual_comissao_revenda",
                schema: "plataforma",
                table: "faturas");

            migrationBuilder.DropColumn(
                name: "percentual_comissao_vendedor",
                schema: "plataforma",
                table: "faturas");

            migrationBuilder.DropColumn(
                name: "valor_comissao_revenda",
                schema: "plataforma",
                table: "faturas");

            migrationBuilder.DropColumn(
                name: "valor_comissao_vendedor",
                schema: "plataforma",
                table: "faturas");
        }
    }
}
