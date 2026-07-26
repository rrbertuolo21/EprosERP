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

        // MAN-PRV (Manutencao Preventiva)
        public DbSet<PlanoPreventivo> PlanosPreventivos => Set<PlanoPreventivo>();
        public DbSet<PlanoPreventivoPeriodicidade> PlanoPreventivoPeriodicidades => Set<PlanoPreventivoPeriodicidade>();
        public DbSet<PlanoPreventivoChecklistItem> PlanoPreventivoChecklistItens => Set<PlanoPreventivoChecklistItem>();
        public DbSet<PlanoPreventivoKitPeca> PlanoPreventivoKitPecas => Set<PlanoPreventivoKitPeca>();
        public DbSet<PlanoPreventivoExecucao> PlanoPreventivoExecucoes => Set<PlanoPreventivoExecucao>();

        // MAN-TRB (Gestao de Trabalho)
        public DbSet<OrdemServico> OrdensServico => Set<OrdemServico>();
        public DbSet<OrdemServicoStatus> OrdemServicoStatus => Set<OrdemServicoStatus>();
        public DbSet<OrdemServicoEquipamento> OrdemServicoEquipamentos => Set<OrdemServicoEquipamento>();
        public DbSet<OrdemServicoEvolucao> OrdemServicoEvolucoes => Set<OrdemServicoEvolucao>();
        public DbSet<OrdemServicoItem> OrdemServicoItens => Set<OrdemServicoItem>();
        public DbSet<OrdemServicoFinanceiroFiscal> OrdemServicoFinanceiros => Set<OrdemServicoFinanceiroFiscal>();

        // MAN-PEC (Pecas de Reposicao)
        public DbSet<RegistroPecaReposicao> RegistrosPecaReposicao => Set<RegistroPecaReposicao>();
        public DbSet<ItemPecaReposicao> ItensPecaReposicao => Set<ItemPecaReposicao>();
        public DbSet<ReservaPeca> ReservasPeca => Set<ReservaPeca>();
        public DbSet<MovimentoPeca> MovimentosPeca => Set<MovimentoPeca>();
        public DbSet<PoliticaReposicao> PoliticasReposicao => Set<PoliticaReposicao>();
        public DbSet<KitPreventivo> KitsPreventivos => Set<KitPreventivo>();

        // MAN-PAR (Gestao de Paradas)
        public DbSet<Parada> Paradas => Set<Parada>();
        public DbSet<ParadaItem> ParadaItens => Set<ParadaItem>();
        public DbSet<MotivoParada> MotivosParada => Set<MotivoParada>();
        public DbSet<ParadaVinculoOs> ParadaVinculosOs => Set<ParadaVinculoOs>();
        public DbSet<ParadaIndicador> ParadaIndicadores => Set<ParadaIndicador>();

        // MAN-IND (Inducao e Configuracao de Equipamentos)
        public DbSet<EquipamentoVinculoPatrimonial> EquipamentoVinculosPatrimoniais => Set<EquipamentoVinculoPatrimonial>();
        public DbSet<EquipamentoInducao> EquipamentoInducoes => Set<EquipamentoInducao>();
        public DbSet<EquipamentoServico> EquipamentoServicos => Set<EquipamentoServico>();
        public DbSet<EquipamentoAtributo> EquipamentoAtributos => Set<EquipamentoAtributo>();

        // MAN-CRV (Confiabilidade e Revisao)
        public DbSet<RevisaoConfiabilidade> RevisoesConfiabilidade => Set<RevisaoConfiabilidade>();
        public DbSet<ModoFalhaConfiabilidade> ModosFalhaConfiabilidade => Set<ModoFalhaConfiabilidade>();
        public DbSet<IndicadorConfiabilidade> IndicadoresConfiabilidade => Set<IndicadorConfiabilidade>();
        public DbSet<RecomendacaoEstrategia> RecomendacoesEstrategia => Set<RecomendacaoEstrategia>();
        public DbSet<HistoricoConfiabilidade> HistoricosConfiabilidade => Set<HistoricoConfiabilidade>();
        public DbSet<AnexoConfiabilidade> AnexosConfiabilidade => Set<AnexoConfiabilidade>();
        public DbSet<ParametroConfiabilidade> ParametrosConfiabilidade => Set<ParametroConfiabilidade>();
        public DbSet<EventoIntegracaoConfiabilidade> EventosIntegracaoConfiabilidade => Set<EventoIntegracaoConfiabilidade>();

        // MAN-PDT (Manutencao Preditiva)
        public DbSet<MonitoramentoPreditivo> MonitoramentosPreditivos => Set<MonitoramentoPreditivo>();
        public DbSet<ItemMonitoramentoPreditivo> ItensMonitoramentoPreditivo => Set<ItemMonitoramentoPreditivo>();
        public DbSet<PontoMedicao> PontosMedicao => Set<PontoMedicao>();
        public DbSet<LeituraCondicao> LeiturasCondicao => Set<LeituraCondicao>();
        public DbSet<RegraMonitoramento> RegrasMonitoramento => Set<RegraMonitoramento>();
        public DbSet<AlarmePreditivo> AlarmesPreditivos => Set<AlarmePreditivo>();
        public DbSet<VinculoOrdemTrabalhoPreditivo> VinculosOrdemTrabalhoPreditivo => Set<VinculoOrdemTrabalhoPreditivo>();
        public DbSet<HistoricoPreditivo> HistoricosPreditivos => Set<HistoricoPreditivo>();
        public DbSet<AnexoPreditivo> AnexosPreditivos => Set<AnexoPreditivo>();
        public DbSet<ParametroPreditivo> ParametrosPreditivos => Set<ParametroPreditivo>();
        public DbSet<EventoIntegracaoPreditivo> EventosIntegracaoPreditivo => Set<EventoIntegracaoPreditivo>();

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
                entity.Property(e => e.Nome).HasMaxLength(200);
                entity.Property(e => e.Codigo).HasMaxLength(60);
                entity.Property(e => e.Setor).HasMaxLength(120);
                entity.Property(e => e.Status).HasMaxLength(30);
                entity.Property(e => e.Criticidade).HasMaxLength(20);
                entity.Property(e => e.Descricao).HasMaxLength(1000);
                entity.Property(e => e.NumeroSerie).HasMaxLength(120);
                entity.Property(e => e.FuncaoOperacional).HasMaxLength(200);
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

            // ===================== MAN-PRV =====================
            modelBuilder.Entity<PlanoPreventivo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_prv_plano");
                entity.HasIndex(e => new { e.TenantId, e.Codigo }).IsUnique();
                entity.HasIndex(e => new { e.TenantId, e.Status });
                entity.Property(e => e.Codigo).HasMaxLength(30);
                entity.Property(e => e.Descricao).HasMaxLength(500);
                entity.Property(e => e.AlvoTipo).HasMaxLength(30);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);

                entity.HasMany(e => e.Periodicidades).WithOne().HasForeignKey(p => p.PlanoId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.ChecklistItens).WithOne().HasForeignKey(p => p.PlanoId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.KitPecas).WithOne().HasForeignKey(p => p.PlanoId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Execucoes).WithOne().HasForeignKey(p => p.PlanoId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PlanoPreventivoPeriodicidade>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_prv_periodicidade");
                entity.HasIndex(e => new { e.TenantId, e.PlanoId });
                entity.Property(e => e.TipoPeriodicidade).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.Situacao).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.UnidadeIntervalo).HasMaxLength(30);
                entity.Property(e => e.ContadorTipo).HasMaxLength(60);
                entity.Property(e => e.Tolerancia).HasMaxLength(60);
            });

            modelBuilder.Entity<PlanoPreventivoChecklistItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_prv_checklist_item");
                entity.HasIndex(e => new { e.PlanoId, e.Sequencia });
                entity.Property(e => e.DescricaoTarefa).HasMaxLength(500);
                entity.Property(e => e.TipoResposta).HasMaxLength(30);
                entity.Property(e => e.CriterioAceite).HasMaxLength(500);
            });

            modelBuilder.Entity<PlanoPreventivoKitPeca>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_prv_kit_peca");
                entity.HasIndex(e => new { e.TenantId, e.PlanoId });
                entity.Property(e => e.Unidade).HasMaxLength(20);
                entity.Property(e => e.Observacao).HasMaxLength(500);
            });

            modelBuilder.Entity<PlanoPreventivoExecucao>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_prv_execucao_programada");
                entity.HasIndex(e => new { e.PlanoId, e.Status, e.DataPrevista });
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.Prioridade).HasMaxLength(30);
                entity.Property(e => e.MotivoCancelamento).HasMaxLength(500);
            });

            // ===================== MAN-TRB =====================
            modelBuilder.Entity<OrdemServico>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_trb_ordem_servico");
                entity.HasIndex(e => new { e.TenantId, e.Numero });
                entity.HasIndex(e => new { e.TenantId, e.StatusCodigo });
                entity.Property(e => e.Numero).HasMaxLength(30);
                entity.Property(e => e.PerfilOrdem).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.StatusCodigo).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.NomeContato).HasMaxLength(200);
                entity.Property(e => e.FoneContato).HasMaxLength(30);
                entity.Property(e => e.HoraInicio).HasMaxLength(8);
                entity.Property(e => e.HoraPrevisao).HasMaxLength(8);
                entity.Property(e => e.HoraFim).HasMaxLength(8);
                entity.Property(e => e.DocumentoPessoa).HasMaxLength(20);
                entity.Property(e => e.InfoEquip1).HasMaxLength(200);
                entity.Property(e => e.InfoEquip2).HasMaxLength(200);
                entity.Property(e => e.InfoEquip3).HasMaxLength(200);
                entity.Property(e => e.ObsEquip1).HasMaxLength(500);
                entity.Property(e => e.ObsEquip2).HasMaxLength(500);
                entity.Property(e => e.ObservacaoCliente).HasMaxLength(1000);
                entity.Property(e => e.ObservacaoAbertura).HasMaxLength(1000);
                entity.Property(e => e.Reclamacao).HasMaxLength(1000);
                entity.Property(e => e.Observacao).HasMaxLength(1000);

                entity.HasMany(e => e.Itens).WithOne().HasForeignKey(i => i.OrdemServicoId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Evolucoes).WithOne().HasForeignKey(i => i.OrdemServicoId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Equipamentos).WithOne().HasForeignKey(i => i.OrdemServicoId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.VinculosFinanceiros).WithOne().HasForeignKey(i => i.OrdemServicoId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OrdemServicoStatus>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_trb_status_os");
                entity.HasIndex(e => new { e.TenantId, e.Codigo }).IsUnique();
                entity.Property(e => e.Codigo).HasMaxLength(30);
                entity.Property(e => e.Nome).HasMaxLength(120);
                entity.Property(e => e.CampoDataAssociado).HasMaxLength(30);
            });

            modelBuilder.Entity<OrdemServicoEquipamento>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_trb_os_equipamento");
                entity.HasIndex(e => e.OrdemServicoId);
                entity.Property(e => e.NumeroSerie).HasMaxLength(120);
                entity.Property(e => e.Observacao).HasMaxLength(500);
            });

            modelBuilder.Entity<OrdemServicoEvolucao>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_trb_os_evolucao");
                entity.HasIndex(e => e.OrdemServicoId);
                entity.Property(e => e.HoraRegistro).HasMaxLength(8);
                entity.Property(e => e.Observacao).HasMaxLength(1000);
            });

            modelBuilder.Entity<OrdemServicoItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_trb_os_item");
                entity.HasIndex(e => e.OrdemServicoId);
                entity.Property(e => e.Tipo).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.TipoSaida).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.Complemento).HasMaxLength(500);
                entity.Property(e => e.Informacao).HasMaxLength(200);
            });

            modelBuilder.Entity<OrdemServicoFinanceiroFiscal>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_trb_os_financeiro");
                entity.HasIndex(e => e.OrdemServicoId);
                entity.Property(e => e.TipoVinculo).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.StatusVinculo).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.Erro).HasMaxLength(1000);
            });

            // ===================== MAN-PEC =====================
            modelBuilder.Entity<RegistroPecaReposicao>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_pec_registro");
                entity.HasIndex(e => new { e.TenantId, e.Codigo }).IsUnique();
                entity.Property(e => e.Codigo).HasMaxLength(30);
                entity.Property(e => e.Descricao).HasMaxLength(500);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);

                entity.HasMany(e => e.Itens).WithOne().HasForeignKey(i => i.RegistroId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ItemPecaReposicao>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_pec_item");
                entity.HasIndex(e => new { e.RegistroId, e.Sequencia });
                entity.Property(e => e.StatusItem).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.TipoSaida).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.Informacao).HasMaxLength(200);
                entity.Property(e => e.Observacao).HasMaxLength(500);

                entity.HasMany(e => e.Reservas).WithOne().HasForeignKey(r => r.ItemId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Movimentos).WithOne().HasForeignKey(m => m.ItemId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ReservaPeca>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_pec_reserva");
                entity.HasIndex(e => e.ItemId);
                entity.Property(e => e.StatusReserva).HasConversion<string>().HasMaxLength(20);
            });

            modelBuilder.Entity<MovimentoPeca>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_pec_movimento");
                entity.HasIndex(e => e.ItemId);
                entity.Property(e => e.TipoMovimento).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.StatusMovimento).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.InformacaoMovimento).HasMaxLength(300);
                entity.Property(e => e.Erro).HasMaxLength(1000);
            });

            modelBuilder.Entity<PoliticaReposicao>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_pec_politica_reposicao");
                entity.HasIndex(e => new { e.TenantId, e.ProdutoId });
                entity.Property(e => e.Criticidade).HasMaxLength(20);
            });

            modelBuilder.Entity<KitPreventivo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_pec_kit_preventivo");
                entity.HasIndex(e => new { e.TenantId, e.PlanoPreventivoId });
                entity.Property(e => e.Observacao).HasMaxLength(500);
            });

            // ===================== MAN-PAR =====================
            modelBuilder.Entity<Parada>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_par_parada");
                entity.HasIndex(e => new { e.TenantId, e.Codigo }).IsUnique();
                entity.HasIndex(e => new { e.TenantId, e.Status });
                entity.Property(e => e.Codigo).HasMaxLength(30);
                entity.Property(e => e.Descricao).HasMaxLength(500);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.TipoParada).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.Observacao).HasMaxLength(1000);

                entity.HasMany(e => e.Itens).WithOne().HasForeignKey(i => i.ParadaId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.VinculosOs).WithOne().HasForeignKey(i => i.ParadaId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Indicadores).WithOne().HasForeignKey(i => i.ParadaId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ParadaItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_par_item");
                entity.HasIndex(e => new { e.ParadaId, e.Sequencia });
                entity.Property(e => e.TipoImpacto).HasMaxLength(60);
                entity.Property(e => e.Unidade).HasMaxLength(20);
                entity.Property(e => e.Observacao).HasMaxLength(500);
            });

            modelBuilder.Entity<MotivoParada>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_par_motivo");
                entity.HasIndex(e => new { e.TenantId, e.Codigo }).IsUnique();
                entity.Property(e => e.Codigo).HasMaxLength(30);
                entity.Property(e => e.Descricao).HasMaxLength(200);
                entity.Property(e => e.TipoParadaAplicavel).HasMaxLength(60);
            });

            modelBuilder.Entity<ParadaVinculoOs>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_par_vinculo_os");
                entity.HasIndex(e => e.ParadaId);
                entity.Property(e => e.TipoVinculo).HasConversion<string>().HasMaxLength(30);
                entity.Property(e => e.StatusVinculo).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.RegraAcionadora).HasMaxLength(500);
                entity.Property(e => e.MensagemErro).HasMaxLength(1000);
            });

            modelBuilder.Entity<ParadaIndicador>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_par_indicador");
                entity.HasIndex(e => e.ParadaId);
                entity.Property(e => e.TipoIndicador).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.Unidade).HasMaxLength(20);
                entity.Property(e => e.FormulaAplicada).HasMaxLength(200);
                entity.Property(e => e.OrigemDados).HasMaxLength(1000);
            });

            // ===================== MAN-IND =====================
            modelBuilder.Entity<EquipamentoVinculoPatrimonial>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_ind_vinculo_patrimonial");
                entity.HasIndex(e => e.EquipamentoId);
                entity.Property(e => e.NumeroPatrimonial).HasMaxLength(60);
                entity.Property(e => e.NomeAtivo).HasMaxLength(200);
                entity.Property(e => e.DescricaoAtivo).HasMaxLength(1000);
                entity.Property(e => e.NumeroNotaFiscal).HasMaxLength(60);
                entity.Property(e => e.ChaveNfe).HasMaxLength(60);
                entity.Property(e => e.MetodoDepreciacao).HasMaxLength(60);
                entity.Property(e => e.TipoDepreciacao).HasMaxLength(60);
                entity.Property(e => e.Funcao).HasMaxLength(200);
            });

            modelBuilder.Entity<EquipamentoInducao>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_ind_inducao");
                entity.HasIndex(e => e.EquipamentoId);
                entity.Property(e => e.StatusInducao).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.Observacao).HasMaxLength(1000);
            });

            modelBuilder.Entity<EquipamentoServico>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_ind_servico_equipamento");
                entity.HasIndex(e => new { e.TenantId, e.EquipamentoId });
            });

            modelBuilder.Entity<EquipamentoAtributo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_ind_atributo");
                entity.HasIndex(e => new { e.EquipamentoId, e.Chave });
                entity.Property(e => e.Chave).HasMaxLength(120);
                entity.Property(e => e.Valor).HasMaxLength(2000);
                entity.Property(e => e.OrigemFuncional).HasMaxLength(60);
            });

            // ===================== MAN-CRV =====================
            modelBuilder.Entity<RevisaoConfiabilidade>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_crv_revisao");
                entity.HasIndex(e => new { e.TenantId, e.Codigo }).IsUnique();
                entity.HasIndex(e => new { e.TenantId, e.Status });
                entity.HasIndex(e => new { e.TenantId, e.AtivoId });
                entity.Property(e => e.Codigo).HasMaxLength(30);
                entity.Property(e => e.Descricao).HasMaxLength(500);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.FuncaoOperacional).HasMaxLength(200);
                entity.Property(e => e.EstadoConservacao).HasMaxLength(60);
                entity.Property(e => e.CriticidadeOperacional).HasMaxLength(30);
                entity.Property(e => e.MotivoRejeicao).HasMaxLength(1000);
                entity.Property(e => e.MotivoSuspensao).HasMaxLength(1000);
                entity.Property(e => e.MotivoEncerramento).HasMaxLength(1000);

                entity.HasMany(e => e.ModosFalha).WithOne().HasForeignKey(i => i.RevisaoId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Indicadores).WithOne().HasForeignKey(i => i.RevisaoId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Recomendacoes).WithOne().HasForeignKey(i => i.RevisaoId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Historicos).WithOne().HasForeignKey(i => i.RevisaoId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Anexos).WithOne().HasForeignKey(i => i.RevisaoId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ModoFalhaConfiabilidade>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_crv_modo_falha");
                entity.HasIndex(e => new { e.RevisaoId, e.Sequencia });
                entity.HasIndex(e => new { e.RevisaoId, e.Rpn });
                entity.Property(e => e.Componente).HasMaxLength(200);
                entity.Property(e => e.ModoFalha).HasMaxLength(500);
                entity.Property(e => e.EfeitoFalha).HasMaxLength(1000);
                entity.Property(e => e.CausaFalha).HasMaxLength(1000);
                entity.Property(e => e.ControleAtual).HasMaxLength(1000);
                entity.Property(e => e.Observacao).HasMaxLength(1000);
            });

            modelBuilder.Entity<IndicadorConfiabilidade>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_crv_indicador");
                entity.HasIndex(e => new { e.RevisaoId, e.TipoIndicador });
                entity.Property(e => e.TipoIndicador).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.CalculadoPor).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.Unidade).HasMaxLength(20);
                entity.Property(e => e.FormulaAplicada).HasMaxLength(200);
                entity.Property(e => e.OrigemDados).HasMaxLength(1000);
            });

            modelBuilder.Entity<RecomendacaoEstrategia>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_crv_recomendacao");
                entity.HasIndex(e => new { e.RevisaoId, e.Status });
                entity.Property(e => e.Estrategia).HasConversion<string>().HasMaxLength(30);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.Justificativa).HasMaxLength(2000);
            });

            modelBuilder.Entity<HistoricoConfiabilidade>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_crv_historico");
                entity.HasIndex(e => new { e.RevisaoId, e.DataHora });
                entity.Property(e => e.Acao).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.IpOrigem).HasMaxLength(60);
                entity.Property(e => e.Justificativa).HasMaxLength(1000);
                entity.Property(e => e.PayloadJson).HasColumnType("jsonb");
            });

            modelBuilder.Entity<AnexoConfiabilidade>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_crv_anexo");
                entity.HasIndex(e => e.RevisaoId);
                entity.Property(e => e.TipoDocumento).HasMaxLength(60);
                entity.Property(e => e.Descricao).HasMaxLength(500);
            });

            modelBuilder.Entity<ParametroConfiabilidade>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_crv_parametro");
                entity.HasIndex(e => new { e.TenantId, e.Chave }).IsUnique();
                entity.Property(e => e.Chave).HasMaxLength(120);
                entity.Property(e => e.ValorJson).HasColumnType("jsonb");
                entity.Property(e => e.Descricao).HasMaxLength(500);
            });

            modelBuilder.Entity<EventoIntegracaoConfiabilidade>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_crv_evento_integracao");
                entity.HasIndex(e => new { e.RevisaoId, e.TipoEvento, e.StatusEnvio });
                entity.Property(e => e.TipoEvento).HasMaxLength(120);
                entity.Property(e => e.DestinoFuncional).HasMaxLength(120);
                entity.Property(e => e.StatusEnvio).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.PayloadJson).HasColumnType("jsonb");
                entity.Property(e => e.UltimoErro).HasMaxLength(1000);
            });

            // ===================== MAN-PDT =====================
            modelBuilder.Entity<MonitoramentoPreditivo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_pdt_monitoramento");
                entity.HasIndex(e => new { e.TenantId, e.Codigo }).IsUnique();
                entity.HasIndex(e => new { e.TenantId, e.Status });
                entity.HasIndex(e => new { e.TenantId, e.EquipamentoId });
                entity.Property(e => e.Codigo).HasMaxLength(30);
                entity.Property(e => e.Descricao).HasMaxLength(500);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.Observacao).HasMaxLength(1000);

                entity.HasMany(e => e.Itens).WithOne().HasForeignKey(i => i.MonitoramentoId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.PontosMedicao).WithOne().HasForeignKey(i => i.MonitoramentoId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Alarmes).WithOne().HasForeignKey(i => i.MonitoramentoId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Historicos).WithOne().HasForeignKey(i => i.MonitoramentoId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Anexos).WithOne().HasForeignKey(i => i.MonitoramentoId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ItemMonitoramentoPreditivo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_pdt_item");
                entity.HasIndex(e => new { e.MonitoramentoId, e.Sequencia });
                entity.Property(e => e.Observacao).HasMaxLength(1000);
            });

            modelBuilder.Entity<PontoMedicao>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_pdt_ponto_medicao");
                entity.HasIndex(e => new { e.MonitoramentoId, e.Variavel, e.LocalTecnico });
                entity.Property(e => e.CodigoPonto).HasMaxLength(60);
                entity.Property(e => e.Variavel).HasMaxLength(60);
                entity.Property(e => e.Unidade).HasMaxLength(20);
                entity.Property(e => e.LocalTecnico).HasMaxLength(120);
                entity.Property(e => e.Periodicidade).HasMaxLength(60);
                entity.Property(e => e.Situacao).HasConversion<string>().HasMaxLength(20);

                entity.HasMany(e => e.Leituras).WithOne().HasForeignKey(i => i.PontoMedicaoId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Regras).WithOne().HasForeignKey(i => i.PontoMedicaoId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<LeituraCondicao>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_pdt_leitura_condicao");
                entity.HasIndex(e => new { e.PontoMedicaoId, e.DataHoraMedicao });
                entity.Property(e => e.Unidade).HasMaxLength(20);
                entity.Property(e => e.Origem).HasMaxLength(120);
                entity.Property(e => e.PayloadJson).HasColumnType("jsonb");
            });

            modelBuilder.Entity<RegraMonitoramento>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_pdt_regra_monitoramento");
                entity.HasIndex(e => new { e.PontoMedicaoId, e.Situacao });
                entity.Property(e => e.TipoRegra).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.Operador).HasMaxLength(30);
                entity.Property(e => e.JanelaAvaliacao).HasMaxLength(60);
                entity.Property(e => e.Severidade).HasMaxLength(30);
                entity.Property(e => e.AcaoEsperada).HasMaxLength(30);
                entity.Property(e => e.Situacao).HasConversion<string>().HasMaxLength(20);
            });

            modelBuilder.Entity<AlarmePreditivo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_pdt_alarme");
                entity.HasIndex(e => new { e.TenantId, e.Status, e.DataHoraDisparo });
                entity.HasIndex(e => e.PontoMedicaoId);
                entity.Property(e => e.Severidade).HasMaxLength(30);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.Descricao).HasMaxLength(1000);
                entity.Property(e => e.MotivoEncerramento).HasMaxLength(1000);

                entity.HasMany(e => e.Vinculos).WithOne().HasForeignKey(i => i.AlarmeId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<VinculoOrdemTrabalhoPreditivo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_pdt_ordem_trabalho_vinculo");
                entity.HasIndex(e => e.AlarmeId);
                entity.Property(e => e.StatusRetorno).HasMaxLength(60);
                entity.Property(e => e.PayloadRetorno).HasColumnType("jsonb");
            });

            modelBuilder.Entity<HistoricoPreditivo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_pdt_historico");
                entity.HasIndex(e => new { e.MonitoramentoId, e.DataHora });
                entity.HasIndex(e => e.AlarmeId);
                entity.Property(e => e.Acao).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.Ip).HasMaxLength(60);
                entity.Property(e => e.PayloadJson).HasColumnType("jsonb");
            });

            modelBuilder.Entity<AnexoPreditivo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_pdt_anexo");
                entity.HasIndex(e => e.MonitoramentoId);
                entity.Property(e => e.TipoDocumento).HasMaxLength(60);
            });

            modelBuilder.Entity<ParametroPreditivo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_pdt_parametro");
                entity.HasIndex(e => new { e.TenantId, e.Chave }).IsUnique();
                entity.Property(e => e.Chave).HasMaxLength(120);
                entity.Property(e => e.ValorJson).HasColumnType("jsonb");
                entity.Property(e => e.Situacao).HasConversion<string>().HasMaxLength(20);
            });

            modelBuilder.Entity<EventoIntegracaoPreditivo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("man_pdt_evento_integracao");
                entity.HasIndex(e => new { e.TenantId, e.TipoEvento, e.Status });
                entity.HasIndex(e => new { e.EntidadeOrigem, e.EntidadeOrigemId });
                entity.Property(e => e.EntidadeOrigem).HasMaxLength(120);
                entity.Property(e => e.TipoEvento).HasMaxLength(120);
                entity.Property(e => e.Direcao).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.PayloadJson).HasColumnType("jsonb");
                entity.Property(e => e.MensagemErro).HasMaxLength(1000);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
