using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Estoque.Migrations
{
    /// <inheritdoc />
    public partial class PortEqualizacaoEstoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_compra_itens_compras_compra_id1",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropIndex(
                name: "i_x_compra_itens_compra_id1",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropColumn(
                name: "compra_id1",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_impostos_compra_id",
                schema: "estoque",
                table: "compra_impostos",
                column: "compra_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "f_k_compra_impostos_compras_compra_id",
                schema: "estoque",
                table: "compra_impostos",
                column: "compra_id",
                principalSchema: "estoque",
                principalTable: "compras",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_compra_impostos_compras_compra_id",
                schema: "estoque",
                table: "compra_impostos");

            migrationBuilder.DropIndex(
                name: "i_x_compra_impostos_compra_id",
                schema: "estoque",
                table: "compra_impostos");

            migrationBuilder.AddColumn<Guid>(
                name: "compra_id1",
                schema: "estoque",
                table: "compra_itens",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "i_x_compra_itens_compra_id1",
                schema: "estoque",
                table: "compra_itens",
                column: "compra_id1");

            migrationBuilder.AddForeignKey(
                name: "f_k_compra_itens_compras_compra_id1",
                schema: "estoque",
                table: "compra_itens",
                column: "compra_id1",
                principalSchema: "estoque",
                principalTable: "compras",
                principalColumn: "id");
        }
    }
}
