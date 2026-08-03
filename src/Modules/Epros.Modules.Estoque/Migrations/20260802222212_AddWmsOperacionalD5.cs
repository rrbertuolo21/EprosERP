using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Estoque.Migrations
{
    /// <inheritdoc />
    public partial class AddWmsOperacionalD5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "capacidade_maxima",
                schema: "estoque",
                table: "wms_enderecos_operacionais",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nivel",
                schema: "estoque",
                table: "wms_enderecos_operacionais",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "posicao",
                schema: "estoque",
                table: "wms_enderecos_operacionais",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "zona",
                schema: "estoque",
                table: "wms_enderecos_operacionais",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "wms_tarefas_separacao",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    armazem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    local_origem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    local_destino_id = table.Column<Guid>(type: "uuid", nullable: true),
                    codigo_lote = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    numero_serie = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    quantidade_solicitada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantidade_conferida = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    observacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("p_k_wms_tarefas_separacao", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_wms_tarefas_separacao_tenant_id_armazem_id_status",
                schema: "estoque",
                table: "wms_tarefas_separacao",
                columns: new[] { "tenant_id", "armazem_id", "status" });

            migrationBuilder.CreateIndex(
                name: "i_x_wms_tarefas_separacao_tenant_id_produto_id",
                schema: "estoque",
                table: "wms_tarefas_separacao",
                columns: new[] { "tenant_id", "produto_id" });

            migrationBuilder.CreateIndex(
                name: "ix__wms_tarefa_separacao_sync_id",
                schema: "estoque",
                table: "wms_tarefas_separacao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__wms_tarefa_separacao_tenant_id",
                schema: "estoque",
                table: "wms_tarefas_separacao",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "wms_tarefas_separacao",
                schema: "estoque");

            migrationBuilder.DropColumn(
                name: "capacidade_maxima",
                schema: "estoque",
                table: "wms_enderecos_operacionais");

            migrationBuilder.DropColumn(
                name: "nivel",
                schema: "estoque",
                table: "wms_enderecos_operacionais");

            migrationBuilder.DropColumn(
                name: "posicao",
                schema: "estoque",
                table: "wms_enderecos_operacionais");

            migrationBuilder.DropColumn(
                name: "zona",
                schema: "estoque",
                table: "wms_enderecos_operacionais");
        }
    }
}
