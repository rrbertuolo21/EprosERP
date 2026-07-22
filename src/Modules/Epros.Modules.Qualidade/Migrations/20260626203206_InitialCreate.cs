using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Qualidade.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "qualidade");

            migrationBuilder.CreateTable(
                name: "inspecoes_lote",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sku = table.Column<string>(type: "text", nullable: false),
                    nome_produto = table.Column<string>(type: "text", nullable: false),
                    quantidade_lote = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    data_inspecao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    responsavel = table.Column<string>(type: "text", nullable: true),
                    observacoes = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_inspecoes_lote", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "nao_conformidades",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    inspecao_lote_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sku = table.Column<string>(type: "text", nullable: false),
                    titulo = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    causa_raiz = table.Column<string>(type: "text", nullable: true),
                    plano_acao = table.Column<string>(type: "text", nullable: true),
                    resolvido_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resolvido_por = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_nao_conformidades", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "qualidade",
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

            migrationBuilder.CreateIndex(
                name: "i_x_inspecoes_lote_tenant_id_compra_id",
                schema: "qualidade",
                table: "inspecoes_lote",
                columns: new[] { "tenant_id", "compra_id" });

            migrationBuilder.CreateIndex(
                name: "ix__inspecao_lote_sync_id",
                schema: "qualidade",
                table: "inspecoes_lote",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__inspecao_lote_tenant_id",
                schema: "qualidade",
                table: "inspecoes_lote",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_nao_conformidades_tenant_id_inspecao_lote_id",
                schema: "qualidade",
                table: "nao_conformidades",
                columns: new[] { "tenant_id", "inspecao_lote_id" });

            migrationBuilder.CreateIndex(
                name: "ix__nao_conformidade_sync_id",
                schema: "qualidade",
                table: "nao_conformidades",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__nao_conformidade_tenant_id",
                schema: "qualidade",
                table: "nao_conformidades",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inspecoes_lote",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "nao_conformidades",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "qualidade");
        }
    }
}
