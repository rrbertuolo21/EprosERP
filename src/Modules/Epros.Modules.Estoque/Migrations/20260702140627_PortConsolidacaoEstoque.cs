using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Estoque.Migrations
{
    /// <inheritdoc />
    public partial class PortConsolidacaoEstoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "i_x_compra_item_impostos_compra_item_id",
                schema: "estoque",
                table: "compra_item_impostos");

            migrationBuilder.AddColumn<long>(
                name: "sequencia_exibicao",
                schema: "estoque",
                table: "produtos",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "sequencia_exibicao",
                schema: "estoque",
                table: "produto_grupos",
                type: "bigint",
                nullable: true);

            // Postgres não converte text->integer automaticamente. Como o banco é zerado (sem dados
            // a preservar), fazemos o cast explícito via USING. A coluna status guarda o enum
            // EVendaStatus como int.
            migrationBuilder.Sql(
                "ALTER TABLE estoque.compras ALTER COLUMN status TYPE integer USING (status::integer);");

            migrationBuilder.AlterColumn<string>(
                name: "natureza_operacao",
                schema: "estoque",
                table: "compras",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<bool>(
                name: "cancelada",
                schema: "estoque",
                table: "compras",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "cfop_correlacao",
                schema: "estoque",
                table: "compra_itens",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "codigo_beneficio_fiscal",
                schema: "estoque",
                table: "compra_itens",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "codigo_ean_tributavel",
                schema: "estoque",
                table: "compra_itens",
                type: "character varying(14)",
                maxLength: 14,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "compoe_valor_total",
                schema: "estoque",
                table: "compra_itens",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "compra_id1",
                schema: "estoque",
                table: "compra_itens",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "excecao_ncm_tipi",
                schema: "estoque",
                table: "compra_itens",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ficha_conteudo_importacao",
                schema: "estoque",
                table: "compra_itens",
                type: "character varying(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "informacoes_adicionais_do_produto",
                schema: "estoque",
                table: "compra_itens",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "integra_faturamento",
                schema: "estoque",
                table: "compra_itens",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "numero_item_pedido_compra",
                schema: "estoque",
                table: "compra_itens",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "numero_pedido_compra",
                schema: "estoque",
                table: "compra_itens",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "quantidade_tributavel",
                schema: "estoque",
                table: "compra_itens",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "unidade_tributavel",
                schema: "estoque",
                table: "compra_itens",
                type: "character varying(6)",
                maxLength: 6,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "valor_desconto_rateado",
                schema: "estoque",
                table: "compra_itens",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_outras_despesas_acessorias_rateado",
                schema: "estoque",
                table: "compra_itens",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_seguro_rateado",
                schema: "estoque",
                table: "compra_itens",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_unitario_tributavel",
                schema: "estoque",
                table: "compra_itens",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "compra_autorizacoes_xml",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_compra_autorizacoes_xml", x => x.id);
                    table.ForeignKey(
                        name: "f_k_compra_autorizacoes_xml_compras_compra_id",
                        column: x => x.compra_id,
                        principalSchema: "estoque",
                        principalTable: "compras",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compra_cobranca_enderecos",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    fone = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    email = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    i_e = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    documento = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
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
                    table.PrimaryKey("p_k_compra_cobranca_enderecos", x => x.id);
                    table.ForeignKey(
                        name: "f_k_compra_cobranca_enderecos_compras_compra_id",
                        column: x => x.compra_id,
                        principalSchema: "estoque",
                        principalTable: "compras",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compra_configuracoes",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_compra_configuracoes", x => x.id);
                    table.ForeignKey(
                        name: "f_k_compra_configuracoes_compras_compra_id",
                        column: x => x.compra_id,
                        principalSchema: "estoque",
                        principalTable: "compras",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compra_destinatarios",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_compra_destinatarios", x => x.id);
                    table.ForeignKey(
                        name: "f_k_compra_destinatarios_compras_compra_id",
                        column: x => x.compra_id,
                        principalSchema: "estoque",
                        principalTable: "compras",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compra_emitentes",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_compra_emitentes", x => x.id);
                    table.ForeignKey(
                        name: "f_k_compra_emitentes_compras_compra_id",
                        column: x => x.compra_id,
                        principalSchema: "estoque",
                        principalTable: "compras",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compra_entregas",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    fone = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    email = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    i_e = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    documento = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
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
                    table.PrimaryKey("p_k_compra_entregas", x => x.id);
                    table.ForeignKey(
                        name: "f_k_compra_entregas_compras_compra_id",
                        column: x => x.compra_id,
                        principalSchema: "estoque",
                        principalTable: "compras",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compra_item_combustiveis",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_anp = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    descricao_anp = table.Column<string>(type: "character varying(95)", maxLength: 95, nullable: true),
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
                    table.PrimaryKey("p_k_compra_item_combustiveis", x => x.id);
                    table.ForeignKey(
                        name: "f_k_compra_item_combustiveis_compra_itens_compra_item_id",
                        column: x => x.compra_item_id,
                        principalSchema: "estoque",
                        principalTable: "compra_itens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compra_item_importacoes",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_declaracao_importacao = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    data_declaracao_importacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    local_desembaraco = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    uf_desembaraco = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    data_desembaraco = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    tipo_via_transporte = table.Column<int>(type: "integer", nullable: false),
                    valor_a_f_r_m_m = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tipo_intermedio = table.Column<int>(type: "integer", nullable: false),
                    cnpj = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    cpf = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    uf_terceiro = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    codigo_exportador = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
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
                    table.PrimaryKey("p_k_compra_item_importacoes", x => x.id);
                    table.ForeignKey(
                        name: "f_k_compra_item_importacoes_compra_itens_compra_item_id",
                        column: x => x.compra_item_id,
                        principalSchema: "estoque",
                        principalTable: "compra_itens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compra_nfe_historicos",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_compra_nfe_historicos", x => x.id);
                    table.ForeignKey(
                        name: "f_k_compra_nfe_historicos_compras_compra_id",
                        column: x => x.compra_id,
                        principalSchema: "estoque",
                        principalTable: "compras",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compra_nfe_referenciadas",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_compra_nfe_referenciadas", x => x.id);
                    table.ForeignKey(
                        name: "f_k_compra_nfe_referenciadas_compras_compra_id",
                        column: x => x.compra_id,
                        principalSchema: "estoque",
                        principalTable: "compras",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compra_nfes",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<long>(type: "bigint", nullable: false),
                    serie = table.Column<int>(type: "integer", nullable: false),
                    data_hora_emissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_hora_saida = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status_interno = table.Column<int>(type: "integer", nullable: false),
                    status_sefaz = table.Column<int>(type: "integer", nullable: false),
                    chave = table.Column<string>(type: "character varying(44)", maxLength: 44, nullable: true),
                    protocolo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    xml = table.Column<string>(type: "text", nullable: true),
                    ultimo_retorno_mensagem_sefaz = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    data_hora_cancelamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    protocolo_cancelamento = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
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
                    table.PrimaryKey("p_k_compra_nfes", x => x.id);
                    table.ForeignKey(
                        name: "f_k_compra_nfes_compras_compra_id",
                        column: x => x.compra_id,
                        principalSchema: "estoque",
                        principalTable: "compras",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compra_pagamentos",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_compra_pagamentos", x => x.id);
                    table.ForeignKey(
                        name: "f_k_compra_pagamentos_compras_compra_id",
                        column: x => x.compra_id,
                        principalSchema: "estoque",
                        principalTable: "compras",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compra_transportes",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_compra_transportes", x => x.id);
                    table.ForeignKey(
                        name: "f_k_compra_transportes_compras_compra_id",
                        column: x => x.compra_id,
                        principalSchema: "estoque",
                        principalTable: "compras",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "produto_grupo_empresas",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_grupo_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_produto_grupo_empresas", x => x.id);
                    table.ForeignKey(
                        name: "f_k_produto_grupo_empresas_produto_grupos_produto_grupo_id",
                        column: x => x.produto_grupo_id,
                        principalSchema: "estoque",
                        principalTable: "produto_grupos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compra_destinatario_enderecos",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_destinatario_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_compra_destinatario_enderecos", x => x.id);
                    table.ForeignKey(
                        name: "f_k_compra_destinatario_enderecos_compra_destinatarios_compra_d~",
                        column: x => x.compra_destinatario_id,
                        principalSchema: "estoque",
                        principalTable: "compra_destinatarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compra_emitente_enderecos",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_emitente_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_compra_emitente_enderecos", x => x.id);
                    table.ForeignKey(
                        name: "f_k_compra_emitente_enderecos_compra_emitentes_compra_emitente_~",
                        column: x => x.compra_emitente_id,
                        principalSchema: "estoque",
                        principalTable: "compra_emitentes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compra_item_combustivel_origens",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_item_combustivel_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_compra_item_combustivel_origens", x => x.id);
                    table.ForeignKey(
                        name: "f_k_compra_item_combustivel_origens_compra_item_combustiveis_co~",
                        column: x => x.compra_item_combustivel_id,
                        principalSchema: "estoque",
                        principalTable: "compra_item_combustiveis",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compra_item_importacao_adicoes",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_item_importacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_adicao = table.Column<int>(type: "integer", nullable: false),
                    numero_sequencial_adicao = table.Column<int>(type: "integer", nullable: false),
                    codigo_fabricante = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    valor_desconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    numero_ato_concessorio = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("p_k_compra_item_importacao_adicoes", x => x.id);
                    table.ForeignKey(
                        name: "f_k_compra_item_importacao_adicoes_compra_item_importacoes_comp~",
                        column: x => x.compra_item_importacao_id,
                        principalSchema: "estoque",
                        principalTable: "compra_item_importacoes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compra_nfe_cartas_correcao",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_nfe_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_compra_nfe_cartas_correcao", x => x.id);
                    table.ForeignKey(
                        name: "f_k_compra_nfe_cartas_correcao_compra_nfes_compra_nfe_id",
                        column: x => x.compra_nfe_id,
                        principalSchema: "estoque",
                        principalTable: "compra_nfes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compra_nfe_intermediadores",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_nfe_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_compra_nfe_intermediadores", x => x.id);
                    table.ForeignKey(
                        name: "f_k_compra_nfe_intermediadores_compra_nfes_compra_nfe_id",
                        column: x => x.compra_nfe_id,
                        principalSchema: "estoque",
                        principalTable: "compra_nfes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compra_transporte_reboques",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_transporte_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_compra_transporte_reboques", x => x.id);
                    table.ForeignKey(
                        name: "f_k_compra_transporte_reboques_compra_transportes_compra_transp~",
                        column: x => x.compra_transporte_id,
                        principalSchema: "estoque",
                        principalTable: "compra_transportes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compra_transporte_transportadoras",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_transporte_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cnpj = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    cpf = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    razao_social = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    inscricao_estadual = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    endereco_completo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    nome_municipio = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
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
                    table.PrimaryKey("p_k_compra_transporte_transportadoras", x => x.id);
                    table.ForeignKey(
                        name: "f_k_compra_transporte_transportadoras_compra_transportes_compra~",
                        column: x => x.compra_transporte_id,
                        principalSchema: "estoque",
                        principalTable: "compra_transportes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compra_transporte_veiculos",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_transporte_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_compra_transporte_veiculos", x => x.id);
                    table.ForeignKey(
                        name: "f_k_compra_transporte_veiculos_compra_transportes_compra_transp~",
                        column: x => x.compra_transporte_id,
                        principalSchema: "estoque",
                        principalTable: "compra_transportes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compra_transporte_volumes",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_transporte_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_compra_transporte_volumes", x => x.id);
                    table.ForeignKey(
                        name: "f_k_compra_transporte_volumes_compra_transportes_compra_transpo~",
                        column: x => x.compra_transporte_id,
                        principalSchema: "estoque",
                        principalTable: "compra_transportes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_compra_itens_compra_id1",
                schema: "estoque",
                table: "compra_itens",
                column: "compra_id1");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_item_impostos_valor_aproximado_compra_item_id",
                schema: "estoque",
                table: "compra_item_impostos_valor_aproximado",
                column: "compra_item_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_compra_item_impostos_ibs_cbs_compra_item_id",
                schema: "estoque",
                table: "compra_item_impostos_ibs_cbs",
                column: "compra_item_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_compra_item_impostos_compra_item_id",
                schema: "estoque",
                table: "compra_item_impostos",
                column: "compra_item_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_compra_autorizacoes_xml_compra_id",
                schema: "estoque",
                table: "compra_autorizacoes_xml",
                column: "compra_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_autorizacoes_xml_tenant_id_compra_id",
                schema: "estoque",
                table: "compra_autorizacoes_xml",
                columns: new[] { "tenant_id", "compra_id" });

            migrationBuilder.CreateIndex(
                name: "ix__compra_autorizacao_xml_sync_id",
                schema: "estoque",
                table: "compra_autorizacoes_xml",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_autorizacao_xml_tenant_id",
                schema: "estoque",
                table: "compra_autorizacoes_xml",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_cobranca_enderecos_compra_id",
                schema: "estoque",
                table: "compra_cobranca_enderecos",
                column: "compra_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_compra_cobranca_enderecos_tenant_id_compra_id",
                schema: "estoque",
                table: "compra_cobranca_enderecos",
                columns: new[] { "tenant_id", "compra_id" });

            migrationBuilder.CreateIndex(
                name: "ix__compra_cobranca_endereco_sync_id",
                schema: "estoque",
                table: "compra_cobranca_enderecos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_cobranca_endereco_tenant_id",
                schema: "estoque",
                table: "compra_cobranca_enderecos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_configuracoes_compra_id",
                schema: "estoque",
                table: "compra_configuracoes",
                column: "compra_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_compra_configuracoes_tenant_id_compra_id",
                schema: "estoque",
                table: "compra_configuracoes",
                columns: new[] { "tenant_id", "compra_id" });

            migrationBuilder.CreateIndex(
                name: "ix__compra_configuracao_sync_id",
                schema: "estoque",
                table: "compra_configuracoes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_configuracao_tenant_id",
                schema: "estoque",
                table: "compra_configuracoes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_destinatario_enderecos_compra_destinatario_id",
                schema: "estoque",
                table: "compra_destinatario_enderecos",
                column: "compra_destinatario_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_destinatario_enderecos_tenant_id_compra_destinatario~",
                schema: "estoque",
                table: "compra_destinatario_enderecos",
                columns: new[] { "tenant_id", "compra_destinatario_id" });

            migrationBuilder.CreateIndex(
                name: "ix__compra_destinatario_endereco_sync_id",
                schema: "estoque",
                table: "compra_destinatario_enderecos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_destinatario_endereco_tenant_id",
                schema: "estoque",
                table: "compra_destinatario_enderecos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_destinatarios_compra_id",
                schema: "estoque",
                table: "compra_destinatarios",
                column: "compra_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_compra_destinatarios_tenant_id_compra_id",
                schema: "estoque",
                table: "compra_destinatarios",
                columns: new[] { "tenant_id", "compra_id" });

            migrationBuilder.CreateIndex(
                name: "ix__compra_destinatario_sync_id",
                schema: "estoque",
                table: "compra_destinatarios",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_destinatario_tenant_id",
                schema: "estoque",
                table: "compra_destinatarios",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_emitente_enderecos_compra_emitente_id",
                schema: "estoque",
                table: "compra_emitente_enderecos",
                column: "compra_emitente_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_compra_emitente_enderecos_tenant_id_compra_emitente_id",
                schema: "estoque",
                table: "compra_emitente_enderecos",
                columns: new[] { "tenant_id", "compra_emitente_id" });

            migrationBuilder.CreateIndex(
                name: "ix__compra_emitente_endereco_sync_id",
                schema: "estoque",
                table: "compra_emitente_enderecos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_emitente_endereco_tenant_id",
                schema: "estoque",
                table: "compra_emitente_enderecos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_emitentes_compra_id",
                schema: "estoque",
                table: "compra_emitentes",
                column: "compra_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_compra_emitentes_tenant_id_compra_id",
                schema: "estoque",
                table: "compra_emitentes",
                columns: new[] { "tenant_id", "compra_id" });

            migrationBuilder.CreateIndex(
                name: "ix__compra_emitente_sync_id",
                schema: "estoque",
                table: "compra_emitentes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_emitente_tenant_id",
                schema: "estoque",
                table: "compra_emitentes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_entregas_compra_id",
                schema: "estoque",
                table: "compra_entregas",
                column: "compra_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_compra_entregas_tenant_id_compra_id",
                schema: "estoque",
                table: "compra_entregas",
                columns: new[] { "tenant_id", "compra_id" });

            migrationBuilder.CreateIndex(
                name: "ix__compra_entrega_sync_id",
                schema: "estoque",
                table: "compra_entregas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_entrega_tenant_id",
                schema: "estoque",
                table: "compra_entregas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_item_combustiveis_compra_item_id",
                schema: "estoque",
                table: "compra_item_combustiveis",
                column: "compra_item_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_compra_item_combustiveis_tenant_id_compra_item_id",
                schema: "estoque",
                table: "compra_item_combustiveis",
                columns: new[] { "tenant_id", "compra_item_id" });

            migrationBuilder.CreateIndex(
                name: "ix__compra_item_combustivel_sync_id",
                schema: "estoque",
                table: "compra_item_combustiveis",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_item_combustivel_tenant_id",
                schema: "estoque",
                table: "compra_item_combustiveis",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_item_combustivel_origens_compra_item_combustivel_id",
                schema: "estoque",
                table: "compra_item_combustivel_origens",
                column: "compra_item_combustivel_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_item_combustivel_origens_tenant_id_compra_item_combu~",
                schema: "estoque",
                table: "compra_item_combustivel_origens",
                columns: new[] { "tenant_id", "compra_item_combustivel_id" });

            migrationBuilder.CreateIndex(
                name: "ix__compra_item_combustivel_origem_sync_id",
                schema: "estoque",
                table: "compra_item_combustivel_origens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_item_combustivel_origem_tenant_id",
                schema: "estoque",
                table: "compra_item_combustivel_origens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_item_importacao_adicoes_compra_item_importacao_id",
                schema: "estoque",
                table: "compra_item_importacao_adicoes",
                column: "compra_item_importacao_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_item_importacao_adicoes_tenant_id_compra_item_import~",
                schema: "estoque",
                table: "compra_item_importacao_adicoes",
                columns: new[] { "tenant_id", "compra_item_importacao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__compra_item_importacao_adicao_sync_id",
                schema: "estoque",
                table: "compra_item_importacao_adicoes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_item_importacao_adicao_tenant_id",
                schema: "estoque",
                table: "compra_item_importacao_adicoes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_item_importacoes_compra_item_id",
                schema: "estoque",
                table: "compra_item_importacoes",
                column: "compra_item_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_item_importacoes_tenant_id_compra_item_id",
                schema: "estoque",
                table: "compra_item_importacoes",
                columns: new[] { "tenant_id", "compra_item_id" });

            migrationBuilder.CreateIndex(
                name: "ix__compra_item_importacao_sync_id",
                schema: "estoque",
                table: "compra_item_importacoes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_item_importacao_tenant_id",
                schema: "estoque",
                table: "compra_item_importacoes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_nfe_cartas_correcao_compra_nfe_id",
                schema: "estoque",
                table: "compra_nfe_cartas_correcao",
                column: "compra_nfe_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_nfe_cartas_correcao_tenant_id_compra_nfe_id",
                schema: "estoque",
                table: "compra_nfe_cartas_correcao",
                columns: new[] { "tenant_id", "compra_nfe_id" });

            migrationBuilder.CreateIndex(
                name: "ix__compra_nfe_carta_correcao_sync_id",
                schema: "estoque",
                table: "compra_nfe_cartas_correcao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_nfe_carta_correcao_tenant_id",
                schema: "estoque",
                table: "compra_nfe_cartas_correcao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_nfe_historicos_compra_id",
                schema: "estoque",
                table: "compra_nfe_historicos",
                column: "compra_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_nfe_historicos_tenant_id_compra_id",
                schema: "estoque",
                table: "compra_nfe_historicos",
                columns: new[] { "tenant_id", "compra_id" });

            migrationBuilder.CreateIndex(
                name: "ix__compra_nfe_historico_sync_id",
                schema: "estoque",
                table: "compra_nfe_historicos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_nfe_historico_tenant_id",
                schema: "estoque",
                table: "compra_nfe_historicos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_nfe_intermediadores_compra_nfe_id",
                schema: "estoque",
                table: "compra_nfe_intermediadores",
                column: "compra_nfe_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_compra_nfe_intermediadores_tenant_id_compra_nfe_id",
                schema: "estoque",
                table: "compra_nfe_intermediadores",
                columns: new[] { "tenant_id", "compra_nfe_id" });

            migrationBuilder.CreateIndex(
                name: "ix__compra_nfe_intermediador_sync_id",
                schema: "estoque",
                table: "compra_nfe_intermediadores",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_nfe_intermediador_tenant_id",
                schema: "estoque",
                table: "compra_nfe_intermediadores",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_nfe_referenciadas_compra_id",
                schema: "estoque",
                table: "compra_nfe_referenciadas",
                column: "compra_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_nfe_referenciadas_tenant_id_compra_id",
                schema: "estoque",
                table: "compra_nfe_referenciadas",
                columns: new[] { "tenant_id", "compra_id" });

            migrationBuilder.CreateIndex(
                name: "ix__compra_nfe_referenciada_sync_id",
                schema: "estoque",
                table: "compra_nfe_referenciadas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_nfe_referenciada_tenant_id",
                schema: "estoque",
                table: "compra_nfe_referenciadas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_nfes_compra_id",
                schema: "estoque",
                table: "compra_nfes",
                column: "compra_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_compra_nfes_tenant_id_compra_id",
                schema: "estoque",
                table: "compra_nfes",
                columns: new[] { "tenant_id", "compra_id" });

            migrationBuilder.CreateIndex(
                name: "ix__compra_nfe_sync_id",
                schema: "estoque",
                table: "compra_nfes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_nfe_tenant_id",
                schema: "estoque",
                table: "compra_nfes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_pagamentos_compra_id",
                schema: "estoque",
                table: "compra_pagamentos",
                column: "compra_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_pagamentos_tenant_id_compra_id",
                schema: "estoque",
                table: "compra_pagamentos",
                columns: new[] { "tenant_id", "compra_id" });

            migrationBuilder.CreateIndex(
                name: "ix__compra_pagamento_sync_id",
                schema: "estoque",
                table: "compra_pagamentos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_pagamento_tenant_id",
                schema: "estoque",
                table: "compra_pagamentos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_transporte_reboques_compra_transporte_id",
                schema: "estoque",
                table: "compra_transporte_reboques",
                column: "compra_transporte_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_transporte_reboques_tenant_id_compra_transporte_id",
                schema: "estoque",
                table: "compra_transporte_reboques",
                columns: new[] { "tenant_id", "compra_transporte_id" });

            migrationBuilder.CreateIndex(
                name: "ix__compra_transporte_reboque_sync_id",
                schema: "estoque",
                table: "compra_transporte_reboques",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_transporte_reboque_tenant_id",
                schema: "estoque",
                table: "compra_transporte_reboques",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_transporte_transportadoras_compra_transporte_id",
                schema: "estoque",
                table: "compra_transporte_transportadoras",
                column: "compra_transporte_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_compra_transporte_transportadoras_tenant_id_compra_transpor~",
                schema: "estoque",
                table: "compra_transporte_transportadoras",
                columns: new[] { "tenant_id", "compra_transporte_id" });

            migrationBuilder.CreateIndex(
                name: "ix__compra_transporte_transportadora_sync_id",
                schema: "estoque",
                table: "compra_transporte_transportadoras",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_transporte_transportadora_tenant_id",
                schema: "estoque",
                table: "compra_transporte_transportadoras",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_transporte_veiculos_compra_transporte_id",
                schema: "estoque",
                table: "compra_transporte_veiculos",
                column: "compra_transporte_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_compra_transporte_veiculos_tenant_id_compra_transporte_id",
                schema: "estoque",
                table: "compra_transporte_veiculos",
                columns: new[] { "tenant_id", "compra_transporte_id" });

            migrationBuilder.CreateIndex(
                name: "ix__compra_transporte_veiculo_sync_id",
                schema: "estoque",
                table: "compra_transporte_veiculos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_transporte_veiculo_tenant_id",
                schema: "estoque",
                table: "compra_transporte_veiculos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_transporte_volumes_compra_transporte_id",
                schema: "estoque",
                table: "compra_transporte_volumes",
                column: "compra_transporte_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_transporte_volumes_tenant_id_compra_transporte_id",
                schema: "estoque",
                table: "compra_transporte_volumes",
                columns: new[] { "tenant_id", "compra_transporte_id" });

            migrationBuilder.CreateIndex(
                name: "ix__compra_transporte_volume_sync_id",
                schema: "estoque",
                table: "compra_transporte_volumes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_transporte_volume_tenant_id",
                schema: "estoque",
                table: "compra_transporte_volumes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_transportes_compra_id",
                schema: "estoque",
                table: "compra_transportes",
                column: "compra_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_compra_transportes_tenant_id_compra_id",
                schema: "estoque",
                table: "compra_transportes",
                columns: new[] { "tenant_id", "compra_id" });

            migrationBuilder.CreateIndex(
                name: "ix__compra_transporte_sync_id",
                schema: "estoque",
                table: "compra_transportes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_transporte_tenant_id",
                schema: "estoque",
                table: "compra_transportes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_produto_grupo_empresas_produto_grupo_id",
                schema: "estoque",
                table: "produto_grupo_empresas",
                column: "produto_grupo_id");

            migrationBuilder.CreateIndex(
                name: "i_x_produto_grupo_empresas_tenant_id_empresa_id",
                schema: "estoque",
                table: "produto_grupo_empresas",
                columns: new[] { "tenant_id", "empresa_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_produto_grupo_empresas_tenant_id_produto_grupo_id_empresa_id",
                schema: "estoque",
                table: "produto_grupo_empresas",
                columns: new[] { "tenant_id", "produto_grupo_id", "empresa_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__produto_grupo_empresa_sync_id",
                schema: "estoque",
                table: "produto_grupo_empresas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__produto_grupo_empresa_tenant_id",
                schema: "estoque",
                table: "produto_grupo_empresas",
                column: "tenant_id");

            migrationBuilder.AddForeignKey(
                name: "f_k_compra_item_impostos_ibs_cbs_compra_itens_compra_item_id",
                schema: "estoque",
                table: "compra_item_impostos_ibs_cbs",
                column: "compra_item_id",
                principalSchema: "estoque",
                principalTable: "compra_itens",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "f_k_compra_item_impostos_valor_aproximado_compra_itens_compra_i~",
                schema: "estoque",
                table: "compra_item_impostos_valor_aproximado",
                column: "compra_item_id",
                principalSchema: "estoque",
                principalTable: "compra_itens",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "f_k_compra_itens_compras_compra_id1",
                schema: "estoque",
                table: "compra_itens",
                column: "compra_id1",
                principalSchema: "estoque",
                principalTable: "compras",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_compra_item_impostos_ibs_cbs_compra_itens_compra_item_id",
                schema: "estoque",
                table: "compra_item_impostos_ibs_cbs");

            migrationBuilder.DropForeignKey(
                name: "f_k_compra_item_impostos_valor_aproximado_compra_itens_compra_i~",
                schema: "estoque",
                table: "compra_item_impostos_valor_aproximado");

            migrationBuilder.DropForeignKey(
                name: "f_k_compra_itens_compras_compra_id1",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropTable(
                name: "compra_autorizacoes_xml",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "compra_cobranca_enderecos",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "compra_configuracoes",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "compra_destinatario_enderecos",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "compra_emitente_enderecos",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "compra_entregas",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "compra_item_combustivel_origens",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "compra_item_importacao_adicoes",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "compra_nfe_cartas_correcao",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "compra_nfe_historicos",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "compra_nfe_intermediadores",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "compra_nfe_referenciadas",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "compra_pagamentos",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "compra_transporte_reboques",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "compra_transporte_transportadoras",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "compra_transporte_veiculos",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "compra_transporte_volumes",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "produto_grupo_empresas",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "compra_destinatarios",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "compra_emitentes",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "compra_item_combustiveis",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "compra_item_importacoes",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "compra_nfes",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "compra_transportes",
                schema: "estoque");

            migrationBuilder.DropIndex(
                name: "i_x_compra_itens_compra_id1",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropIndex(
                name: "i_x_compra_item_impostos_valor_aproximado_compra_item_id",
                schema: "estoque",
                table: "compra_item_impostos_valor_aproximado");

            migrationBuilder.DropIndex(
                name: "i_x_compra_item_impostos_ibs_cbs_compra_item_id",
                schema: "estoque",
                table: "compra_item_impostos_ibs_cbs");

            migrationBuilder.DropIndex(
                name: "i_x_compra_item_impostos_compra_item_id",
                schema: "estoque",
                table: "compra_item_impostos");

            migrationBuilder.DropColumn(
                name: "sequencia_exibicao",
                schema: "estoque",
                table: "produtos");

            migrationBuilder.DropColumn(
                name: "sequencia_exibicao",
                schema: "estoque",
                table: "produto_grupos");

            migrationBuilder.DropColumn(
                name: "cancelada",
                schema: "estoque",
                table: "compras");

            migrationBuilder.DropColumn(
                name: "cfop_correlacao",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropColumn(
                name: "codigo_beneficio_fiscal",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropColumn(
                name: "codigo_ean_tributavel",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropColumn(
                name: "compoe_valor_total",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropColumn(
                name: "compra_id1",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropColumn(
                name: "excecao_ncm_tipi",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropColumn(
                name: "ficha_conteudo_importacao",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropColumn(
                name: "informacoes_adicionais_do_produto",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropColumn(
                name: "integra_faturamento",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropColumn(
                name: "numero_item_pedido_compra",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropColumn(
                name: "numero_pedido_compra",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropColumn(
                name: "quantidade_tributavel",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropColumn(
                name: "unidade_tributavel",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropColumn(
                name: "valor_desconto_rateado",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropColumn(
                name: "valor_outras_despesas_acessorias_rateado",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropColumn(
                name: "valor_seguro_rateado",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropColumn(
                name: "valor_unitario_tributavel",
                schema: "estoque",
                table: "compra_itens");

            // Reverte int->text com cast explícito (Postgres não faz cast automático nesse sentido).
            migrationBuilder.Sql(
                "ALTER TABLE estoque.compras ALTER COLUMN status TYPE character varying(20) USING (status::text);");

            migrationBuilder.AlterColumn<string>(
                name: "natureza_operacao",
                schema: "estoque",
                table: "compras",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(60)",
                oldMaxLength: 60);

            migrationBuilder.CreateIndex(
                name: "i_x_compra_item_impostos_compra_item_id",
                schema: "estoque",
                table: "compra_item_impostos",
                column: "compra_item_id");
        }
    }
}
