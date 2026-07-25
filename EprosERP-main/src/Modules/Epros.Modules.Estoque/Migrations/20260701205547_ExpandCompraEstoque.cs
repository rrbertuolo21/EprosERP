using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Estoque.Migrations
{
    /// <inheritdoc />
    public partial class ExpandCompraEstoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "estoque",
                table: "compras",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "numero_nota",
                schema: "estoque",
                table: "compras",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "fornecedor_nome",
                schema: "estoque",
                table: "compras",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "fornecedor_cnpj",
                schema: "estoque",
                table: "compras",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "chave_acesso",
                schema: "estoque",
                table: "compras",
                type: "character varying(44)",
                maxLength: 44,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "compra_origem",
                schema: "estoque",
                table: "compras",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "data_compra",
                schema: "estoque",
                table: "compras",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "forma_pagamento",
                schema: "estoque",
                table: "compras",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "informacoes_adicionais_fisco",
                schema: "estoque",
                table: "compras",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "informacoes_complementares",
                schema: "estoque",
                table: "compras",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "modalidade_frete",
                schema: "estoque",
                table: "compras",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "modelo_fiscal",
                schema: "estoque",
                table: "compras",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "natureza_operacao",
                schema: "estoque",
                table: "compras",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "tipo_nota",
                schema: "estoque",
                table: "compras",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cest",
                schema: "estoque",
                table: "compra_itens",
                type: "character varying(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "cest_id",
                schema: "estoque",
                table: "compra_itens",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "cfop",
                schema: "estoque",
                table: "compra_itens",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "codigo_anp",
                schema: "estoque",
                table: "compra_itens",
                type: "character varying(9)",
                maxLength: 9,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "codigo_anp_id",
                schema: "estoque",
                table: "compra_itens",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "codigo_ean",
                schema: "estoque",
                table: "compra_itens",
                type: "character varying(14)",
                maxLength: 14,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "codigo_produto",
                schema: "estoque",
                table: "compra_itens",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "descricao_produto",
                schema: "estoque",
                table: "compra_itens",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ncm",
                schema: "estoque",
                table: "compra_itens",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "quantidade_comercial",
                schema: "estoque",
                table: "compra_itens",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "unidade_comercial",
                schema: "estoque",
                table: "compra_itens",
                type: "character varying(6)",
                maxLength: 6,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "valor_custo",
                schema: "estoque",
                table: "compra_itens",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_desconto",
                schema: "estoque",
                table: "compra_itens",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_frete_rateado",
                schema: "estoque",
                table: "compra_itens",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_total_bruto_produtos",
                schema: "estoque",
                table: "compra_itens",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_unitario_comercial",
                schema: "estoque",
                table: "compra_itens",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "i_x_compras_tenant_id_chave_acesso",
                schema: "estoque",
                table: "compras",
                columns: new[] { "tenant_id", "chave_acesso" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "i_x_compras_tenant_id_chave_acesso",
                schema: "estoque",
                table: "compras");

            migrationBuilder.DropColumn(
                name: "compra_origem",
                schema: "estoque",
                table: "compras");

            migrationBuilder.DropColumn(
                name: "data_compra",
                schema: "estoque",
                table: "compras");

            migrationBuilder.DropColumn(
                name: "forma_pagamento",
                schema: "estoque",
                table: "compras");

            migrationBuilder.DropColumn(
                name: "informacoes_adicionais_fisco",
                schema: "estoque",
                table: "compras");

            migrationBuilder.DropColumn(
                name: "informacoes_complementares",
                schema: "estoque",
                table: "compras");

            migrationBuilder.DropColumn(
                name: "modalidade_frete",
                schema: "estoque",
                table: "compras");

            migrationBuilder.DropColumn(
                name: "modelo_fiscal",
                schema: "estoque",
                table: "compras");

            migrationBuilder.DropColumn(
                name: "natureza_operacao",
                schema: "estoque",
                table: "compras");

            migrationBuilder.DropColumn(
                name: "tipo_nota",
                schema: "estoque",
                table: "compras");

            migrationBuilder.DropColumn(
                name: "cest",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropColumn(
                name: "cest_id",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropColumn(
                name: "cfop",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropColumn(
                name: "codigo_anp",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropColumn(
                name: "codigo_anp_id",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropColumn(
                name: "codigo_ean",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropColumn(
                name: "codigo_produto",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropColumn(
                name: "descricao_produto",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropColumn(
                name: "ncm",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropColumn(
                name: "quantidade_comercial",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropColumn(
                name: "unidade_comercial",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropColumn(
                name: "valor_custo",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropColumn(
                name: "valor_desconto",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropColumn(
                name: "valor_frete_rateado",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropColumn(
                name: "valor_total_bruto_produtos",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.DropColumn(
                name: "valor_unitario_comercial",
                schema: "estoque",
                table: "compra_itens");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "estoque",
                table: "compras",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "numero_nota",
                schema: "estoque",
                table: "compras",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "fornecedor_nome",
                schema: "estoque",
                table: "compras",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "fornecedor_cnpj",
                schema: "estoque",
                table: "compras",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "chave_acesso",
                schema: "estoque",
                table: "compras",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(44)",
                oldMaxLength: 44);
        }
    }
}
