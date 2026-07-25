using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Fiscal.Migrations
{
    /// <inheritdoc />
    public partial class PortCaudaV13Fiscal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inutilizacoes_fiscais",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    modelo_documento = table.Column<int>(type: "integer", nullable: false),
                    serie = table.Column<int>(type: "integer", nullable: false),
                    nr_nf_inicial = table.Column<long>(type: "bigint", nullable: false),
                    nr_nf_final = table.Column<long>(type: "bigint", nullable: false),
                    ano = table.Column<int>(type: "integer", nullable: false),
                    ambiente = table.Column<int>(type: "integer", nullable: false),
                    justificativa = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    status_sefaz = table.Column<int>(type: "integer", nullable: false),
                    motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    protocolo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    data_inutilizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_inutilizacoes_fiscais", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_inutilizacoes_fiscais_tenant_id_modelo_documento_serie",
                schema: "plataforma",
                table: "inutilizacoes_fiscais",
                columns: new[] { "tenant_id", "modelo_documento", "serie" });

            migrationBuilder.CreateIndex(
                name: "ix__inutilizacao_fiscal_sync_id",
                schema: "plataforma",
                table: "inutilizacoes_fiscais",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__inutilizacao_fiscal_tenant_id",
                schema: "plataforma",
                table: "inutilizacoes_fiscais",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inutilizacoes_fiscais",
                schema: "plataforma");
        }
    }
}
