using Epros.Infrastructure.Data;
using Epros.Modules.ESG.Domain.Entities;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.ESG.Infrastructure.Data
{
    public class ContextESG : ContextBase
    {
        public DbSet<EmissaoCarbono> EmissoesCarbono => Set<EmissaoCarbono>();
        public DbSet<RelatorioESG> RelatoriosESG => Set<RelatorioESG>();

        public ContextESG(
            DbContextOptions<ContextESG> options,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser) : base(options, tenantProvider, currentUser)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("esg");

            modelBuilder.Entity<EmissaoCarbono>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("emissoes_carbono");
                entity.HasIndex(e => new { e.TenantId, e.DataTransacao });
            });

            modelBuilder.Entity<RelatorioESG>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.ToTable("relatorios_esg");
                entity.HasIndex(r => new { r.TenantId, r.AnoFiscal }).IsUnique();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
