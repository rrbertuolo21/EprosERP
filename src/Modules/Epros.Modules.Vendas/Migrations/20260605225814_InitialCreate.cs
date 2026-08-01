using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Vendas.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "vendas");

            migrationBuilder.CreateTable(
                name: "caixa_movimentos",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    caixa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("p_k_caixa_movimentos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "caixas",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    operador_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    saldo_abertura = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    saldo_fechamento = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    diferenca_fechamento = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    fechado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_caixas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vendas",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    caixa_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("p_k_vendas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "venda_itens",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    preco_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_venda_itens", x => x.id);
                    table.ForeignKey(
                        name: "f_k_venda_itens_vendas_venda_id",
                        column: x => x.venda_id,
                        principalSchema: "vendas",
                        principalTable: "vendas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix__caixa_movimento_sync_id",
                schema: "vendas",
                table: "caixa_movimentos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__caixa_movimento_tenant_id",
                schema: "vendas",
                table: "caixa_movimentos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_caixa_movimentos_tenant_caixa",
                schema: "vendas",
                table: "caixa_movimentos",
                columns: new[] { "tenant_id", "caixa_id" });

            migrationBuilder.CreateIndex(
                name: "ix__caixa_sync_id",
                schema: "vendas",
                table: "caixas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__caixa_tenant_id",
                schema: "vendas",
                table: "caixas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_caixas_tenant_operador",
                schema: "vendas",
                table: "caixas",
                columns: new[] { "tenant_id", "operador_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_venda_itens_venda_id",
                schema: "vendas",
                table: "venda_itens",
                column: "venda_id");

            migrationBuilder.CreateIndex(
                name: "ix__venda_item_sync_id",
                schema: "vendas",
                table: "venda_itens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_item_tenant_id",
                schema: "vendas",
                table: "venda_itens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__venda_sync_id",
                schema: "vendas",
                table: "vendas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_tenant_id",
                schema: "vendas",
                table: "vendas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_vendas_tenant_criado_em",
                schema: "vendas",
                table: "vendas",
                columns: new[] { "tenant_id", "criado_em" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "caixa_movimentos",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "caixas",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "venda_itens",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "vendas",
                schema: "vendas");
        }
    }
}
