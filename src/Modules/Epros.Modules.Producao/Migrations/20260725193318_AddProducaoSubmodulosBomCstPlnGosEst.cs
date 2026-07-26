using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Producao.Migrations
{
    /// <inheritdoc />
    public partial class AddProducaoSubmodulosBomCstPlnGosEst : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "prd_bom_anexo",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    estrutura_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_prd_bom_anexo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_bom_estrutura",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    variacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ingredientes_json = table.Column<string>(type: "text", nullable: true),
                    instrucoes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    percentual_desperdicio = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    custo_ingredientes = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    custo_extra = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tipo_custo_producao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    quantidade_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    preco_final = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    sub_unidade_id = table.Column<Guid>(type: "uuid", nullable: true),
                    versao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    inicio_vigencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fim_vigencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_prd_bom_estrutura", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_bom_grupo_componente",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("p_k_prd_bom_grupo_componente", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_bom_historico",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    estrutura_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_prd_bom_historico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_bom_instrucao",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
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
                    table.PrimaryKey("p_k_prd_bom_instrucao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_bom_instrucao_ordem",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    instrucao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_producao_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_prd_bom_instrucao_ordem", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_cst_anexo",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    custo_producao_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_prd_cst_anexo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_cst_custo_producao",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    referencia_origem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    referencia_id = table.Column<Guid>(type: "uuid", nullable: true),
                    custo_total_previsto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    custo_total_realizado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    desvio_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_prd_cst_custo_producao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_cst_historico",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    custo_producao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    acao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    usuario_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    payload_json = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_prd_cst_historico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_cst_parametro",
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
                    table.PrimaryKey("p_k_prd_cst_parametro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_est_anexo",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    estimativa_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_prd_est_anexo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_est_estimativa",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    proposta_referencia_id = table.Column<Guid>(type: "uuid", nullable: true),
                    estrutura_rascunho_id = table.Column<Guid>(type: "uuid", nullable: true),
                    custo_previsto_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    planejamento_origem_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_prd_est_estimativa", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_est_historico",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    estimativa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    acao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    usuario_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    payload_json = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_prd_est_historico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_est_parametro",
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
                    table.PrimaryKey("p_k_prd_est_parametro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_gos_ficha_anexo",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ficha_producao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    arquivo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    usuario_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    data_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_prd_gos_ficha_anexo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_gos_ficha_historico",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ficha_producao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    acao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    usuario_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    data_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ip = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    conteudo = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    motivo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("p_k_prd_gos_ficha_historico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_gos_ficha_producao",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_venda_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    situacao = table.Column<int>(type: "integer", nullable: false),
                    entrada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    saida = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    logomarca = table.Column<int>(type: "integer", nullable: false),
                    laterais_porta = table.Column<int>(type: "integer", nullable: false),
                    apoio_cabeca = table.Column<int>(type: "integer", nullable: false),
                    transportadora = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ano_modelo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    cor_couro = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    costura = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    tipo_acento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    tipo_encosto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    abd = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    abt = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    observacao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("p_k_prd_gos_ficha_producao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_pln_planejamento",
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
                    table.PrimaryKey("p_k_prd_pln_planejamento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_pln_planejamento_anexo",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    planejamento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    arquivo_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_prd_pln_planejamento_anexo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_pln_planejamento_historico",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    planejamento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    acao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    usuario_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    payload_json = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_prd_pln_planejamento_historico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prd_bom_componente",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    estrutura_id = table.Column<Guid>(type: "uuid", nullable: false),
                    variacao_componente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    sub_unidade_id = table.Column<Guid>(type: "uuid", nullable: true),
                    multiplicador_unidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    percentual_desperdicio = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    quantidade_final = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    grupo_componente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ordem_montagem = table.Column<int>(type: "integer", nullable: true),
                    custo_unitario_com_impostos = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    custo_linha = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_prd_bom_componente", x => x.id);
                    table.ForeignKey(
                        name: "f_k_prd_bom_componente_prd_bom_estrutura_estrutura_id",
                        column: x => x.estrutura_id,
                        principalSchema: "producao",
                        principalTable: "prd_bom_estrutura",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prd_cst_custo_referencia",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    custo_producao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_referencia = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    referencia_id = table.Column<Guid>(type: "uuid", nullable: true),
                    custo_previsto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    custo_realizado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    custo_extra = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    tipo_custo_producao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    percentual_custo_producao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    desvio = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_prd_cst_custo_referencia", x => x.id);
                    table.ForeignKey(
                        name: "f_k_prd_cst_custo_referencia_prd_cst_custo_producao_custo_produ~",
                        column: x => x.custo_producao_id,
                        principalSchema: "producao",
                        principalTable: "prd_cst_custo_producao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prd_est_componente",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    estimativa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_componente = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    referencia_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    tempo_estimado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    taxa_estimada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    custo_previsto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_prd_est_componente", x => x.id);
                    table.ForeignKey(
                        name: "f_k_prd_est_componente_prd_est_estimativa_estimativa_id",
                        column: x => x.estimativa_id,
                        principalSchema: "producao",
                        principalTable: "prd_est_estimativa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prd_pln_snapshot_op",
                schema: "producao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    planejamento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_producao_id = table.Column<Guid>(type: "uuid", nullable: true),
                    inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    previsao_entrega = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    termino = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    porcento_venda = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    porcento_estoque = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    custo_total_previsto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_prd_pln_snapshot_op", x => x.id);
                    table.ForeignKey(
                        name: "f_k_prd_pln_snapshot_op_prd_pln_planejamento_planejamento_id",
                        column: x => x.planejamento_id,
                        principalSchema: "producao",
                        principalTable: "prd_pln_planejamento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_prd_bom_anexo_tenant_id_estrutura_id",
                schema: "producao",
                table: "prd_bom_anexo",
                columns: new[] { "tenant_id", "estrutura_id" });

            migrationBuilder.CreateIndex(
                name: "ix__bom_anexo_sync_id",
                schema: "producao",
                table: "prd_bom_anexo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__bom_anexo_tenant_id",
                schema: "producao",
                table: "prd_bom_anexo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_bom_componente_estrutura_id",
                schema: "producao",
                table: "prd_bom_componente",
                column: "estrutura_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_bom_componente_tenant_id_estrutura_id",
                schema: "producao",
                table: "prd_bom_componente",
                columns: new[] { "tenant_id", "estrutura_id" });

            migrationBuilder.CreateIndex(
                name: "ix__bom_componente_sync_id",
                schema: "producao",
                table: "prd_bom_componente",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__bom_componente_tenant_id",
                schema: "producao",
                table: "prd_bom_componente",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_bom_estrutura_tenant_id_codigo",
                schema: "producao",
                table: "prd_bom_estrutura",
                columns: new[] { "tenant_id", "codigo" });

            migrationBuilder.CreateIndex(
                name: "i_x_prd_bom_estrutura_tenant_id_produto_id_variacao_id",
                schema: "producao",
                table: "prd_bom_estrutura",
                columns: new[] { "tenant_id", "produto_id", "variacao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__bom_estrutura_sync_id",
                schema: "producao",
                table: "prd_bom_estrutura",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__bom_estrutura_tenant_id",
                schema: "producao",
                table: "prd_bom_estrutura",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__bom_grupo_componente_sync_id",
                schema: "producao",
                table: "prd_bom_grupo_componente",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__bom_grupo_componente_tenant_id",
                schema: "producao",
                table: "prd_bom_grupo_componente",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_bom_historico_tenant_id_estrutura_id",
                schema: "producao",
                table: "prd_bom_historico",
                columns: new[] { "tenant_id", "estrutura_id" });

            migrationBuilder.CreateIndex(
                name: "ix__bom_historico_sync_id",
                schema: "producao",
                table: "prd_bom_historico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__bom_historico_tenant_id",
                schema: "producao",
                table: "prd_bom_historico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_bom_instrucao_tenant_id_codigo",
                schema: "producao",
                table: "prd_bom_instrucao",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__bom_instrucao_sync_id",
                schema: "producao",
                table: "prd_bom_instrucao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__bom_instrucao_tenant_id",
                schema: "producao",
                table: "prd_bom_instrucao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_bom_instrucao_ordem_tenant_id_instrucao_id",
                schema: "producao",
                table: "prd_bom_instrucao_ordem",
                columns: new[] { "tenant_id", "instrucao_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_prd_bom_instrucao_ordem_tenant_id_ordem_producao_id",
                schema: "producao",
                table: "prd_bom_instrucao_ordem",
                columns: new[] { "tenant_id", "ordem_producao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__bom_instrucao_ordem_sync_id",
                schema: "producao",
                table: "prd_bom_instrucao_ordem",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__bom_instrucao_ordem_tenant_id",
                schema: "producao",
                table: "prd_bom_instrucao_ordem",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_cst_anexo_tenant_id_custo_producao_id",
                schema: "producao",
                table: "prd_cst_anexo",
                columns: new[] { "tenant_id", "custo_producao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__custo_anexo_sync_id",
                schema: "producao",
                table: "prd_cst_anexo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__custo_anexo_tenant_id",
                schema: "producao",
                table: "prd_cst_anexo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_cst_custo_producao_tenant_id_codigo",
                schema: "producao",
                table: "prd_cst_custo_producao",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__custo_producao_sync_id",
                schema: "producao",
                table: "prd_cst_custo_producao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__custo_producao_tenant_id",
                schema: "producao",
                table: "prd_cst_custo_producao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_cst_custo_referencia_custo_producao_id",
                schema: "producao",
                table: "prd_cst_custo_referencia",
                column: "custo_producao_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_cst_custo_referencia_tenant_id_custo_producao_id",
                schema: "producao",
                table: "prd_cst_custo_referencia",
                columns: new[] { "tenant_id", "custo_producao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__custo_referencia_sync_id",
                schema: "producao",
                table: "prd_cst_custo_referencia",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__custo_referencia_tenant_id",
                schema: "producao",
                table: "prd_cst_custo_referencia",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_cst_historico_tenant_id_custo_producao_id",
                schema: "producao",
                table: "prd_cst_historico",
                columns: new[] { "tenant_id", "custo_producao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__custo_historico_sync_id",
                schema: "producao",
                table: "prd_cst_historico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__custo_historico_tenant_id",
                schema: "producao",
                table: "prd_cst_historico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_cst_parametro_tenant_id_chave",
                schema: "producao",
                table: "prd_cst_parametro",
                columns: new[] { "tenant_id", "chave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__custo_parametro_sync_id",
                schema: "producao",
                table: "prd_cst_parametro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__custo_parametro_tenant_id",
                schema: "producao",
                table: "prd_cst_parametro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_est_anexo_tenant_id_estimativa_id",
                schema: "producao",
                table: "prd_est_anexo",
                columns: new[] { "tenant_id", "estimativa_id" });

            migrationBuilder.CreateIndex(
                name: "ix__estimativa_anexo_sync_id",
                schema: "producao",
                table: "prd_est_anexo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__estimativa_anexo_tenant_id",
                schema: "producao",
                table: "prd_est_anexo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_est_componente_estimativa_id",
                schema: "producao",
                table: "prd_est_componente",
                column: "estimativa_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_est_componente_tenant_id_estimativa_id",
                schema: "producao",
                table: "prd_est_componente",
                columns: new[] { "tenant_id", "estimativa_id" });

            migrationBuilder.CreateIndex(
                name: "ix__estimativa_componente_sync_id",
                schema: "producao",
                table: "prd_est_componente",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__estimativa_componente_tenant_id",
                schema: "producao",
                table: "prd_est_componente",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_est_estimativa_tenant_id_codigo",
                schema: "producao",
                table: "prd_est_estimativa",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__estimativa_sync_id",
                schema: "producao",
                table: "prd_est_estimativa",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__estimativa_tenant_id",
                schema: "producao",
                table: "prd_est_estimativa",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_est_historico_tenant_id_estimativa_id",
                schema: "producao",
                table: "prd_est_historico",
                columns: new[] { "tenant_id", "estimativa_id" });

            migrationBuilder.CreateIndex(
                name: "ix__estimativa_historico_sync_id",
                schema: "producao",
                table: "prd_est_historico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__estimativa_historico_tenant_id",
                schema: "producao",
                table: "prd_est_historico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_est_parametro_tenant_id_chave",
                schema: "producao",
                table: "prd_est_parametro",
                columns: new[] { "tenant_id", "chave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__estimativa_parametro_sync_id",
                schema: "producao",
                table: "prd_est_parametro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__estimativa_parametro_tenant_id",
                schema: "producao",
                table: "prd_est_parametro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_gos_ficha_anexo_tenant_id_ficha_producao_id",
                schema: "producao",
                table: "prd_gos_ficha_anexo",
                columns: new[] { "tenant_id", "ficha_producao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__ficha_producao_anexo_sync_id",
                schema: "producao",
                table: "prd_gos_ficha_anexo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ficha_producao_anexo_tenant_id",
                schema: "producao",
                table: "prd_gos_ficha_anexo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_gos_ficha_historico_tenant_id_ficha_producao_id",
                schema: "producao",
                table: "prd_gos_ficha_historico",
                columns: new[] { "tenant_id", "ficha_producao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__ficha_producao_historico_sync_id",
                schema: "producao",
                table: "prd_gos_ficha_historico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ficha_producao_historico_tenant_id",
                schema: "producao",
                table: "prd_gos_ficha_historico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_gos_ficha_producao_tenant_id_pessoa_id",
                schema: "producao",
                table: "prd_gos_ficha_producao",
                columns: new[] { "tenant_id", "pessoa_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_prd_gos_ficha_producao_tenant_id_situacao",
                schema: "producao",
                table: "prd_gos_ficha_producao",
                columns: new[] { "tenant_id", "situacao" });

            migrationBuilder.CreateIndex(
                name: "i_x_prd_gos_ficha_producao_tenant_id_venda_id",
                schema: "producao",
                table: "prd_gos_ficha_producao",
                columns: new[] { "tenant_id", "venda_id" });

            migrationBuilder.CreateIndex(
                name: "ix__ficha_producao_sync_id",
                schema: "producao",
                table: "prd_gos_ficha_producao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ficha_producao_tenant_id",
                schema: "producao",
                table: "prd_gos_ficha_producao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_pln_planejamento_tenant_id_codigo",
                schema: "producao",
                table: "prd_pln_planejamento",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__planejamento_producao_sync_id",
                schema: "producao",
                table: "prd_pln_planejamento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__planejamento_producao_tenant_id",
                schema: "producao",
                table: "prd_pln_planejamento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_pln_planejamento_anexo_tenant_id_planejamento_id",
                schema: "producao",
                table: "prd_pln_planejamento_anexo",
                columns: new[] { "tenant_id", "planejamento_id" });

            migrationBuilder.CreateIndex(
                name: "ix__planejamento_anexo_sync_id",
                schema: "producao",
                table: "prd_pln_planejamento_anexo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__planejamento_anexo_tenant_id",
                schema: "producao",
                table: "prd_pln_planejamento_anexo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_pln_planejamento_historico_tenant_id_planejamento_id",
                schema: "producao",
                table: "prd_pln_planejamento_historico",
                columns: new[] { "tenant_id", "planejamento_id" });

            migrationBuilder.CreateIndex(
                name: "ix__planejamento_historico_sync_id",
                schema: "producao",
                table: "prd_pln_planejamento_historico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__planejamento_historico_tenant_id",
                schema: "producao",
                table: "prd_pln_planejamento_historico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_pln_snapshot_op_planejamento_id",
                schema: "producao",
                table: "prd_pln_snapshot_op",
                column: "planejamento_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prd_pln_snapshot_op_tenant_id_planejamento_id",
                schema: "producao",
                table: "prd_pln_snapshot_op",
                columns: new[] { "tenant_id", "planejamento_id" });

            migrationBuilder.CreateIndex(
                name: "ix__planejamento_snapshot_op_sync_id",
                schema: "producao",
                table: "prd_pln_snapshot_op",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__planejamento_snapshot_op_tenant_id",
                schema: "producao",
                table: "prd_pln_snapshot_op",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "prd_bom_anexo",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_bom_componente",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_bom_grupo_componente",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_bom_historico",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_bom_instrucao",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_bom_instrucao_ordem",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_cst_anexo",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_cst_custo_referencia",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_cst_historico",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_cst_parametro",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_est_anexo",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_est_componente",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_est_historico",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_est_parametro",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_gos_ficha_anexo",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_gos_ficha_historico",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_gos_ficha_producao",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_pln_planejamento_anexo",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_pln_planejamento_historico",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_pln_snapshot_op",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_bom_estrutura",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_cst_custo_producao",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_est_estimativa",
                schema: "producao");

            migrationBuilder.DropTable(
                name: "prd_pln_planejamento",
                schema: "producao");
        }
    }
}
