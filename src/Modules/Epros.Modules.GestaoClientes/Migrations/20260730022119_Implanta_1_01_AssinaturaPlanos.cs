using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GestaoClientes.Migrations
{
    /// <inheritdoc />
    public partial class Implanta_1_01_AssinaturaPlanos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "duration",
                schema: "plataforma",
                table: "planos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "modulo_crm",
                schema: "plataforma",
                table: "planos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "modulo_financeiro",
                schema: "plataforma",
                table: "planos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "modulo_pdv",
                schema: "plataforma",
                table: "planos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "modulo_projetos",
                schema: "plataforma",
                table: "planos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "modulo_rh",
                schema: "plataforma",
                table: "planos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<decimal>(
                name: "valor_tarifa",
                schema: "plataforma",
                table: "pagamentos_faturas",
                type: "numeric(18,3)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "data_expiracao",
                schema: "plataforma",
                table: "pagamentos_faturas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "data_liberacao_fundos",
                schema: "plataforma",
                table: "pagamentos_faturas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_recebido",
                schema: "plataforma",
                table: "pagamentos_faturas",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "numero",
                schema: "plataforma",
                table: "faturas",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "observacoes",
                schema: "plataforma",
                table: "faturas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "quitada",
                schema: "plataforma",
                table: "faturas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_pago",
                schema: "plataforma",
                table: "faturas",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "principal",
                schema: "plataforma",
                table: "enderecos_pessoas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "cota_empresas",
                schema: "plataforma",
                table: "clientes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "cota_permissoes",
                schema: "plataforma",
                table: "clientes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "cota_usuarios",
                schema: "plataforma",
                table: "clientes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "fatura_itens",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fatura_id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_fatura_itens", x => x.id);
                    table.ForeignKey(
                        name: "f_k_fatura_itens_faturas_fatura_id",
                        column: x => x.fatura_id,
                        principalSchema: "plataforma",
                        principalTable: "faturas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_fatura_itens_fatura_id",
                schema: "plataforma",
                table: "fatura_itens",
                column: "fatura_id");

            migrationBuilder.CreateIndex(
                name: "ix__fatura_item_sync_id",
                schema: "plataforma",
                table: "fatura_itens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fatura_item_tenant_id",
                schema: "plataforma",
                table: "fatura_itens",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fatura_itens",
                schema: "plataforma");

            migrationBuilder.DropColumn(
                name: "duration",
                schema: "plataforma",
                table: "planos");

            migrationBuilder.DropColumn(
                name: "modulo_crm",
                schema: "plataforma",
                table: "planos");

            migrationBuilder.DropColumn(
                name: "modulo_financeiro",
                schema: "plataforma",
                table: "planos");

            migrationBuilder.DropColumn(
                name: "modulo_pdv",
                schema: "plataforma",
                table: "planos");

            migrationBuilder.DropColumn(
                name: "modulo_projetos",
                schema: "plataforma",
                table: "planos");

            migrationBuilder.DropColumn(
                name: "modulo_rh",
                schema: "plataforma",
                table: "planos");

            migrationBuilder.DropColumn(
                name: "data_expiracao",
                schema: "plataforma",
                table: "pagamentos_faturas");

            migrationBuilder.DropColumn(
                name: "data_liberacao_fundos",
                schema: "plataforma",
                table: "pagamentos_faturas");

            migrationBuilder.DropColumn(
                name: "valor_recebido",
                schema: "plataforma",
                table: "pagamentos_faturas");

            migrationBuilder.DropColumn(
                name: "numero",
                schema: "plataforma",
                table: "faturas");

            migrationBuilder.DropColumn(
                name: "observacoes",
                schema: "plataforma",
                table: "faturas");

            migrationBuilder.DropColumn(
                name: "quitada",
                schema: "plataforma",
                table: "faturas");

            migrationBuilder.DropColumn(
                name: "valor_pago",
                schema: "plataforma",
                table: "faturas");

            migrationBuilder.DropColumn(
                name: "principal",
                schema: "plataforma",
                table: "enderecos_pessoas");

            migrationBuilder.DropColumn(
                name: "cota_empresas",
                schema: "plataforma",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "cota_permissoes",
                schema: "plataforma",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "cota_usuarios",
                schema: "plataforma",
                table: "clientes");

            migrationBuilder.AlterColumn<decimal>(
                name: "valor_tarifa",
                schema: "plataforma",
                table: "pagamentos_faturas",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,3)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);
        }
    }
}
