using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Financeiro.Migrations
{
    /// <inheritdoc />
    public partial class PortEqualizacaoFinanceiro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contas_pagar_baixas",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "contas_receber_baixas",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "contas_pagar",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "contas_receber",
                schema: "financas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "plataforma");

            migrationBuilder.CreateTable(
                name: "contas_pagar",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    natureza_financeira_id = table.Column<Guid>(type: "uuid", nullable: true),
                    plano_de_contas_financeiro_item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true),
                    categoria = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    centro_custo_projeto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    data_pagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_vencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    fornecedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    historico = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    numero_documento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_contas_pagar", x => x.id);
                    table.ForeignKey(
                        name: "f_k_contas_pagar__plano_de_contas_itens_plano_de_contas_financeiro_~",
                        column: x => x.plano_de_contas_financeiro_item_id,
                        principalSchema: "financas",
                        principalTable: "plano_de_contas_itens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "f_k_contas_pagar_naturezas_financeiras_natureza_financeira_id",
                        column: x => x.natureza_financeira_id,
                        principalSchema: "financas",
                        principalTable: "naturezas_financeiras",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "contas_receber",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    natureza_financeira_id = table.Column<Guid>(type: "uuid", nullable: true),
                    plano_de_contas_financeiro_item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true),
                    centro_custo_projeto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    data_recebimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_vencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    documento_origem_id = table.Column<Guid>(type: "uuid", nullable: true),
                    historico = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    numero_documento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    origem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_contas_receber", x => x.id);
                    table.ForeignKey(
                        name: "f_k_contas_receber__plano_de_contas_itens_plano_de_contas_financeir~",
                        column: x => x.plano_de_contas_financeiro_item_id,
                        principalSchema: "financas",
                        principalTable: "plano_de_contas_itens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "f_k_contas_receber_naturezas_financeiras_natureza_financeira_id",
                        column: x => x.natureza_financeira_id,
                        principalSchema: "financas",
                        principalTable: "naturezas_financeiras",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "contas_pagar_baixas",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true),
                    conta_pagar_id = table.Column<Guid>(type: "uuid", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    data_pagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    forma_pagamento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    valor_pago = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_contas_pagar_baixas", x => x.id);
                    table.ForeignKey(
                        name: "f_k_contas_pagar_baixas_contas_pagar_conta_pagar_id",
                        column: x => x.conta_pagar_id,
                        principalSchema: "financas",
                        principalTable: "contas_pagar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contas_receber_baixas",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true),
                    conta_receber_id = table.Column<Guid>(type: "uuid", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    data_recebimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    forma_pagamento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    valor_recebido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_contas_receber_baixas", x => x.id);
                    table.ForeignKey(
                        name: "f_k_contas_receber_baixas_contas_receber_conta_receber_id",
                        column: x => x.conta_receber_id,
                        principalSchema: "financas",
                        principalTable: "contas_receber",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_contas_pagar_natureza_financeira_id",
                schema: "financas",
                table: "contas_pagar",
                column: "natureza_financeira_id");

            migrationBuilder.CreateIndex(
                name: "i_x_contas_pagar_plano_de_contas_financeiro_item_id",
                schema: "financas",
                table: "contas_pagar",
                column: "plano_de_contas_financeiro_item_id");

            migrationBuilder.CreateIndex(
                name: "ix__conta_pagar_sync_id",
                schema: "financas",
                table: "contas_pagar",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__conta_pagar_tenant_id",
                schema: "financas",
                table: "contas_pagar",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_contas_pagar_tenant_fornecedor",
                schema: "financas",
                table: "contas_pagar",
                columns: new[] { "tenant_id", "fornecedor_id" });

            migrationBuilder.CreateIndex(
                name: "ix_contas_pagar_tenant_vencimento",
                schema: "financas",
                table: "contas_pagar",
                columns: new[] { "tenant_id", "data_vencimento" });

            migrationBuilder.CreateIndex(
                name: "i_x_contas_pagar_baixas_conta_pagar_id",
                schema: "financas",
                table: "contas_pagar_baixas",
                column: "conta_pagar_id");

            migrationBuilder.CreateIndex(
                name: "ix__conta_pagar_baixa_sync_id",
                schema: "financas",
                table: "contas_pagar_baixas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__conta_pagar_baixa_tenant_id",
                schema: "financas",
                table: "contas_pagar_baixas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_contas_receber_natureza_financeira_id",
                schema: "financas",
                table: "contas_receber",
                column: "natureza_financeira_id");

            migrationBuilder.CreateIndex(
                name: "i_x_contas_receber_plano_de_contas_financeiro_item_id",
                schema: "financas",
                table: "contas_receber",
                column: "plano_de_contas_financeiro_item_id");

            migrationBuilder.CreateIndex(
                name: "ix__conta_receber_sync_id",
                schema: "financas",
                table: "contas_receber",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__conta_receber_tenant_id",
                schema: "financas",
                table: "contas_receber",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_contas_receber_tenant_cliente",
                schema: "financas",
                table: "contas_receber",
                columns: new[] { "tenant_id", "cliente_id" });

            migrationBuilder.CreateIndex(
                name: "ix_contas_receber_tenant_vencimento",
                schema: "financas",
                table: "contas_receber",
                columns: new[] { "tenant_id", "data_vencimento" });

            migrationBuilder.CreateIndex(
                name: "i_x_contas_receber_baixas_conta_receber_id",
                schema: "financas",
                table: "contas_receber_baixas",
                column: "conta_receber_id");

            migrationBuilder.CreateIndex(
                name: "ix__conta_receber_baixa_sync_id",
                schema: "financas",
                table: "contas_receber_baixas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__conta_receber_baixa_tenant_id",
                schema: "financas",
                table: "contas_receber_baixas",
                column: "tenant_id");
        }
    }
}
