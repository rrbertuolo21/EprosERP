using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Estoque.Migrations
{
    /// <inheritdoc />
    public partial class AddSourcingLogisticaEntradaEstoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "lde_documento_entrada_faturas",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    documento_entrada_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    valor_original = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_desconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_liquido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_lde_documento_entrada_faturas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "lde_documento_entrada_transportes",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    documento_entrada_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transportador_id = table.Column<Guid>(type: "uuid", nullable: true),
                    modalidade_frete = table.Column<int>(type: "integer", nullable: true),
                    referencia_transporte = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
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
                    table.PrimaryKey("p_k_lde_documento_entrada_transportes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "lde_documentos_entrada",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    chave_acesso = table.Column<string>(type: "character varying(44)", maxLength: 44, nullable: true),
                    numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    serie = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    data_emissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    natureza_operacao = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    fornecedor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    destinatario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    emitente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    transporte_id = table.Column<Guid>(type: "uuid", nullable: true),
                    fatura_id = table.Column<Guid>(type: "uuid", nullable: true),
                    situacao = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
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
                    table.PrimaryKey("p_k_lde_documentos_entrada", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "lde_entradas",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fornecedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    local_entrega_id = table.Column<Guid>(type: "uuid", nullable: true),
                    documento_entrada_id = table.Column<Guid>(type: "uuid", nullable: true),
                    situacao = table.Column<int>(type: "integer", nullable: false),
                    data_confirmacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    motivo_cancelamento_estorno = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("p_k_lde_entradas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "lde_historicos",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entrada_id = table.Column<Guid>(type: "uuid", nullable: false),
                    evento = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    situacao_anterior = table.Column<int>(type: "integer", nullable: true),
                    situacao_nova = table.Column<int>(type: "integer", nullable: false),
                    motivo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    usuario_id = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
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
                    table.PrimaryKey("p_k_lde_historicos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "lde_locais_entrega_compra",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    fone = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    email = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    inscricao_estadual = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    documento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    uf = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    logradouro = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    numero = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    complemento = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    bairro = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    municipio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    municipio_nome = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    cep = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    pais_id = table.Column<int>(type: "integer", nullable: false),
                    pais_nome = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
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
                    table.PrimaryKey("p_k_lde_locais_entrega_compra", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sc_cotacao_pedido_itens",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cotacao_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pedido_compra_item_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_sc_cotacao_pedido_itens", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sc_cotacoes",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_cotacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    situacao = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
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
                    table.PrimaryKey("p_k_sc_cotacoes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sc_tipos_pedido",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_sc_tipos_pedido", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sc_tipos_requisicao",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_sc_tipos_requisicao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "lde_documento_entrada_duplicatas",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    documento_entrada_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    data_vencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    titulo_pagar_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_lde_documento_entrada_duplicatas", x => x.id);
                    table.ForeignKey(
                        name: "f_k_lde_documento_entrada_duplicatas_lde_documentos_entrada_doc~",
                        column: x => x.documento_entrada_id,
                        principalSchema: "estoque",
                        principalTable: "lde_documentos_entrada",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lde_documento_entrada_itens",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    documento_entrada_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quantidade_documento = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_item = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    dados_tributarios_item = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
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
                    table.PrimaryKey("p_k_lde_documento_entrada_itens", x => x.id);
                    table.ForeignKey(
                        name: "f_k_lde_documento_entrada_itens_lde_documentos_entrada_document~",
                        column: x => x.documento_entrada_id,
                        principalSchema: "estoque",
                        principalTable: "lde_documentos_entrada",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sc_cotacao_fornecedores",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cotacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fornecedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    prazo_entrega = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    condicoes_pagamento = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    desconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_sc_cotacao_fornecedores", x => x.id);
                    table.ForeignKey(
                        name: "f_k_sc_cotacao_fornecedores_sc_cotacoes_cotacao_id",
                        column: x => x.cotacao_id,
                        principalSchema: "estoque",
                        principalTable: "sc_cotacoes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sc_cotacao_itens",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cotacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cotacao_fornecedor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_sc_cotacao_itens", x => x.id);
                    table.ForeignKey(
                        name: "f_k_sc_cotacao_itens_produtos_produto_id",
                        column: x => x.produto_id,
                        principalSchema: "estoque",
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_sc_cotacao_itens_sc_cotacoes_cotacao_id",
                        column: x => x.cotacao_id,
                        principalSchema: "estoque",
                        principalTable: "sc_cotacoes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sc_pedidos_compra",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_pedido_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fornecedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cotacao_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_pedido = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_prevista_entrega = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_previsao_pagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    local_entrega = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    local_cobranca = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    contato = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    forma_pagamento = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    quantidade_parcelas = table.Column<int>(type: "integer", nullable: true),
                    dias_primeiro_vencimento = table.Column<int>(type: "integer", nullable: true),
                    dias_intervalo = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("p_k_sc_pedidos_compra", x => x.id);
                    table.ForeignKey(
                        name: "f_k_sc_pedidos_compra__sc_tipos_pedido_tipo_pedido_id",
                        column: x => x.tipo_pedido_id,
                        principalSchema: "estoque",
                        principalTable: "sc_tipos_pedido",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sc_requisicoes",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_requisicao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_requisicao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_sc_requisicoes", x => x.id);
                    table.ForeignKey(
                        name: "f_k_sc_requisicoes__sc_tipos_requisicao_tipo_requisicao_id",
                        column: x => x.tipo_requisicao_id,
                        principalSchema: "estoque",
                        principalTable: "sc_tipos_requisicao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sc_pedido_compra_itens",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pedido_compra_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_desconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_frete = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_seguro = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_outras_despesas = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_ipi = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_icms = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_sc_pedido_compra_itens", x => x.id);
                    table.ForeignKey(
                        name: "f_k_sc_pedido_compra_itens_produtos_produto_id",
                        column: x => x.produto_id,
                        principalSchema: "estoque",
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_sc_pedido_compra_itens_sc_pedidos_compra_pedido_compra_id",
                        column: x => x.pedido_compra_id,
                        principalSchema: "estoque",
                        principalTable: "sc_pedidos_compra",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sc_requisicao_itens",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    requisicao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    quantidade_cotada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    item_cotado = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
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
                    table.PrimaryKey("p_k_sc_requisicao_itens", x => x.id);
                    table.ForeignKey(
                        name: "f_k_sc_requisicao_itens_produtos_produto_id",
                        column: x => x.produto_id,
                        principalSchema: "estoque",
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_sc_requisicao_itens_sc_requisicoes_requisicao_id",
                        column: x => x.requisicao_id,
                        principalSchema: "estoque",
                        principalTable: "sc_requisicoes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_lde_documento_entrada_duplicatas_documento_entrada_id",
                schema: "estoque",
                table: "lde_documento_entrada_duplicatas",
                column: "documento_entrada_id");

            migrationBuilder.CreateIndex(
                name: "i_x_lde_documento_entrada_duplicatas_tenant_id_documento_entrad~",
                schema: "estoque",
                table: "lde_documento_entrada_duplicatas",
                columns: new[] { "tenant_id", "documento_entrada_id" });

            migrationBuilder.CreateIndex(
                name: "ix__lde_documento_entrada_duplicata_sync_id",
                schema: "estoque",
                table: "lde_documento_entrada_duplicatas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__lde_documento_entrada_duplicata_tenant_id",
                schema: "estoque",
                table: "lde_documento_entrada_duplicatas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_lde_documento_entrada_faturas_tenant_id_documento_entrada_id",
                schema: "estoque",
                table: "lde_documento_entrada_faturas",
                columns: new[] { "tenant_id", "documento_entrada_id" });

            migrationBuilder.CreateIndex(
                name: "ix__lde_documento_entrada_fatura_sync_id",
                schema: "estoque",
                table: "lde_documento_entrada_faturas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__lde_documento_entrada_fatura_tenant_id",
                schema: "estoque",
                table: "lde_documento_entrada_faturas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_lde_documento_entrada_itens_documento_entrada_id",
                schema: "estoque",
                table: "lde_documento_entrada_itens",
                column: "documento_entrada_id");

            migrationBuilder.CreateIndex(
                name: "i_x_lde_documento_entrada_itens_tenant_id_documento_entrada_id",
                schema: "estoque",
                table: "lde_documento_entrada_itens",
                columns: new[] { "tenant_id", "documento_entrada_id" });

            migrationBuilder.CreateIndex(
                name: "ix__lde_documento_entrada_item_sync_id",
                schema: "estoque",
                table: "lde_documento_entrada_itens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__lde_documento_entrada_item_tenant_id",
                schema: "estoque",
                table: "lde_documento_entrada_itens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_lde_documento_entrada_transportes_tenant_id_documento_entra~",
                schema: "estoque",
                table: "lde_documento_entrada_transportes",
                columns: new[] { "tenant_id", "documento_entrada_id" });

            migrationBuilder.CreateIndex(
                name: "ix__lde_documento_entrada_transporte_sync_id",
                schema: "estoque",
                table: "lde_documento_entrada_transportes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__lde_documento_entrada_transporte_tenant_id",
                schema: "estoque",
                table: "lde_documento_entrada_transportes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_lde_documentos_entrada_tenant_id_chave_acesso",
                schema: "estoque",
                table: "lde_documentos_entrada",
                columns: new[] { "tenant_id", "chave_acesso" });

            migrationBuilder.CreateIndex(
                name: "ix__lde_documento_entrada_sync_id",
                schema: "estoque",
                table: "lde_documentos_entrada",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__lde_documento_entrada_tenant_id",
                schema: "estoque",
                table: "lde_documentos_entrada",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_lde_entradas_tenant_id_compra_id",
                schema: "estoque",
                table: "lde_entradas",
                columns: new[] { "tenant_id", "compra_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_lde_entradas_tenant_id_situacao",
                schema: "estoque",
                table: "lde_entradas",
                columns: new[] { "tenant_id", "situacao" });

            migrationBuilder.CreateIndex(
                name: "ix__lde_entrada_sync_id",
                schema: "estoque",
                table: "lde_entradas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__lde_entrada_tenant_id",
                schema: "estoque",
                table: "lde_entradas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_lde_historicos_tenant_id_entrada_id",
                schema: "estoque",
                table: "lde_historicos",
                columns: new[] { "tenant_id", "entrada_id" });

            migrationBuilder.CreateIndex(
                name: "ix__lde_historico_sync_id",
                schema: "estoque",
                table: "lde_historicos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__lde_historico_tenant_id",
                schema: "estoque",
                table: "lde_historicos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_lde_locais_entrega_compra_tenant_id_compra_id",
                schema: "estoque",
                table: "lde_locais_entrega_compra",
                columns: new[] { "tenant_id", "compra_id" });

            migrationBuilder.CreateIndex(
                name: "ix__lde_local_entrega_compra_sync_id",
                schema: "estoque",
                table: "lde_locais_entrega_compra",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__lde_local_entrega_compra_tenant_id",
                schema: "estoque",
                table: "lde_locais_entrega_compra",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_sc_cotacao_fornecedores_cotacao_id",
                schema: "estoque",
                table: "sc_cotacao_fornecedores",
                column: "cotacao_id");

            migrationBuilder.CreateIndex(
                name: "i_x_sc_cotacao_fornecedores_tenant_id_cotacao_id",
                schema: "estoque",
                table: "sc_cotacao_fornecedores",
                columns: new[] { "tenant_id", "cotacao_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_sc_cotacao_fornecedores_tenant_id_fornecedor_id",
                schema: "estoque",
                table: "sc_cotacao_fornecedores",
                columns: new[] { "tenant_id", "fornecedor_id" });

            migrationBuilder.CreateIndex(
                name: "ix__sc_cotacao_fornecedor_sync_id",
                schema: "estoque",
                table: "sc_cotacao_fornecedores",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sc_cotacao_fornecedor_tenant_id",
                schema: "estoque",
                table: "sc_cotacao_fornecedores",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_sc_cotacao_itens_cotacao_id",
                schema: "estoque",
                table: "sc_cotacao_itens",
                column: "cotacao_id");

            migrationBuilder.CreateIndex(
                name: "i_x_sc_cotacao_itens_produto_id",
                schema: "estoque",
                table: "sc_cotacao_itens",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "i_x_sc_cotacao_itens_tenant_id_cotacao_id",
                schema: "estoque",
                table: "sc_cotacao_itens",
                columns: new[] { "tenant_id", "cotacao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__sc_cotacao_item_sync_id",
                schema: "estoque",
                table: "sc_cotacao_itens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sc_cotacao_item_tenant_id",
                schema: "estoque",
                table: "sc_cotacao_itens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_sc_cotacao_pedido_itens_tenant_id_cotacao_item_id",
                schema: "estoque",
                table: "sc_cotacao_pedido_itens",
                columns: new[] { "tenant_id", "cotacao_item_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_sc_cotacao_pedido_itens_tenant_id_pedido_compra_item_id",
                schema: "estoque",
                table: "sc_cotacao_pedido_itens",
                columns: new[] { "tenant_id", "pedido_compra_item_id" });

            migrationBuilder.CreateIndex(
                name: "ix__sc_cotacao_pedido_item_sync_id",
                schema: "estoque",
                table: "sc_cotacao_pedido_itens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sc_cotacao_pedido_item_tenant_id",
                schema: "estoque",
                table: "sc_cotacao_pedido_itens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_sc_cotacoes_tenant_id_situacao",
                schema: "estoque",
                table: "sc_cotacoes",
                columns: new[] { "tenant_id", "situacao" });

            migrationBuilder.CreateIndex(
                name: "ix__sc_cotacao_sync_id",
                schema: "estoque",
                table: "sc_cotacoes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sc_cotacao_tenant_id",
                schema: "estoque",
                table: "sc_cotacoes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_sc_pedido_compra_itens_pedido_compra_id",
                schema: "estoque",
                table: "sc_pedido_compra_itens",
                column: "pedido_compra_id");

            migrationBuilder.CreateIndex(
                name: "i_x_sc_pedido_compra_itens_produto_id",
                schema: "estoque",
                table: "sc_pedido_compra_itens",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "i_x_sc_pedido_compra_itens_tenant_id_pedido_compra_id",
                schema: "estoque",
                table: "sc_pedido_compra_itens",
                columns: new[] { "tenant_id", "pedido_compra_id" });

            migrationBuilder.CreateIndex(
                name: "ix__sc_pedido_compra_item_sync_id",
                schema: "estoque",
                table: "sc_pedido_compra_itens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sc_pedido_compra_item_tenant_id",
                schema: "estoque",
                table: "sc_pedido_compra_itens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_sc_pedidos_compra_tenant_id_cotacao_id",
                schema: "estoque",
                table: "sc_pedidos_compra",
                columns: new[] { "tenant_id", "cotacao_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_sc_pedidos_compra_tenant_id_fornecedor_id",
                schema: "estoque",
                table: "sc_pedidos_compra",
                columns: new[] { "tenant_id", "fornecedor_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_sc_pedidos_compra_tipo_pedido_id",
                schema: "estoque",
                table: "sc_pedidos_compra",
                column: "tipo_pedido_id");

            migrationBuilder.CreateIndex(
                name: "ix__sc_pedido_compra_sync_id",
                schema: "estoque",
                table: "sc_pedidos_compra",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sc_pedido_compra_tenant_id",
                schema: "estoque",
                table: "sc_pedidos_compra",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_sc_requisicao_itens_produto_id",
                schema: "estoque",
                table: "sc_requisicao_itens",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "i_x_sc_requisicao_itens_requisicao_id",
                schema: "estoque",
                table: "sc_requisicao_itens",
                column: "requisicao_id");

            migrationBuilder.CreateIndex(
                name: "i_x_sc_requisicao_itens_tenant_id_requisicao_id",
                schema: "estoque",
                table: "sc_requisicao_itens",
                columns: new[] { "tenant_id", "requisicao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__sc_requisicao_item_sync_id",
                schema: "estoque",
                table: "sc_requisicao_itens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sc_requisicao_item_tenant_id",
                schema: "estoque",
                table: "sc_requisicao_itens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_sc_requisicoes_tenant_id_colaborador_id",
                schema: "estoque",
                table: "sc_requisicoes",
                columns: new[] { "tenant_id", "colaborador_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_sc_requisicoes_tipo_requisicao_id",
                schema: "estoque",
                table: "sc_requisicoes",
                column: "tipo_requisicao_id");

            migrationBuilder.CreateIndex(
                name: "ix__sc_requisicao_sync_id",
                schema: "estoque",
                table: "sc_requisicoes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sc_requisicao_tenant_id",
                schema: "estoque",
                table: "sc_requisicoes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_sc_tipos_pedido_tenant_id_descricao",
                schema: "estoque",
                table: "sc_tipos_pedido",
                columns: new[] { "tenant_id", "descricao" });

            migrationBuilder.CreateIndex(
                name: "ix__sc_tipo_pedido_sync_id",
                schema: "estoque",
                table: "sc_tipos_pedido",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sc_tipo_pedido_tenant_id",
                schema: "estoque",
                table: "sc_tipos_pedido",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_sc_tipos_requisicao_tenant_id_descricao",
                schema: "estoque",
                table: "sc_tipos_requisicao",
                columns: new[] { "tenant_id", "descricao" });

            migrationBuilder.CreateIndex(
                name: "ix__sc_tipo_requisicao_sync_id",
                schema: "estoque",
                table: "sc_tipos_requisicao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sc_tipo_requisicao_tenant_id",
                schema: "estoque",
                table: "sc_tipos_requisicao",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lde_documento_entrada_duplicatas",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "lde_documento_entrada_faturas",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "lde_documento_entrada_itens",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "lde_documento_entrada_transportes",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "lde_entradas",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "lde_historicos",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "lde_locais_entrega_compra",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "sc_cotacao_fornecedores",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "sc_cotacao_itens",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "sc_cotacao_pedido_itens",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "sc_pedido_compra_itens",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "sc_requisicao_itens",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "lde_documentos_entrada",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "sc_cotacoes",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "sc_pedidos_compra",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "sc_requisicoes",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "sc_tipos_pedido",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "sc_tipos_requisicao",
                schema: "estoque");
        }
    }
}
