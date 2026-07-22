using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Aplicativo.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRoleExtensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "api_key",
                schema: "aplicativo",
                table: "usuarios",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "preferencias_usuarios",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    idioma = table.Column<string>(type: "text", nullable: true),
                    tema = table.Column<string>(type: "text", nullable: true),
                    avatar = table.Column<string>(type: "text", nullable: true),
                    recebe_notificacoes = table.Column<bool>(type: "boolean", nullable: false),
                    timezone = table.Column<string>(type: "text", nullable: true),
                    formato_data = table.Column<string>(type: "text", nullable: true),
                    formato_hora = table.Column<string>(type: "text", nullable: true),
                    formato_numero = table.Column<string>(type: "text", nullable: true),
                    preferencias_json = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_preferencias_usuarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sessoes_impersonacao",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_original_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_alvo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    inicio_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fim_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    motivo = table.Column<string>(type: "text", nullable: true),
                    ip_origem = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_sessoes_impersonacao", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix__preferencia_usuario_sync_id",
                schema: "aplicativo",
                table: "preferencias_usuarios",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__preferencia_usuario_tenant_id",
                schema: "aplicativo",
                table: "preferencias_usuarios",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_preferencias_usuarios_usuario",
                schema: "aplicativo",
                table: "preferencias_usuarios",
                column: "usuario_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sessao_impersonacao_sync_id",
                schema: "aplicativo",
                table: "sessoes_impersonacao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sessao_impersonacao_tenant_id",
                schema: "aplicativo",
                table: "sessoes_impersonacao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_sessoes_impersonacao_alvo",
                schema: "aplicativo",
                table: "sessoes_impersonacao",
                column: "usuario_alvo_id");

            migrationBuilder.CreateIndex(
                name: "ix_sessoes_impersonacao_original",
                schema: "aplicativo",
                table: "sessoes_impersonacao",
                column: "usuario_original_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "preferencias_usuarios",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "sessoes_impersonacao",
                schema: "aplicativo");

            migrationBuilder.DropColumn(
                name: "api_key",
                schema: "aplicativo",
                table: "usuarios");
        }
    }
}
