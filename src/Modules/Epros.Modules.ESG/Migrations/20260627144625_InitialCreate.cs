using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.ESG.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "esg");

            migrationBuilder.CreateTable(
                name: "emissoes_carbono",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fonte_emissao = table.Column<string>(type: "text", nullable: false),
                    escopo = table.Column<int>(type: "integer", nullable: false),
                    categoria_ghg = table.Column<string>(type: "text", nullable: false),
                    quantidade_consumo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    unidade_medida = table.Column<string>(type: "text", nullable: false),
                    fator_emissao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_co2e = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    data_transacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_emissoes_carbono", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "relatorios_esg",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ano_fiscal = table.Column<int>(type: "integer", nullable: false),
                    nome_relatorio = table.Column<string>(type: "text", nullable: false),
                    total_escopo1 = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_escopo2 = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_escopo3 = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_geral_co2e = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    parecer_auditoria = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_relatorios_esg", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_emissoes_carbono_tenant_id_data_transacao",
                schema: "esg",
                table: "emissoes_carbono",
                columns: new[] { "tenant_id", "data_transacao" });

            migrationBuilder.CreateIndex(
                name: "ix__emissao_carbono_sync_id",
                schema: "esg",
                table: "emissoes_carbono",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__emissao_carbono_tenant_id",
                schema: "esg",
                table: "emissoes_carbono",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_relatorios_esg_tenant_id_ano_fiscal",
                schema: "esg",
                table: "relatorios_esg",
                columns: new[] { "tenant_id", "ano_fiscal" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__relatorio_e_s_g_sync_id",
                schema: "esg",
                table: "relatorios_esg",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__relatorio_e_s_g_tenant_id",
                schema: "esg",
                table: "relatorios_esg",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "emissoes_carbono",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "relatorios_esg",
                schema: "esg");
        }
    }
}
