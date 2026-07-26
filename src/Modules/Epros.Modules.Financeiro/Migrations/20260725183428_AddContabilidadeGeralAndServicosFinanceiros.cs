using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Financeiro.Migrations
{
    /// <inheritdoc />
    public partial class AddContabilidadeGeralAndServicosFinanceiros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cobrancas_email",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sacado_id = table.Column<Guid>(type: "uuid", nullable: true),
                    nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    periodo = table.Column<string>(type: "text", nullable: true),
                    servicos = table.Column<string>(type: "text", nullable: true),
                    conta = table.Column<string>(type: "text", nullable: true),
                    link_externo = table.Column<string>(type: "text", nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    emails = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    area = table.Column<int>(type: "integer", nullable: false),
                    comprovante_confirmacao = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_cobrancas_email", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "configuracoes_cedente",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "text", nullable: true),
                    documento = table.Column<string>(type: "text", nullable: true),
                    endereco = table.Column<string>(type: "text", nullable: true),
                    numero = table.Column<string>(type: "text", nullable: true),
                    bairro = table.Column<string>(type: "text", nullable: true),
                    cidade = table.Column<string>(type: "text", nullable: true),
                    cep = table.Column<string>(type: "text", nullable: true),
                    u_f = table.Column<string>(type: "text", nullable: true),
                    logo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    receber_ate_dias = table.Column<int>(type: "integer", nullable: false),
                    dias_antecedencia = table.Column<int>(type: "integer", nullable: false),
                    multa_atraso = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    juro = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    instrucao1 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    instrucao2 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    instrucao3 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    instrucao4 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("p_k_configuracoes_cedente", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "contas_contabeis",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_conta = table.Column<string>(type: "text", nullable: false),
                    nome_conta = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    conta_pai_id = table.Column<Guid>(type: "uuid", nullable: true),
                    nome_conta_pai = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    nivel = table.Column<int>(type: "integer", nullable: false),
                    tipo_conta = table.Column<int>(type: "integer", nullable: false),
                    aceita_lancamento = table.Column<bool>(type: "boolean", nullable: false),
                    participa_contabilidade_geral = table.Column<bool>(type: "boolean", nullable: false),
                    participa_orcamento = table.Column<bool>(type: "boolean", nullable: false),
                    participa_depreciacao = table.Column<bool>(type: "boolean", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    fornecedor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: true),
                    banco_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_despesa_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_contas_contabeis", x => x.id);
                    table.ForeignKey(
                        name: "fk_conta_contabil_pai",
                        column: x => x.conta_pai_id,
                        principalSchema: "financas",
                        principalTable: "contas_contabeis",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "contas_emissoras",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    banco_id = table.Column<Guid>(type: "uuid", nullable: false),
                    configuracao_cedente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    nome_banco = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    carteira = table.Column<string>(type: "text", nullable: true),
                    agencia = table.Column<string>(type: "text", nullable: true),
                    digito_agencia = table.Column<string>(type: "text", nullable: true),
                    conta = table.Column<string>(type: "text", nullable: true),
                    digito_conta = table.Column<string>(type: "text", nullable: true),
                    especie = table.Column<string>(type: "text", nullable: true),
                    nosso_numero_atual = table.Column<long>(type: "bigint", nullable: false),
                    tipo_cobranca = table.Column<string>(type: "text", nullable: true),
                    convenio = table.Column<string>(type: "text", nullable: true),
                    contrato = table.Column<string>(type: "text", nullable: true),
                    tipo_carteira = table.Column<string>(type: "text", nullable: true),
                    incremento_nosso_numero = table.Column<long>(type: "bigint", nullable: false),
                    tipo_remessa = table.Column<string>(type: "text", nullable: true),
                    codigo_cliente = table.Column<string>(type: "text", nullable: true),
                    posto = table.Column<string>(type: "text", nullable: true),
                    ativa = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_contas_emissoras", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grupos_recorrencia",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    meses = table.Column<int>(type: "integer", nullable: false),
                    dia_vencimento = table.Column<int>(type: "integer", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_grupos_recorrencia", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "lancamentos_contabeis",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    periodo_contabil_id = table.Column<Guid>(type: "uuid", nullable: true),
                    numero_lancamento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    estado = table.Column<int>(type: "integer", nullable: false),
                    historico = table.Column<string>(type: "text", nullable: true),
                    lancamento_estorno_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_lancamentos_contabeis", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "periodos_contabeis",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ano_fiscal = table.Column<int>(type: "integer", nullable: false),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_fechamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    estado = table.Column<int>(type: "integer", nullable: false),
                    usuario_fechamento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    usuario_reabertura_id = table.Column<Guid>(type: "uuid", nullable: true),
                    motivo_reabertura = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("p_k_periodos_contabeis", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "remessas",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_arquivo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    data_geracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    grupo = table.Column<int>(type: "integer", nullable: false),
                    layout = table.Column<int>(type: "integer", nullable: false),
                    conta_emissora_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade_titulos = table.Column<int>(type: "integer", nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_remessas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "saldos_abertura",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<string>(type: "text", nullable: true),
                    data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    conta_contabil_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_conta = table.Column<string>(type: "text", nullable: true),
                    tipo_saldo = table.Column<int>(type: "integer", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    historico = table.Column<string>(type: "text", nullable: false),
                    contabilizado = table.Column<bool>(type: "boolean", nullable: false),
                    aprovado = table.Column<bool>(type: "boolean", nullable: false),
                    saldo_inicial = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_saldos_abertura", x => x.id);
                    table.ForeignKey(
                        name: "fk_saldo_abertura_conta",
                        column: x => x.conta_contabil_id,
                        principalSchema: "financas",
                        principalTable: "contas_contabeis",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sacados",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    grupo_recorrencia_id = table.Column<Guid>(type: "uuid", nullable: true),
                    nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    documento = table.Column<string>(type: "text", nullable: true),
                    r_g = table.Column<string>(type: "text", nullable: true),
                    inscricao = table.Column<string>(type: "text", nullable: true),
                    endereco = table.Column<string>(type: "text", nullable: true),
                    numero = table.Column<string>(type: "text", nullable: true),
                    complemento = table.Column<string>(type: "text", nullable: true),
                    bairro = table.Column<string>(type: "text", nullable: true),
                    cidade = table.Column<string>(type: "text", nullable: true),
                    u_f = table.Column<string>(type: "text", nullable: true),
                    c_e_p = table.Column<string>(type: "text", nullable: true),
                    telefone = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    bloqueado = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_sacados", x => x.id);
                    table.ForeignKey(
                        name: "fk_sacado_grupo",
                        column: x => x.grupo_recorrencia_id,
                        principalSchema: "financas",
                        principalTable: "grupos_recorrencia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "lancamento_contabil_linhas",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lancamento_contabil_id = table.Column<Guid>(type: "uuid", nullable: false),
                    conta_contabil_id = table.Column<Guid>(type: "uuid", nullable: false),
                    debito = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    credito = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    historico = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_lancamento_contabil_linhas", x => x.id);
                    table.ForeignKey(
                        name: "f_k_lancamento_contabil_linhas_lancamentos_contabeis_lancamento~",
                        column: x => x.lancamento_contabil_id,
                        principalSchema: "financas",
                        principalTable: "lancamentos_contabeis",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "remessa_boletos",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    remessa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    boleto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fatura_cobranca_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_remessa_boletos", x => x.id);
                    table.ForeignKey(
                        name: "f_k_remessa_boletos_remessas_remessa_id",
                        column: x => x.remessa_id,
                        principalSchema: "financas",
                        principalTable: "remessas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "faturas_cobranca",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sacado_id = table.Column<Guid>(type: "uuid", nullable: false),
                    grupo_recorrencia_id = table.Column<Guid>(type: "uuid", nullable: true),
                    referencia = table.Column<string>(type: "text", nullable: true),
                    numero_documento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_vencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    email = table.Column<string>(type: "text", nullable: true),
                    tipo_fatura = table.Column<int>(type: "integer", nullable: false),
                    situacao = table.Column<int>(type: "integer", nullable: false),
                    remetida = table.Column<bool>(type: "boolean", nullable: false),
                    nosso_numero = table.Column<long>(type: "bigint", nullable: true),
                    banco_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_baixa = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    valor_recebido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_faturas_cobranca", x => x.id);
                    table.ForeignKey(
                        name: "fk_fatura_sacado",
                        column: x => x.sacado_id,
                        principalSchema: "financas",
                        principalTable: "sacados",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "boletos",
                schema: "financas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fatura_cobranca_id = table.Column<Guid>(type: "uuid", nullable: false),
                    conta_emissora_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nosso_numero = table.Column<long>(type: "bigint", nullable: false),
                    numero_documento = table.Column<string>(type: "text", nullable: true),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    data_vencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    linha_digitavel = table.Column<string>(type: "text", nullable: true),
                    arquivo = table.Column<string>(type: "text", nullable: true),
                    multa = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    juros = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    instrucao1 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    instrucao2 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    instrucao3 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    instrucao4 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("p_k_boletos", x => x.id);
                    table.ForeignKey(
                        name: "fk_boleto_conta_emissora",
                        column: x => x.conta_emissora_id,
                        principalSchema: "financas",
                        principalTable: "contas_emissoras",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_boleto_fatura",
                        column: x => x.fatura_cobranca_id,
                        principalSchema: "financas",
                        principalTable: "faturas_cobranca",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_boletos_conta_emissora_id",
                schema: "financas",
                table: "boletos",
                column: "conta_emissora_id");

            migrationBuilder.CreateIndex(
                name: "i_x_boletos_fatura_cobranca_id",
                schema: "financas",
                table: "boletos",
                column: "fatura_cobranca_id");

            migrationBuilder.CreateIndex(
                name: "ix__boleto_sync_id",
                schema: "financas",
                table: "boletos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__boleto_tenant_id",
                schema: "financas",
                table: "boletos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_boleto_nosso_numero_conta",
                schema: "financas",
                table: "boletos",
                columns: new[] { "nosso_numero", "conta_emissora_id" });

            migrationBuilder.CreateIndex(
                name: "ix__cobranca_email_sync_id",
                schema: "financas",
                table: "cobrancas_email",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__cobranca_email_tenant_id",
                schema: "financas",
                table: "cobrancas_email",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_cobranca_email_tenant_status",
                schema: "financas",
                table: "cobrancas_email",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__configuracao_cedente_sync_id",
                schema: "financas",
                table: "configuracoes_cedente",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__configuracao_cedente_tenant_id",
                schema: "financas",
                table: "configuracoes_cedente",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_cedente_tenant_empresa",
                schema: "financas",
                table: "configuracoes_cedente",
                columns: new[] { "tenant_id", "empresa_id" });

            migrationBuilder.CreateIndex(
                name: "ix__conta_contabil_sync_id",
                schema: "financas",
                table: "contas_contabeis",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__conta_contabil_tenant_id",
                schema: "financas",
                table: "contas_contabeis",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_conta_contabil_pai",
                schema: "financas",
                table: "contas_contabeis",
                column: "conta_pai_id");

            migrationBuilder.CreateIndex(
                name: "ix_conta_contabil_tenant_codigo",
                schema: "financas",
                table: "contas_contabeis",
                columns: new[] { "tenant_id", "codigo_conta" });

            migrationBuilder.CreateIndex(
                name: "ix__conta_emissora_sync_id",
                schema: "financas",
                table: "contas_emissoras",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__conta_emissora_tenant_id",
                schema: "financas",
                table: "contas_emissoras",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_conta_emissora_tenant_ativa",
                schema: "financas",
                table: "contas_emissoras",
                columns: new[] { "tenant_id", "ativa" });

            migrationBuilder.CreateIndex(
                name: "ix_conta_emissora_tenant_banco",
                schema: "financas",
                table: "contas_emissoras",
                columns: new[] { "tenant_id", "banco_id" });

            migrationBuilder.CreateIndex(
                name: "ix__fatura_cobranca_sync_id",
                schema: "financas",
                table: "faturas_cobranca",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fatura_cobranca_tenant_id",
                schema: "financas",
                table: "faturas_cobranca",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_fatura_nosso_numero",
                schema: "financas",
                table: "faturas_cobranca",
                column: "nosso_numero");

            migrationBuilder.CreateIndex(
                name: "ix_fatura_sacado_venc_situacao",
                schema: "financas",
                table: "faturas_cobranca",
                columns: new[] { "sacado_id", "data_vencimento", "situacao" });

            migrationBuilder.CreateIndex(
                name: "ix__grupo_recorrencia_sync_id",
                schema: "financas",
                table: "grupos_recorrencia",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__grupo_recorrencia_tenant_id",
                schema: "financas",
                table: "grupos_recorrencia",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_grupo_recorrencia_tenant_descricao",
                schema: "financas",
                table: "grupos_recorrencia",
                columns: new[] { "tenant_id", "descricao" });

            migrationBuilder.CreateIndex(
                name: "i_x_lancamento_contabil_linhas_lancamento_contabil_id",
                schema: "financas",
                table: "lancamento_contabil_linhas",
                column: "lancamento_contabil_id");

            migrationBuilder.CreateIndex(
                name: "ix__lancamento_contabil_linha_sync_id",
                schema: "financas",
                table: "lancamento_contabil_linhas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__lancamento_contabil_linha_tenant_id",
                schema: "financas",
                table: "lancamento_contabil_linhas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_lancamento_linha_conta",
                schema: "financas",
                table: "lancamento_contabil_linhas",
                column: "conta_contabil_id");

            migrationBuilder.CreateIndex(
                name: "ix__lancamento_contabil_sync_id",
                schema: "financas",
                table: "lancamentos_contabeis",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__lancamento_contabil_tenant_id",
                schema: "financas",
                table: "lancamentos_contabeis",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_lancamento_contabil_periodo",
                schema: "financas",
                table: "lancamentos_contabeis",
                column: "periodo_contabil_id");

            migrationBuilder.CreateIndex(
                name: "ix_lancamento_contabil_tenant_numero",
                schema: "financas",
                table: "lancamentos_contabeis",
                columns: new[] { "tenant_id", "numero_lancamento" });

            migrationBuilder.CreateIndex(
                name: "ix__periodo_contabil_sync_id",
                schema: "financas",
                table: "periodos_contabeis",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__periodo_contabil_tenant_id",
                schema: "financas",
                table: "periodos_contabeis",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_periodo_contabil_tenant_ano",
                schema: "financas",
                table: "periodos_contabeis",
                columns: new[] { "tenant_id", "ano_fiscal" });

            migrationBuilder.CreateIndex(
                name: "ix__remessa_boleto_sync_id",
                schema: "financas",
                table: "remessa_boletos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__remessa_boleto_tenant_id",
                schema: "financas",
                table: "remessa_boletos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "uq_remessa_boleto",
                schema: "financas",
                table: "remessa_boletos",
                columns: new[] { "remessa_id", "boleto_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__remessa_sync_id",
                schema: "financas",
                table: "remessas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__remessa_tenant_id",
                schema: "financas",
                table: "remessas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "uq_remessa_tenant_arquivo",
                schema: "financas",
                table: "remessas",
                columns: new[] { "tenant_id", "nome_arquivo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_sacados_grupo_recorrencia_id",
                schema: "financas",
                table: "sacados",
                column: "grupo_recorrencia_id");

            migrationBuilder.CreateIndex(
                name: "ix__sacado_sync_id",
                schema: "financas",
                table: "sacados",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sacado_tenant_id",
                schema: "financas",
                table: "sacados",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_sacado_tenant_documento",
                schema: "financas",
                table: "sacados",
                columns: new[] { "tenant_id", "documento" });

            migrationBuilder.CreateIndex(
                name: "i_x_saldos_abertura_conta_contabil_id",
                schema: "financas",
                table: "saldos_abertura",
                column: "conta_contabil_id");

            migrationBuilder.CreateIndex(
                name: "ix__saldo_abertura_sync_id",
                schema: "financas",
                table: "saldos_abertura",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__saldo_abertura_tenant_id",
                schema: "financas",
                table: "saldos_abertura",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_saldo_abertura_tenant_conta",
                schema: "financas",
                table: "saldos_abertura",
                columns: new[] { "tenant_id", "conta_contabil_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "boletos",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "cobrancas_email",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "configuracoes_cedente",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "lancamento_contabil_linhas",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "periodos_contabeis",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "remessa_boletos",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "saldos_abertura",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "contas_emissoras",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "faturas_cobranca",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "lancamentos_contabeis",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "remessas",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "contas_contabeis",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "sacados",
                schema: "financas");

            migrationBuilder.DropTable(
                name: "grupos_recorrencia",
                schema: "financas");
        }
    }
}
