using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Aplicativo.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "aplicativo");

            migrationBuilder.CreateTable(
                name: "custom_pages",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    slug = table.Column<string>(type: "text", nullable: false),
                    conteudo = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_custom_pages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "landing_pages_settings",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    settings_json = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_landing_pages_settings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "marketplaces_settings",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    modulo = table.Column<string>(type: "text", nullable: false),
                    settings_json = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_marketplaces_settings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "newsletter_subscribers",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_newsletter_subscribers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "system_settings",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    chave = table.Column<string>(type: "text", nullable: false),
                    valor = table.Column<string>(type: "text", nullable: false),
                    escopo = table.Column<string>(type: "text", nullable: false),
                    eh_segredo = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_system_settings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios_internos",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    senha = table.Column<string>(type: "text", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    unique_id = table.Column<string>(type: "text", nullable: true),
                    timezone = table.Column<string>(type: "text", nullable: true),
                    primary_admin = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_usuarios_internos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "comunicacoes_super_admin",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    business_ids = table.Column<string>(type: "text", nullable: false),
                    assunto = table.Column<string>(type: "text", nullable: false),
                    mensagem = table.Column<string>(type: "text", nullable: false),
                    enviado_por = table.Column<Guid>(type: "uuid", nullable: false),
                    enviado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_comunicacoes_super_admin", x => x.id);
                    table.ForeignKey(
                        name: "f_k_comunicacoes_super_admin__usuarios_internos_enviado_por",
                        column: x => x.enviado_por,
                        principalSchema: "aplicativo",
                        principalTable: "usuarios_internos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "execucoes_massa_global",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    action_payload = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_execucoes_massa_global", x => x.id);
                    table.ForeignKey(
                        name: "f_k_execucoes_massa_global__usuarios_internos_aprovado_por",
                        column: x => x.aprovado_por,
                        principalSchema: "aplicativo",
                        principalTable: "usuarios_internos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "logs_execucao_massa",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    execute_query_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_tenant_id = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    mensagem = table.Column<string>(type: "text", nullable: true),
                    processado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_logs_execucao_massa", x => x.id);
                    table.ForeignKey(
                        name: "f_k_logs_execucao_massa_execucoes_massa_global_execute_query_id",
                        column: x => x.execute_query_id,
                        principalSchema: "aplicativo",
                        principalTable: "execucoes_massa_global",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_comunicacoes_super_admin_enviado_por",
                schema: "aplicativo",
                table: "comunicacoes_super_admin",
                column: "enviado_por");

            migrationBuilder.CreateIndex(
                name: "ix__comunicacao_super_admin_sync_id",
                schema: "aplicativo",
                table: "comunicacoes_super_admin",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__comunicacao_super_admin_tenant_id",
                schema: "aplicativo",
                table: "comunicacoes_super_admin",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__custom_page_sync_id",
                schema: "aplicativo",
                table: "custom_pages",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__custom_page_tenant_id",
                schema: "aplicativo",
                table: "custom_pages",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_custom_pages_slug",
                schema: "aplicativo",
                table: "custom_pages",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_execucoes_massa_global_aprovado_por",
                schema: "aplicativo",
                table: "execucoes_massa_global",
                column: "aprovado_por");

            migrationBuilder.CreateIndex(
                name: "ix__execucao_massa_global_sync_id",
                schema: "aplicativo",
                table: "execucoes_massa_global",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__execucao_massa_global_tenant_id",
                schema: "aplicativo",
                table: "execucoes_massa_global",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_execute_queries_status_active_unique",
                schema: "aplicativo",
                table: "execucoes_massa_global",
                column: "status",
                unique: true,
                filter: "status = 'Active'");

            migrationBuilder.CreateIndex(
                name: "ix__landing_page_settings_sync_id",
                schema: "aplicativo",
                table: "landing_pages_settings",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__landing_page_settings_tenant_id",
                schema: "aplicativo",
                table: "landing_pages_settings",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__log_execucao_massa_sync_id",
                schema: "aplicativo",
                table: "logs_execucao_massa",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__log_execucao_massa_tenant_id",
                schema: "aplicativo",
                table: "logs_execucao_massa",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_execute_query_logs_idempotency_passed",
                schema: "aplicativo",
                table: "logs_execucao_massa",
                columns: new[] { "execute_query_id", "target_tenant_id" },
                unique: true,
                filter: "status = 'Passed'");

            migrationBuilder.CreateIndex(
                name: "ix__marketplace_settings_sync_id",
                schema: "aplicativo",
                table: "marketplaces_settings",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__marketplace_settings_tenant_id",
                schema: "aplicativo",
                table: "marketplaces_settings",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_marketplace_settings_module",
                schema: "aplicativo",
                table: "marketplaces_settings",
                column: "modulo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__newsletter_subscriber_sync_id",
                schema: "aplicativo",
                table: "newsletter_subscribers",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__newsletter_subscriber_tenant_id",
                schema: "aplicativo",
                table: "newsletter_subscribers",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_newsletter_subscribers_email",
                schema: "aplicativo",
                table: "newsletter_subscribers",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__system_setting_sync_id",
                schema: "aplicativo",
                table: "system_settings",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__system_setting_tenant_id",
                schema: "aplicativo",
                table: "system_settings",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_system_settings_chave_escopo",
                schema: "aplicativo",
                table: "system_settings",
                columns: new[] { "chave", "escopo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__usuario_interno_sync_id",
                schema: "aplicativo",
                table: "usuarios_internos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__usuario_interno_tenant_id",
                schema: "aplicativo",
                table: "usuarios_internos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_landlord_users_email",
                schema: "aplicativo",
                table: "usuarios_internos",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "comunicacoes_super_admin",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "custom_pages",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "landing_pages_settings",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "logs_execucao_massa",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "marketplaces_settings",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "newsletter_subscribers",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "system_settings",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "execucoes_massa_global",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "usuarios_internos",
                schema: "aplicativo");
        }
    }
}
