using Epros.Infrastructure.Data;
using Epros.Modules.RH.Domain.Entities;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.RH.Infrastructure.Data
{
    public partial class ContextRH : ContextBase
    {
        public DbSet<Colaborador> Colaboradores => Set<Colaborador>();
        public DbSet<Timesheet> Timesheets => Set<Timesheet>();
        public DbSet<FolhaPagamento> FolhasPagamento => Set<FolhaPagamento>();
        public DbSet<FolhaPagamentoVerba> FolhaPagamentoVerbas => Set<FolhaPagamentoVerba>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        public ContextRH(
            DbContextOptions<ContextRH> options,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser) : base(options, tenantProvider, currentUser)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("rh");

            modelBuilder.Entity<Colaborador>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.ToTable("colaboradores");
                entity.HasIndex(c => new { c.TenantId, c.Cpf }).IsUnique();
            });

            modelBuilder.Entity<Timesheet>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.ToTable("timesheets");
                entity.HasIndex(t => new { t.TenantId, t.ColaboradorId, t.Data });
            });

            modelBuilder.Entity<FolhaPagamento>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.ToTable("folhas_pagamento");
                entity.HasIndex(f => new { f.TenantId, f.ColaboradorId, f.MesCompetencia, f.AnoCompetencia }).IsUnique();

                entity.HasMany(f => f.Verbas)
                    .WithOne()
                    .HasForeignKey(v => v.FolhaPagamentoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<FolhaPagamentoVerba>(entity =>
            {
                entity.HasKey(v => v.Id);
                entity.ToTable("folhas_pagamento_verbas");
            });

            modelBuilder.Entity<OutboxMessage>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.ToTable("outbox_messages", "rh");
            });

            ConfigurarFrente12(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        partial void ConfigurarFrente12(ModelBuilder modelBuilder);
    }
}
