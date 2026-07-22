using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Financeiro.Migrations
{
    /// <inheritdoc />
    public partial class AddBancoAndContaBancariaAndCartao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bancos",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    descricao = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
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
                    table.PrimaryKey("p_k_bancos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "contas_bancarias",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    banco_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_conta_bancaria = table.Column<int>(type: "integer", nullable: false),
                    apelido = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    titular = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    agencia = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    conta = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    gerente = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    fone_gerente = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    detalhe = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    digito_agencia = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    data_encerramento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_contas_bancarias", x => x.id);
                    table.ForeignKey(
                        name: "f_k_contas_bancarias_bancos_banco_id",
                        column: x => x.banco_id,
                        principalSchema: "financas",
                        principalTable: "bancos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cartoes_de_credito",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    conta_bancaria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    apelido = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    titular = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    bandeira_cartao = table.Column<int>(type: "integer", nullable: false),
                    observacao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("p_k_cartoes_de_credito", x => x.id);
                    table.ForeignKey(
                        name: "f_k_cartoes_de_credito__contas_bancarias_conta_bancaria_id",
                        column: x => x.conta_bancaria_id,
                        principalSchema: "financas",
                        principalTable: "contas_bancarias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cartoes_de_credito_faturas",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cartao_de_credito_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_lancamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_vencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    pago = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_cartoes_de_credito_faturas", x => x.id);
                    table.ForeignKey(
                        name: "f_k_cartoes_de_credito_faturas_cartoes_de_credito_cartao_de_cre~",
                        column: x => x.cartao_de_credito_id,
                        principalSchema: "financas",
                        principalTable: "cartoes_de_credito",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_bancos_tenant_id_codigo",
                schema: "financas",
                table: "bancos",
                columns: new[] { "tenant_id", "codigo" });

            migrationBuilder.CreateIndex(
                name: "ix__banco_sync_id",
                schema: "financas",
                table: "bancos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__banco_tenant_id",
                schema: "financas",
                table: "bancos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_cartoes_de_credito_conta_bancaria_id",
                schema: "financas",
                table: "cartoes_de_credito",
                column: "conta_bancaria_id");

            migrationBuilder.CreateIndex(
                name: "i_x_cartoes_de_credito_tenant_id_apelido",
                schema: "financas",
                table: "cartoes_de_credito",
                columns: new[] { "tenant_id", "apelido" });

            migrationBuilder.CreateIndex(
                name: "ix__cartao_de_credito_sync_id",
                schema: "financas",
                table: "cartoes_de_credito",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__cartao_de_credito_tenant_id",
                schema: "financas",
                table: "cartoes_de_credito",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_cartoes_de_credito_faturas_cartao_de_credito_id",
                schema: "financas",
                table: "cartoes_de_credito_faturas",
                column: "cartao_de_credito_id");

            migrationBuilder.CreateIndex(
                name: "i_x_cartoes_de_credito_faturas_tenant_id_cartao_de_credito_id",
                schema: "financas",
                table: "cartoes_de_credito_faturas",
                columns: new[] { "tenant_id", "cartao_de_credito_id" });

            migrationBuilder.CreateIndex(
                name: "ix__cartao_de_credito_fatura_sync_id",
                schema: "financas",
                table: "cartoes_de_credito_faturas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__cartao_de_credito_fatura_tenant_id",
                schema: "financas",
                table: "cartoes_de_credito_faturas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_contas_bancarias_banco_id",
                schema: "financas",
                table: "contas_bancarias",
                column: "banco_id");

            migrationBuilder.CreateIndex(
                name: "i_x_contas_bancarias_tenant_id_conta",
                schema: "financas",
                table: "contas_bancarias",
                columns: new[] { "tenant_id", "conta" });

            migrationBuilder.CreateIndex(
                name: "ix__conta_bancaria_sync_id",
                schema: "financas",
                table: "contas_bancarias",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__conta_bancaria_tenant_id",
                schema: "financas",
                table: "contas_bancarias",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cartoes_de_credito_faturas",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "cartoes_de_credito",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "contas_bancarias",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "bancos",
                schema: "financas");
        }
    }
}
