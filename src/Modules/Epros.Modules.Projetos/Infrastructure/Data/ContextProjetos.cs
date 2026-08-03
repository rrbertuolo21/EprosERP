using Epros.Infrastructure.Data;
using Epros.Modules.Projetos.Domain.Entities;
using Epros.Modules.Projetos.Domain.Entities.Definicao;
using Epros.Modules.Projetos.Domain.Entities.Orcamento;
using Epros.Modules.Projetos.Domain.Entities.Recursos;
using Epros.Modules.Projetos.Domain.Entities.Rastreamento;
using Epros.Modules.Projetos.Domain.Entities.Faturamento;
using Epros.Modules.Projetos.Domain.Entities.Encerramento;
using Epros.Modules.Projetos.Domain.Entities.Risco;
using Epros.Modules.Projetos.Domain.Entities.Portfolio;
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
        public DbSet<BaselineOrcamento> BaselinesOrcamento => Set<BaselineOrcamento>();

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

        // PRJ-ENC (Encerramento)
        public DbSet<EncerramentoProjeto> Encerramentos => Set<EncerramentoProjeto>();
        public DbSet<ItemEncerramento> ItensEncerramento => Set<ItemEncerramento>();
        public DbSet<HistoricoEncerramento> HistoricosEncerramento => Set<HistoricoEncerramento>();
        public DbSet<AnexoEncerramento> AnexosEncerramento => Set<AnexoEncerramento>();
        public DbSet<ParametroEncerramento> ParametrosEncerramento => Set<ParametroEncerramento>();

        // PRJ-RSK (Gestao de Riscos de Projeto)
        public DbSet<RiscoProjeto> Riscos => Set<RiscoProjeto>();
        public DbSet<EstagioRisco> EstagiosRisco => Set<EstagioRisco>();
        public DbSet<ResponsavelRisco> ResponsaveisRisco => Set<ResponsavelRisco>();
        public DbSet<ComentarioRisco> ComentariosRisco => Set<ComentarioRisco>();
        public DbSet<HistoricoRisco> HistoricosRisco => Set<HistoricoRisco>();
        public DbSet<AnexoRisco> AnexosRisco => Set<AnexoRisco>();
        public DbSet<ParametroRisco> ParametrosRisco => Set<ParametroRisco>();

        // PRJ-PRT (Portfolio e Priorizacao)
        public DbSet<Portfolio> Portfolios => Set<Portfolio>();
        public DbSet<PortfolioItem> ItensPortfolio => Set<PortfolioItem>();
        public DbSet<HistoricoPortfolio> HistoricosPortfolio => Set<HistoricoPortfolio>();
        public DbSet<AnexoPortfolio> AnexosPortfolio => Set<AnexoPortfolio>();
        public DbSet<ParametroPortfolio> ParametrosPortfolio => Set<ParametroPortfolio>();
        public DbSet<Programa> Programas => Set<Programa>();

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
                // MM-a: status canônico persistido como string (mesma coluna varchar(30)).
                entity.Property(p => p.Status).HasConversion<string>().HasMaxLength(30);
                entity.Property(p => p.Tipo).HasConversion<string>().HasMaxLength(30);

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

            // DP-ORC-002: baseline versionada imutável (snapshot).
            modelBuilder.Entity<BaselineOrcamento>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_orcamento_baseline");
                entity.Property(x => x.MarcosSnapshotJson).HasColumnType("text");
                entity.Property(x => x.Motivo).HasMaxLength(1000);
                entity.HasIndex(x => new { x.TenantId, x.OrcamentoProjetoId, x.NumeroBaseline }).IsUnique();
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

            // ===================== PRJ-ENC (Encerramento) =====================
            modelBuilder.Entity<EncerramentoProjeto>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_enc_encerramento");
                entity.Property(x => x.Codigo).HasMaxLength(30);
                entity.Property(x => x.Descricao).HasMaxLength(500);
                entity.Property(x => x.MotivoRejeicao).HasMaxLength(1000);
                entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
                entity.Property(x => x.StatusFinalProjeto).HasConversion<string>().HasMaxLength(30);
                entity.HasIndex(x => new { x.TenantId, x.Codigo }).IsUnique();
                entity.HasIndex(x => new { x.TenantId, x.ProjetoId });

                entity.HasMany(x => x.Itens)
                    .WithOne()
                    .HasForeignKey(i => i.EncerramentoId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(x => x.Historicos)
                    .WithOne()
                    .HasForeignKey(h => h.EncerramentoId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(x => x.Anexos)
                    .WithOne()
                    .HasForeignKey(a => a.EncerramentoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ItemEncerramento>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_enc_encerramento_item");
                entity.Property(x => x.Observacao).HasMaxLength(2000);
                entity.HasIndex(x => new { x.TenantId, x.EncerramentoId, x.Sequencia });
            });

            modelBuilder.Entity<HistoricoEncerramento>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_enc_encerramento_historico");
                entity.Property(x => x.Acao).HasConversion<string>().HasMaxLength(30);
                entity.Property(x => x.Ip).HasMaxLength(60);
                entity.HasIndex(x => new { x.TenantId, x.EncerramentoId });
            });

            modelBuilder.Entity<AnexoEncerramento>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_enc_encerramento_anexo");
                entity.HasIndex(x => new { x.TenantId, x.EncerramentoId });
            });

            modelBuilder.Entity<ParametroEncerramento>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_enc_parametro");
                entity.Property(x => x.Chave).HasMaxLength(120);
                entity.HasIndex(x => new { x.TenantId, x.Chave }).IsUnique();
            });

            // ===================== PRJ-RSK (Gestao de Riscos de Projeto) =====================
            modelBuilder.Entity<RiscoProjeto>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_risco_projeto");
                entity.Property(x => x.Titulo).HasMaxLength(255);
                entity.Property(x => x.Descricao).HasMaxLength(4000);
                entity.Property(x => x.RiscoResidual).HasMaxLength(2000);
                entity.Property(x => x.MotivoRejeicao).HasMaxLength(1000);
                entity.Property(x => x.Prioridade).HasConversion<string>().HasMaxLength(30);
                entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
                entity.Property(x => x.Resposta).HasConversion<string>().HasMaxLength(30);
                entity.HasIndex(x => new { x.TenantId, x.ProjetoId, x.EstagioId });

                entity.HasMany(x => x.Responsaveis)
                    .WithOne()
                    .HasForeignKey(r => r.RiscoId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(x => x.Comentarios)
                    .WithOne()
                    .HasForeignKey(c => c.RiscoId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(x => x.Historicos)
                    .WithOne()
                    .HasForeignKey(h => h.RiscoId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(x => x.Anexos)
                    .WithOne()
                    .HasForeignKey(a => a.RiscoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<EstagioRisco>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_risco_estagio");
                entity.Property(x => x.Nome).HasMaxLength(255);
                entity.Property(x => x.Cor).HasMaxLength(7);
                entity.HasIndex(x => new { x.TenantId, x.Ordem });
            });

            modelBuilder.Entity<ResponsavelRisco>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_risco_responsavel");
                entity.HasIndex(x => new { x.TenantId, x.RiscoId, x.UsuarioId });
            });

            modelBuilder.Entity<ComentarioRisco>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_risco_comentario");
                entity.Property(x => x.Comentario).HasMaxLength(4000);
                entity.HasIndex(x => new { x.TenantId, x.RiscoId });
            });

            modelBuilder.Entity<HistoricoRisco>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_risco_historico");
                entity.Property(x => x.Acao).HasConversion<string>().HasMaxLength(30);
                entity.Property(x => x.Ip).HasMaxLength(60);
                entity.HasIndex(x => new { x.TenantId, x.RiscoId });
            });

            modelBuilder.Entity<AnexoRisco>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_risco_anexo");
                entity.Property(x => x.TipoDocumento).HasMaxLength(50);
                entity.HasIndex(x => new { x.TenantId, x.RiscoId });
            });

            modelBuilder.Entity<ParametroRisco>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_risco_parametro");
                entity.Property(x => x.Chave).HasMaxLength(120);
                entity.HasIndex(x => new { x.TenantId, x.Chave }).IsUnique();
            });

            // ===================== PRJ-PRT (Portfolio e Priorizacao) =====================
            modelBuilder.Entity<Portfolio>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_portfolio");
                entity.Property(x => x.Codigo).HasMaxLength(30);
                entity.Property(x => x.Descricao).HasMaxLength(500);
                entity.Property(x => x.TipoPortfolio).HasMaxLength(50);
                entity.Property(x => x.Justificativa).HasMaxLength(4000);
                entity.Property(x => x.MotivoRejeicao).HasMaxLength(1000);
                entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
                entity.HasIndex(x => new { x.TenantId, x.Codigo }).IsUnique();

                entity.HasMany(x => x.Itens)
                    .WithOne()
                    .HasForeignKey(i => i.PortfolioId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(x => x.Historicos)
                    .WithOne()
                    .HasForeignKey(h => h.PortfolioId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(x => x.Anexos)
                    .WithOne()
                    .HasForeignKey(a => a.PortfolioId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PortfolioItem>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_portfolio_item");
                entity.Property(x => x.TipoItem).HasMaxLength(50);
                entity.Property(x => x.Titulo).HasMaxLength(255);
                entity.Property(x => x.JustificativaPrioridade).HasMaxLength(4000);
                entity.Property(x => x.Observacao).HasMaxLength(4000);
                entity.HasIndex(x => new { x.TenantId, x.PortfolioId, x.Sequencia });
            });

            modelBuilder.Entity<HistoricoPortfolio>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_portfolio_historico");
                entity.Property(x => x.Acao).HasConversion<string>().HasMaxLength(30);
                entity.Property(x => x.Ip).HasMaxLength(60);
                entity.Property(x => x.Motivo).HasMaxLength(1000);
                entity.HasIndex(x => new { x.TenantId, x.PortfolioId });
            });

            modelBuilder.Entity<AnexoPortfolio>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_portfolio_anexo");
                entity.Property(x => x.TipoAnexo).HasMaxLength(50);
                entity.HasIndex(x => new { x.TenantId, x.PortfolioId });
            });

            modelBuilder.Entity<ParametroPortfolio>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_portfolio_parametro");
                entity.Property(x => x.Chave).HasMaxLength(120);
                entity.HasIndex(x => new { x.TenantId, x.Chave }).IsUnique();
            });

            // T-PRG: Programa (hierarquia Portfólio > Programa > Projeto).
            modelBuilder.Entity<Programa>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.ToTable("prj_programa");
                entity.Property(x => x.Codigo).HasMaxLength(30);
                entity.Property(x => x.Nome).HasMaxLength(255);
                entity.Property(x => x.Descricao).HasMaxLength(2000);
                entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
                entity.HasIndex(x => new { x.TenantId, x.Codigo }).IsUnique();
                entity.HasIndex(x => new { x.TenantId, x.PortfolioId });
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
