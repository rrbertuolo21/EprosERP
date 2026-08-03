using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Estoque.Migrations
{
    /// <inheritdoc />
    public partial class AddRastreabilidadeLoteEstoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bloqueios_lote",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lote_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_bloqueio = table.Column<int>(type: "integer", nullable: false),
                    motivo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    data_bloqueio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_desbloqueio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    motivo_desbloqueio = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("p_k_bloqueios_lote", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "lotes_estoque",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    local_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ficha_entrada_id = table.Column<Guid>(type: "uuid", nullable: true),
                    codigo_lote = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    data_fabricacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_validade = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_recebimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    origem = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    observacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    quantidade_recebida = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantidade_disponivel = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantidade_bloqueada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantidade_consumida = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_lotes_estoque", x => x.id);
                    table.ForeignKey(
                        name: "f_k_lotes_estoque__produtos_produto_id",
                        column: x => x.produto_id,
                        principalSchema: "estoque",
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "numeros_seriais",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lote_id = table.Column<Guid>(type: "uuid", nullable: true),
                    numero = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    ficha_entrada_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ficha_saida_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_numeros_seriais", x => x.id);
                    table.ForeignKey(
                        name: "f_k_numeros_seriais__produtos_produto_id",
                        column: x => x.produto_id,
                        principalSchema: "estoque",
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "recalls_lote",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lote_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_recall = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    motivo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    data_abertura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_encerramento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_recalls_lote", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_bloqueios_lote_tenant_id_lote_id",
                schema: "estoque",
                table: "bloqueios_lote",
                columns: new[] { "tenant_id", "lote_id" });

            migrationBuilder.CreateIndex(
                name: "ix__bloqueio_lote_sync_id",
                schema: "estoque",
                table: "bloqueios_lote",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__bloqueio_lote_tenant_id",
                schema: "estoque",
                table: "bloqueios_lote",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_lotes_estoque_produto_id",
                schema: "estoque",
                table: "lotes_estoque",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "i_x_lotes_estoque_tenant_id_produto_id_codigo_lote",
                schema: "estoque",
                table: "lotes_estoque",
                columns: new[] { "tenant_id", "produto_id", "codigo_lote" });

            migrationBuilder.CreateIndex(
                name: "i_x_lotes_estoque_tenant_id_status",
                schema: "estoque",
                table: "lotes_estoque",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__lote_estoque_sync_id",
                schema: "estoque",
                table: "lotes_estoque",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__lote_estoque_tenant_id",
                schema: "estoque",
                table: "lotes_estoque",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_numeros_seriais_produto_id",
                schema: "estoque",
                table: "numeros_seriais",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "i_x_numeros_seriais_tenant_id_produto_id_numero",
                schema: "estoque",
                table: "numeros_seriais",
                columns: new[] { "tenant_id", "produto_id", "numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_numeros_seriais_tenant_id_status",
                schema: "estoque",
                table: "numeros_seriais",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__numero_serial_sync_id",
                schema: "estoque",
                table: "numeros_seriais",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__numero_serial_tenant_id",
                schema: "estoque",
                table: "numeros_seriais",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_recalls_lote_tenant_id_lote_id",
                schema: "estoque",
                table: "recalls_lote",
                columns: new[] { "tenant_id", "lote_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_recalls_lote_tenant_id_status",
                schema: "estoque",
                table: "recalls_lote",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__recall_lote_sync_id",
                schema: "estoque",
                table: "recalls_lote",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__recall_lote_tenant_id",
                schema: "estoque",
                table: "recalls_lote",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bloqueios_lote",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "lotes_estoque",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "numeros_seriais",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "recalls_lote",
                schema: "estoque");
        }
    }
}
