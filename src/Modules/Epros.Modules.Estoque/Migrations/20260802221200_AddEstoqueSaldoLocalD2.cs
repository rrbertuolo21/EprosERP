using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Estoque.Migrations
{
    /// <inheritdoc />
    public partial class AddEstoqueSaldoLocalD2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "estoque_saldos_locais",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    local_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_lote = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    numero_serie = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    quantidade_saldo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantidade_reservada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_saldo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_custo_medio = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    data_validade = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_estoque_saldos_locais", x => x.id);
                    table.ForeignKey(
                        name: "f_k_estoque_saldos_locais__produtos_produto_id",
                        column: x => x.produto_id,
                        principalSchema: "estoque",
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_estoque_saldos_locais_produto_id",
                schema: "estoque",
                table: "estoque_saldos_locais",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "i_x_estoque_saldos_locais_tenant_id_empresa_id_produto_id_local~",
                schema: "estoque",
                table: "estoque_saldos_locais",
                columns: new[] { "tenant_id", "empresa_id", "produto_id", "local_id", "codigo_lote", "numero_serie" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_estoque_saldos_locais_tenant_id_produto_id_data_validade",
                schema: "estoque",
                table: "estoque_saldos_locais",
                columns: new[] { "tenant_id", "produto_id", "data_validade" });

            migrationBuilder.CreateIndex(
                name: "ix__estoque_saldo_local_sync_id",
                schema: "estoque",
                table: "estoque_saldos_locais",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__estoque_saldo_local_tenant_id",
                schema: "estoque",
                table: "estoque_saldos_locais",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "estoque_saldos_locais",
                schema: "estoque");
        }
    }
}
