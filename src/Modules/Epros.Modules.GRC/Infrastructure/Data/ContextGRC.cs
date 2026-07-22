using Epros.Infrastructure.Data;
using Epros.Modules.GRC.Domain.Entities;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GRC.Infrastructure.Data
{
    public class ContextGRC : ContextBase
    {
        public DbSet<RiscoCorporativo> RiscosCorporativos => Set<RiscoCorporativo>();
        public DbSet<ControleInterno> ControlesInternos => Set<ControleInterno>();
        public DbSet<IncidenteCompliance> IncidentesCompliance => Set<IncidenteCompliance>();
        public DbSet<Denuncia> Denuncias => Set<Denuncia>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        public ContextGRC(
            DbContextOptions<ContextGRC> options,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser) : base(options, tenantProvider, currentUser)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("grc");

            modelBuilder.Entity<RiscoCorporativo>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.ToTable("riscos_corporativos");
            });

            modelBuilder.Entity<ControleInterno>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.ToTable("controles_internos");
                entity.HasIndex(c => new { c.TenantId, c.Codigo }).IsUnique();
            });

            modelBuilder.Entity<IncidenteCompliance>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.ToTable("incidentes_compliance");
            });

            modelBuilder.Entity<Denuncia>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.ToTable("denuncias");
                entity.HasIndex(d => new { d.TenantId, d.CodigoAcompanhamento }).IsUnique();
            });

            modelBuilder.Entity<OutboxMessage>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.ToTable("outbox_messages", "grc");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
