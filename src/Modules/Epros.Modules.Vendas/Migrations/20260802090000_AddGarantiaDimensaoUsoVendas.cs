using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Vendas.Migrations
{
    /// <summary>
    /// Correção INV-01 (VEN-GAR): garantia com DUAS dimensões de vigência — tempo + uso (km/horas),
    /// vence o que ocorrer primeiro. Adiciona limite_uso/unidade_uso à política e
    /// unidade_uso/uso_origem/uso_vencimento à cobertura. Fonte: EF_7_VENDAS_GARANTIAS_V1 §0.1/§10.
    /// Migração escrita à mão (Up/Down enxutos) para evitar o ruído de xmin do scaffold automático.
    /// </summary>
    [Migration("20260802090000_AddGarantiaDimensaoUsoVendas")]
    public partial class AddGarantiaDimensaoUsoVendas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ven_garantia_politicas: dimensão de USO
            migrationBuilder.AddColumn<decimal>(
                name: "limite_uso",
                schema: "vendas",
                table: "ven_garantia_politicas",
                type: "numeric(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "unidade_uso",
                schema: "vendas",
                table: "ven_garantia_politicas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // ven_garantia_coberturas: dimensão de USO (origem/vencimento) + unidade
            migrationBuilder.AddColumn<int>(
                name: "unidade_uso",
                schema: "vendas",
                table: "ven_garantia_coberturas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "uso_origem",
                schema: "vendas",
                table: "ven_garantia_coberturas",
                type: "numeric(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "uso_vencimento",
                schema: "vendas",
                table: "ven_garantia_coberturas",
                type: "numeric(18,4)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "limite_uso", schema: "vendas", table: "ven_garantia_politicas");
            migrationBuilder.DropColumn(name: "unidade_uso", schema: "vendas", table: "ven_garantia_politicas");
            migrationBuilder.DropColumn(name: "unidade_uso", schema: "vendas", table: "ven_garantia_coberturas");
            migrationBuilder.DropColumn(name: "uso_origem", schema: "vendas", table: "ven_garantia_coberturas");
            migrationBuilder.DropColumn(name: "uso_vencimento", schema: "vendas", table: "ven_garantia_coberturas");
        }
    }
}
