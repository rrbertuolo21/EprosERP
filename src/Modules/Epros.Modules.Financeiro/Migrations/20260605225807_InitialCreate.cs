using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Financeiro.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "financas");

            migrationBuilder.EnsureSchema(
                name: "estoque");

            migrationBuilder.EnsureSchema(
                name: "plataforma");

            migrationBuilder.CreateTable(
                name: "contas_pagar",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    fornecedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    data_vencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_pagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    numero_documento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    categoria = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    historico = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("p_k_contas_pagar", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "contas_receber",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    data_vencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_recebimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    origem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    documento_origem_id = table.Column<Guid>(type: "uuid", nullable: true),
                    numero_documento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    historico = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("p_k_contas_receber", x => x.id);
                });

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS estoque.outbox_messages (
                    id uuid NOT NULL,
                    tenant_id text NOT NULL,
                    event_type text NOT NULL,
                    payload text NOT NULL,
                    criado_em timestamp with time zone NOT NULL,
                    processado_em timestamp with time zone,
                    erro text,
                    tentativas integer NOT NULL,
                    CONSTRAINT p_k_outbox_messages PRIMARY KEY (id)
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS plataforma.pessoas (
                    id uuid NOT NULL,
                    eh_fornecedor boolean NOT NULL,
                    status integer NOT NULL,
                    tipo_pessoa integer NOT NULL,
                    tenant_id text NOT NULL,
                    criado_por text NOT NULL,
                    criado_em timestamp with time zone NOT NULL,
                    deletado_em timestamp with time zone,
                    CONSTRAINT p_k_pessoas PRIMARY KEY (id)
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS plataforma.pessoas_juridicas (
                    pessoa_id uuid NOT NULL,
                    cnpj text NOT NULL,
                    razao_social text NOT NULL,
                    tenant_id text NOT NULL,
                    criado_por text NOT NULL,
                    criado_em timestamp with time zone NOT NULL,
                    deletado_em timestamp with time zone,
                    CONSTRAINT p_k_pessoas_juridicas PRIMARY KEY (pessoa_id)
                );
            ");

            migrationBuilder.CreateTable(
                name: "contas_pagar_baixas",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    conta_pagar_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor_pago = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    data_pagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    forma_pagamento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("p_k_contas_pagar_baixas", x => x.id);
                    table.ForeignKey(
                        name: "f_k_contas_pagar_baixas_contas_pagar_conta_pagar_id",
                        column: x => x.conta_pagar_id,
                        principalSchema: "financas",
                        principalTable: "contas_pagar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contas_receber_baixas",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    conta_receber_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor_recebido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    data_recebimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    forma_pagamento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("p_k_contas_receber_baixas", x => x.id);
                    table.ForeignKey(
                        name: "f_k_contas_receber_baixas_contas_receber_conta_receber_id",
                        column: x => x.conta_receber_id,
                        principalSchema: "financas",
                        principalTable: "contas_receber",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix__conta_pagar_sync_id",
                schema: "financas",
                table: "contas_pagar",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__conta_pagar_tenant_id",
                schema: "financas",
                table: "contas_pagar",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_contas_pagar_tenant_fornecedor",
                schema: "financas",
                table: "contas_pagar",
                columns: new[] { "tenant_id", "fornecedor_id" });

            migrationBuilder.CreateIndex(
                name: "ix_contas_pagar_tenant_vencimento",
                schema: "financas",
                table: "contas_pagar",
                columns: new[] { "tenant_id", "data_vencimento" });

            migrationBuilder.CreateIndex(
                name: "i_x_contas_pagar_baixas_conta_pagar_id",
                schema: "financas",
                table: "contas_pagar_baixas",
                column: "conta_pagar_id");

            migrationBuilder.CreateIndex(
                name: "ix__conta_pagar_baixa_sync_id",
                schema: "financas",
                table: "contas_pagar_baixas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__conta_pagar_baixa_tenant_id",
                schema: "financas",
                table: "contas_pagar_baixas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__conta_receber_sync_id",
                schema: "financas",
                table: "contas_receber",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__conta_receber_tenant_id",
                schema: "financas",
                table: "contas_receber",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_contas_receber_tenant_cliente",
                schema: "financas",
                table: "contas_receber",
                columns: new[] { "tenant_id", "cliente_id" });

            migrationBuilder.CreateIndex(
                name: "ix_contas_receber_tenant_vencimento",
                schema: "financas",
                table: "contas_receber",
                columns: new[] { "tenant_id", "data_vencimento" });

            migrationBuilder.CreateIndex(
                name: "i_x_contas_receber_baixas_conta_receber_id",
                schema: "financas",
                table: "contas_receber_baixas",
                column: "conta_receber_id");

            migrationBuilder.CreateIndex(
                name: "ix__conta_receber_baixa_sync_id",
                schema: "financas",
                table: "contas_receber_baixas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__conta_receber_baixa_tenant_id",
                schema: "financas",
                table: "contas_receber_baixas",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contas_pagar_baixas",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "contas_receber_baixas",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "pessoas",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "pessoas_juridicas",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "contas_pagar",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "contas_receber",
                schema: "financas");
        }
    }
}
