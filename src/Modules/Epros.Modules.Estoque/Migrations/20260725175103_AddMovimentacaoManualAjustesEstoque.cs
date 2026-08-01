using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Estoque.Migrations
{
    /// <inheritdoc />
    public partial class AddMovimentacaoManualAjustesEstoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "local_id",
                schema: "estoque",
                table: "produto_ficha_estoque_saidas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "data_validade",
                schema: "estoque",
                table: "produto_ficha_estoque_entradas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "local_id",
                schema: "estoque",
                table: "produto_ficha_estoque_entradas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "lote",
                schema: "estoque",
                table: "produto_ficha_estoque_entradas",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "documento_consumidor_entrada_id",
                schema: "estoque",
                table: "fatos_geradores_estoque",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "documento_consumidor_saida_id",
                schema: "estoque",
                table: "fatos_geradores_estoque",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "documento_entrada_id",
                schema: "estoque",
                table: "fatos_geradores_estoque",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "documento_saida_id",
                schema: "estoque",
                table: "fatos_geradores_estoque",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "referencia_externa",
                schema: "estoque",
                table: "fatos_geradores_estoque",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motivo",
                schema: "estoque",
                table: "estoque_movimentos_manuais",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "situacao",
                schema: "estoque",
                table: "estoque_movimentos_manuais",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ajustes_estoque",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    local_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_ajuste = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    tipo_ajuste = table.Column<int>(type: "integer", nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_recuperado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    observacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    situacao = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_ajustes_estoque", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "avarias_estoque",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    categoria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    preco_compra = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    data_avaria = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    nota = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    referencia = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    situacao = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_avarias_estoque", x => x.id);
                    table.ForeignKey(
                        name: "f_k_avarias_estoque__produtos_produto_id",
                        column: x => x.produto_id,
                        principalSchema: "estoque",
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "historicos_estoque",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    evento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    situacao_anterior = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    situacao_nova = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    motivo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    usuario_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("p_k_historicos_estoque", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "requisicoes_internas",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_requisicao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    situacao = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_requisicoes_internas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "saldos_iniciais_importacoes",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    arquivo_nome = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    situacao = table.Column<int>(type: "integer", nullable: false),
                    linhas_total = table.Column<int>(type: "integer", nullable: false),
                    linhas_processadas = table.Column<int>(type: "integer", nullable: false),
                    linhas_erro = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_saldos_iniciais_importacoes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transferencias_estoque",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    local_origem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    local_destino_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_transferencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    situacao = table.Column<int>(type: "integer", nullable: false),
                    valor_frete = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    observacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("p_k_transferencias_estoque", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ajustes_estoque_itens",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ajuste_estoque_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    lote = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    ficha_entrada_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_ajustes_estoque_itens", x => x.id);
                    table.ForeignKey(
                        name: "f_k_ajustes_estoque_itens__produtos_produto_id",
                        column: x => x.produto_id,
                        principalSchema: "estoque",
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_ajustes_estoque_itens_ajustes_estoque_ajuste_estoque_id",
                        column: x => x.ajuste_estoque_id,
                        principalSchema: "estoque",
                        principalTable: "ajustes_estoque",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "requisicoes_internas_itens",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    requisicao_interna_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_requisicoes_internas_itens", x => x.id);
                    table.ForeignKey(
                        name: "f_k_requisicoes_internas_itens_produtos_produto_id",
                        column: x => x.produto_id,
                        principalSchema: "estoque",
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_requisicoes_internas_itens_requisicoes_internas_requisicao_~",
                        column: x => x.requisicao_interna_id,
                        principalSchema: "estoque",
                        principalTable: "requisicoes_internas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "saldos_iniciais_itens",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    saldo_inicial_importacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_codigo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    local_nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    local_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    custo_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    lote = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    data_validade = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    mensagem_erro = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("p_k_saldos_iniciais_itens", x => x.id);
                    table.ForeignKey(
                        name: "f_k_saldos_iniciais_itens_saldos_iniciais_importacoes_saldo_ini~",
                        column: x => x.saldo_inicial_importacao_id,
                        principalSchema: "estoque",
                        principalTable: "saldos_iniciais_importacoes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transferencias_estoque_itens",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transferencia_estoque_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    lote = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    data_validade = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_transferencias_estoque_itens", x => x.id);
                    table.ForeignKey(
                        name: "f_k_transferencias_estoque_itens_produtos_produto_id",
                        column: x => x.produto_id,
                        principalSchema: "estoque",
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_transferencias_estoque_itens_transferencias_estoque_transfe~",
                        column: x => x.transferencia_estoque_id,
                        principalSchema: "estoque",
                        principalTable: "transferencias_estoque",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_estoque_movimentos_manuais_tenant_id_situacao",
                schema: "estoque",
                table: "estoque_movimentos_manuais",
                columns: new[] { "tenant_id", "situacao" });

            migrationBuilder.CreateIndex(
                name: "i_x_ajustes_estoque_tenant_id_situacao",
                schema: "estoque",
                table: "ajustes_estoque",
                columns: new[] { "tenant_id", "situacao" });

            migrationBuilder.CreateIndex(
                name: "ix__ajuste_estoque_sync_id",
                schema: "estoque",
                table: "ajustes_estoque",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ajuste_estoque_tenant_id",
                schema: "estoque",
                table: "ajustes_estoque",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ajustes_estoque_itens_ajuste_estoque_id",
                schema: "estoque",
                table: "ajustes_estoque_itens",
                column: "ajuste_estoque_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ajustes_estoque_itens_produto_id",
                schema: "estoque",
                table: "ajustes_estoque_itens",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ajustes_estoque_itens_tenant_id_ajuste_estoque_id",
                schema: "estoque",
                table: "ajustes_estoque_itens",
                columns: new[] { "tenant_id", "ajuste_estoque_id" });

            migrationBuilder.CreateIndex(
                name: "ix__ajuste_estoque_item_sync_id",
                schema: "estoque",
                table: "ajustes_estoque_itens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ajuste_estoque_item_tenant_id",
                schema: "estoque",
                table: "ajustes_estoque_itens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_avarias_estoque_produto_id",
                schema: "estoque",
                table: "avarias_estoque",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "i_x_avarias_estoque_tenant_id_produto_id",
                schema: "estoque",
                table: "avarias_estoque",
                columns: new[] { "tenant_id", "produto_id" });

            migrationBuilder.CreateIndex(
                name: "ix__avaria_estoque_sync_id",
                schema: "estoque",
                table: "avarias_estoque",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__avaria_estoque_tenant_id",
                schema: "estoque",
                table: "avarias_estoque",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_historicos_estoque_tenant_id_entidade_entidade_id",
                schema: "estoque",
                table: "historicos_estoque",
                columns: new[] { "tenant_id", "entidade", "entidade_id" });

            migrationBuilder.CreateIndex(
                name: "ix__historico_estoque_sync_id",
                schema: "estoque",
                table: "historicos_estoque",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__historico_estoque_tenant_id",
                schema: "estoque",
                table: "historicos_estoque",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_requisicoes_internas_tenant_id_colaborador_id",
                schema: "estoque",
                table: "requisicoes_internas",
                columns: new[] { "tenant_id", "colaborador_id" });

            migrationBuilder.CreateIndex(
                name: "ix__requisicao_interna_sync_id",
                schema: "estoque",
                table: "requisicoes_internas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__requisicao_interna_tenant_id",
                schema: "estoque",
                table: "requisicoes_internas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_requisicoes_internas_itens_produto_id",
                schema: "estoque",
                table: "requisicoes_internas_itens",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "i_x_requisicoes_internas_itens_requisicao_interna_id",
                schema: "estoque",
                table: "requisicoes_internas_itens",
                column: "requisicao_interna_id");

            migrationBuilder.CreateIndex(
                name: "i_x_requisicoes_internas_itens_tenant_id_requisicao_interna_id",
                schema: "estoque",
                table: "requisicoes_internas_itens",
                columns: new[] { "tenant_id", "requisicao_interna_id" });

            migrationBuilder.CreateIndex(
                name: "ix__requisicao_interna_item_sync_id",
                schema: "estoque",
                table: "requisicoes_internas_itens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__requisicao_interna_item_tenant_id",
                schema: "estoque",
                table: "requisicoes_internas_itens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_saldos_iniciais_importacoes_tenant_id_situacao",
                schema: "estoque",
                table: "saldos_iniciais_importacoes",
                columns: new[] { "tenant_id", "situacao" });

            migrationBuilder.CreateIndex(
                name: "ix__saldo_inicial_importacao_sync_id",
                schema: "estoque",
                table: "saldos_iniciais_importacoes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__saldo_inicial_importacao_tenant_id",
                schema: "estoque",
                table: "saldos_iniciais_importacoes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_saldos_iniciais_itens_saldo_inicial_importacao_id",
                schema: "estoque",
                table: "saldos_iniciais_itens",
                column: "saldo_inicial_importacao_id");

            migrationBuilder.CreateIndex(
                name: "i_x_saldos_iniciais_itens_tenant_id_saldo_inicial_importacao_id",
                schema: "estoque",
                table: "saldos_iniciais_itens",
                columns: new[] { "tenant_id", "saldo_inicial_importacao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__saldo_inicial_item_sync_id",
                schema: "estoque",
                table: "saldos_iniciais_itens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__saldo_inicial_item_tenant_id",
                schema: "estoque",
                table: "saldos_iniciais_itens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_transferencias_estoque_tenant_id_situacao",
                schema: "estoque",
                table: "transferencias_estoque",
                columns: new[] { "tenant_id", "situacao" });

            migrationBuilder.CreateIndex(
                name: "ix__transferencia_estoque_sync_id",
                schema: "estoque",
                table: "transferencias_estoque",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__transferencia_estoque_tenant_id",
                schema: "estoque",
                table: "transferencias_estoque",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_transferencias_estoque_itens_produto_id",
                schema: "estoque",
                table: "transferencias_estoque_itens",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "i_x_transferencias_estoque_itens_tenant_id_transferencia_estoqu~",
                schema: "estoque",
                table: "transferencias_estoque_itens",
                columns: new[] { "tenant_id", "transferencia_estoque_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_transferencias_estoque_itens_transferencia_estoque_id",
                schema: "estoque",
                table: "transferencias_estoque_itens",
                column: "transferencia_estoque_id");

            migrationBuilder.CreateIndex(
                name: "ix__transferencia_estoque_item_sync_id",
                schema: "estoque",
                table: "transferencias_estoque_itens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__transferencia_estoque_item_tenant_id",
                schema: "estoque",
                table: "transferencias_estoque_itens",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ajustes_estoque_itens",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "avarias_estoque",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "historicos_estoque",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "requisicoes_internas_itens",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "saldos_iniciais_itens",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "transferencias_estoque_itens",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "ajustes_estoque",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "requisicoes_internas",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "saldos_iniciais_importacoes",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "transferencias_estoque",
                schema: "estoque");

            migrationBuilder.DropIndex(
                name: "i_x_estoque_movimentos_manuais_tenant_id_situacao",
                schema: "estoque",
                table: "estoque_movimentos_manuais");

            migrationBuilder.DropColumn(
                name: "local_id",
                schema: "estoque",
                table: "produto_ficha_estoque_saidas");

            migrationBuilder.DropColumn(
                name: "data_validade",
                schema: "estoque",
                table: "produto_ficha_estoque_entradas");

            migrationBuilder.DropColumn(
                name: "local_id",
                schema: "estoque",
                table: "produto_ficha_estoque_entradas");

            migrationBuilder.DropColumn(
                name: "lote",
                schema: "estoque",
                table: "produto_ficha_estoque_entradas");

            migrationBuilder.DropColumn(
                name: "documento_consumidor_entrada_id",
                schema: "estoque",
                table: "fatos_geradores_estoque");

            migrationBuilder.DropColumn(
                name: "documento_consumidor_saida_id",
                schema: "estoque",
                table: "fatos_geradores_estoque");

            migrationBuilder.DropColumn(
                name: "documento_entrada_id",
                schema: "estoque",
                table: "fatos_geradores_estoque");

            migrationBuilder.DropColumn(
                name: "documento_saida_id",
                schema: "estoque",
                table: "fatos_geradores_estoque");

            migrationBuilder.DropColumn(
                name: "referencia_externa",
                schema: "estoque",
                table: "fatos_geradores_estoque");

            migrationBuilder.DropColumn(
                name: "motivo",
                schema: "estoque",
                table: "estoque_movimentos_manuais");

            migrationBuilder.DropColumn(
                name: "situacao",
                schema: "estoque",
                table: "estoque_movimentos_manuais");
        }
    }
}
