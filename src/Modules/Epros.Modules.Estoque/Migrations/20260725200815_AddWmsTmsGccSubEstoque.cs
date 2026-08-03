using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Estoque.Migrations
{
    /// <inheritdoc />
    public partial class AddWmsTmsGccSubEstoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "situacao",
                schema: "estoque",
                table: "compra_transportes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "gcc_contratos_compra",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fornecedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_contrato = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    vigencia_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    vigencia_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    situacao = table.Column<int>(type: "integer", nullable: false),
                    observacao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("p_k_gcc_contratos_compra", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sub_documentos_fiscais",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    envio_id = table.Column<Guid>(type: "uuid", nullable: true),
                    retorno_id = table.Column<Guid>(type: "uuid", nullable: true),
                    documento_fiscal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cfop_remessa = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    cfop_retorno = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
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
                    table.PrimaryKey("p_k_sub_documentos_fiscais", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sub_envios",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_envio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    documento_fiscal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_sub_envios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sub_historicos",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    evento = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    dados_anteriores = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    dados_novos = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_evento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_sub_historicos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sub_ordens",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    numero_ordem = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    ordem_producao_id = table.Column<Guid>(type: "uuid", nullable: true),
                    fornecedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_emissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_prevista_retorno = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    observacao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("p_k_sub_ordens", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sub_retornos",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_retorno = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    documento_fiscal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_sub_retornos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sub_saldos_terceiro",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fornecedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quantidade_enviada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantidade_retornada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantidade_em_poder_terceiro = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantidade_perda = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ultimo_movimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_sub_saldos_terceiro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sub_servicos_cobranca",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_id = table.Column<Guid>(type: "uuid", nullable: true),
                    valor_servico = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_sub_servicos_cobranca", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "wms_armazens",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    endereco = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cidade = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    cep = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    usuario_dono_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_wms_armazens", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "gcc_consumos_contrato",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contrato_compra_id = table.Column<Guid>(type: "uuid", nullable: false),
                    contrato_compra_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quantidade_consumida = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_consumido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    data_consumo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_gcc_consumos_contrato", x => x.id);
                    table.ForeignKey(
                        name: "f_k_gcc_consumos_contrato__gcc_contratos_compra_contrato_compra_id",
                        column: x => x.contrato_compra_id,
                        principalSchema: "estoque",
                        principalTable: "gcc_contratos_compra",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gcc_contratos_compra_aditivos",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contrato_compra_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_aditivo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    tipo_aditivo = table.Column<int>(type: "integer", nullable: false),
                    justificativa = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    data_aditivo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    aprovado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    aprovado_por = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_gcc_contratos_compra_aditivos", x => x.id);
                    table.ForeignKey(
                        name: "f_k_gcc_contratos_compra_aditivos_gcc_contratos_compra_contrato~",
                        column: x => x.contrato_compra_id,
                        principalSchema: "estoque",
                        principalTable: "gcc_contratos_compra",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gcc_contratos_compra_itens",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contrato_compra_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    preco_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantidade_comprometida = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantidade_consumida = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_consumido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    saldo_quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    saldo_valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_gcc_contratos_compra_itens", x => x.id);
                    table.ForeignKey(
                        name: "f_k_gcc_contratos_compra_itens_gcc_contratos_compra_contrato_co~",
                        column: x => x.contrato_compra_id,
                        principalSchema: "estoque",
                        principalTable: "gcc_contratos_compra",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sub_envio_itens",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    envio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade_enviada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    lote_id = table.Column<Guid>(type: "uuid", nullable: true),
                    local_origem_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_sub_envio_itens", x => x.id);
                    table.ForeignKey(
                        name: "f_k_sub_envio_itens_sub_envios_envio_id",
                        column: x => x.envio_id,
                        principalSchema: "estoque",
                        principalTable: "sub_envios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sub_ordem_itens",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade_planejada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    unidade = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: true),
                    operacao_terceirizada = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
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
                    table.PrimaryKey("p_k_sub_ordem_itens", x => x.id);
                    table.ForeignKey(
                        name: "f_k_sub_ordem_itens_sub_ordens_ordem_id",
                        column: x => x.ordem_id,
                        principalSchema: "estoque",
                        principalTable: "sub_ordens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sub_retorno_itens",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    retorno_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade_retorno = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantidade_aprovada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    quantidade_perda = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    quantidade_sucata = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    rendimento = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_sub_retorno_itens", x => x.id);
                    table.ForeignKey(
                        name: "f_k_sub_retorno_itens_sub_retornos_retorno_id",
                        column: x => x.retorno_id,
                        principalSchema: "estoque",
                        principalTable: "sub_retornos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wms_enderecos_operacionais",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    armazem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rua = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    estante = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    caixa = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
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
                    table.PrimaryKey("p_k_wms_enderecos_operacionais", x => x.id);
                    table.ForeignKey(
                        name: "f_k_wms_enderecos_operacionais_wms_armazens_armazem_id",
                        column: x => x.armazem_id,
                        principalSchema: "estoque",
                        principalTable: "wms_armazens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_gcc_consumos_contrato_contrato_compra_id",
                schema: "estoque",
                table: "gcc_consumos_contrato",
                column: "contrato_compra_id");

            migrationBuilder.CreateIndex(
                name: "i_x_gcc_consumos_contrato_tenant_id_contrato_compra_id",
                schema: "estoque",
                table: "gcc_consumos_contrato",
                columns: new[] { "tenant_id", "contrato_compra_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_gcc_consumos_contrato_tenant_id_contrato_compra_item_id",
                schema: "estoque",
                table: "gcc_consumos_contrato",
                columns: new[] { "tenant_id", "contrato_compra_item_id" });

            migrationBuilder.CreateIndex(
                name: "ix__gcc_consumo_contrato_sync_id",
                schema: "estoque",
                table: "gcc_consumos_contrato",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__gcc_consumo_contrato_tenant_id",
                schema: "estoque",
                table: "gcc_consumos_contrato",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_gcc_contratos_compra_tenant_id_fornecedor_id",
                schema: "estoque",
                table: "gcc_contratos_compra",
                columns: new[] { "tenant_id", "fornecedor_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_gcc_contratos_compra_tenant_id_situacao",
                schema: "estoque",
                table: "gcc_contratos_compra",
                columns: new[] { "tenant_id", "situacao" });

            migrationBuilder.CreateIndex(
                name: "ix__gcc_contrato_compra_sync_id",
                schema: "estoque",
                table: "gcc_contratos_compra",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__gcc_contrato_compra_tenant_id",
                schema: "estoque",
                table: "gcc_contratos_compra",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_gcc_contratos_compra_aditivos_contrato_compra_id",
                schema: "estoque",
                table: "gcc_contratos_compra_aditivos",
                column: "contrato_compra_id");

            migrationBuilder.CreateIndex(
                name: "i_x_gcc_contratos_compra_aditivos_tenant_id_contrato_compra_id",
                schema: "estoque",
                table: "gcc_contratos_compra_aditivos",
                columns: new[] { "tenant_id", "contrato_compra_id" });

            migrationBuilder.CreateIndex(
                name: "ix__gcc_contrato_compra_aditivo_sync_id",
                schema: "estoque",
                table: "gcc_contratos_compra_aditivos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__gcc_contrato_compra_aditivo_tenant_id",
                schema: "estoque",
                table: "gcc_contratos_compra_aditivos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_gcc_contratos_compra_itens_contrato_compra_id",
                schema: "estoque",
                table: "gcc_contratos_compra_itens",
                column: "contrato_compra_id");

            migrationBuilder.CreateIndex(
                name: "i_x_gcc_contratos_compra_itens_tenant_id_contrato_compra_id",
                schema: "estoque",
                table: "gcc_contratos_compra_itens",
                columns: new[] { "tenant_id", "contrato_compra_id" });

            migrationBuilder.CreateIndex(
                name: "ix__gcc_contrato_compra_item_sync_id",
                schema: "estoque",
                table: "gcc_contratos_compra_itens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__gcc_contrato_compra_item_tenant_id",
                schema: "estoque",
                table: "gcc_contratos_compra_itens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_sub_documentos_fiscais_tenant_id_ordem_id",
                schema: "estoque",
                table: "sub_documentos_fiscais",
                columns: new[] { "tenant_id", "ordem_id" });

            migrationBuilder.CreateIndex(
                name: "ix__sub_documento_fiscal_sync_id",
                schema: "estoque",
                table: "sub_documentos_fiscais",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sub_documento_fiscal_tenant_id",
                schema: "estoque",
                table: "sub_documentos_fiscais",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_sub_envio_itens_envio_id",
                schema: "estoque",
                table: "sub_envio_itens",
                column: "envio_id");

            migrationBuilder.CreateIndex(
                name: "i_x_sub_envio_itens_tenant_id_envio_id",
                schema: "estoque",
                table: "sub_envio_itens",
                columns: new[] { "tenant_id", "envio_id" });

            migrationBuilder.CreateIndex(
                name: "ix__sub_envio_item_sync_id",
                schema: "estoque",
                table: "sub_envio_itens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sub_envio_item_tenant_id",
                schema: "estoque",
                table: "sub_envio_itens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_sub_envios_tenant_id_ordem_id",
                schema: "estoque",
                table: "sub_envios",
                columns: new[] { "tenant_id", "ordem_id" });

            migrationBuilder.CreateIndex(
                name: "ix__sub_envio_sync_id",
                schema: "estoque",
                table: "sub_envios",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sub_envio_tenant_id",
                schema: "estoque",
                table: "sub_envios",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_sub_historicos_tenant_id_ordem_id",
                schema: "estoque",
                table: "sub_historicos",
                columns: new[] { "tenant_id", "ordem_id" });

            migrationBuilder.CreateIndex(
                name: "ix__sub_historico_sync_id",
                schema: "estoque",
                table: "sub_historicos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sub_historico_tenant_id",
                schema: "estoque",
                table: "sub_historicos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_sub_ordem_itens_ordem_id",
                schema: "estoque",
                table: "sub_ordem_itens",
                column: "ordem_id");

            migrationBuilder.CreateIndex(
                name: "i_x_sub_ordem_itens_tenant_id_ordem_id",
                schema: "estoque",
                table: "sub_ordem_itens",
                columns: new[] { "tenant_id", "ordem_id" });

            migrationBuilder.CreateIndex(
                name: "ix__sub_ordem_item_sync_id",
                schema: "estoque",
                table: "sub_ordem_itens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sub_ordem_item_tenant_id",
                schema: "estoque",
                table: "sub_ordem_itens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_sub_ordens_tenant_id_fornecedor_id",
                schema: "estoque",
                table: "sub_ordens",
                columns: new[] { "tenant_id", "fornecedor_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_sub_ordens_tenant_id_status",
                schema: "estoque",
                table: "sub_ordens",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__sub_ordem_sync_id",
                schema: "estoque",
                table: "sub_ordens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sub_ordem_tenant_id",
                schema: "estoque",
                table: "sub_ordens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_sub_retorno_itens_retorno_id",
                schema: "estoque",
                table: "sub_retorno_itens",
                column: "retorno_id");

            migrationBuilder.CreateIndex(
                name: "i_x_sub_retorno_itens_tenant_id_retorno_id",
                schema: "estoque",
                table: "sub_retorno_itens",
                columns: new[] { "tenant_id", "retorno_id" });

            migrationBuilder.CreateIndex(
                name: "ix__sub_retorno_item_sync_id",
                schema: "estoque",
                table: "sub_retorno_itens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sub_retorno_item_tenant_id",
                schema: "estoque",
                table: "sub_retorno_itens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_sub_retornos_tenant_id_ordem_id",
                schema: "estoque",
                table: "sub_retornos",
                columns: new[] { "tenant_id", "ordem_id" });

            migrationBuilder.CreateIndex(
                name: "ix__sub_retorno_sync_id",
                schema: "estoque",
                table: "sub_retornos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sub_retorno_tenant_id",
                schema: "estoque",
                table: "sub_retornos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_sub_saldos_terceiro_tenant_id_fornecedor_id_produto_id_orde~",
                schema: "estoque",
                table: "sub_saldos_terceiro",
                columns: new[] { "tenant_id", "fornecedor_id", "produto_id", "ordem_id" });

            migrationBuilder.CreateIndex(
                name: "ix__sub_saldo_terceiro_sync_id",
                schema: "estoque",
                table: "sub_saldos_terceiro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sub_saldo_terceiro_tenant_id",
                schema: "estoque",
                table: "sub_saldos_terceiro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_sub_servicos_cobranca_tenant_id_ordem_id",
                schema: "estoque",
                table: "sub_servicos_cobranca",
                columns: new[] { "tenant_id", "ordem_id" });

            migrationBuilder.CreateIndex(
                name: "ix__sub_servico_cobranca_sync_id",
                schema: "estoque",
                table: "sub_servicos_cobranca",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sub_servico_cobranca_tenant_id",
                schema: "estoque",
                table: "sub_servicos_cobranca",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_wms_armazens_tenant_id_cidade",
                schema: "estoque",
                table: "wms_armazens",
                columns: new[] { "tenant_id", "cidade" });

            migrationBuilder.CreateIndex(
                name: "i_x_wms_armazens_tenant_id_nome",
                schema: "estoque",
                table: "wms_armazens",
                columns: new[] { "tenant_id", "nome" });

            migrationBuilder.CreateIndex(
                name: "i_x_wms_armazens_tenant_id_usuario_dono_id",
                schema: "estoque",
                table: "wms_armazens",
                columns: new[] { "tenant_id", "usuario_dono_id" });

            migrationBuilder.CreateIndex(
                name: "ix__wms_armazem_sync_id",
                schema: "estoque",
                table: "wms_armazens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__wms_armazem_tenant_id",
                schema: "estoque",
                table: "wms_armazens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_wms_enderecos_operacionais_armazem_id",
                schema: "estoque",
                table: "wms_enderecos_operacionais",
                column: "armazem_id");

            migrationBuilder.CreateIndex(
                name: "i_x_wms_enderecos_operacionais_tenant_id_armazem_id",
                schema: "estoque",
                table: "wms_enderecos_operacionais",
                columns: new[] { "tenant_id", "armazem_id" });

            migrationBuilder.CreateIndex(
                name: "ix__wms_endereco_operacional_sync_id",
                schema: "estoque",
                table: "wms_enderecos_operacionais",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__wms_endereco_operacional_tenant_id",
                schema: "estoque",
                table: "wms_enderecos_operacionais",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gcc_consumos_contrato",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "gcc_contratos_compra_aditivos",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "gcc_contratos_compra_itens",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "sub_documentos_fiscais",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "sub_envio_itens",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "sub_historicos",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "sub_ordem_itens",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "sub_retorno_itens",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "sub_saldos_terceiro",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "sub_servicos_cobranca",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "wms_enderecos_operacionais",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "gcc_contratos_compra",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "sub_envios",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "sub_ordens",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "sub_retornos",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "wms_armazens",
                schema: "estoque");

            migrationBuilder.DropColumn(
                name: "situacao",
                schema: "estoque",
                table: "compra_transportes");
        }
    }
}
