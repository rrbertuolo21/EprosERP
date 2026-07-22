using Epros.Infrastructure.Data;
using Epros.Modules.Projetos.Domain.Entities;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Projetos.Infrastructure.Data
{
    public class ContextProjetos : ContextBase
    {
        public DbSet<Projeto> Projetos => Set<Projeto>();
        public DbSet<WbsItem> ItensWbs => Set<WbsItem>();
        public DbSet<AlocacaoRecurso> Alocacoes => Set<AlocacaoRecurso>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        public ContextProjetos(
            DbContextOptions<ContextProjetos> options,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser) : base(options, tenantProvider, currentUser)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("projetos");

            modelBuilder.Entity<Projeto>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.ToTable("projetos");
                entity.HasIndex(p => new { p.TenantId, p.ClienteId });

                entity.HasMany(p => p.ItensWbs)
                    .WithOne()
                    .HasForeignKey(i => i.ProjetoId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.Alocacoes)
                    .WithOne()
                    .HasForeignKey(a => a.ProjetoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<WbsItem>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.ToTable("wbs_itens");
            });

            modelBuilder.Entity<AlocacaoRecurso>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.ToTable("alocacoes");
                entity.HasIndex(a => new { a.TenantId, a.ColaboradorId });
            });

            modelBuilder.Entity<OutboxMessage>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.ToTable("outbox_messages", "projetos");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
