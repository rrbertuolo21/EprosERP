using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Fiscal.Migrations
{
    /// <inheritdoc />
    public partial class PortConsolidacaoFiscal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // FK obrigatoria (dominio Servico valida EmpresaId != Guid.Empty; legado exigia EmpresaId > 0).
            // Removido defaultValue Guid.Empty (1R.1/1R.2) para evitar FK invalida em novos registros.
            // Banco zerado => AddColumn NOT NULL sem default e seguro.
            migrationBuilder.AddColumn<Guid>(
                name: "empresa_id",
                schema: "plataforma",
                table: "servicos",
                type: "uuid",
                nullable: false);

            migrationBuilder.AddColumn<long>(
                name: "sequencia_exibicao",
                schema: "plataforma",
                table: "servicos",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "cests",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
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
                    table.PrimaryKey("p_k_cests", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "codigos_anp",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    data_inicio_vigencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_final_vigencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_codigos_anp", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "configuracoes_d_fe",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    n_fe_serie_producao = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    n_fe_ultimo_nr_producao = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    n_fe_serie_homologacao = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    n_fe_ultimo_nr_homologacao = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    nfce_csc_producao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    nfce_id_csc_producao = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    nfce_serie_producao = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    nfce_ultimo_nr_producao = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    nfce_csc_homologacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    nfce_id_csc_homologacao = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    nfce_serie_homologacao = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    nfce_ultimo_nr_homologacao = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("p_k_configuracoes_d_fe", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "configuracoes_impressao_nfce",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    detalhe_venda_normal = table.Column<int>(type: "integer", nullable: true),
                    detalhe_venda_contingencia = table.Column<int>(type: "integer", nullable: true),
                    imprime_desconto_item = table.Column<bool>(type: "boolean", nullable: true),
                    imprime_fone_emitente = table.Column<bool>(type: "boolean", nullable: true),
                    margem_esquerda = table.Column<float>(type: "real", nullable: true),
                    margem_direita = table.Column<float>(type: "real", nullable: true),
                    modo_impressao = table.Column<int>(type: "integer", nullable: true),
                    nfce_layout_qr_code = table.Column<int>(type: "integer", nullable: true),
                    versao_qr_code = table.Column<int>(type: "integer", nullable: true),
                    segunda_via_contingencia = table.Column<bool>(type: "boolean", nullable: true),
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
                    table.PrimaryKey("p_k_configuracoes_impressao_nfce", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "csts_ibs_cbs",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cst = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    data_inicio_vigencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_fim_vigencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_csts_ibs_cbs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "enquadramentos_ipi",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    tipo_operacao = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_enquadramentos_ipi", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fcp_aliquotas_uf",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    uf = table.Column<int>(type: "integer", nullable: false),
                    valor_aliquota = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    observacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("p_k_fcp_aliquotas_uf", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "icms_aliquotas_interestaduais",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    uf_origem = table.Column<int>(type: "integer", nullable: false),
                    uf_destino = table.Column<int>(type: "integer", nullable: false),
                    valor_aliquota = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_icms_aliquotas_interestaduais", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ncms",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_ncm = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    descricao = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: false),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tipo_ato_ini = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    numero_ato_ini = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ano_ato_ini = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
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
                    table.PrimaryKey("p_k_ncms", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "observacoes_nfe",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
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
                    table.PrimaryKey("p_k_observacoes_nfe", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tributario_grupos",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequencia_exibicao = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("p_k_tributario_grupos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "classificacoes_tributarias",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cst_ibs_cbs_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    data_inicio_vigencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_fim_vigencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ind_nfe = table.Column<bool>(type: "boolean", nullable: false),
                    ind_nfce = table.Column<bool>(type: "boolean", nullable: false),
                    ind_cte = table.Column<bool>(type: "boolean", nullable: false),
                    ind_cteos = table.Column<bool>(type: "boolean", nullable: false),
                    ind_nfse = table.Column<bool>(type: "boolean", nullable: false),
                    ind_trib_regular = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_classificacoes_tributarias", x => x.id);
                    table.ForeignKey(
                        name: "f_k_classificacoes_tributarias__csts_ibs_cbs_cst_ibs_cbs_id",
                        column: x => x.cst_ibs_cbs_id,
                        principalSchema: "plataforma",
                        principalTable: "csts_ibs_cbs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ncm_tributacoes",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequencia_exibicao = table.Column<long>(type: "bigint", nullable: true),
                    tributario_grupo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_beneficio_fiscal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cod_regra = table.Column<int>(type: "integer", nullable: false),
                    descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cfop_nota_consumidor = table.Column<int>(type: "integer", nullable: false),
                    cfop_nota_fiscal = table.Column<int>(type: "integer", nullable: false),
                    cfop_nota_fiscal_interestadual = table.Column<int>(type: "integer", nullable: false),
                    origem = table.Column<int>(type: "integer", nullable: false),
                    csosn_nota_consumidor = table.Column<int>(type: "integer", nullable: false),
                    cst_icms_nota_consumidor = table.Column<int>(type: "integer", nullable: false),
                    csosn_nota_fiscal = table.Column<int>(type: "integer", nullable: false),
                    cst_icms_nota_fiscal_interna = table.Column<int>(type: "integer", nullable: false),
                    cst_icms_nota_fiscal_interstadual = table.Column<int>(type: "integer", nullable: false),
                    cst_pis = table.Column<int>(type: "integer", nullable: false),
                    cst_cofins = table.Column<int>(type: "integer", nullable: false),
                    valor_unit_fixo_pis = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_unit_fixo_cofins = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_aliquota_pis = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_aliquota_cofins = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    cst_pis_cofins_entrada = table.Column<int>(type: "integer", nullable: false),
                    cst_ipi_saida = table.Column<int>(type: "integer", nullable: false),
                    cst_ipi_entrada = table.Column<int>(type: "integer", nullable: false),
                    valor_aliquota_ipi = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_percentual_reducacao_bc_ipi = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tipo_reducao_ipi = table.Column<int>(type: "integer", nullable: false),
                    destino_reducao_ipi = table.Column<int>(type: "integer", nullable: false),
                    ipi_embutido = table.Column<bool>(type: "boolean", nullable: false),
                    enquadramento_ipi = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    codigo_valor_fiscal_icms_interna = table.Column<int>(type: "integer", nullable: false),
                    codigo_valor_fiscalcms_interstadual = table.Column<int>(type: "integer", nullable: false),
                    valor_aliquota_icms_interna = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_percentual_reducacao_bc_icms_interna = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tipo_reducao_icms_interna = table.Column<int>(type: "integer", nullable: false),
                    destino_reducao_icms_interna = table.Column<int>(type: "integer", nullable: false),
                    valor_aliquota_icms_interstadual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_percentual_reducacao_bc_icms_interstadual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tipo_reducao_icms_interstadual = table.Column<int>(type: "integer", nullable: false),
                    destino_reducao_icms_interstadual = table.Column<int>(type: "integer", nullable: false),
                    codigo_beneficio_fiscal_icms = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    motivo_desoneracao_icms = table.Column<int>(type: "integer", nullable: false),
                    informacoes_complementares = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    informacoes_adicionais_ao_fisco = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    cst_ibs_cbs_nfe = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    c_class_trib_nfe = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    cst_ibs_cbs_nfce = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    c_class_trib_nfce = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
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
                    table.PrimaryKey("p_k_ncm_tributacoes", x => x.id);
                    table.ForeignKey(
                        name: "f_k_ncm_tributacoes__tributario_grupos_tributario_grupo_id",
                        column: x => x.tributario_grupo_id,
                        principalSchema: "plataforma",
                        principalTable: "tributario_grupos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_ncm_tributacoes_codigos_beneficios_fiscais_codigo_beneficio~",
                        column: x => x.codigo_beneficio_fiscal_id,
                        principalSchema: "plataforma",
                        principalTable: "codigos_beneficios_fiscais",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tributario_grupo_empresas",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tributario_grupo_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_tributario_grupo_empresas", x => x.id);
                    table.ForeignKey(
                        name: "f_k_tributario_grupo_empresas_tributario_grupos_tributario_grup~",
                        column: x => x.tributario_grupo_id,
                        principalSchema: "plataforma",
                        principalTable: "tributario_grupos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "classificacoes_tributarias_anexos",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    classificacao_tributaria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nro_anexo = table.Column<int>(type: "integer", nullable: false),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    data_inicio_vigencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_fim_vigencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_classificacoes_tributarias_anexos", x => x.id);
                    table.ForeignKey(
                        name: "f_k_classificacoes_tributarias_anexos_classificacoes_tributaria~",
                        column: x => x.classificacao_tributaria_id,
                        principalSchema: "plataforma",
                        principalTable: "classificacoes_tributarias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ncm_configuracoes",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ncm_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ncm_tributacao_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_ncm_configuracoes", x => x.id);
                    table.ForeignKey(
                        name: "f_k_ncm_configuracoes__ncm_tributacoes_ncm_tributacao_id",
                        column: x => x.ncm_tributacao_id,
                        principalSchema: "plataforma",
                        principalTable: "ncm_tributacoes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ncm_tributacao_empresas",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ncm_tributacao_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_ncm_tributacao_empresas", x => x.id);
                    table.ForeignKey(
                        name: "f_k_ncm_tributacao_empresas_ncm_tributacoes_ncm_tributacao_id",
                        column: x => x.ncm_tributacao_id,
                        principalSchema: "plataforma",
                        principalTable: "ncm_tributacoes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ncm_tributacao_fundo_combate_pobrezas",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ncm_tributacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    uf = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    valor_percentual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_ncm_tributacao_fundo_combate_pobrezas", x => x.id);
                    table.ForeignKey(
                        name: "f_k_ncm_tributacao_fundo_combate_pobrezas_ncm_tributacoes_ncm_t~",
                        column: x => x.ncm_tributacao_id,
                        principalSchema: "plataforma",
                        principalTable: "ncm_tributacoes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ncm_tributacao_sts",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ncm_tributacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    uf = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    tipo_calculo = table.Column<int>(type: "integer", nullable: false),
                    valor_aliquota_icms_st = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_mva = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_percentual_reducao_bc_icms_st = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tipo_reducao_icms_st = table.Column<int>(type: "integer", nullable: false),
                    valor_unitario_st = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_percentual_fcp_st = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_ncm_tributacao_sts", x => x.id);
                    table.ForeignKey(
                        name: "f_k_ncm_tributacao_sts_ncm_tributacoes_ncm_tributacao_id",
                        column: x => x.ncm_tributacao_id,
                        principalSchema: "plataforma",
                        principalTable: "ncm_tributacoes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_servicos_tenant_id_empresa_id",
                schema: "plataforma",
                table: "servicos",
                columns: new[] { "tenant_id", "empresa_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_cests_tenant_id_codigo",
                schema: "plataforma",
                table: "cests",
                columns: new[] { "tenant_id", "codigo" });

            migrationBuilder.CreateIndex(
                name: "ix__cest_sync_id",
                schema: "plataforma",
                table: "cests",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__cest_tenant_id",
                schema: "plataforma",
                table: "cests",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_classificacoes_tributarias_cst_ibs_cbs_id",
                schema: "plataforma",
                table: "classificacoes_tributarias",
                column: "cst_ibs_cbs_id");

            migrationBuilder.CreateIndex(
                name: "i_x_classificacoes_tributarias_tenant_id_cst_ibs_cbs_id",
                schema: "plataforma",
                table: "classificacoes_tributarias",
                columns: new[] { "tenant_id", "cst_ibs_cbs_id" });

            migrationBuilder.CreateIndex(
                name: "ix__classificacao_tributaria_sync_id",
                schema: "plataforma",
                table: "classificacoes_tributarias",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__classificacao_tributaria_tenant_id",
                schema: "plataforma",
                table: "classificacoes_tributarias",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_classificacoes_tributarias_anexos_classificacao_tributaria_~",
                schema: "plataforma",
                table: "classificacoes_tributarias_anexos",
                column: "classificacao_tributaria_id");

            migrationBuilder.CreateIndex(
                name: "i_x_classificacoes_tributarias_anexos_tenant_id_classificacao_t~",
                schema: "plataforma",
                table: "classificacoes_tributarias_anexos",
                columns: new[] { "tenant_id", "classificacao_tributaria_id" });

            migrationBuilder.CreateIndex(
                name: "ix__classificacao_tributaria_anexo_sync_id",
                schema: "plataforma",
                table: "classificacoes_tributarias_anexos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__classificacao_tributaria_anexo_tenant_id",
                schema: "plataforma",
                table: "classificacoes_tributarias_anexos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_codigos_anp_tenant_id_codigo",
                schema: "plataforma",
                table: "codigos_anp",
                columns: new[] { "tenant_id", "codigo" });

            migrationBuilder.CreateIndex(
                name: "ix__codigo_anp_sync_id",
                schema: "plataforma",
                table: "codigos_anp",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__codigo_anp_tenant_id",
                schema: "plataforma",
                table: "codigos_anp",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_configuracoes_d_fe_tenant_id_empresa_id",
                schema: "plataforma",
                table: "configuracoes_d_fe",
                columns: new[] { "tenant_id", "empresa_id" });

            migrationBuilder.CreateIndex(
                name: "ix__configuracao_d_fe_sync_id",
                schema: "plataforma",
                table: "configuracoes_d_fe",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__configuracao_d_fe_tenant_id",
                schema: "plataforma",
                table: "configuracoes_d_fe",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_configuracoes_impressao_nfce_tenant_id_empresa_id",
                schema: "plataforma",
                table: "configuracoes_impressao_nfce",
                columns: new[] { "tenant_id", "empresa_id" });

            migrationBuilder.CreateIndex(
                name: "ix__configuracao_impressao_nfce_sync_id",
                schema: "plataforma",
                table: "configuracoes_impressao_nfce",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__configuracao_impressao_nfce_tenant_id",
                schema: "plataforma",
                table: "configuracoes_impressao_nfce",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_csts_ibs_cbs_tenant_id_cst",
                schema: "plataforma",
                table: "csts_ibs_cbs",
                columns: new[] { "tenant_id", "cst" });

            migrationBuilder.CreateIndex(
                name: "ix__cst_ibs_cbs_sync_id",
                schema: "plataforma",
                table: "csts_ibs_cbs",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__cst_ibs_cbs_tenant_id",
                schema: "plataforma",
                table: "csts_ibs_cbs",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_enquadramentos_ipi_tenant_id_codigo",
                schema: "plataforma",
                table: "enquadramentos_ipi",
                columns: new[] { "tenant_id", "codigo" });

            migrationBuilder.CreateIndex(
                name: "ix__enquadramento_ipi_sync_id",
                schema: "plataforma",
                table: "enquadramentos_ipi",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__enquadramento_ipi_tenant_id",
                schema: "plataforma",
                table: "enquadramentos_ipi",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_fcp_aliquotas_uf_tenant_id_uf",
                schema: "plataforma",
                table: "fcp_aliquotas_uf",
                columns: new[] { "tenant_id", "uf" });

            migrationBuilder.CreateIndex(
                name: "ix__fcp_aliquota_uf_sync_id",
                schema: "plataforma",
                table: "fcp_aliquotas_uf",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fcp_aliquota_uf_tenant_id",
                schema: "plataforma",
                table: "fcp_aliquotas_uf",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_icms_aliquotas_interestaduais_tenant_id_uf_origem_uf_destino",
                schema: "plataforma",
                table: "icms_aliquotas_interestaduais",
                columns: new[] { "tenant_id", "uf_origem", "uf_destino" });

            migrationBuilder.CreateIndex(
                name: "ix__icms_aliquota_interestadual_sync_id",
                schema: "plataforma",
                table: "icms_aliquotas_interestaduais",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__icms_aliquota_interestadual_tenant_id",
                schema: "plataforma",
                table: "icms_aliquotas_interestaduais",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ncm_configuracoes_ncm_tributacao_id",
                schema: "plataforma",
                table: "ncm_configuracoes",
                column: "ncm_tributacao_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ncm_configuracoes_tenant_id_ncm_tributacao_id",
                schema: "plataforma",
                table: "ncm_configuracoes",
                columns: new[] { "tenant_id", "ncm_tributacao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__ncm_configuracao_sync_id",
                schema: "plataforma",
                table: "ncm_configuracoes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ncm_configuracao_tenant_id",
                schema: "plataforma",
                table: "ncm_configuracoes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ncm_tributacao_empresas_ncm_tributacao_id",
                schema: "plataforma",
                table: "ncm_tributacao_empresas",
                column: "ncm_tributacao_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ncm_tributacao_empresas_tenant_id_empresa_id",
                schema: "plataforma",
                table: "ncm_tributacao_empresas",
                columns: new[] { "tenant_id", "empresa_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_ncm_tributacao_empresas_tenant_id_ncm_tributacao_id",
                schema: "plataforma",
                table: "ncm_tributacao_empresas",
                columns: new[] { "tenant_id", "ncm_tributacao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__ncm_tributacao_empresa_sync_id",
                schema: "plataforma",
                table: "ncm_tributacao_empresas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ncm_tributacao_empresa_tenant_id",
                schema: "plataforma",
                table: "ncm_tributacao_empresas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ncm_tributacao_fundo_combate_pobrezas_ncm_tributacao_id",
                schema: "plataforma",
                table: "ncm_tributacao_fundo_combate_pobrezas",
                column: "ncm_tributacao_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ncm_tributacao_fundo_combate_pobrezas_tenant_id_ncm_tributa~",
                schema: "plataforma",
                table: "ncm_tributacao_fundo_combate_pobrezas",
                columns: new[] { "tenant_id", "ncm_tributacao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__ncm_tributacao_fundo_combate_pobreza_sync_id",
                schema: "plataforma",
                table: "ncm_tributacao_fundo_combate_pobrezas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ncm_tributacao_fundo_combate_pobreza_tenant_id",
                schema: "plataforma",
                table: "ncm_tributacao_fundo_combate_pobrezas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ncm_tributacao_sts_ncm_tributacao_id",
                schema: "plataforma",
                table: "ncm_tributacao_sts",
                column: "ncm_tributacao_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ncm_tributacao_sts_tenant_id_ncm_tributacao_id",
                schema: "plataforma",
                table: "ncm_tributacao_sts",
                columns: new[] { "tenant_id", "ncm_tributacao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__ncm_tributacao_st_sync_id",
                schema: "plataforma",
                table: "ncm_tributacao_sts",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ncm_tributacao_st_tenant_id",
                schema: "plataforma",
                table: "ncm_tributacao_sts",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ncm_tributacoes_codigo_beneficio_fiscal_id",
                schema: "plataforma",
                table: "ncm_tributacoes",
                column: "codigo_beneficio_fiscal_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ncm_tributacoes_tenant_id_tributario_grupo_id",
                schema: "plataforma",
                table: "ncm_tributacoes",
                columns: new[] { "tenant_id", "tributario_grupo_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_ncm_tributacoes_tributario_grupo_id",
                schema: "plataforma",
                table: "ncm_tributacoes",
                column: "tributario_grupo_id");

            migrationBuilder.CreateIndex(
                name: "ix__ncm_tributacao_sync_id",
                schema: "plataforma",
                table: "ncm_tributacoes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ncm_tributacao_tenant_id",
                schema: "plataforma",
                table: "ncm_tributacoes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ncms_tenant_id_codigo_ncm",
                schema: "plataforma",
                table: "ncms",
                columns: new[] { "tenant_id", "codigo_ncm" });

            migrationBuilder.CreateIndex(
                name: "ix__ncm_sync_id",
                schema: "plataforma",
                table: "ncms",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ncm_tenant_id",
                schema: "plataforma",
                table: "ncms",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__observacao_nfe_sync_id",
                schema: "plataforma",
                table: "observacoes_nfe",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__observacao_nfe_tenant_id",
                schema: "plataforma",
                table: "observacoes_nfe",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_tributario_grupo_empresas_tenant_id_empresa_id",
                schema: "plataforma",
                table: "tributario_grupo_empresas",
                columns: new[] { "tenant_id", "empresa_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_tributario_grupo_empresas_tenant_id_tributario_grupo_id",
                schema: "plataforma",
                table: "tributario_grupo_empresas",
                columns: new[] { "tenant_id", "tributario_grupo_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_tributario_grupo_empresas_tributario_grupo_id",
                schema: "plataforma",
                table: "tributario_grupo_empresas",
                column: "tributario_grupo_id");

            migrationBuilder.CreateIndex(
                name: "ix__tributario_grupo_empresa_sync_id",
                schema: "plataforma",
                table: "tributario_grupo_empresas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__tributario_grupo_empresa_tenant_id",
                schema: "plataforma",
                table: "tributario_grupo_empresas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_tributario_grupos_tenant_id_descricao",
                schema: "plataforma",
                table: "tributario_grupos",
                columns: new[] { "tenant_id", "descricao" });

            migrationBuilder.CreateIndex(
                name: "ix__tributario_grupo_sync_id",
                schema: "plataforma",
                table: "tributario_grupos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__tributario_grupo_tenant_id",
                schema: "plataforma",
                table: "tributario_grupos",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cests",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "classificacoes_tributarias_anexos",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "codigos_anp",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "configuracoes_d_fe",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "configuracoes_impressao_nfce",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "enquadramentos_ipi",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "fcp_aliquotas_uf",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "icms_aliquotas_interestaduais",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "ncm_configuracoes",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "ncm_tributacao_empresas",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "ncm_tributacao_fundo_combate_pobrezas",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "ncm_tributacao_sts",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "ncms",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "observacoes_nfe",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "tributario_grupo_empresas",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "classificacoes_tributarias",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "ncm_tributacoes",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "csts_ibs_cbs",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "tributario_grupos",
                schema: "plataforma");

            migrationBuilder.DropIndex(
                name: "i_x_servicos_tenant_id_empresa_id",
                schema: "plataforma",
                table: "servicos");

            migrationBuilder.DropColumn(
                name: "empresa_id",
                schema: "plataforma",
                table: "servicos");

            migrationBuilder.DropColumn(
                name: "sequencia_exibicao",
                schema: "plataforma",
                table: "servicos");

            migrationBuilder.EnsureSchema(
                name: "estoque");
        }
    }
}
