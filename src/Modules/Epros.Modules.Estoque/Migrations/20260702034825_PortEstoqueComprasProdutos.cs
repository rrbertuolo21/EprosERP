using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Estoque.Migrations
{
    /// <inheritdoc />
    public partial class PortEstoqueComprasProdutos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "unidade_medida",
                schema: "estoque",
                table: "unidades_medida",
                type: "character varying(6)",
                maxLength: 6,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "descricao",
                schema: "estoque",
                table: "unidades_medida",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "produto_grupo_id",
                schema: "estoque",
                table: "unidades_medida",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ean",
                schema: "estoque",
                table: "produtos",
                type: "character varying(14)",
                maxLength: 14,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "descricao",
                schema: "estoque",
                table: "produtos",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "codigo_produto_balanca",
                schema: "estoque",
                table: "produtos",
                type: "character varying(13)",
                maxLength: 13,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "codigo",
                schema: "estoque",
                table: "produtos",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "produto_grupo_id",
                schema: "estoque",
                table: "produtos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "descricao",
                schema: "estoque",
                table: "marcas",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "produto_grupo_id",
                schema: "estoque",
                table: "marcas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "descricao",
                schema: "estoque",
                table: "categorias",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "produto_grupo_id",
                schema: "estoque",
                table: "categorias",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "adicionais",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    valor_preco = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_adicionais", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "balancas",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    qnt_digito_identificador = table.Column<int>(type: "integer", nullable: false),
                    qnt_digito_codigo_produto = table.Column<int>(type: "integer", nullable: false),
                    qnt_digito_valor_produto = table.Column<int>(type: "integer", nullable: false),
                    qnt_casa_decimal = table.Column<int>(type: "integer", nullable: false),
                    tipo_valor = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_balancas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "compra_faturas",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_compra_faturas", x => x.id);
                    table.ForeignKey(
                        name: "f_k_compra_faturas_compras_compra_id",
                        column: x => x.compra_id,
                        principalSchema: "estoque",
                        principalTable: "compras",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compra_impostos",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_compra_impostos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "compra_item_impostos",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_item_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_compra_item_impostos", x => x.id);
                    table.ForeignKey(
                        name: "f_k_compra_item_impostos_compra_itens_compra_item_id",
                        column: x => x.compra_item_id,
                        principalSchema: "estoque",
                        principalTable: "compra_itens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compra_item_impostos_ibs_cbs",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cst = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
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
                    table.PrimaryKey("p_k_compra_item_impostos_ibs_cbs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "compra_item_impostos_valor_aproximado",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_item_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_compra_item_impostos_valor_aproximado", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "compra_totais",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_compra_totais", x => x.id);
                    table.ForeignKey(
                        name: "f_k_compra_totais_compras_compra_id",
                        column: x => x.compra_id,
                        principalSchema: "estoque",
                        principalTable: "compras",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compra_totais_ibs_cbs",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_compra_totais_ibs_cbs", x => x.id);
                    table.ForeignKey(
                        name: "f_k_compra_totais_ibs_cbs_compras_compra_id",
                        column: x => x.compra_id,
                        principalSchema: "estoque",
                        principalTable: "compras",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "estoque_movimentos_manuais",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_estoque = table.Column<int>(type: "integer", nullable: false),
                    tipo_movimento = table.Column<int>(type: "integer", nullable: false),
                    quantidade_movimentada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_estoque_movimentos_manuais", x => x.id);
                    table.ForeignKey(
                        name: "f_k_estoque_movimentos_manuais__produtos_produto_id",
                        column: x => x.produto_id,
                        principalSchema: "estoque",
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "estoque_produtos",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade_saldo_estoque = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantidade_estoque_minimo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantidade_estoque_maximo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantidade_estoque_reservado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_saldo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_custo_medio = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tipo_custeio_estoque = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_estoque_produtos", x => x.id);
                    table.ForeignKey(
                        name: "f_k_estoque_produtos__produtos_produto_id",
                        column: x => x.produto_id,
                        principalSchema: "estoque",
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "produto_grupos",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("p_k_produto_grupos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "produto_historico_reajustes",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_produto = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    valor_antigo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tipo = table.Column<int>(type: "integer", nullable: false),
                    fator = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_fixo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_novo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("p_k_produto_historico_reajustes", x => x.id);
                    table.ForeignKey(
                        name: "f_k_produto_historico_reajustes_produtos_produto_id",
                        column: x => x.produto_id,
                        principalSchema: "estoque",
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "produtos_especificos",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor_percentual_glp_derivado_petroleo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_percentual_gas_natural_nacional = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_percentual_gas_natural_importado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_partida = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    uf_consumo = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_produtos_especificos", x => x.id);
                    table.ForeignKey(
                        name: "f_k_produtos_especificos_produtos_produto_id",
                        column: x => x.produto_id,
                        principalSchema: "estoque",
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "unidades_medida_tributavel",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_ncm = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    data_inicio_vigencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_fim_vigencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    unidade_medida = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    descricao = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
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
                    table.PrimaryKey("p_k_unidades_medida_tributavel", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "adicionais_produtos",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    adicionais_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_adicionais_produtos", x => x.id);
                    table.ForeignKey(
                        name: "f_k_adicionais_produtos__produtos_produto_id",
                        column: x => x.produto_id,
                        principalSchema: "estoque",
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_adicionais_produtos_adicionais_adicionais_id",
                        column: x => x.adicionais_id,
                        principalSchema: "estoque",
                        principalTable: "adicionais",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "compra_fatura_duplicatas",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_fatura_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_compra_fatura_duplicatas", x => x.id);
                    table.ForeignKey(
                        name: "f_k_compra_fatura_duplicatas_compra_faturas_compra_fatura_id",
                        column: x => x.compra_fatura_id,
                        principalSchema: "estoque",
                        principalTable: "compra_faturas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "fatos_geradores_estoque",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: true),
                    compra_id = table.Column<Guid>(type: "uuid", nullable: true),
                    estoque_movimento_manual_id = table.Column<Guid>(type: "uuid", nullable: true),
                    origem = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_fatos_geradores_estoque", x => x.id);
                    table.ForeignKey(
                        name: "f_k_fatos_geradores_estoque_compras_compra_id",
                        column: x => x.compra_id,
                        principalSchema: "estoque",
                        principalTable: "compras",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_fatos_geradores_estoque_estoque_movimentos_manuais_estoque_~",
                        column: x => x.estoque_movimento_manual_id,
                        principalSchema: "estoque",
                        principalTable: "estoque_movimentos_manuais",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "produto_especifico_combustivel_origens",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_especifico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    indicador_importacao = table.Column<int>(type: "integer", nullable: false),
                    uf_origem = table.Column<int>(type: "integer", nullable: false),
                    valor_percentual_uf = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_produto_especifico_combustivel_origens", x => x.id);
                    table.ForeignKey(
                        name: "f_k_produto_especifico_combustivel_origens_produtos_especificos~",
                        column: x => x.produto_especifico_id,
                        principalSchema: "estoque",
                        principalTable: "produtos_especificos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "produto_ficha_estoque_entradas",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fato_gerador_estoque_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_estoque = table.Column<int>(type: "integer", nullable: false),
                    quantidade_movimentada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantidade_saldo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_saldo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_produto_ficha_estoque_entradas", x => x.id);
                    table.ForeignKey(
                        name: "f_k_produto_ficha_estoque_entradas_fatos_geradores_estoque_fato~",
                        column: x => x.fato_gerador_estoque_id,
                        principalSchema: "estoque",
                        principalTable: "fatos_geradores_estoque",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_produto_ficha_estoque_entradas_produtos_produto_id",
                        column: x => x.produto_id,
                        principalSchema: "estoque",
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "produto_ficha_estoque_saidas",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fato_gerador_estoque_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_ficha_estoque_entrada_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade_movimentada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_custo_medio = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_total_custo_medio = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_produto_ficha_estoque_saidas", x => x.id);
                    table.ForeignKey(
                        name: "f_k_produto_ficha_estoque_saidas_fatos_geradores_estoque_fato_g~",
                        column: x => x.fato_gerador_estoque_id,
                        principalSchema: "estoque",
                        principalTable: "fatos_geradores_estoque",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_produto_ficha_estoque_saidas_produto_ficha_estoque_entradas~",
                        column: x => x.produto_ficha_estoque_entrada_id,
                        principalSchema: "estoque",
                        principalTable: "produto_ficha_estoque_entradas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_produto_ficha_estoque_saidas_produtos_produto_id",
                        column: x => x.produto_id,
                        principalSchema: "estoque",
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_unidades_medida_produto_grupo_id",
                schema: "estoque",
                table: "unidades_medida",
                column: "produto_grupo_id");

            migrationBuilder.CreateIndex(
                name: "i_x_produtos_balanca_id",
                schema: "estoque",
                table: "produtos",
                column: "balanca_id");

            migrationBuilder.CreateIndex(
                name: "i_x_produtos_produto_grupo_id",
                schema: "estoque",
                table: "produtos",
                column: "produto_grupo_id");

            migrationBuilder.CreateIndex(
                name: "i_x_marcas_produto_grupo_id",
                schema: "estoque",
                table: "marcas",
                column: "produto_grupo_id");

            migrationBuilder.CreateIndex(
                name: "i_x_categorias_produto_grupo_id",
                schema: "estoque",
                table: "categorias",
                column: "produto_grupo_id");

            migrationBuilder.CreateIndex(
                name: "i_x_adicionais_tenant_id_descricao",
                schema: "estoque",
                table: "adicionais",
                columns: new[] { "tenant_id", "descricao" });

            migrationBuilder.CreateIndex(
                name: "ix__adicionais_sync_id",
                schema: "estoque",
                table: "adicionais",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__adicionais_tenant_id",
                schema: "estoque",
                table: "adicionais",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_adicionais_produtos_adicionais_id",
                schema: "estoque",
                table: "adicionais_produtos",
                column: "adicionais_id");

            migrationBuilder.CreateIndex(
                name: "i_x_adicionais_produtos_produto_id",
                schema: "estoque",
                table: "adicionais_produtos",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "i_x_adicionais_produtos_tenant_id_produto_id",
                schema: "estoque",
                table: "adicionais_produtos",
                columns: new[] { "tenant_id", "produto_id" });

            migrationBuilder.CreateIndex(
                name: "ix__adicionais_produto_sync_id",
                schema: "estoque",
                table: "adicionais_produtos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__adicionais_produto_tenant_id",
                schema: "estoque",
                table: "adicionais_produtos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_balancas_tenant_id_nome",
                schema: "estoque",
                table: "balancas",
                columns: new[] { "tenant_id", "nome" });

            migrationBuilder.CreateIndex(
                name: "ix__balanca_sync_id",
                schema: "estoque",
                table: "balancas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__balanca_tenant_id",
                schema: "estoque",
                table: "balancas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_fatura_duplicatas_compra_fatura_id",
                schema: "estoque",
                table: "compra_fatura_duplicatas",
                column: "compra_fatura_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_fatura_duplicatas_tenant_id_compra_fatura_id",
                schema: "estoque",
                table: "compra_fatura_duplicatas",
                columns: new[] { "tenant_id", "compra_fatura_id" });

            migrationBuilder.CreateIndex(
                name: "ix__compra_fatura_duplicata_sync_id",
                schema: "estoque",
                table: "compra_fatura_duplicatas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_fatura_duplicata_tenant_id",
                schema: "estoque",
                table: "compra_fatura_duplicatas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_faturas_compra_id",
                schema: "estoque",
                table: "compra_faturas",
                column: "compra_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_fatura_sync_id",
                schema: "estoque",
                table: "compra_faturas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_fatura_tenant_id",
                schema: "estoque",
                table: "compra_faturas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_impostos_tenant_id_compra_id",
                schema: "estoque",
                table: "compra_impostos",
                columns: new[] { "tenant_id", "compra_id" });

            migrationBuilder.CreateIndex(
                name: "ix__compra_imposto_sync_id",
                schema: "estoque",
                table: "compra_impostos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_imposto_tenant_id",
                schema: "estoque",
                table: "compra_impostos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_item_impostos_compra_item_id",
                schema: "estoque",
                table: "compra_item_impostos",
                column: "compra_item_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_item_impostos_tenant_id_compra_item_id",
                schema: "estoque",
                table: "compra_item_impostos",
                columns: new[] { "tenant_id", "compra_item_id" });

            migrationBuilder.CreateIndex(
                name: "ix__compra_item_imposto_sync_id",
                schema: "estoque",
                table: "compra_item_impostos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_item_imposto_tenant_id",
                schema: "estoque",
                table: "compra_item_impostos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_item_impostos_ibs_cbs_tenant_id_compra_item_id",
                schema: "estoque",
                table: "compra_item_impostos_ibs_cbs",
                columns: new[] { "tenant_id", "compra_item_id" });

            migrationBuilder.CreateIndex(
                name: "ix__compra_item_imposto_ibs_cbs_sync_id",
                schema: "estoque",
                table: "compra_item_impostos_ibs_cbs",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_item_imposto_ibs_cbs_tenant_id",
                schema: "estoque",
                table: "compra_item_impostos_ibs_cbs",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_item_impostos_valor_aproximado_tenant_id_compra_item~",
                schema: "estoque",
                table: "compra_item_impostos_valor_aproximado",
                columns: new[] { "tenant_id", "compra_item_id" });

            migrationBuilder.CreateIndex(
                name: "ix__compra_item_imposto_valor_aproximado_sync_id",
                schema: "estoque",
                table: "compra_item_impostos_valor_aproximado",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_item_imposto_valor_aproximado_tenant_id",
                schema: "estoque",
                table: "compra_item_impostos_valor_aproximado",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_totais_compra_id",
                schema: "estoque",
                table: "compra_totais",
                column: "compra_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_total_sync_id",
                schema: "estoque",
                table: "compra_totais",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_total_tenant_id",
                schema: "estoque",
                table: "compra_totais",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compra_totais_ibs_cbs_compra_id",
                schema: "estoque",
                table: "compra_totais_ibs_cbs",
                column: "compra_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_total_ibs_cbs_sync_id",
                schema: "estoque",
                table: "compra_totais_ibs_cbs",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compra_total_ibs_cbs_tenant_id",
                schema: "estoque",
                table: "compra_totais_ibs_cbs",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_estoque_movimentos_manuais_produto_id",
                schema: "estoque",
                table: "estoque_movimentos_manuais",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "i_x_estoque_movimentos_manuais_tenant_id_produto_id",
                schema: "estoque",
                table: "estoque_movimentos_manuais",
                columns: new[] { "tenant_id", "produto_id" });

            migrationBuilder.CreateIndex(
                name: "ix__estoque_movimento_manual_sync_id",
                schema: "estoque",
                table: "estoque_movimentos_manuais",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__estoque_movimento_manual_tenant_id",
                schema: "estoque",
                table: "estoque_movimentos_manuais",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_estoque_produtos_produto_id",
                schema: "estoque",
                table: "estoque_produtos",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "i_x_estoque_produtos_tenant_id_empresa_id_produto_id",
                schema: "estoque",
                table: "estoque_produtos",
                columns: new[] { "tenant_id", "empresa_id", "produto_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__estoque_produto_sync_id",
                schema: "estoque",
                table: "estoque_produtos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__estoque_produto_tenant_id",
                schema: "estoque",
                table: "estoque_produtos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_fatos_geradores_estoque_compra_id",
                schema: "estoque",
                table: "fatos_geradores_estoque",
                column: "compra_id");

            migrationBuilder.CreateIndex(
                name: "i_x_fatos_geradores_estoque_estoque_movimento_manual_id",
                schema: "estoque",
                table: "fatos_geradores_estoque",
                column: "estoque_movimento_manual_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_fatos_geradores_estoque_tenant_id_origem",
                schema: "estoque",
                table: "fatos_geradores_estoque",
                columns: new[] { "tenant_id", "origem" });

            migrationBuilder.CreateIndex(
                name: "ix__fato_gerador_estoque_sync_id",
                schema: "estoque",
                table: "fatos_geradores_estoque",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fato_gerador_estoque_tenant_id",
                schema: "estoque",
                table: "fatos_geradores_estoque",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_produto_especifico_combustivel_origens_produto_especifico_id",
                schema: "estoque",
                table: "produto_especifico_combustivel_origens",
                column: "produto_especifico_id");

            migrationBuilder.CreateIndex(
                name: "i_x_produto_especifico_combustivel_origens_tenant_id_produto_es~",
                schema: "estoque",
                table: "produto_especifico_combustivel_origens",
                columns: new[] { "tenant_id", "produto_especifico_id" });

            migrationBuilder.CreateIndex(
                name: "ix__produto_especifico_combustivel_origem_sync_id",
                schema: "estoque",
                table: "produto_especifico_combustivel_origens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__produto_especifico_combustivel_origem_tenant_id",
                schema: "estoque",
                table: "produto_especifico_combustivel_origens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_produto_ficha_estoque_entradas_fato_gerador_estoque_id",
                schema: "estoque",
                table: "produto_ficha_estoque_entradas",
                column: "fato_gerador_estoque_id");

            migrationBuilder.CreateIndex(
                name: "i_x_produto_ficha_estoque_entradas_produto_id",
                schema: "estoque",
                table: "produto_ficha_estoque_entradas",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "i_x_produto_ficha_estoque_entradas_tenant_id_empresa_id_produto~",
                schema: "estoque",
                table: "produto_ficha_estoque_entradas",
                columns: new[] { "tenant_id", "empresa_id", "produto_id" });

            migrationBuilder.CreateIndex(
                name: "ix__produto_ficha_estoque_entrada_sync_id",
                schema: "estoque",
                table: "produto_ficha_estoque_entradas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__produto_ficha_estoque_entrada_tenant_id",
                schema: "estoque",
                table: "produto_ficha_estoque_entradas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_produto_ficha_estoque_saidas_fato_gerador_estoque_id",
                schema: "estoque",
                table: "produto_ficha_estoque_saidas",
                column: "fato_gerador_estoque_id");

            migrationBuilder.CreateIndex(
                name: "i_x_produto_ficha_estoque_saidas_produto_ficha_estoque_entrada_~",
                schema: "estoque",
                table: "produto_ficha_estoque_saidas",
                column: "produto_ficha_estoque_entrada_id");

            migrationBuilder.CreateIndex(
                name: "i_x_produto_ficha_estoque_saidas_produto_id",
                schema: "estoque",
                table: "produto_ficha_estoque_saidas",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "i_x_produto_ficha_estoque_saidas_tenant_id_empresa_id_produto_id",
                schema: "estoque",
                table: "produto_ficha_estoque_saidas",
                columns: new[] { "tenant_id", "empresa_id", "produto_id" });

            migrationBuilder.CreateIndex(
                name: "ix__produto_ficha_estoque_saida_sync_id",
                schema: "estoque",
                table: "produto_ficha_estoque_saidas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__produto_ficha_estoque_saida_tenant_id",
                schema: "estoque",
                table: "produto_ficha_estoque_saidas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_produto_grupos_tenant_id_descricao",
                schema: "estoque",
                table: "produto_grupos",
                columns: new[] { "tenant_id", "descricao" });

            migrationBuilder.CreateIndex(
                name: "ix__produto_grupo_sync_id",
                schema: "estoque",
                table: "produto_grupos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__produto_grupo_tenant_id",
                schema: "estoque",
                table: "produto_grupos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_produto_historico_reajustes_produto_id",
                schema: "estoque",
                table: "produto_historico_reajustes",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "i_x_produto_historico_reajustes_tenant_id_produto_id",
                schema: "estoque",
                table: "produto_historico_reajustes",
                columns: new[] { "tenant_id", "produto_id" });

            migrationBuilder.CreateIndex(
                name: "ix__produto_historico_reajuste_sync_id",
                schema: "estoque",
                table: "produto_historico_reajustes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__produto_historico_reajuste_tenant_id",
                schema: "estoque",
                table: "produto_historico_reajustes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_produtos_especificos_produto_id",
                schema: "estoque",
                table: "produtos_especificos",
                column: "produto_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_produtos_especificos_tenant_id_produto_id",
                schema: "estoque",
                table: "produtos_especificos",
                columns: new[] { "tenant_id", "produto_id" });

            migrationBuilder.CreateIndex(
                name: "ix__produto_especifico_sync_id",
                schema: "estoque",
                table: "produtos_especificos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__produto_especifico_tenant_id",
                schema: "estoque",
                table: "produtos_especificos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_unidades_medida_tributavel_tenant_id_codigo_ncm",
                schema: "estoque",
                table: "unidades_medida_tributavel",
                columns: new[] { "tenant_id", "codigo_ncm" });

            migrationBuilder.CreateIndex(
                name: "ix__unidade_medida_tributavel_sync_id",
                schema: "estoque",
                table: "unidades_medida_tributavel",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__unidade_medida_tributavel_tenant_id",
                schema: "estoque",
                table: "unidades_medida_tributavel",
                column: "tenant_id");

            migrationBuilder.AddForeignKey(
                name: "f_k_categorias__produto_grupos_produto_grupo_id",
                schema: "estoque",
                table: "categorias",
                column: "produto_grupo_id",
                principalSchema: "estoque",
                principalTable: "produto_grupos",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "f_k_marcas__produto_grupos_produto_grupo_id",
                schema: "estoque",
                table: "marcas",
                column: "produto_grupo_id",
                principalSchema: "estoque",
                principalTable: "produto_grupos",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "f_k_produtos__produto_grupos_produto_grupo_id",
                schema: "estoque",
                table: "produtos",
                column: "produto_grupo_id",
                principalSchema: "estoque",
                principalTable: "produto_grupos",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "f_k_produtos_balancas_balanca_id",
                schema: "estoque",
                table: "produtos",
                column: "balanca_id",
                principalSchema: "estoque",
                principalTable: "balancas",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "f_k_unidades_medida_produto_grupos_produto_grupo_id",
                schema: "estoque",
                table: "unidades_medida",
                column: "produto_grupo_id",
                principalSchema: "estoque",
                principalTable: "produto_grupos",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_categorias__produto_grupos_produto_grupo_id",
                schema: "estoque",
                table: "categorias");

            migrationBuilder.DropForeignKey(
                name: "f_k_marcas__produto_grupos_produto_grupo_id",
                schema: "estoque",
                table: "marcas");

            migrationBuilder.DropForeignKey(
                name: "f_k_produtos__produto_grupos_produto_grupo_id",
                schema: "estoque",
                table: "produtos");

            migrationBuilder.DropForeignKey(
                name: "f_k_produtos_balancas_balanca_id",
                schema: "estoque",
                table: "produtos");

            migrationBuilder.DropForeignKey(
                name: "f_k_unidades_medida_produto_grupos_produto_grupo_id",
                schema: "estoque",
                table: "unidades_medida");

            migrationBuilder.DropTable(
                name: "adicionais_produtos",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "balancas",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "compra_fatura_duplicatas",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "compra_impostos",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "compra_item_impostos",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "compra_item_impostos_ibs_cbs",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "compra_item_impostos_valor_aproximado",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "compra_totais",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "compra_totais_ibs_cbs",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "estoque_produtos",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "produto_especifico_combustivel_origens",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "produto_ficha_estoque_saidas",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "produto_grupos",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "produto_historico_reajustes",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "unidades_medida_tributavel",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "adicionais",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "compra_faturas",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "produtos_especificos",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "produto_ficha_estoque_entradas",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "fatos_geradores_estoque",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "estoque_movimentos_manuais",
                schema: "estoque");

            migrationBuilder.DropIndex(
                name: "i_x_unidades_medida_produto_grupo_id",
                schema: "estoque",
                table: "unidades_medida");

            migrationBuilder.DropIndex(
                name: "i_x_produtos_balanca_id",
                schema: "estoque",
                table: "produtos");

            migrationBuilder.DropIndex(
                name: "i_x_produtos_produto_grupo_id",
                schema: "estoque",
                table: "produtos");

            migrationBuilder.DropIndex(
                name: "i_x_marcas_produto_grupo_id",
                schema: "estoque",
                table: "marcas");

            migrationBuilder.DropIndex(
                name: "i_x_categorias_produto_grupo_id",
                schema: "estoque",
                table: "categorias");

            migrationBuilder.DropColumn(
                name: "produto_grupo_id",
                schema: "estoque",
                table: "unidades_medida");

            migrationBuilder.DropColumn(
                name: "produto_grupo_id",
                schema: "estoque",
                table: "produtos");

            migrationBuilder.DropColumn(
                name: "produto_grupo_id",
                schema: "estoque",
                table: "marcas");

            migrationBuilder.DropColumn(
                name: "produto_grupo_id",
                schema: "estoque",
                table: "categorias");

            migrationBuilder.AlterColumn<string>(
                name: "unidade_medida",
                schema: "estoque",
                table: "unidades_medida",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(6)",
                oldMaxLength: 6);

            migrationBuilder.AlterColumn<string>(
                name: "descricao",
                schema: "estoque",
                table: "unidades_medida",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "ean",
                schema: "estoque",
                table: "produtos",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(14)",
                oldMaxLength: 14);

            migrationBuilder.AlterColumn<string>(
                name: "descricao",
                schema: "estoque",
                table: "produtos",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120);

            migrationBuilder.AlterColumn<string>(
                name: "codigo_produto_balanca",
                schema: "estoque",
                table: "produtos",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(13)",
                oldMaxLength: 13);

            migrationBuilder.AlterColumn<string>(
                name: "codigo",
                schema: "estoque",
                table: "produtos",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(60)",
                oldMaxLength: 60);

            migrationBuilder.AlterColumn<string>(
                name: "descricao",
                schema: "estoque",
                table: "marcas",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "descricao",
                schema: "estoque",
                table: "categorias",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);
        }
    }
}
