using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Qualidade.Migrations
{
    /// <inheritdoc />
    public partial class AddQualidadeSubmodulosNcrInsAcrAdmAtr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "qld_acr_analise",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequencia_exibicao = table.Column<long>(type: "bigint", nullable: true),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    tipo_analise = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    local_id = table.Column<Guid>(type: "uuid", nullable: true),
                    documento_fiscal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    versao = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_qld_acr_analise", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_acr_anexo",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    analise_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    entidade_id = table.Column<Guid>(type: "uuid", nullable: true),
                    arquivo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_anexo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("p_k_qld_acr_anexo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_acr_documento_fiscal",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    local_id = table.Column<Guid>(type: "uuid", nullable: true),
                    chave_fiscal_referencia = table.Column<string>(type: "character varying(44)", maxLength: 44, nullable: true),
                    chave_fiscal_gerada = table.Column<string>(type: "character varying(44)", maxLength: 44, nullable: true),
                    numero_fiscal_referencia = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    numero_fiscal_gerado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    status_fiscal = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    tipo_documento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    valor_integral = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_devolvido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    devolucao_parcial = table.Column<bool>(type: "boolean", nullable: false),
                    motivo_fiscal = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    observacao_fiscal = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    sequencia_cce = table.Column<int>(type: "integer", nullable: true),
                    dados_transporte_json = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_qld_acr_documento_fiscal", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_acr_evento_estoque",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    resultado_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_evento = table.Column<int>(type: "integer", nullable: false),
                    lote = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    local_origem_id = table.Column<Guid>(type: "uuid", nullable: true),
                    local_destino_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status_evento = table.Column<int>(type: "integer", nullable: false),
                    retorno_json = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_qld_acr_evento_estoque", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_acr_evento_ncr",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    resultado_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ncr_id = table.Column<Guid>(type: "uuid", nullable: true),
                    gatilho = table.Column<int>(type: "integer", nullable: false),
                    status_evento = table.Column<int>(type: "integer", nullable: false),
                    retorno_json = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_qld_acr_evento_ncr", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_acr_historico",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    analise_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    acao = table.Column<int>(type: "integer", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ip = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    payload_json = table.Column<string>(type: "text", nullable: false),
                    motivo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ocorrido_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_qld_acr_historico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_acr_item",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    analise_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequencia = table.Column<int>(type: "integer", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    codigo_item = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    nome_item = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    lote = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    quantidade_integral = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    quantidade_analisada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    unidade_medida = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    item_parcial = table.Column<bool>(type: "boolean", nullable: false),
                    ncm = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cfop = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    valor_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_qld_acr_item", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_acr_motivo",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    descricao = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    categoria = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    severidade = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("p_k_qld_acr_motivo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_acr_parametro",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    chave = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    valor_json = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_qld_acr_parametro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_acr_resultado",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    analise_id = table.Column<Guid>(type: "uuid", nullable: false),
                    resultado_inspecao = table.Column<int>(type: "integer", nullable: false),
                    motivo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    severidade = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    quantidade_afetada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    justificativa = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    aprovado_por = table.Column<Guid>(type: "uuid", nullable: true),
                    decidido_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status_fiscal_ignorado = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_qld_acr_resultado", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_adm_anexo",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    qualidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    entidade_id = table.Column<Guid>(type: "uuid", nullable: true),
                    arquivo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_anexo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("p_k_qld_adm_anexo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_adm_documento_qms",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    qualidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    titulo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    tipo_documento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    versao_documento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    vigencia_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    vigencia_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    aprovador_id = table.Column<Guid>(type: "uuid", nullable: true),
                    arquivo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    motivo_revisao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("p_k_qld_adm_documento_qms", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_adm_historico",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    qualidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    acao = table.Column<int>(type: "integer", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ip = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    payload_json = table.Column<string>(type: "text", nullable: false),
                    motivo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ocorrido_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_qld_adm_historico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_adm_item",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    qualidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequencia = table.Column<int>(type: "integer", nullable: false),
                    tipo_item = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_qld_adm_item", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_adm_kpi",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    objetivo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    formula = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    fonte_dados = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    periodicidade = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    meta = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    unidade = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    periodo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    resultado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    status_resultado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("p_k_qld_adm_kpi", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_adm_objetivo",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    qualidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    objetivo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    area_id = table.Column<Guid>(type: "uuid", nullable: true),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    meta = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    unidade = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    prazo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_qld_adm_objetivo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_adm_parametro",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    chave = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    valor_json = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_qld_adm_parametro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_adm_programa_auditoria",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    qualidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    tipo_auditoria = table.Column<int>(type: "integer", nullable: false),
                    escopo = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    data_prevista = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_realizada = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    auditor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    area_auditada_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_qld_adm_programa_auditoria", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_adm_qualidade",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequencia_exibicao = table.Column<long>(type: "bigint", nullable: true),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    versao = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_qld_adm_qualidade", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_atr_anexo",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    atributo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    entidade_id = table.Column<Guid>(type: "uuid", nullable: true),
                    arquivo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_anexo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("p_k_qld_atr_anexo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_atr_atributo",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequencia_exibicao = table.Column<long>(type: "bigint", nullable: true),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    nome_interno = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    rotulo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    tipo_atributo = table.Column<int>(type: "integer", nullable: false),
                    tipo_caracteristica = table.Column<int>(type: "integer", nullable: true),
                    tipo_dado = table.Column<int>(type: "integer", nullable: false),
                    escopo = table.Column<int>(type: "integer", nullable: false),
                    posicao = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    exibir_formulario_padrao = table.Column<bool>(type: "boolean", nullable: false),
                    obrigatorio = table.Column<bool>(type: "boolean", nullable: false),
                    sensivel_lgpd = table.Column<bool>(type: "boolean", nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: true),
                    versao = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_qld_atr_atributo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_atr_especificacao",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    atributo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    contexto_tipo = table.Column<int>(type: "integer", nullable: true),
                    contexto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    versao_especificacao = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    valor_nominal = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    limite_inferior = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    limite_superior = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    tolerancia_menos = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    tolerancia_mais = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    unidade = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    metodo_medicao = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    criticidade = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    vigencia_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    vigencia_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_qld_atr_especificacao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_atr_historico",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    atributo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    acao = table.Column<int>(type: "integer", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payload_json = table.Column<string>(type: "text", nullable: false),
                    motivo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ocorrido_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_qld_atr_historico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_atr_opcao",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    atributo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    rotulo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    posicao = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("p_k_qld_atr_opcao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_atr_parametro",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    chave = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    valor_json = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_qld_atr_parametro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_atr_valor",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    atributo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    contexto_tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    contexto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor_texto = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    valor_numero = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_data = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    valor_booleano = table.Column<bool>(type: "boolean", nullable: true),
                    valor_json = table.Column<string>(type: "text", nullable: true),
                    preenchido_por = table.Column<Guid>(type: "uuid", nullable: true),
                    preenchido_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_qld_atr_valor", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_atr_vinculo_contexto",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    atributo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    contexto_tipo = table.Column<int>(type: "integer", nullable: false),
                    contexto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    obrigatorio = table.Column<bool>(type: "boolean", nullable: false),
                    posicao = table.Column<int>(type: "integer", nullable: true),
                    vigencia_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    vigencia_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_qld_atr_vinculo_contexto", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_ins_amostra",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    execucao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequencia = table.Column<int>(type: "integer", nullable: false),
                    identificador_amostra = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_qld_ins_amostra", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_ins_caracteristica",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    plano_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequencia = table.Column<int>(type: "integer", nullable: false),
                    atributo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    tipo_caracteristica = table.Column<int>(type: "integer", nullable: false),
                    tipo_dado = table.Column<int>(type: "integer", nullable: false),
                    unidade_medida_id = table.Column<Guid>(type: "uuid", nullable: true),
                    valor_nominal = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    limite_inferior = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    limite_superior = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    criterio_qualitativo = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    obrigatoria = table.Column<bool>(type: "boolean", nullable: false),
                    metodo_medicao = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    instrumento_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_qld_ins_caracteristica", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_ins_execucao",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    plano_id = table.Column<Guid>(type: "uuid", nullable: false),
                    referencia_tipo = table.Column<int>(type: "integer", nullable: false),
                    referencia_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    quantidade_lote = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    tamanho_amostra_calculado = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    inspetor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_conclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resultado_preliminar = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("p_k_qld_ins_execucao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_ins_medicao",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    execucao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amostra_id = table.Column<Guid>(type: "uuid", nullable: true),
                    caracteristica_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor_decimal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_texto = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    valor_booleano = table.Column<bool>(type: "boolean", nullable: true),
                    resultado = table.Column<int>(type: "integer", nullable: false),
                    desvio = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    observacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    medido_por = table.Column<Guid>(type: "uuid", nullable: false),
                    medido_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_qld_ins_medicao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_ins_plano",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequencia_exibicao = table.Column<long>(type: "bigint", nullable: true),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    contexto = table.Column<int>(type: "integer", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    processo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    etapa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_inicio_vigencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_fim_vigencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    versao = table.Column<int>(type: "integer", nullable: false),
                    motivo_status = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("p_k_qld_ins_plano", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_ins_regra_amostragem",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    plano_id = table.Column<Guid>(type: "uuid", nullable: false),
                    caracteristica_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_amostragem = table.Column<int>(type: "integer", nullable: false),
                    nivel_inspecao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    aql = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    faixa_lote_min = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    faixa_lote_max = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    tamanho_amostra = table.Column<int>(type: "integer", nullable: true),
                    criterio_aceite = table.Column<int>(type: "integer", nullable: true),
                    criterio_rejeicao = table.Column<int>(type: "integer", nullable: true),
                    severidade = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("p_k_qld_ins_regra_amostragem", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_ins_resultado",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    execucao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    resultado = table.Column<int>(type: "integer", nullable: false),
                    total_amostras = table.Column<int>(type: "integer", nullable: false),
                    total_desvios = table.Column<int>(type: "integer", nullable: false),
                    criterio_aceite_aplicado = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    gerar_acr = table.Column<bool>(type: "boolean", nullable: false),
                    gerar_ncr = table.Column<bool>(type: "boolean", nullable: false),
                    conclusao = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    concluido_por = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_qld_ins_resultado", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_ncr_acao_capa",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ncr_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_acao = table.Column<int>(type: "integer", nullable: false),
                    descricao = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    prazo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    evidencia_obrigatoria = table.Column<bool>(type: "boolean", nullable: false),
                    data_conclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resultado = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    motivo_cancelamento = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("p_k_qld_ncr_acao_capa", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_ncr_anexo",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ncr_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entidade_alvo = table.Column<int>(type: "integer", nullable: false),
                    entidade_alvo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    arquivo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_evidencia = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("p_k_qld_ncr_anexo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_ncr_causa_raiz",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ncr_id = table.Column<Guid>(type: "uuid", nullable: false),
                    metodo = table.Column<int>(type: "integer", nullable: false),
                    descricao_analise = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    causa_identificada = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    conclusao = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    aprovado_por = table.Column<Guid>(type: "uuid", nullable: true),
                    aprovado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_qld_ncr_causa_raiz", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_ncr_evento_integracao",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ncr_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_evento = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    direcao = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_qld_ncr_evento_integracao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_ncr_historico",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ncr_id = table.Column<Guid>(type: "uuid", nullable: false),
                    acao = table.Column<int>(type: "integer", nullable: false),
                    estado_anterior = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    estado_novo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ip_origem = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    payload_json = table.Column<string>(type: "text", nullable: false),
                    justificativa = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("p_k_qld_ncr_historico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_ncr_origem_ref",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ncr_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_origem = table.Column<int>(type: "integer", nullable: false),
                    referencia_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    referencia_codigo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    origem_principal = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_qld_ncr_origem_ref", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_ncr_parametro",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    chave = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    valor_json = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_qld_ncr_parametro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_ncr_registro",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequencia_exibicao = table.Column<long>(type: "bigint", nullable: true),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    titulo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    descricao = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    origem_principal = table.Column<int>(type: "integer", nullable: false),
                    severidade = table.Column<string>(type: "text", nullable: true),
                    prioridade = table.Column<int>(type: "integer", nullable: false),
                    status_registro = table.Column<int>(type: "integer", nullable: false),
                    etapa_ncr = table.Column<int>(type: "integer", nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    area_responsavel_id = table.Column<Guid>(type: "uuid", nullable: true),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    lote_id = table.Column<Guid>(type: "uuid", nullable: true),
                    serial = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_ocorrencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_limite_triagem = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_encerramento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    conclusao = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    motivo_cancelamento = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    versao = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_qld_ncr_registro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_ncr_verificacao_eficacia",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ncr_id = table.Column<Guid>(type: "uuid", nullable: false),
                    acao_capa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    criterio = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    resultado = table.Column<int>(type: "integer", nullable: false),
                    descricao_resultado = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    verificado_por = table.Column<Guid>(type: "uuid", nullable: false),
                    verificado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    proxima_acao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("p_k_qld_ncr_verificacao_eficacia", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_qld_acr_analise_tenant_id_codigo",
                schema: "qualidade",
                table: "qld_acr_analise",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_qld_acr_analise_tenant_id_status",
                schema: "qualidade",
                table: "qld_acr_analise",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__acr_analise_sync_id",
                schema: "qualidade",
                table: "qld_acr_analise",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__acr_analise_tenant_id",
                schema: "qualidade",
                table: "qld_acr_analise",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_acr_anexo_tenant_id_analise_id",
                schema: "qualidade",
                table: "qld_acr_anexo",
                columns: new[] { "tenant_id", "analise_id" });

            migrationBuilder.CreateIndex(
                name: "ix__acr_anexo_sync_id",
                schema: "qualidade",
                table: "qld_acr_anexo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__acr_anexo_tenant_id",
                schema: "qualidade",
                table: "qld_acr_anexo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_acr_documento_fiscal_tenant_id_chave_fiscal_referencia",
                schema: "qualidade",
                table: "qld_acr_documento_fiscal",
                columns: new[] { "tenant_id", "chave_fiscal_referencia" });

            migrationBuilder.CreateIndex(
                name: "ix__acr_documento_fiscal_sync_id",
                schema: "qualidade",
                table: "qld_acr_documento_fiscal",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__acr_documento_fiscal_tenant_id",
                schema: "qualidade",
                table: "qld_acr_documento_fiscal",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_acr_evento_estoque_tenant_id_resultado_id",
                schema: "qualidade",
                table: "qld_acr_evento_estoque",
                columns: new[] { "tenant_id", "resultado_id" });

            migrationBuilder.CreateIndex(
                name: "ix__acr_evento_estoque_sync_id",
                schema: "qualidade",
                table: "qld_acr_evento_estoque",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__acr_evento_estoque_tenant_id",
                schema: "qualidade",
                table: "qld_acr_evento_estoque",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_acr_evento_ncr_tenant_id_resultado_id",
                schema: "qualidade",
                table: "qld_acr_evento_ncr",
                columns: new[] { "tenant_id", "resultado_id" });

            migrationBuilder.CreateIndex(
                name: "ix__acr_evento_ncr_sync_id",
                schema: "qualidade",
                table: "qld_acr_evento_ncr",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__acr_evento_ncr_tenant_id",
                schema: "qualidade",
                table: "qld_acr_evento_ncr",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_acr_historico_tenant_id_analise_id",
                schema: "qualidade",
                table: "qld_acr_historico",
                columns: new[] { "tenant_id", "analise_id" });

            migrationBuilder.CreateIndex(
                name: "ix__acr_historico_sync_id",
                schema: "qualidade",
                table: "qld_acr_historico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__acr_historico_tenant_id",
                schema: "qualidade",
                table: "qld_acr_historico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_acr_item_tenant_id_analise_id",
                schema: "qualidade",
                table: "qld_acr_item",
                columns: new[] { "tenant_id", "analise_id" });

            migrationBuilder.CreateIndex(
                name: "ix__acr_item_sync_id",
                schema: "qualidade",
                table: "qld_acr_item",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__acr_item_tenant_id",
                schema: "qualidade",
                table: "qld_acr_item",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_acr_motivo_tenant_id_codigo",
                schema: "qualidade",
                table: "qld_acr_motivo",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__acr_motivo_sync_id",
                schema: "qualidade",
                table: "qld_acr_motivo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__acr_motivo_tenant_id",
                schema: "qualidade",
                table: "qld_acr_motivo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_acr_parametro_tenant_id_chave",
                schema: "qualidade",
                table: "qld_acr_parametro",
                columns: new[] { "tenant_id", "chave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__acr_parametro_sync_id",
                schema: "qualidade",
                table: "qld_acr_parametro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__acr_parametro_tenant_id",
                schema: "qualidade",
                table: "qld_acr_parametro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_acr_resultado_tenant_id_analise_id",
                schema: "qualidade",
                table: "qld_acr_resultado",
                columns: new[] { "tenant_id", "analise_id" });

            migrationBuilder.CreateIndex(
                name: "ix__acr_resultado_sync_id",
                schema: "qualidade",
                table: "qld_acr_resultado",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__acr_resultado_tenant_id",
                schema: "qualidade",
                table: "qld_acr_resultado",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_adm_anexo_tenant_id_qualidade_id",
                schema: "qualidade",
                table: "qld_adm_anexo",
                columns: new[] { "tenant_id", "qualidade_id" });

            migrationBuilder.CreateIndex(
                name: "ix__adm_anexo_sync_id",
                schema: "qualidade",
                table: "qld_adm_anexo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__adm_anexo_tenant_id",
                schema: "qualidade",
                table: "qld_adm_anexo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_adm_documento_qms_tenant_id_codigo_versao_documento",
                schema: "qualidade",
                table: "qld_adm_documento_qms",
                columns: new[] { "tenant_id", "codigo", "versao_documento" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_qld_adm_documento_qms_tenant_id_qualidade_id",
                schema: "qualidade",
                table: "qld_adm_documento_qms",
                columns: new[] { "tenant_id", "qualidade_id" });

            migrationBuilder.CreateIndex(
                name: "ix__adm_documento_qms_sync_id",
                schema: "qualidade",
                table: "qld_adm_documento_qms",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__adm_documento_qms_tenant_id",
                schema: "qualidade",
                table: "qld_adm_documento_qms",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_adm_historico_tenant_id_qualidade_id",
                schema: "qualidade",
                table: "qld_adm_historico",
                columns: new[] { "tenant_id", "qualidade_id" });

            migrationBuilder.CreateIndex(
                name: "ix__adm_historico_sync_id",
                schema: "qualidade",
                table: "qld_adm_historico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__adm_historico_tenant_id",
                schema: "qualidade",
                table: "qld_adm_historico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_adm_item_tenant_id_qualidade_id",
                schema: "qualidade",
                table: "qld_adm_item",
                columns: new[] { "tenant_id", "qualidade_id" });

            migrationBuilder.CreateIndex(
                name: "ix__adm_item_sync_id",
                schema: "qualidade",
                table: "qld_adm_item",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__adm_item_tenant_id",
                schema: "qualidade",
                table: "qld_adm_item",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_adm_kpi_tenant_id_objetivo_id",
                schema: "qualidade",
                table: "qld_adm_kpi",
                columns: new[] { "tenant_id", "objetivo_id" });

            migrationBuilder.CreateIndex(
                name: "ix__adm_kpi_sync_id",
                schema: "qualidade",
                table: "qld_adm_kpi",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__adm_kpi_tenant_id",
                schema: "qualidade",
                table: "qld_adm_kpi",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_adm_objetivo_tenant_id_qualidade_id",
                schema: "qualidade",
                table: "qld_adm_objetivo",
                columns: new[] { "tenant_id", "qualidade_id" });

            migrationBuilder.CreateIndex(
                name: "ix__adm_objetivo_sync_id",
                schema: "qualidade",
                table: "qld_adm_objetivo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__adm_objetivo_tenant_id",
                schema: "qualidade",
                table: "qld_adm_objetivo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_adm_parametro_tenant_id_chave",
                schema: "qualidade",
                table: "qld_adm_parametro",
                columns: new[] { "tenant_id", "chave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__adm_parametro_sync_id",
                schema: "qualidade",
                table: "qld_adm_parametro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__adm_parametro_tenant_id",
                schema: "qualidade",
                table: "qld_adm_parametro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_adm_programa_auditoria_tenant_id_qualidade_id",
                schema: "qualidade",
                table: "qld_adm_programa_auditoria",
                columns: new[] { "tenant_id", "qualidade_id" });

            migrationBuilder.CreateIndex(
                name: "ix__adm_programa_auditoria_sync_id",
                schema: "qualidade",
                table: "qld_adm_programa_auditoria",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__adm_programa_auditoria_tenant_id",
                schema: "qualidade",
                table: "qld_adm_programa_auditoria",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_adm_qualidade_tenant_id_codigo",
                schema: "qualidade",
                table: "qld_adm_qualidade",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_qld_adm_qualidade_tenant_id_status",
                schema: "qualidade",
                table: "qld_adm_qualidade",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__adm_qualidade_sync_id",
                schema: "qualidade",
                table: "qld_adm_qualidade",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__adm_qualidade_tenant_id",
                schema: "qualidade",
                table: "qld_adm_qualidade",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_atr_anexo_tenant_id_atributo_id",
                schema: "qualidade",
                table: "qld_atr_anexo",
                columns: new[] { "tenant_id", "atributo_id" });

            migrationBuilder.CreateIndex(
                name: "ix__atr_anexo_sync_id",
                schema: "qualidade",
                table: "qld_atr_anexo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__atr_anexo_tenant_id",
                schema: "qualidade",
                table: "qld_atr_anexo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_atr_atributo_tenant_id_codigo",
                schema: "qualidade",
                table: "qld_atr_atributo",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_qld_atr_atributo_tenant_id_nome_interno_escopo",
                schema: "qualidade",
                table: "qld_atr_atributo",
                columns: new[] { "tenant_id", "nome_interno", "escopo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_qld_atr_atributo_tenant_id_status",
                schema: "qualidade",
                table: "qld_atr_atributo",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__atr_atributo_sync_id",
                schema: "qualidade",
                table: "qld_atr_atributo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__atr_atributo_tenant_id",
                schema: "qualidade",
                table: "qld_atr_atributo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_atr_especificacao_tenant_id_atributo_id",
                schema: "qualidade",
                table: "qld_atr_especificacao",
                columns: new[] { "tenant_id", "atributo_id" });

            migrationBuilder.CreateIndex(
                name: "ix__atr_especificacao_sync_id",
                schema: "qualidade",
                table: "qld_atr_especificacao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__atr_especificacao_tenant_id",
                schema: "qualidade",
                table: "qld_atr_especificacao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_atr_historico_tenant_id_atributo_id",
                schema: "qualidade",
                table: "qld_atr_historico",
                columns: new[] { "tenant_id", "atributo_id" });

            migrationBuilder.CreateIndex(
                name: "ix__atr_historico_sync_id",
                schema: "qualidade",
                table: "qld_atr_historico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__atr_historico_tenant_id",
                schema: "qualidade",
                table: "qld_atr_historico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_atr_opcao_tenant_id_atributo_id_codigo",
                schema: "qualidade",
                table: "qld_atr_opcao",
                columns: new[] { "tenant_id", "atributo_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__atr_opcao_sync_id",
                schema: "qualidade",
                table: "qld_atr_opcao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__atr_opcao_tenant_id",
                schema: "qualidade",
                table: "qld_atr_opcao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_atr_parametro_tenant_id_chave",
                schema: "qualidade",
                table: "qld_atr_parametro",
                columns: new[] { "tenant_id", "chave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__atr_parametro_sync_id",
                schema: "qualidade",
                table: "qld_atr_parametro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__atr_parametro_tenant_id",
                schema: "qualidade",
                table: "qld_atr_parametro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_atr_valor_tenant_id_atributo_id",
                schema: "qualidade",
                table: "qld_atr_valor",
                columns: new[] { "tenant_id", "atributo_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_qld_atr_valor_tenant_id_contexto_tipo_contexto_id",
                schema: "qualidade",
                table: "qld_atr_valor",
                columns: new[] { "tenant_id", "contexto_tipo", "contexto_id" });

            migrationBuilder.CreateIndex(
                name: "ix__atr_valor_sync_id",
                schema: "qualidade",
                table: "qld_atr_valor",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__atr_valor_tenant_id",
                schema: "qualidade",
                table: "qld_atr_valor",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_atr_vinculo_contexto_tenant_id_atributo_id",
                schema: "qualidade",
                table: "qld_atr_vinculo_contexto",
                columns: new[] { "tenant_id", "atributo_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_qld_atr_vinculo_contexto_tenant_id_contexto_tipo_contexto_id",
                schema: "qualidade",
                table: "qld_atr_vinculo_contexto",
                columns: new[] { "tenant_id", "contexto_tipo", "contexto_id" });

            migrationBuilder.CreateIndex(
                name: "ix__atr_vinculo_contexto_sync_id",
                schema: "qualidade",
                table: "qld_atr_vinculo_contexto",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__atr_vinculo_contexto_tenant_id",
                schema: "qualidade",
                table: "qld_atr_vinculo_contexto",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_ins_amostra_tenant_id_execucao_id_sequencia",
                schema: "qualidade",
                table: "qld_ins_amostra",
                columns: new[] { "tenant_id", "execucao_id", "sequencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__amostra_inspecionada_sync_id",
                schema: "qualidade",
                table: "qld_ins_amostra",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__amostra_inspecionada_tenant_id",
                schema: "qualidade",
                table: "qld_ins_amostra",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_ins_caracteristica_tenant_id_plano_id_sequencia",
                schema: "qualidade",
                table: "qld_ins_caracteristica",
                columns: new[] { "tenant_id", "plano_id", "sequencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__caracteristica_plano_sync_id",
                schema: "qualidade",
                table: "qld_ins_caracteristica",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__caracteristica_plano_tenant_id",
                schema: "qualidade",
                table: "qld_ins_caracteristica",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_ins_execucao_tenant_id_plano_id",
                schema: "qualidade",
                table: "qld_ins_execucao",
                columns: new[] { "tenant_id", "plano_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_qld_ins_execucao_tenant_id_status",
                schema: "qualidade",
                table: "qld_ins_execucao",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__execucao_inspecao_sync_id",
                schema: "qualidade",
                table: "qld_ins_execucao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__execucao_inspecao_tenant_id",
                schema: "qualidade",
                table: "qld_ins_execucao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_ins_medicao_tenant_id_execucao_id",
                schema: "qualidade",
                table: "qld_ins_medicao",
                columns: new[] { "tenant_id", "execucao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__medicao_sync_id",
                schema: "qualidade",
                table: "qld_ins_medicao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__medicao_tenant_id",
                schema: "qualidade",
                table: "qld_ins_medicao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_ins_plano_tenant_id_codigo",
                schema: "qualidade",
                table: "qld_ins_plano",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_qld_ins_plano_tenant_id_status",
                schema: "qualidade",
                table: "qld_ins_plano",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__plano_inspecao_sync_id",
                schema: "qualidade",
                table: "qld_ins_plano",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__plano_inspecao_tenant_id",
                schema: "qualidade",
                table: "qld_ins_plano",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_ins_regra_amostragem_tenant_id_plano_id",
                schema: "qualidade",
                table: "qld_ins_regra_amostragem",
                columns: new[] { "tenant_id", "plano_id" });

            migrationBuilder.CreateIndex(
                name: "ix__regra_amostragem_sync_id",
                schema: "qualidade",
                table: "qld_ins_regra_amostragem",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__regra_amostragem_tenant_id",
                schema: "qualidade",
                table: "qld_ins_regra_amostragem",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_ins_resultado_tenant_id_execucao_id",
                schema: "qualidade",
                table: "qld_ins_resultado",
                columns: new[] { "tenant_id", "execucao_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__resultado_inspecao_sync_id",
                schema: "qualidade",
                table: "qld_ins_resultado",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__resultado_inspecao_tenant_id",
                schema: "qualidade",
                table: "qld_ins_resultado",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_ncr_acao_capa_tenant_id_ncr_id",
                schema: "qualidade",
                table: "qld_ncr_acao_capa",
                columns: new[] { "tenant_id", "ncr_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_qld_ncr_acao_capa_tenant_id_status",
                schema: "qualidade",
                table: "qld_ncr_acao_capa",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__ncr_acao_capa_sync_id",
                schema: "qualidade",
                table: "qld_ncr_acao_capa",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ncr_acao_capa_tenant_id",
                schema: "qualidade",
                table: "qld_ncr_acao_capa",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_ncr_anexo_tenant_id_ncr_id",
                schema: "qualidade",
                table: "qld_ncr_anexo",
                columns: new[] { "tenant_id", "ncr_id" });

            migrationBuilder.CreateIndex(
                name: "ix__ncr_anexo_sync_id",
                schema: "qualidade",
                table: "qld_ncr_anexo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ncr_anexo_tenant_id",
                schema: "qualidade",
                table: "qld_ncr_anexo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_ncr_causa_raiz_tenant_id_ncr_id",
                schema: "qualidade",
                table: "qld_ncr_causa_raiz",
                columns: new[] { "tenant_id", "ncr_id" });

            migrationBuilder.CreateIndex(
                name: "ix__ncr_causa_raiz_sync_id",
                schema: "qualidade",
                table: "qld_ncr_causa_raiz",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ncr_causa_raiz_tenant_id",
                schema: "qualidade",
                table: "qld_ncr_causa_raiz",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_ncr_evento_integracao_tenant_id_ncr_id",
                schema: "qualidade",
                table: "qld_ncr_evento_integracao",
                columns: new[] { "tenant_id", "ncr_id" });

            migrationBuilder.CreateIndex(
                name: "ix__ncr_evento_integracao_sync_id",
                schema: "qualidade",
                table: "qld_ncr_evento_integracao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ncr_evento_integracao_tenant_id",
                schema: "qualidade",
                table: "qld_ncr_evento_integracao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_ncr_historico_tenant_id_ncr_id",
                schema: "qualidade",
                table: "qld_ncr_historico",
                columns: new[] { "tenant_id", "ncr_id" });

            migrationBuilder.CreateIndex(
                name: "ix__ncr_historico_sync_id",
                schema: "qualidade",
                table: "qld_ncr_historico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ncr_historico_tenant_id",
                schema: "qualidade",
                table: "qld_ncr_historico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_ncr_origem_ref_tenant_id_ncr_id",
                schema: "qualidade",
                table: "qld_ncr_origem_ref",
                columns: new[] { "tenant_id", "ncr_id" });

            migrationBuilder.CreateIndex(
                name: "ix__ncr_origem_ref_sync_id",
                schema: "qualidade",
                table: "qld_ncr_origem_ref",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ncr_origem_ref_tenant_id",
                schema: "qualidade",
                table: "qld_ncr_origem_ref",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_ncr_parametro_tenant_id_chave",
                schema: "qualidade",
                table: "qld_ncr_parametro",
                columns: new[] { "tenant_id", "chave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ncr_parametro_sync_id",
                schema: "qualidade",
                table: "qld_ncr_parametro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ncr_parametro_tenant_id",
                schema: "qualidade",
                table: "qld_ncr_parametro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_ncr_registro_tenant_id_codigo",
                schema: "qualidade",
                table: "qld_ncr_registro",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_qld_ncr_registro_tenant_id_origem_principal_prioridade",
                schema: "qualidade",
                table: "qld_ncr_registro",
                columns: new[] { "tenant_id", "origem_principal", "prioridade" });

            migrationBuilder.CreateIndex(
                name: "i_x_qld_ncr_registro_tenant_id_status_registro_etapa_ncr",
                schema: "qualidade",
                table: "qld_ncr_registro",
                columns: new[] { "tenant_id", "status_registro", "etapa_ncr" });

            migrationBuilder.CreateIndex(
                name: "ix__ncr_registro_sync_id",
                schema: "qualidade",
                table: "qld_ncr_registro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ncr_registro_tenant_id",
                schema: "qualidade",
                table: "qld_ncr_registro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_ncr_verificacao_eficacia_tenant_id_ncr_id",
                schema: "qualidade",
                table: "qld_ncr_verificacao_eficacia",
                columns: new[] { "tenant_id", "ncr_id" });

            migrationBuilder.CreateIndex(
                name: "ix__ncr_verificacao_eficacia_sync_id",
                schema: "qualidade",
                table: "qld_ncr_verificacao_eficacia",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ncr_verificacao_eficacia_tenant_id",
                schema: "qualidade",
                table: "qld_ncr_verificacao_eficacia",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "qld_acr_analise",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_acr_anexo",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_acr_documento_fiscal",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_acr_evento_estoque",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_acr_evento_ncr",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_acr_historico",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_acr_item",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_acr_motivo",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_acr_parametro",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_acr_resultado",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_adm_anexo",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_adm_documento_qms",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_adm_historico",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_adm_item",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_adm_kpi",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_adm_objetivo",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_adm_parametro",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_adm_programa_auditoria",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_adm_qualidade",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_atr_anexo",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_atr_atributo",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_atr_especificacao",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_atr_historico",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_atr_opcao",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_atr_parametro",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_atr_valor",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_atr_vinculo_contexto",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_ins_amostra",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_ins_caracteristica",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_ins_execucao",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_ins_medicao",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_ins_plano",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_ins_regra_amostragem",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_ins_resultado",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_ncr_acao_capa",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_ncr_anexo",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_ncr_causa_raiz",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_ncr_evento_integracao",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_ncr_historico",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_ncr_origem_ref",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_ncr_parametro",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_ncr_registro",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_ncr_verificacao_eficacia",
                schema: "qualidade");
        }
    }
}
