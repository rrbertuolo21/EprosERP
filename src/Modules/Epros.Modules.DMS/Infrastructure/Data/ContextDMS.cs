using Epros.Infrastructure.Data;
using Epros.Modules.DMS.Domain.Entities;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.DMS.Infrastructure.Data
{
    public class ContextDMS : ContextBase
    {
        // ===== MVP existente =====
        public DbSet<VendaVeiculo> VendasVeiculos => Set<VendaVeiculo>();
        public DbSet<OrdemServicoDms> OrdensServicoDms => Set<OrdemServicoDms>();
        public DbSet<GarantiaMontadora> GarantiasMontadora => Set<GarantiaMontadora>();

        // ===== CON-CRM (CRM de Concessionária) =====
        public DbSet<ProspectShowroom> ProspectsShowroom => Set<ProspectShowroom>();
        public DbSet<OportunidadeConcessionaria> OportunidadesConcessionaria => Set<OportunidadeConcessionaria>();
        public DbSet<TestDrive> TestDrives => Set<TestDrive>();

        // ===== CON-VEN (Vendas F&I) =====
        public DbSet<EstoqueVeiculo> EstoqueVeiculos => Set<EstoqueVeiculo>();
        public DbSet<ReservaVeiculo> ReservasVeiculo => Set<ReservaVeiculo>();
        public DbSet<PropostaVenda> PropostasVenda => Set<PropostaVenda>();

        // ===== CON-SRV (Gestão de Serviços) =====
        public DbSet<TipoServicoConcessionaria> TiposServicoConcessionaria => Set<TipoServicoConcessionaria>();
        public DbSet<OperacaoServico> OperacoesServico => Set<OperacaoServico>();
        public DbSet<PacoteServico> PacotesServico => Set<PacoteServico>();

        // ===== CON-MNT (Manutenção) =====
        public DbSet<OrdemServicoManutencao> OrdensServicoManutencao => Set<OrdemServicoManutencao>();
        public DbSet<OrcamentoManutencao> OrcamentosManutencao => Set<OrcamentoManutencao>();

        // ===== CON-PES (Peças de Reposição) =====
        public DbSet<PecaReposicao> PecasReposicao => Set<PecaReposicao>();
        public DbSet<DemandaPeca> DemandasPeca => Set<DemandaPeca>();
        public DbSet<ReservaPeca> ReservasPeca => Set<ReservaPeca>();

        // ===== CON-GAR (Garantias) =====
        public DbSet<PlanoGarantia> PlanosGarantia => Set<PlanoGarantia>();
        public DbSet<VeiculoGarantia> VeiculosGarantia => Set<VeiculoGarantia>();
        public DbSet<SolicitacaoGarantia> SolicitacoesGarantia => Set<SolicitacaoGarantia>();

        // ===== CON-DEV (Desenvolvimento de Concessionárias) =====
        public DbSet<RedeNo> RedeNos => Set<RedeNo>();
        public DbSet<ContratoRede> ContratosRede => Set<ContratoRede>();

        // ===== CON-FIN (Finanças / F&I) =====
        public DbSet<JornadaFin> JornadasFin => Set<JornadaFin>();
        public DbSet<SimulacaoFin> SimulacoesFin => Set<SimulacaoFin>();
        public DbSet<ContratoFin> ContratosFin => Set<ContratoFin>();

        // ===== Outbox de integração entre módulos =====
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        public ContextDMS(
            DbContextOptions<ContextDMS> options,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser) : base(options, tenantProvider, currentUser)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("concessionarias");

            // ---------- MVP existente ----------
            modelBuilder.Entity<VendaVeiculo>(entity =>
            {
                entity.HasKey(v => v.Id);
                entity.ToTable("vendas_veiculos");
                entity.Property(v => v.Chassi).HasMaxLength(17);
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

            // ---------- CON-CRM ----------
            modelBuilder.Entity<ProspectShowroom>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.ToTable("con_crm_prospect_showroom");
                entity.Property(p => p.Origem).HasMaxLength(60);
                entity.Property(p => p.Status).HasMaxLength(40);
                entity.HasIndex(p => new { p.TenantId, p.ContactId, p.UnidadeId });
            });

            modelBuilder.Entity<OportunidadeConcessionaria>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.ToTable("con_crm_oportunidade");
                entity.Property(o => o.Etapa).HasMaxLength(40);
                entity.HasIndex(o => new { o.TenantId, o.ProspectId });
                entity.HasIndex(o => new { o.TenantId, o.VendaId });
            });

            modelBuilder.Entity<TestDrive>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.ToTable("con_crm_test_drive");
                entity.Property(t => t.Status).HasMaxLength(30);
                entity.HasIndex(t => new { t.TenantId, t.VeiculoDemonstracaoId, t.Inicio, t.Fim });
            });

            // ---------- CON-VEN ----------
            modelBuilder.Entity<EstoqueVeiculo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("con_ven_estoque_veiculo");
                entity.Property(e => e.ChassiVin).HasMaxLength(17);
                entity.Property(e => e.Status).HasMaxLength(30);
                entity.HasIndex(e => new { e.TenantId, e.ChassiVin }).IsUnique();
            });

            modelBuilder.Entity<ReservaVeiculo>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.ToTable("con_ven_reserva");
                entity.Property(r => r.Status).HasMaxLength(30);
                entity.HasIndex(r => new { r.TenantId, r.EstoqueVeiculoId, r.Status });
            });

            modelBuilder.Entity<PropostaVenda>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.ToTable("con_ven_proposta");
                entity.Property(p => p.Status).HasMaxLength(30);
                entity.HasIndex(p => new { p.TenantId, p.OportunidadeId, p.Versao }).IsUnique();
            });

            // ---------- CON-SRV ----------
            modelBuilder.Entity<TipoServicoConcessionaria>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.ToTable("con_srv_tipo_servico");
                entity.Property(t => t.Codigo).HasMaxLength(30);
                entity.Property(t => t.Nome).HasMaxLength(200);
                entity.Property(t => t.Descricao).HasMaxLength(500);
                entity.Property(t => t.Status).HasMaxLength(30);
                entity.HasIndex(t => new { t.TenantId, t.Codigo }).IsUnique();
            });

            modelBuilder.Entity<OperacaoServico>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.ToTable("con_srv_operacao");
                entity.Property(o => o.Codigo).HasMaxLength(30);
                entity.Property(o => o.Descricao).HasMaxLength(500);
                entity.Property(o => o.TmoUnidade).HasMaxLength(20);
                entity.Property(o => o.NaturezaPadrao).HasMaxLength(60);
                entity.HasIndex(o => new { o.TenantId, o.Codigo, o.Versao }).IsUnique();
            });

            modelBuilder.Entity<PacoteServico>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.ToTable("con_srv_pacote");
                entity.Property(p => p.Codigo).HasMaxLength(30);
                entity.Property(p => p.Nome).HasMaxLength(200);
                entity.Property(p => p.Status).HasMaxLength(30);
                entity.HasIndex(p => new { p.TenantId, p.Codigo }).IsUnique();
            });

            // ---------- CON-MNT ----------
            modelBuilder.Entity<OrdemServicoManutencao>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.ToTable("con_mnt_os_extensao");
                entity.Property(o => o.ChassiVin).HasMaxLength(17);
                entity.Property(o => o.Placa).HasMaxLength(8);
                entity.Property(o => o.TipoAtendimento).HasMaxLength(40);
                entity.Property(o => o.StatusOficina).HasMaxLength(40);
                entity.HasIndex(o => new { o.TenantId, o.VeiculoId });
            });

            modelBuilder.Entity<OrcamentoManutencao>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.ToTable("con_mnt_orcamento");
                entity.Property(o => o.Status).HasMaxLength(30);
                entity.HasIndex(o => new { o.TenantId, o.OrdemServicoId, o.Versao }).IsUnique();
            });

            // ---------- CON-PES ----------
            modelBuilder.Entity<PecaReposicao>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.ToTable("con_pes_peca");
                entity.Property(p => p.FamiliaTecnica).HasMaxLength(60);
                entity.Property(p => p.Criticidade).HasMaxLength(40);
                entity.Property(p => p.Status).HasMaxLength(30);
                entity.HasIndex(p => new { p.TenantId, p.ProdutoId }).IsUnique();
            });

            modelBuilder.Entity<DemandaPeca>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.ToTable("con_pes_demanda");
                entity.Property(d => d.OrigemTipo).HasMaxLength(40);
                entity.Property(d => d.Prioridade).HasMaxLength(30);
                entity.Property(d => d.Status).HasMaxLength(30);
                entity.HasIndex(d => new { d.TenantId, d.OrigemTipo, d.OrigemId, d.ItemOrigemId }).IsUnique();
            });

            modelBuilder.Entity<ReservaPeca>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.ToTable("con_pes_reserva");
                entity.HasIndex(r => new { r.TenantId, r.DemandaId });
            });

            // ---------- CON-GAR ----------
            modelBuilder.Entity<PlanoGarantia>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.ToTable("con_gar_plano");
                entity.Property(p => p.Codigo).HasMaxLength(30);
                entity.Property(p => p.Nome).HasMaxLength(200);
                entity.Property(p => p.Descricao).HasMaxLength(500);
                entity.Property(p => p.DuracaoTipo).HasMaxLength(10);
                entity.Property(p => p.Status).HasMaxLength(30);
                entity.HasIndex(p => new { p.TenantId, p.Codigo }).IsUnique();
            });

            modelBuilder.Entity<VeiculoGarantia>(entity =>
            {
                entity.HasKey(v => v.Id);
                entity.ToTable("con_gar_veiculo_garantia");
                entity.Property(v => v.ChassiVin).HasMaxLength(17);
                entity.Property(v => v.Status).HasMaxLength(30);
                entity.HasIndex(v => new { v.TenantId, v.VeiculoId });
            });

            modelBuilder.Entity<SolicitacaoGarantia>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.ToTable("con_gar_solicitacao");
                entity.Property(s => s.Protocolo).HasMaxLength(60);
                entity.Property(s => s.Status).HasMaxLength(30);
                entity.Property(s => s.Sintoma).HasMaxLength(1000);
                entity.Property(s => s.RelatoCliente).HasMaxLength(2000);
                entity.HasIndex(s => new { s.TenantId, s.Protocolo });
            });

            // ---------- CON-DEV ----------
            modelBuilder.Entity<RedeNo>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.ToTable("con_dev_rede_no");
                entity.Property(r => r.Codigo).HasMaxLength(30);
                entity.Property(r => r.TipoNo).HasMaxLength(40);
                entity.Property(r => r.Status).HasMaxLength(30);
                entity.HasIndex(r => new { r.TenantId, r.Codigo }).IsUnique();
            });

            modelBuilder.Entity<ContratoRede>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.ToTable("con_dev_contrato");
                entity.Property(c => c.Tipo).HasMaxLength(60);
                entity.Property(c => c.Status).HasMaxLength(30);
                entity.HasIndex(c => new { c.TenantId, c.RedeNoId });
            });

            // ---------- CON-FIN ----------
            modelBuilder.Entity<JornadaFin>(entity =>
            {
                entity.HasKey(j => j.Id);
                entity.ToTable("con_fin_jornada");
                entity.Property(j => j.Status).HasMaxLength(30);
                entity.HasIndex(j => new { j.TenantId, j.OportunidadeId });
            });

            modelBuilder.Entity<SimulacaoFin>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.ToTable("con_fin_simulacao");
                entity.Property(s => s.ChaveIdempotencia).HasMaxLength(100);
                entity.Property(s => s.PrazoUnidade).HasMaxLength(20);
                entity.Property(s => s.OrigemVersao).HasMaxLength(60);
                // Resultado do motor F&I (NF-01)
                entity.Property(s => s.Sistema).HasMaxLength(10);
                entity.Property(s => s.TaxaJurosMensal).HasPrecision(18, 8);
                entity.Property(s => s.ValorParcela).HasPrecision(18, 2);
                entity.Property(s => s.TotalPago).HasPrecision(18, 2);
                entity.Property(s => s.TotalJuros).HasPrecision(18, 2);
                entity.Property(s => s.Iof).HasPrecision(18, 2);
                entity.Property(s => s.CetAnual).HasPrecision(18, 6);
                entity.HasIndex(s => new { s.TenantId, s.ChaveIdempotencia }).IsUnique();
            });

            modelBuilder.Entity<ContratoFin>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.ToTable("con_fin_contrato");
                entity.Property(c => c.NumeroContrato).HasMaxLength(60);
                entity.Property(c => c.Status).HasMaxLength(30);
                entity.HasIndex(c => new { c.TenantId, c.VendaId }).IsUnique();
            });

            // ---------- Outbox ----------
            modelBuilder.Entity<OutboxMessage>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.ToTable("outbox_messages");
                entity.Property(m => m.EventType).HasMaxLength(200);
                entity.HasIndex(m => m.ProcessadoEm);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
