using System.Collections.Generic;
using System.Text.Json;
using Epros.Infrastructure.Data;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Domain.Entities.Upload;
using Epros.Modules.Aplicativo.Domain.Entities.Workflow;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Infrastructure.Data
{
    public class ContextAplicativo : ContextBase
    {
        public DbSet<UsuarioInterno> UsuariosInternos => Set<UsuarioInterno>();
        public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
        public DbSet<ComunicacaoSuperAdmin> ComunicacoesSuperAdmin => Set<ComunicacaoSuperAdmin>();
        public DbSet<LandingPageSettings> LandingPagesSettings => Set<LandingPageSettings>();
        public DbSet<MarketplaceSettings> MarketplacesSettings => Set<MarketplaceSettings>();
        public DbSet<CustomPage> CustomPages => Set<CustomPage>();
        public DbSet<NewsletterSubscriber> NewsletterSubscribers => Set<NewsletterSubscriber>();
        public DbSet<ExecucaoMassaGlobal> ExecucoesMassaGlobal => Set<ExecucaoMassaGlobal>();
        public DbSet<LogExecucaoMassa> LogsExecucaoMassa => Set<LogExecucaoMassa>();
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<UsuarioEmpresa> UsuariosEmpresas => Set<UsuarioEmpresa>();
        public DbSet<AcessoUsuarioTenant> AcessosUsuarioTenant => Set<AcessoUsuarioTenant>();
        public DbSet<IdentidadeExterna> IdentidadesExternas => Set<IdentidadeExterna>();
        public DbSet<SessaoUsuario> SessoesUsuarios => Set<SessaoUsuario>();
        public DbSet<PersonalAccessToken> PersonalAccessTokens => Set<PersonalAccessToken>();
        public DbSet<HistoricoLogin> HistoricosLogin => Set<HistoricoLogin>();
        public DbSet<BannedIp> BannedIps => Set<BannedIp>();
        public DbSet<ConfiguracaoEmpresa> ConfiguracoesEmpresas => Set<ConfiguracaoEmpresa>();
        public DbSet<Idioma> Idiomas => Set<Idioma>();
        public DbSet<AnoFinanceiro> AnosFinanceiros => Set<AnoFinanceiro>();
        public DbSet<SessaoImpersonacao> SessoesImpersonacao => Set<SessaoImpersonacao>();
        public DbSet<PreferenciaUsuario> PreferenciasUsuarios => Set<PreferenciaUsuario>();
        public DbSet<InstalacaoState> InstalacaoStates => Set<InstalacaoState>();
        public DbSet<UpdateLog> UpdateLogs => Set<UpdateLog>();
        // RBAC/Menu (Menu, MenuItemNivel1/2, PerfilUsuario, PerfilUsuarioAcesso) DEPRECADO no Aplicativo.
        // Dono único = GestaoClientes (PerfilAcesso + PerfilAcessoMenu + Menu, schema plataforma). Ver CONVENCAO_CODIGO.md §1.2.
        public DbSet<Epros.Shared.Domain.Events.OutboxMessage> OutboxMessages => Set<Epros.Shared.Domain.Events.OutboxMessage>();

        // Governança de upgrade/versão (Super Admin — APP-TEN-010)
        public DbSet<SolicitacaoUpgradeVersao> SolicitacoesUpgradeVersao => Set<SolicitacaoUpgradeVersao>();

        // Motor de Workflow genérico (PLT-WF)
        public DbSet<WfDefinicao> WfDefinicoes => Set<WfDefinicao>();
        public DbSet<WfEstado> WfEstados => Set<WfEstado>();
        public DbSet<WfTransicao> WfTransicoes => Set<WfTransicao>();
        public DbSet<WfInstancia> WfInstancias => Set<WfInstancia>();
        public DbSet<WfTarefa> WfTarefas => Set<WfTarefa>();
        public DbSet<WfSolicitacao> WfSolicitacoes => Set<WfSolicitacao>();
        public DbSet<WfHistorico> WfHistoricos => Set<WfHistorico>();
        public DbSet<WfAnexo> WfAnexos => Set<WfAnexo>();
        public DbSet<WfEventoDominio> WfEventosDominio => Set<WfEventoDominio>();
        public DbSet<WfParametro> WfParametros => Set<WfParametro>();
        public DbSet<WfAgendamento> WfAgendamentos => Set<WfAgendamento>();
        public DbSet<WfJob> WfJobs => Set<WfJob>();
        public DbSet<WfJobTentativa> WfJobTentativas => Set<WfJobTentativa>();

        // Upload e Migração de Dados genéricos (PLT-UPL)
        public DbSet<UplConfiguracao> UplConfiguracoes => Set<UplConfiguracao>();
        public DbSet<UplExecucaoUpload> UplExecucoesUpload => Set<UplExecucaoUpload>();
        public DbSet<UplUploadParte> UplUploadPartes => Set<UplUploadParte>();
        public DbSet<UplArquivo> UplArquivos => Set<UplArquivo>();
        public DbSet<UplFilaUrlRemota> UplFilasUrlRemota => Set<UplFilaUrlRemota>();
        public DbSet<UplExecucaoImportacao> UplExecucoesImportacao => Set<UplExecucaoImportacao>();
        public DbSet<UplImportacaoLinha> UplImportacaoLinhas => Set<UplImportacaoLinha>();
        public DbSet<UplImportacaoErro> UplImportacaoErros => Set<UplImportacaoErro>();
        public DbSet<UplMapeamentoImportacao> UplMapeamentosImportacao => Set<UplMapeamentoImportacao>();
        public DbSet<UplImportacaoXml> UplImportacoesXml => Set<UplImportacaoXml>();
        public DbSet<UplArquivoXmlSaida> UplArquivosXmlSaida => Set<UplArquivoXmlSaida>();
        public DbSet<UplExecucaoExportacao> UplExecucoesExportacao => Set<UplExecucaoExportacao>();
        public DbSet<UplExportacaoCampo> UplExportacaoCampos => Set<UplExportacaoCampo>();
        public DbSet<UplAtualizacaoVersao> UplAtualizacoesVersao => Set<UplAtualizacaoVersao>();
        public DbSet<UplAtualizacaoBloco> UplAtualizacoesBloco => Set<UplAtualizacaoBloco>();
        public DbSet<UplAtualizacaoJob> UplAtualizacoesJob => Set<UplAtualizacaoJob>();
        public DbSet<UplMigracaoOffline> UplMigracoesOffline => Set<UplMigracaoOffline>();
        public DbSet<UplHistorico> UplHistoricos => Set<UplHistorico>();


        public ContextAplicativo(
            DbContextOptions<ContextAplicativo> options,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser) : base(options, tenantProvider, currentUser)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define o schema do banco do PostgreSQL para o macrodomínio do Aplicativo
            modelBuilder.HasDefaultSchema("aplicativo");

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UsuarioInterno>(entity =>
            {
                entity.HasKey(u => u.Id);
                
                // Email único (REG-013)
                entity.HasIndex(u => u.Email)
                      .IsUnique()
                      .HasDatabaseName("ix_landlord_users_email");
            });

            modelBuilder.Entity<SystemSetting>(entity =>
            {
                entity.HasKey(s => s.Id);

                // Chave + Escopo único
                entity.HasIndex(s => new { s.Chave, s.Escopo })
                      .IsUnique()
                      .HasDatabaseName("ix_system_settings_chave_escopo");
            });

            modelBuilder.Entity<ComunicacaoSuperAdmin>(entity =>
            {
                entity.HasKey(c => c.Id);

                // Configura a serialização do List<string> para string JSON no banco de dados (REG-020)
                entity.Property(c => c.BusinessIds)
                      .HasConversion(
                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                          v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null!) ?? new List<string>())
                      .HasColumnType("text");

                entity.Property(c => c.Canais)
                      .HasConversion(
                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                          v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null!) ?? new List<string>())
                      .HasColumnType("text");

                entity.HasOne<UsuarioInterno>()
                      .WithMany()
                      .HasForeignKey(c => c.EnviadoPor)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<LandingPageSettings>(entity =>
            {
                entity.HasKey(l => l.Id);
            });

            modelBuilder.Entity<MarketplaceSettings>(entity =>
            {
                entity.HasKey(m => m.Id);

                // Módulo único para upsert controlado
                entity.HasIndex(m => m.Modulo)
                      .IsUnique()
                      .HasDatabaseName("ix_marketplace_settings_module");
            });

            modelBuilder.Entity<CustomPage>(entity =>
            {
                entity.HasKey(p => p.Id);

                // Slug único (REG-024)
                entity.HasIndex(p => p.Slug)
                      .IsUnique()
                      .HasDatabaseName("ix_custom_pages_slug");
            });

            modelBuilder.Entity<NewsletterSubscriber>(entity =>
            {
                entity.HasKey(n => n.Id);

                // Email único na newsletter
                entity.HasIndex(n => n.Email)
                      .IsUnique()
                      .HasDatabaseName("ix_newsletter_subscribers_email");

                // Token único para descadastro LGPD
                entity.HasIndex(n => n.TokenDescadastro)
                      .IsUnique()
                      .HasDatabaseName("ix_newsletter_subscribers_token_descadastro");
            });

            modelBuilder.Entity<ExecucaoMassaGlobal>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne<UsuarioInterno>()
                      .WithMany()
                      .HasForeignKey(e => e.AprovadoPor)
                      .OnDelete(DeleteBehavior.Restrict);

                // Apenas UMA execução ativa por vez no sistema (REG-026)
                entity.HasIndex(e => e.Status)
                      .IsUnique()
                      .HasFilter("status = 'Active'")
                      .HasDatabaseName("ix_execute_queries_status_active_unique");
            });

            modelBuilder.Entity<LogExecucaoMassa>(entity =>
            {
                entity.HasKey(l => l.Id);

                entity.HasOne<ExecucaoMassaGlobal>()
                      .WithMany()
                      .HasForeignKey(l => l.ExecuteQueryId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Idempotência: Um tenant aprovado (Passed) não reprocessa na mesma execução (REG-027)
                entity.HasIndex(l => new { l.ExecuteQueryId, l.TargetTenantId })
                      .IsUnique()
                      .HasFilter("status = 'Passed'")
                      .HasDatabaseName("ix_execute_query_logs_idempotency_passed");
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Login).HasMaxLength(20);
                entity.HasIndex(u => u.Email).IsUnique().HasDatabaseName("ix_usuarios_email");
            });

            modelBuilder.Entity<UsuarioEmpresa>(entity =>
            {
                entity.HasKey(ue => ue.Id);
                entity.HasIndex(ue => new { ue.UsuarioId, ue.EmpresaId }).IsUnique().HasDatabaseName("ix_usuario_empresas_usuario_empresa");
                entity.Property(ue => ue.PerfilAcessoId).HasColumnName("perfil_usuario_id");
            });

            modelBuilder.Entity<AcessoUsuarioTenant>(entity =>
            {
                entity.HasKey(a => a.Id);
                // Membership N:N: um vínculo por (identidade global, tenant concedido).
                entity.HasIndex(a => new { a.UsuarioId, a.TenantId }).IsUnique().HasDatabaseName("ix_acessos_usuario_tenant_usuario_tenant");
            });

            modelBuilder.Entity<IdentidadeExterna>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.SubjectId).HasMaxLength(255);
                entity.Property(i => i.EmailProvedor).HasMaxLength(320);
                // Login social (1.04 PASS 3): o par (provedor, sub) identifica unicamente a conta social.
                entity.HasIndex(i => new { i.Provedor, i.SubjectId }).IsUnique().HasDatabaseName("ix_identidades_externas_provedor_subject");
                // Lookup por usuário (listar/gerir vínculos sociais de uma identidade).
                entity.HasIndex(i => i.UsuarioId).HasDatabaseName("ix_identidades_externas_usuario");
            });

            modelBuilder.Entity<SessaoUsuario>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.HasIndex(s => s.TokenSessao).IsUnique().HasDatabaseName("ix_sessoes_token");
                entity.HasIndex(s => s.Jti).HasDatabaseName("ix_sessoes_jti");
            });

            modelBuilder.Entity<PersonalAccessToken>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.TokenHash).IsUnique().HasDatabaseName("ix_tokens_hash");
            });

            modelBuilder.Entity<HistoricoLogin>(entity =>
            {
                entity.HasKey(h => h.Id);
                entity.HasIndex(h => h.EmailTentado).HasDatabaseName("ix_historico_login_email");
            });

            modelBuilder.Entity<BannedIp>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.HasIndex(b => b.IpAddress).IsUnique().HasDatabaseName("ix_banned_ips_ip");
            });

            modelBuilder.Entity<ConfiguracaoEmpresa>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasIndex(c => c.EmpresaId).IsUnique().HasDatabaseName("ix_configuracoes_empresas_empresa");
            });

            modelBuilder.Entity<Idioma>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.HasIndex(i => new { i.TenantId, i.Code }).IsUnique().HasDatabaseName("ix_idiomas_tenant_code");
            });

            modelBuilder.Entity<AnoFinanceiro>(entity =>
            {
                entity.HasKey(a => a.Id);
            });

            modelBuilder.Entity<SessaoImpersonacao>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.HasIndex(s => s.UsuarioOriginalId).HasDatabaseName("ix_sessoes_impersonacao_original");
                entity.HasIndex(s => s.UsuarioAlvoId).HasDatabaseName("ix_sessoes_impersonacao_alvo");
            });

            modelBuilder.Entity<PreferenciaUsuario>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.UsuarioId).IsUnique().HasDatabaseName("ix_preferencias_usuarios_usuario");
            });

            modelBuilder.Entity<Epros.Shared.Domain.Events.OutboxMessage>(entity =>
            {
                entity.ToTable("outbox_messages", "aplicativo");
                entity.HasKey(o => o.Id);
            });

            modelBuilder.Entity<InstalacaoState>(entity =>
            {
                entity.ToTable("installation_state", "aplicativo");
                entity.HasKey(i => i.Id);
            });

            modelBuilder.Entity<UpdateLog>(entity =>
            {
                entity.ToTable("update_logs", "aplicativo");
                entity.HasKey(u => u.Id);
            });

            // RBAC/Menu DEPRECADO no Aplicativo (P0.8): Menu/MenuItemNivel1/2/PerfilUsuario/PerfilUsuarioAcesso
            // foram removidos deste Context. Dono único = GestaoClientes (PerfilAcesso + PerfilAcessoMenu + Menu,
            // schema plataforma). O auth resolve permissões/menu via ContextGestaoClientes. Ver CONVENCAO_CODIGO.md §1.2.

            // ===== Governança de upgrade/versão (Super Admin — APP-TEN-010) =====
            modelBuilder.Entity<SolicitacaoUpgradeVersao>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.VersaoAtual).HasMaxLength(50);
                entity.Property(s => s.VersaoAlvo).HasMaxLength(50);
                entity.Property(s => s.Motivo).HasMaxLength(1000);
                entity.Property(s => s.SolicitadoPor).HasMaxLength(100);
                entity.Property(s => s.AprovadoPor).HasMaxLength(100);
                entity.Property(s => s.Comentario).HasMaxLength(1000);
                // Impede dois upgrades simultâneos em execução (bloqueio de reexecução — REG-019).
                entity.HasIndex(s => s.Status)
                      .IsUnique()
                      .HasFilter("status = 3")
                      .HasDatabaseName("ix_upgrade_versao_em_execucao_unico");
            });

            // ===== Motor de Workflow genérico (PLT-WF) =====
            modelBuilder.Entity<WfDefinicao>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Modulo).HasMaxLength(100);
                entity.Property(d => d.Entidade).HasMaxLength(150);
                entity.Property(d => d.Nome).HasMaxLength(200);
                entity.HasIndex(d => new { d.TenantId, d.Modulo, d.Entidade }).HasDatabaseName("ix_wf_definicoes_tenant_modulo_entidade");
            });

            modelBuilder.Entity<WfEstado>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Codigo).HasMaxLength(50);
                entity.Property(e => e.Nome).HasMaxLength(150);
                entity.HasIndex(e => new { e.DefinicaoId, e.Codigo }).IsUnique().HasDatabaseName("ix_wf_estados_definicao_codigo");
            });

            modelBuilder.Entity<WfTransicao>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.HasIndex(t => new { t.DefinicaoId, t.Evento }).HasDatabaseName("ix_wf_transicoes_definicao_evento");
            });

            modelBuilder.Entity<WfInstancia>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Modulo).HasMaxLength(100);
                entity.Property(i => i.EntidadeTipo).HasMaxLength(150);
                entity.Property(i => i.EntidadeIdReferencia).HasMaxLength(100);
                entity.Property(i => i.Descricao).HasMaxLength(500);
                entity.Property(i => i.CommandType).HasMaxLength(500);
                entity.Property(i => i.AprovadoPor).HasMaxLength(100);
                entity.Property(i => i.Comentario).HasMaxLength(1000);
                entity.HasIndex(i => new { i.TenantId, i.EntidadeTipo, i.EntidadeIdReferencia }).HasDatabaseName("ix_wf_instancias_entidade");
                entity.HasIndex(i => i.Status).HasDatabaseName("ix_wf_instancias_status");
            });

            modelBuilder.Entity<WfTarefa>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Titulo).HasMaxLength(300);
                entity.HasIndex(t => t.InstanciaId).HasDatabaseName("ix_wf_tarefas_instancia");
                entity.HasIndex(t => t.Status).HasDatabaseName("ix_wf_tarefas_status");
            });

            modelBuilder.Entity<WfSolicitacao>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Reason).HasMaxLength(1000);
                entity.Property(s => s.Attachment).HasMaxLength(500);
                entity.Property(s => s.ApproverComment).HasMaxLength(1000);
                entity.Property(s => s.EmployeeId).HasMaxLength(100);
                entity.Property(s => s.LeaveTypeId).HasMaxLength(100);
                entity.Property(s => s.ApprovedBy).HasMaxLength(100);
                entity.Property(s => s.CreatorId).HasMaxLength(100);
                entity.HasIndex(s => s.Status).HasDatabaseName("ix_wf_solicitacoes_status");
            });

            modelBuilder.Entity<WfHistorico>(entity =>
            {
                entity.HasKey(h => h.Id);
                entity.Property(h => h.EntidadeTipo).HasMaxLength(150);
                entity.Property(h => h.EntidadeIdReferencia).HasMaxLength(100);
                entity.Property(h => h.Acao).HasMaxLength(100);
                entity.Property(h => h.EstadoAnterior).HasMaxLength(50);
                entity.Property(h => h.EstadoNovo).HasMaxLength(50);
                entity.Property(h => h.IpOrigem).HasMaxLength(64);
                entity.HasIndex(h => h.InstanciaId).HasDatabaseName("ix_wf_historicos_instancia");
                entity.HasIndex(h => new { h.EntidadeTipo, h.EntidadeIdReferencia }).HasDatabaseName("ix_wf_historicos_entidade");
            });

            modelBuilder.Entity<WfAnexo>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.EntidadeTipo).HasMaxLength(50);
                entity.Property(a => a.EntidadeIdReferencia).HasMaxLength(100);
                entity.Property(a => a.ArquivoId).HasMaxLength(100);
            });

            modelBuilder.Entity<WfEventoDominio>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EntidadeTipo).HasMaxLength(150);
                entity.Property(e => e.EntidadeIdReferencia).HasMaxLength(100);
                entity.Property(e => e.Chave).HasMaxLength(150);
            });

            modelBuilder.Entity<WfParametro>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Chave).HasMaxLength(150);
                entity.HasIndex(p => new { p.TenantId, p.Chave }).IsUnique().HasDatabaseName("ix_wf_parametros_tenant_chave");
            });

            modelBuilder.Entity<WfAgendamento>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Nome).HasMaxLength(200);
                entity.Property(a => a.ExpressaoIntervalar).HasMaxLength(100);
                entity.HasIndex(a => a.Ativo).HasDatabaseName("ix_wf_agendamentos_ativo");
            });

            modelBuilder.Entity<WfJob>(entity =>
            {
                entity.HasKey(j => j.Id);
                entity.Property(j => j.Nome).HasMaxLength(200);
                entity.HasIndex(j => new { j.AgendamentoId, j.Status }).HasDatabaseName("ix_wf_jobs_agenda_status");
            });

            modelBuilder.Entity<WfJobTentativa>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Mensagem).HasMaxLength(2000);
                entity.HasIndex(t => t.JobId).HasDatabaseName("ix_wf_job_tentativas_job");
            });

            // ===== Upload e Migração de Dados genéricos (PLT-UPL) =====
            modelBuilder.Entity<UplConfiguracao>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Chave).HasMaxLength(150);
                entity.HasIndex(c => new { c.TenantId, c.Chave }).IsUnique().HasDatabaseName("ix_upl_configuracoes_tenant_chave");
            });

            modelBuilder.Entity<UplExecucaoUpload>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NomeOriginal).HasMaxLength(400);
                entity.Property(e => e.Extensao).HasMaxLength(20);
                entity.Property(e => e.MimeType).HasMaxLength(150);
                entity.Property(e => e.MensagemErro).HasMaxLength(2000);
                entity.Property(e => e.PastaDestinoId).HasMaxLength(200);
                entity.HasIndex(e => e.Status).HasDatabaseName("ix_upl_execucoes_upload_status");
            });

            modelBuilder.Entity<UplUploadParte>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.CaminhoTemporario).HasMaxLength(1000);
                entity.HasIndex(p => p.ExecucaoUploadId).HasDatabaseName("ix_upl_upload_partes_execucao");
                entity.HasIndex(p => p.CriadoEm).HasDatabaseName("ix_upl_upload_partes_criado_em");
            });

            modelBuilder.Entity<UplArquivo>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.NomeOriginal).HasMaxLength(400);
                entity.Property(a => a.NomeArmazenado).HasMaxLength(200);
                entity.Property(a => a.Extensao).HasMaxLength(20);
                entity.Property(a => a.HashArquivo).HasMaxLength(128);
                entity.Property(a => a.PastaId).HasMaxLength(200);
                entity.Property(a => a.ServidorStorageId).HasMaxLength(100);
                entity.HasIndex(a => new { a.HashArquivo, a.TamanhoBytes }).HasDatabaseName("ix_upl_arquivos_hash_tamanho");
                entity.HasIndex(a => a.NomeArmazenado).IsUnique().HasDatabaseName("ix_upl_arquivos_nome_armazenado");
            });

            modelBuilder.Entity<UplFilaUrlRemota>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.Property(f => f.Url).HasMaxLength(2000);
                entity.Property(f => f.ServidorProcessamentoId).HasMaxLength(100);
                entity.Property(f => f.PastaDestinoId).HasMaxLength(200);
                entity.Property(f => f.MensagemErro).HasMaxLength(2000);
                entity.HasIndex(f => f.StatusJob).HasDatabaseName("ix_upl_fila_url_remota_status");
            });

            modelBuilder.Entity<UplExecucaoImportacao>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ImportRef).HasMaxLength(64);
                entity.Property(e => e.TipoImportacao).HasMaxLength(50);
                entity.Property(e => e.ArquivoTemporarioChave).HasMaxLength(200);
                entity.Property(e => e.ArquivoTemporarioNome).HasMaxLength(400);
                entity.Property(e => e.ReferenciaErro).HasMaxLength(64);
                entity.HasIndex(e => e.ImportRef).IsUnique().HasDatabaseName("ix_upl_execucoes_importacao_import_ref");
                entity.HasIndex(e => e.Status).HasDatabaseName("ix_upl_execucoes_importacao_status");
            });

            modelBuilder.Entity<UplImportacaoLinha>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.Property(l => l.EntidadeDestino).HasMaxLength(150);
                entity.Property(l => l.EntidadeDestinoId).HasMaxLength(100);
                entity.HasIndex(l => l.ExecucaoImportacaoId).HasDatabaseName("ix_upl_importacao_linhas_execucao");
                entity.HasIndex(l => l.Status).HasDatabaseName("ix_upl_importacao_linhas_status");
            });

            modelBuilder.Entity<UplImportacaoErro>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ReferenciaErro).HasMaxLength(64);
                entity.Property(e => e.Atributo).HasMaxLength(150);
                entity.Property(e => e.Mensagem).HasMaxLength(2000);
                entity.Property(e => e.FormatoExibicao).HasMaxLength(20);
                entity.HasIndex(e => e.ReferenciaErro).HasDatabaseName("ix_upl_importacao_erros_referencia");
            });

            modelBuilder.Entity<UplMapeamentoImportacao>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.TipoImportacao).HasMaxLength(50);
                entity.Property(m => m.Nome).HasMaxLength(200);
                entity.HasIndex(m => new { m.UsuarioId, m.TipoImportacao }).HasDatabaseName("ix_upl_mapeamentos_usuario_tipo");
            });

            modelBuilder.Entity<UplImportacaoXml>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.NfeId).HasMaxLength(100);
                entity.Property(x => x.TipoEvento).HasMaxLength(50);
                entity.Property(x => x.MensagemErroImportacaoXml).HasMaxLength(2000);
                entity.Property(x => x.MensagemErroCadastro).HasMaxLength(2000);
                entity.Property(x => x.MensagemErroSalvarPdf).HasMaxLength(2000);
                entity.HasIndex(x => x.NfeId).HasDatabaseName("ix_upl_importacao_xml_nfe");
                entity.HasIndex(x => x.StatusImportacaoXml).HasDatabaseName("ix_upl_importacao_xml_status");
            });

            modelBuilder.Entity<UplArquivoXmlSaida>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.NomeArquivo).HasMaxLength(400);
                entity.Property(a => a.MensagemErro).HasMaxLength(2000);
                entity.HasIndex(a => a.Status).HasDatabaseName("ix_upl_arquivo_xml_saida_status");
            });

            modelBuilder.Entity<UplExecucaoExportacao>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Entidade).HasMaxLength(100);
                entity.Property(e => e.UrlDownload).HasMaxLength(1000);
                entity.Property(e => e.MensagemErro).HasMaxLength(2000);
                entity.HasIndex(e => e.Status).HasDatabaseName("ix_upl_execucoes_exportacao_status");
            });

            modelBuilder.Entity<UplExportacaoCampo>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.ChaveCampo).HasMaxLength(150);
                entity.Property(c => c.Rotulo).HasMaxLength(200);
                entity.HasIndex(c => c.ExecucaoExportacaoId).HasDatabaseName("ix_upl_exportacao_campos_execucao");
            });

            modelBuilder.Entity<UplAtualizacaoVersao>(entity =>
            {
                entity.HasKey(v => v.Id);
                entity.Property(v => v.VersaoAtual).HasMaxLength(50);
                entity.Property(v => v.VersaoAlvo).HasMaxLength(50);
                entity.HasIndex(v => v.Status).HasDatabaseName("ix_upl_atualizacao_versao_status");
            });

            modelBuilder.Entity<UplAtualizacaoBloco>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.NomeArquivo).HasMaxLength(400);
                entity.Property(b => b.IdentificadorBloco).HasMaxLength(200);
                // Idempotência por escopo: um bloco/arquivo não é reaplicado (EF UPLOAD 7.12.5).
                entity.HasIndex(b => new { b.TenantId, b.NomeArquivo }).IsUnique().HasDatabaseName("ix_upl_atualizacao_bloco_tenant_arquivo");
            });

            modelBuilder.Entity<UplAtualizacaoJob>(entity =>
            {
                entity.HasKey(j => j.Id);
                entity.Property(j => j.Tipo).HasMaxLength(30);
                entity.Property(j => j.Nome).HasMaxLength(200);
                entity.Property(j => j.FuncaoNome).HasMaxLength(200);
                entity.HasIndex(j => j.Status).HasDatabaseName("ix_upl_atualizacao_job_status");
            });

            modelBuilder.Entity<UplMigracaoOffline>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.ContaDestino).HasMaxLength(200);
                entity.Property(m => m.CaminhoOrigem).HasMaxLength(1000);
                entity.Property(m => m.PastaInicialDestino).HasMaxLength(1000);
                entity.Property(m => m.Modo).HasMaxLength(20);
                entity.Property(m => m.MensagemErro).HasMaxLength(2000);
                entity.HasIndex(m => m.Status).HasDatabaseName("ix_upl_migracao_offline_status");
            });

            modelBuilder.Entity<UplHistorico>(entity =>
            {
                entity.HasKey(h => h.Id);
                entity.Property(h => h.Entidade).HasMaxLength(150);
                entity.Property(h => h.EntidadeIdReferencia).HasMaxLength(100);
                entity.Property(h => h.Acao).HasMaxLength(100);
                entity.Property(h => h.IpOrigem).HasMaxLength(64);
                entity.HasIndex(h => new { h.Entidade, h.EntidadeIdReferencia }).HasDatabaseName("ix_upl_historicos_entidade");
            });
        }
    }
}
