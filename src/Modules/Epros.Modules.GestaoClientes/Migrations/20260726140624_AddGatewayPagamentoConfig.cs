using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GestaoClientes.Migrations
{
    /// <inheritdoc />
    public partial class AddGatewayPagamentoConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "qr_code",
                schema: "plataforma",
                table: "pagamentos_faturas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "qr_code_base64",
                schema: "plataforma",
                table: "pagamentos_faturas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ticket_url",
                schema: "plataforma",
                table: "pagamentos_faturas",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "configuracoes_gateway_pagamento",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provedor = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ambiente = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    access_token = table.Column<string>(type: "text", nullable: false),
                    public_key = table.Column<string>(type: "text", nullable: true),
                    webhook_secret = table.Column<string>(type: "text", nullable: true),
                    moeda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    notification_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    tenant_alvo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("p_k_configuracoes_gateway_pagamento", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix__configuracao_gateway_pagamento_sync_id",
                schema: "plataforma",
                table: "configuracoes_gateway_pagamento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__configuracao_gateway_pagamento_tenant_id",
                schema: "plataforma",
                table: "configuracoes_gateway_pagamento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_config_gateway_pagamento_tenant_provedor_ativo",
                schema: "plataforma",
                table: "configuracoes_gateway_pagamento",
                columns: new[] { "tenant_alvo", "provedor", "ativo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "configuracoes_gateway_pagamento",
                schema: "plataforma");

            migrationBuilder.DropColumn(
                name: "qr_code",
                schema: "plataforma",
                table: "pagamentos_faturas");

            migrationBuilder.DropColumn(
                name: "qr_code_base64",
                schema: "plataforma",
                table: "pagamentos_faturas");

            migrationBuilder.DropColumn(
                name: "ticket_url",
                schema: "plataforma",
                table: "pagamentos_faturas");
        }
    }
}
