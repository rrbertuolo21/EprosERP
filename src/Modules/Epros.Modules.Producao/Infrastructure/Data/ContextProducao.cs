using Epros.Infrastructure.Data;
using Epros.Modules.Producao.Domain.Entities;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Producao.Infrastructure.Data
{
    public class ContextProducao : ContextBase
    {
        public DbSet<ListaMateriais> ListasMateriais => Set<ListaMateriais>();
        public DbSet<BomItem> BomItens => Set<BomItem>();
        public DbSet<OrdemProducao> OrdensProducao => Set<OrdemProducao>();
        public DbSet<ApontamentoProducao> Apontamentos => Set<ApontamentoProducao>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        public ContextProducao(
            DbContextOptions<ContextProducao> options,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser) : base(options, tenantProvider, currentUser)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("producao");

            modelBuilder.Entity<ListaMateriais>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.ToTable("listas_materiais");
                entity.HasIndex(l => new { l.TenantId, l.ProdutoAcabadoSku });
                
                entity.HasMany(l => l.Itens)
                    .WithOne()
                    .HasForeignKey(i => i.ListaMateriaisId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<BomItem>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.ToTable("bom_itens");
                entity.HasIndex(i => new { i.TenantId, i.InsumoSku });
            });

            modelBuilder.Entity<OrdemProducao>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.ToTable("ordens_producao");
                entity.HasIndex(o => new { o.TenantId, o.Codigo }).IsUnique();
                entity.HasIndex(o => new { o.TenantId, o.ProdutoAcabadoSku });

                entity.HasMany(o => o.Apontamentos)
                    .WithOne()
                    .HasForeignKey(a => a.OrdemProducaoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ApontamentoProducao>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.ToTable("apontamentos");
                entity.HasIndex(a => new { a.TenantId, a.OrdemProducaoId });
            });

            modelBuilder.Entity<OutboxMessage>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.ToTable("outbox_messages", "producao");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
