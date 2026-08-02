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

        partial void ConfigurarPlataforma(ModelBuilder modelBuilder)
        {
            ConfigurarGed(modelBuilder);
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
    }
}
