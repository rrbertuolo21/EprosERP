using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Financeiro.Migrations
{
    /// <inheritdoc />
    public partial class PortConsolidacaoFinanceiro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "sequencia_exibicao",
                schema: "financas",
                table: "planos_de_contas",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "sequencia_exibicao",
                schema: "financas",
                table: "plano_de_contas_itens",
                type: "bigint",
                nullable: true);

            // FK obrigatoria (dominio valida empresa != Guid.Empty). Removido defaultValue Guid.Empty
            // (1R.1/1R.2) para evitar novos registros nascerem com FK invalida. Banco zerado => AddColumn
            // NOT NULL sem default e seguro; entidade sempre atribui EmpresaId real.
            migrationBuilder.AddColumn<Guid>(
                name: "empresa_id",
                schema: "financas",
                table: "naturezas_financeiras",
                type: "uuid",
                nullable: false);

            migrationBuilder.AddColumn<long>(
                name: "sequencia_exibicao",
                schema: "financas",
                table: "naturezas_financeiras",
                type: "bigint",
                nullable: true);

            // FK obrigatoria (dominio valida empresa != Guid.Empty). Removido defaultValue Guid.Empty
            // (1R.1/1R.2) para evitar novos registros nascerem com FK invalida. Banco zerado => seguro.
            migrationBuilder.AddColumn<Guid>(
                name: "empresa_id",
                schema: "financas",
                table: "contas_bancarias",
                type: "uuid",
                nullable: false);

            migrationBuilder.AddColumn<long>(
                name: "sequencia_exibicao",
                schema: "financas",
                table: "contas_bancarias",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "sequencia_exibicao",
                schema: "financas",
                table: "cartoes_de_credito",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "fatos_geradores_financeiros",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    origem = table.Column<int>(type: "integer", nullable: false),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: true),
                    compra_id = table.Column<Guid>(type: "uuid", nullable: true),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("p_k_fatos_geradores_financeiros", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "importacoes_arquivo_ofx",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_banco = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    numero_conta = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    tipo_conta = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    data_inicio_extrato = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_fim_extrato = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_importacoes_arquivo_ofx", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plano_de_contas_empresas",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    plano_de_contas_financeiro_id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_plano_de_contas_empresas", x => x.id);
                    table.ForeignKey(
                        name: "f_k_plano_de_contas_empresas_planos_de_contas_plano_de_contas_f~",
                        column: x => x.plano_de_contas_financeiro_id,
                        principalSchema: "financas",
                        principalTable: "planos_de_contas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contas_a_pagar",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequencia_exibicao = table.Column<long>(type: "bigint", nullable: true),
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plano_de_contas_financeiro_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fato_gerador_financeiro_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_pessoa = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    situacao = table.Column<int>(type: "integer", nullable: false),
                    data_vencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_emissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_baixa = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    documento = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    valor_titulo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_total_desconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_total_multa = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_total_juros = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_total_troco = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_total_acrescimo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_total_pago = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_total_a_pagar_titulo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_inicial_desconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_inicial_multa = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_inicial_juros = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_inicial_acrescimo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_inicial_a_pagar_titulo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    numero_parcela = table.Column<int>(type: "integer", nullable: false),
                    detalhamento = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    justificativa_cancelamento = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
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
                    table.PrimaryKey("p_k_contas_a_pagar", x => x.id);
                    table.ForeignKey(
                        name: "f_k_contas_a_pagar__fatos_geradores_financeiros_fato_gerador_finan~",
                        column: x => x.fato_gerador_financeiro_id,
                        principalSchema: "financas",
                        principalTable: "fatos_geradores_financeiros",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_contas_a_pagar__plano_de_contas_itens_plano_de_contas_financeir~",
                        column: x => x.plano_de_contas_financeiro_item_id,
                        principalSchema: "financas",
                        principalTable: "plano_de_contas_itens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "contas_a_receber",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequencia_exibicao = table.Column<long>(type: "bigint", nullable: true),
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plano_de_contas_financeiro_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fato_gerador_financeiro_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_pessoa = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    situacao = table.Column<int>(type: "integer", nullable: false),
                    data_vencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_emissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_baixa = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    documento = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    valor_titulo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_total_desconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_total_multa = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_total_juros = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_total_troco = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_total_acrescimo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_total_recebido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_total_a_receber_titulo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_inicial_desconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_inicial_multa = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_inicial_juros = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_inicial_acrescimo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_inicial_a_receber_titulo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    numero_parcela = table.Column<int>(type: "integer", nullable: false),
                    detalhamento = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    justificativa_cancelamento = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
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
                    table.PrimaryKey("p_k_contas_a_receber", x => x.id);
                    table.ForeignKey(
                        name: "f_k_contas_a_receber__fatos_geradores_financeiros_fato_gerador_fin~",
                        column: x => x.fato_gerador_financeiro_id,
                        principalSchema: "financas",
                        principalTable: "fatos_geradores_financeiros",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_contas_a_receber__plano_de_contas_itens_plano_de_contas_finance~",
                        column: x => x.plano_de_contas_financeiro_item_id,
                        principalSchema: "financas",
                        principalTable: "plano_de_contas_itens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "contas_a_pagar_itens",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contas_a_pagar_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plano_de_contas_financeiro_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    conta_bancaria_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_pagamento = table.Column<int>(type: "integer", nullable: false),
                    valor_parcela = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_pago = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_desconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_multa = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_juros = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_troco = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_acrescimo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_a_pagar = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    data_pagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_contas_a_pagar_itens", x => x.id);
                    table.ForeignKey(
                        name: "f_k_contas_a_pagar_itens__plano_de_contas_itens_plano_de_contas_fin~",
                        column: x => x.plano_de_contas_financeiro_item_id,
                        principalSchema: "financas",
                        principalTable: "plano_de_contas_itens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_contas_a_pagar_itens_contas_a_pagar_contas_a_pagar_id",
                        column: x => x.contas_a_pagar_id,
                        principalSchema: "financas",
                        principalTable: "contas_a_pagar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_contas_a_pagar_itens_contas_bancarias_conta_bancaria_id",
                        column: x => x.conta_bancaria_id,
                        principalSchema: "financas",
                        principalTable: "contas_bancarias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "contas_a_receber_itens",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contas_a_receber_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plano_de_contas_financeiro_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    conta_bancaria_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_pagamento = table.Column<int>(type: "integer", nullable: false),
                    valor_parcela = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_pago = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_desconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_multa = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_juros = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_troco = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_acrescimo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_a_receber = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    data_recebimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_contas_a_receber_itens", x => x.id);
                    table.ForeignKey(
                        name: "f_k_contas_a_receber_itens__plano_de_contas_itens_plano_de_contas_f~",
                        column: x => x.plano_de_contas_financeiro_item_id,
                        principalSchema: "financas",
                        principalTable: "plano_de_contas_itens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_contas_a_receber_itens_contas_a_receber_contas_a_receber_id",
                        column: x => x.contas_a_receber_id,
                        principalSchema: "financas",
                        principalTable: "contas_a_receber",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_contas_a_receber_itens_contas_bancarias_conta_bancaria_id",
                        column: x => x.conta_bancaria_id,
                        principalSchema: "financas",
                        principalTable: "contas_bancarias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "importacoes_arquivo_ofx_transacoes",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    importacacao_arquivo_ofx_id = table.Column<Guid>(type: "uuid", nullable: false),
                    contas_a_receber_id = table.Column<Guid>(type: "uuid", nullable: true),
                    contas_a_pagar_id = table.Column<Guid>(type: "uuid", nullable: true),
                    identificador_transacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    conciliado = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_importacoes_arquivo_ofx_transacoes", x => x.id);
                    table.ForeignKey(
                        name: "f_k_importacoes_arquivo_ofx_transacoes_contas_a_pagar_contas_a_~",
                        column: x => x.contas_a_pagar_id,
                        principalSchema: "financas",
                        principalTable: "contas_a_pagar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "f_k_importacoes_arquivo_ofx_transacoes_contas_a_receber_contas_~",
                        column: x => x.contas_a_receber_id,
                        principalSchema: "financas",
                        principalTable: "contas_a_receber",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "f_k_importacoes_arquivo_ofx_transacoes_importacoes_arquivo_ofx_~",
                        column: x => x.importacacao_arquivo_ofx_id,
                        principalSchema: "financas",
                        principalTable: "importacoes_arquivo_ofx",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_natureza_tenant_empresa",
                schema: "financas",
                table: "naturezas_financeiras",
                columns: new[] { "tenant_id", "empresa_id" });

            migrationBuilder.CreateIndex(
                name: "ix_conta_bancaria_tenant_empresa",
                schema: "financas",
                table: "contas_bancarias",
                columns: new[] { "tenant_id", "empresa_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_contas_a_pagar_fato_gerador_financeiro_id",
                schema: "financas",
                table: "contas_a_pagar",
                column: "fato_gerador_financeiro_id");

            migrationBuilder.CreateIndex(
                name: "i_x_contas_a_pagar_plano_de_contas_financeiro_item_id",
                schema: "financas",
                table: "contas_a_pagar",
                column: "plano_de_contas_financeiro_item_id");

            migrationBuilder.CreateIndex(
                name: "ix__contas_a_pagar_sync_id",
                schema: "financas",
                table: "contas_a_pagar",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__contas_a_pagar_tenant_id",
                schema: "financas",
                table: "contas_a_pagar",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_contas_a_pagar_tenant_pessoa",
                schema: "financas",
                table: "contas_a_pagar",
                columns: new[] { "tenant_id", "pessoa_id" });

            migrationBuilder.CreateIndex(
                name: "ix_contas_a_pagar_tenant_vencimento",
                schema: "financas",
                table: "contas_a_pagar",
                columns: new[] { "tenant_id", "data_vencimento" });

            migrationBuilder.CreateIndex(
                name: "i_x_contas_a_pagar_itens_conta_bancaria_id",
                schema: "financas",
                table: "contas_a_pagar_itens",
                column: "conta_bancaria_id");

            migrationBuilder.CreateIndex(
                name: "i_x_contas_a_pagar_itens_contas_a_pagar_id",
                schema: "financas",
                table: "contas_a_pagar_itens",
                column: "contas_a_pagar_id");

            migrationBuilder.CreateIndex(
                name: "i_x_contas_a_pagar_itens_plano_de_contas_financeiro_item_id",
                schema: "financas",
                table: "contas_a_pagar_itens",
                column: "plano_de_contas_financeiro_item_id");

            migrationBuilder.CreateIndex(
                name: "ix__contas_a_pagar_item_sync_id",
                schema: "financas",
                table: "contas_a_pagar_itens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__contas_a_pagar_item_tenant_id",
                schema: "financas",
                table: "contas_a_pagar_itens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_contas_a_pagar_item_tenant_conta",
                schema: "financas",
                table: "contas_a_pagar_itens",
                columns: new[] { "tenant_id", "contas_a_pagar_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_contas_a_receber_fato_gerador_financeiro_id",
                schema: "financas",
                table: "contas_a_receber",
                column: "fato_gerador_financeiro_id");

            migrationBuilder.CreateIndex(
                name: "i_x_contas_a_receber_plano_de_contas_financeiro_item_id",
                schema: "financas",
                table: "contas_a_receber",
                column: "plano_de_contas_financeiro_item_id");

            migrationBuilder.CreateIndex(
                name: "ix__contas_a_receber_sync_id",
                schema: "financas",
                table: "contas_a_receber",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__contas_a_receber_tenant_id",
                schema: "financas",
                table: "contas_a_receber",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_contas_a_receber_tenant_pessoa",
                schema: "financas",
                table: "contas_a_receber",
                columns: new[] { "tenant_id", "pessoa_id" });

            migrationBuilder.CreateIndex(
                name: "ix_contas_a_receber_tenant_vencimento",
                schema: "financas",
                table: "contas_a_receber",
                columns: new[] { "tenant_id", "data_vencimento" });

            migrationBuilder.CreateIndex(
                name: "i_x_contas_a_receber_itens_conta_bancaria_id",
                schema: "financas",
                table: "contas_a_receber_itens",
                column: "conta_bancaria_id");

            migrationBuilder.CreateIndex(
                name: "i_x_contas_a_receber_itens_contas_a_receber_id",
                schema: "financas",
                table: "contas_a_receber_itens",
                column: "contas_a_receber_id");

            migrationBuilder.CreateIndex(
                name: "i_x_contas_a_receber_itens_plano_de_contas_financeiro_item_id",
                schema: "financas",
                table: "contas_a_receber_itens",
                column: "plano_de_contas_financeiro_item_id");

            migrationBuilder.CreateIndex(
                name: "ix__contas_a_receber_item_sync_id",
                schema: "financas",
                table: "contas_a_receber_itens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__contas_a_receber_item_tenant_id",
                schema: "financas",
                table: "contas_a_receber_itens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_contas_a_receber_item_tenant_conta",
                schema: "financas",
                table: "contas_a_receber_itens",
                columns: new[] { "tenant_id", "contas_a_receber_id" });

            migrationBuilder.CreateIndex(
                name: "ix__fato_gerador_financeiro_sync_id",
                schema: "financas",
                table: "fatos_geradores_financeiros",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fato_gerador_financeiro_tenant_id",
                schema: "financas",
                table: "fatos_geradores_financeiros",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_fato_gerador_tenant_origem",
                schema: "financas",
                table: "fatos_geradores_financeiros",
                columns: new[] { "tenant_id", "origem" });

            migrationBuilder.CreateIndex(
                name: "ix__importacacao_arquivo_ofx_sync_id",
                schema: "financas",
                table: "importacoes_arquivo_ofx",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__importacacao_arquivo_ofx_tenant_id",
                schema: "financas",
                table: "importacoes_arquivo_ofx",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_importacao_ofx_tenant_conta",
                schema: "financas",
                table: "importacoes_arquivo_ofx",
                columns: new[] { "tenant_id", "numero_conta" });

            migrationBuilder.CreateIndex(
                name: "i_x_importacoes_arquivo_ofx_transacoes_contas_a_pagar_id",
                schema: "financas",
                table: "importacoes_arquivo_ofx_transacoes",
                column: "contas_a_pagar_id");

            migrationBuilder.CreateIndex(
                name: "i_x_importacoes_arquivo_ofx_transacoes_contas_a_receber_id",
                schema: "financas",
                table: "importacoes_arquivo_ofx_transacoes",
                column: "contas_a_receber_id");

            migrationBuilder.CreateIndex(
                name: "i_x_importacoes_arquivo_ofx_transacoes_importacacao_arquivo_ofx~",
                schema: "financas",
                table: "importacoes_arquivo_ofx_transacoes",
                column: "importacacao_arquivo_ofx_id");

            migrationBuilder.CreateIndex(
                name: "ix__importacacao_arquivo_ofx_transacao_sync_id",
                schema: "financas",
                table: "importacoes_arquivo_ofx_transacoes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__importacacao_arquivo_ofx_transacao_tenant_id",
                schema: "financas",
                table: "importacoes_arquivo_ofx_transacoes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ofx_transacao_tenant_conciliado",
                schema: "financas",
                table: "importacoes_arquivo_ofx_transacoes",
                columns: new[] { "tenant_id", "conciliado" });

            migrationBuilder.CreateIndex(
                name: "i_x_plano_de_contas_empresas_plano_de_contas_financeiro_id",
                schema: "financas",
                table: "plano_de_contas_empresas",
                column: "plano_de_contas_financeiro_id");

            migrationBuilder.CreateIndex(
                name: "ix__plano_de_contas_financeiro_empresa_sync_id",
                schema: "financas",
                table: "plano_de_contas_empresas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__plano_de_contas_financeiro_empresa_tenant_id",
                schema: "financas",
                table: "plano_de_contas_empresas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_plano_empresa_tenant_empresa",
                schema: "financas",
                table: "plano_de_contas_empresas",
                columns: new[] { "tenant_id", "empresa_id" });

            migrationBuilder.CreateIndex(
                name: "ix_plano_empresa_tenant_plano_empresa",
                schema: "financas",
                table: "plano_de_contas_empresas",
                columns: new[] { "tenant_id", "plano_de_contas_financeiro_id", "empresa_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contas_a_pagar_itens",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "contas_a_receber_itens",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "importacoes_arquivo_ofx_transacoes",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "plano_de_contas_empresas",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "contas_a_pagar",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "contas_a_receber",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "importacoes_arquivo_ofx",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "fatos_geradores_financeiros",
                schema: "financas");

            migrationBuilder.DropIndex(
                name: "ix_natureza_tenant_empresa",
                schema: "financas",
                table: "naturezas_financeiras");

            migrationBuilder.DropIndex(
                name: "ix_conta_bancaria_tenant_empresa",
                schema: "financas",
                table: "contas_bancarias");

            migrationBuilder.DropColumn(
                name: "sequencia_exibicao",
                schema: "financas",
                table: "planos_de_contas");

            migrationBuilder.DropColumn(
                name: "sequencia_exibicao",
                schema: "financas",
                table: "plano_de_contas_itens");

            migrationBuilder.DropColumn(
                name: "empresa_id",
                schema: "financas",
                table: "naturezas_financeiras");

            migrationBuilder.DropColumn(
                name: "sequencia_exibicao",
                schema: "financas",
                table: "naturezas_financeiras");

            migrationBuilder.DropColumn(
                name: "empresa_id",
                schema: "financas",
                table: "contas_bancarias");

            migrationBuilder.DropColumn(
                name: "sequencia_exibicao",
                schema: "financas",
                table: "contas_bancarias");

            migrationBuilder.DropColumn(
                name: "sequencia_exibicao",
                schema: "financas",
                table: "cartoes_de_credito");
        }
    }
}
