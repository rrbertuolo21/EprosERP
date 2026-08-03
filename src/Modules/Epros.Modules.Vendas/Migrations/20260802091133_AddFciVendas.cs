using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Vendas.Migrations
{
    /// <summary>
    /// VEN-FCI (Faturamento Comercial Internacional): cria as 9 tabelas fci_* (documento comercial,
    /// itens, imposto, preferencia, configuracao, lancamento estoque/razao, pdf, historico).
    /// Documento COMERCIAL internacional (FCI-001) — nao e documento fiscal BR. Migracao higienizada
    /// (removido o ruido de xmin do scaffold; mantidas apenas as operacoes fci_).
    /// </summary>
    public partial class AddFciVendas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fci_configuracao_documento",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_documento = table.Column<int>(type: "integer", nullable: false),
                    nome_tipo_documento = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    indice_inicial = table.Column<int>(type: "integer", nullable: false),
                    prefixo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    sufixo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    numero_pedido = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    numero_transporte = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    veiculo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    mostrar_desconto = table.Column<bool>(type: "boolean", nullable: false),
                    mostrar_imposto = table.Column<bool>(type: "boolean", nullable: false),
                    mostrar_codigo_barras = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_fci_configuracao_documento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fci_documento_comercial",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_documento = table.Column<int>(type: "integer", nullable: false),
                    serial = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    data_documento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    documento_origem_id = table.Column<Guid>(type: "uuid", nullable: true),
                    armazem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ano_financeiro_id = table.Column<int>(type: "integer", nullable: false),
                    moeda = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    taxa_cambio = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 2, nullable: true),
                    incoterm = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    imposto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    aliquota_imposto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    total_imposto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    percentual_desconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    tipo_desconto = table.Column<int>(type: "integer", nullable: false),
                    desconto_documento = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_frete = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_bruto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_liquido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_geral = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_pago = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    saldo_anterior = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    saldo_em_aberto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    referencia = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    observacao = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_fci_documento_comercial", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fci_historico",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    documento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    entidade_tipo = table.Column<int>(type: "integer", nullable: false),
                    evento = table.Column<int>(type: "integer", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dados_anteriores_json = table.Column<string>(type: "jsonb", nullable: true),
                    dados_novos_json = table.Column<string>(type: "jsonb", nullable: true),
                    data_evento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_fci_historico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fci_imposto",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    aliquota = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_fci_imposto", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fci_lancamento_estoque",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    documento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    documento_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_documento = table.Column<int>(type: "integer", nullable: false),
                    numero_documento = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    armazem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade_entrada = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 2, nullable: false),
                    quantidade_saida = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 2, nullable: false),
                    modo_calculo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    data_lancamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_fci_lancamento_estoque", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fci_lancamento_razao",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    documento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    documento_item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_documento = table.Column<int>(type: "integer", nullable: false),
                    numero_documento = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    conta_id = table.Column<Guid>(type: "uuid", nullable: false),
                    debito = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    credito = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    categoria = table.Column<int>(type: "integer", nullable: false),
                    data_lancamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_fci_lancamento_razao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fci_pdf_documento",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    documento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_saida = table.Column<int>(type: "integer", nullable: false),
                    pagina_origem = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    arquivo_referencia = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    data_geracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_fci_pdf_documento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fci_preferencia_geral",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    mostrar_moeda = table.Column<bool>(type: "boolean", nullable: false),
                    permitir_caixa_negativo = table.Column<bool>(type: "boolean", nullable: false),
                    permitir_estoque_negativo = table.Column<bool>(type: "boolean", nullable: false),
                    modo_calculo_estoque = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    controlar_limite_credito = table.Column<bool>(type: "boolean", nullable: false),
                    permitir_desconto = table.Column<bool>(type: "boolean", nullable: false),
                    imposto_na_compra = table.Column<bool>(type: "boolean", nullable: false),
                    imposto_na_venda = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_fci_preferencia_geral", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fci_documento_item",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    documento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_origem_id = table.Column<Guid>(type: "uuid", nullable: true),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    unidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 2, nullable: false),
                    valor_unitario = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 2, nullable: false),
                    lote_id = table.Column<Guid>(type: "uuid", nullable: true),
                    desconto = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 2, nullable: false),
                    valor_desconto = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 2, nullable: false),
                    imposto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    aliquota_imposto = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 2, nullable: true),
                    valor_imposto = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 2, nullable: true),
                    valor_bruto = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 2, nullable: false),
                    valor_liquido = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 2, nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 2, nullable: false),
                    conta_receita_id = table.Column<Guid>(type: "uuid", nullable: false),
                    projeto_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_fci_documento_item", x => x.id);
                    table.ForeignKey(
                        name: "f_k_fci_documento_item_fci_documento_comercial_documento_id",
                        column: x => x.documento_id,
                        principalSchema: "vendas",
                        principalTable: "fci_documento_comercial",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix__fci_configuracao_documento_sync_id",
                schema: "vendas",
                table: "fci_configuracao_documento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fci_configuracao_documento_tenant_id",
                schema: "vendas",
                table: "fci_configuracao_documento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "uq_fci_config_tenant_tipo",
                schema: "vendas",
                table: "fci_configuracao_documento",
                columns: new[] { "tenant_id", "tipo_documento" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fci_documento_comercial_sync_id",
                schema: "vendas",
                table: "fci_documento_comercial",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fci_documento_comercial_tenant_id",
                schema: "vendas",
                table: "fci_documento_comercial",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_fci_documento_tenant_cliente",
                schema: "vendas",
                table: "fci_documento_comercial",
                columns: new[] { "tenant_id", "cliente_id" });

            migrationBuilder.CreateIndex(
                name: "uq_fci_documento_tenant_tipo_numero",
                schema: "vendas",
                table: "fci_documento_comercial",
                columns: new[] { "tenant_id", "tipo_documento", "numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_fci_documento_item_documento_id",
                schema: "vendas",
                table: "fci_documento_item",
                column: "documento_id");

            migrationBuilder.CreateIndex(
                name: "ix__fci_documento_item_sync_id",
                schema: "vendas",
                table: "fci_documento_item",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fci_documento_item_tenant_id",
                schema: "vendas",
                table: "fci_documento_item",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_fci_item_tenant_documento",
                schema: "vendas",
                table: "fci_documento_item",
                columns: new[] { "tenant_id", "documento_id" });

            migrationBuilder.CreateIndex(
                name: "ix__fci_historico_sync_id",
                schema: "vendas",
                table: "fci_historico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fci_historico_tenant_id",
                schema: "vendas",
                table: "fci_historico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_fci_historico_tenant_documento",
                schema: "vendas",
                table: "fci_historico",
                columns: new[] { "tenant_id", "documento_id" });

            migrationBuilder.CreateIndex(
                name: "ix__fci_imposto_sync_id",
                schema: "vendas",
                table: "fci_imposto",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fci_imposto_tenant_id",
                schema: "vendas",
                table: "fci_imposto",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "uq_fci_imposto_tenant_nome",
                schema: "vendas",
                table: "fci_imposto",
                columns: new[] { "tenant_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fci_lancamento_estoque_sync_id",
                schema: "vendas",
                table: "fci_lancamento_estoque",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fci_lancamento_estoque_tenant_id",
                schema: "vendas",
                table: "fci_lancamento_estoque",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_fci_estoque_tenant_documento",
                schema: "vendas",
                table: "fci_lancamento_estoque",
                columns: new[] { "tenant_id", "documento_id" });

            migrationBuilder.CreateIndex(
                name: "ix__fci_lancamento_razao_sync_id",
                schema: "vendas",
                table: "fci_lancamento_razao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fci_lancamento_razao_tenant_id",
                schema: "vendas",
                table: "fci_lancamento_razao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_fci_razao_tenant_documento",
                schema: "vendas",
                table: "fci_lancamento_razao",
                columns: new[] { "tenant_id", "documento_id" });

            migrationBuilder.CreateIndex(
                name: "ix__fci_pdf_documento_sync_id",
                schema: "vendas",
                table: "fci_pdf_documento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fci_pdf_documento_tenant_id",
                schema: "vendas",
                table: "fci_pdf_documento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_fci_pdf_tenant_documento",
                schema: "vendas",
                table: "fci_pdf_documento",
                columns: new[] { "tenant_id", "documento_id" });

            migrationBuilder.CreateIndex(
                name: "ix__fci_preferencia_geral_sync_id",
                schema: "vendas",
                table: "fci_preferencia_geral",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fci_preferencia_geral_tenant_id",
                schema: "vendas",
                table: "fci_preferencia_geral",
                column: "tenant_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fci_configuracao_documento",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "fci_documento_item",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "fci_historico",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "fci_imposto",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "fci_lancamento_estoque",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "fci_lancamento_razao",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "fci_pdf_documento",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "fci_preferencia_geral",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "fci_documento_comercial",
                schema: "vendas");
        }
    }
}
