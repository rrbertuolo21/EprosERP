using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Manutencao.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmodulosPreventivaTrabalhoPecasParadasInducao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "descricao",
                schema: "manutencao",
                table: "equipamentos",
                type: "character varying(1000)", maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "tipo_equipamento_id",
                schema: "manutencao",
                table: "equipamentos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "marca_id",
                schema: "manutencao",
                table: "equipamentos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "numero_serie",
                schema: "manutencao",
                table: "equipamentos",
                type: "character varying(120)", maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "funcao_operacional",
                schema: "manutencao",
                table: "equipamentos",
                type: "character varying(200)", maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "estado_conservacao_id",
                schema: "manutencao",
                table: "equipamentos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "responsavel_id",
                schema: "manutencao",
                table: "equipamentos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "local_id",
                schema: "manutencao",
                table: "equipamentos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "versao",
                schema: "manutencao",
                table: "equipamentos",
                type: "integer",
                nullable: false, defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "man_prv_plano",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    alvo_tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    alvo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    versao = table.Column<int>(type: "integer", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: true),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_man_prv_plano", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "man_prv_periodicidade",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    plano_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_periodicidade = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    intervalo = table.Column<int>(type: "integer", nullable: true),
                    unidade_intervalo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    contador_tipo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    contador_base = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    contador_proximo = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    tolerancia = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    proxima_execucao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    situacao = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: true),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_man_prv_periodicidade", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "man_prv_checklist_item",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    plano_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequencia = table.Column<int>(type: "integer", nullable: false),
                    descricao_tarefa = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    obrigatorio = table.Column<bool>(type: "boolean", nullable: false),
                    tipo_resposta = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    exige_evidencia = table.Column<bool>(type: "boolean", nullable: false),
                    criterio_aceite = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: true),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_man_prv_checklist_item", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "man_prv_kit_peca",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    plano_id = table.Column<Guid>(type: "uuid", nullable: false),
                    peca_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    unidade = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    obrigatoria = table.Column<bool>(type: "boolean", nullable: false),
                    observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: true),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_man_prv_kit_peca", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "man_prv_execucao_programada",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    plano_id = table.Column<Guid>(type: "uuid", nullable: false),
                    periodicidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_prevista = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    contador_previsto = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    prioridade = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    data_geracao_ordem = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_conclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ordem_trabalho_id = table.Column<Guid>(type: "uuid", nullable: true),
                    motivo_cancelamento = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: true),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_man_prv_execucao_programada", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "man_trb_ordem_servico",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    perfil_ordem = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_pessoa = table.Column<int>(type: "integer", nullable: false),
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_abertura = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_orcamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_aprovacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_montagem = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_pronta = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_entrega = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    hora_inicio = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    data_previsao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    hora_previsao = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    hora_fim = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    nome_contato = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    fone_contato = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    observacao_cliente = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    observacao_abertura = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    reclamacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    observacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    garantia = table.Column<bool>(type: "boolean", nullable: false),
                    tipo_atendimento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_equipamento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    marca_id = table.Column<Guid>(type: "uuid", nullable: true),
                    info_equip1 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    info_equip2 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    info_equip3 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    obs_equip1 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    obs_equip2 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status_codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    usuario_comissao_orcamento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    usuario_comissao_montagem_id = table.Column<Guid>(type: "uuid", nullable: true),
                    faturado = table.Column<bool>(type: "boolean", nullable: false),
                    documento_fiscal_emitido = table.Column<bool>(type: "boolean", nullable: false),
                    documento_fiscal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cancelado = table.Column<bool>(type: "boolean", nullable: false),
                    documento_pessoa = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    pagamento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    parcelamento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    u_f_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_n_f = table.Column<int>(type: "integer", nullable: true),
                    versao = table.Column<int>(type: "integer", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: true),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_man_trb_ordem_servico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "man_trb_status_os",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: true),
                    campo_data_associado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: true),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_man_trb_status_os", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "man_trb_os_equipamento",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_servico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipamento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_serie = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    tipo_cobertura = table.Column<int>(type: "integer", nullable: true),
                    observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: true),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_man_trb_os_equipamento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "man_trb_os_evolucao",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_servico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    hora_registro = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    observacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    enviar_email = table.Column<bool>(type: "boolean", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: true),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_man_trb_os_evolucao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "man_trb_os_item",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_servico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    complemento = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    quantidade_entregue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    valor_unitario = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    valor_subtotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    taxa_desconto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    valor_desconto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    valor_custo = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    valor_venda = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    tipo_saida = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    grade_id = table.Column<Guid>(type: "uuid", nullable: true),
                    informacao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: true),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_man_trb_os_item", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "man_trb_os_financeiro",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_servico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_vinculo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    referencia_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status_vinculo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    data_vinculo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    erro = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: true),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_man_trb_os_financeiro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "man_pec_registro",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_servico_id = table.Column<Guid>(type: "uuid", nullable: true),
                    plano_preventivo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    versao = table.Column<int>(type: "integer", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: true),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_man_pec_registro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "man_pec_item",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    registro_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_servico_id = table.Column<Guid>(type: "uuid", nullable: true),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    grade_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sequencia = table.Column<int>(type: "integer", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    quantidade_entregue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    tabela_preco_id = table.Column<Guid>(type: "uuid", nullable: true),
                    valor_produto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    valor_venda = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    valor_custo = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    valor_unitario = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    valor_subtotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    taxa_desconto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    valor_desconto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    tipo_saida = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    informacao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status_item = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: true),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_man_pec_item", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "man_pec_reserva",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    grade_id = table.Column<Guid>(type: "uuid", nullable: true),
                    local_estoque_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quantidade_reservada = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    status_reserva = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    data_reserva = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: true),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_man_pec_reserva", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "man_pec_movimento",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    grade_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_movimento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    movimento_estoque_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status_movimento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    informacao_movimento = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    data_movimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    erro = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: true),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_man_pec_movimento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "man_pec_politica_reposicao",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    grade_id = table.Column<Guid>(type: "uuid", nullable: true),
                    local_estoque_id = table.Column<Guid>(type: "uuid", nullable: true),
                    estoque_minimo = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    estoque_maximo = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ponto_pedido = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    lead_time_dias = table.Column<int>(type: "integer", nullable: true),
                    criticidade = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: true),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_man_pec_politica_reposicao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "man_pec_kit_preventivo",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    plano_preventivo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    grade_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quantidade_prevista = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    sequencia = table.Column<int>(type: "integer", nullable: true),
                    observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: true),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_man_pec_kit_preventivo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "man_par_parada",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipamento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    linha_id = table.Column<Guid>(type: "uuid", nullable: true),
                    celula_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_hora_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_hora_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    duracao_minutos = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    tipo_parada = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    motivo_parada_id = table.Column<Guid>(type: "uuid", nullable: true),
                    os_gerada_id = table.Column<Guid>(type: "uuid", nullable: true),
                    observacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    usuario_registro_id = table.Column<Guid>(type: "uuid", nullable: false),
                    versao = table.Column<int>(type: "integer", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: true),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_man_par_parada", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "man_par_item",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    parada_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequencia = table.Column<int>(type: "integer", nullable: false),
                    tipo_impacto = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    unidade = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: true),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_man_par_item", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "man_par_motivo",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    motivo_pai_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_parada_aplicavel = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    exige_observacao = table.Column<bool>(type: "boolean", nullable: false),
                    exige_anexo = table.Column<bool>(type: "boolean", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: true),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_man_par_motivo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "man_par_vinculo_os",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    parada_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_servico_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_vinculo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    regra_acionadora = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status_vinculo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    data_hora_acionamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    mensagem_erro = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: true),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_man_par_vinculo_os", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "man_par_indicador",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    parada_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_indicador = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    periodo_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    periodo_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    unidade = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    formula_aplicada = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    origem_dados = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    data_calculo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: true),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_man_par_indicador", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "man_ind_vinculo_patrimonial",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipamento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ativo_patrimonial_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_patrimonial = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    nome_ativo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    descricao_ativo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    data_aquisicao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_aceite = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_cadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_vistoria = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_baixa = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    vencimento_garantia = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    numero_nota_fiscal = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    chave_nfe = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    valor_original = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    valor_compra = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    valor_atualizado = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    valor_baixa = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    deprecia = table.Column<bool>(type: "boolean", nullable: true),
                    metodo_depreciacao = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    inicio_depreciacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ultima_depreciacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tipo_depreciacao = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    taxa_anual = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    taxa_mensal = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    taxa_acelerada = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    taxa_incentivada = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    funcao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: true),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_man_ind_vinculo_patrimonial", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "man_ind_inducao",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipamento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_aceite = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status_inducao = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: true),
                    checklist_json = table.Column<string>(type: "text", nullable: true),
                    observacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: true),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_man_ind_inducao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "man_ind_servico_equipamento",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    servico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipamento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ativo_patrimonial_id = table.Column<Guid>(type: "uuid", nullable: true),
                    vigencia_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    vigencia_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: true),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_man_ind_servico_equipamento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "man_ind_atributo",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipamento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    chave = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    valor = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    origem_funcional = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: true),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_man_ind_atributo", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix__plano_preventivo_sync_id",
                schema: "manutencao",
                table: "man_prv_plano",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__plano_preventivo_tenant_id",
                schema: "manutencao",
                table: "man_prv_plano",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_prv_plano_tenant_id_codigo",
                schema: "manutencao",
                table: "man_prv_plano",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_man_prv_plano_tenant_id_status",
                schema: "manutencao",
                table: "man_prv_plano",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__plano_preventivo_periodicidade_sync_id",
                schema: "manutencao",
                table: "man_prv_periodicidade",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__plano_preventivo_periodicidade_tenant_id",
                schema: "manutencao",
                table: "man_prv_periodicidade",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_prv_periodicidade_tenant_id_plano_id",
                schema: "manutencao",
                table: "man_prv_periodicidade",
                columns: new[] { "tenant_id", "plano_id" });

            migrationBuilder.CreateIndex(
                name: "ix__plano_preventivo_checklist_item_sync_id",
                schema: "manutencao",
                table: "man_prv_checklist_item",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__plano_preventivo_checklist_item_tenant_id",
                schema: "manutencao",
                table: "man_prv_checklist_item",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_prv_checklist_item_plano_id_sequencia",
                schema: "manutencao",
                table: "man_prv_checklist_item",
                columns: new[] { "plano_id", "sequencia" });

            migrationBuilder.CreateIndex(
                name: "ix__plano_preventivo_kit_peca_sync_id",
                schema: "manutencao",
                table: "man_prv_kit_peca",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__plano_preventivo_kit_peca_tenant_id",
                schema: "manutencao",
                table: "man_prv_kit_peca",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_prv_kit_peca_tenant_id_plano_id",
                schema: "manutencao",
                table: "man_prv_kit_peca",
                columns: new[] { "tenant_id", "plano_id" });

            migrationBuilder.CreateIndex(
                name: "ix__plano_preventivo_execucao_sync_id",
                schema: "manutencao",
                table: "man_prv_execucao_programada",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__plano_preventivo_execucao_tenant_id",
                schema: "manutencao",
                table: "man_prv_execucao_programada",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_prv_execucao_programada_plano_id_status_data_prevista",
                schema: "manutencao",
                table: "man_prv_execucao_programada",
                columns: new[] { "plano_id", "status", "data_prevista" });

            migrationBuilder.CreateIndex(
                name: "ix__ordem_servico_sync_id",
                schema: "manutencao",
                table: "man_trb_ordem_servico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ordem_servico_tenant_id",
                schema: "manutencao",
                table: "man_trb_ordem_servico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_trb_ordem_servico_tenant_id_numero",
                schema: "manutencao",
                table: "man_trb_ordem_servico",
                columns: new[] { "tenant_id", "numero" });

            migrationBuilder.CreateIndex(
                name: "i_x_man_trb_ordem_servico_tenant_id_status_codigo",
                schema: "manutencao",
                table: "man_trb_ordem_servico",
                columns: new[] { "tenant_id", "status_codigo" });

            migrationBuilder.CreateIndex(
                name: "ix__ordem_servico_status_sync_id",
                schema: "manutencao",
                table: "man_trb_status_os",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ordem_servico_status_tenant_id",
                schema: "manutencao",
                table: "man_trb_status_os",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_trb_status_os_tenant_id_codigo",
                schema: "manutencao",
                table: "man_trb_status_os",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ordem_servico_equipamento_sync_id",
                schema: "manutencao",
                table: "man_trb_os_equipamento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ordem_servico_equipamento_tenant_id",
                schema: "manutencao",
                table: "man_trb_os_equipamento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_trb_os_equipamento_ordem_servico_id",
                schema: "manutencao",
                table: "man_trb_os_equipamento",
                column: "ordem_servico_id");

            migrationBuilder.CreateIndex(
                name: "ix__ordem_servico_evolucao_sync_id",
                schema: "manutencao",
                table: "man_trb_os_evolucao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ordem_servico_evolucao_tenant_id",
                schema: "manutencao",
                table: "man_trb_os_evolucao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_trb_os_evolucao_ordem_servico_id",
                schema: "manutencao",
                table: "man_trb_os_evolucao",
                column: "ordem_servico_id");

            migrationBuilder.CreateIndex(
                name: "ix__ordem_servico_item_sync_id",
                schema: "manutencao",
                table: "man_trb_os_item",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ordem_servico_item_tenant_id",
                schema: "manutencao",
                table: "man_trb_os_item",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_trb_os_item_ordem_servico_id",
                schema: "manutencao",
                table: "man_trb_os_item",
                column: "ordem_servico_id");

            migrationBuilder.CreateIndex(
                name: "ix__ordem_servico_financeiro_fiscal_sync_id",
                schema: "manutencao",
                table: "man_trb_os_financeiro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ordem_servico_financeiro_fiscal_tenant_id",
                schema: "manutencao",
                table: "man_trb_os_financeiro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_trb_os_financeiro_ordem_servico_id",
                schema: "manutencao",
                table: "man_trb_os_financeiro",
                column: "ordem_servico_id");

            migrationBuilder.CreateIndex(
                name: "ix__registro_peca_reposicao_sync_id",
                schema: "manutencao",
                table: "man_pec_registro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__registro_peca_reposicao_tenant_id",
                schema: "manutencao",
                table: "man_pec_registro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_pec_registro_tenant_id_codigo",
                schema: "manutencao",
                table: "man_pec_registro",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__item_peca_reposicao_sync_id",
                schema: "manutencao",
                table: "man_pec_item",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__item_peca_reposicao_tenant_id",
                schema: "manutencao",
                table: "man_pec_item",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_pec_item_registro_id_sequencia",
                schema: "manutencao",
                table: "man_pec_item",
                columns: new[] { "registro_id", "sequencia" });

            migrationBuilder.CreateIndex(
                name: "ix__reserva_peca_sync_id",
                schema: "manutencao",
                table: "man_pec_reserva",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__reserva_peca_tenant_id",
                schema: "manutencao",
                table: "man_pec_reserva",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_pec_reserva_item_id",
                schema: "manutencao",
                table: "man_pec_reserva",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "ix__movimento_peca_sync_id",
                schema: "manutencao",
                table: "man_pec_movimento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__movimento_peca_tenant_id",
                schema: "manutencao",
                table: "man_pec_movimento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_pec_movimento_item_id",
                schema: "manutencao",
                table: "man_pec_movimento",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "ix__politica_reposicao_sync_id",
                schema: "manutencao",
                table: "man_pec_politica_reposicao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__politica_reposicao_tenant_id",
                schema: "manutencao",
                table: "man_pec_politica_reposicao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_pec_politica_reposicao_tenant_id_produto_id",
                schema: "manutencao",
                table: "man_pec_politica_reposicao",
                columns: new[] { "tenant_id", "produto_id" });

            migrationBuilder.CreateIndex(
                name: "ix__kit_preventivo_sync_id",
                schema: "manutencao",
                table: "man_pec_kit_preventivo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__kit_preventivo_tenant_id",
                schema: "manutencao",
                table: "man_pec_kit_preventivo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_pec_kit_preventivo_tenant_id_plano_preventivo_id",
                schema: "manutencao",
                table: "man_pec_kit_preventivo",
                columns: new[] { "tenant_id", "plano_preventivo_id" });

            migrationBuilder.CreateIndex(
                name: "ix__parada_sync_id",
                schema: "manutencao",
                table: "man_par_parada",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__parada_tenant_id",
                schema: "manutencao",
                table: "man_par_parada",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_par_parada_tenant_id_codigo",
                schema: "manutencao",
                table: "man_par_parada",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_man_par_parada_tenant_id_status",
                schema: "manutencao",
                table: "man_par_parada",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__parada_item_sync_id",
                schema: "manutencao",
                table: "man_par_item",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__parada_item_tenant_id",
                schema: "manutencao",
                table: "man_par_item",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_par_item_parada_id_sequencia",
                schema: "manutencao",
                table: "man_par_item",
                columns: new[] { "parada_id", "sequencia" });

            migrationBuilder.CreateIndex(
                name: "ix__motivo_parada_sync_id",
                schema: "manutencao",
                table: "man_par_motivo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__motivo_parada_tenant_id",
                schema: "manutencao",
                table: "man_par_motivo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_par_motivo_tenant_id_codigo",
                schema: "manutencao",
                table: "man_par_motivo",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__parada_vinculo_os_sync_id",
                schema: "manutencao",
                table: "man_par_vinculo_os",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__parada_vinculo_os_tenant_id",
                schema: "manutencao",
                table: "man_par_vinculo_os",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_par_vinculo_os_parada_id",
                schema: "manutencao",
                table: "man_par_vinculo_os",
                column: "parada_id");

            migrationBuilder.CreateIndex(
                name: "ix__parada_indicador_sync_id",
                schema: "manutencao",
                table: "man_par_indicador",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__parada_indicador_tenant_id",
                schema: "manutencao",
                table: "man_par_indicador",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_par_indicador_parada_id",
                schema: "manutencao",
                table: "man_par_indicador",
                column: "parada_id");

            migrationBuilder.CreateIndex(
                name: "ix__equipamento_vinculo_patrimonial_sync_id",
                schema: "manutencao",
                table: "man_ind_vinculo_patrimonial",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__equipamento_vinculo_patrimonial_tenant_id",
                schema: "manutencao",
                table: "man_ind_vinculo_patrimonial",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_ind_vinculo_patrimonial_equipamento_id",
                schema: "manutencao",
                table: "man_ind_vinculo_patrimonial",
                column: "equipamento_id");

            migrationBuilder.CreateIndex(
                name: "ix__equipamento_inducao_sync_id",
                schema: "manutencao",
                table: "man_ind_inducao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__equipamento_inducao_tenant_id",
                schema: "manutencao",
                table: "man_ind_inducao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_ind_inducao_equipamento_id",
                schema: "manutencao",
                table: "man_ind_inducao",
                column: "equipamento_id");

            migrationBuilder.CreateIndex(
                name: "ix__equipamento_servico_sync_id",
                schema: "manutencao",
                table: "man_ind_servico_equipamento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__equipamento_servico_tenant_id",
                schema: "manutencao",
                table: "man_ind_servico_equipamento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_ind_servico_equipamento_tenant_id_equipamento_id",
                schema: "manutencao",
                table: "man_ind_servico_equipamento",
                columns: new[] { "tenant_id", "equipamento_id" });

            migrationBuilder.CreateIndex(
                name: "ix__equipamento_atributo_sync_id",
                schema: "manutencao",
                table: "man_ind_atributo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__equipamento_atributo_tenant_id",
                schema: "manutencao",
                table: "man_ind_atributo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_ind_atributo_equipamento_id_chave",
                schema: "manutencao",
                table: "man_ind_atributo",
                columns: new[] { "equipamento_id", "chave" });

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "man_prv_plano",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_prv_periodicidade",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_prv_checklist_item",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_prv_kit_peca",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_prv_execucao_programada",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_trb_ordem_servico",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_trb_status_os",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_trb_os_equipamento",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_trb_os_evolucao",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_trb_os_item",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_trb_os_financeiro",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_pec_registro",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_pec_item",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_pec_reserva",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_pec_movimento",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_pec_politica_reposicao",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_pec_kit_preventivo",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_par_parada",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_par_item",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_par_motivo",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_par_vinculo_os",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_par_indicador",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_ind_vinculo_patrimonial",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_ind_inducao",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_ind_servico_equipamento",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_ind_atributo",
                schema: "manutencao");

            migrationBuilder.DropColumn(
                name: "descricao",
                schema: "manutencao",
                table: "equipamentos");

            migrationBuilder.DropColumn(
                name: "tipo_equipamento_id",
                schema: "manutencao",
                table: "equipamentos");

            migrationBuilder.DropColumn(
                name: "marca_id",
                schema: "manutencao",
                table: "equipamentos");

            migrationBuilder.DropColumn(
                name: "numero_serie",
                schema: "manutencao",
                table: "equipamentos");

            migrationBuilder.DropColumn(
                name: "funcao_operacional",
                schema: "manutencao",
                table: "equipamentos");

            migrationBuilder.DropColumn(
                name: "estado_conservacao_id",
                schema: "manutencao",
                table: "equipamentos");

            migrationBuilder.DropColumn(
                name: "responsavel_id",
                schema: "manutencao",
                table: "equipamentos");

            migrationBuilder.DropColumn(
                name: "local_id",
                schema: "manutencao",
                table: "equipamentos");

            migrationBuilder.DropColumn(
                name: "versao",
                schema: "manutencao",
                table: "equipamentos");

        }
    }
}
