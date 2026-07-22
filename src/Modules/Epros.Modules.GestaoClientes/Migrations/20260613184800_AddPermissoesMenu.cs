using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GestaoClientes.Migrations
{
    /// <inheritdoc />
    public partial class AddPermissoesMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "eh_mei",
                schema: "plataforma",
                table: "empresas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "cupons",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "text", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    valor_desconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    limite_uso = table.Column<int>(type: "integer", nullable: true),
                    quantidade_usos = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    valido_ate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_cupons", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "menus",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    icon = table.Column<string>(type: "text", nullable: true),
                    to = table.Column<string>(type: "text", nullable: true),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    modulo = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_menus", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "perfis_acessos",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_perfis_acessos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pedidos_saa_s",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plano_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cupom_id = table.Column<Guid>(type: "uuid", nullable: true),
                    valor_base = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_desconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    moeda = table.Column<string>(type: "text", nullable: false),
                    metodo_pagamento = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    assinatura_criada_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_pedidos_saa_s", x => x.id);
                    table.ForeignKey(
                        name: "f_k_pedidos_saa_s__planos_plano_id",
                        column: x => x.plano_id,
                        principalSchema: "plataforma",
                        principalTable: "planos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_pedidos_saa_s_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalSchema: "plataforma",
                        principalTable: "clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_pedidos_saa_s_cupons_cupom_id",
                        column: x => x.cupom_id,
                        principalSchema: "plataforma",
                        principalTable: "cupons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "menus_itens_nivel1",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    menu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    icon = table.Column<string>(type: "text", nullable: true),
                    to = table.Column<string>(type: "text", nullable: true),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    modulo = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_menus_itens_nivel1", x => x.id);
                    table.ForeignKey(
                        name: "f_k_menus_itens_nivel1_menus_menu_id",
                        column: x => x.menu_id,
                        principalSchema: "plataforma",
                        principalTable: "menus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "perfis_acessos_menus",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    perfil_acesso_id = table.Column<Guid>(type: "uuid", nullable: false),
                    menu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    menu_item_nivel1_id = table.Column<Guid>(type: "uuid", nullable: true),
                    menu_item_nivel2_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ver = table.Column<bool>(type: "boolean", nullable: false),
                    editar = table.Column<bool>(type: "boolean", nullable: false),
                    excluir = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_perfis_acessos_menus", x => x.id);
                    table.ForeignKey(
                        name: "f_k_perfis_acessos_menus_perfis_acessos_perfil_acesso_id",
                        column: x => x.perfil_acesso_id,
                        principalSchema: "plataforma",
                        principalTable: "perfis_acessos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pagamentos_globais",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    assinatura_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pedido_id = table.Column<Guid>(type: "uuid", nullable: true),
                    fatura_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_pagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    gateway = table.Column<string>(type: "text", nullable: false),
                    transaction_id = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_pagamentos_globais", x => x.id);
                    table.ForeignKey(
                        name: "f_k_pagamentos_globais__pedidos_saa_s_pedido_id",
                        column: x => x.pedido_id,
                        principalSchema: "plataforma",
                        principalTable: "pedidos_saa_s",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "f_k_pagamentos_globais_assinaturas_clientes_assinatura_id",
                        column: x => x.assinatura_id,
                        principalSchema: "plataforma",
                        principalTable: "assinaturas_clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_pagamentos_globais_faturas_fatura_id",
                        column: x => x.fatura_id,
                        principalSchema: "plataforma",
                        principalTable: "faturas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "pagamentos_transferencias",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fatura_id = table.Column<Guid>(type: "uuid", nullable: true),
                    pedido_id = table.Column<Guid>(type: "uuid", nullable: true),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    moeda = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    justificativa = table.Column<string>(type: "text", nullable: true),
                    data_analise = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    analisado_por = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_pagamentos_transferencias", x => x.id);
                    table.ForeignKey(
                        name: "f_k_pagamentos_transferencias__pedidos_saa_s_pedido_id",
                        column: x => x.pedido_id,
                        principalSchema: "plataforma",
                        principalTable: "pedidos_saa_s",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "f_k_pagamentos_transferencias_faturas_fatura_id",
                        column: x => x.fatura_id,
                        principalSchema: "plataforma",
                        principalTable: "faturas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "sessoes_pagamentos",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    gateway_ref = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    assinatura_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pedido_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_sessoes_pagamentos", x => x.id);
                    table.ForeignKey(
                        name: "f_k_sessoes_pagamentos_assinaturas_clientes_assinatura_id",
                        column: x => x.assinatura_id,
                        principalSchema: "plataforma",
                        principalTable: "assinaturas_clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_sessoes_pagamentos_pedidos_saa_s_pedido_id",
                        column: x => x.pedido_id,
                        principalSchema: "plataforma",
                        principalTable: "pedidos_saa_s",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usos_cupons",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cupom_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pedido_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_usos_cupons", x => x.id);
                    table.ForeignKey(
                        name: "f_k_usos_cupons_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalSchema: "plataforma",
                        principalTable: "clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_usos_cupons_cupons_cupom_id",
                        column: x => x.cupom_id,
                        principalSchema: "plataforma",
                        principalTable: "cupons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_usos_cupons_pedidos_saa_s_pedido_id",
                        column: x => x.pedido_id,
                        principalSchema: "plataforma",
                        principalTable: "pedidos_saa_s",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "menus_itens_nivel2",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    menu_item_nivel1_id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    icon = table.Column<string>(type: "text", nullable: true),
                    to = table.Column<string>(type: "text", nullable: true),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    modulo = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_menus_itens_nivel2", x => x.id);
                    table.ForeignKey(
                        name: "f_k_menus_itens_nivel2_menus_itens_nivel1_menu_item_nivel1_id",
                        column: x => x.menu_item_nivel1_id,
                        principalSchema: "plataforma",
                        principalTable: "menus_itens_nivel1",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "comprovantes_pagamentos",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pagamento_transferencia_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_arquivo = table.Column<string>(type: "text", nullable: false),
                    caminho_arquivo = table.Column<string>(type: "text", nullable: false),
                    tamanho_bytes = table.Column<long>(type: "bigint", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    data_comprovante = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status_leitura = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_comprovantes_pagamentos", x => x.id);
                    table.ForeignKey(
                        name: "f_k_comprovantes_pagamentos__pagamentos_transferencias_pagamento_~",
                        column: x => x.pagamento_transferencia_id,
                        principalSchema: "plataforma",
                        principalTable: "pagamentos_transferencias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_pagamentos_fatura_payment_id",
                schema: "plataforma",
                table: "pagamentos_faturas",
                column: "identificador_pagamento",
                unique: true,
                filter: "identificador_pagamento IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "i_x_comprovantes_pagamentos_pagamento_transferencia_id",
                schema: "plataforma",
                table: "comprovantes_pagamentos",
                column: "pagamento_transferencia_id");

            migrationBuilder.CreateIndex(
                name: "ix__comprovante_pagamento_sync_id",
                schema: "plataforma",
                table: "comprovantes_pagamentos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__comprovante_pagamento_tenant_id",
                schema: "plataforma",
                table: "comprovantes_pagamentos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__cupom_sync_id",
                schema: "plataforma",
                table: "cupons",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__cupom_tenant_id",
                schema: "plataforma",
                table: "cupons",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_cupons_codigo",
                schema: "plataforma",
                table: "cupons",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_menus_itens_nivel1_menu_id",
                schema: "plataforma",
                table: "menus_itens_nivel1",
                column: "menu_id");

            migrationBuilder.CreateIndex(
                name: "i_x_menus_itens_nivel2_menu_item_nivel1_id",
                schema: "plataforma",
                table: "menus_itens_nivel2",
                column: "menu_item_nivel1_id");

            migrationBuilder.CreateIndex(
                name: "i_x_pagamentos_globais_assinatura_id",
                schema: "plataforma",
                table: "pagamentos_globais",
                column: "assinatura_id");

            migrationBuilder.CreateIndex(
                name: "i_x_pagamentos_globais_fatura_id",
                schema: "plataforma",
                table: "pagamentos_globais",
                column: "fatura_id");

            migrationBuilder.CreateIndex(
                name: "i_x_pagamentos_globais_pedido_id",
                schema: "plataforma",
                table: "pagamentos_globais",
                column: "pedido_id");

            migrationBuilder.CreateIndex(
                name: "ix__pagamento_global_sync_id",
                schema: "plataforma",
                table: "pagamentos_globais",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pagamento_global_tenant_id",
                schema: "plataforma",
                table: "pagamentos_globais",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_pagamentos_transferencias_fatura_id",
                schema: "plataforma",
                table: "pagamentos_transferencias",
                column: "fatura_id");

            migrationBuilder.CreateIndex(
                name: "i_x_pagamentos_transferencias_pedido_id",
                schema: "plataforma",
                table: "pagamentos_transferencias",
                column: "pedido_id");

            migrationBuilder.CreateIndex(
                name: "ix__pagamento_transferencia_sync_id",
                schema: "plataforma",
                table: "pagamentos_transferencias",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pagamento_transferencia_tenant_id",
                schema: "plataforma",
                table: "pagamentos_transferencias",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_pedidos_saa_s_cliente_id",
                schema: "plataforma",
                table: "pedidos_saa_s",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "i_x_pedidos_saa_s_cupom_id",
                schema: "plataforma",
                table: "pedidos_saa_s",
                column: "cupom_id");

            migrationBuilder.CreateIndex(
                name: "i_x_pedidos_saa_s_plano_id",
                schema: "plataforma",
                table: "pedidos_saa_s",
                column: "plano_id");

            migrationBuilder.CreateIndex(
                name: "ix__pedido_saa_s_sync_id",
                schema: "plataforma",
                table: "pedidos_saa_s",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pedido_saa_s_tenant_id",
                schema: "plataforma",
                table: "pedidos_saa_s",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__perfil_acesso_sync_id",
                schema: "plataforma",
                table: "perfis_acessos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__perfil_acesso_tenant_id",
                schema: "plataforma",
                table: "perfis_acessos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_perfis_acesso_tenant_descricao",
                schema: "plataforma",
                table: "perfis_acessos",
                columns: new[] { "tenant_id", "descricao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__perfil_acesso_menu_sync_id",
                schema: "plataforma",
                table: "perfis_acessos_menus",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__perfil_acesso_menu_tenant_id",
                schema: "plataforma",
                table: "perfis_acessos_menus",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_perfis_acessos_menus_combinacao_unica",
                schema: "plataforma",
                table: "perfis_acessos_menus",
                columns: new[] { "perfil_acesso_id", "menu_id", "menu_item_nivel1_id", "menu_item_nivel2_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_sessoes_pagamentos_assinatura_id",
                schema: "plataforma",
                table: "sessoes_pagamentos",
                column: "assinatura_id");

            migrationBuilder.CreateIndex(
                name: "i_x_sessoes_pagamentos_pedido_id",
                schema: "plataforma",
                table: "sessoes_pagamentos",
                column: "pedido_id");

            migrationBuilder.CreateIndex(
                name: "ix__sessao_pagamento_sync_id",
                schema: "plataforma",
                table: "sessoes_pagamentos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sessao_pagamento_tenant_id",
                schema: "plataforma",
                table: "sessoes_pagamentos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_usos_cupons_cupom_id",
                schema: "plataforma",
                table: "usos_cupons",
                column: "cupom_id");

            migrationBuilder.CreateIndex(
                name: "i_x_usos_cupons_pedido_id",
                schema: "plataforma",
                table: "usos_cupons",
                column: "pedido_id");

            migrationBuilder.CreateIndex(
                name: "ix__uso_cupom_sync_id",
                schema: "plataforma",
                table: "usos_cupons",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__uso_cupom_tenant_id",
                schema: "plataforma",
                table: "usos_cupons",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_usos_cupons_usuario_cupom_pedido",
                schema: "plataforma",
                table: "usos_cupons",
                columns: new[] { "cliente_id", "cupom_id", "pedido_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "comprovantes_pagamentos",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "menus_itens_nivel2",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "pagamentos_globais",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "perfis_acessos_menus",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "sessoes_pagamentos",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "usos_cupons",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "pagamentos_transferencias",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "menus_itens_nivel1",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "perfis_acessos",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "pedidos_saa_s",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "menus",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "cupons",
                schema: "plataforma");

            migrationBuilder.DropIndex(
                name: "ix_pagamentos_fatura_payment_id",
                schema: "plataforma",
                table: "pagamentos_faturas");

            migrationBuilder.DropColumn(
                name: "eh_mei",
                schema: "plataforma",
                table: "empresas");
        }
    }
}
