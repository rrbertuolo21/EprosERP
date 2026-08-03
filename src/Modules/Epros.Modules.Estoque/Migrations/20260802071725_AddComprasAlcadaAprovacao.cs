using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Estoque.Migrations
{
    /// <inheritdoc />
    public partial class AddComprasAlcadaAprovacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compras_alcada_regras",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nivel = table.Column<int>(type: "integer", nullable: false),
                    valor_minimo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_maximo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    comprador_id = table.Column<Guid>(type: "uuid", nullable: true),
                    categoria_compra = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    aprovador_id = table.Column<Guid>(type: "uuid", nullable: true),
                    papel_aprovador = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_compras_alcada_regras", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "compras_pedidos_aprovacao",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    origem_tipo = table.Column<int>(type: "integer", nullable: false),
                    origem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    comprador_id = table.Column<Guid>(type: "uuid", nullable: true),
                    categoria_compra = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    nivel_atual = table.Column<int>(type: "integer", nullable: false),
                    quantidade_niveis = table.Column<int>(type: "integer", nullable: false),
                    decidido_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_compras_pedidos_aprovacao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "compras_pedidos_aprovacao_niveis",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pedido_aprovacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nivel = table.Column<int>(type: "integer", nullable: false),
                    aprovador_id = table.Column<Guid>(type: "uuid", nullable: true),
                    papel_aprovador = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    valor_minimo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_maximo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    decidido_por = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    decidido_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    justificativa = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("p_k_compras_pedidos_aprovacao_niveis", x => x.id);
                    table.ForeignKey(
                        name: "f_k_compras_pedidos_aprovacao_niveis_compras_pedidos_aprovacao_~",
                        column: x => x.pedido_aprovacao_id,
                        principalSchema: "estoque",
                        principalTable: "compras_pedidos_aprovacao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_compras_alcada_regras_tenant_id_ativo",
                schema: "estoque",
                table: "compras_alcada_regras",
                columns: new[] { "tenant_id", "ativo" });

            migrationBuilder.CreateIndex(
                name: "i_x_compras_alcada_regras_tenant_id_nivel",
                schema: "estoque",
                table: "compras_alcada_regras",
                columns: new[] { "tenant_id", "nivel" });

            migrationBuilder.CreateIndex(
                name: "ix__compras_alcada_regra_sync_id",
                schema: "estoque",
                table: "compras_alcada_regras",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compras_alcada_regra_tenant_id",
                schema: "estoque",
                table: "compras_alcada_regras",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compras_pedidos_aprovacao_tenant_id_origem_tipo_origem_id",
                schema: "estoque",
                table: "compras_pedidos_aprovacao",
                columns: new[] { "tenant_id", "origem_tipo", "origem_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_compras_pedidos_aprovacao_tenant_id_status",
                schema: "estoque",
                table: "compras_pedidos_aprovacao",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__compras_pedido_aprovacao_sync_id",
                schema: "estoque",
                table: "compras_pedidos_aprovacao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compras_pedido_aprovacao_tenant_id",
                schema: "estoque",
                table: "compras_pedidos_aprovacao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compras_pedidos_aprovacao_niveis_pedido_aprovacao_id",
                schema: "estoque",
                table: "compras_pedidos_aprovacao_niveis",
                column: "pedido_aprovacao_id");

            migrationBuilder.CreateIndex(
                name: "i_x_compras_pedidos_aprovacao_niveis_tenant_id_pedido_aprovacao~",
                schema: "estoque",
                table: "compras_pedidos_aprovacao_niveis",
                columns: new[] { "tenant_id", "pedido_aprovacao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__compras_pedido_aprovacao_nivel_sync_id",
                schema: "estoque",
                table: "compras_pedidos_aprovacao_niveis",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compras_pedido_aprovacao_nivel_tenant_id",
                schema: "estoque",
                table: "compras_pedidos_aprovacao_niveis",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compras_alcada_regras",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "compras_pedidos_aprovacao_niveis",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "compras_pedidos_aprovacao",
                schema: "estoque");
        }
    }
}
