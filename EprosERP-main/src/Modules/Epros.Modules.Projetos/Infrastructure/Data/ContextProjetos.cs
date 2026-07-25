using Epros.Infrastructure.Data;
using Epros.Modules.Projetos.Domain.Entities;
using Epros.Modules.Projetos.Domain.Entities.Definicao;
using Epros.Modules.Projetos.Domain.Entities.Orcamento;
using Epros.Modules.Projetos.Domain.Entities.Recursos;
using Epros.Modules.Projetos.Domain.Entities.Rastreamento;
using Epros.Modules.Projetos.Domain.Entities.Faturamento;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Projetos.Infrastructure.Data
{
    public class ContextProjetos : ContextBase
    {
        // PRJ-DEF (existente + enriquecimento)
        public DbSet<Projeto> Projetos => Set<Projeto>();
        public DbSet<WbsItem> ItensWbs => Set<WbsItem>();
        public DbSet<AlocacaoRecurso> Alocacoes => Set<AlocacaoRecurso>();
        public DbSet<ProjetoCliente> ProjetoClientes => Set<ProjetoCliente>();
        public DbSet<ProjetoMembro> ProjetoMembros => Set<ProjetoMembro>();
        public DbSet<ProjetoArquivo> ProjetoArquivos => Set<ProjetoArquivo>();
        public DbSet<ProjetoAtividade> ProjetoAtividades => Set<ProjetoAtividade>();
        public DbSet<ProjetoTarefaModelo> ProjetoTarefasModelo => Set<ProjetoTarefaModelo>();

        // PRJ-ORC
        public DbSet<OrcamentoProjeto> Orcamentos => Set<OrcamentoProjeto>();
        public DbSet<MarcoOrcamentario> MarcosOrcamentarios => Set<MarcoOrcamentario>();

        // PRJ-REC
        public DbSet<RecursoTimesheet> RecursoTimesheets => Set<RecursoTimesheet>();
        public DbSet<RecursoAlocacao> RecursoAlocacoes => Set<RecursoAlocacao>();

        // PRJ-RST
        public DbSet<EstagioTarefa> EstagiosTarefa => Set<EstagioTarefa>();
        public DbSet<TarefaProjeto> TarefasProjeto => Set<TarefaProjeto>();
        public DbSet<SubtarefaChecklist> SubtarefasChecklist => Set<SubtarefaChecklist>();
        public DbSet<DependenciaTarefa> DependenciasTarefa => Set<DependenciaTarefa>();
        public DbSet<TimerOperacional> TimersOperacionais => Set<TimerOperacional>();
        public DbSet<ReuniaoAcompanhamento> ReunioesAcompanhamento => Set<ReuniaoAcompanhamento>();

        // PRJ-FAT
        public DbSet<FaturamentoProjeto> Faturamentos => Set<FaturamentoProjeto>();
        public DbSet<ItemFaturamentoProjeto> ItensFaturamento => Set<ItemFaturamentoProjeto>();

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

            // ===================== PRJ-DEF =====================
            modelBuilder.Entity<Projeto>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.ToTable("projetos");
                entity.HasIndex(p => new { p.TenantId, p.ClienteId });
                entity.Property(p => p.Nome).HasMaxLength(255);
                entity.Property(p => p.Status).HasMaxLength(30);

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
                entity.Property(i => i.Nome).HasMaxLength(255);
                entity.Property(i => i.Status).HasMaxLength(30);
            });

            modelBuilder.Entity<AlocacaoRecurso>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.ToTable("alocacoes");
                entity.HasIndex(a => new { a.TenantId, a.ColaboradorId });
                entity.Property(a => a.Funcao).HasMaxLength(120);
            });

            modelBuilder.Entity<ProjetoCliente>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_def_projeto_cliente");
                entity.HasIndex(x => new { x.TenantId, x.ProjetoId, x.ClienteId });
            });

            modelBuilder.Entity<ProjetoMembro>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_def_projeto_membro");
                entity.Property(x => x.Papel).HasMaxLength(50);
                entity.HasIndex(x => new { x.TenantId, x.ProjetoId, x.UsuarioId });
            });

            modelBuilder.Entity<ProjetoArquivo>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_def_projeto_arquivo");
                entity.Property(x => x.NomeArquivo).HasMaxLength(255);
                entity.Property(x => x.CaminhoArquivo).HasMaxLength(1000);
                entity.HasIndex(x => new { x.TenantId, x.ProjetoId });
            });

            modelBuilder.Entity<ProjetoAtividade>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_def_projeto_atividade");
                entity.Property(x => x.TipoUsuario).HasMaxLength(50);
                entity.Property(x => x.TipoAtividade).HasMaxLength(100);
                entity.Property(x => x.Observacao).HasMaxLength(2000);
                entity.HasIndex(x => new { x.TenantId, x.ProjetoId, x.TipoAtividade });
            });

            modelBuilder.Entity<ProjetoTarefaModelo>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_def_tarefa_modelo");
                entity.Property(x => x.Nome).HasMaxLength(255);
                entity.Property(x => x.UnidadeDuracao).HasMaxLength(30);
                entity.HasIndex(x => new { x.TenantId, x.ProjetoId });
            });

            // ===================== PRJ-ORC =====================
            modelBuilder.Entity<OrcamentoProjeto>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_orcamento_projeto");
                entity.HasIndex(x => new { x.TenantId, x.ProjetoId });
                entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
                entity.Property(x => x.BillingType).HasConversion<string>().HasMaxLength(30);

                entity.HasMany(x => x.Marcos)
                    .WithOne()
                    .HasForeignKey(m => m.OrcamentoProjetoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MarcoOrcamentario>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_orcamento_marco");
                entity.Property(x => x.Titulo).HasMaxLength(255);
                entity.Property(x => x.Resumo).HasMaxLength(2000);
                entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
                entity.HasIndex(x => new { x.TenantId, x.ProjetoId });
            });

            // ===================== PRJ-REC =====================
            modelBuilder.Entity<RecursoTimesheet>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_recurso_timesheet");
                entity.Property(x => x.Notas).HasMaxLength(2000);
                entity.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(30);
                entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
                entity.HasIndex(x => new { x.TenantId, x.UsuarioId, x.Data });
                entity.HasIndex(x => new { x.TenantId, x.ProjetoId, x.TarefaId });
            });

            modelBuilder.Entity<RecursoAlocacao>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_recurso_alocacao");
                entity.Property(x => x.PapelNoProjeto).HasMaxLength(100);
                entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
                entity.HasIndex(x => new { x.TenantId, x.RecursoId, x.ProjetoId });
            });

            // ===================== PRJ-RST =====================
            modelBuilder.Entity<EstagioTarefa>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_rst_estagio");
                entity.Property(x => x.Nome).HasMaxLength(120);
                entity.Property(x => x.Cor).HasMaxLength(30);
                entity.HasIndex(x => new { x.TenantId, x.Ordem });
            });

            modelBuilder.Entity<TarefaProjeto>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_rst_tarefa");
                entity.Property(x => x.Titulo).HasMaxLength(255);
                entity.Property(x => x.Descricao).HasMaxLength(4000);
                entity.Property(x => x.Prioridade).HasMaxLength(30);
                entity.Property(x => x.Visibilidade).HasMaxLength(30);
                entity.Property(x => x.Estado).HasConversion<string>().HasMaxLength(30);
                entity.HasIndex(x => new { x.TenantId, x.ProjetoId, x.EstagioId });
            });

            modelBuilder.Entity<SubtarefaChecklist>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_rst_subtarefa");
                entity.Property(x => x.Nome).HasMaxLength(255);
                entity.HasIndex(x => new { x.TenantId, x.TarefaId });
            });

            modelBuilder.Entity<DependenciaTarefa>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_rst_dependencia");
                entity.Property(x => x.TipoDependencia).HasConversion<string>().HasMaxLength(30);
                entity.Property(x => x.Observacao).HasMaxLength(1000);
                entity.HasIndex(x => new { x.TenantId, x.TarefaDependenteId });
            });

            modelBuilder.Entity<TimerOperacional>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_rst_timer");
                entity.Property(x => x.TipoRegistro).HasMaxLength(30);
                entity.Property(x => x.Observacao).HasMaxLength(1000);
                entity.HasIndex(x => new { x.TenantId, x.TarefaId });
            });

            modelBuilder.Entity<ReuniaoAcompanhamento>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_rst_reuniao");
                entity.Property(x => x.Nome).HasMaxLength(255);
                entity.Property(x => x.Tipo).HasMaxLength(50);
                entity.Property(x => x.Departamento).HasMaxLength(120);
                entity.Property(x => x.Local).HasMaxLength(255);
                entity.HasIndex(x => new { x.TenantId, x.ProjetoId });
            });

            // ===================== PRJ-FAT =====================
            modelBuilder.Entity<FaturamentoProjeto>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_faturamento_projeto");
                entity.Property(x => x.Codigo).HasMaxLength(30);
                entity.Property(x => x.Descricao).HasMaxLength(500);
                entity.Property(x => x.Moeda).HasMaxLength(10);
                entity.Property(x => x.MotivoRejeicao).HasMaxLength(1000);
                entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
                entity.Property(x => x.ModalidadeFaturamento).HasConversion<string>().HasMaxLength(30);
                entity.HasIndex(x => new { x.TenantId, x.Codigo }).IsUnique();
                entity.HasIndex(x => new { x.TenantId, x.ProjetoId });

                entity.HasMany(x => x.Itens)
                    .WithOne()
                    .HasForeignKey(i => i.FaturamentoProjetoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ItemFaturamentoProjeto>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_faturamento_projeto_item");
                entity.Property(x => x.Observacao).HasMaxLength(2000);
                entity.Property(x => x.OrigemTipo).HasMaxLength(50);
                entity.Property(x => x.TipoItem).HasConversion<string>().HasMaxLength(30);
                entity.HasIndex(x => new { x.TenantId, x.FaturamentoProjetoId, x.Sequencia });
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
