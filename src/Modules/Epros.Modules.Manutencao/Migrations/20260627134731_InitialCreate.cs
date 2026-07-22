using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Manutencao.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "manutencao");

            migrationBuilder.CreateTable(
                name: "equipamentos",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    codigo = table.Column<string>(type: "text", nullable: false),
                    setor = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    data_aquisicao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    criticidade = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_equipamentos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ordens_manutencao",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipamento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    descricao_problema = table.Column<string>(type: "text", nullable: false),
                    descricao_servico_executado = table.Column<string>(type: "text", nullable: false),
                    responsavel = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    data_abertura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_conclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    custo_pecas = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    custo_mao_obra = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_ordens_manutencao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "manutencao",
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
                name: "ordens_manutencao_pecas",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_manutencao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_ordens_manutencao_pecas", x => x.id);
                    table.ForeignKey(
                        name: "f_k_ordens_manutencao_pecas_ordens_manutencao_ordem_manutencao_~",
                        column: x => x.ordem_manutencao_id,
                        principalSchema: "manutencao",
                        principalTable: "ordens_manutencao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_equipamentos_tenant_id_codigo",
                schema: "manutencao",
                table: "equipamentos",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__equipamento_sync_id",
                schema: "manutencao",
                table: "equipamentos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__equipamento_tenant_id",
                schema: "manutencao",
                table: "equipamentos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ordens_manutencao_tenant_id_equipamento_id",
                schema: "manutencao",
                table: "ordens_manutencao",
                columns: new[] { "tenant_id", "equipamento_id" });

            migrationBuilder.CreateIndex(
                name: "ix__ordem_manutencao_sync_id",
                schema: "manutencao",
                table: "ordens_manutencao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ordem_manutencao_tenant_id",
                schema: "manutencao",
                table: "ordens_manutencao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ordens_manutencao_pecas_ordem_manutencao_id",
                schema: "manutencao",
                table: "ordens_manutencao_pecas",
                column: "ordem_manutencao_id");

            migrationBuilder.CreateIndex(
                name: "ix__ordem_manutencao_peca_sync_id",
                schema: "manutencao",
                table: "ordens_manutencao_pecas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ordem_manutencao_peca_tenant_id",
                schema: "manutencao",
                table: "ordens_manutencao_pecas",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "equipamentos",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "ordens_manutencao_pecas",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "ordens_manutencao",
                schema: "manutencao");
        }
    }
}
