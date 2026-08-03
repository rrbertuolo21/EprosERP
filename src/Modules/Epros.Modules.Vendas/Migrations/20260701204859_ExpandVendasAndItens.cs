using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Vendas.Migrations
{
    /// <inheritdoc />
    public partial class ExpandVendasAndItens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "cliente_id",
                schema: "vendas",
                table: "vendas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "data_venda",
                schema: "vendas",
                table: "vendas",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "forma_pagamento",
                schema: "vendas",
                table: "vendas",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "incluir_frete_no_total",
                schema: "vendas",
                table: "vendas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "informacoes_adicionais_fisco",
                schema: "vendas",
                table: "vendas",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "informacoes_complementares",
                schema: "vendas",
                table: "vendas",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "modalidade_frete",
                schema: "vendas",
                table: "vendas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "modelo_fiscal",
                schema: "vendas",
                table: "vendas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "natureza_operacao",
                schema: "vendas",
                table: "vendas",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "valor_desconto",
                schema: "vendas",
                table: "vendas",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_frete",
                schema: "vendas",
                table: "vendas",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "venda_origem",
                schema: "vendas",
                table: "vendas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cest",
                schema: "vendas",
                table: "venda_itens",
                type: "character varying(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "cest_id",
                schema: "vendas",
                table: "venda_itens",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "cfop",
                schema: "vendas",
                table: "venda_itens",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "codigo_anp",
                schema: "vendas",
                table: "venda_itens",
                type: "character varying(9)",
                maxLength: 9,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "codigo_anp_id",
                schema: "vendas",
                table: "venda_itens",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "codigo_ean",
                schema: "vendas",
                table: "venda_itens",
                type: "character varying(14)",
                maxLength: 14,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "codigo_produto",
                schema: "vendas",
                table: "venda_itens",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "descricao_produto",
                schema: "vendas",
                table: "venda_itens",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ncm",
                schema: "vendas",
                table: "venda_itens",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "quantidade_comercial",
                schema: "vendas",
                table: "venda_itens",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "unidade_comercial",
                schema: "vendas",
                table: "venda_itens",
                type: "character varying(6)",
                maxLength: 6,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "valor_custo",
                schema: "vendas",
                table: "venda_itens",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_desconto",
                schema: "vendas",
                table: "venda_itens",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_frete_rateado",
                schema: "vendas",
                table: "venda_itens",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_total_bruto_produtos",
                schema: "vendas",
                table: "venda_itens",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_unitario_comercial",
                schema: "vendas",
                table: "venda_itens",
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
                name: "cliente_id",
                schema: "vendas",
                table: "vendas");

            migrationBuilder.DropColumn(
                name: "data_venda",
                schema: "vendas",
                table: "vendas");

            migrationBuilder.DropColumn(
                name: "forma_pagamento",
                schema: "vendas",
                table: "vendas");

            migrationBuilder.DropColumn(
                name: "incluir_frete_no_total",
                schema: "vendas",
                table: "vendas");

            migrationBuilder.DropColumn(
                name: "informacoes_adicionais_fisco",
                schema: "vendas",
                table: "vendas");

            migrationBuilder.DropColumn(
                name: "informacoes_complementares",
                schema: "vendas",
                table: "vendas");

            migrationBuilder.DropColumn(
                name: "modalidade_frete",
                schema: "vendas",
                table: "vendas");

            migrationBuilder.DropColumn(
                name: "modelo_fiscal",
                schema: "vendas",
                table: "vendas");

            migrationBuilder.DropColumn(
                name: "natureza_operacao",
                schema: "vendas",
                table: "vendas");

            migrationBuilder.DropColumn(
                name: "valor_desconto",
                schema: "vendas",
                table: "vendas");

            migrationBuilder.DropColumn(
                name: "valor_frete",
                schema: "vendas",
                table: "vendas");

            migrationBuilder.DropColumn(
                name: "venda_origem",
                schema: "vendas",
                table: "vendas");

            migrationBuilder.DropColumn(
                name: "cest",
                schema: "vendas",
                table: "venda_itens");

            migrationBuilder.DropColumn(
                name: "cest_id",
                schema: "vendas",
                table: "venda_itens");

            migrationBuilder.DropColumn(
                name: "cfop",
                schema: "vendas",
                table: "venda_itens");

            migrationBuilder.DropColumn(
                name: "codigo_anp",
                schema: "vendas",
                table: "venda_itens");

            migrationBuilder.DropColumn(
                name: "codigo_anp_id",
                schema: "vendas",
                table: "venda_itens");

            migrationBuilder.DropColumn(
                name: "codigo_ean",
                schema: "vendas",
                table: "venda_itens");

            migrationBuilder.DropColumn(
                name: "codigo_produto",
                schema: "vendas",
                table: "venda_itens");

            migrationBuilder.DropColumn(
                name: "descricao_produto",
                schema: "vendas",
                table: "venda_itens");

            migrationBuilder.DropColumn(
                name: "ncm",
                schema: "vendas",
                table: "venda_itens");

            migrationBuilder.DropColumn(
                name: "quantidade_comercial",
                schema: "vendas",
                table: "venda_itens");

            migrationBuilder.DropColumn(
                name: "unidade_comercial",
                schema: "vendas",
                table: "venda_itens");

            migrationBuilder.DropColumn(
                name: "valor_custo",
                schema: "vendas",
                table: "venda_itens");

            migrationBuilder.DropColumn(
                name: "valor_desconto",
                schema: "vendas",
                table: "venda_itens");

            migrationBuilder.DropColumn(
                name: "valor_frete_rateado",
                schema: "vendas",
                table: "venda_itens");

            migrationBuilder.DropColumn(
                name: "valor_total_bruto_produtos",
                schema: "vendas",
                table: "venda_itens");

            migrationBuilder.DropColumn(
                name: "valor_unitario_comercial",
                schema: "vendas",
                table: "venda_itens");
        }
    }
}
