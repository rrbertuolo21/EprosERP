using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Projetos.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "projetos");

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "projetos",
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
                name: "projetos",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_termino = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    orcamento_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    custo_acumulado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    percentual_conclusao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_projetos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "alocacoes",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    projeto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    funcao = table.Column<string>(type: "text", nullable: false),
                    custo_hora = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    horas_planejadas = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_alocacoes", x => x.id);
                    table.ForeignKey(
                        name: "f_k_alocacoes_projetos_projeto_id",
                        column: x => x.projeto_id,
                        principalSchema: "projetos",
                        principalTable: "projetos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wbs_itens",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    projeto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_termino = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    percentual_conclusao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    peso_ponderado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_wbs_itens", x => x.id);
                    table.ForeignKey(
                        name: "f_k_wbs_itens_projetos_projeto_id",
                        column: x => x.projeto_id,
                        principalSchema: "projetos",
                        principalTable: "projetos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_alocacoes_projeto_id",
                schema: "projetos",
                table: "alocacoes",
                column: "projeto_id");

            migrationBuilder.CreateIndex(
                name: "i_x_alocacoes_tenant_id_colaborador_id",
                schema: "projetos",
                table: "alocacoes",
                columns: new[] { "tenant_id", "colaborador_id" });

            migrationBuilder.CreateIndex(
                name: "ix__alocacao_recurso_sync_id",
                schema: "projetos",
                table: "alocacoes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__alocacao_recurso_tenant_id",
                schema: "projetos",
                table: "alocacoes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_projetos_tenant_id_cliente_id",
                schema: "projetos",
                table: "projetos",
                columns: new[] { "tenant_id", "cliente_id" });

            migrationBuilder.CreateIndex(
                name: "ix__projeto_sync_id",
                schema: "projetos",
                table: "projetos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__projeto_tenant_id",
                schema: "projetos",
                table: "projetos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_wbs_itens_projeto_id",
                schema: "projetos",
                table: "wbs_itens",
                column: "projeto_id");

            migrationBuilder.CreateIndex(
                name: "ix__wbs_item_sync_id",
                schema: "projetos",
                table: "wbs_itens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__wbs_item_tenant_id",
                schema: "projetos",
                table: "wbs_itens",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alocacoes",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "wbs_itens",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "projetos",
                schema: "projetos");
        }
    }
}
