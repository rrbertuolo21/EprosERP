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

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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

        }
    }
}
