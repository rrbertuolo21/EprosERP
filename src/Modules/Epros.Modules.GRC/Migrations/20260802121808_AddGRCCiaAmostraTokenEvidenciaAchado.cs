using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GRC.Migrations
{
    /// <inheritdoc />
    public partial class AddGRCCiaAmostraTokenEvidenciaAchado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "aprovado",
                schema: "grc",
                table: "grc_cia_achado",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "aprovador_id",
                schema: "grc",
                table: "grc_cia_achado",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "grc_cia_amostra",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    teste_controle_id = table.Column<Guid>(type: "uuid", nullable: true),
                    plano_auditoria_id = table.Column<Guid>(type: "uuid", nullable: true),
                    metodo = table.Column<string>(type: "text", nullable: false),
                    tamanho = table.Column<int>(type: "integer", nullable: false),
                    criterio = table.Column<string>(type: "text", nullable: true),
                    justificativa = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_grc_cia_amostra", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_cia_evidencia",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    achado_id = table.Column<Guid>(type: "uuid", nullable: true),
                    teste_controle_id = table.Column<Guid>(type: "uuid", nullable: true),
                    arquivo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_grc_cia_evidencia", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_cia_token_acesso",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    plano_auditoria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "text", nullable: false),
                    auditor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    escopo = table.Column<string>(type: "text", nullable: true),
                    expira_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revogado = table.Column<bool>(type: "boolean", nullable: false),
                    ultimo_acesso_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_grc_cia_token_acesso", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_grc_cia_amostra_plano_auditoria_id",
                schema: "grc",
                table: "grc_cia_amostra",
                column: "plano_auditoria_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_cia_amostra_teste_controle_id",
                schema: "grc",
                table: "grc_cia_amostra",
                column: "teste_controle_id");

            migrationBuilder.CreateIndex(
                name: "ix__amostra_auditoria_sync_id",
                schema: "grc",
                table: "grc_cia_amostra",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__amostra_auditoria_tenant_id",
                schema: "grc",
                table: "grc_cia_amostra",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_cia_evidencia_achado_id",
                schema: "grc",
                table: "grc_cia_evidencia",
                column: "achado_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_cia_evidencia_teste_controle_id",
                schema: "grc",
                table: "grc_cia_evidencia",
                column: "teste_controle_id");

            migrationBuilder.CreateIndex(
                name: "ix__evidencia_auditoria_sync_id",
                schema: "grc",
                table: "grc_cia_evidencia",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__evidencia_auditoria_tenant_id",
                schema: "grc",
                table: "grc_cia_evidencia",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_cia_token_acesso_plano_auditoria_id",
                schema: "grc",
                table: "grc_cia_token_acesso",
                column: "plano_auditoria_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_cia_token_acesso_tenant_id_token_hash",
                schema: "grc",
                table: "grc_cia_token_acesso",
                columns: new[] { "tenant_id", "token_hash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__token_acesso_auditoria_sync_id",
                schema: "grc",
                table: "grc_cia_token_acesso",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__token_acesso_auditoria_tenant_id",
                schema: "grc",
                table: "grc_cia_token_acesso",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "grc_cia_amostra",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_cia_evidencia",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_cia_token_acesso",
                schema: "grc");

            migrationBuilder.DropColumn(
                name: "aprovado",
                schema: "grc",
                table: "grc_cia_achado");

            migrationBuilder.DropColumn(
                name: "aprovador_id",
                schema: "grc",
                table: "grc_cia_achado");
        }
    }
}
