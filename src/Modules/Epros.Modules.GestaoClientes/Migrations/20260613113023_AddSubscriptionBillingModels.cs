using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GestaoClientes.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionBillingModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "grupo_plano_id",
                schema: "plataforma",
                table: "planos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "limite_empresas",
                schema: "plataforma",
                table: "planos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "limite_usuarios",
                schema: "plataforma",
                table: "planos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "recursos_inclusos",
                schema: "plataforma",
                table: "planos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "dia_vencimento",
                schema: "plataforma",
                table: "clientes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "revenda_id",
                schema: "plataforma",
                table: "clientes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status_saa_s",
                schema: "plataforma",
                table: "clientes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "vendedor_id",
                schema: "plataforma",
                table: "clientes",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "assinaturas_clientes",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plano_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    trial_ate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    metodo_pagamento = table.Column<string>(type: "text", nullable: false),
                    transacao_id = table.Column<string>(type: "text", nullable: true),
                    detalhes_pacote_json = table.Column<string>(type: "text", nullable: true),
                    arquivada = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_assinaturas_clientes", x => x.id);
                    table.ForeignKey(
                        name: "f_k_assinaturas_clientes__clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalSchema: "plataforma",
                        principalTable: "clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_assinaturas_clientes__planos_plano_id",
                        column: x => x.plano_id,
                        principalSchema: "plataforma",
                        principalTable: "planos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "composicoes_faturamento",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    data_inicial = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_final = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    pode_reajustar = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_composicoes_faturamento", x => x.id);
                    table.ForeignKey(
                        name: "f_k_composicoes_faturamento_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalSchema: "plataforma",
                        principalTable: "clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "configuracoes_globais",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    chave = table.Column<string>(type: "text", nullable: false),
                    valor = table.Column<string>(type: "text", nullable: false),
                    eh_segredo = table.Column<bool>(type: "boolean", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_configuracoes_globais", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "execucoes_massa",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_operacao = table.Column<string>(type: "text", nullable: false),
                    parametros = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    aprovadores_json = table.Column<string>(type: "text", nullable: false),
                    resultado_log = table.Column<string>(type: "text", nullable: true),
                    executado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_execucoes_massa", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grupo_planos",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_grupo_planos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pagamentos_faturas",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fatura_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_pagamento = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    valor_pago = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_tarifa = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    identificador_pagamento = table.Column<string>(type: "text", nullable: true),
                    pago_manualmente = table.Column<bool>(type: "boolean", nullable: false),
                    data_pagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_pagamentos_faturas", x => x.id);
                    table.ForeignKey(
                        name: "f_k_pagamentos_faturas_faturas_fatura_id",
                        column: x => x.fatura_id,
                        principalSchema: "plataforma",
                        principalTable: "faturas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "revendas",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    percentual_comissao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_revendas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "historicos_reajustes",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    composicao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    valor_atual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_novo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    percentual_reajuste = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tipo_reajuste = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_historicos_reajustes", x => x.id);
                    table.ForeignKey(
                        name: "f_k_historicos_reajustes_composicoes_faturamento_composicao_id",
                        column: x => x.composicao_id,
                        principalSchema: "plataforma",
                        principalTable: "composicoes_faturamento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vendedores",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    revenda_id = table.Column<Guid>(type: "uuid", nullable: true),
                    nome = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    telefone = table.Column<string>(type: "text", nullable: true),
                    percentual_comissao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_vendedores", x => x.id);
                    table.ForeignKey(
                        name: "f_k_vendedores_revendas_revenda_id",
                        column: x => x.revenda_id,
                        principalSchema: "plataforma",
                        principalTable: "revendas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_planos_grupo_plano_id",
                schema: "plataforma",
                table: "planos",
                column: "grupo_plano_id");

            migrationBuilder.CreateIndex(
                name: "i_x_clientes_revenda_id",
                schema: "plataforma",
                table: "clientes",
                column: "revenda_id");

            migrationBuilder.CreateIndex(
                name: "i_x_clientes_vendedor_id",
                schema: "plataforma",
                table: "clientes",
                column: "vendedor_id");

            migrationBuilder.CreateIndex(
                name: "i_x_assinaturas_clientes_cliente_id",
                schema: "plataforma",
                table: "assinaturas_clientes",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "i_x_assinaturas_clientes_plano_id",
                schema: "plataforma",
                table: "assinaturas_clientes",
                column: "plano_id");

            migrationBuilder.CreateIndex(
                name: "ix__assinatura_cliente_sync_id",
                schema: "plataforma",
                table: "assinaturas_clientes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__assinatura_cliente_tenant_id",
                schema: "plataforma",
                table: "assinaturas_clientes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_composicoes_faturamento_cliente_id",
                schema: "plataforma",
                table: "composicoes_faturamento",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix__composicao_faturamento_sync_id",
                schema: "plataforma",
                table: "composicoes_faturamento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__composicao_faturamento_tenant_id",
                schema: "plataforma",
                table: "composicoes_faturamento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__configuracao_global_sync_id",
                schema: "plataforma",
                table: "configuracoes_globais",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__configuracao_global_tenant_id",
                schema: "plataforma",
                table: "configuracoes_globais",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_configuracoes_globais_chave",
                schema: "plataforma",
                table: "configuracoes_globais",
                column: "chave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__execucao_massa_sync_id",
                schema: "plataforma",
                table: "execucoes_massa",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__execucao_massa_tenant_id",
                schema: "plataforma",
                table: "execucoes_massa",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__grupo_plano_sync_id",
                schema: "plataforma",
                table: "grupo_planos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__grupo_plano_tenant_id",
                schema: "plataforma",
                table: "grupo_planos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_historicos_reajustes_composicao_id",
                schema: "plataforma",
                table: "historicos_reajustes",
                column: "composicao_id");

            migrationBuilder.CreateIndex(
                name: "ix__historico_reajuste_sync_id",
                schema: "plataforma",
                table: "historicos_reajustes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__historico_reajuste_tenant_id",
                schema: "plataforma",
                table: "historicos_reajustes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_pagamentos_faturas_fatura_id",
                schema: "plataforma",
                table: "pagamentos_faturas",
                column: "fatura_id");

            migrationBuilder.CreateIndex(
                name: "ix__pagamento_fatura_sync_id",
                schema: "plataforma",
                table: "pagamentos_faturas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pagamento_fatura_tenant_id",
                schema: "plataforma",
                table: "pagamentos_faturas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__revenda_sync_id",
                schema: "plataforma",
                table: "revendas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__revenda_tenant_id",
                schema: "plataforma",
                table: "revendas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_vendedores_revenda_id",
                schema: "plataforma",
                table: "vendedores",
                column: "revenda_id");

            migrationBuilder.CreateIndex(
                name: "ix__vendedor_sync_id",
                schema: "plataforma",
                table: "vendedores",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__vendedor_tenant_id",
                schema: "plataforma",
                table: "vendedores",
                column: "tenant_id");

            migrationBuilder.AddForeignKey(
                name: "f_k_clientes__revendas_revenda_id",
                schema: "plataforma",
                table: "clientes",
                column: "revenda_id",
                principalSchema: "plataforma",
                principalTable: "revendas",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "f_k_clientes__vendedores_vendedor_id",
                schema: "plataforma",
                table: "clientes",
                column: "vendedor_id",
                principalSchema: "plataforma",
                principalTable: "vendedores",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "f_k_planos_grupo_planos_grupo_plano_id",
                schema: "plataforma",
                table: "planos",
                column: "grupo_plano_id",
                principalSchema: "plataforma",
                principalTable: "grupo_planos",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_clientes__revendas_revenda_id",
                schema: "plataforma",
                table: "clientes");

            migrationBuilder.DropForeignKey(
                name: "f_k_clientes__vendedores_vendedor_id",
                schema: "plataforma",
                table: "clientes");

            migrationBuilder.DropForeignKey(
                name: "f_k_planos_grupo_planos_grupo_plano_id",
                schema: "plataforma",
                table: "planos");

            migrationBuilder.DropTable(
                name: "assinaturas_clientes",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "configuracoes_globais",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "execucoes_massa",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "grupo_planos",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "historicos_reajustes",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "pagamentos_faturas",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "vendedores",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "composicoes_faturamento",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "revendas",
                schema: "plataforma");

            migrationBuilder.DropIndex(
                name: "i_x_planos_grupo_plano_id",
                schema: "plataforma",
                table: "planos");

            migrationBuilder.DropIndex(
                name: "i_x_clientes_revenda_id",
                schema: "plataforma",
                table: "clientes");

            migrationBuilder.DropIndex(
                name: "i_x_clientes_vendedor_id",
                schema: "plataforma",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "grupo_plano_id",
                schema: "plataforma",
                table: "planos");

            migrationBuilder.DropColumn(
                name: "limite_empresas",
                schema: "plataforma",
                table: "planos");

            migrationBuilder.DropColumn(
                name: "limite_usuarios",
                schema: "plataforma",
                table: "planos");

            migrationBuilder.DropColumn(
                name: "recursos_inclusos",
                schema: "plataforma",
                table: "planos");

            migrationBuilder.DropColumn(
                name: "dia_vencimento",
                schema: "plataforma",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "revenda_id",
                schema: "plataforma",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "status_saa_s",
                schema: "plataforma",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "vendedor_id",
                schema: "plataforma",
                table: "clientes");
        }
    }
}
