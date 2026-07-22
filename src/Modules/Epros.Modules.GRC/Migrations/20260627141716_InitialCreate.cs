using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GRC.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "grc");

            migrationBuilder.CreateTable(
                name: "controles_internos",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    frequencia = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_controles_internos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "denuncias",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_acompanhamento = table.Column<string>(type: "text", nullable: false),
                    relato = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    data_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    parecer_final = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_denuncias", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "incidentes_compliance",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    titulo = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    origem = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    gravidade = table.Column<string>(type: "text", nullable: false),
                    resolucao = table.Column<string>(type: "text", nullable: true),
                    data_abertura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_fechamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_incidentes_compliance", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    event_type = table.Column<string>(type: "text", nullable: false),
                    payload = table.Column<string>(type: "text", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    erro = table.Column<string>(type: "text", nullable: true),
                    tentativas = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "riscos_corporativos",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    titulo = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    categoria = table.Column<string>(type: "text", nullable: false),
                    probabilidade = table.Column<int>(type: "integer", nullable: false),
                    impacto = table.Column<int>(type: "integer", nullable: false),
                    nivel_risco = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    acoes_mitigadoras = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_riscos_corporativos", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_controles_internos_tenant_id_codigo",
                schema: "grc",
                table: "controles_internos",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__controle_interno_sync_id",
                schema: "grc",
                table: "controles_internos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__controle_interno_tenant_id",
                schema: "grc",
                table: "controles_internos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_denuncias_tenant_id_codigo_acompanhamento",
                schema: "grc",
                table: "denuncias",
                columns: new[] { "tenant_id", "codigo_acompanhamento" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__denuncia_sync_id",
                schema: "grc",
                table: "denuncias",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__denuncia_tenant_id",
                schema: "grc",
                table: "denuncias",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__incidente_compliance_sync_id",
                schema: "grc",
                table: "incidentes_compliance",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__incidente_compliance_tenant_id",
                schema: "grc",
                table: "incidentes_compliance",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__risco_corporativo_sync_id",
                schema: "grc",
                table: "riscos_corporativos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__risco_corporativo_tenant_id",
                schema: "grc",
                table: "riscos_corporativos",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "controles_internos",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "denuncias",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "incidentes_compliance",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "riscos_corporativos",
                schema: "grc");
        }
    }
}
