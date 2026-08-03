using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Aplicativo.Migrations
{
    /// <summary>
    /// Cria as tabelas que estavam no modelo/snapshot mas cuja migration CreateTable nunca foi
    /// gerada (drift Classe A) — endpoints retornavam 500 por relacao inexistente e o smoke-from-zero
    /// nao subia. DDL extraido do proprio modelo (Database.GenerateCreateScript), portanto fiel as
    /// configuracoes de entidade. Sem xmin fisico (o modelo usa xmin de sistema do Postgres).
    /// </summary>
    public partial class CreateWfUplSuperadminFaltantes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE TABLE aplicativo.solicitacoes_upgrade_versao (
    id uuid NOT NULL,
    versao_atual character varying(50) NOT NULL,
    versao_alvo character varying(50) NOT NULL,
    motivo character varying(1000) NOT NULL,
    status integer NOT NULL,
    solicitado_por character varying(100),
    aprovado_por character varying(100),
    comentario character varying(1000),
    aprovado_em timestamp with time zone,
    executado_em timestamp with time zone,
    log text,
    rollback_disponivel boolean NOT NULL,
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_solicitacoes_upgrade_versao PRIMARY KEY (id)
);


CREATE TABLE aplicativo.upl_arquivos (
    id uuid NOT NULL,
    owner_usuario_id uuid NOT NULL,
    uploaded_usuario_id uuid,
    nome_original character varying(400) NOT NULL,
    nome_armazenado character varying(200) NOT NULL,
    extensao character varying(20),
    tamanho_bytes bigint NOT NULL,
    hash_arquivo character varying(128),
    pasta_id character varying(200),
    servidor_storage_id character varying(100),
    origem_upload integer NOT NULL,
    status integer NOT NULL,
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_upl_arquivos PRIMARY KEY (id)
);


CREATE TABLE aplicativo.upl_arquivos_xml_saida (
    id uuid NOT NULL,
    nome_arquivo character varying(400) NOT NULL,
    qtd_xmls integer NOT NULL,
    qtd_xmls_invalidos integer NOT NULL,
    qtd_produtos_localizados integer NOT NULL,
    qtd_clientes_localizados integer NOT NULL,
    qtd_produtos_importados integer NOT NULL,
    qtd_clientes_importados integer NOT NULL,
    mensagem_erro character varying(2000) NOT NULL,
    status integer NOT NULL,
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_upl_arquivos_xml_saida PRIMARY KEY (id)
);


CREATE TABLE aplicativo.upl_atualizacoes_bloco (
    id uuid NOT NULL,
    atualizacao_versao_id uuid NOT NULL,
    nome_arquivo character varying(400) NOT NULL,
    identificador_bloco character varying(200),
    status integer NOT NULL,
    log text,
    aplicado_em timestamp with time zone,
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_upl_atualizacoes_bloco PRIMARY KEY (id)
);


CREATE TABLE aplicativo.upl_atualizacoes_job (
    id uuid NOT NULL,
    tipo character varying(30) NOT NULL,
    nome character varying(200) NOT NULL,
    funcao_nome character varying(200),
    status integer NOT NULL,
    payload_json text,
    log text,
    finalizado_em timestamp with time zone,
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_upl_atualizacoes_job PRIMARY KEY (id)
);


CREATE TABLE aplicativo.upl_atualizacoes_versao (
    id uuid NOT NULL,
    versao_atual character varying(50) NOT NULL,
    versao_alvo character varying(50) NOT NULL,
    status integer NOT NULL,
    log text,
    iniciado_em timestamp with time zone,
    finalizado_em timestamp with time zone,
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_upl_atualizacoes_versao PRIMARY KEY (id)
);


CREATE TABLE aplicativo.upl_configuracoes (
    id uuid NOT NULL,
    chave character varying(150) NOT NULL,
    valor text,
    ativo boolean NOT NULL,
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_upl_configuracoes PRIMARY KEY (id)
);


CREATE TABLE aplicativo.upl_execucoes_exportacao (
    id uuid NOT NULL,
    usuario_id uuid NOT NULL,
    entidade character varying(100) NOT NULL,
    filtros_json text,
    status integer NOT NULL,
    arquivo_id uuid,
    url_download character varying(1000),
    mensagem_erro character varying(2000),
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_upl_execucoes_exportacao PRIMARY KEY (id)
);


CREATE TABLE aplicativo.upl_execucoes_importacao (
    id uuid NOT NULL,
    usuario_id uuid NOT NULL,
    import_ref character varying(64) NOT NULL,
    tipo_importacao character varying(50) NOT NULL,
    arquivo_id uuid,
    arquivo_temporario_chave character varying(200),
    arquivo_temporario_nome character varying(400),
    status integer NOT NULL,
    total_linhas integer,
    linhas_sucesso integer,
    linhas_ignoradas integer,
    quantidade_erros integer,
    referencia_erro character varying(64),
    resultado integer,
    finalizado_em timestamp with time zone,
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_upl_execucoes_importacao PRIMARY KEY (id)
);


CREATE TABLE aplicativo.upl_execucoes_upload (
    id uuid NOT NULL,
    usuario_id uuid NOT NULL,
    usuario_upload_id uuid,
    origem integer NOT NULL,
    nome_original character varying(400) NOT NULL,
    extensao character varying(20),
    tamanho_bytes bigint,
    mime_type character varying(150),
    status integer NOT NULL,
    mensagem_erro character varying(2000),
    pasta_destino_id character varying(200),
    arquivo_id uuid,
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_upl_execucoes_upload PRIMARY KEY (id)
);


CREATE TABLE aplicativo.upl_exportacao_campos (
    id uuid NOT NULL,
    execucao_exportacao_id uuid NOT NULL,
    origem_campo integer NOT NULL,
    chave_campo character varying(150) NOT NULL,
    rotulo character varying(200),
    ordem integer,
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_upl_exportacao_campos PRIMARY KEY (id)
);


CREATE TABLE aplicativo.upl_filas_url_remota (
    id uuid NOT NULL,
    usuario_id uuid NOT NULL,
    url character varying(2000) NOT NULL,
    servidor_processamento_id character varying(100),
    status_job integer NOT NULL,
    tamanho_total bigint,
    tamanho_baixado bigint,
    percentual_download numeric(18,2),
    pasta_destino_id character varying(200),
    novo_arquivo_id uuid,
    mensagem_erro character varying(2000),
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_upl_filas_url_remota PRIMARY KEY (id)
);


CREATE TABLE aplicativo.upl_historicos (
    id uuid NOT NULL,
    entidade character varying(150) NOT NULL,
    entidade_id_referencia character varying(100) NOT NULL,
    acao character varying(100) NOT NULL,
    usuario_id uuid,
    ip_origem character varying(64),
    payload_json text,
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_upl_historicos PRIMARY KEY (id)
);


CREATE TABLE aplicativo.upl_importacao_erros (
    id uuid NOT NULL,
    execucao_importacao_id uuid NOT NULL,
    referencia_erro character varying(64) NOT NULL,
    numero_linha integer,
    atributo character varying(150),
    mensagem character varying(2000) NOT NULL,
    formato_exibicao character varying(20),
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_upl_importacao_erros PRIMARY KEY (id)
);


CREATE TABLE aplicativo.upl_importacao_linhas (
    id uuid NOT NULL,
    execucao_importacao_id uuid NOT NULL,
    numero_linha integer NOT NULL,
    status integer NOT NULL,
    entidade_destino character varying(150),
    entidade_destino_id character varying(100),
    payload_linha text,
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_upl_importacao_linhas PRIMARY KEY (id)
);


CREATE TABLE aplicativo.upl_importacoes_xml (
    id uuid NOT NULL,
    empresa_id uuid,
    xml text NOT NULL,
    tipo_de_xml integer NOT NULL,
    nfe_id character varying(100) NOT NULL,
    status_importacao_xml integer NOT NULL,
    mensagem_erro_importacao_xml character varying(2000),
    status_cadastro integer NOT NULL,
    mensagem_erro_cadastro character varying(2000),
    status_salvar_pdf integer NOT NULL,
    mensagem_erro_salvar_pdf character varying(2000),
    codigo_sefaz integer NOT NULL,
    tipo_evento character varying(50) NOT NULL,
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_upl_importacoes_xml PRIMARY KEY (id)
);


CREATE TABLE aplicativo.upl_mapeamentos_importacao (
    id uuid NOT NULL,
    usuario_id uuid NOT NULL,
    tipo_importacao character varying(50) NOT NULL,
    nome character varying(200) NOT NULL,
    mapa_colunas text NOT NULL,
    ativo boolean NOT NULL,
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_upl_mapeamentos_importacao PRIMARY KEY (id)
);


CREATE TABLE aplicativo.upl_migracoes_offline (
    id uuid NOT NULL,
    usuario_id uuid NOT NULL,
    conta_destino character varying(200) NOT NULL,
    caminho_origem character varying(1000) NOT NULL,
    pasta_inicial_destino character varying(1000) NOT NULL,
    modo character varying(20) NOT NULL,
    status integer NOT NULL,
    arquivos_processados integer,
    mensagem_erro character varying(2000),
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_upl_migracoes_offline PRIMARY KEY (id)
);


CREATE TABLE aplicativo.upl_upload_partes (
    id uuid NOT NULL,
    execucao_upload_id uuid NOT NULL,
    byte_inicio bigint NOT NULL,
    byte_fim bigint NOT NULL,
    total_bytes bigint NOT NULL,
    caminho_temporario character varying(1000) NOT NULL,
    completa boolean NOT NULL,
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_upl_upload_partes PRIMARY KEY (id)
);


CREATE TABLE aplicativo.wf_agendamentos (
    id uuid NOT NULL,
    nome character varying(200) NOT NULL,
    expressao_intervalar character varying(100) NOT NULL,
    ativo boolean NOT NULL,
    proxima_execucao_em timestamp with time zone,
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_wf_agendamentos PRIMARY KEY (id)
);


CREATE TABLE aplicativo.wf_anexos (
    id uuid NOT NULL,
    entidade_tipo character varying(50) NOT NULL,
    entidade_id_referencia character varying(100) NOT NULL,
    arquivo_id character varying(100) NOT NULL,
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_wf_anexos PRIMARY KEY (id)
);


CREATE TABLE aplicativo.wf_definicoes (
    id uuid NOT NULL,
    modulo character varying(100) NOT NULL,
    entidade character varying(150) NOT NULL,
    nome character varying(200) NOT NULL,
    versao integer NOT NULL,
    status integer NOT NULL,
    criado_por_usuario_id uuid,
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_wf_definicoes PRIMARY KEY (id)
);


CREATE TABLE aplicativo.wf_estados (
    id uuid NOT NULL,
    definicao_id uuid NOT NULL,
    codigo character varying(50) NOT NULL,
    nome character varying(150) NOT NULL,
    inicial boolean NOT NULL,
    final boolean NOT NULL,
    ativo boolean NOT NULL,
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_wf_estados PRIMARY KEY (id)
);


CREATE TABLE aplicativo.wf_eventos_dominio (
    id uuid NOT NULL,
    entidade_tipo character varying(150) NOT NULL,
    entidade_id_referencia character varying(100) NOT NULL,
    chave character varying(150) NOT NULL,
    valor text,
    publicado boolean NOT NULL,
    publicado_em timestamp with time zone,
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_wf_eventos_dominio PRIMARY KEY (id)
);


CREATE TABLE aplicativo.wf_historicos (
    id uuid NOT NULL,
    instancia_id uuid,
    entidade_tipo character varying(150) NOT NULL,
    entidade_id_referencia character varying(100) NOT NULL,
    acao character varying(100) NOT NULL,
    estado_anterior character varying(50),
    estado_novo character varying(50),
    usuario_id uuid,
    ip_origem character varying(64),
    payload_json text,
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_wf_historicos PRIMARY KEY (id)
);


CREATE TABLE aplicativo.wf_instancias (
    id uuid NOT NULL,
    definicao_id uuid NOT NULL,
    entidade_tipo character varying(150) NOT NULL,
    entidade_id_referencia character varying(100) NOT NULL,
    estado_atual_id uuid,
    responsavel_usuario_id uuid,
    status integer NOT NULL,
    modulo character varying(100) NOT NULL,
    descricao character varying(500) NOT NULL,
    valor_referencia numeric(18,2),
    command_type character varying(500),
    payload text,
    comentario character varying(1000),
    aprovado_por_usuario_id uuid,
    aprovado_por character varying(100),
    decidido_em timestamp with time zone,
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_wf_instancias PRIMARY KEY (id)
);


CREATE TABLE aplicativo.wf_job_tentativas (
    id uuid NOT NULL,
    job_id uuid NOT NULL,
    numero_tentativa integer NOT NULL,
    status integer NOT NULL,
    mensagem character varying(2000),
    iniciado_em timestamp with time zone,
    finalizado_em timestamp with time zone,
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_wf_job_tentativas PRIMARY KEY (id)
);


CREATE TABLE aplicativo.wf_jobs (
    id uuid NOT NULL,
    agendamento_id uuid NOT NULL,
    nome character varying(200) NOT NULL,
    status integer NOT NULL,
    tentativa_atual integer NOT NULL,
    contexto_usuario_id uuid,
    previsto_para timestamp with time zone NOT NULL,
    iniciado_em timestamp with time zone,
    finalizado_em timestamp with time zone,
    log text,
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_wf_jobs PRIMARY KEY (id)
);


CREATE TABLE aplicativo.wf_parametros (
    id uuid NOT NULL,
    chave character varying(150) NOT NULL,
    valor text,
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_wf_parametros PRIMARY KEY (id)
);


CREATE TABLE aplicativo.wf_solicitacoes (
    id uuid NOT NULL,
    start_date timestamp with time zone,
    end_date timestamp with time zone,
    total_days numeric(18,2),
    reason character varying(1000),
    attachment character varying(500),
    status integer NOT NULL,
    approver_comment character varying(1000),
    approved_at timestamp with time zone,
    employee_id character varying(100),
    leave_type_id character varying(100),
    approved_by character varying(100),
    creator_id character varying(100),
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_wf_solicitacoes PRIMARY KEY (id)
);


CREATE TABLE aplicativo.wf_tarefas (
    id uuid NOT NULL,
    instancia_id uuid NOT NULL,
    titulo character varying(300) NOT NULL,
    responsavel_usuario_id uuid,
    responsavel_papel integer,
    status integer NOT NULL,
    prazo_em timestamp with time zone,
    concluida_em timestamp with time zone,
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_wf_tarefas PRIMARY KEY (id)
);


CREATE TABLE aplicativo.wf_transicoes (
    id uuid NOT NULL,
    definicao_id uuid NOT NULL,
    estado_origem_id uuid NOT NULL,
    estado_destino_id uuid NOT NULL,
    evento integer NOT NULL,
    permissao_requerida integer NOT NULL,
    exige_comentario boolean NOT NULL,
    publica_evento boolean NOT NULL,
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_wf_transicoes PRIMARY KEY (id)
);


CREATE UNIQUE INDEX ix__solicitacao_upgrade_versao_sync_id ON aplicativo.solicitacoes_upgrade_versao (sync_id);


CREATE INDEX ix__solicitacao_upgrade_versao_tenant_id ON aplicativo.solicitacoes_upgrade_versao (tenant_id);


CREATE UNIQUE INDEX ix_upgrade_versao_em_execucao_unico ON aplicativo.solicitacoes_upgrade_versao (status) WHERE status = 3;


CREATE UNIQUE INDEX ix__upl_arquivo_sync_id ON aplicativo.upl_arquivos (sync_id);


CREATE INDEX ix__upl_arquivo_tenant_id ON aplicativo.upl_arquivos (tenant_id);


CREATE INDEX ix_upl_arquivos_hash_tamanho ON aplicativo.upl_arquivos (hash_arquivo, tamanho_bytes);


CREATE UNIQUE INDEX ix_upl_arquivos_nome_armazenado ON aplicativo.upl_arquivos (nome_armazenado);


CREATE UNIQUE INDEX ix__upl_arquivo_xml_saida_sync_id ON aplicativo.upl_arquivos_xml_saida (sync_id);


CREATE INDEX ix__upl_arquivo_xml_saida_tenant_id ON aplicativo.upl_arquivos_xml_saida (tenant_id);


CREATE INDEX ix_upl_arquivo_xml_saida_status ON aplicativo.upl_arquivos_xml_saida (status);


CREATE UNIQUE INDEX ix__upl_atualizacao_bloco_sync_id ON aplicativo.upl_atualizacoes_bloco (sync_id);


CREATE INDEX ix__upl_atualizacao_bloco_tenant_id ON aplicativo.upl_atualizacoes_bloco (tenant_id);


CREATE UNIQUE INDEX ix_upl_atualizacao_bloco_tenant_arquivo ON aplicativo.upl_atualizacoes_bloco (tenant_id, nome_arquivo);


CREATE UNIQUE INDEX ix__upl_atualizacao_job_sync_id ON aplicativo.upl_atualizacoes_job (sync_id);


CREATE INDEX ix__upl_atualizacao_job_tenant_id ON aplicativo.upl_atualizacoes_job (tenant_id);


CREATE INDEX ix_upl_atualizacao_job_status ON aplicativo.upl_atualizacoes_job (status);


CREATE UNIQUE INDEX ix__upl_atualizacao_versao_sync_id ON aplicativo.upl_atualizacoes_versao (sync_id);


CREATE INDEX ix__upl_atualizacao_versao_tenant_id ON aplicativo.upl_atualizacoes_versao (tenant_id);


CREATE INDEX ix_upl_atualizacao_versao_status ON aplicativo.upl_atualizacoes_versao (status);


CREATE UNIQUE INDEX ix__upl_configuracao_sync_id ON aplicativo.upl_configuracoes (sync_id);


CREATE INDEX ix__upl_configuracao_tenant_id ON aplicativo.upl_configuracoes (tenant_id);


CREATE UNIQUE INDEX ix_upl_configuracoes_tenant_chave ON aplicativo.upl_configuracoes (tenant_id, chave);


CREATE UNIQUE INDEX ix__upl_execucao_exportacao_sync_id ON aplicativo.upl_execucoes_exportacao (sync_id);


CREATE INDEX ix__upl_execucao_exportacao_tenant_id ON aplicativo.upl_execucoes_exportacao (tenant_id);


CREATE INDEX ix_upl_execucoes_exportacao_status ON aplicativo.upl_execucoes_exportacao (status);


CREATE UNIQUE INDEX ix__upl_execucao_importacao_sync_id ON aplicativo.upl_execucoes_importacao (sync_id);


CREATE INDEX ix__upl_execucao_importacao_tenant_id ON aplicativo.upl_execucoes_importacao (tenant_id);


CREATE UNIQUE INDEX ix_upl_execucoes_importacao_import_ref ON aplicativo.upl_execucoes_importacao (import_ref);


CREATE INDEX ix_upl_execucoes_importacao_status ON aplicativo.upl_execucoes_importacao (status);


CREATE UNIQUE INDEX ix__upl_execucao_upload_sync_id ON aplicativo.upl_execucoes_upload (sync_id);


CREATE INDEX ix__upl_execucao_upload_tenant_id ON aplicativo.upl_execucoes_upload (tenant_id);


CREATE INDEX ix_upl_execucoes_upload_status ON aplicativo.upl_execucoes_upload (status);


CREATE UNIQUE INDEX ix__upl_exportacao_campo_sync_id ON aplicativo.upl_exportacao_campos (sync_id);


CREATE INDEX ix__upl_exportacao_campo_tenant_id ON aplicativo.upl_exportacao_campos (tenant_id);


CREATE INDEX ix_upl_exportacao_campos_execucao ON aplicativo.upl_exportacao_campos (execucao_exportacao_id);


CREATE UNIQUE INDEX ix__upl_fila_url_remota_sync_id ON aplicativo.upl_filas_url_remota (sync_id);


CREATE INDEX ix__upl_fila_url_remota_tenant_id ON aplicativo.upl_filas_url_remota (tenant_id);


CREATE INDEX ix_upl_fila_url_remota_status ON aplicativo.upl_filas_url_remota (status_job);


CREATE UNIQUE INDEX ix__upl_historico_sync_id ON aplicativo.upl_historicos (sync_id);


CREATE INDEX ix__upl_historico_tenant_id ON aplicativo.upl_historicos (tenant_id);


CREATE INDEX ix_upl_historicos_entidade ON aplicativo.upl_historicos (entidade, entidade_id_referencia);


CREATE UNIQUE INDEX ix__upl_importacao_erro_sync_id ON aplicativo.upl_importacao_erros (sync_id);


CREATE INDEX ix__upl_importacao_erro_tenant_id ON aplicativo.upl_importacao_erros (tenant_id);


CREATE INDEX ix_upl_importacao_erros_referencia ON aplicativo.upl_importacao_erros (referencia_erro);


CREATE UNIQUE INDEX ix__upl_importacao_linha_sync_id ON aplicativo.upl_importacao_linhas (sync_id);


CREATE INDEX ix__upl_importacao_linha_tenant_id ON aplicativo.upl_importacao_linhas (tenant_id);


CREATE INDEX ix_upl_importacao_linhas_execucao ON aplicativo.upl_importacao_linhas (execucao_importacao_id);


CREATE INDEX ix_upl_importacao_linhas_status ON aplicativo.upl_importacao_linhas (status);


CREATE UNIQUE INDEX ix__upl_importacao_xml_sync_id ON aplicativo.upl_importacoes_xml (sync_id);


CREATE INDEX ix__upl_importacao_xml_tenant_id ON aplicativo.upl_importacoes_xml (tenant_id);


CREATE INDEX ix_upl_importacao_xml_nfe ON aplicativo.upl_importacoes_xml (nfe_id);


CREATE INDEX ix_upl_importacao_xml_status ON aplicativo.upl_importacoes_xml (status_importacao_xml);


CREATE UNIQUE INDEX ix__upl_mapeamento_importacao_sync_id ON aplicativo.upl_mapeamentos_importacao (sync_id);


CREATE INDEX ix__upl_mapeamento_importacao_tenant_id ON aplicativo.upl_mapeamentos_importacao (tenant_id);


CREATE INDEX ix_upl_mapeamentos_usuario_tipo ON aplicativo.upl_mapeamentos_importacao (usuario_id, tipo_importacao);


CREATE UNIQUE INDEX ix__upl_migracao_offline_sync_id ON aplicativo.upl_migracoes_offline (sync_id);


CREATE INDEX ix__upl_migracao_offline_tenant_id ON aplicativo.upl_migracoes_offline (tenant_id);


CREATE INDEX ix_upl_migracao_offline_status ON aplicativo.upl_migracoes_offline (status);


CREATE UNIQUE INDEX ix__upl_upload_parte_sync_id ON aplicativo.upl_upload_partes (sync_id);


CREATE INDEX ix__upl_upload_parte_tenant_id ON aplicativo.upl_upload_partes (tenant_id);


CREATE INDEX ix_upl_upload_partes_criado_em ON aplicativo.upl_upload_partes (criado_em);


CREATE INDEX ix_upl_upload_partes_execucao ON aplicativo.upl_upload_partes (execucao_upload_id);


CREATE UNIQUE INDEX ix__wf_agendamento_sync_id ON aplicativo.wf_agendamentos (sync_id);


CREATE INDEX ix__wf_agendamento_tenant_id ON aplicativo.wf_agendamentos (tenant_id);


CREATE INDEX ix_wf_agendamentos_ativo ON aplicativo.wf_agendamentos (ativo);


CREATE UNIQUE INDEX ix__wf_anexo_sync_id ON aplicativo.wf_anexos (sync_id);


CREATE INDEX ix__wf_anexo_tenant_id ON aplicativo.wf_anexos (tenant_id);


CREATE UNIQUE INDEX ix__wf_definicao_sync_id ON aplicativo.wf_definicoes (sync_id);


CREATE INDEX ix__wf_definicao_tenant_id ON aplicativo.wf_definicoes (tenant_id);


CREATE INDEX ix_wf_definicoes_tenant_modulo_entidade ON aplicativo.wf_definicoes (tenant_id, modulo, entidade);


CREATE UNIQUE INDEX ix__wf_estado_sync_id ON aplicativo.wf_estados (sync_id);


CREATE INDEX ix__wf_estado_tenant_id ON aplicativo.wf_estados (tenant_id);


CREATE UNIQUE INDEX ix_wf_estados_definicao_codigo ON aplicativo.wf_estados (definicao_id, codigo);


CREATE UNIQUE INDEX ix__wf_evento_dominio_sync_id ON aplicativo.wf_eventos_dominio (sync_id);


CREATE INDEX ix__wf_evento_dominio_tenant_id ON aplicativo.wf_eventos_dominio (tenant_id);


CREATE UNIQUE INDEX ix__wf_historico_sync_id ON aplicativo.wf_historicos (sync_id);


CREATE INDEX ix__wf_historico_tenant_id ON aplicativo.wf_historicos (tenant_id);


CREATE INDEX ix_wf_historicos_entidade ON aplicativo.wf_historicos (entidade_tipo, entidade_id_referencia);


CREATE INDEX ix_wf_historicos_instancia ON aplicativo.wf_historicos (instancia_id);


CREATE UNIQUE INDEX ix__wf_instancia_sync_id ON aplicativo.wf_instancias (sync_id);


CREATE INDEX ix__wf_instancia_tenant_id ON aplicativo.wf_instancias (tenant_id);


CREATE INDEX ix_wf_instancias_entidade ON aplicativo.wf_instancias (tenant_id, entidade_tipo, entidade_id_referencia);


CREATE INDEX ix_wf_instancias_status ON aplicativo.wf_instancias (status);


CREATE UNIQUE INDEX ix__wf_job_tentativa_sync_id ON aplicativo.wf_job_tentativas (sync_id);


CREATE INDEX ix__wf_job_tentativa_tenant_id ON aplicativo.wf_job_tentativas (tenant_id);


CREATE INDEX ix_wf_job_tentativas_job ON aplicativo.wf_job_tentativas (job_id);


CREATE UNIQUE INDEX ix__wf_job_sync_id ON aplicativo.wf_jobs (sync_id);


CREATE INDEX ix__wf_job_tenant_id ON aplicativo.wf_jobs (tenant_id);


CREATE INDEX ix_wf_jobs_agenda_status ON aplicativo.wf_jobs (agendamento_id, status);


CREATE UNIQUE INDEX ix__wf_parametro_sync_id ON aplicativo.wf_parametros (sync_id);


CREATE INDEX ix__wf_parametro_tenant_id ON aplicativo.wf_parametros (tenant_id);


CREATE UNIQUE INDEX ix_wf_parametros_tenant_chave ON aplicativo.wf_parametros (tenant_id, chave);


CREATE UNIQUE INDEX ix__wf_solicitacao_sync_id ON aplicativo.wf_solicitacoes (sync_id);


CREATE INDEX ix__wf_solicitacao_tenant_id ON aplicativo.wf_solicitacoes (tenant_id);


CREATE INDEX ix_wf_solicitacoes_status ON aplicativo.wf_solicitacoes (status);


CREATE UNIQUE INDEX ix__wf_tarefa_sync_id ON aplicativo.wf_tarefas (sync_id);


CREATE INDEX ix__wf_tarefa_tenant_id ON aplicativo.wf_tarefas (tenant_id);


CREATE INDEX ix_wf_tarefas_instancia ON aplicativo.wf_tarefas (instancia_id);


CREATE INDEX ix_wf_tarefas_status ON aplicativo.wf_tarefas (status);


CREATE UNIQUE INDEX ix__wf_transicao_sync_id ON aplicativo.wf_transicoes (sync_id);


CREATE INDEX ix__wf_transicao_tenant_id ON aplicativo.wf_transicoes (tenant_id);


CREATE INDEX ix_wf_transicoes_definicao_evento ON aplicativo.wf_transicoes (definicao_id, evento);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS aplicativo.wf_agendamentos CASCADE;
                DROP TABLE IF EXISTS aplicativo.wf_anexos CASCADE;
                DROP TABLE IF EXISTS aplicativo.wf_definicoes CASCADE;
                DROP TABLE IF EXISTS aplicativo.wf_estados CASCADE;
                DROP TABLE IF EXISTS aplicativo.wf_eventos_dominio CASCADE;
                DROP TABLE IF EXISTS aplicativo.wf_historicos CASCADE;
                DROP TABLE IF EXISTS aplicativo.wf_instancias CASCADE;
                DROP TABLE IF EXISTS aplicativo.wf_job_tentativas CASCADE;
                DROP TABLE IF EXISTS aplicativo.wf_jobs CASCADE;
                DROP TABLE IF EXISTS aplicativo.wf_parametros CASCADE;
                DROP TABLE IF EXISTS aplicativo.wf_solicitacoes CASCADE;
                DROP TABLE IF EXISTS aplicativo.wf_tarefas CASCADE;
                DROP TABLE IF EXISTS aplicativo.wf_transicoes CASCADE;
                DROP TABLE IF EXISTS aplicativo.upl_arquivos CASCADE;
                DROP TABLE IF EXISTS aplicativo.upl_arquivos_xml_saida CASCADE;
                DROP TABLE IF EXISTS aplicativo.upl_atualizacoes_bloco CASCADE;
                DROP TABLE IF EXISTS aplicativo.upl_atualizacoes_job CASCADE;
                DROP TABLE IF EXISTS aplicativo.upl_atualizacoes_versao CASCADE;
                DROP TABLE IF EXISTS aplicativo.upl_configuracoes CASCADE;
                DROP TABLE IF EXISTS aplicativo.upl_execucoes_exportacao CASCADE;
                DROP TABLE IF EXISTS aplicativo.upl_execucoes_importacao CASCADE;
                DROP TABLE IF EXISTS aplicativo.upl_execucoes_upload CASCADE;
                DROP TABLE IF EXISTS aplicativo.upl_exportacao_campos CASCADE;
                DROP TABLE IF EXISTS aplicativo.upl_filas_url_remota CASCADE;
                DROP TABLE IF EXISTS aplicativo.upl_historicos CASCADE;
                DROP TABLE IF EXISTS aplicativo.upl_importacao_erros CASCADE;
                DROP TABLE IF EXISTS aplicativo.upl_importacao_linhas CASCADE;
                DROP TABLE IF EXISTS aplicativo.upl_importacoes_xml CASCADE;
                DROP TABLE IF EXISTS aplicativo.upl_mapeamentos_importacao CASCADE;
                DROP TABLE IF EXISTS aplicativo.upl_migracoes_offline CASCADE;
                DROP TABLE IF EXISTS aplicativo.upl_upload_partes CASCADE;
                DROP TABLE IF EXISTS aplicativo.solicitacoes_upgrade_versao CASCADE;
");
        }
    }
}
