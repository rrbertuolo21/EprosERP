using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Estoque.Migrations
{
    /// <summary>
    /// ESTOQUE — fatia EST01 (motor único + custeio médio móvel + estoque negativo configurável).
    /// Aditiva: adiciona a política de estoque negativo por produto (D8). O saldo/custo verdadeiro já
    /// vive no kardex (EstoqueProduto + fichas), sem mudança de chave de saldo (isso é a fatia D2).
    /// Sem drift de xmin (padrão do repo).
    /// </summary>
    public partial class Implanta_EST01_MotorUnicoCusteio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // D8 — estoque negativo configurável por produto, default false (BLOQUEIA saída acima do saldo).
            migrationBuilder.AddColumn<bool>(
                name: "permite_estoque_negativo",
                schema: "estoque",
                table: "produtos",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "permite_estoque_negativo",
                schema: "estoque",
                table: "produtos");
        }
    }
}
