using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GestaoClientes.Migrations
{
    /// <inheritdoc />
    public partial class Implanta_1_08B_CartaoBoleto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "codigo_barras",
                schema: "plataforma",
                table: "pagamentos_faturas",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "data_vencimento_boleto",
                schema: "plataforma",
                table: "pagamentos_faturas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "linha_digitavel",
                schema: "plataforma",
                table: "pagamentos_faturas",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "url_boleto",
                schema: "plataforma",
                table: "pagamentos_faturas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "proxima_cobranca_em",
                schema: "plataforma",
                table: "assinaturas_clientes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ultima_renovacao_em",
                schema: "plataforma",
                table: "assinaturas_clientes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "meios_pagamento_clientes",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    bandeira = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ultimos_quatro = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    validade_mes = table.Column<int>(type: "integer", nullable: true),
                    validade_ano = table.Column<int>(type: "integer", nullable: true),
                    customer_id_gateway = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    card_id_gateway = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    padrao = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_meios_pagamento_clientes", x => x.id);
                    table.ForeignKey(
                        name: "f_k_meios_pagamento_clientes_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalSchema: "plataforma",
                        principalTable: "clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix__meio_pagamento_cliente_sync_id",
                schema: "plataforma",
                table: "meios_pagamento_clientes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__meio_pagamento_cliente_tenant_id",
                schema: "plataforma",
                table: "meios_pagamento_clientes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_meios_pagamento_cliente_cliente_ativo",
                schema: "plataforma",
                table: "meios_pagamento_clientes",
                columns: new[] { "cliente_id", "ativo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "meios_pagamento_clientes",
                schema: "plataforma");

            migrationBuilder.DropColumn(
                name: "codigo_barras",
                schema: "plataforma",
                table: "pagamentos_faturas");

            migrationBuilder.DropColumn(
                name: "data_vencimento_boleto",
                schema: "plataforma",
                table: "pagamentos_faturas");

            migrationBuilder.DropColumn(
                name: "linha_digitavel",
                schema: "plataforma",
                table: "pagamentos_faturas");

            migrationBuilder.DropColumn(
                name: "url_boleto",
                schema: "plataforma",
                table: "pagamentos_faturas");

            migrationBuilder.DropColumn(
                name: "proxima_cobranca_em",
                schema: "plataforma",
                table: "assinaturas_clientes");

            migrationBuilder.DropColumn(
                name: "ultima_renovacao_em",
                schema: "plataforma",
                table: "assinaturas_clientes");
        }
    }
}
