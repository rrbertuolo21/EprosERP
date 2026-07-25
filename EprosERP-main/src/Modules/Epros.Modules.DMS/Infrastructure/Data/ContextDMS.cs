using Epros.Infrastructure.Data;
using Epros.Modules.DMS.Domain.Entities;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.DMS.Infrastructure.Data
{
    public class ContextDMS : ContextBase
    {
        public DbSet<VendaVeiculo> VendasVeiculos => Set<VendaVeiculo>();
        public DbSet<OrdemServicoDms> OrdensServicoDms => Set<OrdemServicoDms>();
        public DbSet<GarantiaMontadora> GarantiasMontadora => Set<GarantiaMontadora>();

        public ContextDMS(
            DbContextOptions<ContextDMS> options,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser) : base(options, tenantProvider, currentUser)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("concessionarias");

            modelBuilder.Entity<VendaVeiculo>(entity =>
            {
                entity.HasKey(v => v.Id);
                entity.ToTable("vendas_veiculos");
                entity.HasIndex(v => new { v.TenantId, v.Chassi }).IsUnique();
            });

            modelBuilder.Entity<OrdemServicoDms>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.ToTable("ordens_servico_dms");
                entity.HasIndex(o => new { o.TenantId, o.NumeroOs }).IsUnique();
            });

            modelBuilder.Entity<GarantiaMontadora>(entity =>
            {
                entity.HasKey(g => g.Id);
                entity.ToTable("garantias_montadora");
                entity.HasIndex(g => new { g.TenantId, g.OrdemServicoDmsId });
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
