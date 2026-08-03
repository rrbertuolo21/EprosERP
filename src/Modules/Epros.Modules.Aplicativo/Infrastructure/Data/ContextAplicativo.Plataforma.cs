using Epros.Modules.Aplicativo.Domain.Entities.Plataforma.Analytics;
using Epros.Modules.Aplicativo.Domain.Entities.Plataforma.Assinatura;
using Epros.Modules.Aplicativo.Domain.Entities.Plataforma.Conectores;
using Epros.Modules.Aplicativo.Domain.Entities.Plataforma.Ged;
using Epros.Modules.Aplicativo.Domain.Entities.Plataforma.Iot;
using Epros.Modules.Aplicativo.Domain.Entities.Plataforma.Sdk;
using Epros.Modules.Aplicativo.Domain.Entities.Plataforma.Wizards;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Infrastructure.Data
{
    /// <summary>
    /// PLT — PLATAFORMA COMPARTILHADA. Configuração EF Core dos submódulos spec-only construídos
    /// sobre as transversais (GED/T10, Assinatura/T10, cofre/T5, eventos/T2). Parcial de
    /// <see cref="ContextAplicativo"/>; o método <c>ConfigurarPlataforma</c> é chamado ao final de
    /// OnModelCreating. Schema "aplicativo" (padrão do módulo). Cada submódulo tem sua seção.
    /// </summary>
    public partial class ContextAplicativo
    {
        // ===== GED canônico (submódulo 1) =====
        public DbSet<VinculoDocumentoGed> VinculosDocumentoGed => Set<VinculoDocumentoGed>();
        public DbSet<PoliticaRetencaoGed> PoliticasRetencaoGed => Set<PoliticaRetencaoGed>();
        public DbSet<HashBloqueadoGed> HashesBloqueadosGed => Set<HashBloqueadoGed>();

        // ===== Assinatura eletrônica ICP (submódulo 2) =====
        public DbSet<SolicitacaoAssinatura> SolicitacoesAssinatura => Set<SolicitacaoAssinatura>();
        public DbSet<SignatarioAssinatura> SignatariosAssinatura => Set<SignatarioAssinatura>();
        public DbSet<RegistroAssinatura> RegistrosAssinatura => Set<RegistroAssinatura>();
        public DbSet<EvidenciaAssinatura> EvidenciasAssinatura => Set<EvidenciaAssinatura>();
        public DbSet<PoliticaAssinatura> PoliticasAssinatura => Set<PoliticaAssinatura>();
        public DbSet<HistoricoAssinatura> HistoricosAssinatura => Set<HistoricoAssinatura>();

        // ===== Analytics (submódulo 3) =====
        public DbSet<DefinicaoKpi> DefinicoesKpi => Set<DefinicaoKpi>();
        public DbSet<Dashboard> Dashboards => Set<Dashboard>();
        public DbSet<DashboardWidget> DashboardWidgets => Set<DashboardWidget>();
        public DbSet<SnapshotMetrica> SnapshotsMetrica => Set<SnapshotMetrica>();
        public DbSet<ExportacaoAnalytics> ExportacoesAnalytics => Set<ExportacaoAnalytics>();

        // ===== Conectores/Webhooks (submódulo 4) =====
        public DbSet<EndpointWebhook> EndpointsWebhook => Set<EndpointWebhook>();
        public DbSet<EntregaWebhook> EntregasWebhook => Set<EntregaWebhook>();

        // ===== Wizards (submódulo 5) =====
        public DbSet<DefinicaoWizard> DefinicoesWizard => Set<DefinicaoWizard>();
        public DbSet<EtapaWizard> EtapasWizard => Set<EtapaWizard>();
        public DbSet<CampoWizard> CamposWizard => Set<CampoWizard>();
        public DbSet<ExecucaoWizard> ExecucoesWizard => Set<ExecucaoWizard>();

        // ===== IoT (submódulo 6) =====
        public DbSet<DispositivoIot> DispositivosIot => Set<DispositivoIot>();
        public DbSet<SensorIot> SensoresIot => Set<SensorIot>();
        public DbSet<LeituraSensor> LeiturasSensor => Set<LeituraSensor>();

        // ===== SDK/Extensões (submódulo 7) =====
        public DbSet<ChaveApiPublica> ChavesApi => Set<ChaveApiPublica>();

        partial void ConfigurarPlataforma(ModelBuilder modelBuilder)
        {
            ConfigurarGed(modelBuilder);
            ConfigurarAssinatura(modelBuilder);
            ConfigurarAnalytics(modelBuilder);
            ConfigurarConectores(modelBuilder);
            ConfigurarWizards(modelBuilder);
            ConfigurarIot(modelBuilder);
            ConfigurarSdk(modelBuilder);
        }

        private static void ConfigurarGed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VinculoDocumentoGed>(e =>
            {
                e.ToTable("plt_ged_vinculos", "aplicativo");
                e.HasKey(x => x.Id);
                e.Property(x => x.ModuloDestino).HasMaxLength(100);
                e.Property(x => x.EntidadeTipo).HasMaxLength(150);
                e.Property(x => x.EntidadeId).HasMaxLength(100);
                e.HasIndex(x => new { x.DocumentoId, x.EntidadeTipo, x.EntidadeId })
                    .IsUnique().HasDatabaseName("ix_plt_ged_vinculos_documento_entidade");
                e.HasIndex(x => new { x.EntidadeTipo, x.EntidadeId }).HasDatabaseName("ix_plt_ged_vinculos_entidade");
            });

            modelBuilder.Entity<PoliticaRetencaoGed>(e =>
            {
                e.ToTable("plt_ged_politicas_retencao", "aplicativo");
                e.HasKey(x => x.Id);
                e.Property(x => x.TipoDocumento).HasMaxLength(100);
                e.Property(x => x.BaseLegal).HasMaxLength(500);
                e.Property(x => x.AcaoAposPrazo).HasMaxLength(30);
                e.HasIndex(x => new { x.TenantId, x.TipoDocumento })
                    .IsUnique().HasDatabaseName("ix_plt_ged_politicas_retencao_tenant_tipo");
            });

            modelBuilder.Entity<HashBloqueadoGed>(e =>
            {
                e.ToTable("plt_ged_hashes_bloqueados", "aplicativo");
                e.HasKey(x => x.Id);
                e.Property(x => x.Hash).HasMaxLength(128);
                e.Property(x => x.Motivo).HasMaxLength(500);
                e.HasIndex(x => new { x.TenantId, x.Hash })
                    .IsUnique().HasDatabaseName("ix_plt_ged_hashes_bloqueados_tenant_hash");
            });
        }

        private static void ConfigurarAssinatura(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SolicitacaoAssinatura>(e =>
            {
                e.ToTable("plt_assinatura_solicitacoes", "aplicativo");
                e.HasKey(x => x.Id);
                e.Property(x => x.Estado).HasMaxLength(30);
                e.Property(x => x.TipoAssinatura).HasMaxLength(50);
                e.HasIndex(x => x.DocumentoId).HasDatabaseName("ix_plt_assinatura_solicitacoes_documento");
                e.HasIndex(x => x.Estado).HasDatabaseName("ix_plt_assinatura_solicitacoes_estado");
            });

            modelBuilder.Entity<SignatarioAssinatura>(e =>
            {
                e.ToTable("plt_assinatura_signatarios", "aplicativo");
                e.HasKey(x => x.Id);
                e.Property(x => x.Nome).HasMaxLength(200);
                e.Property(x => x.Identificacao).HasMaxLength(200);
                e.Property(x => x.LinkToken).HasMaxLength(64);
                e.Property(x => x.Status).HasMaxLength(20);
                e.HasIndex(x => x.SolicitacaoId).HasDatabaseName("ix_plt_assinatura_signatarios_solicitacao");
                e.HasIndex(x => x.LinkToken).IsUnique().HasFilter("link_token IS NOT NULL")
                    .HasDatabaseName("ix_plt_assinatura_signatarios_link_token");
            });

            modelBuilder.Entity<RegistroAssinatura>(e =>
            {
                e.ToTable("plt_assinatura_registros", "aplicativo");
                e.HasKey(x => x.Id);
                e.Property(x => x.CarimboTempo).HasMaxLength(64);
                e.HasIndex(x => x.SolicitacaoId).HasDatabaseName("ix_plt_assinatura_registros_solicitacao");
            });

            modelBuilder.Entity<EvidenciaAssinatura>(e =>
            {
                e.ToTable("plt_assinatura_evidencias", "aplicativo");
                e.HasKey(x => x.Id);
                e.Property(x => x.Hash).HasMaxLength(128);
                e.Property(x => x.CertificadoSerial).HasMaxLength(128);
                e.Property(x => x.CadeiaIcp).HasMaxLength(4000);
                e.HasIndex(x => x.RegistroId).HasDatabaseName("ix_plt_assinatura_evidencias_registro");
            });

            modelBuilder.Entity<PoliticaAssinatura>(e =>
            {
                e.ToTable("plt_assinatura_politicas", "aplicativo");
                e.HasKey(x => x.Id);
                e.Property(x => x.TipoDocumento).HasMaxLength(100);
                e.Property(x => x.TipoAssinatura).HasMaxLength(50);
                e.HasIndex(x => new { x.TenantId, x.TipoDocumento })
                    .IsUnique().HasDatabaseName("ix_plt_assinatura_politicas_tenant_tipo");
            });

            modelBuilder.Entity<HistoricoAssinatura>(e =>
            {
                e.ToTable("plt_assinatura_historicos", "aplicativo");
                e.HasKey(x => x.Id);
                e.Property(x => x.Acao).HasMaxLength(50);
                e.Property(x => x.Detalhe).HasMaxLength(1000);
                e.HasIndex(x => x.SolicitacaoId).HasDatabaseName("ix_plt_assinatura_historicos_solicitacao");
            });
        }

        private static void ConfigurarAnalytics(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DefinicaoKpi>(e =>
            {
                e.ToTable("plt_analytics_kpis", "aplicativo");
                e.HasKey(x => x.Id);
                e.Property(x => x.Codigo).HasMaxLength(100);
                e.Property(x => x.Nome).HasMaxLength(200);
                e.Property(x => x.Descricao).HasMaxLength(1000);
                e.Property(x => x.Unidade).HasMaxLength(20);
                e.Property(x => x.Categoria).HasMaxLength(100);
                e.Property(x => x.FonteModulo).HasMaxLength(100);
                e.Property(x => x.ChaveConsulta).HasMaxLength(200);
                e.HasIndex(x => new { x.TenantId, x.Codigo, x.Versao })
                    .IsUnique().HasDatabaseName("ix_plt_analytics_kpis_tenant_codigo_versao");
            });

            modelBuilder.Entity<Dashboard>(e =>
            {
                e.ToTable("plt_analytics_dashboards", "aplicativo");
                e.HasKey(x => x.Id);
                e.Property(x => x.Codigo).HasMaxLength(100);
                e.Property(x => x.Nome).HasMaxLength(200);
                e.Property(x => x.Descricao).HasMaxLength(1000);
                e.HasIndex(x => new { x.TenantId, x.Codigo })
                    .IsUnique().HasDatabaseName("ix_plt_analytics_dashboards_tenant_codigo");
            });

            modelBuilder.Entity<DashboardWidget>(e =>
            {
                e.ToTable("plt_analytics_widgets", "aplicativo");
                e.HasKey(x => x.Id);
                e.Property(x => x.KpiCodigo).HasMaxLength(100);
                e.Property(x => x.Titulo).HasMaxLength(200);
                e.Property(x => x.TipoVisualizacao).HasMaxLength(30);
                e.HasIndex(x => x.DashboardId).HasDatabaseName("ix_plt_analytics_widgets_dashboard");
            });

            modelBuilder.Entity<SnapshotMetrica>(e =>
            {
                e.ToTable("plt_analytics_snapshots", "aplicativo");
                e.HasKey(x => x.Id);
                e.Property(x => x.KpiCodigo).HasMaxLength(100);
                e.Property(x => x.Competencia).HasMaxLength(20);
                e.Property(x => x.Valor).HasPrecision(20, 4);
                e.Property(x => x.Dimensao).HasMaxLength(150);
                e.Property(x => x.Origem).HasMaxLength(100);
                e.HasIndex(x => new { x.KpiCodigo, x.Competencia, x.Dimensao })
                    .HasDatabaseName("ix_plt_analytics_snapshots_kpi_competencia_dimensao");
            });

            modelBuilder.Entity<ExportacaoAnalytics>(e =>
            {
                e.ToTable("plt_analytics_exportacoes", "aplicativo");
                e.HasKey(x => x.Id);
                e.Property(x => x.Recurso).HasMaxLength(150);
                e.Property(x => x.Formato).HasMaxLength(10);
                e.Property(x => x.Status).HasMaxLength(20);
                e.Property(x => x.UrlRef).HasMaxLength(1000);
                e.HasIndex(x => x.Status).HasDatabaseName("ix_plt_analytics_exportacoes_status");
            });
        }

        private static void ConfigurarConectores(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EndpointWebhook>(e =>
            {
                e.ToTable("plt_conector_endpoints", "aplicativo");
                e.HasKey(x => x.Id);
                e.Property(x => x.Nome).HasMaxLength(200);
                e.Property(x => x.Url).HasMaxLength(2000);
                e.Property(x => x.EventosInscritosJson).HasColumnType("text");
                e.Property(x => x.SegredoCifrado).HasMaxLength(4000);
                e.Property(x => x.HeadersJson).HasColumnType("text");
                e.HasIndex(x => x.Ativo).HasDatabaseName("ix_plt_conector_endpoints_ativo");
            });

            modelBuilder.Entity<EntregaWebhook>(e =>
            {
                e.ToTable("plt_conector_entregas", "aplicativo");
                e.HasKey(x => x.Id);
                e.Property(x => x.EventType).HasMaxLength(150);
                e.Property(x => x.Payload).HasColumnType("text");
                e.Property(x => x.AssinaturaHmac).HasMaxLength(128);
                e.Property(x => x.Status).HasMaxLength(20);
                e.Property(x => x.UltimoErro).HasMaxLength(2000);
                e.HasIndex(x => new { x.Status, x.ProximaTentativaEm })
                    .HasDatabaseName("ix_plt_conector_entregas_status_proxima");
                e.HasIndex(x => x.EndpointId).HasDatabaseName("ix_plt_conector_entregas_endpoint");
            });
        }

        private static void ConfigurarWizards(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DefinicaoWizard>(e =>
            {
                e.ToTable("plt_wizard_definicoes", "aplicativo");
                e.HasKey(x => x.Id);
                e.Property(x => x.Codigo).HasMaxLength(100);
                e.Property(x => x.Nome).HasMaxLength(200);
                e.Property(x => x.Descricao).HasMaxLength(1000);
                e.HasIndex(x => new { x.TenantId, x.Codigo })
                    .IsUnique().HasDatabaseName("ix_plt_wizard_definicoes_tenant_codigo");
            });

            modelBuilder.Entity<EtapaWizard>(e =>
            {
                e.ToTable("plt_wizard_etapas", "aplicativo");
                e.HasKey(x => x.Id);
                e.Property(x => x.Titulo).HasMaxLength(200);
                e.Property(x => x.Descricao).HasMaxLength(1000);
                e.HasIndex(x => new { x.DefinicaoId, x.Ordem })
                    .IsUnique().HasDatabaseName("ix_plt_wizard_etapas_definicao_ordem");
            });

            modelBuilder.Entity<CampoWizard>(e =>
            {
                e.ToTable("plt_wizard_campos", "aplicativo");
                e.HasKey(x => x.Id);
                e.Property(x => x.Chave).HasMaxLength(100);
                e.Property(x => x.Rotulo).HasMaxLength(200);
                e.Property(x => x.Tipo).HasMaxLength(30);
                e.Property(x => x.OpcoesJson).HasColumnType("text");
                e.HasIndex(x => new { x.EtapaId, x.Chave })
                    .IsUnique().HasDatabaseName("ix_plt_wizard_campos_etapa_chave");
            });

            modelBuilder.Entity<ExecucaoWizard>(e =>
            {
                e.ToTable("plt_wizard_execucoes", "aplicativo");
                e.HasKey(x => x.Id);
                e.Property(x => x.Status).HasMaxLength(20);
                e.Property(x => x.RespostasJson).HasColumnType("text");
                e.Property(x => x.TokenPublico).HasMaxLength(64);
                e.HasIndex(x => x.DefinicaoId).HasDatabaseName("ix_plt_wizard_execucoes_definicao");
                e.HasIndex(x => x.TokenPublico).IsUnique().HasFilter("token_publico IS NOT NULL")
                    .HasDatabaseName("ix_plt_wizard_execucoes_token");
            });
        }

        private static void ConfigurarIot(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DispositivoIot>(e =>
            {
                e.ToTable("plt_iot_dispositivos", "aplicativo");
                e.HasKey(x => x.Id);
                e.Property(x => x.Codigo).HasMaxLength(100);
                e.Property(x => x.Nome).HasMaxLength(200);
                e.Property(x => x.Tipo).HasMaxLength(100);
                e.Property(x => x.Protocolo).HasMaxLength(20);
                e.Property(x => x.AtivoVinculadoTipo).HasMaxLength(150);
                e.Property(x => x.AtivoVinculadoId).HasMaxLength(100);
                e.HasIndex(x => new { x.TenantId, x.Codigo })
                    .IsUnique().HasDatabaseName("ix_plt_iot_dispositivos_tenant_codigo");
                e.HasIndex(x => new { x.AtivoVinculadoTipo, x.AtivoVinculadoId })
                    .HasDatabaseName("ix_plt_iot_dispositivos_ativo");
            });

            modelBuilder.Entity<SensorIot>(e =>
            {
                e.ToTable("plt_iot_sensores", "aplicativo");
                e.HasKey(x => x.Id);
                e.Property(x => x.Codigo).HasMaxLength(100);
                e.Property(x => x.Grandeza).HasMaxLength(100);
                e.Property(x => x.Unidade).HasMaxLength(20);
                e.Property(x => x.LimiteMin).HasPrecision(20, 6);
                e.Property(x => x.LimiteMax).HasPrecision(20, 6);
                e.HasIndex(x => new { x.DispositivoId, x.Codigo })
                    .IsUnique().HasDatabaseName("ix_plt_iot_sensores_dispositivo_codigo");
            });

            modelBuilder.Entity<LeituraSensor>(e =>
            {
                e.ToTable("plt_iot_leituras", "aplicativo");
                e.HasKey(x => x.Id);
                e.Property(x => x.Valor).HasPrecision(20, 6);
                e.HasIndex(x => new { x.SensorId, x.MedidoEm })
                    .HasDatabaseName("ix_plt_iot_leituras_sensor_medido");
                e.HasIndex(x => x.ForaFaixa).HasDatabaseName("ix_plt_iot_leituras_fora_faixa");
            });
        }

        private static void ConfigurarSdk(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChaveApiPublica>(e =>
            {
                e.ToTable("plt_sdk_chaves_api", "aplicativo");
                e.HasKey(x => x.Id);
                e.Property(x => x.Nome).HasMaxLength(200);
                e.Property(x => x.Prefixo).HasMaxLength(20);
                e.Property(x => x.HashChave).HasMaxLength(128);
                e.Property(x => x.EscoposJson).HasColumnType("text");
                e.HasIndex(x => x.HashChave).IsUnique().HasDatabaseName("ix_plt_sdk_chaves_api_hash");
                e.HasIndex(x => x.Prefixo).HasDatabaseName("ix_plt_sdk_chaves_api_prefixo");
            });
        }
    }
}
