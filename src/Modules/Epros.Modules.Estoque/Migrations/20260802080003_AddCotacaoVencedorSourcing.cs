using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Estoque.Migrations
{
    /// <inheritdoc />
    public partial class AddCotacaoVencedorSourcing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "decidida_em",
                schema: "estoque",
                table: "sc_cotacoes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "fornecedor_vencedor_id",
                schema: "estoque",
                table: "sc_cotacoes",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "decidida_em",
                schema: "estoque",
                table: "sc_cotacoes");

            migrationBuilder.DropColumn(
                name: "fornecedor_vencedor_id",
                schema: "estoque",
                table: "sc_cotacoes");
        }
    }
}
