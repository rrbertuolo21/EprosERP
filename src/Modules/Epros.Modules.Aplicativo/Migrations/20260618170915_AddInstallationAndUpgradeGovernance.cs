using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Aplicativo.Migrations
{
    /// <inheritdoc />
    public partial class AddInstallationAndUpgradeGovernance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "installation_state",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_by = table.Column<string>(type: "text", nullable: true),
                    database_initialized = table.Column<bool>(type: "boolean", nullable: false),
                    admin_created = table.Column<bool>(type: "boolean", nullable: false),
                    system_settings_seeded = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_installation_state", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "update_logs",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    versao_alvo = table.Column<string>(type: "text", nullable: false),
                    executado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    executado_por = table.Column<string>(type: "text", nullable: false),
                    sucesso = table.Column<bool>(type: "boolean", nullable: false),
                    log = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_update_logs", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix__instalacao_state_sync_id",
                schema: "aplicativo",
                table: "installation_state",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__instalacao_state_tenant_id",
                schema: "aplicativo",
                table: "installation_state",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__update_log_sync_id",
                schema: "aplicativo",
                table: "update_logs",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__update_log_tenant_id",
                schema: "aplicativo",
                table: "update_logs",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "installation_state",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "update_logs",
                schema: "aplicativo");
        }
    }
}
