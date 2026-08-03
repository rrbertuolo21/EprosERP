using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GestaoClientes.Migrations
{
    /// <inheritdoc />
    public partial class Implanta_1_08D_MudancaPlano : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "cancelada_em",
                schema: "plataforma",
                table: "assinaturas_clientes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cancelada_por",
                schema: "plataforma",
                table: "assinaturas_clientes",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motivo_cancelamento",
                schema: "plataforma",
                table: "assinaturas_clientes",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ajustes_proracao",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    assinatura_cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plano_anterior_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plano_novo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    preco_anterior = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    preco_novo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    dias_ciclo = table.Column<int>(type: "integer", nullable: false),
                    dias_restantes = table.Column<int>(type: "integer", nullable: false),
                    valor_ajuste = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fatura_id = table.Column<Guid>(type: "uuid", nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_ajustes_proracao", x => x.id);
                    table.ForeignKey(
                        name: "f_k_ajustes_proracao__assinaturas_clientes_assinatura_cliente_id",
                        column: x => x.assinatura_cliente_id,
                        principalSchema: "plataforma",
                        principalTable: "assinaturas_clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix__ajuste_proracao_sync_id",
                schema: "plataforma",
                table: "ajustes_proracao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ajuste_proracao_tenant_id",
                schema: "plataforma",
                table: "ajustes_proracao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ajustes_proracao_assinatura",
                schema: "plataforma",
                table: "ajustes_proracao",
                column: "assinatura_cliente_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ajustes_proracao",
                schema: "plataforma");

            migrationBuilder.DropColumn(
                name: "cancelada_em",
                schema: "plataforma",
                table: "assinaturas_clientes");

            migrationBuilder.DropColumn(
                name: "cancelada_por",
                schema: "plataforma",
                table: "assinaturas_clientes");

            migrationBuilder.DropColumn(
                name: "motivo_cancelamento",
                schema: "plataforma",
                table: "assinaturas_clientes");
        }
    }
}
