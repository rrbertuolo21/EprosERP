using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GestaoClientes.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "plataforma");

            migrationBuilder.CreateTable(
                name: "clientes",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    razao_social = table.Column<string>(type: "text", nullable: false),
                    cnpj = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    plano_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_clientes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "faturas",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    data_vencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_pagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_faturas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "planos",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    preco = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_planos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "modulos_plano",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_modulo = table.Column<string>(type: "text", nullable: false),
                    plano_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_modulos_plano", x => x.id);
                    table.ForeignKey(
                        name: "f_k_modulos_plano__planos_plano_id",
                        column: x => x.plano_id,
                        principalSchema: "plataforma",
                        principalTable: "planos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix__cliente_sync_id",
                schema: "plataforma",
                table: "clientes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__cliente_tenant_id",
                schema: "plataforma",
                table: "clientes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__fatura_sync_id",
                schema: "plataforma",
                table: "faturas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fatura_tenant_id",
                schema: "plataforma",
                table: "faturas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_modulos_plano_plano_id",
                schema: "plataforma",
                table: "modulos_plano",
                column: "plano_id");

            migrationBuilder.CreateIndex(
                name: "ix__modulo_plano_sync_id",
                schema: "plataforma",
                table: "modulos_plano",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__modulo_plano_tenant_id",
                schema: "plataforma",
                table: "modulos_plano",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__plano_sync_id",
                schema: "plataforma",
                table: "planos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__plano_tenant_id",
                schema: "plataforma",
                table: "planos",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clientes",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "faturas",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "modulos_plano",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "planos",
                schema: "plataforma");
        }
    }
}
