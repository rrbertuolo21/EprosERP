using Epros.Infrastructure.Data;
using Epros.Modules.Manutencao.Domain.Entities;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Manutencao.Infrastructure.Data
{
    public class ContextManutencao : ContextBase
    {
        public DbSet<Equipamento> Equipamentos => Set<Equipamento>();
        public DbSet<OrdemManutencao> OrdensManutencao => Set<OrdemManutencao>();
        public DbSet<OrdemManutencaoPeca> OrdemManutencaoPecas => Set<OrdemManutencaoPeca>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        public ContextManutencao(
            DbContextOptions<ContextManutencao> options,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser) : base(options, tenantProvider, currentUser)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("manutencao");

            modelBuilder.Entity<Equipamento>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("equipamentos");
                entity.HasIndex(e => new { e.TenantId, e.Codigo }).IsUnique();
            });

            modelBuilder.Entity<OrdemManutencao>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.ToTable("ordens_manutencao");
                entity.HasIndex(o => new { o.TenantId, o.EquipamentoId });

                entity.HasMany(o => o.Pecas)
                    .WithOne()
                    .HasForeignKey(p => p.OrdemManutencaoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OrdemManutencaoPeca>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.ToTable("ordens_manutencao_pecas");
            });

            modelBuilder.Entity<OutboxMessage>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.ToTable("outbox_messages", "manutencao");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
