using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Producao.Migrations
{
    /// <inheritdoc />
    public partial class AddBomTipoComponenteEPrecisao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_pln_snapshot_op",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_pln_planejamento_historico",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_pln_planejamento_anexo",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_pln_planejamento",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_mrp_planejamento_historico",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_mrp_planejamento_anexo",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_mrp_planejamento",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_mes_servico_equipamento",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_mes_servico",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_mes_parametro",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_mes_ordem_item",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_mes_ordem",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_mes_movimento_producao",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_mes_historico",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_mes_consumo_material",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_mes_anexo",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_gos_ficha_producao",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_gos_ficha_historico",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_gos_ficha_anexo",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_est_parametro",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_est_historico",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_est_estimativa",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_est_componente",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_est_anexo",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_esc_programacao",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_esc_parametro",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_esc_operacao",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_esc_historico",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_esc_anexo",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_cst_parametro",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_cst_historico",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_cst_custo_referencia",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_cst_custo_producao",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_cst_anexo",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_bom_instrucao_ordem",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_bom_instrucao",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_bom_historico",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_bom_grupo_componente",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_bom_estrutura",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<string>(
                name: "tipo_componente",
                schema: "producao",
                table: "prd_bom_componente",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_bom_componente",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "prd_bom_anexo",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "ordens_producao",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "listas_materiais",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "bom_itens",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "producao",
                table: "apontamentos",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_pln_snapshot_op");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_pln_planejamento_historico");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_pln_planejamento_anexo");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_pln_planejamento");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_mrp_planejamento_historico");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_mrp_planejamento_anexo");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_mrp_planejamento");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_mes_servico_equipamento");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_mes_servico");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_mes_parametro");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_mes_ordem_item");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_mes_ordem");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_mes_movimento_producao");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_mes_historico");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_mes_consumo_material");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_mes_anexo");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_gos_ficha_producao");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_gos_ficha_historico");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_gos_ficha_anexo");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_est_parametro");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_est_historico");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_est_estimativa");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_est_componente");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_est_anexo");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_esc_programacao");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_esc_parametro");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_esc_operacao");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_esc_historico");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_esc_anexo");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_cst_parametro");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_cst_historico");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_cst_custo_referencia");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_cst_custo_producao");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_cst_anexo");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_bom_instrucao_ordem");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_bom_instrucao");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_bom_historico");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_bom_grupo_componente");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_bom_estrutura");

            migrationBuilder.DropColumn(
                name: "tipo_componente",
                schema: "producao",
                table: "prd_bom_componente");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_bom_componente");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "prd_bom_anexo");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "ordens_producao");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "listas_materiais");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "bom_itens");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "producao",
                table: "apontamentos");
        }
    }
}
