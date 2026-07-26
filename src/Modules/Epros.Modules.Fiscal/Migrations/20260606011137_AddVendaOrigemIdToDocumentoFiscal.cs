using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Fiscal.Migrations
{
    /// <inheritdoc />
    public partial class AddVendaOrigemIdToDocumentoFiscal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "estoque");

            migrationBuilder.AddColumn<Guid>(
                name: "venda_origem_id",
                schema: "plataforma",
                table: "documentos_fiscais",
                type: "uuid",
                nullable: true);

            // Stub 'estoque.produtos' removido: o modulo Estoque e o dono da tabela
            // (a leitura cross-module usa ProdutoLookup com ExcludeFromMigrations).
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "venda_origem_id",
                schema: "plataforma",
                table: "documentos_fiscais");
        }
    }
}
