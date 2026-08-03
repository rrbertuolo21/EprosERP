using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Financeiro.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmodulosFinanceiroEvolucao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ativos_fixos",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_aquisicao_id = table.Column<Guid>(type: "uuid", nullable: true),
                    estado_conservacao_id = table.Column<Guid>(type: "uuid", nullable: true),
                    grupo_bem_id = table.Column<Guid>(type: "uuid", nullable: true),
                    setor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    fornecedor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: true),
                    numero_nb = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    descricao = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    numero_serie = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    funcao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    numero_nota_fiscal = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    chave_nfe = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    data_aquisicao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_aceite = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_cadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_contabilizado = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_vistoria = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_marcacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_baixa = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    vencimento_garantia = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    valor_original = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_compra = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_atualizado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_baixa = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    deprecia = table.Column<bool>(type: "boolean", nullable: false),
                    metodo_depreciacao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    tipo_depreciacao = table.Column<int>(type: "integer", nullable: true),
                    inicio_depreciacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ultima_depreciacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    taxa_anual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    taxa_mensal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    taxa_acelerada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    taxa_incentivada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_ativos_fixos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "balancetes_consolidados",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    grupo_consolidacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    periodo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    estado = table.Column<int>(type: "integer", nullable: false),
                    total_debito = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    total_credito = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    saldo_final = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_balancetes_consolidados", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "caixas_operacionais",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    local_id = table.Column<Guid>(type: "uuid", nullable: true),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    valor_inicial = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_fechamento = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    total_comprovantes_cartao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    total_cheques = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    observacao_fechamento = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    data_fechamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_caixas_operacionais", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "centros_custo",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    descricao = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    pai_id = table.Column<Guid>(type: "uuid", nullable: true),
                    estado = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_centros_custo", x => x.id);
                    table.ForeignKey(
                        name: "fk_centro_custo_pai",
                        column: x => x.pai_id,
                        principalSchema: "financas",
                        principalTable: "centros_custo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cheques",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo = table.Column<int>(type: "integer", nullable: false),
                    tipo_pessoa = table.Column<int>(type: "integer", nullable: false),
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    emissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    vencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    situacao = table.Column<int>(type: "integer", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    conta_id = table.Column<Guid>(type: "uuid", nullable: true),
                    caixa_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_cheques", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "contas_financeiras",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    numero_conta = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    tipo_conta_id = table.Column<Guid>(type: "uuid", nullable: true),
                    nota = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    fechada = table.Column<bool>(type: "boolean", nullable: false),
                    saldo_abertura = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_contas_financeiras", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "demonstrativos",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    grupo_consolidacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    periodo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    estado = table.Column<int>(type: "integer", nullable: false),
                    data_publicacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    usuario_publicacao_id = table.Column<Guid>(type: "uuid", nullable: true),
                    total_agregado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_demonstrativos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "depreciacoes_mensais",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ativo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    competencia = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    metodo_depreciacao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    taxa_aplicada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    contabilizado = table.Column<bool>(type: "boolean", nullable: false),
                    data_contabilizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_depreciacoes_mensais", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "dimensoes_analiticas",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    valor = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
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
                    table.PrimaryKey("p_k_dimensoes_analiticas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "eliminacoes_intercompany",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    grupo_consolidacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    periodo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    empresa_origem_id = table.Column<Guid>(type: "uuid", nullable: true),
                    empresa_destino_id = table.Column<Guid>(type: "uuid", nullable: true),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("p_k_eliminacoes_intercompany", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "exposicoes_cambiais",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    moeda_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor_exposto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    data_referencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    origem_exposicao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    entidade_origem_tipo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    entidade_origem_id = table.Column<Guid>(type: "uuid", nullable: true),
                    taxa_referencia_id = table.Column<Guid>(type: "uuid", nullable: true),
                    valor_moeda_base = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_exposicoes_cambiais", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grupos_bem",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    conta_ativo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    conta_depreciacao_id = table.Column<Guid>(type: "uuid", nullable: true),
                    conta_baixa_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_grupos_bem", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grupos_consolidacao",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("p_k_grupos_consolidacao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "meta_categorias",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    escopo = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_meta_categorias", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "moedas",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_iso = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    simbolo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("p_k_moedas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "movimentacoes_ativo",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ativo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_movimentacao = table.Column<int>(type: "integer", nullable: false),
                    data_movimentacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("p_k_movimentacoes_ativo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "movimentos_financeiros",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    emissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    caixa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    conta_id = table.Column<Guid>(type: "uuid", nullable: true),
                    credito = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    debito = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    cheque_id = table.Column<Guid>(type: "uuid", nullable: true),
                    contas_pagar_id = table.Column<Guid>(type: "uuid", nullable: true),
                    contas_receber_id = table.Column<Guid>(type: "uuid", nullable: true),
                    conciliado = table.Column<bool>(type: "boolean", nullable: false),
                    pagamento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    planejamento = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_movimentos_financeiros", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "periodos_orcamentarios",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_periodos_orcamentarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "planos_contrato",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    periodicidade = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_planos_contrato", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "programas_subsidio",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequencia_exibicao = table.Column<long>(type: "bigint", nullable: true),
                    orgao = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    vigencia_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    vigencia_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    estado = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_programas_subsidio", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reavaliacoes_titulo",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_reavaliacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    total_valor_original = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_valor_reavaliado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_variacao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("p_k_reavaliacoes_titulo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "versoes_orcamentarias",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    periodo_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    periodo_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_versoes_orcamentarias", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "balancete_linhas",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    balancete_consolidado_id = table.Column<Guid>(type: "uuid", nullable: false),
                    conta_id = table.Column<Guid>(type: "uuid", nullable: true),
                    codigo_conta = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    nome_conta = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    saldo_anterior = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    debito = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    credito = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    saldo_final = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_balancete_linhas", x => x.id);
                    table.ForeignKey(
                        name: "f_k_balancete_linhas_balancetes_consolidados_balancete_consolid~",
                        column: x => x.balancete_consolidado_id,
                        principalSchema: "financas",
                        principalTable: "balancetes_consolidados",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "alocacoes_centro_custo",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    titulo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_titulo = table.Column<int>(type: "integer", nullable: true),
                    centro_custo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    percentual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_rateado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_alocacoes_centro_custo", x => x.id);
                    table.ForeignKey(
                        name: "fk_alocacao_centro_custo",
                        column: x => x.centro_custo_id,
                        principalSchema: "financas",
                        principalTable: "centros_custo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "transacoes_conta_financeira",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    conta_financeira_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tipo = table.Column<int>(type: "integer", nullable: false),
                    subtipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    data_operacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    nota = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    transacao_origem_id = table.Column<Guid>(type: "uuid", nullable: true),
                    pagamento_transacao_id = table.Column<Guid>(type: "uuid", nullable: true),
                    transferencia_par_id = table.Column<Guid>(type: "uuid", nullable: true),
                    conta_transferencia_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_transacoes_conta_financeira", x => x.id);
                    table.ForeignKey(
                        name: "f_k_transacoes_conta_financeira_contas_financeiras_conta_financ~",
                        column: x => x.conta_financeira_id,
                        principalSchema: "financas",
                        principalTable: "contas_financeiras",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "demonstrativo_linhas",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    demonstrativo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: true),
                    codigo_linha = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    descricao_linha = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    tipo_linha = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("p_k_demonstrativo_linhas", x => x.id);
                    table.ForeignKey(
                        name: "f_k_demonstrativo_linhas_demonstrativos_demonstrativo_id",
                        column: x => x.demonstrativo_id,
                        principalSchema: "financas",
                        principalTable: "demonstrativos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "grupos_empresa",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    grupo_consolidacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_grupos_empresa", x => x.id);
                    table.ForeignKey(
                        name: "f_k_grupos_empresa_grupos_consolidacao_grupo_consolidacao_id",
                        column: x => x.grupo_consolidacao_id,
                        principalSchema: "financas",
                        principalTable: "grupos_consolidacao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "metas",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    categoria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    prioridade = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_alvo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_metas", x => x.id);
                    table.ForeignKey(
                        name: "fk_meta_categoria",
                        column: x => x.categoria_id,
                        principalSchema: "financas",
                        principalTable: "meta_categorias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "taxas_cambio",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    moeda_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_taxa = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    taxa_compra = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    taxa_venda = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    origem_taxa = table.Column<int>(type: "integer", nullable: true),
                    observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("p_k_taxas_cambio", x => x.id);
                    table.ForeignKey(
                        name: "fk_taxa_cambio_moeda",
                        column: x => x.moeda_id,
                        principalSchema: "financas",
                        principalTable: "moedas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "budgets",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    periodo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_budgets", x => x.id);
                    table.ForeignKey(
                        name: "fk_budget_periodo",
                        column: x => x.periodo_id,
                        principalSchema: "financas",
                        principalTable: "periodos_orcamentarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contratos_financeiros",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    plano_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    motivo_cancelamento = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("p_k_contratos_financeiros", x => x.id);
                    table.ForeignKey(
                        name: "fk_contrato_financeiro_plano",
                        column: x => x.plano_id,
                        principalSchema: "financas",
                        principalTable: "planos_contrato",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "utilizacoes_subsidio",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    programa_subsidio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    titulo_pagar_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor_elegivel = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_utilizacoes_subsidio", x => x.id);
                    table.ForeignKey(
                        name: "fk_utilizacao_subsidio_programa",
                        column: x => x.programa_subsidio_id,
                        principalSchema: "financas",
                        principalTable: "programas_subsidio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reavaliacoes_item",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reavaliacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    moeda_id = table.Column<Guid>(type: "uuid", nullable: false),
                    titulo_tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    titulo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    taxa_cambio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor_original_moeda = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_reavaliado_base = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_variacao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    contabilizado = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_reavaliacoes_item", x => x.id);
                    table.ForeignKey(
                        name: "f_k_reavaliacoes_item__reavaliacoes_titulo_reavaliacao_id",
                        column: x => x.reavaliacao_id,
                        principalSchema: "financas",
                        principalTable: "reavaliacoes_titulo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "linhas_orcamento",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    versao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    conta_id = table.Column<Guid>(type: "uuid", nullable: false),
                    centro_custo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    periodo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    valor_orcado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_realizado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    variacao_valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    variacao_percentual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_linhas_orcamento", x => x.id);
                    table.ForeignKey(
                        name: "f_k_linhas_orcamento__versoes_orcamentarias_versao_id",
                        column: x => x.versao_id,
                        principalSchema: "financas",
                        principalTable: "versoes_orcamentarias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "alocacoes_dimensao",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    alocacao_centro_custo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dimensao_analitica_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_alocacoes_dimensao", x => x.id);
                    table.ForeignKey(
                        name: "fk_alocacao_dimensao_alocacao",
                        column: x => x.alocacao_centro_custo_id,
                        principalSchema: "financas",
                        principalTable: "alocacoes_centro_custo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_alocacao_dimensao_dimensao",
                        column: x => x.dimensao_analitica_id,
                        principalSchema: "financas",
                        principalTable: "dimensoes_analiticas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "meta_contribuicoes",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    meta_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    data = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_meta_contribuicoes", x => x.id);
                    table.ForeignKey(
                        name: "f_k_meta_contribuicoes_metas_meta_id",
                        column: x => x.meta_id,
                        principalSchema: "financas",
                        principalTable: "metas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "meta_milestones",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    meta_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    descricao = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
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
                    table.PrimaryKey("p_k_meta_milestones", x => x.id);
                    table.ForeignKey(
                        name: "f_k_meta_milestones_metas_meta_id",
                        column: x => x.meta_id,
                        principalSchema: "financas",
                        principalTable: "metas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "meta_trackings",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    meta_id = table.Column<Guid>(type: "uuid", nullable: false),
                    percentual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status_progresso = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    data = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_meta_trackings", x => x.id);
                    table.ForeignKey(
                        name: "f_k_meta_trackings_metas_meta_id",
                        column: x => x.meta_id,
                        principalSchema: "financas",
                        principalTable: "metas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "budget_alocacoes",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    budget_id = table.Column<Guid>(type: "uuid", nullable: false),
                    conta_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor_alocado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    auto_aprovar = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_budget_alocacoes", x => x.id);
                    table.ForeignKey(
                        name: "fk_budget_alocacao_budget",
                        column: x => x.budget_id,
                        principalSchema: "financas",
                        principalTable: "budgets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "faturas_recorrentes",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contrato_id = table.Column<Guid>(type: "uuid", nullable: false),
                    competencia = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    titulo_contas_receber_id = table.Column<Guid>(type: "uuid", nullable: true),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_faturas_recorrentes", x => x.id);
                    table.ForeignKey(
                        name: "fk_fatura_recorrente_contrato",
                        column: x => x.contrato_id,
                        principalSchema: "financas",
                        principalTable: "contratos_financeiros",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reajustes_contrato",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contrato_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_reajuste = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    valor_anterior = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_novo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    aprovacao_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_reajustes_contrato", x => x.id);
                    table.ForeignKey(
                        name: "fk_reajuste_contrato",
                        column: x => x.contrato_id,
                        principalSchema: "financas",
                        principalTable: "contratos_financeiros",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_alocacoes_centro_custo_centro_custo_id",
                schema: "financas",
                table: "alocacoes_centro_custo",
                column: "centro_custo_id");

            migrationBuilder.CreateIndex(
                name: "ix__alocacao_centro_custo_sync_id",
                schema: "financas",
                table: "alocacoes_centro_custo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__alocacao_centro_custo_tenant_id",
                schema: "financas",
                table: "alocacoes_centro_custo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_alocacao_cc_titulo",
                schema: "financas",
                table: "alocacoes_centro_custo",
                column: "titulo_id");

            migrationBuilder.CreateIndex(
                name: "i_x_alocacoes_dimensao_dimensao_analitica_id",
                schema: "financas",
                table: "alocacoes_dimensao",
                column: "dimensao_analitica_id");

            migrationBuilder.CreateIndex(
                name: "ix__alocacao_dimensao_sync_id",
                schema: "financas",
                table: "alocacoes_dimensao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__alocacao_dimensao_tenant_id",
                schema: "financas",
                table: "alocacoes_dimensao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_alocacao_dimensao_vinculo",
                schema: "financas",
                table: "alocacoes_dimensao",
                columns: new[] { "alocacao_centro_custo_id", "dimensao_analitica_id" });

            migrationBuilder.CreateIndex(
                name: "ix__ativo_fixo_sync_id",
                schema: "financas",
                table: "ativos_fixos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ativo_fixo_tenant_id",
                schema: "financas",
                table: "ativos_fixos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ativo_fixo_tenant_nb",
                schema: "financas",
                table: "ativos_fixos",
                columns: new[] { "tenant_id", "numero_nb" });

            migrationBuilder.CreateIndex(
                name: "ix_ativo_fixo_tenant_status",
                schema: "financas",
                table: "ativos_fixos",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__balancete_linha_sync_id",
                schema: "financas",
                table: "balancete_linhas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__balancete_linha_tenant_id",
                schema: "financas",
                table: "balancete_linhas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_balancete_linha_balancete",
                schema: "financas",
                table: "balancete_linhas",
                column: "balancete_consolidado_id");

            migrationBuilder.CreateIndex(
                name: "ix__balancete_consolidado_sync_id",
                schema: "financas",
                table: "balancetes_consolidados",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__balancete_consolidado_tenant_id",
                schema: "financas",
                table: "balancetes_consolidados",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_balancete_grupo_periodo",
                schema: "financas",
                table: "balancetes_consolidados",
                columns: new[] { "grupo_consolidacao_id", "periodo" });

            migrationBuilder.CreateIndex(
                name: "ix__budget_alocacao_sync_id",
                schema: "financas",
                table: "budget_alocacoes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__budget_alocacao_tenant_id",
                schema: "financas",
                table: "budget_alocacoes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_budget_alocacao_budget",
                schema: "financas",
                table: "budget_alocacoes",
                column: "budget_id");

            migrationBuilder.CreateIndex(
                name: "ix__budget_sync_id",
                schema: "financas",
                table: "budgets",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__budget_tenant_id",
                schema: "financas",
                table: "budgets",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_budget_periodo",
                schema: "financas",
                table: "budgets",
                column: "periodo_id");

            migrationBuilder.CreateIndex(
                name: "ix__caixa_operacional_sync_id",
                schema: "financas",
                table: "caixas_operacionais",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__caixa_operacional_tenant_id",
                schema: "financas",
                table: "caixas_operacionais",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_caixa_operacional_tenant_status",
                schema: "financas",
                table: "caixas_operacionais",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "i_x_centros_custo_pai_id",
                schema: "financas",
                table: "centros_custo",
                column: "pai_id");

            migrationBuilder.CreateIndex(
                name: "ix__centro_custo_sync_id",
                schema: "financas",
                table: "centros_custo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__centro_custo_tenant_id",
                schema: "financas",
                table: "centros_custo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_centro_custo_tenant_codigo",
                schema: "financas",
                table: "centros_custo",
                columns: new[] { "tenant_id", "codigo" });

            migrationBuilder.CreateIndex(
                name: "ix__cheque_sync_id",
                schema: "financas",
                table: "cheques",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__cheque_tenant_id",
                schema: "financas",
                table: "cheques",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_cheque_tenant_situacao",
                schema: "financas",
                table: "cheques",
                columns: new[] { "tenant_id", "situacao" });

            migrationBuilder.CreateIndex(
                name: "ix_cheque_vencimento",
                schema: "financas",
                table: "cheques",
                column: "vencimento");

            migrationBuilder.CreateIndex(
                name: "ix__conta_financeira_sync_id",
                schema: "financas",
                table: "contas_financeiras",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__conta_financeira_tenant_id",
                schema: "financas",
                table: "contas_financeiras",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_conta_financeira_tenant_fechada",
                schema: "financas",
                table: "contas_financeiras",
                columns: new[] { "tenant_id", "fechada" });

            migrationBuilder.CreateIndex(
                name: "i_x_contratos_financeiros_plano_id",
                schema: "financas",
                table: "contratos_financeiros",
                column: "plano_id");

            migrationBuilder.CreateIndex(
                name: "ix__contrato_financeiro_sync_id",
                schema: "financas",
                table: "contratos_financeiros",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__contrato_financeiro_tenant_id",
                schema: "financas",
                table: "contratos_financeiros",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_contrato_financeiro_pessoa",
                schema: "financas",
                table: "contratos_financeiros",
                columns: new[] { "tenant_id", "pessoa_id" });

            migrationBuilder.CreateIndex(
                name: "ix_contrato_financeiro_status",
                schema: "financas",
                table: "contratos_financeiros",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__demonstrativo_linha_sync_id",
                schema: "financas",
                table: "demonstrativo_linhas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__demonstrativo_linha_tenant_id",
                schema: "financas",
                table: "demonstrativo_linhas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_demonstrativo_linha_demo",
                schema: "financas",
                table: "demonstrativo_linhas",
                column: "demonstrativo_id");

            migrationBuilder.CreateIndex(
                name: "ix__demonstrativo_sync_id",
                schema: "financas",
                table: "demonstrativos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__demonstrativo_tenant_id",
                schema: "financas",
                table: "demonstrativos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_demonstrativo_grupo_periodo",
                schema: "financas",
                table: "demonstrativos",
                columns: new[] { "grupo_consolidacao_id", "periodo" });

            migrationBuilder.CreateIndex(
                name: "ix__depreciacao_mensal_sync_id",
                schema: "financas",
                table: "depreciacoes_mensais",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__depreciacao_mensal_tenant_id",
                schema: "financas",
                table: "depreciacoes_mensais",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_depreciacao_ativo_competencia",
                schema: "financas",
                table: "depreciacoes_mensais",
                columns: new[] { "ativo_id", "competencia" });

            migrationBuilder.CreateIndex(
                name: "ix__dimensao_analitica_sync_id",
                schema: "financas",
                table: "dimensoes_analiticas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__dimensao_analitica_tenant_id",
                schema: "financas",
                table: "dimensoes_analiticas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_dimensao_analitica_tenant_tipo",
                schema: "financas",
                table: "dimensoes_analiticas",
                columns: new[] { "tenant_id", "tipo" });

            migrationBuilder.CreateIndex(
                name: "ix__eliminacao_intercompany_sync_id",
                schema: "financas",
                table: "eliminacoes_intercompany",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__eliminacao_intercompany_tenant_id",
                schema: "financas",
                table: "eliminacoes_intercompany",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_eliminacao_grupo_periodo",
                schema: "financas",
                table: "eliminacoes_intercompany",
                columns: new[] { "grupo_consolidacao_id", "periodo" });

            migrationBuilder.CreateIndex(
                name: "ix__exposicao_cambial_sync_id",
                schema: "financas",
                table: "exposicoes_cambiais",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__exposicao_cambial_tenant_id",
                schema: "financas",
                table: "exposicoes_cambiais",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_exposicao_cambial_moeda_status",
                schema: "financas",
                table: "exposicoes_cambiais",
                columns: new[] { "tenant_id", "moeda_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__fatura_recorrente_sync_id",
                schema: "financas",
                table: "faturas_recorrentes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fatura_recorrente_tenant_id",
                schema: "financas",
                table: "faturas_recorrentes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_fatura_recorrente_contrato_comp",
                schema: "financas",
                table: "faturas_recorrentes",
                columns: new[] { "contrato_id", "competencia" });

            migrationBuilder.CreateIndex(
                name: "ix__grupo_bem_sync_id",
                schema: "financas",
                table: "grupos_bem",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__grupo_bem_tenant_id",
                schema: "financas",
                table: "grupos_bem",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_grupo_bem_tenant_codigo",
                schema: "financas",
                table: "grupos_bem",
                columns: new[] { "tenant_id", "codigo" });

            migrationBuilder.CreateIndex(
                name: "ix__grupo_consolidacao_sync_id",
                schema: "financas",
                table: "grupos_consolidacao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__grupo_consolidacao_tenant_id",
                schema: "financas",
                table: "grupos_consolidacao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_grupo_consolidacao_tenant_codigo",
                schema: "financas",
                table: "grupos_consolidacao",
                columns: new[] { "tenant_id", "codigo" });

            migrationBuilder.CreateIndex(
                name: "ix__grupo_empresa_sync_id",
                schema: "financas",
                table: "grupos_empresa",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__grupo_empresa_tenant_id",
                schema: "financas",
                table: "grupos_empresa",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_grupo_empresa_vinculo",
                schema: "financas",
                table: "grupos_empresa",
                columns: new[] { "grupo_consolidacao_id", "empresa_id" });

            migrationBuilder.CreateIndex(
                name: "ix__linha_orcamento_sync_id",
                schema: "financas",
                table: "linhas_orcamento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__linha_orcamento_tenant_id",
                schema: "financas",
                table: "linhas_orcamento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_linha_orcamento_versao",
                schema: "financas",
                table: "linhas_orcamento",
                column: "versao_id");

            migrationBuilder.CreateIndex(
                name: "ix__meta_categoria_sync_id",
                schema: "financas",
                table: "meta_categorias",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__meta_categoria_tenant_id",
                schema: "financas",
                table: "meta_categorias",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_meta_categoria_tenant_codigo",
                schema: "financas",
                table: "meta_categorias",
                columns: new[] { "tenant_id", "codigo" });

            migrationBuilder.CreateIndex(
                name: "ix__meta_contribuicao_sync_id",
                schema: "financas",
                table: "meta_contribuicoes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__meta_contribuicao_tenant_id",
                schema: "financas",
                table: "meta_contribuicoes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_meta_contribuicao_meta",
                schema: "financas",
                table: "meta_contribuicoes",
                column: "meta_id");

            migrationBuilder.CreateIndex(
                name: "ix__meta_milestone_sync_id",
                schema: "financas",
                table: "meta_milestones",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__meta_milestone_tenant_id",
                schema: "financas",
                table: "meta_milestones",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_meta_milestone_meta",
                schema: "financas",
                table: "meta_milestones",
                column: "meta_id");

            migrationBuilder.CreateIndex(
                name: "ix__meta_tracking_sync_id",
                schema: "financas",
                table: "meta_trackings",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__meta_tracking_tenant_id",
                schema: "financas",
                table: "meta_trackings",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_meta_tracking_meta",
                schema: "financas",
                table: "meta_trackings",
                column: "meta_id");

            migrationBuilder.CreateIndex(
                name: "ix__meta_sync_id",
                schema: "financas",
                table: "metas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__meta_tenant_id",
                schema: "financas",
                table: "metas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_meta_categoria_ref",
                schema: "financas",
                table: "metas",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "ix__moeda_sync_id",
                schema: "financas",
                table: "moedas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__moeda_tenant_id",
                schema: "financas",
                table: "moedas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_moeda_tenant_codigo",
                schema: "financas",
                table: "moedas",
                columns: new[] { "tenant_id", "codigo_iso" });

            migrationBuilder.CreateIndex(
                name: "ix__movimentacao_ativo_sync_id",
                schema: "financas",
                table: "movimentacoes_ativo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__movimentacao_ativo_tenant_id",
                schema: "financas",
                table: "movimentacoes_ativo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_movimentacao_ativo_data",
                schema: "financas",
                table: "movimentacoes_ativo",
                columns: new[] { "ativo_id", "data_movimentacao" });

            migrationBuilder.CreateIndex(
                name: "ix__movimento_financeiro_sync_id",
                schema: "financas",
                table: "movimentos_financeiros",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__movimento_financeiro_tenant_id",
                schema: "financas",
                table: "movimentos_financeiros",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_movimento_financeiro_conciliado",
                schema: "financas",
                table: "movimentos_financeiros",
                columns: new[] { "tenant_id", "conciliado" });

            migrationBuilder.CreateIndex(
                name: "ix_movimento_financeiro_emissao",
                schema: "financas",
                table: "movimentos_financeiros",
                column: "emissao");

            migrationBuilder.CreateIndex(
                name: "ix__periodo_orcamentario_sync_id",
                schema: "financas",
                table: "periodos_orcamentarios",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__periodo_orcamentario_tenant_id",
                schema: "financas",
                table: "periodos_orcamentarios",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_periodo_orcamentario_tenant_status",
                schema: "financas",
                table: "periodos_orcamentarios",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__plano_contrato_sync_id",
                schema: "financas",
                table: "planos_contrato",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__plano_contrato_tenant_id",
                schema: "financas",
                table: "planos_contrato",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_plano_contrato_tenant_status",
                schema: "financas",
                table: "planos_contrato",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__programa_subsidio_sync_id",
                schema: "financas",
                table: "programas_subsidio",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__programa_subsidio_tenant_id",
                schema: "financas",
                table: "programas_subsidio",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_programa_subsidio_tenant_estado",
                schema: "financas",
                table: "programas_subsidio",
                columns: new[] { "tenant_id", "estado" });

            migrationBuilder.CreateIndex(
                name: "ix__reajuste_contrato_sync_id",
                schema: "financas",
                table: "reajustes_contrato",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__reajuste_contrato_tenant_id",
                schema: "financas",
                table: "reajustes_contrato",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_reajuste_contrato",
                schema: "financas",
                table: "reajustes_contrato",
                column: "contrato_id");

            migrationBuilder.CreateIndex(
                name: "ix__reavaliacao_item_sync_id",
                schema: "financas",
                table: "reavaliacoes_item",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__reavaliacao_item_tenant_id",
                schema: "financas",
                table: "reavaliacoes_item",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_reavaliacao_item_reavaliacao",
                schema: "financas",
                table: "reavaliacoes_item",
                column: "reavaliacao_id");

            migrationBuilder.CreateIndex(
                name: "ix__reavaliacao_titulo_sync_id",
                schema: "financas",
                table: "reavaliacoes_titulo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__reavaliacao_titulo_tenant_id",
                schema: "financas",
                table: "reavaliacoes_titulo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_reavaliacao_titulo_data",
                schema: "financas",
                table: "reavaliacoes_titulo",
                columns: new[] { "tenant_id", "data_reavaliacao" });

            migrationBuilder.CreateIndex(
                name: "ix__taxa_cambio_sync_id",
                schema: "financas",
                table: "taxas_cambio",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__taxa_cambio_tenant_id",
                schema: "financas",
                table: "taxas_cambio",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_taxa_cambio_moeda_data",
                schema: "financas",
                table: "taxas_cambio",
                columns: new[] { "moeda_id", "data_taxa" });

            migrationBuilder.CreateIndex(
                name: "ix__transacao_conta_financeira_sync_id",
                schema: "financas",
                table: "transacoes_conta_financeira",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__transacao_conta_financeira_tenant_id",
                schema: "financas",
                table: "transacoes_conta_financeira",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_transacao_conta_data",
                schema: "financas",
                table: "transacoes_conta_financeira",
                columns: new[] { "conta_financeira_id", "data_operacao" });

            migrationBuilder.CreateIndex(
                name: "ix__utilizacao_subsidio_sync_id",
                schema: "financas",
                table: "utilizacoes_subsidio",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__utilizacao_subsidio_tenant_id",
                schema: "financas",
                table: "utilizacoes_subsidio",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_utilizacao_subsidio_programa",
                schema: "financas",
                table: "utilizacoes_subsidio",
                column: "programa_subsidio_id");

            migrationBuilder.CreateIndex(
                name: "ix__versao_orcamentaria_sync_id",
                schema: "financas",
                table: "versoes_orcamentarias",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__versao_orcamentaria_tenant_id",
                schema: "financas",
                table: "versoes_orcamentarias",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_versao_orcamentaria_tenant_status",
                schema: "financas",
                table: "versoes_orcamentarias",
                columns: new[] { "tenant_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alocacoes_dimensao",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "ativos_fixos",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "balancete_linhas",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "budget_alocacoes",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "caixas_operacionais",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "cheques",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "demonstrativo_linhas",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "depreciacoes_mensais",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "eliminacoes_intercompany",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "exposicoes_cambiais",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "faturas_recorrentes",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "grupos_bem",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "grupos_empresa",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "linhas_orcamento",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "meta_contribuicoes",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "meta_milestones",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "meta_trackings",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "movimentacoes_ativo",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "movimentos_financeiros",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "reajustes_contrato",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "reavaliacoes_item",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "taxas_cambio",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "transacoes_conta_financeira",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "utilizacoes_subsidio",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "alocacoes_centro_custo",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "dimensoes_analiticas",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "balancetes_consolidados",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "budgets",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "demonstrativos",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "grupos_consolidacao",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "versoes_orcamentarias",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "metas",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "contratos_financeiros",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "reavaliacoes_titulo",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "moedas",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "contas_financeiras",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "programas_subsidio",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "centros_custo",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "periodos_orcamentarios",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "meta_categorias",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "planos_contrato",
                schema: "financas");
        }
    }
}
