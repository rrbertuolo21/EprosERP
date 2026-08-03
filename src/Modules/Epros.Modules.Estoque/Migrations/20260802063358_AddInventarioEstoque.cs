using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Estoque.Migrations
{
    /// <inheritdoc />
    public partial class AddInventarioEstoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inventario_movimento_ajustes",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    inventario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    inventario_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fato_gerador_estoque_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_aplicacao = table.Column<int>(type: "integer", nullable: false),
                    quantidade_aplicada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_inventario_movimento_ajustes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inventario_reajustes",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_reajuste = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    porcentagem = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    tipo_reajuste = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    situacao = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_inventario_reajustes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inventarios",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_contagem = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tipo_inventario = table.Column<int>(type: "integer", nullable: false),
                    estoque_atualizado = table.Column<bool>(type: "boolean", nullable: false),
                    situacao = table.Column<int>(type: "integer", nullable: false),
                    acuracidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    observacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    motivo_cancelamento = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("p_k_inventarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inventario_reajuste_itens",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reajuste_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor_original = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_reajuste = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_inventario_reajuste_itens", x => x.id);
                    table.ForeignKey(
                        name: "f_k_inventario_reajuste_itens__produtos_produto_id",
                        column: x => x.produto_id,
                        principalSchema: "estoque",
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_inventario_reajuste_itens_inventario_reajustes_reajuste_id",
                        column: x => x.reajuste_id,
                        principalSchema: "estoque",
                        principalTable: "inventario_reajustes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "inventario_itens",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    inventario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    local_id = table.Column<Guid>(type: "uuid", nullable: true),
                    lote = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    quantidade_sistema = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    contagem01 = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    contagem02 = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    contagem03 = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    quantidade_contada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    fechado_contagem = table.Column<bool>(type: "boolean", nullable: false),
                    divergencia = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_inventario_itens", x => x.id);
                    table.ForeignKey(
                        name: "f_k_inventario_itens__produtos_produto_id",
                        column: x => x.produto_id,
                        principalSchema: "estoque",
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_inventario_itens_inventarios_inventario_id",
                        column: x => x.inventario_id,
                        principalSchema: "estoque",
                        principalTable: "inventarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_inventario_itens_inventario_id",
                schema: "estoque",
                table: "inventario_itens",
                column: "inventario_id");

            migrationBuilder.CreateIndex(
                name: "i_x_inventario_itens_produto_id",
                schema: "estoque",
                table: "inventario_itens",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "i_x_inventario_itens_tenant_id_inventario_id",
                schema: "estoque",
                table: "inventario_itens",
                columns: new[] { "tenant_id", "inventario_id" });

            migrationBuilder.CreateIndex(
                name: "ix__inventario_item_sync_id",
                schema: "estoque",
                table: "inventario_itens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__inventario_item_tenant_id",
                schema: "estoque",
                table: "inventario_itens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_inventario_movimento_ajustes_tenant_id_inventario_id",
                schema: "estoque",
                table: "inventario_movimento_ajustes",
                columns: new[] { "tenant_id", "inventario_id" });

            migrationBuilder.CreateIndex(
                name: "ix__inventario_movimento_ajuste_sync_id",
                schema: "estoque",
                table: "inventario_movimento_ajustes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__inventario_movimento_ajuste_tenant_id",
                schema: "estoque",
                table: "inventario_movimento_ajustes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_inventario_reajuste_itens_produto_id",
                schema: "estoque",
                table: "inventario_reajuste_itens",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "i_x_inventario_reajuste_itens_reajuste_id",
                schema: "estoque",
                table: "inventario_reajuste_itens",
                column: "reajuste_id");

            migrationBuilder.CreateIndex(
                name: "i_x_inventario_reajuste_itens_tenant_id_reajuste_id",
                schema: "estoque",
                table: "inventario_reajuste_itens",
                columns: new[] { "tenant_id", "reajuste_id" });

            migrationBuilder.CreateIndex(
                name: "ix__inventario_reajuste_item_sync_id",
                schema: "estoque",
                table: "inventario_reajuste_itens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__inventario_reajuste_item_tenant_id",
                schema: "estoque",
                table: "inventario_reajuste_itens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_inventario_reajustes_tenant_id_colaborador_id",
                schema: "estoque",
                table: "inventario_reajustes",
                columns: new[] { "tenant_id", "colaborador_id" });

            migrationBuilder.CreateIndex(
                name: "ix__inventario_reajuste_sync_id",
                schema: "estoque",
                table: "inventario_reajustes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__inventario_reajuste_tenant_id",
                schema: "estoque",
                table: "inventario_reajustes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_inventarios_tenant_id_empresa_id",
                schema: "estoque",
                table: "inventarios",
                columns: new[] { "tenant_id", "empresa_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_inventarios_tenant_id_situacao",
                schema: "estoque",
                table: "inventarios",
                columns: new[] { "tenant_id", "situacao" });

            migrationBuilder.CreateIndex(
                name: "ix__inventario_sync_id",
                schema: "estoque",
                table: "inventarios",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__inventario_tenant_id",
                schema: "estoque",
                table: "inventarios",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inventario_itens",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "inventario_movimento_ajustes",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "inventario_reajuste_itens",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "inventarios",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "inventario_reajustes",
                schema: "estoque");

        }
    }
}
