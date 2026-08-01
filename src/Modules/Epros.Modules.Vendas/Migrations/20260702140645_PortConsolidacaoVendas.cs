using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Vendas.Migrations
{
    /// <inheritdoc />
    public partial class PortConsolidacaoVendas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "natureza_operacao",
                schema: "vendas",
                table: "vendas",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "caminho_pdf_cupom_nao_fiscal",
                schema: "vendas",
                table: "vendas",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "data_ultimo_processamento",
                schema: "vendas",
                table: "vendas",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "documento_fiscal_id",
                schema: "vendas",
                table: "vendas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "numero_cupom_nao_fiscal",
                schema: "vendas",
                table: "vendas",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "cfop_correlacao",
                schema: "vendas",
                table: "venda_itens",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "codigo_beneficio_fiscal",
                schema: "vendas",
                table: "venda_itens",
                type: "character varying(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "codigo_ean_tributavel",
                schema: "vendas",
                table: "venda_itens",
                type: "character varying(14)",
                maxLength: 14,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "compoe_valor_total",
                schema: "vendas",
                table: "venda_itens",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "excecao_ncm_tipi",
                schema: "vendas",
                table: "venda_itens",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ficha_conteudo_importacao",
                schema: "vendas",
                table: "venda_itens",
                type: "character varying(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "informacoes_adicionais_do_produto",
                schema: "vendas",
                table: "venda_itens",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "integra_faturamento",
                schema: "vendas",
                table: "venda_itens",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "numero_item_pedido_compra",
                schema: "vendas",
                table: "venda_itens",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "numero_pedido_compra",
                schema: "vendas",
                table: "venda_itens",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "quantidade_tributavel",
                schema: "vendas",
                table: "venda_itens",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "unidade_tributavel",
                schema: "vendas",
                table: "venda_itens",
                type: "character varying(6)",
                maxLength: 6,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "valor_desconto_rateado",
                schema: "vendas",
                table: "venda_itens",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_outras_depesas_acessorias_rateado",
                schema: "vendas",
                table: "venda_itens",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_seguro_rateado",
                schema: "vendas",
                table: "venda_itens",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_unitario_tributavel",
                schema: "vendas",
                table: "venda_itens",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "fatos_geradores_financeiros",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    origem = table.Column<int>(type: "integer", nullable: false),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: true),
                    compra_id = table.Column<Guid>(type: "uuid", nullable: true),
                    descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    table.ForeignKey(
                        name: "f_k_fatos_geradores_financeiros__vendas_venda_id",
                        column: x => x.venda_id,
                        principalSchema: "vendas",
                        principalTable: "vendas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venda_autorizacoes_xml",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: false),
                    documento = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
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
                    table.PrimaryKey("p_k_venda_autorizacoes_xml", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_autorizacoes_xml_vendas_venda_id",
                        column: x => x.venda_id,
                        principalSchema: "vendas",
                        principalTable: "vendas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venda_cobranca_enderecos",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    fone = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    email = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    i_e = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    documento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    uf = table.Column<int>(type: "integer", nullable: false),
                    logradouro = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    numero = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    complemento = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    bairro = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    municipio_id = table.Column<int>(type: "integer", nullable: false),
                    municipio_nome = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    cep = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    pais_id = table.Column<int>(type: "integer", nullable: false),
                    pais_nome = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    endereco_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_venda_cobranca_enderecos", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_cobranca_enderecos_vendas_venda_id",
                        column: x => x.venda_id,
                        principalSchema: "vendas",
                        principalTable: "vendas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venda_configuracoes",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_operacao = table.Column<int>(type: "integer", nullable: false),
                    tipo_formato_impressao_danfe = table.Column<int>(type: "integer", nullable: false),
                    tipo_emissao = table.Column<int>(type: "integer", nullable: false),
                    tipo_ambiente = table.Column<int>(type: "integer", nullable: false),
                    finalidade_emissao = table.Column<int>(type: "integer", nullable: false),
                    indicador_finalidade_operacao = table.Column<int>(type: "integer", nullable: false),
                    tipo_atendimento = table.Column<int>(type: "integer", nullable: false),
                    indicador_intermediador_marketplace = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_venda_configuracoes", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_configuracoes_vendas_venda_id",
                        column: x => x.venda_id,
                        principalSchema: "vendas",
                        principalTable: "vendas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venda_destinatarios",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cnpj = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    cpf = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    razao_social = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    telefone = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    inscricao_estadual = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    identificador_estrangeiro = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    indicador_i_e = table.Column<int>(type: "integer", nullable: false),
                    email = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    eh_consumidor_final = table.Column<bool>(type: "boolean", nullable: false),
                    enviar_destinatatio_na_nfce = table.Column<bool>(type: "boolean", nullable: false),
                    documento_consumidor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("p_k_venda_destinatarios", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_destinatarios_vendas_venda_id",
                        column: x => x.venda_id,
                        principalSchema: "vendas",
                        principalTable: "vendas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venda_emitentes",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cnpj = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    cpf = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    razao_social = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    nome_fantasia = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    telefone = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    inscricao_estadual = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    inscricao_estadual_s_t = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    inscricao_municipal = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    cnae = table.Column<int>(type: "integer", nullable: false),
                    regime_tributario = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_venda_emitentes", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_emitentes_vendas_venda_id",
                        column: x => x.venda_id,
                        principalSchema: "vendas",
                        principalTable: "vendas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venda_entregas",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    fone = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    email = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    i_e = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    documento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    uf = table.Column<int>(type: "integer", nullable: false),
                    logradouro = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    numero = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    complemento = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    bairro = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    municipio_id = table.Column<int>(type: "integer", nullable: false),
                    municipio_nome = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    cep = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    pais_id = table.Column<int>(type: "integer", nullable: false),
                    pais_nome = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    endereco_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_venda_entregas", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_entregas_vendas_venda_id",
                        column: x => x.venda_id,
                        principalSchema: "vendas",
                        principalTable: "vendas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venda_faturas",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_fatura = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    valor_original = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_desconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_liquido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_venda_faturas", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_faturas_vendas_venda_id",
                        column: x => x.venda_id,
                        principalSchema: "vendas",
                        principalTable: "vendas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venda_impostos",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor_aliquota_credito_icms = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_venda_impostos", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_impostos_vendas_venda_id",
                        column: x => x.venda_id,
                        principalSchema: "vendas",
                        principalTable: "vendas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venda_item_combustiveis",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_anp = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    descricao_anp = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    quantidade_combustivel_faturada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    uf_consumo = table.Column<int>(type: "integer", nullable: false),
                    percentual_glp_derivado_petroleo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    percentual_gas_natural_nacional = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    percentual_gas_natural_importado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_partida = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_venda_item_combustiveis", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_item_combustiveis_venda_itens_venda_item_id",
                        column: x => x.venda_item_id,
                        principalSchema: "vendas",
                        principalTable: "venda_itens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venda_item_impostos",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    origem = table.Column<int>(type: "integer", nullable: false),
                    cst_icms = table.Column<int>(type: "integer", nullable: false),
                    csosn = table.Column<int>(type: "integer", nullable: false),
                    modalidade_determinacao_base_calculo_icms = table.Column<int>(type: "integer", nullable: false),
                    valor_base_de_calculo_icms = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    percentual_reducao_base_de_calculo_icms = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aliquota_icms = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_imposto_icms = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    modalidade_base_de_calculos_s_t = table.Column<int>(type: "integer", nullable: false),
                    percentual_mva_base_de_calculo_s_t = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    percentual_reducao_base_de_calculo_s_t = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_base_de_calculo_st = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aliquota_st = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_imposto_st = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    motivo_desoneracao_icms = table.Column<int>(type: "integer", nullable: false),
                    valor_base_de_calculo_st_retido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_imposto_st_retido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    percentual_credito_simples_nacional_icms = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_imposto_credito_simples_nacional_icms = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_base_de_calculo_fcp = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    percentual_fcp = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_imposto_fcp = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_operacao_diferimento_icms = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    percentual_diferimento_icms = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_imposto_diferimento_icms = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    cst_ipi_saida = table.Column<int>(type: "integer", nullable: false),
                    valor_base_de_calculo_ipi = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aliquota_ipi = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_imposto_diferimento_ipi = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_quantidade_total_para_tributacao_ipi = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_por_unidade_tributavel_ipi = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    cst_pis = table.Column<int>(type: "integer", nullable: false),
                    valor_base_de_calculo_pis = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aliquota_pis = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_quantidade_vendida_produto_pis = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aliquota_por_unidade_vendida_pis = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_imposto_diferimento_pis = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    cst_cofins = table.Column<int>(type: "integer", nullable: false),
                    valor_base_de_calculo_cofins = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aliquota_cofins = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_quantidade_vendida_produto_cofins = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aliquota_por_unidade_vendida_cofins = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_imposto_diferimento_cofins = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tipo_reducao_icms = table.Column<int>(type: "integer", nullable: false),
                    tipo_reducao_icms_st = table.Column<int>(type: "integer", nullable: false),
                    valor_base_de_calculo_fcp_st = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    percentual_fcp_st = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_imposto_fcp_st = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_icms_proprio_subistituto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_aliquota_icms_interna = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_aliquota_icms_interna_estadual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    enquadramento_ipi = table.Column<int>(type: "integer", nullable: false),
                    valor_reducao_ipi_percentual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ipi_embutido = table.Column<bool>(type: "boolean", nullable: false),
                    difal_tipo_calculo_por_dentro = table.Column<bool>(type: "boolean", nullable: false),
                    tipo_reducao_ipi = table.Column<int>(type: "integer", nullable: false),
                    tipo_calculo_base_icms_st = table.Column<int>(type: "integer", nullable: false),
                    valor_unit_fixado_icms_st = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_base_de_calculo_difal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_imposto_devido_difal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_imposto_devido_recolher_st = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_imposto_devido_fcp = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_icms_isento = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_icms_outros = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    icms_observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    valor_ipi_isento = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_ipi_outros = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ipi_observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("p_k_venda_item_impostos", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_item_impostos_venda_itens_venda_item_id",
                        column: x => x.venda_item_id,
                        principalSchema: "vendas",
                        principalTable: "venda_itens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venda_item_impostos_ibs_cbs",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cst = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    c_class_trib = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    aliquota_estadual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aliquota_municipal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aliquota_cbs = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aliquota_estadual_reducao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aliquota_municipal_reducao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aliquota_cbs_reducao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aliquota_estadual_diferimento = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aliquota_municipal_diferimento = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aliquota_cbs_diferimento = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aliquota_efetiva_estadual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aliquota_efetiva_municipal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aliquota_efetiva_cbs = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_base_de_calculo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_imposto_devido_estadual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_imposto_devido_municipal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_imposto_devido_cbs = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_venda_item_impostos_ibs_cbs", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_item_impostos_ibs_cbs_venda_itens_venda_item_id",
                        column: x => x.venda_item_id,
                        principalSchema: "vendas",
                        principalTable: "venda_itens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venda_item_impostos_valor_aproximado",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    aliquota_nacional_federal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aliquota_importado_federal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aliquota_estadual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aliquota_municipal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    versao = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    fonte = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
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
                    table.PrimaryKey("p_k_venda_item_impostos_valor_aproximado", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_item_impostos_valor_aproximado_venda_itens_venda_item~",
                        column: x => x.venda_item_id,
                        principalSchema: "vendas",
                        principalTable: "venda_itens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venda_nf_historicos",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
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
                    table.PrimaryKey("p_k_venda_nf_historicos", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_nf_historicos_vendas_venda_id",
                        column: x => x.venda_id,
                        principalSchema: "vendas",
                        principalTable: "vendas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venda_nfces",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<long>(type: "bigint", nullable: false),
                    serie = table.Column<int>(type: "integer", nullable: false),
                    id_csc = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    csc = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    status_interno = table.Column<int>(type: "integer", nullable: false),
                    chave = table.Column<string>(type: "character varying(44)", maxLength: 44, nullable: true),
                    data_hora_emissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status_sefaz = table.Column<int>(type: "integer", nullable: false),
                    protocolo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    xml = table.Column<string>(type: "text", nullable: true),
                    ultimo_retorno_mensagem_sefaz = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    data_hora_cancelamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    protocolo_cancelamento = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    status_sefaz_cancelamento = table.Column<int>(type: "integer", nullable: false),
                    motivo_cancelamento = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    xml_cancelamento = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_venda_nfces", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_nfces_vendas_venda_id",
                        column: x => x.venda_id,
                        principalSchema: "vendas",
                        principalTable: "vendas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venda_nfe_historicos",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_venda_nfe_historicos", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_nfe_historicos_vendas_venda_id",
                        column: x => x.venda_id,
                        principalSchema: "vendas",
                        principalTable: "vendas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venda_nfe_referenciadas",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: false),
                    chave = table.Column<string>(type: "character varying(44)", maxLength: 44, nullable: false),
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
                    table.PrimaryKey("p_k_venda_nfe_referenciadas", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_nfe_referenciadas_vendas_venda_id",
                        column: x => x.venda_id,
                        principalSchema: "vendas",
                        principalTable: "vendas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venda_nfes",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<long>(type: "bigint", nullable: false),
                    serie = table.Column<int>(type: "integer", nullable: false),
                    data_hora_emissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_hora_saida = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status_interno = table.Column<int>(type: "integer", nullable: false),
                    status_sefaz = table.Column<int>(type: "integer", nullable: false),
                    chave = table.Column<string>(type: "character varying(44)", maxLength: 44, nullable: true),
                    protocolo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    xml = table.Column<string>(type: "text", nullable: true),
                    ultimo_retorno_mensagem_sefaz = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    data_hora_cancelamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    protocolo_cancelamento = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    status_sefaz_cancelamento = table.Column<int>(type: "integer", nullable: false),
                    motivo_cancelamento = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    xml_cancelamento = table.Column<string>(type: "text", nullable: true),
                    embute_frete = table.Column<bool>(type: "boolean", nullable: false),
                    embute_seguro = table.Column<bool>(type: "boolean", nullable: false),
                    embute_acrescimo = table.Column<bool>(type: "boolean", nullable: false),
                    embute_outro = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_venda_nfes", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_nfes_vendas_venda_id",
                        column: x => x.venda_id,
                        principalSchema: "vendas",
                        principalTable: "vendas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venda_pagamentos",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor_troco = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    indicador_pagamento = table.Column<int>(type: "integer", nullable: false),
                    tipo_pagamento = table.Column<int>(type: "integer", nullable: false),
                    valor_pagamento = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    cartao_tipo_integracao = table.Column<int>(type: "integer", nullable: false),
                    cartao_cnpj_intermediador_financeira = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    cartao_bandeira = table.Column<int>(type: "integer", nullable: false),
                    cartao_codigo_autorizacao_operacao = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("p_k_venda_pagamentos", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_pagamentos_vendas_venda_id",
                        column: x => x.venda_id,
                        principalSchema: "vendas",
                        principalTable: "vendas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venda_totais",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor_base_de_calculo_icms = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_icms = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_icms_desonerado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_fcp = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_base_de_calculo_st = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_st = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_fcp_st = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_fcp_retido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_produto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_frete = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_seguro = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_desconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_imposto_importacao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_ipi = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_ipi_devolucao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_pis = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_cofins = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_outro = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_nota_fiscal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_venda_totais", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_totais_vendas_venda_id",
                        column: x => x.venda_id,
                        principalSchema: "vendas",
                        principalTable: "vendas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venda_totais_ibs_cbs",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor_base_de_calculo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_imposto_devido_estadual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_imposto_devido_municipal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_imposto_devido_cbs = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_venda_totais_ibs_cbs", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_totais_ibs_cbs_vendas_venda_id",
                        column: x => x.venda_id,
                        principalSchema: "vendas",
                        principalTable: "vendas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venda_transportes",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_venda_transportes", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_transportes_vendas_venda_id",
                        column: x => x.venda_id,
                        principalSchema: "vendas",
                        principalTable: "vendas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venda_destinatario_enderecos",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_destinatario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_endereco = table.Column<int>(type: "integer", nullable: false),
                    uf = table.Column<int>(type: "integer", nullable: false),
                    logradouro = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    numero = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    complemento = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    bairro = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    municipio_id = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_venda_destinatario_enderecos", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_destinatario_enderecos_venda_destinatarios_venda_dest~",
                        column: x => x.venda_destinatario_id,
                        principalSchema: "vendas",
                        principalTable: "venda_destinatarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venda_emitente_enderecos",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_emitente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    uf = table.Column<int>(type: "integer", nullable: false),
                    logradouro = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    numero = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    complemento = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    bairro = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    municipio_id = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_venda_emitente_enderecos", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_emitente_enderecos_venda_emitentes_venda_emitente_id",
                        column: x => x.venda_emitente_id,
                        principalSchema: "vendas",
                        principalTable: "venda_emitentes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venda_fatura_duplicatas",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_fatura_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_duplicata = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    data_vencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valor_duplicata = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_venda_fatura_duplicatas", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_fatura_duplicatas_venda_faturas_venda_fatura_id",
                        column: x => x.venda_fatura_id,
                        principalSchema: "vendas",
                        principalTable: "venda_faturas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venda_item_combustivel_origens",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_item_combustivel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    indicador_importacao = table.Column<int>(type: "integer", nullable: false),
                    uf_origem = table.Column<int>(type: "integer", nullable: false),
                    percentual_origem = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_venda_item_combustivel_origens", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_item_combustivel_origens_venda_item_combustiveis_vend~",
                        column: x => x.venda_item_combustivel_id,
                        principalSchema: "vendas",
                        principalTable: "venda_item_combustiveis",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venda_item_imposto_ibs_cbs_tributacoes_regulares",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_item_imposto_ibs_cbs_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cst = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    c_class_trib = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    aliquota_efetiva_ibs_estadual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_ibs_estadual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aliquota_efetiva_ibs_municipal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_ibs_municipal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aliquota_efetiva_cbs = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_cbs = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_venda_item_imposto_ibs_cbs_tributacoes_regulares", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_item_imposto_ibs_cbs_tributacoes_regulares_venda_item~",
                        column: x => x.venda_item_imposto_ibs_cbs_id,
                        principalSchema: "vendas",
                        principalTable: "venda_item_impostos_ibs_cbs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venda_nfe_cartas_correcao",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_nfe_id = table.Column<Guid>(type: "uuid", nullable: false),
                    texto_correcao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    sequencia_evento = table.Column<int>(type: "integer", nullable: false),
                    status_sefaz = table.Column<int>(type: "integer", nullable: false),
                    motivo_rejeicao_sefaz = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
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
                    table.PrimaryKey("p_k_venda_nfe_cartas_correcao", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_nfe_cartas_correcao_venda_nfes_venda_nfe_id",
                        column: x => x.venda_nfe_id,
                        principalSchema: "vendas",
                        principalTable: "venda_nfes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venda_nfe_exportacoes",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_nfe_id = table.Column<Guid>(type: "uuid", nullable: false),
                    uf_saida_pais = table.Column<int>(type: "integer", nullable: false),
                    local_exportacao = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    local_despacho = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
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
                    table.PrimaryKey("p_k_venda_nfe_exportacoes", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_nfe_exportacoes_venda_nfes_venda_nfe_id",
                        column: x => x.venda_nfe_id,
                        principalSchema: "vendas",
                        principalTable: "venda_nfes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venda_nfe_intermediadores",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_nfe_id = table.Column<Guid>(type: "uuid", nullable: false),
                    documento = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    identificador_intermediador = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
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
                    table.PrimaryKey("p_k_venda_nfe_intermediadores", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_nfe_intermediadores_venda_nfes_venda_nfe_id",
                        column: x => x.venda_nfe_id,
                        principalSchema: "vendas",
                        principalTable: "venda_nfes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venda_transporte_reboques",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_transporte_id = table.Column<Guid>(type: "uuid", nullable: false),
                    veiculo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    placa = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    uf = table.Column<int>(type: "integer", nullable: false),
                    rntrc = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
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
                    table.PrimaryKey("p_k_venda_transporte_reboques", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_transporte_reboques_venda_transportes_venda_transport~",
                        column: x => x.venda_transporte_id,
                        principalSchema: "vendas",
                        principalTable: "venda_transportes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venda_transporte_transportadoras",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_transporte_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cnpj = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    cpf = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    razao_social = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    inscricao_estadual = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    logradouro = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    numero = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    complemento = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    bairro = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    municipio = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    uf = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_venda_transporte_transportadoras", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_transporte_transportadoras_venda_transportes_venda_tr~",
                        column: x => x.venda_transporte_id,
                        principalSchema: "vendas",
                        principalTable: "venda_transportes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venda_transporte_veiculos",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_transporte_id = table.Column<Guid>(type: "uuid", nullable: false),
                    veiculo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    placa = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    uf = table.Column<int>(type: "integer", nullable: false),
                    rntrc = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
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
                    table.PrimaryKey("p_k_venda_transporte_veiculos", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_transporte_veiculos_venda_transportes_venda_transport~",
                        column: x => x.venda_transporte_id,
                        principalSchema: "vendas",
                        principalTable: "venda_transportes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venda_transporte_volumes",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_transporte_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade_volumes = table.Column<int>(type: "integer", nullable: false),
                    especie = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    numero_volumes = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    peso_liquido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    peso_bruto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    marca = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
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
                    table.PrimaryKey("p_k_venda_transporte_volumes", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_transporte_volumes_venda_transportes_venda_transporte~",
                        column: x => x.venda_transporte_id,
                        principalSchema: "vendas",
                        principalTable: "venda_transportes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_venda_itens_tenant_venda",
                schema: "vendas",
                table: "venda_itens",
                columns: new[] { "tenant_id", "venda_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_fatos_geradores_financeiros_venda_id",
                schema: "vendas",
                table: "fatos_geradores_financeiros",
                column: "venda_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fato_gerador_financeiro_sync_id",
                schema: "vendas",
                table: "fatos_geradores_financeiros",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fato_gerador_financeiro_tenant_id",
                schema: "vendas",
                table: "fatos_geradores_financeiros",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_fato_gerador_fin_tenant_compra",
                schema: "vendas",
                table: "fatos_geradores_financeiros",
                columns: new[] { "tenant_id", "compra_id" });

            migrationBuilder.CreateIndex(
                name: "ix_fato_gerador_fin_tenant_venda",
                schema: "vendas",
                table: "fatos_geradores_financeiros",
                columns: new[] { "tenant_id", "venda_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_venda_autorizacoes_xml_venda_id",
                schema: "vendas",
                table: "venda_autorizacoes_xml",
                column: "venda_id");

            migrationBuilder.CreateIndex(
                name: "ix__venda_autorizacao_xml_sync_id",
                schema: "vendas",
                table: "venda_autorizacoes_xml",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_autorizacao_xml_tenant_id",
                schema: "vendas",
                table: "venda_autorizacoes_xml",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_venda_cobranca_enderecos_venda_id",
                schema: "vendas",
                table: "venda_cobranca_enderecos",
                column: "venda_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_cobranca_endereco_sync_id",
                schema: "vendas",
                table: "venda_cobranca_enderecos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_cobranca_endereco_tenant_id",
                schema: "vendas",
                table: "venda_cobranca_enderecos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_venda_configuracoes_venda_id",
                schema: "vendas",
                table: "venda_configuracoes",
                column: "venda_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_configuracao_sync_id",
                schema: "vendas",
                table: "venda_configuracoes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_configuracao_tenant_id",
                schema: "vendas",
                table: "venda_configuracoes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_venda_destinatario_enderecos_venda_destinatario_id",
                schema: "vendas",
                table: "venda_destinatario_enderecos",
                column: "venda_destinatario_id");

            migrationBuilder.CreateIndex(
                name: "ix__venda_destinatario_endereco_sync_id",
                schema: "vendas",
                table: "venda_destinatario_enderecos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_destinatario_endereco_tenant_id",
                schema: "vendas",
                table: "venda_destinatario_enderecos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_venda_destinatarios_venda_id",
                schema: "vendas",
                table: "venda_destinatarios",
                column: "venda_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_destinatario_sync_id",
                schema: "vendas",
                table: "venda_destinatarios",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_destinatario_tenant_id",
                schema: "vendas",
                table: "venda_destinatarios",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_venda_emitente_enderecos_venda_emitente_id",
                schema: "vendas",
                table: "venda_emitente_enderecos",
                column: "venda_emitente_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_emitente_endereco_sync_id",
                schema: "vendas",
                table: "venda_emitente_enderecos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_emitente_endereco_tenant_id",
                schema: "vendas",
                table: "venda_emitente_enderecos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_venda_emitentes_venda_id",
                schema: "vendas",
                table: "venda_emitentes",
                column: "venda_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_emitente_sync_id",
                schema: "vendas",
                table: "venda_emitentes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_emitente_tenant_id",
                schema: "vendas",
                table: "venda_emitentes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_venda_entregas_venda_id",
                schema: "vendas",
                table: "venda_entregas",
                column: "venda_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_entrega_sync_id",
                schema: "vendas",
                table: "venda_entregas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_entrega_tenant_id",
                schema: "vendas",
                table: "venda_entregas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_venda_fatura_duplicatas_venda_fatura_id",
                schema: "vendas",
                table: "venda_fatura_duplicatas",
                column: "venda_fatura_id");

            migrationBuilder.CreateIndex(
                name: "ix__venda_fatura_duplicata_sync_id",
                schema: "vendas",
                table: "venda_fatura_duplicatas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_fatura_duplicata_tenant_id",
                schema: "vendas",
                table: "venda_fatura_duplicatas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_venda_faturas_venda_id",
                schema: "vendas",
                table: "venda_faturas",
                column: "venda_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_fatura_sync_id",
                schema: "vendas",
                table: "venda_faturas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_fatura_tenant_id",
                schema: "vendas",
                table: "venda_faturas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_venda_impostos_venda_id",
                schema: "vendas",
                table: "venda_impostos",
                column: "venda_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_imposto_sync_id",
                schema: "vendas",
                table: "venda_impostos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_imposto_tenant_id",
                schema: "vendas",
                table: "venda_impostos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_venda_item_combustiveis_venda_item_id",
                schema: "vendas",
                table: "venda_item_combustiveis",
                column: "venda_item_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_item_combustivel_sync_id",
                schema: "vendas",
                table: "venda_item_combustiveis",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_item_combustivel_tenant_id",
                schema: "vendas",
                table: "venda_item_combustiveis",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_venda_item_combustivel_origens_venda_item_combustivel_id",
                schema: "vendas",
                table: "venda_item_combustivel_origens",
                column: "venda_item_combustivel_id");

            migrationBuilder.CreateIndex(
                name: "ix__venda_item_combustivel_origem_sync_id",
                schema: "vendas",
                table: "venda_item_combustivel_origens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_item_combustivel_origem_tenant_id",
                schema: "vendas",
                table: "venda_item_combustivel_origens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_venda_item_imposto_ibs_cbs_tributacoes_regulares_venda_item~",
                schema: "vendas",
                table: "venda_item_imposto_ibs_cbs_tributacoes_regulares",
                column: "venda_item_imposto_ibs_cbs_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_item_imposto_ibs_cbs_tributacao_regular_sync_id",
                schema: "vendas",
                table: "venda_item_imposto_ibs_cbs_tributacoes_regulares",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_item_imposto_ibs_cbs_tributacao_regular_tenant_id",
                schema: "vendas",
                table: "venda_item_imposto_ibs_cbs_tributacoes_regulares",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_venda_item_impostos_venda_item_id",
                schema: "vendas",
                table: "venda_item_impostos",
                column: "venda_item_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_item_imposto_sync_id",
                schema: "vendas",
                table: "venda_item_impostos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_item_imposto_tenant_id",
                schema: "vendas",
                table: "venda_item_impostos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_venda_item_impostos_ibs_cbs_venda_item_id",
                schema: "vendas",
                table: "venda_item_impostos_ibs_cbs",
                column: "venda_item_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_item_imposto_ibs_cbs_sync_id",
                schema: "vendas",
                table: "venda_item_impostos_ibs_cbs",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_item_imposto_ibs_cbs_tenant_id",
                schema: "vendas",
                table: "venda_item_impostos_ibs_cbs",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_venda_item_impostos_valor_aproximado_venda_item_id",
                schema: "vendas",
                table: "venda_item_impostos_valor_aproximado",
                column: "venda_item_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_item_imposto_valor_aproximado_sync_id",
                schema: "vendas",
                table: "venda_item_impostos_valor_aproximado",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_item_imposto_valor_aproximado_tenant_id",
                schema: "vendas",
                table: "venda_item_impostos_valor_aproximado",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_venda_nf_historicos_venda_id",
                schema: "vendas",
                table: "venda_nf_historicos",
                column: "venda_id");

            migrationBuilder.CreateIndex(
                name: "ix__venda_nf_historico_sync_id",
                schema: "vendas",
                table: "venda_nf_historicos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_nf_historico_tenant_id",
                schema: "vendas",
                table: "venda_nf_historicos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_venda_nf_historico_tenant_venda",
                schema: "vendas",
                table: "venda_nf_historicos",
                columns: new[] { "tenant_id", "venda_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_venda_nfces_venda_id",
                schema: "vendas",
                table: "venda_nfces",
                column: "venda_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_nfce_sync_id",
                schema: "vendas",
                table: "venda_nfces",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_nfce_tenant_id",
                schema: "vendas",
                table: "venda_nfces",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_venda_nfe_cartas_correcao_venda_nfe_id",
                schema: "vendas",
                table: "venda_nfe_cartas_correcao",
                column: "venda_nfe_id");

            migrationBuilder.CreateIndex(
                name: "ix__venda_nfe_carta_correcao_sync_id",
                schema: "vendas",
                table: "venda_nfe_cartas_correcao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_nfe_carta_correcao_tenant_id",
                schema: "vendas",
                table: "venda_nfe_cartas_correcao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_venda_nfe_exportacoes_venda_nfe_id",
                schema: "vendas",
                table: "venda_nfe_exportacoes",
                column: "venda_nfe_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_nfe_exportacao_sync_id",
                schema: "vendas",
                table: "venda_nfe_exportacoes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_nfe_exportacao_tenant_id",
                schema: "vendas",
                table: "venda_nfe_exportacoes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_venda_nfe_historicos_venda_id",
                schema: "vendas",
                table: "venda_nfe_historicos",
                column: "venda_id");

            migrationBuilder.CreateIndex(
                name: "ix__venda_nfe_historico_sync_id",
                schema: "vendas",
                table: "venda_nfe_historicos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_nfe_historico_tenant_id",
                schema: "vendas",
                table: "venda_nfe_historicos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_venda_nfe_historico_tenant_venda",
                schema: "vendas",
                table: "venda_nfe_historicos",
                columns: new[] { "tenant_id", "venda_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_venda_nfe_intermediadores_venda_nfe_id",
                schema: "vendas",
                table: "venda_nfe_intermediadores",
                column: "venda_nfe_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_nfe_intermediador_sync_id",
                schema: "vendas",
                table: "venda_nfe_intermediadores",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_nfe_intermediador_tenant_id",
                schema: "vendas",
                table: "venda_nfe_intermediadores",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_venda_nfe_referenciadas_venda_id",
                schema: "vendas",
                table: "venda_nfe_referenciadas",
                column: "venda_id");

            migrationBuilder.CreateIndex(
                name: "ix__venda_nfe_referenciada_sync_id",
                schema: "vendas",
                table: "venda_nfe_referenciadas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_nfe_referenciada_tenant_id",
                schema: "vendas",
                table: "venda_nfe_referenciadas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_venda_nfes_venda_id",
                schema: "vendas",
                table: "venda_nfes",
                column: "venda_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_nfe_sync_id",
                schema: "vendas",
                table: "venda_nfes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_nfe_tenant_id",
                schema: "vendas",
                table: "venda_nfes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_venda_pagamentos_venda_id",
                schema: "vendas",
                table: "venda_pagamentos",
                column: "venda_id");

            migrationBuilder.CreateIndex(
                name: "ix__venda_pagamento_sync_id",
                schema: "vendas",
                table: "venda_pagamentos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_pagamento_tenant_id",
                schema: "vendas",
                table: "venda_pagamentos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_venda_pagamentos_tenant_venda",
                schema: "vendas",
                table: "venda_pagamentos",
                columns: new[] { "tenant_id", "venda_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_venda_totais_venda_id",
                schema: "vendas",
                table: "venda_totais",
                column: "venda_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_total_sync_id",
                schema: "vendas",
                table: "venda_totais",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_total_tenant_id",
                schema: "vendas",
                table: "venda_totais",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_venda_totais_ibs_cbs_venda_id",
                schema: "vendas",
                table: "venda_totais_ibs_cbs",
                column: "venda_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_total_ibs_cbs_sync_id",
                schema: "vendas",
                table: "venda_totais_ibs_cbs",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_total_ibs_cbs_tenant_id",
                schema: "vendas",
                table: "venda_totais_ibs_cbs",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_venda_transporte_reboques_venda_transporte_id",
                schema: "vendas",
                table: "venda_transporte_reboques",
                column: "venda_transporte_id");

            migrationBuilder.CreateIndex(
                name: "ix__venda_transporte_reboque_sync_id",
                schema: "vendas",
                table: "venda_transporte_reboques",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_transporte_reboque_tenant_id",
                schema: "vendas",
                table: "venda_transporte_reboques",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_venda_transporte_transportadoras_venda_transporte_id",
                schema: "vendas",
                table: "venda_transporte_transportadoras",
                column: "venda_transporte_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_transporte_transportadora_sync_id",
                schema: "vendas",
                table: "venda_transporte_transportadoras",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_transporte_transportadora_tenant_id",
                schema: "vendas",
                table: "venda_transporte_transportadoras",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_venda_transporte_veiculos_venda_transporte_id",
                schema: "vendas",
                table: "venda_transporte_veiculos",
                column: "venda_transporte_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_transporte_veiculo_sync_id",
                schema: "vendas",
                table: "venda_transporte_veiculos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_transporte_veiculo_tenant_id",
                schema: "vendas",
                table: "venda_transporte_veiculos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_venda_transporte_volumes_venda_transporte_id",
                schema: "vendas",
                table: "venda_transporte_volumes",
                column: "venda_transporte_id");

            migrationBuilder.CreateIndex(
                name: "ix__venda_transporte_volume_sync_id",
                schema: "vendas",
                table: "venda_transporte_volumes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_transporte_volume_tenant_id",
                schema: "vendas",
                table: "venda_transporte_volumes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_venda_transportes_venda_id",
                schema: "vendas",
                table: "venda_transportes",
                column: "venda_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_transporte_sync_id",
                schema: "vendas",
                table: "venda_transportes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_transporte_tenant_id",
                schema: "vendas",
                table: "venda_transportes",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fatos_geradores_financeiros",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_autorizacoes_xml",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_cobranca_enderecos",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_configuracoes",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_destinatario_enderecos",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_emitente_enderecos",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_entregas",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_fatura_duplicatas",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_impostos",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_item_combustivel_origens",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_item_imposto_ibs_cbs_tributacoes_regulares",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_item_impostos",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_item_impostos_valor_aproximado",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_nf_historicos",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_nfces",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_nfe_cartas_correcao",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_nfe_exportacoes",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_nfe_historicos",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_nfe_intermediadores",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_nfe_referenciadas",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_pagamentos",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_totais",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_totais_ibs_cbs",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_transporte_reboques",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_transporte_transportadoras",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_transporte_veiculos",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_transporte_volumes",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_destinatarios",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_emitentes",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_faturas",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_item_combustiveis",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_item_impostos_ibs_cbs",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_nfes",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_transportes",
                schema: "vendas");

            migrationBuilder.DropIndex(
                name: "ix_venda_itens_tenant_venda",
                schema: "vendas",
                table: "venda_itens");

            migrationBuilder.DropColumn(
                name: "caminho_pdf_cupom_nao_fiscal",
                schema: "vendas",
                table: "vendas");

            migrationBuilder.DropColumn(
                name: "data_ultimo_processamento",
                schema: "vendas",
                table: "vendas");

            migrationBuilder.DropColumn(
                name: "documento_fiscal_id",
                schema: "vendas",
                table: "vendas");

            migrationBuilder.DropColumn(
                name: "numero_cupom_nao_fiscal",
                schema: "vendas",
                table: "vendas");

            migrationBuilder.DropColumn(
                name: "cfop_correlacao",
                schema: "vendas",
                table: "venda_itens");

            migrationBuilder.DropColumn(
                name: "codigo_beneficio_fiscal",
                schema: "vendas",
                table: "venda_itens");

            migrationBuilder.DropColumn(
                name: "codigo_ean_tributavel",
                schema: "vendas",
                table: "venda_itens");

            migrationBuilder.DropColumn(
                name: "compoe_valor_total",
                schema: "vendas",
                table: "venda_itens");

            migrationBuilder.DropColumn(
                name: "excecao_ncm_tipi",
                schema: "vendas",
                table: "venda_itens");

            migrationBuilder.DropColumn(
                name: "ficha_conteudo_importacao",
                schema: "vendas",
                table: "venda_itens");

            migrationBuilder.DropColumn(
                name: "informacoes_adicionais_do_produto",
                schema: "vendas",
                table: "venda_itens");

            migrationBuilder.DropColumn(
                name: "integra_faturamento",
                schema: "vendas",
                table: "venda_itens");

            migrationBuilder.DropColumn(
                name: "numero_item_pedido_compra",
                schema: "vendas",
                table: "venda_itens");

            migrationBuilder.DropColumn(
                name: "numero_pedido_compra",
                schema: "vendas",
                table: "venda_itens");

            migrationBuilder.DropColumn(
                name: "quantidade_tributavel",
                schema: "vendas",
                table: "venda_itens");

            migrationBuilder.DropColumn(
                name: "unidade_tributavel",
                schema: "vendas",
                table: "venda_itens");

            migrationBuilder.DropColumn(
                name: "valor_desconto_rateado",
                schema: "vendas",
                table: "venda_itens");

            migrationBuilder.DropColumn(
                name: "valor_outras_depesas_acessorias_rateado",
                schema: "vendas",
                table: "venda_itens");

            migrationBuilder.DropColumn(
                name: "valor_seguro_rateado",
                schema: "vendas",
                table: "venda_itens");

            migrationBuilder.DropColumn(
                name: "valor_unitario_tributavel",
                schema: "vendas",
                table: "venda_itens");

            migrationBuilder.AlterColumn<string>(
                name: "natureza_operacao",
                schema: "vendas",
                table: "vendas",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(60)",
                oldMaxLength: 60);
        }
    }
}
