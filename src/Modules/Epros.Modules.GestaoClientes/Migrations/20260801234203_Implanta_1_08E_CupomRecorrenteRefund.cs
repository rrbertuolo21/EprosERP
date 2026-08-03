using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GestaoClientes.Migrations
{
    /// <inheritdoc />
    public partial class Implanta_1_08E_CupomRecorrenteRefund : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "pedido_id",
                schema: "plataforma",
                table: "usos_cupons",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "fatura_id",
                schema: "plataforma",
                table: "usos_cupons",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "data_estorno",
                schema: "plataforma",
                table: "pagamentos_faturas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "identificador_estorno",
                schema: "plataforma",
                table: "pagamentos_faturas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_estornado",
                schema: "plataforma",
                table: "pagamentos_faturas",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "cupom_id",
                schema: "plataforma",
                table: "assinaturas_clientes",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "i_x_usos_cupons_fatura_id",
                schema: "plataforma",
                table: "usos_cupons",
                column: "fatura_id");

            migrationBuilder.CreateIndex(
                name: "ix_usos_cupons_cliente_cupom_fatura",
                schema: "plataforma",
                table: "usos_cupons",
                columns: new[] { "cliente_id", "cupom_id", "fatura_id" },
                unique: true,
                filter: "fatura_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "i_x_assinaturas_clientes_cupom_id",
                schema: "plataforma",
                table: "assinaturas_clientes",
                column: "cupom_id");

            migrationBuilder.AddForeignKey(
                name: "f_k_assinaturas_clientes__cupons_cupom_id",
                schema: "plataforma",
                table: "assinaturas_clientes",
                column: "cupom_id",
                principalSchema: "plataforma",
                principalTable: "cupons",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "f_k_usos_cupons_faturas_fatura_id",
                schema: "plataforma",
                table: "usos_cupons",
                column: "fatura_id",
                principalSchema: "plataforma",
                principalTable: "faturas",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_assinaturas_clientes__cupons_cupom_id",
                schema: "plataforma",
                table: "assinaturas_clientes");

            migrationBuilder.DropForeignKey(
                name: "f_k_usos_cupons_faturas_fatura_id",
                schema: "plataforma",
                table: "usos_cupons");

            migrationBuilder.DropIndex(
                name: "i_x_usos_cupons_fatura_id",
                schema: "plataforma",
                table: "usos_cupons");

            migrationBuilder.DropIndex(
                name: "ix_usos_cupons_cliente_cupom_fatura",
                schema: "plataforma",
                table: "usos_cupons");

            migrationBuilder.DropIndex(
                name: "i_x_assinaturas_clientes_cupom_id",
                schema: "plataforma",
                table: "assinaturas_clientes");

            migrationBuilder.DropColumn(
                name: "fatura_id",
                schema: "plataforma",
                table: "usos_cupons");

            migrationBuilder.DropColumn(
                name: "data_estorno",
                schema: "plataforma",
                table: "pagamentos_faturas");

            migrationBuilder.DropColumn(
                name: "identificador_estorno",
                schema: "plataforma",
                table: "pagamentos_faturas");

            migrationBuilder.DropColumn(
                name: "valor_estornado",
                schema: "plataforma",
                table: "pagamentos_faturas");

            migrationBuilder.DropColumn(
                name: "cupom_id",
                schema: "plataforma",
                table: "assinaturas_clientes");

            migrationBuilder.AlterColumn<Guid>(
                name: "pedido_id",
                schema: "plataforma",
                table: "usos_cupons",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
