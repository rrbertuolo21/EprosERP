using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Aplicativo.Migrations
{
    /// <inheritdoc />
    public partial class Plataforma_Submodulos_SpecOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "plt_analytics_dashboards",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    layout_json = table.Column<string>(type: "text", nullable: true),
                    publico = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
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
                    table.PrimaryKey("p_k_dashboards", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plt_analytics_exportacoes",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    recurso = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    formato = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    filtro_json = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    url_ref = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
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
                    table.PrimaryKey("p_k_exportacoes_analytics", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plt_analytics_kpis",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    unidade = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    categoria = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    fonte_modulo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    chave_consulta = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    versao = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
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
                    table.PrimaryKey("p_k_definicoes_kpi", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plt_analytics_snapshots",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kpi_codigo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    competencia = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: false),
                    dimensao = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    capturado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origem = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
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
                    table.PrimaryKey("p_k_snapshots_metrica", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plt_analytics_widgets",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    dashboard_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kpi_codigo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tipo_visualizacao = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    config_json = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
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
                    table.PrimaryKey("p_k_dashboard_widgets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plt_assinatura_evidencias",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    registro_id = table.Column<Guid>(type: "uuid", nullable: false),
                    hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    certificado_serial = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    cadeia_icp = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    valor_evidencia = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
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
                    table.PrimaryKey("p_k_evidencias_assinatura", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plt_assinatura_historicos",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    solicitacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    acao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    detalhe = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
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
                    table.PrimaryKey("p_k_historicos_assinatura", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plt_assinatura_politicas",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_documento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    min_assinaturas = table.Column<int>(type: "integer", nullable: false),
                    tipo_assinatura = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    exige_ordem = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
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
                    table.PrimaryKey("p_k_politicas_assinatura", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plt_assinatura_registros",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    solicitacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    signatario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assinado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    carimbo_tempo = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
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
                    table.PrimaryKey("p_k_registros_assinatura", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plt_assinatura_signatarios",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    solicitacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    obrigatorio = table.Column<bool>(type: "boolean", nullable: false),
                    externo = table.Column<bool>(type: "boolean", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    identificacao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    link_token = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    revogado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
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
                    table.PrimaryKey("p_k_signatarios_assinatura", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plt_assinatura_solicitacoes",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    documento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    versao_documento = table.Column<int>(type: "integer", nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    link_publico = table.Column<bool>(type: "boolean", nullable: false),
                    min_assinaturas = table.Column<int>(type: "integer", nullable: false),
                    exige_ordem = table.Column<bool>(type: "boolean", nullable: false),
                    tipo_assinatura = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
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
                    table.PrimaryKey("p_k_solicitacoes_assinatura", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plt_conector_endpoints",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    eventos_inscritos_json = table.Column<string>(type: "text", nullable: false),
                    segredo_cifrado = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    headers_json = table.Column<string>(type: "text", nullable: true),
                    max_tentativas = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
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
                    table.PrimaryKey("p_k_endpoints_webhook", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plt_conector_entregas",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    endpoint_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    payload = table.Column<string>(type: "text", nullable: false),
                    assinatura_hmac = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tentativas = table.Column<int>(type: "integer", nullable: false),
                    proxima_tentativa_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    codigo_resposta_http = table.Column<int>(type: "integer", nullable: true),
                    ultimo_erro = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
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
                    table.PrimaryKey("p_k_entregas_webhook", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plt_ged_hashes_bloqueados",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
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
                    table.PrimaryKey("p_k_hashes_bloqueados_ged", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plt_ged_politicas_retencao",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_documento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    prazo_retencao_dias = table.Column<int>(type: "integer", nullable: false),
                    base_legal = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    acao_apos_prazo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    hold_juridico = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
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
                    table.PrimaryKey("p_k_politicas_retencao_ged", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plt_ged_vinculos",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    documento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    modulo_destino = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entidade_tipo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    entidade_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
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
                    table.PrimaryKey("p_k_vinculos_documento_ged", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plt_iot_dispositivos",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tipo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    protocolo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ativo_vinculado_tipo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ativo_vinculado_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    ultima_leitura_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
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
                    table.PrimaryKey("p_k_dispositivos_iot", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plt_iot_leituras",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sensor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(20,6)", precision: 20, scale: 6, nullable: false),
                    medido_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fora_faixa = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
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
                    table.PrimaryKey("p_k_leituras_sensor", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plt_iot_sensores",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    dispositivo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    grandeza = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    unidade = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    limite_min = table.Column<decimal>(type: "numeric(20,6)", precision: 20, scale: 6, nullable: true),
                    limite_max = table.Column<decimal>(type: "numeric(20,6)", precision: 20, scale: 6, nullable: true),
                    retencao_dias = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
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
                    table.PrimaryKey("p_k_sensores_iot", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plt_sdk_chaves_api",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    prefixo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    hash_chave = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    escopos_json = table.Column<string>(type: "text", nullable: false),
                    ativa = table.Column<bool>(type: "boolean", nullable: false),
                    expira_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ultimo_uso_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revogada_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
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
                    table.PrimaryKey("p_k_chaves_api", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plt_wizard_campos",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    etapa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    chave = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    rotulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    obrigatorio = table.Column<bool>(type: "boolean", nullable: false),
                    opcoes_json = table.Column<string>(type: "text", nullable: true),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
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
                    table.PrimaryKey("p_k_campos_wizard", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plt_wizard_definicoes",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    publico = table.Column<bool>(type: "boolean", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
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
                    table.PrimaryKey("p_k_definicoes_wizard", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plt_wizard_etapas",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    definicao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
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
                    table.PrimaryKey("p_k_etapas_wizard", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plt_wizard_execucoes",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    definicao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    etapa_atual_ordem = table.Column<int>(type: "integer", nullable: false),
                    respostas_json = table.Column<string>(type: "text", nullable: false),
                    canal_publico = table.Column<bool>(type: "boolean", nullable: false),
                    token_publico = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
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
                    table.PrimaryKey("p_k_execucoes_wizard", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix__dashboard_sync_id",
                schema: "aplicativo",
                table: "plt_analytics_dashboards",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__dashboard_tenant_id",
                schema: "aplicativo",
                table: "plt_analytics_dashboards",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_plt_analytics_dashboards_tenant_codigo",
                schema: "aplicativo",
                table: "plt_analytics_dashboards",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__exportacao_analytics_sync_id",
                schema: "aplicativo",
                table: "plt_analytics_exportacoes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__exportacao_analytics_tenant_id",
                schema: "aplicativo",
                table: "plt_analytics_exportacoes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_plt_analytics_exportacoes_status",
                schema: "aplicativo",
                table: "plt_analytics_exportacoes",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix__definicao_kpi_sync_id",
                schema: "aplicativo",
                table: "plt_analytics_kpis",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__definicao_kpi_tenant_id",
                schema: "aplicativo",
                table: "plt_analytics_kpis",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_plt_analytics_kpis_tenant_codigo_versao",
                schema: "aplicativo",
                table: "plt_analytics_kpis",
                columns: new[] { "tenant_id", "codigo", "versao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__snapshot_metrica_sync_id",
                schema: "aplicativo",
                table: "plt_analytics_snapshots",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__snapshot_metrica_tenant_id",
                schema: "aplicativo",
                table: "plt_analytics_snapshots",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_plt_analytics_snapshots_kpi_competencia_dimensao",
                schema: "aplicativo",
                table: "plt_analytics_snapshots",
                columns: new[] { "kpi_codigo", "competencia", "dimensao" });

            migrationBuilder.CreateIndex(
                name: "ix__dashboard_widget_sync_id",
                schema: "aplicativo",
                table: "plt_analytics_widgets",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__dashboard_widget_tenant_id",
                schema: "aplicativo",
                table: "plt_analytics_widgets",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_plt_analytics_widgets_dashboard",
                schema: "aplicativo",
                table: "plt_analytics_widgets",
                column: "dashboard_id");

            migrationBuilder.CreateIndex(
                name: "ix__evidencia_assinatura_sync_id",
                schema: "aplicativo",
                table: "plt_assinatura_evidencias",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__evidencia_assinatura_tenant_id",
                schema: "aplicativo",
                table: "plt_assinatura_evidencias",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_plt_assinatura_evidencias_registro",
                schema: "aplicativo",
                table: "plt_assinatura_evidencias",
                column: "registro_id");

            migrationBuilder.CreateIndex(
                name: "ix__historico_assinatura_sync_id",
                schema: "aplicativo",
                table: "plt_assinatura_historicos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__historico_assinatura_tenant_id",
                schema: "aplicativo",
                table: "plt_assinatura_historicos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_plt_assinatura_historicos_solicitacao",
                schema: "aplicativo",
                table: "plt_assinatura_historicos",
                column: "solicitacao_id");

            migrationBuilder.CreateIndex(
                name: "ix__politica_assinatura_sync_id",
                schema: "aplicativo",
                table: "plt_assinatura_politicas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__politica_assinatura_tenant_id",
                schema: "aplicativo",
                table: "plt_assinatura_politicas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_plt_assinatura_politicas_tenant_tipo",
                schema: "aplicativo",
                table: "plt_assinatura_politicas",
                columns: new[] { "tenant_id", "tipo_documento" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__registro_assinatura_sync_id",
                schema: "aplicativo",
                table: "plt_assinatura_registros",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__registro_assinatura_tenant_id",
                schema: "aplicativo",
                table: "plt_assinatura_registros",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_plt_assinatura_registros_solicitacao",
                schema: "aplicativo",
                table: "plt_assinatura_registros",
                column: "solicitacao_id");

            migrationBuilder.CreateIndex(
                name: "ix__signatario_assinatura_sync_id",
                schema: "aplicativo",
                table: "plt_assinatura_signatarios",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__signatario_assinatura_tenant_id",
                schema: "aplicativo",
                table: "plt_assinatura_signatarios",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_plt_assinatura_signatarios_link_token",
                schema: "aplicativo",
                table: "plt_assinatura_signatarios",
                column: "link_token",
                unique: true,
                filter: "link_token IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_plt_assinatura_signatarios_solicitacao",
                schema: "aplicativo",
                table: "plt_assinatura_signatarios",
                column: "solicitacao_id");

            migrationBuilder.CreateIndex(
                name: "ix__solicitacao_assinatura_sync_id",
                schema: "aplicativo",
                table: "plt_assinatura_solicitacoes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__solicitacao_assinatura_tenant_id",
                schema: "aplicativo",
                table: "plt_assinatura_solicitacoes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_plt_assinatura_solicitacoes_documento",
                schema: "aplicativo",
                table: "plt_assinatura_solicitacoes",
                column: "documento_id");

            migrationBuilder.CreateIndex(
                name: "ix_plt_assinatura_solicitacoes_estado",
                schema: "aplicativo",
                table: "plt_assinatura_solicitacoes",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "ix__endpoint_webhook_sync_id",
                schema: "aplicativo",
                table: "plt_conector_endpoints",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__endpoint_webhook_tenant_id",
                schema: "aplicativo",
                table: "plt_conector_endpoints",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_plt_conector_endpoints_ativo",
                schema: "aplicativo",
                table: "plt_conector_endpoints",
                column: "ativo");

            migrationBuilder.CreateIndex(
                name: "ix__entrega_webhook_sync_id",
                schema: "aplicativo",
                table: "plt_conector_entregas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__entrega_webhook_tenant_id",
                schema: "aplicativo",
                table: "plt_conector_entregas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_plt_conector_entregas_endpoint",
                schema: "aplicativo",
                table: "plt_conector_entregas",
                column: "endpoint_id");

            migrationBuilder.CreateIndex(
                name: "ix_plt_conector_entregas_status_proxima",
                schema: "aplicativo",
                table: "plt_conector_entregas",
                columns: new[] { "status", "proxima_tentativa_em" });

            migrationBuilder.CreateIndex(
                name: "ix__hash_bloqueado_ged_sync_id",
                schema: "aplicativo",
                table: "plt_ged_hashes_bloqueados",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__hash_bloqueado_ged_tenant_id",
                schema: "aplicativo",
                table: "plt_ged_hashes_bloqueados",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_plt_ged_hashes_bloqueados_tenant_hash",
                schema: "aplicativo",
                table: "plt_ged_hashes_bloqueados",
                columns: new[] { "tenant_id", "hash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__politica_retencao_ged_sync_id",
                schema: "aplicativo",
                table: "plt_ged_politicas_retencao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__politica_retencao_ged_tenant_id",
                schema: "aplicativo",
                table: "plt_ged_politicas_retencao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_plt_ged_politicas_retencao_tenant_tipo",
                schema: "aplicativo",
                table: "plt_ged_politicas_retencao",
                columns: new[] { "tenant_id", "tipo_documento" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__vinculo_documento_ged_sync_id",
                schema: "aplicativo",
                table: "plt_ged_vinculos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__vinculo_documento_ged_tenant_id",
                schema: "aplicativo",
                table: "plt_ged_vinculos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_plt_ged_vinculos_documento_entidade",
                schema: "aplicativo",
                table: "plt_ged_vinculos",
                columns: new[] { "documento_id", "entidade_tipo", "entidade_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_plt_ged_vinculos_entidade",
                schema: "aplicativo",
                table: "plt_ged_vinculos",
                columns: new[] { "entidade_tipo", "entidade_id" });

            migrationBuilder.CreateIndex(
                name: "ix__dispositivo_iot_sync_id",
                schema: "aplicativo",
                table: "plt_iot_dispositivos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__dispositivo_iot_tenant_id",
                schema: "aplicativo",
                table: "plt_iot_dispositivos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_plt_iot_dispositivos_ativo",
                schema: "aplicativo",
                table: "plt_iot_dispositivos",
                columns: new[] { "ativo_vinculado_tipo", "ativo_vinculado_id" });

            migrationBuilder.CreateIndex(
                name: "ix_plt_iot_dispositivos_tenant_codigo",
                schema: "aplicativo",
                table: "plt_iot_dispositivos",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__leitura_sensor_sync_id",
                schema: "aplicativo",
                table: "plt_iot_leituras",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__leitura_sensor_tenant_id",
                schema: "aplicativo",
                table: "plt_iot_leituras",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_plt_iot_leituras_fora_faixa",
                schema: "aplicativo",
                table: "plt_iot_leituras",
                column: "fora_faixa");

            migrationBuilder.CreateIndex(
                name: "ix_plt_iot_leituras_sensor_medido",
                schema: "aplicativo",
                table: "plt_iot_leituras",
                columns: new[] { "sensor_id", "medido_em" });

            migrationBuilder.CreateIndex(
                name: "ix__sensor_iot_sync_id",
                schema: "aplicativo",
                table: "plt_iot_sensores",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sensor_iot_tenant_id",
                schema: "aplicativo",
                table: "plt_iot_sensores",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_plt_iot_sensores_dispositivo_codigo",
                schema: "aplicativo",
                table: "plt_iot_sensores",
                columns: new[] { "dispositivo_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__chave_api_publica_sync_id",
                schema: "aplicativo",
                table: "plt_sdk_chaves_api",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__chave_api_publica_tenant_id",
                schema: "aplicativo",
                table: "plt_sdk_chaves_api",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_plt_sdk_chaves_api_hash",
                schema: "aplicativo",
                table: "plt_sdk_chaves_api",
                column: "hash_chave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_plt_sdk_chaves_api_prefixo",
                schema: "aplicativo",
                table: "plt_sdk_chaves_api",
                column: "prefixo");

            migrationBuilder.CreateIndex(
                name: "ix__campo_wizard_sync_id",
                schema: "aplicativo",
                table: "plt_wizard_campos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__campo_wizard_tenant_id",
                schema: "aplicativo",
                table: "plt_wizard_campos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_plt_wizard_campos_etapa_chave",
                schema: "aplicativo",
                table: "plt_wizard_campos",
                columns: new[] { "etapa_id", "chave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__definicao_wizard_sync_id",
                schema: "aplicativo",
                table: "plt_wizard_definicoes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__definicao_wizard_tenant_id",
                schema: "aplicativo",
                table: "plt_wizard_definicoes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_plt_wizard_definicoes_tenant_codigo",
                schema: "aplicativo",
                table: "plt_wizard_definicoes",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__etapa_wizard_sync_id",
                schema: "aplicativo",
                table: "plt_wizard_etapas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__etapa_wizard_tenant_id",
                schema: "aplicativo",
                table: "plt_wizard_etapas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_plt_wizard_etapas_definicao_ordem",
                schema: "aplicativo",
                table: "plt_wizard_etapas",
                columns: new[] { "definicao_id", "ordem" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__execucao_wizard_sync_id",
                schema: "aplicativo",
                table: "plt_wizard_execucoes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__execucao_wizard_tenant_id",
                schema: "aplicativo",
                table: "plt_wizard_execucoes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_plt_wizard_execucoes_definicao",
                schema: "aplicativo",
                table: "plt_wizard_execucoes",
                column: "definicao_id");

            migrationBuilder.CreateIndex(
                name: "ix_plt_wizard_execucoes_token",
                schema: "aplicativo",
                table: "plt_wizard_execucoes",
                column: "token_publico",
                unique: true,
                filter: "token_publico IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "plt_analytics_dashboards",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "plt_analytics_exportacoes",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "plt_analytics_kpis",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "plt_analytics_snapshots",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "plt_analytics_widgets",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "plt_assinatura_evidencias",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "plt_assinatura_historicos",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "plt_assinatura_politicas",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "plt_assinatura_registros",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "plt_assinatura_signatarios",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "plt_assinatura_solicitacoes",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "plt_conector_endpoints",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "plt_conector_entregas",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "plt_ged_hashes_bloqueados",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "plt_ged_politicas_retencao",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "plt_ged_vinculos",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "plt_iot_dispositivos",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "plt_iot_leituras",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "plt_iot_sensores",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "plt_sdk_chaves_api",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "plt_wizard_campos",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "plt_wizard_definicoes",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "plt_wizard_etapas",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "plt_wizard_execucoes",
                schema: "aplicativo");
        }
    }
}
