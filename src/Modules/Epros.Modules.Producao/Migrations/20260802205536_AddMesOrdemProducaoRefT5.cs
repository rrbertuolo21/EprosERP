using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Producao.Migrations
{
    /// <inheritdoc />
    public partial class AddMesOrdemProducaoRefT5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ordem_producao_ref_id",
                schema: "producao",
                table: "prd_mes_ordem",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "prd_ctr_centro_trabalho",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    minutos_por_turno = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    turnos_por_dia = table.Column<int>(type: "integer", nullable: false),
                    dias_uteis_por_semana = table.Column<int>(type: "integer", nullable: false),
                    eficiencia_percentual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    motivo_rejeicao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("p_k_prd_ctr_centro_trabalho", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_mrp_necessidade",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    planejamento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nivel = table.Column<int>(type: "integer", nullable: false),
                    data_referencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    necessidade_bruta = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    disponibilidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    recebimentos_programados = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    estoque_seguranca = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    necessidade_liquida = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_prd_mrp_necessidade", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_mrp_sugestao",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    planejamento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    data_sugerida = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    documento_gerado_id = table.Column<Guid>(type: "uuid", nullable: true),
                    impacto_financeiro = table.Column<bool>(type: "boolean", nullable: false),
                    motivo_cancelamento = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("p_k_prd_mrp_sugestao", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_prd_mes_ordem_tenant_id_ordem_producao_ref_id",
                schema: "producao",
                table: "prd_mes_ordem",
                columns: new[] { "tenant_id", "ordem_producao_ref_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_prd_ctr_centro_trabalho_tenant_id_codigo",
                schema: "producao",
                table: "prd_ctr_centro_trabalho",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_prd_ctr_centro_trabalho_tenant_id_status",
                schema: "producao",
                table: "prd_ctr_centro_trabalho",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__centro_trabalho_sync_id",
                schema: "producao",
                table: "prd_ctr_centro_trabalho",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__centro_trabalho_tenant_id",
                schema: "producao",
                table: "prd_ctr_centro_trabalho",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_mrp_necessidade_tenant_id_item_id",
                schema: "producao",
                table: "prd_mrp_necessidade",
                columns: new[] { "tenant_id", "item_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_prd_mrp_necessidade_tenant_id_planejamento_id",
                schema: "producao",
                table: "prd_mrp_necessidade",
                columns: new[] { "tenant_id", "planejamento_id" });

            migrationBuilder.CreateIndex(
                name: "ix__mrp_necessidade_sync_id",
                schema: "producao",
                table: "prd_mrp_necessidade",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__mrp_necessidade_tenant_id",
                schema: "producao",
                table: "prd_mrp_necessidade",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_mrp_sugestao_tenant_id_estado",
                schema: "producao",
                table: "prd_mrp_sugestao",
                columns: new[] { "tenant_id", "estado" });

            migrationBuilder.CreateIndex(
                name: "i_x_prd_mrp_sugestao_tenant_id_item_id",
                schema: "producao",
                table: "prd_mrp_sugestao",
                columns: new[] { "tenant_id", "item_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_prd_mrp_sugestao_tenant_id_planejamento_id",
                schema: "producao",
                table: "prd_mrp_sugestao",
                columns: new[] { "tenant_id", "planejamento_id" });

            migrationBuilder.CreateIndex(
                name: "ix__mrp_sugestao_sync_id",
                schema: "producao",
                table: "prd_mrp_sugestao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__mrp_sugestao_tenant_id",
                schema: "producao",
                table: "prd_mrp_sugestao",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "prd_ctr_centro_trabalho",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_mrp_necessidade",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_mrp_sugestao",
                schema: "producao");

            migrationBuilder.DropIndex(
                name: "i_x_prd_mes_ordem_tenant_id_ordem_producao_ref_id",
                schema: "producao",
                table: "prd_mes_ordem");

            migrationBuilder.DropColumn(
                name: "ordem_producao_ref_id",
                schema: "producao",
                table: "prd_mes_ordem");
        }
    }
}
