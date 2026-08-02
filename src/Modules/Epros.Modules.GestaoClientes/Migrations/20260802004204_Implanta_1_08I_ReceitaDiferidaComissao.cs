using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GestaoClientes.Migrations
{
    /// <summary>
    /// 1.08I — Migration ADITIVA: tabelas de RECONHECIMENTO DE RECEITA por competência (diferimento anual =
    /// 12 avos, CPC 47) e de APURAÇÃO DE COMISSÃO parametrizável (base/momento = PARÂMETRO, valida contador).
    /// Nenhuma coluna/tabela existente é alterada. A coluna de sistema "xmin" (lock otimista REG-017) NÃO é
    /// materializada pela migration — é a coluna de sistema do PostgreSQL usada como concurrency token.
    /// </summary>
    /// <inheritdoc />
    public partial class Implanta_1_08I_ReceitaDiferidaComissao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "apuracoes_comissao",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fatura_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    revenda_id = table.Column<Guid>(type: "uuid", nullable: true),
                    vendedor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    @base = table.Column<string>(name: "base", type: "character varying(20)", maxLength: 20, nullable: false),
                    momento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    valor_base = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    percentual_revenda = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    percentual_vendedor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_comissao_revenda = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_comissao_vendedor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    competencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    estornada = table.Column<bool>(type: "boolean", nullable: false),
                    estornada_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    motivo_estorno = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_apuracoes_comissao", x => x.id);
                    table.ForeignKey(
                        name: "f_k_apuracoes_comissao__faturas_fatura_id",
                        column: x => x.fatura_id,
                        principalSchema: "plataforma",
                        principalTable: "faturas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reconhecimentos_receita",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fatura_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    competencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    sequencia = table.Column<int>(type: "integer", nullable: false),
                    total_parcelas = table.Column<int>(type: "integer", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    reconhecido_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_reconhecimentos_receita", x => x.id);
                    table.ForeignKey(
                        name: "f_k_reconhecimentos_receita_faturas_fatura_id",
                        column: x => x.fatura_id,
                        principalSchema: "plataforma",
                        principalTable: "faturas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix__apuracao_comissao_sync_id",
                schema: "plataforma",
                table: "apuracoes_comissao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__apuracao_comissao_tenant_id",
                schema: "plataforma",
                table: "apuracoes_comissao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_apuracao_comissao_fatura",
                schema: "plataforma",
                table: "apuracoes_comissao",
                column: "fatura_id");

            migrationBuilder.CreateIndex(
                name: "ix__reconhecimento_receita_sync_id",
                schema: "plataforma",
                table: "reconhecimentos_receita",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__reconhecimento_receita_tenant_id",
                schema: "plataforma",
                table: "reconhecimentos_receita",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_reconhecimento_receita_fatura",
                schema: "plataforma",
                table: "reconhecimentos_receita",
                column: "fatura_id");

            migrationBuilder.CreateIndex(
                name: "ix_reconhecimento_receita_status_competencia",
                schema: "plataforma",
                table: "reconhecimentos_receita",
                columns: new[] { "status", "competencia" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "apuracoes_comissao",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "reconhecimentos_receita",
                schema: "plataforma");
        }
    }
}
