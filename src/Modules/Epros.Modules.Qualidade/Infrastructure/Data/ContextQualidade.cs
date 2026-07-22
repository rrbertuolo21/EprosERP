using Epros.Infrastructure.Data;
using Epros.Modules.Qualidade.Domain.Entities;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Qualidade.Infrastructure.Data
{
    public class ContextQualidade : ContextBase
    {
        public DbSet<InspecaoLote> InspecoesLote => Set<InspecaoLote>();
        public DbSet<NaoConformidade> NaoConformidades => Set<NaoConformidade>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        public ContextQualidade(
            DbContextOptions<ContextQualidade> options,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser) : base(options, tenantProvider, currentUser)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("qualidade");

            modelBuilder.Entity<InspecaoLote>(entity =>
            {
                entity.HasKey(i => i.Id);
                // Nome da tabela snake_case
                entity.ToTable("inspecoes_lote");
                entity.HasIndex(i => new { i.TenantId, i.CompraId });
            });

            modelBuilder.Entity<NaoConformidade>(entity =>
            {
                entity.HasKey(nc => nc.Id);
                entity.ToTable("nao_conformidades");
                entity.HasIndex(nc => new { nc.TenantId, nc.InspecaoLoteId });
            });

            modelBuilder.Entity<OutboxMessage>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.ToTable("outbox_messages", "qualidade");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
