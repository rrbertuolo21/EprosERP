using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Financeiro.Migrations
{
    /// <summary>
    /// FIN-PO — Orçamento comercial (EF §6.2/§8.2/§12.3-12.4): cria po_orcamento_comercial e
    /// po_orcamento_comercial_item. Segue a convenção do módulo (schema financas, snake_case,
    /// colunas base de auditoria/tenant/sync; xmin é coluna de sistema do Postgres, mapeada por
    /// convenção e não materializada em DDL).
    /// </summary>
    /// <inheritdoc />
    public partial class AddOrcamentoComercialFinPo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "orcamentos_comerciais",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vendedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transportadora_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    condicao_pagamento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    data_cadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_entrega = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    validade = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tipo_frete = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    valor_subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_frete = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    taxa_comissao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_comissao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    taxa_desconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_desconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    observacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    status_pedido = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("p_k_orcamentos_comerciais", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "orcamento_comercial_itens",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    orcamento_comercial_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    taxa_desconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_desconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_orcamento_comercial_itens", x => x.id);
                    table.ForeignKey(
                        name: "f_k_orcamento_comercial_itens_orcamentos_comerciais_orcamento_c~",
                        column: x => x.orcamento_comercial_id,
                        principalSchema: "financas",
                        principalTable: "orcamentos_comerciais",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix__orcamento_comercial_sync_id",
                schema: "financas",
                table: "orcamentos_comerciais",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__orcamento_comercial_tenant_id",
                schema: "financas",
                table: "orcamentos_comerciais",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_orcamento_comercial_tenant_codigo",
                schema: "financas",
                table: "orcamentos_comerciais",
                columns: new[] { "tenant_id", "codigo" });

            migrationBuilder.CreateIndex(
                name: "ix_orcamento_comercial_tenant_status",
                schema: "financas",
                table: "orcamentos_comerciais",
                columns: new[] { "tenant_id", "status_pedido" });

            migrationBuilder.CreateIndex(
                name: "ix__orcamento_comercial_item_sync_id",
                schema: "financas",
                table: "orcamento_comercial_itens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__orcamento_comercial_item_tenant_id",
                schema: "financas",
                table: "orcamento_comercial_itens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_orcamento_comercial_item_cabecalho",
                schema: "financas",
                table: "orcamento_comercial_itens",
                column: "orcamento_comercial_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "orcamento_comercial_itens",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "orcamentos_comerciais",
                schema: "financas");
        }
    }
}
