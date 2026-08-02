using Epros.Modules.Aplicativo.Domain.Entities.Plataforma.Assinatura;
using Epros.Modules.Aplicativo.Domain.Entities.Plataforma.Ged;
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

        partial void ConfigurarPlataforma(ModelBuilder modelBuilder)
        {
            ConfigurarGed(modelBuilder);
            ConfigurarAssinatura(modelBuilder);
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
    }
}
