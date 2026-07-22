using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Estoque.Migrations
{
    /// <inheritdoc />
    public partial class ExpandProdutoEstoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ativo",
                schema: "estoque",
                table: "produtos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "balanca_id",
                schema: "estoque",
                table: "produtos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "categoria_id",
                schema: "estoque",
                table: "produtos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "cest_id",
                schema: "estoque",
                table: "produtos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "codigo",
                schema: "estoque",
                table: "produtos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "codigo_anp_id",
                schema: "estoque",
                table: "produtos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "codigo_produto_balanca",
                schema: "estoque",
                table: "produtos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "descricao",
                schema: "estoque",
                table: "produtos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ean",
                schema: "estoque",
                table: "produtos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "imagem",
                schema: "estoque",
                table: "produtos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "marca_produto_id",
                schema: "estoque",
                table: "produtos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ncm_id",
                schema: "estoque",
                table: "produtos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "peso_bruto",
                schema: "estoque",
                table: "produtos",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "peso_liquido",
                schema: "estoque",
                table: "produtos",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "tipo_produto",
                schema: "estoque",
                table: "produtos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "unidade_medida_comercial_id",
                schema: "estoque",
                table: "produtos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "utiliza_balanca",
                schema: "estoque",
                table: "produtos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_compra",
                schema: "estoque",
                table: "produtos",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_venda",
                schema: "estoque",
                table: "produtos",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_venda_prazo",
                schema: "estoque",
                table: "produtos",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "categorias",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_categorias", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "marcas",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_marcas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "unidades_medida",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    unidade_medida = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    fator = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_unidades_medida", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_produtos_categoria_id",
                schema: "estoque",
                table: "produtos",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "i_x_produtos_marca_produto_id",
                schema: "estoque",
                table: "produtos",
                column: "marca_produto_id");

            migrationBuilder.CreateIndex(
                name: "i_x_produtos_unidade_medida_comercial_id",
                schema: "estoque",
                table: "produtos",
                column: "unidade_medida_comercial_id");

            migrationBuilder.CreateIndex(
                name: "i_x_categorias_tenant_id_descricao",
                schema: "estoque",
                table: "categorias",
                columns: new[] { "tenant_id", "descricao" });

            migrationBuilder.CreateIndex(
                name: "ix__categoria_produto_sync_id",
                schema: "estoque",
                table: "categorias",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__categoria_produto_tenant_id",
                schema: "estoque",
                table: "categorias",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_marcas_tenant_id_descricao",
                schema: "estoque",
                table: "marcas",
                columns: new[] { "tenant_id", "descricao" });

            migrationBuilder.CreateIndex(
                name: "ix__marca_produto_sync_id",
                schema: "estoque",
                table: "marcas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__marca_produto_tenant_id",
                schema: "estoque",
                table: "marcas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_unidades_medida_tenant_id_unidade_medida",
                schema: "estoque",
                table: "unidades_medida",
                columns: new[] { "tenant_id", "unidade_medida" });

            migrationBuilder.CreateIndex(
                name: "ix__unidade_medida_comercial_sync_id",
                schema: "estoque",
                table: "unidades_medida",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__unidade_medida_comercial_tenant_id",
                schema: "estoque",
                table: "unidades_medida",
                column: "tenant_id");

            migrationBuilder.AddForeignKey(
                name: "f_k_produtos__unidades_medida_unidade_medida_comercial_id",
                schema: "estoque",
                table: "produtos",
                column: "unidade_medida_comercial_id",
                principalSchema: "estoque",
                principalTable: "unidades_medida",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "f_k_produtos_categorias_categoria_id",
                schema: "estoque",
                table: "produtos",
                column: "categoria_id",
                principalSchema: "estoque",
                principalTable: "categorias",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "f_k_produtos_marcas_marca_produto_id",
                schema: "estoque",
                table: "produtos",
                column: "marca_produto_id",
                principalSchema: "estoque",
                principalTable: "marcas",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_produtos__unidades_medida_unidade_medida_comercial_id",
                schema: "estoque",
                table: "produtos");

            migrationBuilder.DropForeignKey(
                name: "f_k_produtos_categorias_categoria_id",
                schema: "estoque",
                table: "produtos");

            migrationBuilder.DropForeignKey(
                name: "f_k_produtos_marcas_marca_produto_id",
                schema: "estoque",
                table: "produtos");

            migrationBuilder.DropTable(
                name: "categorias",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "marcas",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "unidades_medida",
                schema: "estoque");

            migrationBuilder.DropIndex(
                name: "i_x_produtos_categoria_id",
                schema: "estoque",
                table: "produtos");

            migrationBuilder.DropIndex(
                name: "i_x_produtos_marca_produto_id",
                schema: "estoque",
                table: "produtos");

            migrationBuilder.DropIndex(
                name: "i_x_produtos_unidade_medida_comercial_id",
                schema: "estoque",
                table: "produtos");

            migrationBuilder.DropColumn(
                name: "ativo",
                schema: "estoque",
                table: "produtos");

            migrationBuilder.DropColumn(
                name: "balanca_id",
                schema: "estoque",
                table: "produtos");

            migrationBuilder.DropColumn(
                name: "categoria_id",
                schema: "estoque",
                table: "produtos");

            migrationBuilder.DropColumn(
                name: "cest_id",
                schema: "estoque",
                table: "produtos");

            migrationBuilder.DropColumn(
                name: "codigo",
                schema: "estoque",
                table: "produtos");

            migrationBuilder.DropColumn(
                name: "codigo_anp_id",
                schema: "estoque",
                table: "produtos");

            migrationBuilder.DropColumn(
                name: "codigo_produto_balanca",
                schema: "estoque",
                table: "produtos");

            migrationBuilder.DropColumn(
                name: "descricao",
                schema: "estoque",
                table: "produtos");

            migrationBuilder.DropColumn(
                name: "ean",
                schema: "estoque",
                table: "produtos");

            migrationBuilder.DropColumn(
                name: "imagem",
                schema: "estoque",
                table: "produtos");

            migrationBuilder.DropColumn(
                name: "marca_produto_id",
                schema: "estoque",
                table: "produtos");

            migrationBuilder.DropColumn(
                name: "ncm_id",
                schema: "estoque",
                table: "produtos");

            migrationBuilder.DropColumn(
                name: "peso_bruto",
                schema: "estoque",
                table: "produtos");

            migrationBuilder.DropColumn(
                name: "peso_liquido",
                schema: "estoque",
                table: "produtos");

            migrationBuilder.DropColumn(
                name: "tipo_produto",
                schema: "estoque",
                table: "produtos");

            migrationBuilder.DropColumn(
                name: "unidade_medida_comercial_id",
                schema: "estoque",
                table: "produtos");

            migrationBuilder.DropColumn(
                name: "utiliza_balanca",
                schema: "estoque",
                table: "produtos");

            migrationBuilder.DropColumn(
                name: "valor_compra",
                schema: "estoque",
                table: "produtos");

            migrationBuilder.DropColumn(
                name: "valor_venda",
                schema: "estoque",
                table: "produtos");

            migrationBuilder.DropColumn(
                name: "valor_venda_prazo",
                schema: "estoque",
                table: "produtos");
        }
    }
}
