using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Financeiro.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanoDeContasAndNatureza : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // plataforma.pessoas pertence ao módulo GestaoClientes (Lookup no Financeiro).
            // Operação idempotente para não colidir quando a tabela já foi criada por GestaoClientes.
            migrationBuilder.Sql(
                "ALTER TABLE plataforma.pessoas ADD COLUMN IF NOT EXISTS eh_cliente boolean NOT NULL DEFAULT false;");

            migrationBuilder.AddColumn<Guid>(
                name: "centro_custo_projeto_id",
                schema: "financas",
                table: "contas_receber",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "natureza_financeira_id",
                schema: "financas",
                table: "contas_receber",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "plano_de_contas_financeiro_item_id",
                schema: "financas",
                table: "contas_receber",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "centro_custo_projeto_id",
                schema: "financas",
                table: "contas_pagar",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "natureza_financeira_id",
                schema: "financas",
                table: "contas_pagar",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "plano_de_contas_financeiro_item_id",
                schema: "financas",
                table: "contas_pagar",
                type: "uuid",
                nullable: true);

            // plataforma.pessoas_fisicas pertence ao módulo GestaoClientes (Lookup no Financeiro).
            // Criação idempotente para não colidir quando a tabela já foi criada por GestaoClientes.
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS plataforma.pessoas_fisicas (
                    pessoa_id uuid NOT NULL,
                    cpf text NOT NULL,
                    nome text NOT NULL,
                    tenant_id text NOT NULL,
                    criado_por text NOT NULL,
                    criado_em timestamp with time zone NOT NULL,
                    deletado_em timestamp with time zone,
                    CONSTRAINT p_k_pessoas_fisicas PRIMARY KEY (pessoa_id)
                );
            ");

            migrationBuilder.CreateTable(
                name: "naturezas_financeiras",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    tipo_configuracao_natureza = table.Column<int>(type: "integer", nullable: false),
                    item_plano_de_contas_financeiro_dinheiro_id = table.Column<Guid>(type: "uuid", nullable: true),
                    item_plano_de_contas_financeiro_cartao_cheque_id = table.Column<Guid>(type: "uuid", nullable: true),
                    item_plano_de_contas_financeiro_cartao_credito_id = table.Column<Guid>(type: "uuid", nullable: true),
                    item_plano_de_contas_financeiro_cartao_debito_id = table.Column<Guid>(type: "uuid", nullable: true),
                    item_plano_de_contas_financeiro_cartao_da_loja_id = table.Column<Guid>(type: "uuid", nullable: true),
                    item_plano_de_contas_financeiro_vale_alimentacao_id = table.Column<Guid>(type: "uuid", nullable: true),
                    item_plano_de_contas_financeiro_vale_refeicao_id = table.Column<Guid>(type: "uuid", nullable: true),
                    item_plano_de_contas_financeiro_vale_presente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    item_plano_de_contas_financeiro_vale_combustivel_id = table.Column<Guid>(type: "uuid", nullable: true),
                    item_plano_de_contas_financeiro_duplicata_mercantil_id = table.Column<Guid>(type: "uuid", nullable: true),
                    item_plano_de_contas_financeiro_boleto_bancario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    item_plano_de_contas_financeiro_deposito_bancario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    item_plano_de_contas_financeiro_pagamento_instantaneo_pix_dinamico_id = table.Column<Guid>(type: "uuid", nullable: true),
                    item_plano_de_contas_financeiro_transferencia_bancaria_id = table.Column<Guid>(type: "uuid", nullable: true),
                    item_plano_de_contas_financeiro_programa_de_fidelidade_id = table.Column<Guid>(type: "uuid", nullable: true),
                    item_plano_de_contas_financeiro_pagamento_instantaneo_pix_estatico_id = table.Column<Guid>(type: "uuid", nullable: true),
                    item_plano_de_contas_financeiro_credito_em_loja_id = table.Column<Guid>(type: "uuid", nullable: true),
                    item_plano_de_contas_financeiro_pagamento_eletronico_nao_informado_id = table.Column<Guid>(type: "uuid", nullable: true),
                    item_plano_de_contas_financeiro_outros_id = table.Column<Guid>(type: "uuid", nullable: true),
                    item_plano_de_contas_financeiro_desconto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    item_plano_de_contas_financeiro_acrescimo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    item_plano_de_contas_financeiro_juros_id = table.Column<Guid>(type: "uuid", nullable: true),
                    item_plano_de_contas_financeiro_multa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    item_plano_de_contas_financeiro_troco_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_naturezas_financeiras", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "planos_de_contas",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    configuracao_codigo_natureza_financeira_recebimento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    configuracao_codigo_natureza_financeira_pagamento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    descricao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    mascara = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    eh_padrao = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_planos_de_contas", x => x.id);
                    table.ForeignKey(
                        name: "fk_plano_natureza_pagamento",
                        column: x => x.configuracao_codigo_natureza_financeira_pagamento_id,
                        principalSchema: "financas",
                        principalTable: "naturezas_financeiras",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_plano_natureza_recebimento",
                        column: x => x.configuracao_codigo_natureza_financeira_recebimento_id,
                        principalSchema: "financas",
                        principalTable: "naturezas_financeiras",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "plano_de_contas_itens",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    plano_de_contas_financeiro_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    descricao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tipo_detalhamento = table.Column<int>(type: "integer", nullable: false),
                    movimenta_caixa = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_plano_de_contas_itens", x => x.id);
                    table.ForeignKey(
                        name: "f_k_plano_de_contas_itens_planos_de_contas_plano_de_contas_fina~",
                        column: x => x.plano_de_contas_financeiro_id,
                        principalSchema: "financas",
                        principalTable: "planos_de_contas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_contas_receber_natureza_financeira_id",
                schema: "financas",
                table: "contas_receber",
                column: "natureza_financeira_id");

            migrationBuilder.CreateIndex(
                name: "i_x_contas_receber_plano_de_contas_financeiro_item_id",
                schema: "financas",
                table: "contas_receber",
                column: "plano_de_contas_financeiro_item_id");

            migrationBuilder.CreateIndex(
                name: "i_x_contas_pagar_natureza_financeira_id",
                schema: "financas",
                table: "contas_pagar",
                column: "natureza_financeira_id");

            migrationBuilder.CreateIndex(
                name: "i_x_contas_pagar_plano_de_contas_financeiro_item_id",
                schema: "financas",
                table: "contas_pagar",
                column: "plano_de_contas_financeiro_item_id");

            migrationBuilder.CreateIndex(
                name: "i_x_naturezas_financeiras_tenant_id_descricao",
                schema: "financas",
                table: "naturezas_financeiras",
                columns: new[] { "tenant_id", "descricao" });

            migrationBuilder.CreateIndex(
                name: "ix__configuracao_codigo_natureza_financeira_sync_id",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__configuracao_codigo_natureza_financeira_tenant_id",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_nat_acrescimo",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_acrescimo_id");

            migrationBuilder.CreateIndex(
                name: "ix_nat_boleto",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_boleto_bancario_id");

            migrationBuilder.CreateIndex(
                name: "ix_nat_cartao_loja",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_cartao_da_loja_id");

            migrationBuilder.CreateIndex(
                name: "ix_nat_cheque",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_cartao_cheque_id");

            migrationBuilder.CreateIndex(
                name: "ix_nat_credito",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_cartao_credito_id");

            migrationBuilder.CreateIndex(
                name: "ix_nat_credito_loja",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_credito_em_loja_id");

            migrationBuilder.CreateIndex(
                name: "ix_nat_debito",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_cartao_debito_id");

            migrationBuilder.CreateIndex(
                name: "ix_nat_deposito",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_deposito_bancario_id");

            migrationBuilder.CreateIndex(
                name: "ix_nat_desconto",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_desconto_id");

            migrationBuilder.CreateIndex(
                name: "ix_nat_dinheiro",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_dinheiro_id");

            migrationBuilder.CreateIndex(
                name: "ix_nat_duplicata",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_duplicata_mercantil_id");

            migrationBuilder.CreateIndex(
                name: "ix_nat_fidelidade",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_programa_de_fidelidade_id");

            migrationBuilder.CreateIndex(
                name: "ix_nat_juros",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_juros_id");

            migrationBuilder.CreateIndex(
                name: "ix_nat_multa",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_multa_id");

            migrationBuilder.CreateIndex(
                name: "ix_nat_outros",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_outros_id");

            migrationBuilder.CreateIndex(
                name: "ix_nat_pag_eletronico",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_pagamento_eletronico_nao_informado_id");

            migrationBuilder.CreateIndex(
                name: "ix_nat_pix_dinamico",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_pagamento_instantaneo_pix_dinamico_id");

            migrationBuilder.CreateIndex(
                name: "ix_nat_pix_estatico",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_pagamento_instantaneo_pix_estatico_id");

            migrationBuilder.CreateIndex(
                name: "ix_nat_transferencia",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_transferencia_bancaria_id");

            migrationBuilder.CreateIndex(
                name: "ix_nat_troco",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_troco_id");

            migrationBuilder.CreateIndex(
                name: "ix_nat_vale_alimentacao",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_vale_alimentacao_id");

            migrationBuilder.CreateIndex(
                name: "ix_nat_vale_combustivel",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_vale_combustivel_id");

            migrationBuilder.CreateIndex(
                name: "ix_nat_vale_presente",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_vale_presente_id");

            migrationBuilder.CreateIndex(
                name: "ix_nat_vale_refeicao",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_vale_refeicao_id");

            migrationBuilder.CreateIndex(
                name: "i_x_plano_de_contas_itens_plano_de_contas_financeiro_id",
                schema: "financas",
                table: "plano_de_contas_itens",
                column: "plano_de_contas_financeiro_id");

            migrationBuilder.CreateIndex(
                name: "i_x_plano_de_contas_itens_tenant_id_codigo",
                schema: "financas",
                table: "plano_de_contas_itens",
                columns: new[] { "tenant_id", "codigo" });

            migrationBuilder.CreateIndex(
                name: "ix__plano_de_contas_financeiro_item_sync_id",
                schema: "financas",
                table: "plano_de_contas_itens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__plano_de_contas_financeiro_item_tenant_id",
                schema: "financas",
                table: "plano_de_contas_itens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_planos_de_contas_tenant_id_descricao",
                schema: "financas",
                table: "planos_de_contas",
                columns: new[] { "tenant_id", "descricao" });

            migrationBuilder.CreateIndex(
                name: "ix__plano_de_contas_financeiro_sync_id",
                schema: "financas",
                table: "planos_de_contas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__plano_de_contas_financeiro_tenant_id",
                schema: "financas",
                table: "planos_de_contas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_plano_nat_pagamento",
                schema: "financas",
                table: "planos_de_contas",
                column: "configuracao_codigo_natureza_financeira_pagamento_id");

            migrationBuilder.CreateIndex(
                name: "ix_plano_nat_recebimento",
                schema: "financas",
                table: "planos_de_contas",
                column: "configuracao_codigo_natureza_financeira_recebimento_id");

            migrationBuilder.AddForeignKey(
                name: "f_k_contas_pagar__plano_de_contas_itens_plano_de_contas_financeiro_~",
                schema: "financas",
                table: "contas_pagar",
                column: "plano_de_contas_financeiro_item_id",
                principalSchema: "financas",
                principalTable: "plano_de_contas_itens",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "f_k_contas_pagar_naturezas_financeiras_natureza_financeira_id",
                schema: "financas",
                table: "contas_pagar",
                column: "natureza_financeira_id",
                principalSchema: "financas",
                principalTable: "naturezas_financeiras",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "f_k_contas_receber__plano_de_contas_itens_plano_de_contas_financeir~",
                schema: "financas",
                table: "contas_receber",
                column: "plano_de_contas_financeiro_item_id",
                principalSchema: "financas",
                principalTable: "plano_de_contas_itens",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "f_k_contas_receber_naturezas_financeiras_natureza_financeira_id",
                schema: "financas",
                table: "contas_receber",
                column: "natureza_financeira_id",
                principalSchema: "financas",
                principalTable: "naturezas_financeiras",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_natureza_acrescimo",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_acrescimo_id",
                principalSchema: "financas",
                principalTable: "plano_de_contas_itens",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_natureza_boleto",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_boleto_bancario_id",
                principalSchema: "financas",
                principalTable: "plano_de_contas_itens",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_natureza_cartao_loja",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_cartao_da_loja_id",
                principalSchema: "financas",
                principalTable: "plano_de_contas_itens",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_natureza_cheque",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_cartao_cheque_id",
                principalSchema: "financas",
                principalTable: "plano_de_contas_itens",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_natureza_credito",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_cartao_credito_id",
                principalSchema: "financas",
                principalTable: "plano_de_contas_itens",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_natureza_credito_loja",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_credito_em_loja_id",
                principalSchema: "financas",
                principalTable: "plano_de_contas_itens",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_natureza_debito",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_cartao_debito_id",
                principalSchema: "financas",
                principalTable: "plano_de_contas_itens",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_natureza_deposito",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_deposito_bancario_id",
                principalSchema: "financas",
                principalTable: "plano_de_contas_itens",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_natureza_desconto",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_desconto_id",
                principalSchema: "financas",
                principalTable: "plano_de_contas_itens",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_natureza_dinheiro",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_dinheiro_id",
                principalSchema: "financas",
                principalTable: "plano_de_contas_itens",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_natureza_duplicata",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_duplicata_mercantil_id",
                principalSchema: "financas",
                principalTable: "plano_de_contas_itens",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_natureza_fidelidade",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_programa_de_fidelidade_id",
                principalSchema: "financas",
                principalTable: "plano_de_contas_itens",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_natureza_juros",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_juros_id",
                principalSchema: "financas",
                principalTable: "plano_de_contas_itens",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_natureza_multa",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_multa_id",
                principalSchema: "financas",
                principalTable: "plano_de_contas_itens",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_natureza_outros",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_outros_id",
                principalSchema: "financas",
                principalTable: "plano_de_contas_itens",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_natureza_pagamento_eletronico",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_pagamento_eletronico_nao_informado_id",
                principalSchema: "financas",
                principalTable: "plano_de_contas_itens",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_natureza_pix_dinamico",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_pagamento_instantaneo_pix_dinamico_id",
                principalSchema: "financas",
                principalTable: "plano_de_contas_itens",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_natureza_pix_estatico",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_pagamento_instantaneo_pix_estatico_id",
                principalSchema: "financas",
                principalTable: "plano_de_contas_itens",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_natureza_transferencia",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_transferencia_bancaria_id",
                principalSchema: "financas",
                principalTable: "plano_de_contas_itens",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_natureza_troco",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_troco_id",
                principalSchema: "financas",
                principalTable: "plano_de_contas_itens",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_natureza_vale_alimentacao",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_vale_alimentacao_id",
                principalSchema: "financas",
                principalTable: "plano_de_contas_itens",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_natureza_vale_combustivel",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_vale_combustivel_id",
                principalSchema: "financas",
                principalTable: "plano_de_contas_itens",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_natureza_vale_presente",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_vale_presente_id",
                principalSchema: "financas",
                principalTable: "plano_de_contas_itens",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_natureza_vale_refeicao",
                schema: "financas",
                table: "naturezas_financeiras",
                column: "item_plano_de_contas_financeiro_vale_refeicao_id",
                principalSchema: "financas",
                principalTable: "plano_de_contas_itens",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_contas_pagar__plano_de_contas_itens_plano_de_contas_financeiro_~",
                schema: "financas",
                table: "contas_pagar");

            migrationBuilder.DropForeignKey(
                name: "f_k_contas_pagar_naturezas_financeiras_natureza_financeira_id",
                schema: "financas",
                table: "contas_pagar");

            migrationBuilder.DropForeignKey(
                name: "f_k_contas_receber__plano_de_contas_itens_plano_de_contas_financeir~",
                schema: "financas",
                table: "contas_receber");

            migrationBuilder.DropForeignKey(
                name: "f_k_contas_receber_naturezas_financeiras_natureza_financeira_id",
                schema: "financas",
                table: "contas_receber");

            migrationBuilder.DropForeignKey(
                name: "fk_natureza_acrescimo",
                schema: "financas",
                table: "naturezas_financeiras");

            migrationBuilder.DropForeignKey(
                name: "fk_natureza_boleto",
                schema: "financas",
                table: "naturezas_financeiras");

            migrationBuilder.DropForeignKey(
                name: "fk_natureza_cartao_loja",
                schema: "financas",
                table: "naturezas_financeiras");

            migrationBuilder.DropForeignKey(
                name: "fk_natureza_cheque",
                schema: "financas",
                table: "naturezas_financeiras");

            migrationBuilder.DropForeignKey(
                name: "fk_natureza_credito",
                schema: "financas",
                table: "naturezas_financeiras");

            migrationBuilder.DropForeignKey(
                name: "fk_natureza_credito_loja",
                schema: "financas",
                table: "naturezas_financeiras");

            migrationBuilder.DropForeignKey(
                name: "fk_natureza_debito",
                schema: "financas",
                table: "naturezas_financeiras");

            migrationBuilder.DropForeignKey(
                name: "fk_natureza_deposito",
                schema: "financas",
                table: "naturezas_financeiras");

            migrationBuilder.DropForeignKey(
                name: "fk_natureza_desconto",
                schema: "financas",
                table: "naturezas_financeiras");

            migrationBuilder.DropForeignKey(
                name: "fk_natureza_dinheiro",
                schema: "financas",
                table: "naturezas_financeiras");

            migrationBuilder.DropForeignKey(
                name: "fk_natureza_duplicata",
                schema: "financas",
                table: "naturezas_financeiras");

            migrationBuilder.DropForeignKey(
                name: "fk_natureza_fidelidade",
                schema: "financas",
                table: "naturezas_financeiras");

            migrationBuilder.DropForeignKey(
                name: "fk_natureza_juros",
                schema: "financas",
                table: "naturezas_financeiras");

            migrationBuilder.DropForeignKey(
                name: "fk_natureza_multa",
                schema: "financas",
                table: "naturezas_financeiras");

            migrationBuilder.DropForeignKey(
                name: "fk_natureza_outros",
                schema: "financas",
                table: "naturezas_financeiras");

            migrationBuilder.DropForeignKey(
                name: "fk_natureza_pagamento_eletronico",
                schema: "financas",
                table: "naturezas_financeiras");

            migrationBuilder.DropForeignKey(
                name: "fk_natureza_pix_dinamico",
                schema: "financas",
                table: "naturezas_financeiras");

            migrationBuilder.DropForeignKey(
                name: "fk_natureza_pix_estatico",
                schema: "financas",
                table: "naturezas_financeiras");

            migrationBuilder.DropForeignKey(
                name: "fk_natureza_transferencia",
                schema: "financas",
                table: "naturezas_financeiras");

            migrationBuilder.DropForeignKey(
                name: "fk_natureza_troco",
                schema: "financas",
                table: "naturezas_financeiras");

            migrationBuilder.DropForeignKey(
                name: "fk_natureza_vale_alimentacao",
                schema: "financas",
                table: "naturezas_financeiras");

            migrationBuilder.DropForeignKey(
                name: "fk_natureza_vale_combustivel",
                schema: "financas",
                table: "naturezas_financeiras");

            migrationBuilder.DropForeignKey(
                name: "fk_natureza_vale_presente",
                schema: "financas",
                table: "naturezas_financeiras");

            migrationBuilder.DropForeignKey(
                name: "fk_natureza_vale_refeicao",
                schema: "financas",
                table: "naturezas_financeiras");

            // plataforma.pessoas_fisicas pertence ao módulo GestaoClientes: drop idempotente/seguro.
            migrationBuilder.Sql("DROP TABLE IF EXISTS plataforma.pessoas_fisicas;");

            migrationBuilder.DropTable(
                name: "plano_de_contas_itens",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "planos_de_contas",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "naturezas_financeiras",
                schema: "financas");

            migrationBuilder.DropIndex(
                name: "i_x_contas_receber_natureza_financeira_id",
                schema: "financas",
                table: "contas_receber");

            migrationBuilder.DropIndex(
                name: "i_x_contas_receber_plano_de_contas_financeiro_item_id",
                schema: "financas",
                table: "contas_receber");

            migrationBuilder.DropIndex(
                name: "i_x_contas_pagar_natureza_financeira_id",
                schema: "financas",
                table: "contas_pagar");

            migrationBuilder.DropIndex(
                name: "i_x_contas_pagar_plano_de_contas_financeiro_item_id",
                schema: "financas",
                table: "contas_pagar");

            // plataforma.pessoas pertence ao módulo GestaoClientes: drop de coluna idempotente/seguro.
            migrationBuilder.Sql("ALTER TABLE plataforma.pessoas DROP COLUMN IF EXISTS eh_cliente;");

            migrationBuilder.DropColumn(
                name: "centro_custo_projeto_id",
                schema: "financas",
                table: "contas_receber");

            migrationBuilder.DropColumn(
                name: "natureza_financeira_id",
                schema: "financas",
                table: "contas_receber");

            migrationBuilder.DropColumn(
                name: "plano_de_contas_financeiro_item_id",
                schema: "financas",
                table: "contas_receber");

            migrationBuilder.DropColumn(
                name: "centro_custo_projeto_id",
                schema: "financas",
                table: "contas_pagar");

            migrationBuilder.DropColumn(
                name: "natureza_financeira_id",
                schema: "financas",
                table: "contas_pagar");

            migrationBuilder.DropColumn(
                name: "plano_de_contas_financeiro_item_id",
                schema: "financas",
                table: "contas_pagar");
        }
    }
}
