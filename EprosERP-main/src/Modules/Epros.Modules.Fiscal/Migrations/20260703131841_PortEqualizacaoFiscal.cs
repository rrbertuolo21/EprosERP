using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Fiscal.Migrations
{
    /// <inheritdoc />
    public partial class PortEqualizacaoFiscal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "empresa_id",
                schema: "plataforma",
                table: "documentos_fiscais",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "aliquota_cofins",
                schema: "plataforma",
                table: "documento_fiscal_itens",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "aliquota_ipi",
                schema: "plataforma",
                table: "documento_fiscal_itens",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "aliquota_pis",
                schema: "plataforma",
                table: "documento_fiscal_itens",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "base_calculo_icms",
                schema: "plataforma",
                table: "documento_fiscal_itens",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "c_class_trib",
                schema: "plataforma",
                table: "documento_fiscal_itens",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "csosn",
                schema: "plataforma",
                table: "documento_fiscal_itens",
                type: "character varying(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cst_ibs_cbs",
                schema: "plataforma",
                table: "documento_fiscal_itens",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cst_ipi",
                schema: "plataforma",
                table: "documento_fiscal_itens",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cst_pis_cofins",
                schema: "plataforma",
                table: "documento_fiscal_itens",
                type: "character varying(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "impostos_calculados",
                schema: "plataforma",
                table: "documento_fiscal_itens",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "origem",
                schema: "plataforma",
                table: "documento_fiscal_itens",
                type: "character varying(1)",
                maxLength: 1,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "valor_cofins",
                schema: "plataforma",
                table: "documento_fiscal_itens",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_fcp",
                schema: "plataforma",
                table: "documento_fiscal_itens",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_icms_st",
                schema: "plataforma",
                table: "documento_fiscal_itens",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_ipi",
                schema: "plataforma",
                table: "documento_fiscal_itens",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_pis",
                schema: "plataforma",
                table: "documento_fiscal_itens",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "empresa_id",
                schema: "plataforma",
                table: "documentos_fiscais");

            migrationBuilder.DropColumn(
                name: "aliquota_cofins",
                schema: "plataforma",
                table: "documento_fiscal_itens");

            migrationBuilder.DropColumn(
                name: "aliquota_ipi",
                schema: "plataforma",
                table: "documento_fiscal_itens");

            migrationBuilder.DropColumn(
                name: "aliquota_pis",
                schema: "plataforma",
                table: "documento_fiscal_itens");

            migrationBuilder.DropColumn(
                name: "base_calculo_icms",
                schema: "plataforma",
                table: "documento_fiscal_itens");

            migrationBuilder.DropColumn(
                name: "c_class_trib",
                schema: "plataforma",
                table: "documento_fiscal_itens");

            migrationBuilder.DropColumn(
                name: "csosn",
                schema: "plataforma",
                table: "documento_fiscal_itens");

            migrationBuilder.DropColumn(
                name: "cst_ibs_cbs",
                schema: "plataforma",
                table: "documento_fiscal_itens");

            migrationBuilder.DropColumn(
                name: "cst_ipi",
                schema: "plataforma",
                table: "documento_fiscal_itens");

            migrationBuilder.DropColumn(
                name: "cst_pis_cofins",
                schema: "plataforma",
                table: "documento_fiscal_itens");

            migrationBuilder.DropColumn(
                name: "impostos_calculados",
                schema: "plataforma",
                table: "documento_fiscal_itens");

            migrationBuilder.DropColumn(
                name: "origem",
                schema: "plataforma",
                table: "documento_fiscal_itens");

            migrationBuilder.DropColumn(
                name: "valor_cofins",
                schema: "plataforma",
                table: "documento_fiscal_itens");

            migrationBuilder.DropColumn(
                name: "valor_fcp",
                schema: "plataforma",
                table: "documento_fiscal_itens");

            migrationBuilder.DropColumn(
                name: "valor_icms_st",
                schema: "plataforma",
                table: "documento_fiscal_itens");

            migrationBuilder.DropColumn(
                name: "valor_ipi",
                schema: "plataforma",
                table: "documento_fiscal_itens");

            migrationBuilder.DropColumn(
                name: "valor_pis",
                schema: "plataforma",
                table: "documento_fiscal_itens");
        }
    }
}
