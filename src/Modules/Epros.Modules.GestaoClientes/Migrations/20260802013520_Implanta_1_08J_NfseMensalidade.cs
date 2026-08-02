using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GestaoClientes.Migrations
{
    /// <summary>
    /// 1.08J — Migration ADITIVA: tabela de NFS-e da mensalidade SaaS por competência
    /// (<c>nfse_mensalidades</c>) — MECANISMO/HOOK que registra a necessidade de emitir a NFS-e do serviço
    /// prestado pela Siser (ISS, LC 116/2003 item 1.05/1.03) quando uma fatura de assinatura é paga. 1 por
    /// (fatura, competência), idempotente (índice único). Nenhuma coluna/tabela existente é alterada.
    /// A coluna de sistema "xmin" (lock otimista REG-017) NÃO é materializada pela migration — é a coluna de
    /// sistema do PostgreSQL usada como concurrency token. ⛔ A emissão REAL (alíquota/subitem/município/
    /// certificado/provedor) é dependência do overlay `negocio-siser` + contador + infra; o mecanismo apenas
    /// prepara e mantém Pendente.
    /// </summary>
    /// <inheritdoc />
    public partial class Implanta_1_08J_NfseMensalidade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nfse_mensalidades",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fatura_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    competencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valor_base = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ambiente = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    motivo = table.Column<string>(type: "text", nullable: true),
                    numero_nfse = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    emitida_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_nfse_mensalidades", x => x.id);
                    table.ForeignKey(
                        name: "f_k_nfse_mensalidades_faturas_fatura_id",
                        column: x => x.fatura_id,
                        principalSchema: "plataforma",
                        principalTable: "faturas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix__nfse_mensalidade_sync_id",
                schema: "plataforma",
                table: "nfse_mensalidades",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__nfse_mensalidade_tenant_id",
                schema: "plataforma",
                table: "nfse_mensalidades",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_nfse_mensalidade_fatura_competencia",
                schema: "plataforma",
                table: "nfse_mensalidades",
                columns: new[] { "fatura_id", "competencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_nfse_mensalidade_status_competencia",
                schema: "plataforma",
                table: "nfse_mensalidades",
                columns: new[] { "status", "competencia" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nfse_mensalidades",
                schema: "plataforma");
        }
    }
}
