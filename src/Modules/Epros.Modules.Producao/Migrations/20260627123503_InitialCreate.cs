using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Producao.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "producao");

            migrationBuilder.CreateTable(
                name: "listas_materiais",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_acabado_sku = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    versao = table.Column<string>(type: "text", nullable: false),
                    ativa = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_listas_materiais", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ordens_producao",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "text", nullable: false),
                    produto_acabado_sku = table.Column<string>(type: "text", nullable: false),
                    quantidade_planejada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantidade_produzida = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantidade_refugada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    data_abertura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    custo_total_producao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_ordens_producao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    event_type = table.Column<string>(type: "text", nullable: false),
                    payload = table.Column<string>(type: "text", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    erro = table.Column<string>(type: "text", nullable: true),
                    tentativas = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bom_itens",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lista_materiais_id = table.Column<Guid>(type: "uuid", nullable: false),
                    insumo_sku = table.Column<string>(type: "text", nullable: false),
                    quantidade_necessaria = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    unidade_medida = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_bom_itens", x => x.id);
                    table.ForeignKey(
                        name: "f_k_bom_itens_listas_materiais_lista_materiais_id",
                        column: x => x.lista_materiais_id,
                        principalSchema: "producao",
                        principalTable: "listas_materiais",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "apontamentos",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_producao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade_apontada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantidade_refugada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    operador = table.Column<string>(type: "text", nullable: false),
                    data_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_apontamentos", x => x.id);
                    table.ForeignKey(
                        name: "f_k_apontamentos_ordens_producao_ordem_producao_id",
                        column: x => x.ordem_producao_id,
                        principalSchema: "producao",
                        principalTable: "ordens_producao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_apontamentos_ordem_producao_id",
                schema: "producao",
                table: "apontamentos",
                column: "ordem_producao_id");

            migrationBuilder.CreateIndex(
                name: "i_x_apontamentos_tenant_id_ordem_producao_id",
                schema: "producao",
                table: "apontamentos",
                columns: new[] { "tenant_id", "ordem_producao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__apontamento_producao_sync_id",
                schema: "producao",
                table: "apontamentos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__apontamento_producao_tenant_id",
                schema: "producao",
                table: "apontamentos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_bom_itens_lista_materiais_id",
                schema: "producao",
                table: "bom_itens",
                column: "lista_materiais_id");

            migrationBuilder.CreateIndex(
                name: "i_x_bom_itens_tenant_id_insumo_sku",
                schema: "producao",
                table: "bom_itens",
                columns: new[] { "tenant_id", "insumo_sku" });

            migrationBuilder.CreateIndex(
                name: "ix__bom_item_sync_id",
                schema: "producao",
                table: "bom_itens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__bom_item_tenant_id",
                schema: "producao",
                table: "bom_itens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_listas_materiais_tenant_id_produto_acabado_sku",
                schema: "producao",
                table: "listas_materiais",
                columns: new[] { "tenant_id", "produto_acabado_sku" });

            migrationBuilder.CreateIndex(
                name: "ix__lista_materiais_sync_id",
                schema: "producao",
                table: "listas_materiais",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__lista_materiais_tenant_id",
                schema: "producao",
                table: "listas_materiais",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ordens_producao_tenant_id_codigo",
                schema: "producao",
                table: "ordens_producao",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_ordens_producao_tenant_id_produto_acabado_sku",
                schema: "producao",
                table: "ordens_producao",
                columns: new[] { "tenant_id", "produto_acabado_sku" });

            migrationBuilder.CreateIndex(
                name: "ix__ordem_producao_sync_id",
                schema: "producao",
                table: "ordens_producao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ordem_producao_tenant_id",
                schema: "producao",
                table: "ordens_producao",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "apontamentos",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "bom_itens",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "ordens_producao",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "listas_materiais",
                schema: "producao");
        }
    }
}
