using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Estoque.Migrations
{
    /// <inheritdoc />
    public partial class AddDevolucaoCompra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "com_devolucao_compra",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_origem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fornecedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    data_devolucao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    tipo = table.Column<int>(type: "integer", nullable: false),
                    motivo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    documento_fiscal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cfop = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    confirmada_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancelada_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
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
                    table.PrimaryKey("p_k_com_devolucao_compra", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "com_devolucao_compra_item",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    devolucao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_item_origem_id = table.Column<Guid>(type: "uuid", nullable: true),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
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
                    table.PrimaryKey("p_k_com_devolucao_compra_item", x => x.id);
                    table.ForeignKey(
                        name: "f_k_com_devolucao_compra_item_com_devolucao_compra_devolucao_id",
                        column: x => x.devolucao_id,
                        principalSchema: "estoque",
                        principalTable: "com_devolucao_compra",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_com_devolucao_compra_tenant_id_compra_origem_id",
                schema: "estoque",
                table: "com_devolucao_compra",
                columns: new[] { "tenant_id", "compra_origem_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_com_devolucao_compra_tenant_id_numero",
                schema: "estoque",
                table: "com_devolucao_compra",
                columns: new[] { "tenant_id", "numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_com_devolucao_compra_tenant_id_status",
                schema: "estoque",
                table: "com_devolucao_compra",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__devolucao_compra_sync_id",
                schema: "estoque",
                table: "com_devolucao_compra",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__devolucao_compra_tenant_id",
                schema: "estoque",
                table: "com_devolucao_compra",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_com_devolucao_compra_item_compra_item_origem_id",
                schema: "estoque",
                table: "com_devolucao_compra_item",
                column: "compra_item_origem_id");

            migrationBuilder.CreateIndex(
                name: "i_x_com_devolucao_compra_item_devolucao_id",
                schema: "estoque",
                table: "com_devolucao_compra_item",
                column: "devolucao_id");

            migrationBuilder.CreateIndex(
                name: "i_x_com_devolucao_compra_item_tenant_id_devolucao_id",
                schema: "estoque",
                table: "com_devolucao_compra_item",
                columns: new[] { "tenant_id", "devolucao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__devolucao_compra_item_sync_id",
                schema: "estoque",
                table: "com_devolucao_compra_item",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__devolucao_compra_item_tenant_id",
                schema: "estoque",
                table: "com_devolucao_compra_item",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "com_devolucao_compra_item",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "com_devolucao_compra",
                schema: "estoque");
        }
    }
}
