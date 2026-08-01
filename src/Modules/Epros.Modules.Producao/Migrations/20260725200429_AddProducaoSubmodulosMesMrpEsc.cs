using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Producao.Migrations
{
    /// <inheritdoc />
    public partial class AddProducaoSubmodulosMesMrpEsc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "prd_esc_anexo",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    programacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    arquivo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    usuario_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    data_anexo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_prd_esc_anexo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_esc_historico",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    programacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    acao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    usuario_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    payload_json = table.Column<string>(type: "text", nullable: false),
                    status_anterior = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    status_novo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ip_origem = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
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
                    table.PrimaryKey("p_k_prd_esc_historico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_esc_parametro",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    chave = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    valor = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
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
                    table.PrimaryKey("p_k_prd_esc_parametro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_esc_programacao",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plano_producao_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ordem_producao_id = table.Column<Guid>(type: "uuid", nullable: true),
                    centro_trabalho_id = table.Column<Guid>(type: "uuid", nullable: true),
                    prioridade = table.Column<int>(type: "integer", nullable: true),
                    motivo_rejeicao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("p_k_prd_esc_programacao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_mes_anexo",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    arquivo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    usuario_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    anexado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_prd_mes_anexo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_mes_consumo_material",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    estrutura_id = table.Column<Guid>(type: "uuid", nullable: false),
                    componente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    variacao_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quantidade_prevista = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantidade_consumida = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    percentual_desperdicio = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    unidade_id = table.Column<Guid>(type: "uuid", nullable: true),
                    custo_consumo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("p_k_prd_mes_consumo_material", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_mes_historico",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    acao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status_anterior = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    status_novo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    usuario_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    origem_ip = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    conteudo_json = table.Column<string>(type: "text", nullable: false),
                    registrado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_prd_mes_historico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_mes_movimento_producao",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    movimento_pai_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_movimento = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    variacao_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    local_estoque_id = table.Column<Guid>(type: "uuid", nullable: false),
                    confirmado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_prd_mes_movimento_producao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_mes_ordem",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    referencia = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    previsao_entrega = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    termino = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_transacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    local_estoque_id = table.Column<Guid>(type: "uuid", nullable: true),
                    estrutura_id = table.Column<Guid>(type: "uuid", nullable: true),
                    produto_acabado_id = table.Column<Guid>(type: "uuid", nullable: true),
                    variacao_produto_acabado_id = table.Column<Guid>(type: "uuid", nullable: true),
                    custo_total_previsto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    custo_total_realizado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    percentual_venda = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    percentual_estoque = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_total_final = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    desperdicio_unidades = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    custo_producao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    tipo_custo_producao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    finalizada = table.Column<bool>(type: "boolean", nullable: false),
                    lote = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    validade = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    motivo_rejeicao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("p_k_prd_mes_ordem", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_mes_parametro",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    prefixo_referencia = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    bloquear_edicao_quantidade_insumo = table.Column<bool>(type: "boolean", nullable: false),
                    atualizar_preco_produto_final = table.Column<bool>(type: "boolean", nullable: false),
                    exigir_estrutura_ativa = table.Column<bool>(type: "boolean", nullable: false),
                    versao_parametro = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("p_k_prd_mes_parametro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_mes_servico",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_ordem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    inicio_previsto = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    termino_previsto = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    horas_previsto = table.Column<int>(type: "integer", nullable: false),
                    minutos_previsto = table.Column<int>(type: "integer", nullable: false),
                    segundos_previsto = table.Column<int>(type: "integer", nullable: false),
                    custo_previsto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    inicio_realizado = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    termino_realizado = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    horas_realizado = table.Column<int>(type: "integer", nullable: false),
                    minutos_realizado = table.Column<int>(type: "integer", nullable: false),
                    segundos_realizado = table.Column<int>(type: "integer", nullable: false),
                    custo_realizado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("p_k_prd_mes_servico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_mes_servico_equipamento",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    servico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipamento_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_prd_mes_servico_equipamento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_mrp_planejamento",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    motivo_rejeicao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("p_k_prd_mrp_planejamento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_mrp_planejamento_anexo",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    planejamento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    arquivo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    usuario_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    anexado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_prd_mrp_planejamento_anexo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_mrp_planejamento_historico",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    planejamento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    acao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    usuario_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    payload_json = table.Column<string>(type: "text", nullable: false),
                    status_anterior = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    status_novo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ip = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_prd_mrp_planejamento_historico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_esc_operacao",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    programacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    servico_id = table.Column<Guid>(type: "uuid", nullable: true),
                    equipamento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sequencia = table.Column<int>(type: "integer", nullable: false),
                    inicio_previsto = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    termino_previsto = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    inicio_realizado = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    termino_realizado = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    horas_previstas = table.Column<int>(type: "integer", nullable: false),
                    minutos_previstos = table.Column<int>(type: "integer", nullable: false),
                    segundos_previstos = table.Column<int>(type: "integer", nullable: false),
                    horas_realizadas = table.Column<int>(type: "integer", nullable: false),
                    minutos_realizados = table.Column<int>(type: "integer", nullable: false),
                    segundos_realizados = table.Column<int>(type: "integer", nullable: false),
                    custo_previsto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    custo_realizado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_prd_esc_operacao", x => x.id);
                    table.ForeignKey(
                        name: "f_k_prd_esc_operacao_prd_esc_programacao_programacao_id",
                        column: x => x.programacao_id,
                        principalSchema: "producao",
                        principalTable: "prd_esc_programacao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prd_mes_ordem_item",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    variacao_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quantidade_produzir = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantidade_produzida = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantidade_entregue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    custo_previsto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    custo_realizado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("p_k_prd_mes_ordem_item", x => x.id);
                    table.ForeignKey(
                        name: "f_k_prd_mes_ordem_item_prd_mes_ordem_ordem_id",
                        column: x => x.ordem_id,
                        principalSchema: "producao",
                        principalTable: "prd_mes_ordem",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_prd_esc_anexo_tenant_id_programacao_id",
                schema: "producao",
                table: "prd_esc_anexo",
                columns: new[] { "tenant_id", "programacao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__esc_anexo_sync_id",
                schema: "producao",
                table: "prd_esc_anexo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__esc_anexo_tenant_id",
                schema: "producao",
                table: "prd_esc_anexo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_esc_historico_tenant_id_programacao_id",
                schema: "producao",
                table: "prd_esc_historico",
                columns: new[] { "tenant_id", "programacao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__esc_historico_sync_id",
                schema: "producao",
                table: "prd_esc_historico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__esc_historico_tenant_id",
                schema: "producao",
                table: "prd_esc_historico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_esc_operacao_programacao_id",
                schema: "producao",
                table: "prd_esc_operacao",
                column: "programacao_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_esc_operacao_tenant_id_programacao_id",
                schema: "producao",
                table: "prd_esc_operacao",
                columns: new[] { "tenant_id", "programacao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__esc_operacao_sync_id",
                schema: "producao",
                table: "prd_esc_operacao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__esc_operacao_tenant_id",
                schema: "producao",
                table: "prd_esc_operacao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_esc_parametro_tenant_id_chave",
                schema: "producao",
                table: "prd_esc_parametro",
                columns: new[] { "tenant_id", "chave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__esc_parametro_sync_id",
                schema: "producao",
                table: "prd_esc_parametro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__esc_parametro_tenant_id",
                schema: "producao",
                table: "prd_esc_parametro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_esc_programacao_tenant_id_centro_trabalho_id",
                schema: "producao",
                table: "prd_esc_programacao",
                columns: new[] { "tenant_id", "centro_trabalho_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_prd_esc_programacao_tenant_id_codigo",
                schema: "producao",
                table: "prd_esc_programacao",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_prd_esc_programacao_tenant_id_status",
                schema: "producao",
                table: "prd_esc_programacao",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__esc_programacao_sync_id",
                schema: "producao",
                table: "prd_esc_programacao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__esc_programacao_tenant_id",
                schema: "producao",
                table: "prd_esc_programacao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_mes_anexo_tenant_id_ordem_id",
                schema: "producao",
                table: "prd_mes_anexo",
                columns: new[] { "tenant_id", "ordem_id" });

            migrationBuilder.CreateIndex(
                name: "ix__mes_anexo_sync_id",
                schema: "producao",
                table: "prd_mes_anexo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__mes_anexo_tenant_id",
                schema: "producao",
                table: "prd_mes_anexo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_mes_consumo_material_tenant_id_estrutura_id",
                schema: "producao",
                table: "prd_mes_consumo_material",
                columns: new[] { "tenant_id", "estrutura_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_prd_mes_consumo_material_tenant_id_ordem_id",
                schema: "producao",
                table: "prd_mes_consumo_material",
                columns: new[] { "tenant_id", "ordem_id" });

            migrationBuilder.CreateIndex(
                name: "ix__mes_consumo_material_sync_id",
                schema: "producao",
                table: "prd_mes_consumo_material",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__mes_consumo_material_tenant_id",
                schema: "producao",
                table: "prd_mes_consumo_material",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_mes_historico_tenant_id_ordem_id",
                schema: "producao",
                table: "prd_mes_historico",
                columns: new[] { "tenant_id", "ordem_id" });

            migrationBuilder.CreateIndex(
                name: "ix__mes_historico_sync_id",
                schema: "producao",
                table: "prd_mes_historico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__mes_historico_tenant_id",
                schema: "producao",
                table: "prd_mes_historico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_mes_movimento_producao_tenant_id_movimento_pai_id",
                schema: "producao",
                table: "prd_mes_movimento_producao",
                columns: new[] { "tenant_id", "movimento_pai_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_prd_mes_movimento_producao_tenant_id_ordem_id",
                schema: "producao",
                table: "prd_mes_movimento_producao",
                columns: new[] { "tenant_id", "ordem_id" });

            migrationBuilder.CreateIndex(
                name: "ix__mes_movimento_producao_sync_id",
                schema: "producao",
                table: "prd_mes_movimento_producao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__mes_movimento_producao_tenant_id",
                schema: "producao",
                table: "prd_mes_movimento_producao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_mes_ordem_tenant_id_empresa_id",
                schema: "producao",
                table: "prd_mes_ordem",
                columns: new[] { "tenant_id", "empresa_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_prd_mes_ordem_tenant_id_referencia",
                schema: "producao",
                table: "prd_mes_ordem",
                columns: new[] { "tenant_id", "referencia" });

            migrationBuilder.CreateIndex(
                name: "i_x_prd_mes_ordem_tenant_id_status",
                schema: "producao",
                table: "prd_mes_ordem",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__mes_ordem_sync_id",
                schema: "producao",
                table: "prd_mes_ordem",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__mes_ordem_tenant_id",
                schema: "producao",
                table: "prd_mes_ordem",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_mes_ordem_item_ordem_id",
                schema: "producao",
                table: "prd_mes_ordem_item",
                column: "ordem_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_mes_ordem_item_tenant_id_ordem_id",
                schema: "producao",
                table: "prd_mes_ordem_item",
                columns: new[] { "tenant_id", "ordem_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_prd_mes_ordem_item_tenant_id_produto_id",
                schema: "producao",
                table: "prd_mes_ordem_item",
                columns: new[] { "tenant_id", "produto_id" });

            migrationBuilder.CreateIndex(
                name: "ix__mes_ordem_item_sync_id",
                schema: "producao",
                table: "prd_mes_ordem_item",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__mes_ordem_item_tenant_id",
                schema: "producao",
                table: "prd_mes_ordem_item",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__mes_parametro_sync_id",
                schema: "producao",
                table: "prd_mes_parametro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__mes_parametro_tenant_id",
                schema: "producao",
                table: "prd_mes_parametro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_mes_servico_tenant_id_item_ordem_id",
                schema: "producao",
                table: "prd_mes_servico",
                columns: new[] { "tenant_id", "item_ordem_id" });

            migrationBuilder.CreateIndex(
                name: "ix__mes_servico_sync_id",
                schema: "producao",
                table: "prd_mes_servico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__mes_servico_tenant_id",
                schema: "producao",
                table: "prd_mes_servico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_mes_servico_equipamento_tenant_id_equipamento_id",
                schema: "producao",
                table: "prd_mes_servico_equipamento",
                columns: new[] { "tenant_id", "equipamento_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_prd_mes_servico_equipamento_tenant_id_servico_id",
                schema: "producao",
                table: "prd_mes_servico_equipamento",
                columns: new[] { "tenant_id", "servico_id" });

            migrationBuilder.CreateIndex(
                name: "ix__mes_servico_equipamento_sync_id",
                schema: "producao",
                table: "prd_mes_servico_equipamento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__mes_servico_equipamento_tenant_id",
                schema: "producao",
                table: "prd_mes_servico_equipamento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_mrp_planejamento_tenant_id_codigo",
                schema: "producao",
                table: "prd_mrp_planejamento",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_prd_mrp_planejamento_tenant_id_status",
                schema: "producao",
                table: "prd_mrp_planejamento",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__mrp_planejamento_sync_id",
                schema: "producao",
                table: "prd_mrp_planejamento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__mrp_planejamento_tenant_id",
                schema: "producao",
                table: "prd_mrp_planejamento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_mrp_planejamento_anexo_tenant_id_planejamento_id",
                schema: "producao",
                table: "prd_mrp_planejamento_anexo",
                columns: new[] { "tenant_id", "planejamento_id" });

            migrationBuilder.CreateIndex(
                name: "ix__mrp_planejamento_anexo_sync_id",
                schema: "producao",
                table: "prd_mrp_planejamento_anexo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__mrp_planejamento_anexo_tenant_id",
                schema: "producao",
                table: "prd_mrp_planejamento_anexo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_mrp_planejamento_historico_tenant_id_planejamento_id",
                schema: "producao",
                table: "prd_mrp_planejamento_historico",
                columns: new[] { "tenant_id", "planejamento_id" });

            migrationBuilder.CreateIndex(
                name: "ix__mrp_planejamento_historico_sync_id",
                schema: "producao",
                table: "prd_mrp_planejamento_historico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__mrp_planejamento_historico_tenant_id",
                schema: "producao",
                table: "prd_mrp_planejamento_historico",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "prd_esc_anexo",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_esc_historico",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_esc_operacao",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_esc_parametro",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_mes_anexo",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_mes_consumo_material",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_mes_historico",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_mes_movimento_producao",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_mes_ordem_item",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_mes_parametro",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_mes_servico",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_mes_servico_equipamento",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_mrp_planejamento",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_mrp_planejamento_anexo",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_mrp_planejamento_historico",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_esc_programacao",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_mes_ordem",
                schema: "producao");
        }
    }
}
