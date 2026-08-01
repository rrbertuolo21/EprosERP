using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Aplicativo.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ativo",
                schema: "aplicativo",
                table: "newsletter_subscribers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "banned_ips",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ip_address = table.Column<string>(type: "text", nullable: false),
                    expiracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    motivo = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_banned_ips", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "historicos_login",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    email_tentado = table.Column<string>(type: "text", nullable: false),
                    ip_address = table.Column<string>(type: "text", nullable: false),
                    user_agent = table.Column<string>(type: "text", nullable: false),
                    sucesso = table.Column<bool>(type: "boolean", nullable: false),
                    detalhes = table.Column<string>(type: "text", nullable: false),
                    data_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_historicos_login", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "personal_access_tokens",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    token_hash = table.Column<string>(type: "text", nullable: false),
                    expiracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ultimo_uso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revogado = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_personal_access_tokens", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sessoes_usuarios",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_sessao = table.Column<string>(type: "text", nullable: false),
                    ip_address = table.Column<string>(type: "text", nullable: false),
                    user_agent = table.Column<string>(type: "text", nullable: false),
                    expiracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revogado = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_sessoes_usuarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    telefone = table.Column<string>(type: "text", nullable: true),
                    mfa_habilitado = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    tipo = table.Column<int>(type: "integer", nullable: false),
                    forcar_troca_senha = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false),
                    lockout_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_usuarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios_empresas",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    perfil_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    eh_admin = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_usuarios_empresas", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix__banned_ip_sync_id",
                schema: "aplicativo",
                table: "banned_ips",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__banned_ip_tenant_id",
                schema: "aplicativo",
                table: "banned_ips",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_banned_ips_ip",
                schema: "aplicativo",
                table: "banned_ips",
                column: "ip_address",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__historico_login_sync_id",
                schema: "aplicativo",
                table: "historicos_login",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__historico_login_tenant_id",
                schema: "aplicativo",
                table: "historicos_login",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_historico_login_email",
                schema: "aplicativo",
                table: "historicos_login",
                column: "email_tentado");

            migrationBuilder.CreateIndex(
                name: "ix__personal_access_token_sync_id",
                schema: "aplicativo",
                table: "personal_access_tokens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__personal_access_token_tenant_id",
                schema: "aplicativo",
                table: "personal_access_tokens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_tokens_hash",
                schema: "aplicativo",
                table: "personal_access_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sessao_usuario_sync_id",
                schema: "aplicativo",
                table: "sessoes_usuarios",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sessao_usuario_tenant_id",
                schema: "aplicativo",
                table: "sessoes_usuarios",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_sessoes_token",
                schema: "aplicativo",
                table: "sessoes_usuarios",
                column: "token_sessao",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__usuario_sync_id",
                schema: "aplicativo",
                table: "usuarios",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__usuario_tenant_id",
                schema: "aplicativo",
                table: "usuarios",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_email",
                schema: "aplicativo",
                table: "usuarios",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__usuario_empresa_sync_id",
                schema: "aplicativo",
                table: "usuarios_empresas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__usuario_empresa_tenant_id",
                schema: "aplicativo",
                table: "usuarios_empresas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_usuario_empresas_usuario_empresa",
                schema: "aplicativo",
                table: "usuarios_empresas",
                columns: new[] { "usuario_id", "empresa_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "banned_ips",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "historicos_login",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "personal_access_tokens",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "sessoes_usuarios",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "usuarios",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "usuarios_empresas",
                schema: "aplicativo");

            migrationBuilder.DropColumn(
                name: "ativo",
                schema: "aplicativo",
                table: "newsletter_subscribers");
        }
    }
}
