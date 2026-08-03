using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Vendas.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmodulosPendentesVendas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ven_contrato_anexos",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contrato_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_arquivo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    referencia_arquivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    usuario_upload_id = table.Column<Guid>(type: "uuid", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_ven_contrato_anexos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_contrato_assinaturas",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contrato_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    parte = table.Column<int>(type: "integer", nullable: false),
                    tipo_assinatura = table.Column<int>(type: "integer", nullable: false),
                    dados_assinatura = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    assinado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_ven_contrato_assinaturas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_contrato_comentarios",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contrato_id = table.Column<Guid>(type: "uuid", nullable: false),
                    comentario = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    editado = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_ven_contrato_comentarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_contrato_configuracoes",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    prefixo_contrato = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    automacao_padrao_json = table.Column<string>(type: "jsonb", nullable: true),
                    usuarios_padrao_json = table.Column<string>(type: "jsonb", nullable: true),
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
                    table.PrimaryKey("p_k_ven_contrato_configuracoes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_contrato_historicos",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contrato_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_ven_contrato_historicos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_contrato_modelos",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    titulo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    corpo = table.Column<string>(type: "text", nullable: false),
                    cor_cabecalho = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    sistema = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_ven_contrato_modelos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_contrato_notas",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contrato_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nota = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    editado = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_ven_contrato_notas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_contrato_renovacoes",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contrato_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    notas = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_ven_contrato_renovacoes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_contrato_tipos",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_ven_contrato_tipos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_contratos",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    identificador_publico = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    assunto = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    numero_contrato = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    tipo_origem = table.Column<int>(type: "integer", nullable: false),
                    numero_modelo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    descricao = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    corpo_documento = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    publicado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    enviado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    publicacao_agendada_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    empresa_assinou = table.Column<bool>(type: "boolean", nullable: false),
                    cliente_assinou = table.Column<bool>(type: "boolean", nullable: false),
                    visualizado = table.Column<bool>(type: "boolean", nullable: false),
                    projeto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    lead_id = table.Column<Guid>(type: "uuid", nullable: true),
                    proposta_id = table.Column<Guid>(type: "uuid", nullable: true),
                    pedido_id = table.Column<Guid>(type: "uuid", nullable: true),
                    categoria_id = table.Column<Guid>(type: "uuid", nullable: true),
                    automacao_habilitada = table.Column<bool>(type: "boolean", nullable: false),
                    automacao_config_json = table.Column<string>(type: "jsonb", nullable: true),
                    automacao_resultado_json = table.Column<string>(type: "jsonb", nullable: true),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequencia_exibicao = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("p_k_ven_contratos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_demanda_cenarios",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    previsao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tipo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("p_k_ven_demanda_cenarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_demanda_consensos",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    previsao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cenario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    versao_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    aprovado_por = table.Column<Guid>(type: "uuid", nullable: true),
                    aprovado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    observacoes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("p_k_ven_demanda_consensos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_demanda_historicos",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    previsao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    evento = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    detalhe = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    valores_anteriores = table.Column<string>(type: "jsonb", nullable: true),
                    valores_novos = table.Column<string>(type: "jsonb", nullable: true),
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
                    table.PrimaryKey("p_k_ven_demanda_historicos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_demanda_integracoes",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    previsao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    destino = table.Column<int>(type: "integer", nullable: false),
                    direcao = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    data_processamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    mensagem = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("p_k_ven_demanda_integracoes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_demanda_itens",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    previsao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cenario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    versao_id = table.Column<Guid>(type: "uuid", nullable: true),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    periodo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    quantidade_historica = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    quantidade_prevista = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantidade_ajustada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    quantidade_aprovada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    unidade_medida_id = table.Column<Guid>(type: "uuid", nullable: true),
                    observacoes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("p_k_ven_demanda_itens", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_demanda_previsoes",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    codigo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    periodo_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    periodo_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    cenario_vigente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    versao_vigente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    observacoes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    sequencia_exibicao = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("p_k_ven_demanda_previsoes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_demanda_versoes",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    previsao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_versao = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    data_geracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    gerado_por = table.Column<Guid>(type: "uuid", nullable: false),
                    observacoes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("p_k_ven_demanda_versoes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_eco_carrosseis",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    link_acao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    nome_botao = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    imagem = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    cor_fundo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("p_k_ven_eco_carrosseis", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_eco_clientes",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    sobrenome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    cpf = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    inscricao_estadual = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    telefone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    senha_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    token_cliente = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_ven_eco_clientes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_eco_configuracoes_loja",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    rua = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    bairro = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    cidade = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    uf = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    cep = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    telefone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    email = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    link_facebook = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    link_twitter = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    link_instagram = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    frete_gratis_valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    pagamento_public_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    pagamento_access_token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    funcionamento = table.Column<string>(type: "text", nullable: true),
                    politica_privacidade = table.Column<string>(type: "text", nullable: true),
                    mensagem_agradecimento = table.Column<string>(type: "text", nullable: true),
                    latitude = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    longitude = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    mapa_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    mapa_api_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    token_loja = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    cor_fundo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cor_botao = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    logo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    imagem_contato = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    icone = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    timer_carrossel = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("p_k_ven_eco_configuracoes_loja", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_eco_contatos",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    texto = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
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
                    table.PrimaryKey("p_k_ven_eco_contatos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_eco_cupons",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tipo = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_ven_eco_cupons", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_eco_enderecos_cliente",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rua = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    bairro = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    cep = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    cidade = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    uf = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    complemento = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
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
                    table.PrimaryKey("p_k_ven_eco_enderecos_cliente", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_eco_favoritos_produto",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_ven_eco_favoritos_produto", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_eco_frete_gratis_cidades",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    uf = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
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
                    table.PrimaryKey("p_k_ven_eco_frete_gratis_cidades", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_eco_historicos",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entidade = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    entidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    evento = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    dados_anteriores = table.Column<string>(type: "jsonb", nullable: true),
                    dados_novos = table.Column<string>(type: "jsonb", nullable: true),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_ven_eco_historicos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_eco_newsletters",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
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
                    table.PrimaryKey("p_k_ven_eco_newsletters", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_eco_pedido_itens",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pedido_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    variacao_id = table.Column<Guid>(type: "uuid", nullable: true),
                    valor_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_ven_eco_pedido_itens", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_eco_pedidos",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    endereco_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status_pagamento_codigo = table.Column<int>(type: "integer", nullable: false),
                    status_preparacao_codigo = table.Column<int>(type: "integer", nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_frete = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_desconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tipo_frete = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: true),
                    numero_nfe = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    observacao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    rand_pedido = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    hash = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    token_pedido = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cupom_desconto = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: true),
                    transacao_id = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    forma_pagamento = table.Column<int>(type: "integer", nullable: true),
                    status_pagamento = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    status_detalhe = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    link_boleto = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    qr_code = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    qr_code_base64 = table.Column<string>(type: "text", nullable: true),
                    codigo_rastreio = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    sequencia_exibicao = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("p_k_ven_eco_pedidos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_expedicao_historicos",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    expedicao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    evento = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    status_anterior = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    status_novo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    detalhe = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    trace_id = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
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
                    table.PrimaryKey("p_k_ven_expedicao_historicos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_expedicao_itens_entrega",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    expedicao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pedido_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quantidade_vendida = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantidade_entregue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    saldo_entrega = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    data_entrega = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    usuario_entrega_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_ven_expedicao_itens_entrega", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_expedicao_locais_entrega",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    expedicao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    documento_fiscal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cpf_cnpj = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    logradouro = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    complemento = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    bairro = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    codigo_municipio = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    nome_municipio = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    uf = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
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
                    table.PrimaryKey("p_k_ven_expedicao_locais_entrega", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_expedicao_reboques",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transporte_id = table.Column<Guid>(type: "uuid", nullable: false),
                    veiculo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    placa = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    rntrc = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    uf = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
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
                    table.PrimaryKey("p_k_ven_expedicao_reboques", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_expedicao_transportadoras",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transporte_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cnpj = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    cpf = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    razao_social = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    inscricao_estadual = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    endereco_completo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    nome_municipio = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    uf = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
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
                    table.PrimaryKey("p_k_ven_expedicao_transportadoras", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_expedicao_transportes",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    expedicao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    modalidade_frete = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    conhecimento_transporte = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    possui_transportadora = table.Column<bool>(type: "boolean", nullable: false),
                    possui_veiculo = table.Column<bool>(type: "boolean", nullable: false),
                    possui_volumes = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_ven_expedicao_transportes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_expedicao_veiculos",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transporte_id = table.Column<Guid>(type: "uuid", nullable: false),
                    veiculo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    placa = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    rntrc = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    uf = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
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
                    table.PrimaryKey("p_k_ven_expedicao_veiculos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_expedicao_volumes",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transporte_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade_volumes = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    especie = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    marca = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    numero_volumes = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    peso_liquido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    peso_bruto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_ven_expedicao_volumes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_expedicoes",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pedido_id = table.Column<Guid>(type: "uuid", nullable: false),
                    documento_fiscal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    romaneio_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    data_expedicao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_confirmacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    observacoes = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    trace_id = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    sequencia_exibicao = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("p_k_ven_expedicoes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_garantia_coberturas",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    garantia_politica_id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: true),
                    venda_item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    numero_serie_lote = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    data_origem = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_vencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    situacao = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_ven_garantia_coberturas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_garantia_historicos",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entidade_tipo = table.Column<int>(type: "integer", nullable: false),
                    entidade_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_ven_garantia_historicos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_garantia_politicas",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    duracao = table.Column<int>(type: "integer", nullable: false),
                    tipo_duracao = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    sequencia_exibicao = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("p_k_ven_garantia_politicas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_portal_auditorias",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_cliente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    usuario_interno_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    recurso = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    acao = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    entidade_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    detalhe = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("p_k_ven_portal_auditorias", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_portal_formulario_responsaveis",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    formulario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_interno_id = table.Column<Guid>(type: "uuid", nullable: false),
                    papel = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
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
                    table.PrimaryKey("p_k_ven_portal_formulario_responsaveis", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_portal_formularios",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    publico = table.Column<bool>(type: "boolean", nullable: false),
                    configuracao_campos = table.Column<string>(type: "jsonb", nullable: true),
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
                    table.PrimaryKey("p_k_ven_portal_formularios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_portal_permissoes",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recurso = table.Column<int>(type: "integer", nullable: false),
                    pode_visualizar = table.Column<bool>(type: "boolean", nullable: false),
                    pode_criar = table.Column<bool>(type: "boolean", nullable: false),
                    pode_baixar = table.Column<bool>(type: "boolean", nullable: false),
                    pode_administrar = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_ven_portal_permissoes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_portal_solicitacoes",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    usuario_cliente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    formulario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: true),
                    assunto = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    descricao = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    dados_formulario = table.Column<string>(type: "jsonb", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    aberta_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    respondida_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    encerrada_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_ven_portal_solicitacoes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ven_portal_usuarios_cliente",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    telefone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    administrador_cliente = table.Column<bool>(type: "boolean", nullable: false),
                    ultimo_acesso_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_ven_portal_usuarios_cliente", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix__contrato_anexo_sync_id",
                schema: "vendas",
                table: "ven_contrato_anexos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__contrato_anexo_tenant_id",
                schema: "vendas",
                table: "ven_contrato_anexos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_contrato_anexos_tenant_contrato",
                schema: "vendas",
                table: "ven_contrato_anexos",
                columns: new[] { "tenant_id", "contrato_id" });

            migrationBuilder.CreateIndex(
                name: "ix__contrato_assinatura_sync_id",
                schema: "vendas",
                table: "ven_contrato_assinaturas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__contrato_assinatura_tenant_id",
                schema: "vendas",
                table: "ven_contrato_assinaturas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "uq_ven_contrato_assinaturas_contrato_usuario",
                schema: "vendas",
                table: "ven_contrato_assinaturas",
                columns: new[] { "tenant_id", "contrato_id", "usuario_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__contrato_comentario_sync_id",
                schema: "vendas",
                table: "ven_contrato_comentarios",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__contrato_comentario_tenant_id",
                schema: "vendas",
                table: "ven_contrato_comentarios",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_contrato_comentarios_tenant_contrato",
                schema: "vendas",
                table: "ven_contrato_comentarios",
                columns: new[] { "tenant_id", "contrato_id" });

            migrationBuilder.CreateIndex(
                name: "ix__contrato_configuracao_sync_id",
                schema: "vendas",
                table: "ven_contrato_configuracoes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__contrato_configuracao_tenant_id",
                schema: "vendas",
                table: "ven_contrato_configuracoes",
                column: "tenant_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__contrato_historico_sync_id",
                schema: "vendas",
                table: "ven_contrato_historicos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__contrato_historico_tenant_id",
                schema: "vendas",
                table: "ven_contrato_historicos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_contrato_historicos_tenant_contrato",
                schema: "vendas",
                table: "ven_contrato_historicos",
                columns: new[] { "tenant_id", "contrato_id" });

            migrationBuilder.CreateIndex(
                name: "ix__contrato_modelo_sync_id",
                schema: "vendas",
                table: "ven_contrato_modelos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__contrato_modelo_tenant_id",
                schema: "vendas",
                table: "ven_contrato_modelos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__contrato_nota_sync_id",
                schema: "vendas",
                table: "ven_contrato_notas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__contrato_nota_tenant_id",
                schema: "vendas",
                table: "ven_contrato_notas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_contrato_notas_tenant_contrato",
                schema: "vendas",
                table: "ven_contrato_notas",
                columns: new[] { "tenant_id", "contrato_id" });

            migrationBuilder.CreateIndex(
                name: "ix__contrato_renovacao_sync_id",
                schema: "vendas",
                table: "ven_contrato_renovacoes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__contrato_renovacao_tenant_id",
                schema: "vendas",
                table: "ven_contrato_renovacoes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_contrato_renovacoes_tenant_contrato",
                schema: "vendas",
                table: "ven_contrato_renovacoes",
                columns: new[] { "tenant_id", "contrato_id" });

            migrationBuilder.CreateIndex(
                name: "ix__contrato_tipo_sync_id",
                schema: "vendas",
                table: "ven_contrato_tipos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__contrato_tipo_tenant_id",
                schema: "vendas",
                table: "ven_contrato_tipos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__contrato_sync_id",
                schema: "vendas",
                table: "ven_contratos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__contrato_tenant_id",
                schema: "vendas",
                table: "ven_contratos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_contratos_tenant_cliente",
                schema: "vendas",
                table: "ven_contratos",
                columns: new[] { "tenant_id", "cliente_id" });

            migrationBuilder.CreateIndex(
                name: "ix_ven_contratos_tenant_status",
                schema: "vendas",
                table: "ven_contratos",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "uq_ven_contratos_identificador_publico",
                schema: "vendas",
                table: "ven_contratos",
                column: "identificador_publico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_ven_contratos_tenant_numero",
                schema: "vendas",
                table: "ven_contratos",
                columns: new[] { "tenant_id", "numero_contrato" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__demanda_cenario_sync_id",
                schema: "vendas",
                table: "ven_demanda_cenarios",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__demanda_cenario_tenant_id",
                schema: "vendas",
                table: "ven_demanda_cenarios",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_demanda_cenarios_tenant_previsao",
                schema: "vendas",
                table: "ven_demanda_cenarios",
                columns: new[] { "tenant_id", "previsao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__demanda_consenso_sync_id",
                schema: "vendas",
                table: "ven_demanda_consensos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__demanda_consenso_tenant_id",
                schema: "vendas",
                table: "ven_demanda_consensos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_demanda_consensos_tenant_previsao",
                schema: "vendas",
                table: "ven_demanda_consensos",
                columns: new[] { "tenant_id", "previsao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__demanda_historico_sync_id",
                schema: "vendas",
                table: "ven_demanda_historicos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__demanda_historico_tenant_id",
                schema: "vendas",
                table: "ven_demanda_historicos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_demanda_historicos_tenant_previsao",
                schema: "vendas",
                table: "ven_demanda_historicos",
                columns: new[] { "tenant_id", "previsao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__demanda_integracao_sync_id",
                schema: "vendas",
                table: "ven_demanda_integracoes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__demanda_integracao_tenant_id",
                schema: "vendas",
                table: "ven_demanda_integracoes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_demanda_integracoes_tenant_previsao",
                schema: "vendas",
                table: "ven_demanda_integracoes",
                columns: new[] { "tenant_id", "previsao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__demanda_item_sync_id",
                schema: "vendas",
                table: "ven_demanda_itens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__demanda_item_tenant_id",
                schema: "vendas",
                table: "ven_demanda_itens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_demanda_itens_tenant_previsao",
                schema: "vendas",
                table: "ven_demanda_itens",
                columns: new[] { "tenant_id", "previsao_id" });

            migrationBuilder.CreateIndex(
                name: "ix_ven_demanda_itens_tenant_produto",
                schema: "vendas",
                table: "ven_demanda_itens",
                columns: new[] { "tenant_id", "produto_id" });

            migrationBuilder.CreateIndex(
                name: "ix__demanda_previsao_sync_id",
                schema: "vendas",
                table: "ven_demanda_previsoes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__demanda_previsao_tenant_id",
                schema: "vendas",
                table: "ven_demanda_previsoes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_demanda_previsoes_tenant_status",
                schema: "vendas",
                table: "ven_demanda_previsoes",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__demanda_versao_sync_id",
                schema: "vendas",
                table: "ven_demanda_versoes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__demanda_versao_tenant_id",
                schema: "vendas",
                table: "ven_demanda_versoes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_demanda_versoes_tenant_previsao",
                schema: "vendas",
                table: "ven_demanda_versoes",
                columns: new[] { "tenant_id", "previsao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__eco_carrossel_sync_id",
                schema: "vendas",
                table: "ven_eco_carrosseis",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__eco_carrossel_tenant_id",
                schema: "vendas",
                table: "ven_eco_carrosseis",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__eco_cliente_sync_id",
                schema: "vendas",
                table: "ven_eco_clientes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__eco_cliente_tenant_id",
                schema: "vendas",
                table: "ven_eco_clientes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "uq_ven_eco_clientes_tenant_email",
                schema: "vendas",
                table: "ven_eco_clientes",
                columns: new[] { "tenant_id", "email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__eco_configuracao_loja_sync_id",
                schema: "vendas",
                table: "ven_eco_configuracoes_loja",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__eco_configuracao_loja_tenant_id",
                schema: "vendas",
                table: "ven_eco_configuracoes_loja",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "uq_ven_eco_config_token_loja",
                schema: "vendas",
                table: "ven_eco_configuracoes_loja",
                column: "token_loja",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__eco_contato_sync_id",
                schema: "vendas",
                table: "ven_eco_contatos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__eco_contato_tenant_id",
                schema: "vendas",
                table: "ven_eco_contatos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__eco_cupom_sync_id",
                schema: "vendas",
                table: "ven_eco_cupons",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__eco_cupom_tenant_id",
                schema: "vendas",
                table: "ven_eco_cupons",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "uq_ven_eco_cupons_tenant_codigo",
                schema: "vendas",
                table: "ven_eco_cupons",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__eco_endereco_cliente_sync_id",
                schema: "vendas",
                table: "ven_eco_enderecos_cliente",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__eco_endereco_cliente_tenant_id",
                schema: "vendas",
                table: "ven_eco_enderecos_cliente",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_eco_enderecos_tenant_cliente",
                schema: "vendas",
                table: "ven_eco_enderecos_cliente",
                columns: new[] { "tenant_id", "cliente_id" });

            migrationBuilder.CreateIndex(
                name: "ix__eco_favorito_produto_sync_id",
                schema: "vendas",
                table: "ven_eco_favoritos_produto",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__eco_favorito_produto_tenant_id",
                schema: "vendas",
                table: "ven_eco_favoritos_produto",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_eco_favoritos_tenant_cliente",
                schema: "vendas",
                table: "ven_eco_favoritos_produto",
                columns: new[] { "tenant_id", "cliente_id" });

            migrationBuilder.CreateIndex(
                name: "ix__eco_frete_gratis_cidade_sync_id",
                schema: "vendas",
                table: "ven_eco_frete_gratis_cidades",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__eco_frete_gratis_cidade_tenant_id",
                schema: "vendas",
                table: "ven_eco_frete_gratis_cidades",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__eco_historico_sync_id",
                schema: "vendas",
                table: "ven_eco_historicos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__eco_historico_tenant_id",
                schema: "vendas",
                table: "ven_eco_historicos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_eco_historicos_tenant_entidade",
                schema: "vendas",
                table: "ven_eco_historicos",
                columns: new[] { "tenant_id", "entidade_id" });

            migrationBuilder.CreateIndex(
                name: "ix__eco_newsletter_sync_id",
                schema: "vendas",
                table: "ven_eco_newsletters",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__eco_newsletter_tenant_id",
                schema: "vendas",
                table: "ven_eco_newsletters",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_eco_newsletters_tenant_email",
                schema: "vendas",
                table: "ven_eco_newsletters",
                columns: new[] { "tenant_id", "email" });

            migrationBuilder.CreateIndex(
                name: "ix__eco_pedido_item_sync_id",
                schema: "vendas",
                table: "ven_eco_pedido_itens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__eco_pedido_item_tenant_id",
                schema: "vendas",
                table: "ven_eco_pedido_itens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_eco_pedido_itens_tenant_pedido",
                schema: "vendas",
                table: "ven_eco_pedido_itens",
                columns: new[] { "tenant_id", "pedido_id" });

            migrationBuilder.CreateIndex(
                name: "ix__eco_pedido_sync_id",
                schema: "vendas",
                table: "ven_eco_pedidos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__eco_pedido_tenant_id",
                schema: "vendas",
                table: "ven_eco_pedidos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_eco_pedidos_tenant_cliente",
                schema: "vendas",
                table: "ven_eco_pedidos",
                columns: new[] { "tenant_id", "cliente_id" });

            migrationBuilder.CreateIndex(
                name: "ix_ven_eco_pedidos_tenant_status_pag",
                schema: "vendas",
                table: "ven_eco_pedidos",
                columns: new[] { "tenant_id", "status_pagamento_codigo" });

            migrationBuilder.CreateIndex(
                name: "uq_ven_eco_pedidos_token",
                schema: "vendas",
                table: "ven_eco_pedidos",
                column: "token_pedido",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__expedicao_historico_sync_id",
                schema: "vendas",
                table: "ven_expedicao_historicos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__expedicao_historico_tenant_id",
                schema: "vendas",
                table: "ven_expedicao_historicos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_expedicao_historicos_tenant_exp",
                schema: "vendas",
                table: "ven_expedicao_historicos",
                columns: new[] { "tenant_id", "expedicao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__expedicao_item_entrega_sync_id",
                schema: "vendas",
                table: "ven_expedicao_itens_entrega",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__expedicao_item_entrega_tenant_id",
                schema: "vendas",
                table: "ven_expedicao_itens_entrega",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_expedicao_itens_tenant_exp",
                schema: "vendas",
                table: "ven_expedicao_itens_entrega",
                columns: new[] { "tenant_id", "expedicao_id" });

            migrationBuilder.CreateIndex(
                name: "ix_ven_expedicao_itens_tenant_pedido_item",
                schema: "vendas",
                table: "ven_expedicao_itens_entrega",
                columns: new[] { "tenant_id", "pedido_item_id" });

            migrationBuilder.CreateIndex(
                name: "ix__expedicao_local_entrega_sync_id",
                schema: "vendas",
                table: "ven_expedicao_locais_entrega",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__expedicao_local_entrega_tenant_id",
                schema: "vendas",
                table: "ven_expedicao_locais_entrega",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_expedicao_locais_tenant_exp",
                schema: "vendas",
                table: "ven_expedicao_locais_entrega",
                columns: new[] { "tenant_id", "expedicao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__expedicao_reboque_sync_id",
                schema: "vendas",
                table: "ven_expedicao_reboques",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__expedicao_reboque_tenant_id",
                schema: "vendas",
                table: "ven_expedicao_reboques",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_expedicao_reboques_tenant_transp",
                schema: "vendas",
                table: "ven_expedicao_reboques",
                columns: new[] { "tenant_id", "transporte_id" });

            migrationBuilder.CreateIndex(
                name: "ix__expedicao_transportadora_sync_id",
                schema: "vendas",
                table: "ven_expedicao_transportadoras",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__expedicao_transportadora_tenant_id",
                schema: "vendas",
                table: "ven_expedicao_transportadoras",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_expedicao_transportadoras_tenant_transp",
                schema: "vendas",
                table: "ven_expedicao_transportadoras",
                columns: new[] { "tenant_id", "transporte_id" });

            migrationBuilder.CreateIndex(
                name: "ix__expedicao_transporte_sync_id",
                schema: "vendas",
                table: "ven_expedicao_transportes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__expedicao_transporte_tenant_id",
                schema: "vendas",
                table: "ven_expedicao_transportes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_expedicao_transportes_tenant_exp",
                schema: "vendas",
                table: "ven_expedicao_transportes",
                columns: new[] { "tenant_id", "expedicao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__expedicao_veiculo_sync_id",
                schema: "vendas",
                table: "ven_expedicao_veiculos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__expedicao_veiculo_tenant_id",
                schema: "vendas",
                table: "ven_expedicao_veiculos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_expedicao_veiculos_tenant_transp",
                schema: "vendas",
                table: "ven_expedicao_veiculos",
                columns: new[] { "tenant_id", "transporte_id" });

            migrationBuilder.CreateIndex(
                name: "ix__expedicao_volume_sync_id",
                schema: "vendas",
                table: "ven_expedicao_volumes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__expedicao_volume_tenant_id",
                schema: "vendas",
                table: "ven_expedicao_volumes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_expedicao_volumes_tenant_transp",
                schema: "vendas",
                table: "ven_expedicao_volumes",
                columns: new[] { "tenant_id", "transporte_id" });

            migrationBuilder.CreateIndex(
                name: "ix__expedicao_sync_id",
                schema: "vendas",
                table: "ven_expedicoes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__expedicao_tenant_id",
                schema: "vendas",
                table: "ven_expedicoes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_expedicoes_tenant_pedido",
                schema: "vendas",
                table: "ven_expedicoes",
                columns: new[] { "tenant_id", "pedido_id" });

            migrationBuilder.CreateIndex(
                name: "ix_ven_expedicoes_tenant_status",
                schema: "vendas",
                table: "ven_expedicoes",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__garantia_cobertura_sync_id",
                schema: "vendas",
                table: "ven_garantia_coberturas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__garantia_cobertura_tenant_id",
                schema: "vendas",
                table: "ven_garantia_coberturas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_garantia_coberturas_tenant_politica",
                schema: "vendas",
                table: "ven_garantia_coberturas",
                columns: new[] { "tenant_id", "garantia_politica_id" });

            migrationBuilder.CreateIndex(
                name: "ix_ven_garantia_coberturas_tenant_venda",
                schema: "vendas",
                table: "ven_garantia_coberturas",
                columns: new[] { "tenant_id", "venda_id" });

            migrationBuilder.CreateIndex(
                name: "ix__garantia_historico_sync_id",
                schema: "vendas",
                table: "ven_garantia_historicos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__garantia_historico_tenant_id",
                schema: "vendas",
                table: "ven_garantia_historicos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_garantia_historicos_tenant_entidade",
                schema: "vendas",
                table: "ven_garantia_historicos",
                columns: new[] { "tenant_id", "entidade_id" });

            migrationBuilder.CreateIndex(
                name: "ix__garantia_politica_sync_id",
                schema: "vendas",
                table: "ven_garantia_politicas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__garantia_politica_tenant_id",
                schema: "vendas",
                table: "ven_garantia_politicas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__portal_auditoria_sync_id",
                schema: "vendas",
                table: "ven_portal_auditorias",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__portal_auditoria_tenant_id",
                schema: "vendas",
                table: "ven_portal_auditorias",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_portal_auditorias_tenant_usuario",
                schema: "vendas",
                table: "ven_portal_auditorias",
                columns: new[] { "tenant_id", "usuario_cliente_id" });

            migrationBuilder.CreateIndex(
                name: "ix__portal_formulario_responsavel_sync_id",
                schema: "vendas",
                table: "ven_portal_formulario_responsaveis",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__portal_formulario_responsavel_tenant_id",
                schema: "vendas",
                table: "ven_portal_formulario_responsaveis",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_portal_form_resp_tenant_form",
                schema: "vendas",
                table: "ven_portal_formulario_responsaveis",
                columns: new[] { "tenant_id", "formulario_id" });

            migrationBuilder.CreateIndex(
                name: "ix__portal_formulario_sync_id",
                schema: "vendas",
                table: "ven_portal_formularios",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__portal_formulario_tenant_id",
                schema: "vendas",
                table: "ven_portal_formularios",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__portal_permissao_sync_id",
                schema: "vendas",
                table: "ven_portal_permissoes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__portal_permissao_tenant_id",
                schema: "vendas",
                table: "ven_portal_permissoes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "uq_ven_portal_permissoes_usuario_recurso",
                schema: "vendas",
                table: "ven_portal_permissoes",
                columns: new[] { "tenant_id", "usuario_cliente_id", "recurso" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__portal_solicitacao_sync_id",
                schema: "vendas",
                table: "ven_portal_solicitacoes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__portal_solicitacao_tenant_id",
                schema: "vendas",
                table: "ven_portal_solicitacoes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_portal_solicitacoes_tenant_cliente",
                schema: "vendas",
                table: "ven_portal_solicitacoes",
                columns: new[] { "tenant_id", "cliente_id" });

            migrationBuilder.CreateIndex(
                name: "ix_ven_portal_solicitacoes_tenant_status",
                schema: "vendas",
                table: "ven_portal_solicitacoes",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__portal_usuario_cliente_sync_id",
                schema: "vendas",
                table: "ven_portal_usuarios_cliente",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__portal_usuario_cliente_tenant_id",
                schema: "vendas",
                table: "ven_portal_usuarios_cliente",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ven_portal_usuarios_tenant_cliente",
                schema: "vendas",
                table: "ven_portal_usuarios_cliente",
                columns: new[] { "tenant_id", "cliente_id" });

            migrationBuilder.CreateIndex(
                name: "uq_ven_portal_usuarios_tenant_email",
                schema: "vendas",
                table: "ven_portal_usuarios_cliente",
                columns: new[] { "tenant_id", "email" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ven_contrato_anexos",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_contrato_assinaturas",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_contrato_comentarios",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_contrato_configuracoes",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_contrato_historicos",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_contrato_modelos",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_contrato_notas",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_contrato_renovacoes",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_contrato_tipos",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_contratos",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_demanda_cenarios",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_demanda_consensos",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_demanda_historicos",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_demanda_integracoes",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_demanda_itens",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_demanda_previsoes",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_demanda_versoes",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_eco_carrosseis",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_eco_clientes",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_eco_configuracoes_loja",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_eco_contatos",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_eco_cupons",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_eco_enderecos_cliente",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_eco_favoritos_produto",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_eco_frete_gratis_cidades",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_eco_historicos",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_eco_newsletters",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_eco_pedido_itens",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_eco_pedidos",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_expedicao_historicos",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_expedicao_itens_entrega",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_expedicao_locais_entrega",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_expedicao_reboques",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_expedicao_transportadoras",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_expedicao_transportes",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_expedicao_veiculos",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_expedicao_volumes",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_expedicoes",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_garantia_coberturas",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_garantia_historicos",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_garantia_politicas",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_portal_auditorias",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_portal_formulario_responsaveis",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_portal_formularios",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_portal_permissoes",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_portal_solicitacoes",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "ven_portal_usuarios_cliente",
                schema: "vendas");
        }
    }
}
