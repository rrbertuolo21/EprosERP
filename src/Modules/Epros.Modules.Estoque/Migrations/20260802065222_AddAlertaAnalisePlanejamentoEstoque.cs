using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Estoque.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertaAnalisePlanejamentoEstoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "alertas_estoque",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    posicao_estoque_id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_alerta = table.Column<int>(type: "integer", nullable: false),
                    quantidade_referencia = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantidade_atual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status_alerta = table.Column<int>(type: "integer", nullable: false),
                    data_alerta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    resolvido_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resolvido_por = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("p_k_alertas_estoque", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_alertas_estoque_tenant_id_posicao_estoque_id_tipo_alerta",
                schema: "estoque",
                table: "alertas_estoque",
                columns: new[] { "tenant_id", "posicao_estoque_id", "tipo_alerta" });

            migrationBuilder.CreateIndex(
                name: "i_x_alertas_estoque_tenant_id_status_alerta",
                schema: "estoque",
                table: "alertas_estoque",
                columns: new[] { "tenant_id", "status_alerta" });

            migrationBuilder.CreateIndex(
                name: "ix__alerta_estoque_sync_id",
                schema: "estoque",
                table: "alertas_estoque",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__alerta_estoque_tenant_id",
                schema: "estoque",
                table: "alertas_estoque",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alertas_estoque",
                schema: "estoque");
        }
    }
}
