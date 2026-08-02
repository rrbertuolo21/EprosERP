using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Aplicativo.Migrations
{
    /// <inheritdoc />
    public partial class Implanta_Transversais_Compartilhadas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "wf_transicoes",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "wf_tarefas",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "wf_solicitacoes",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "wf_parametros",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "wf_jobs",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "wf_job_tentativas",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "wf_instancias",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "wf_historicos",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "wf_eventos_dominio",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "wf_estados",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "wf_definicoes",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "wf_anexos",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "wf_agendamentos",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "usuarios_internos",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "usuarios_empresas",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "usuarios",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_upload_partes",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_migracoes_offline",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_mapeamentos_importacao",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_importacoes_xml",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_importacao_linhas",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_importacao_erros",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_historicos",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_filas_url_remota",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_exportacao_campos",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_execucoes_upload",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_execucoes_importacao",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_execucoes_exportacao",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_configuracoes",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_atualizacoes_versao",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_atualizacoes_job",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_atualizacoes_bloco",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_arquivos_xml_saida",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_arquivos",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "update_logs",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "system_settings",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "solicitacoes_upgrade_versao",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "sessoes_usuarios",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "sessoes_impersonacao",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "preferencias_usuarios",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "personal_access_tokens",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "newsletter_subscribers",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "marketplaces_settings",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "logs_execucao_massa",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "landing_pages_settings",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "installation_state",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "idiomas",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "identidades_externas",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "historicos_login",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "execucoes_massa_global",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "custom_pages",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "configuracoes_empresas",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "comunicacoes_super_admin",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "banned_ips",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "anos_financeiros",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "aplicativo",
                table: "acessos_usuario_tenant",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateTable(
                name: "documentos_ged",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    tipo_documento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    tamanho_bytes = table.Column<long>(type: "bigint", nullable: false),
                    mime_type = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    versao = table.Column<int>(type: "integer", nullable: false),
                    storage_ref = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    modulo_origem = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    entidade_origem_tipo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    entidade_origem_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status_assinatura = table.Column<int>(type: "integer", nullable: false),
                    assinado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_documentos_ged", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "registros_auditoria",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    entidade = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    entidade_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    acao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    valores_antes = table.Column<string>(type: "text", nullable: true),
                    valores_depois = table.Column<string>(type: "text", nullable: true),
                    usuario = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ip_origem = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ocorrido_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_registros_auditoria", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sequencias_numeracao",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_documento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ultimo_valor = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("p_k_sequencias_numeracao", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix__documento_ged_sync_id",
                schema: "aplicativo",
                table: "documentos_ged",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__documento_ged_tenant_id",
                schema: "aplicativo",
                table: "documentos_ged",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_documentos_ged_origem",
                schema: "aplicativo",
                table: "documentos_ged",
                columns: new[] { "entidade_origem_tipo", "entidade_origem_id" });

            migrationBuilder.CreateIndex(
                name: "ix_documentos_ged_tenant_hash",
                schema: "aplicativo",
                table: "documentos_ged",
                columns: new[] { "tenant_id", "hash" });

            migrationBuilder.CreateIndex(
                name: "ix_registros_auditoria_ocorrido_em",
                schema: "aplicativo",
                table: "registros_auditoria",
                column: "ocorrido_em");

            migrationBuilder.CreateIndex(
                name: "ix_registros_auditoria_tenant_entidade",
                schema: "aplicativo",
                table: "registros_auditoria",
                columns: new[] { "tenant_id", "entidade", "entidade_id" });

            migrationBuilder.CreateIndex(
                name: "ix__sequencia_numeracao_sync_id",
                schema: "aplicativo",
                table: "sequencias_numeracao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sequencia_numeracao_tenant_id",
                schema: "aplicativo",
                table: "sequencias_numeracao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_sequencias_numeracao_tenant_tipo",
                schema: "aplicativo",
                table: "sequencias_numeracao",
                columns: new[] { "tenant_id", "tipo_documento" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "documentos_ged",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "registros_auditoria",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "sequencias_numeracao",
                schema: "aplicativo");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "wf_transicoes");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "wf_tarefas");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "wf_solicitacoes");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "wf_parametros");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "wf_jobs");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "wf_job_tentativas");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "wf_instancias");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "wf_historicos");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "wf_eventos_dominio");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "wf_estados");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "wf_definicoes");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "wf_anexos");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "wf_agendamentos");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "usuarios_internos");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "usuarios_empresas");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_upload_partes");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_migracoes_offline");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_mapeamentos_importacao");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_importacoes_xml");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_importacao_linhas");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_importacao_erros");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_historicos");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_filas_url_remota");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_exportacao_campos");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_execucoes_upload");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_execucoes_importacao");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_execucoes_exportacao");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_configuracoes");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_atualizacoes_versao");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_atualizacoes_job");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_atualizacoes_bloco");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_arquivos_xml_saida");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "upl_arquivos");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "update_logs");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "system_settings");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "solicitacoes_upgrade_versao");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "sessoes_usuarios");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "sessoes_impersonacao");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "preferencias_usuarios");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "personal_access_tokens");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "newsletter_subscribers");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "marketplaces_settings");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "logs_execucao_massa");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "landing_pages_settings");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "installation_state");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "idiomas");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "identidades_externas");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "historicos_login");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "execucoes_massa_global");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "custom_pages");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "configuracoes_empresas");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "comunicacoes_super_admin");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "banned_ips");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "anos_financeiros");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "aplicativo",
                table: "acessos_usuario_tenant");
        }
    }
}
