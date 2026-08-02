using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.DMS.Migrations
{
    /// <inheritdoc />
    public partial class AddCalculoSimulacaoFin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "concessionarias",
                table: "vendas_veiculos",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "concessionarias",
                table: "ordens_servico_dms",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "concessionarias",
                table: "garantias_montadora",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "concessionarias",
                table: "con_ven_reserva",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "concessionarias",
                table: "con_ven_proposta",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "concessionarias",
                table: "con_ven_estoque_veiculo",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "concessionarias",
                table: "con_srv_tipo_servico",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "concessionarias",
                table: "con_srv_pacote",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "concessionarias",
                table: "con_srv_operacao",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "concessionarias",
                table: "con_pes_reserva",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "concessionarias",
                table: "con_pes_peca",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "concessionarias",
                table: "con_pes_demanda",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "concessionarias",
                table: "con_mnt_os_extensao",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "concessionarias",
                table: "con_mnt_orcamento",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "concessionarias",
                table: "con_gar_veiculo_garantia",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "concessionarias",
                table: "con_gar_solicitacao",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "concessionarias",
                table: "con_gar_plano",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<bool>(
                name: "calculada",
                schema: "concessionarias",
                table: "con_fin_simulacao",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "cet_anual",
                schema: "concessionarias",
                table: "con_fin_simulacao",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "iof",
                schema: "concessionarias",
                table: "con_fin_simulacao",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sistema",
                schema: "concessionarias",
                table: "con_fin_simulacao",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "taxa_juros_mensal",
                schema: "concessionarias",
                table: "con_fin_simulacao",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "total_juros",
                schema: "concessionarias",
                table: "con_fin_simulacao",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "total_pago",
                schema: "concessionarias",
                table: "con_fin_simulacao",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_parcela",
                schema: "concessionarias",
                table: "con_fin_simulacao",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "concessionarias",
                table: "con_fin_simulacao",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "concessionarias",
                table: "con_fin_jornada",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "concessionarias",
                table: "con_fin_contrato",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "concessionarias",
                table: "con_dev_rede_no",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "concessionarias",
                table: "con_dev_contrato",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "concessionarias",
                table: "con_crm_test_drive",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "concessionarias",
                table: "con_crm_prospect_showroom",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "concessionarias",
                table: "con_crm_oportunidade",
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
                schema: "concessionarias",
                table: "vendas_veiculos");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "concessionarias",
                table: "ordens_servico_dms");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "concessionarias",
                table: "garantias_montadora");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "concessionarias",
                table: "con_ven_reserva");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "concessionarias",
                table: "con_ven_proposta");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "concessionarias",
                table: "con_ven_estoque_veiculo");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "concessionarias",
                table: "con_srv_tipo_servico");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "concessionarias",
                table: "con_srv_pacote");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "concessionarias",
                table: "con_srv_operacao");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "concessionarias",
                table: "con_pes_reserva");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "concessionarias",
                table: "con_pes_peca");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "concessionarias",
                table: "con_pes_demanda");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "concessionarias",
                table: "con_mnt_os_extensao");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "concessionarias",
                table: "con_mnt_orcamento");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "concessionarias",
                table: "con_gar_veiculo_garantia");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "concessionarias",
                table: "con_gar_solicitacao");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "concessionarias",
                table: "con_gar_plano");

            migrationBuilder.DropColumn(
                name: "calculada",
                schema: "concessionarias",
                table: "con_fin_simulacao");

            migrationBuilder.DropColumn(
                name: "cet_anual",
                schema: "concessionarias",
                table: "con_fin_simulacao");

            migrationBuilder.DropColumn(
                name: "iof",
                schema: "concessionarias",
                table: "con_fin_simulacao");

            migrationBuilder.DropColumn(
                name: "sistema",
                schema: "concessionarias",
                table: "con_fin_simulacao");

            migrationBuilder.DropColumn(
                name: "taxa_juros_mensal",
                schema: "concessionarias",
                table: "con_fin_simulacao");

            migrationBuilder.DropColumn(
                name: "total_juros",
                schema: "concessionarias",
                table: "con_fin_simulacao");

            migrationBuilder.DropColumn(
                name: "total_pago",
                schema: "concessionarias",
                table: "con_fin_simulacao");

            migrationBuilder.DropColumn(
                name: "valor_parcela",
                schema: "concessionarias",
                table: "con_fin_simulacao");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "concessionarias",
                table: "con_fin_simulacao");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "concessionarias",
                table: "con_fin_jornada");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "concessionarias",
                table: "con_fin_contrato");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "concessionarias",
                table: "con_dev_rede_no");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "concessionarias",
                table: "con_dev_contrato");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "concessionarias",
                table: "con_crm_test_drive");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "concessionarias",
                table: "con_crm_prospect_showroom");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "concessionarias",
                table: "con_crm_oportunidade");
        }
    }
}
