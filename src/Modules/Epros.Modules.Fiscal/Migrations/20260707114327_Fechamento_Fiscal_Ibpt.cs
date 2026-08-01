using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Fiscal.Migrations
{
    /// <inheritdoc />
    public partial class Fechamento_Fiscal_Ibpt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ibpts",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    uf = table.Column<int>(type: "integer", maxLength: 2, nullable: false),
                    ex = table.Column<int>(type: "integer", nullable: false),
                    tipo = table.Column<int>(type: "integer", nullable: false),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    aliquota_nacional_federal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aliquota_importados_federal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aliquota_estadual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aliquota_municipal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    versao = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    chave = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    vigencia_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    vigencia_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_ibpts", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_ibpts_codigo_uf_ex",
                schema: "plataforma",
                table: "ibpts",
                columns: new[] { "codigo", "uf", "ex" });

            migrationBuilder.CreateIndex(
                name: "ix__ibpt_sync_id",
                schema: "plataforma",
                table: "ibpts",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ibpt_tenant_id",
                schema: "plataforma",
                table: "ibpts",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ibpts",
                schema: "plataforma");
        }
    }
}
