using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Vendas.Migrations
{
    /// <inheritdoc />
    public partial class PortEqualizacaoVendas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fatos_geradores_financeiros",
                schema: "vendas");

            // status mudou de string (character varying(20)) para int (enum EVendaStatus).
            // Postgres nao faz cast automatico varchar->int; banco sera zerado (sem dados), USING (0).
            migrationBuilder.Sql("ALTER TABLE vendas.vendas ALTER COLUMN status TYPE integer USING (0);");

            migrationBuilder.AddColumn<bool>(
                name: "cancelada",
                schema: "vendas",
                table: "vendas",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cancelada",
                schema: "vendas",
                table: "vendas");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "vendas",
                table: "vendas",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateTable(
                name: "fatos_geradores_financeiros",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: true),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true),
                    compra_id = table.Column<Guid>(type: "uuid", nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    origem = table.Column<int>(type: "integer", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_fatos_geradores_financeiros", x => x.id);
                    table.ForeignKey(
                        name: "f_k_fatos_geradores_financeiros__vendas_venda_id",
                        column: x => x.venda_id,
                        principalSchema: "vendas",
                        principalTable: "vendas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_fatos_geradores_financeiros_venda_id",
                schema: "vendas",
                table: "fatos_geradores_financeiros",
                column: "venda_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fato_gerador_financeiro_sync_id",
                schema: "vendas",
                table: "fatos_geradores_financeiros",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fato_gerador_financeiro_tenant_id",
                schema: "vendas",
                table: "fatos_geradores_financeiros",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_fato_gerador_fin_tenant_compra",
                schema: "vendas",
                table: "fatos_geradores_financeiros",
                columns: new[] { "tenant_id", "compra_id" });

            migrationBuilder.CreateIndex(
                name: "ix_fato_gerador_fin_tenant_venda",
                schema: "vendas",
                table: "fatos_geradores_financeiros",
                columns: new[] { "tenant_id", "venda_id" });
        }
    }
}
