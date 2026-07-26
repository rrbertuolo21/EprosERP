using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Vendas.Migrations
{
    /// <inheritdoc />
    public partial class AddCrmAndGestaoServicosVendas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "crm_atividades",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entidade_tipo = table.Column<int>(type: "integer", nullable: false),
                    lead_id = table.Column<Guid>(type: "uuid", nullable: true),
                    oportunidade_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: true),
                    campanha_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_atividade = table.Column<int>(type: "integer", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    assunto = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    data = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    hora = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    prioridade = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: true),
                    tipo_chamada = table.Column<int>(type: "integer", nullable: true),
                    duracao = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    destinatario = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    resultado = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    comentario = table.Column<string>(type: "text", nullable: true),
                    arquivo_nome = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    arquivo_referencia = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    recorrencia_json = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_crm_atividades", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "crm_campanha_listas",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    campanha_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lista_publico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_uso = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
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
                    table.PrimaryKey("p_k_crm_campanha_listas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "crm_campanha_logs",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    campanha_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rastreador_id = table.Column<Guid>(type: "uuid", nullable: true),
                    alvo_tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    alvo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atividade_tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    marketing_id = table.Column<Guid>(type: "uuid", nullable: true),
                    email = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    ocorrencias = table.Column<int>(type: "integer", nullable: false),
                    endereco_ip = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
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
                    table.PrimaryKey("p_k_crm_campanha_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "crm_campanha_rastreadores",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    campanha_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    chave_rastreamento = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    opt_out = table.Column<bool>(type: "boolean", nullable: false),
                    url_mensagem = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("p_k_crm_campanha_rastreadores", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "crm_campanhas",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tipo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    status = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    frequencia = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    moeda_id = table.Column<Guid>(type: "uuid", nullable: true),
                    orcamento = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    custo_esperado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    custo_real = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    receita_esperada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    impressoes = table.Column<int>(type: "integer", nullable: true),
                    objetivo = table.Column<string>(type: "text", nullable: true),
                    conteudo = table.Column<string>(type: "text", nullable: true),
                    responsavel_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_crm_campanhas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "crm_clientes_fidelizados",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    celular = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    total_pontos = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    data_aniversario = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ultimo_ponto = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_cadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_crm_clientes_fidelizados", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "crm_configuracoes_pix_relacional",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    link_api_app_vendas = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    token_pix = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    cnpj_empresa = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
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
                    table.PrimaryKey("p_k_crm_configuracoes_pix_relacional", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "crm_etapas",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pipeline_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_etapa = table.Column<int>(type: "integer", nullable: false),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_crm_etapas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "crm_etiquetas",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pipeline_id = table.Column<Guid>(type: "uuid", nullable: true),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    cor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("p_k_crm_etiquetas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "crm_historicos",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entidade_tipo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    entidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    evento = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dados_anteriores_json = table.Column<string>(type: "text", nullable: true),
                    dados_novos_json = table.Column<string>(type: "text", nullable: true),
                    observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("p_k_crm_historicos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "crm_lead_usuarios",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lead_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_crm_lead_usuarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "crm_leads",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pipeline_id = table.Column<Guid>(type: "uuid", nullable: true),
                    etapa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    primeiro_nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    sobrenome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    email = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    telefone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    assunto = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    valor_estimado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    origem_id = table.Column<Guid>(type: "uuid", nullable: true),
                    fontes_json = table.Column<string>(type: "text", nullable: true),
                    produtos_json = table.Column<string>(type: "text", nullable: true),
                    etiquetas_json = table.Column<string>(type: "text", nullable: true),
                    notas = table.Column<string>(type: "text", nullable: true),
                    posicao_kanban = table.Column<int>(type: "integer", nullable: true),
                    convertido = table.Column<bool>(type: "boolean", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    estado_arquivo = table.Column<int>(type: "integer", nullable: false),
                    visibilidade = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    contato_id = table.Column<Guid>(type: "uuid", nullable: true),
                    oportunidade_id = table.Column<Guid>(type: "uuid", nullable: true),
                    campanha_id = table.Column<Guid>(type: "uuid", nullable: true),
                    endereco_ip_captacao = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    imagem_capa = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    campos_customizados_json = table.Column<string>(type: "text", nullable: true),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    responsavel_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_crm_leads", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "crm_lista_publico_membros",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lista_publico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_membro = table.Column<int>(type: "integer", nullable: false),
                    lead_id = table.Column<Guid>(type: "uuid", nullable: true),
                    contato_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_crm_lista_publico_membros", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "crm_listas_publico",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tipo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    dominio_exclusao = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    contagem_membros = table.Column<int>(type: "integer", nullable: false),
                    responsavel_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_crm_listas_publico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "crm_oportunidade_participantes",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    oportunidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_participante = table.Column<int>(type: "integer", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    permissoes_json = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_crm_oportunidade_participantes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "crm_oportunidades",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pipeline_id = table.Column<Guid>(type: "uuid", nullable: false),
                    etapa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    moeda_id = table.Column<Guid>(type: "uuid", nullable: true),
                    valor_convertido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    probabilidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    data_fechamento_prevista = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    origem_id = table.Column<Guid>(type: "uuid", nullable: true),
                    fontes_json = table.Column<string>(type: "text", nullable: true),
                    produtos_json = table.Column<string>(type: "text", nullable: true),
                    etiquetas_json = table.Column<string>(type: "text", nullable: true),
                    telefone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    notas = table.Column<string>(type: "text", nullable: true),
                    posicao_kanban = table.Column<int>(type: "integer", nullable: true),
                    lead_origem_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cliente_principal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    motivo_perda = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("p_k_crm_oportunidades", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "crm_origens",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
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
                    table.PrimaryKey("p_k_crm_origens", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "crm_pagamentos_pix_relacional",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entidade_origem_tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    entidade_origem_id = table.Column<Guid>(type: "uuid", nullable: true),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    provedor_pagamento_id = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    qr_code = table.Column<string>(type: "text", nullable: true),
                    qr_code_imagem = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    data_aprovacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_crm_pagamentos_pix_relacional", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "crm_pipelines",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    criador_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_crm_pipelines", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "crm_pontuacoes_fidelizacao",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_fidelizado_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    pontos = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    observacao = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("p_k_crm_pontuacoes_fidelizacao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "crm_ticket_respostas",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                    texto = table.Column<string>(type: "text", nullable: false),
                    tipo = table.Column<int>(type: "integer", nullable: false),
                    origem = table.Column<int>(type: "integer", nullable: true),
                    remetente_tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    remetente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    anexos_json = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_crm_ticket_respostas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "crm_ticket_status",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    titulo = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    cor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    uso_cliente_respondeu = table.Column<bool>(type: "boolean", nullable: true),
                    uso_equipe_respondeu = table.Column<bool>(type: "boolean", nullable: true),
                    padrao_sistema = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_crm_ticket_status", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "crm_tickets",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    titulo = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    status_id = table.Column<Guid>(type: "uuid", nullable: false),
                    prioridade = table.Column<int>(type: "integer", nullable: true),
                    categoria_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    projeto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    origem = table.Column<int>(type: "integer", nullable: true),
                    estado_arquivo = table.Column<int>(type: "integer", nullable: false),
                    tipo_usuario_abertura = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    usuario_abertura_id = table.Column<Guid>(type: "uuid", nullable: true),
                    contato_abertura_id = table.Column<Guid>(type: "uuid", nullable: true),
                    campos_customizados_json = table.Column<string>(type: "text", nullable: true),
                    ultima_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_mudanca_status = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_resolucao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_crm_tickets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "crm_webform_responsaveis",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    webform_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_crm_webform_responsaveis", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "crm_webforms",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    identificador_unico = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    estrutura_json = table.Column<string>(type: "text", nullable: false),
                    css_customizado = table.Column<string>(type: "text", nullable: true),
                    mensagem_agradecimento = table.Column<string>(type: "text", nullable: true),
                    texto_botao = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    campanha_id = table.Column<Guid>(type: "uuid", nullable: true),
                    lead_titulo_padrao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    lead_status_padrao = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    lead_origem_id = table.Column<Guid>(type: "uuid", nullable: true),
                    usar_captcha = table.Column<bool>(type: "boolean", nullable: false),
                    notificar_administrador = table.Column<bool>(type: "boolean", nullable: false),
                    contador_submissoes = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_crm_webforms", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "servico_catalogos",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    descricao = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    taxa = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    permite_campos_customizados = table.Column<bool>(type: "boolean", nullable: false),
                    grupo_preco_localidade_json = table.Column<string>(type: "text", nullable: true),
                    taxa_embalagem_entrega = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    tipo_taxa_embalagem_entrega = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
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
                    table.PrimaryKey("p_k_servico_catalogos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "servico_faturas",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    funcionario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    conta_pagamento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    numero_fatura = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    data_fatura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    desconto_cabecalho = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_desconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_imposto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_imposto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    custo_envio_entrega = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_geral = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_liquido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_pago = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    saldo_devido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    troco = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    detalhes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
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
                    table.PrimaryKey("p_k_servico_faturas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "servico_historicos",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entidade = table.Column<int>(type: "integer", nullable: false),
                    entidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    usuario_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    evento = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    status_anterior = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    status_novo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    valores_anteriores = table.Column<string>(type: "text", nullable: true),
                    valores_novos = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_servico_historicos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "servico_lancamento_financeiro_refs",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fatura_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_fatura = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    tipo_lancamento = table.Column<int>(type: "integer", nullable: false),
                    conta_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status_integracao = table.Column<int>(type: "integer", nullable: false),
                    mensagem_integracao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("p_k_servico_lancamento_financeiro_refs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "servico_fatura_linhas",
                schema: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fatura_id = table.Column<Guid>(type: "uuid", nullable: false),
                    servico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_servico = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    preco_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    desconto_percentual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_servico_fatura_linhas", x => x.id);
                    table.ForeignKey(
                        name: "f_k_servico_fatura_linhas_servico_faturas_fatura_id",
                        column: x => x.fatura_id,
                        principalSchema: "vendas",
                        principalTable: "servico_faturas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix__crm_atividade_sync_id",
                schema: "vendas",
                table: "crm_atividades",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__crm_atividade_tenant_id",
                schema: "vendas",
                table: "crm_atividades",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_crm_atividades_tenant_lead",
                schema: "vendas",
                table: "crm_atividades",
                columns: new[] { "tenant_id", "lead_id" });

            migrationBuilder.CreateIndex(
                name: "ix_crm_atividades_tenant_oport",
                schema: "vendas",
                table: "crm_atividades",
                columns: new[] { "tenant_id", "oportunidade_id" });

            migrationBuilder.CreateIndex(
                name: "ix__crm_campanha_lista_sync_id",
                schema: "vendas",
                table: "crm_campanha_listas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__crm_campanha_lista_tenant_id",
                schema: "vendas",
                table: "crm_campanha_listas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_crm_campanha_listas_tenant_campanha",
                schema: "vendas",
                table: "crm_campanha_listas",
                columns: new[] { "tenant_id", "campanha_id" });

            migrationBuilder.CreateIndex(
                name: "ix__crm_campanha_log_sync_id",
                schema: "vendas",
                table: "crm_campanha_logs",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__crm_campanha_log_tenant_id",
                schema: "vendas",
                table: "crm_campanha_logs",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_crm_campanha_logs_tenant_campanha",
                schema: "vendas",
                table: "crm_campanha_logs",
                columns: new[] { "tenant_id", "campanha_id" });

            migrationBuilder.CreateIndex(
                name: "ix__crm_campanha_rastreador_sync_id",
                schema: "vendas",
                table: "crm_campanha_rastreadores",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__crm_campanha_rastreador_tenant_id",
                schema: "vendas",
                table: "crm_campanha_rastreadores",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_crm_campanha_rastreadores_tenant_campanha",
                schema: "vendas",
                table: "crm_campanha_rastreadores",
                columns: new[] { "tenant_id", "campanha_id" });

            migrationBuilder.CreateIndex(
                name: "ix__crm_campanha_sync_id",
                schema: "vendas",
                table: "crm_campanhas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__crm_campanha_tenant_id",
                schema: "vendas",
                table: "crm_campanhas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__crm_cliente_fidelizado_sync_id",
                schema: "vendas",
                table: "crm_clientes_fidelizados",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__crm_cliente_fidelizado_tenant_id",
                schema: "vendas",
                table: "crm_clientes_fidelizados",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_crm_clientes_fidelizados_tenant_cliente",
                schema: "vendas",
                table: "crm_clientes_fidelizados",
                columns: new[] { "tenant_id", "cliente_id" });

            migrationBuilder.CreateIndex(
                name: "ix__crm_configuracao_pix_relacional_sync_id",
                schema: "vendas",
                table: "crm_configuracoes_pix_relacional",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__crm_configuracao_pix_relacional_tenant_id",
                schema: "vendas",
                table: "crm_configuracoes_pix_relacional",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__crm_etapa_sync_id",
                schema: "vendas",
                table: "crm_etapas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__crm_etapa_tenant_id",
                schema: "vendas",
                table: "crm_etapas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_crm_etapas_tenant_pipeline",
                schema: "vendas",
                table: "crm_etapas",
                columns: new[] { "tenant_id", "pipeline_id" });

            migrationBuilder.CreateIndex(
                name: "ix__crm_etiqueta_sync_id",
                schema: "vendas",
                table: "crm_etiquetas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__crm_etiqueta_tenant_id",
                schema: "vendas",
                table: "crm_etiquetas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__crm_historico_sync_id",
                schema: "vendas",
                table: "crm_historicos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__crm_historico_tenant_id",
                schema: "vendas",
                table: "crm_historicos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_crm_historicos_tenant_entidade",
                schema: "vendas",
                table: "crm_historicos",
                columns: new[] { "tenant_id", "entidade_id" });

            migrationBuilder.CreateIndex(
                name: "ix__crm_lead_usuario_sync_id",
                schema: "vendas",
                table: "crm_lead_usuarios",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__crm_lead_usuario_tenant_id",
                schema: "vendas",
                table: "crm_lead_usuarios",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_crm_lead_usuarios_tenant_lead",
                schema: "vendas",
                table: "crm_lead_usuarios",
                columns: new[] { "tenant_id", "lead_id" });

            migrationBuilder.CreateIndex(
                name: "ix__crm_lead_sync_id",
                schema: "vendas",
                table: "crm_leads",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__crm_lead_tenant_id",
                schema: "vendas",
                table: "crm_leads",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_crm_leads_tenant_etapa",
                schema: "vendas",
                table: "crm_leads",
                columns: new[] { "tenant_id", "etapa_id" });

            migrationBuilder.CreateIndex(
                name: "ix_crm_leads_tenant_status",
                schema: "vendas",
                table: "crm_leads",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__crm_lista_publico_membro_sync_id",
                schema: "vendas",
                table: "crm_lista_publico_membros",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__crm_lista_publico_membro_tenant_id",
                schema: "vendas",
                table: "crm_lista_publico_membros",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_crm_lista_membros_tenant_lista",
                schema: "vendas",
                table: "crm_lista_publico_membros",
                columns: new[] { "tenant_id", "lista_publico_id" });

            migrationBuilder.CreateIndex(
                name: "ix__crm_lista_publico_sync_id",
                schema: "vendas",
                table: "crm_listas_publico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__crm_lista_publico_tenant_id",
                schema: "vendas",
                table: "crm_listas_publico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__crm_oportunidade_participante_sync_id",
                schema: "vendas",
                table: "crm_oportunidade_participantes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__crm_oportunidade_participante_tenant_id",
                schema: "vendas",
                table: "crm_oportunidade_participantes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_crm_oport_participantes_tenant_oport",
                schema: "vendas",
                table: "crm_oportunidade_participantes",
                columns: new[] { "tenant_id", "oportunidade_id" });

            migrationBuilder.CreateIndex(
                name: "ix__crm_oportunidade_sync_id",
                schema: "vendas",
                table: "crm_oportunidades",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__crm_oportunidade_tenant_id",
                schema: "vendas",
                table: "crm_oportunidades",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_crm_oportunidades_tenant_etapa",
                schema: "vendas",
                table: "crm_oportunidades",
                columns: new[] { "tenant_id", "etapa_id" });

            migrationBuilder.CreateIndex(
                name: "ix_crm_oportunidades_tenant_status",
                schema: "vendas",
                table: "crm_oportunidades",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__crm_origem_sync_id",
                schema: "vendas",
                table: "crm_origens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__crm_origem_tenant_id",
                schema: "vendas",
                table: "crm_origens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__crm_pagamento_pix_relacional_sync_id",
                schema: "vendas",
                table: "crm_pagamentos_pix_relacional",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__crm_pagamento_pix_relacional_tenant_id",
                schema: "vendas",
                table: "crm_pagamentos_pix_relacional",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_crm_pagamentos_pix_tenant_status",
                schema: "vendas",
                table: "crm_pagamentos_pix_relacional",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__crm_pipeline_sync_id",
                schema: "vendas",
                table: "crm_pipelines",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__crm_pipeline_tenant_id",
                schema: "vendas",
                table: "crm_pipelines",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__crm_pontuacao_fidelizacao_sync_id",
                schema: "vendas",
                table: "crm_pontuacoes_fidelizacao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__crm_pontuacao_fidelizacao_tenant_id",
                schema: "vendas",
                table: "crm_pontuacoes_fidelizacao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_crm_pontuacoes_tenant_cliente",
                schema: "vendas",
                table: "crm_pontuacoes_fidelizacao",
                columns: new[] { "tenant_id", "cliente_fidelizado_id" });

            migrationBuilder.CreateIndex(
                name: "ix__crm_ticket_resposta_sync_id",
                schema: "vendas",
                table: "crm_ticket_respostas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__crm_ticket_resposta_tenant_id",
                schema: "vendas",
                table: "crm_ticket_respostas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_crm_ticket_respostas_tenant_ticket",
                schema: "vendas",
                table: "crm_ticket_respostas",
                columns: new[] { "tenant_id", "ticket_id" });

            migrationBuilder.CreateIndex(
                name: "ix__crm_ticket_status_sync_id",
                schema: "vendas",
                table: "crm_ticket_status",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__crm_ticket_status_tenant_id",
                schema: "vendas",
                table: "crm_ticket_status",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__crm_ticket_sync_id",
                schema: "vendas",
                table: "crm_tickets",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__crm_ticket_tenant_id",
                schema: "vendas",
                table: "crm_tickets",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_crm_tickets_tenant_cliente",
                schema: "vendas",
                table: "crm_tickets",
                columns: new[] { "tenant_id", "cliente_id" });

            migrationBuilder.CreateIndex(
                name: "ix_crm_tickets_tenant_status",
                schema: "vendas",
                table: "crm_tickets",
                columns: new[] { "tenant_id", "status_id" });

            migrationBuilder.CreateIndex(
                name: "ix__crm_webform_responsavel_sync_id",
                schema: "vendas",
                table: "crm_webform_responsaveis",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__crm_webform_responsavel_tenant_id",
                schema: "vendas",
                table: "crm_webform_responsaveis",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_crm_webform_responsaveis_tenant_webform",
                schema: "vendas",
                table: "crm_webform_responsaveis",
                columns: new[] { "tenant_id", "webform_id" });

            migrationBuilder.CreateIndex(
                name: "ix__crm_webform_sync_id",
                schema: "vendas",
                table: "crm_webforms",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__crm_webform_tenant_id",
                schema: "vendas",
                table: "crm_webforms",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "uq_crm_webforms_tenant_identificador",
                schema: "vendas",
                table: "crm_webforms",
                columns: new[] { "tenant_id", "identificador_unico" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__servico_catalogo_sync_id",
                schema: "vendas",
                table: "servico_catalogos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__servico_catalogo_tenant_id",
                schema: "vendas",
                table: "servico_catalogos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_servico_catalogos_tenant_nome",
                schema: "vendas",
                table: "servico_catalogos",
                columns: new[] { "tenant_id", "nome" });

            migrationBuilder.CreateIndex(
                name: "i_x_servico_fatura_linhas_fatura_id",
                schema: "vendas",
                table: "servico_fatura_linhas",
                column: "fatura_id");

            migrationBuilder.CreateIndex(
                name: "ix__servico_fatura_linha_sync_id",
                schema: "vendas",
                table: "servico_fatura_linhas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__servico_fatura_linha_tenant_id",
                schema: "vendas",
                table: "servico_fatura_linhas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_servico_fatura_linhas_tenant_fatura",
                schema: "vendas",
                table: "servico_fatura_linhas",
                columns: new[] { "tenant_id", "fatura_id" });

            migrationBuilder.CreateIndex(
                name: "ix__servico_fatura_sync_id",
                schema: "vendas",
                table: "servico_faturas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__servico_fatura_tenant_id",
                schema: "vendas",
                table: "servico_faturas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_servico_faturas_tenant_cliente",
                schema: "vendas",
                table: "servico_faturas",
                columns: new[] { "tenant_id", "cliente_id" });

            migrationBuilder.CreateIndex(
                name: "uq_servico_faturas_tenant_numero",
                schema: "vendas",
                table: "servico_faturas",
                columns: new[] { "tenant_id", "numero_fatura" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__servico_historico_sync_id",
                schema: "vendas",
                table: "servico_historicos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__servico_historico_tenant_id",
                schema: "vendas",
                table: "servico_historicos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_servico_historicos_tenant_entidade",
                schema: "vendas",
                table: "servico_historicos",
                columns: new[] { "tenant_id", "entidade_id" });

            migrationBuilder.CreateIndex(
                name: "ix__servico_lancamento_financeiro_ref_sync_id",
                schema: "vendas",
                table: "servico_lancamento_financeiro_refs",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__servico_lancamento_financeiro_ref_tenant_id",
                schema: "vendas",
                table: "servico_lancamento_financeiro_refs",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_servico_lanc_fin_ref_tenant_fatura",
                schema: "vendas",
                table: "servico_lancamento_financeiro_refs",
                columns: new[] { "tenant_id", "fatura_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "crm_atividades",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "crm_campanha_listas",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "crm_campanha_logs",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "crm_campanha_rastreadores",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "crm_campanhas",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "crm_clientes_fidelizados",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "crm_configuracoes_pix_relacional",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "crm_etapas",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "crm_etiquetas",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "crm_historicos",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "crm_lead_usuarios",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "crm_leads",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "crm_lista_publico_membros",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "crm_listas_publico",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "crm_oportunidade_participantes",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "crm_oportunidades",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "crm_origens",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "crm_pagamentos_pix_relacional",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "crm_pipelines",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "crm_pontuacoes_fidelizacao",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "crm_ticket_respostas",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "crm_ticket_status",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "crm_tickets",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "crm_webform_responsaveis",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "crm_webforms",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "servico_catalogos",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "servico_fatura_linhas",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "servico_historicos",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "servico_lancamento_financeiro_refs",
                schema: "vendas");

            migrationBuilder.DropTable(
                name: "servico_faturas",
                schema: "vendas");
        }
    }
}
