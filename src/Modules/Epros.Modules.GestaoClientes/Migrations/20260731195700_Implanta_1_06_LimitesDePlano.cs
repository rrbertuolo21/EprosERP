using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GestaoClientes.Migrations
{
    /// <inheritdoc />
    public partial class Implanta_1_06_LimitesDePlano : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "dias_tolerancia_inadimplencia",
                schema: "plataforma",
                table: "planos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "limite_clientes",
                schema: "plataforma",
                table: "planos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "cota_clientes",
                schema: "plataforma",
                table: "clientes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "webhook_eventos_processados",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provedor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    evento_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    acao = table.Column<string>(type: "text", nullable: true),
                    processado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_webhook_eventos_processados", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix__webhook_evento_processado_sync_id",
                schema: "plataforma",
                table: "webhook_eventos_processados",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__webhook_evento_processado_tenant_id",
                schema: "plataforma",
                table: "webhook_eventos_processados",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ux__webhook_evento_provedor_evento_id",
                schema: "plataforma",
                table: "webhook_eventos_processados",
                columns: new[] { "provedor", "evento_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "webhook_eventos_processados",
                schema: "plataforma");

            migrationBuilder.DropColumn(
                name: "dias_tolerancia_inadimplencia",
                schema: "plataforma",
                table: "planos");

            migrationBuilder.DropColumn(
                name: "limite_clientes",
                schema: "plataforma",
                table: "planos");

            migrationBuilder.DropColumn(
                name: "cota_clientes",
                schema: "plataforma",
                table: "clientes");
        }
    }
}
