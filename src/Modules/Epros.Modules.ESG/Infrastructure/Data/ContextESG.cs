using Epros.Infrastructure.Data;
using Epros.Modules.ESG.Domain.Entities;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.ESG.Infrastructure.Data
{
    public class ContextESG : ContextBase
    {
        // Nucleo pre-existente
        public DbSet<EmissaoCarbono> EmissoesCarbono => Set<EmissaoCarbono>();
        public DbSet<RelatorioESG> RelatoriosESG => Set<RelatorioESG>();

        // ESG-GHG (Pegada de Carbono)
        public DbSet<InventarioGee> InventariosGee => Set<InventarioGee>();
        public DbSet<FonteEmissaoGee> FontesEmissaoGee => Set<FonteEmissaoGee>();
        public DbSet<DadoAtividadeGee> DadosAtividadeGee => Set<DadoAtividadeGee>();
        public DbSet<FatorEmissaoGee> FatoresEmissaoGee => Set<FatorEmissaoGee>();
        public DbSet<CalculoGee> CalculosGee => Set<CalculoGee>();
        public DbSet<ConsolidacaoGee> ConsolidacoesGee => Set<ConsolidacaoGee>();

        // ESG-EHS (Gestao Ambiental EHS)
        public DbSet<RegistroEhs> RegistrosEhs => Set<RegistroEhs>();
        public DbSet<AtividadeOcupacional> AtividadesOcupacionais => Set<AtividadeOcupacional>();
        public DbSet<FatorRisco> FatoresRisco => Set<FatorRisco>();
        public DbSet<Cat> Cats => Set<Cat>();
        public DbSet<LicencaAmbiental> LicencasAmbientais => Set<LicencaAmbiental>();
        public DbSet<Condicionante> Condicionantes => Set<Condicionante>();
        public DbSet<ResiduoMovimento> ResiduosMovimento => Set<ResiduoMovimento>();
        public DbSet<Incidente> Incidentes => Set<Incidente>();
        public DbSet<AcaoCorretiva> AcoesCorretivas => Set<AcaoCorretiva>();

        // ESG-REL (Relatorios ESG)
        public DbSet<FrameworkRel> FrameworksRel => Set<FrameworkRel>();
        public DbSet<RequisitoRel> RequisitosRel => Set<RequisitoRel>();
        public DbSet<ItemRelatorioEsg> ItensRelatorioEsg => Set<ItemRelatorioEsg>();
        public DbSet<IndicadorReferencia> IndicadoresReferencia => Set<IndicadorReferencia>();
        public DbSet<SnapshotIndicador> SnapshotsIndicador => Set<SnapshotIndicador>();
        public DbSet<PendenciaRelatorio> PendenciasRelatorio => Set<PendenciaRelatorio>();

        // ESG-ECO (Economia Circular)
        public DbSet<DevolucaoEco> DevolucoesEco => Set<DevolucaoEco>();
        public DbSet<DevolucaoItemEco> DevolucoesItensEco => Set<DevolucaoItemEco>();
        public DbSet<FluxoCircular> FluxosCirculares => Set<FluxoCircular>();
        public DbSet<TriagemEco> TriagensEco => Set<TriagemEco>();
        public DbSet<DestinoEco> DestinosEco => Set<DestinoEco>();
        public DbSet<MetaCircular> MetasCirculares => Set<MetaCircular>();
        public DbSet<MedicaoCircular> MedicoesCirculares => Set<MedicaoCircular>();

        // ESG-DIV (Diversidade e Responsabilidade Social)
        public DbSet<DivPrograma> ProgramasDiv => Set<DivPrograma>();
        public DbSet<DivIndicador> IndicadoresDiv => Set<DivIndicador>();
        public DbSet<DivMedicao> MedicoesDiv => Set<DivMedicao>();
        public DbSet<DivMeta> MetasDiv => Set<DivMeta>();
        public DbSet<DivAcao> AcoesDiv => Set<DivAcao>();
        public DbSet<DivParametro> ParametrosDiv => Set<DivParametro>();

        // ESG-TSU (Transporte Sustentavel)
        public DbSet<TsuRegistro> RegistrosTsu => Set<TsuRegistro>();
        public DbSet<TsuOperacao> OperacoesTsu => Set<TsuOperacao>();
        public DbSet<TsuTrecho> TrechosTsu => Set<TsuTrecho>();
        public DbSet<TsuCalculo> CalculosTsu => Set<TsuCalculo>();
        public DbSet<TsuMetaModal> MetasModalTsu => Set<TsuMetaModal>();
        public DbSet<TsuMedicao> MedicoesModalTsu => Set<TsuMedicao>();
        public DbSet<TsuParametro> ParametrosTsu => Set<TsuParametro>();

        public ContextESG(
            DbContextOptions<ContextESG> options,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser) : base(options, tenantProvider, currentUser)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("esg");

            modelBuilder.Entity<EmissaoCarbono>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("emissoes_carbono");
                entity.Property(e => e.FatorCodigo).HasMaxLength(30);
                entity.Property(e => e.FatorVersao).HasMaxLength(30);
                entity.Property(e => e.FatorFonte).HasMaxLength(200);
                entity.HasIndex(e => new { e.TenantId, e.DataTransacao });
                entity.HasIndex(e => new { e.TenantId, e.FatorPendente });
            });

            modelBuilder.Entity<RelatorioESG>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.ToTable("relatorios_esg");
                entity.HasIndex(r => new { r.TenantId, r.AnoFiscal }).IsUnique();
            });

            // ================= ESG-GHG =================
            modelBuilder.Entity<InventarioGee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("ghg_inventario");
                entity.Property(e => e.Codigo).HasMaxLength(30);
                entity.Property(e => e.CriterioFronteira).HasMaxLength(200);
                entity.Property(e => e.Metodologia).HasMaxLength(200);
                entity.Property(e => e.Observacao).HasMaxLength(1000);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.HasIndex(e => new { e.TenantId, e.Codigo, e.Versao }).IsUnique();
            });

            modelBuilder.Entity<FonteEmissaoGee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("ghg_fonte_emissao");
                entity.Property(e => e.Codigo).HasMaxLength(30);
                entity.Property(e => e.Descricao).HasMaxLength(200);
                entity.Property(e => e.Categoria).HasMaxLength(100);
                entity.HasIndex(e => new { e.TenantId, e.InventarioId, e.Escopo, e.Categoria });
            });

            modelBuilder.Entity<DadoAtividadeGee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("ghg_dado_atividade");
                entity.Property(e => e.Unidade).HasMaxLength(30);
                entity.Property(e => e.OrigemDado).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.ReferenciaOperacional).HasMaxLength(100);
                entity.HasIndex(e => new { e.TenantId, e.FonteEmissaoId });
            });

            modelBuilder.Entity<FatorEmissaoGee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("ghg_fator_emissao");
                entity.Property(e => e.Codigo).HasMaxLength(30);
                entity.Property(e => e.Versao).HasMaxLength(30);
                entity.Property(e => e.FonteReferencia).HasMaxLength(200);
                entity.Property(e => e.UnidadeEntrada).HasMaxLength(30);
                entity.Property(e => e.UnidadeSaida).HasMaxLength(30);
                entity.HasIndex(e => new { e.TenantId, e.Codigo, e.Versao }).IsUnique();
            });

            modelBuilder.Entity<CalculoGee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("ghg_calculo");
                entity.Property(e => e.FormulaVersao).HasMaxLength(30);
                entity.Property(e => e.MemoriaCalculo).HasMaxLength(2000);
                entity.HasIndex(e => new { e.TenantId, e.DadoAtividadeId });
            });

            modelBuilder.Entity<ConsolidacaoGee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("ghg_consolidacao");
                entity.Property(e => e.Dimensao).HasMaxLength(100);
                entity.HasIndex(e => new { e.TenantId, e.InventarioId });
            });

            // ================= ESG-EHS =================
            modelBuilder.Entity<RegistroEhs>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("ehs_registro");
                entity.Property(e => e.Codigo).HasMaxLength(30);
                entity.Property(e => e.Descricao).HasMaxLength(500);
                entity.Property(e => e.Tipo).HasMaxLength(50);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.HasIndex(e => new { e.TenantId, e.Codigo }).IsUnique();
            });

            modelBuilder.Entity<AtividadeOcupacional>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("ehs_atividade");
                entity.Property(e => e.Descricao).HasMaxLength(500);
                entity.HasIndex(e => new { e.TenantId, e.RegistroEhsId });
            });

            modelBuilder.Entity<FatorRisco>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("ehs_fator_risco");
                entity.Property(e => e.Tipo).HasMaxLength(50);
                entity.Property(e => e.FatorRiscoDescricao).HasMaxLength(200);
                entity.Property(e => e.Intensidade).HasMaxLength(50);
                entity.Property(e => e.TecnicaUtilizada).HasMaxLength(100);
                entity.Property(e => e.EpcEficaz).HasMaxLength(30);
                entity.Property(e => e.EpiEficaz).HasMaxLength(30);
                entity.Property(e => e.CaEpi).HasMaxLength(30);
                entity.Property(e => e.AtendimentoNr061).HasMaxLength(30);
                entity.Property(e => e.AtendimentoNr062).HasMaxLength(30);
                entity.Property(e => e.AtendimentoNr063).HasMaxLength(30);
                entity.Property(e => e.AtendimentoNr064).HasMaxLength(30);
                entity.Property(e => e.AtendimentoNr065).HasMaxLength(30);
                entity.HasIndex(e => new { e.TenantId, e.AtividadeId });
            });

            modelBuilder.Entity<Cat>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("ehs_cat");
                entity.Property(e => e.NumeroCat).HasMaxLength(30);
                entity.HasIndex(e => new { e.TenantId, e.NumeroCat });
            });

            modelBuilder.Entity<LicencaAmbiental>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("ehs_licenca");
                entity.Property(e => e.Tipo).HasMaxLength(50);
                entity.Property(e => e.Numero).HasMaxLength(50);
                entity.Property(e => e.Autoridade).HasMaxLength(200);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.HasIndex(e => new { e.TenantId, e.Numero, e.Autoridade });
            });

            modelBuilder.Entity<Condicionante>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("ehs_condicionante");
                entity.Property(e => e.Codigo).HasMaxLength(30);
                entity.Property(e => e.Descricao).HasMaxLength(500);
                entity.Property(e => e.Periodicidade).HasMaxLength(50);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.HasIndex(e => new { e.TenantId, e.LicencaId });
            });

            modelBuilder.Entity<ResiduoMovimento>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("ehs_residuo_movimento");
                entity.Property(e => e.TipoResiduo).HasMaxLength(100);
                entity.Property(e => e.Classificacao).HasMaxLength(50);
                entity.Property(e => e.Origem).HasMaxLength(100);
                entity.Property(e => e.Unidade).HasMaxLength(30);
                entity.Property(e => e.TipoMovimento).HasMaxLength(50);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.HasIndex(e => new { e.TenantId, e.RegistroEhsId });
            });

            modelBuilder.Entity<Incidente>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("ehs_incidente");
                entity.Property(e => e.Tipo).HasMaxLength(50);
                entity.Property(e => e.Descricao).HasMaxLength(1000);
                entity.Property(e => e.Gravidade).HasMaxLength(50);
                entity.Property(e => e.Impacto).HasMaxLength(500);
                entity.Property(e => e.NumeroCat).HasMaxLength(30);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.HasIndex(e => new { e.TenantId, e.RegistroEhsId });
            });

            modelBuilder.Entity<AcaoCorretiva>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("ehs_acao_corretiva");
                entity.Property(e => e.Descricao).HasMaxLength(1000);
                entity.Property(e => e.Causa).HasMaxLength(500);
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.Eficacia).HasMaxLength(200);
                entity.HasIndex(e => new { e.TenantId, e.IncidenteId });
            });

            // ================= ESG-REL =================
            modelBuilder.Entity<FrameworkRel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("rel_framework");
                entity.Property(e => e.Codigo).HasMaxLength(50);
                entity.Property(e => e.Versao).HasMaxLength(30);
                entity.Property(e => e.Descricao).HasMaxLength(500);
                entity.HasIndex(e => new { e.TenantId, e.Codigo, e.Versao }).IsUnique();
            });

            modelBuilder.Entity<RequisitoRel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("rel_requisito");
                entity.Property(e => e.Codigo).HasMaxLength(50);
                entity.Property(e => e.Titulo).HasMaxLength(500);
                entity.Property(e => e.TipoResposta).HasMaxLength(30);
                entity.HasIndex(e => new { e.TenantId, e.FrameworkId, e.Codigo }).IsUnique();
            });

            modelBuilder.Entity<ItemRelatorioEsg>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("rel_item");
                entity.Property(e => e.Observacao).HasMaxLength(4000);
                entity.Property(e => e.TipoConteudo).HasMaxLength(30);
                entity.Property(e => e.StatusPreenchimento).HasMaxLength(30);
                entity.Property(e => e.Justificativa).HasMaxLength(2000);
                entity.HasIndex(e => new { e.TenantId, e.RelatorioId, e.Sequencia }).IsUnique();
            });

            modelBuilder.Entity<IndicadorReferencia>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("rel_indicador_referencia");
                entity.Property(e => e.OrigemDominio).HasMaxLength(50);
                entity.Property(e => e.OrigemEntidade).HasMaxLength(100);
                entity.Property(e => e.OrigemId).HasMaxLength(100);
                entity.Property(e => e.CodigoIndicador).HasMaxLength(50);
                entity.Property(e => e.RegraComposicao).HasMaxLength(200);
                entity.HasIndex(e => new { e.TenantId, e.ItemId });
            });

            modelBuilder.Entity<SnapshotIndicador>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("rel_snapshot");
                entity.Property(e => e.OrigemVersao).HasMaxLength(50);
                entity.Property(e => e.ValorTexto).HasMaxLength(4000);
                entity.Property(e => e.Unidade).HasMaxLength(30);
                entity.Property(e => e.Dimensoes).HasMaxLength(2000);
                entity.Property(e => e.StatusOrigem).HasMaxLength(30);
                entity.Property(e => e.HashConteudo).HasMaxLength(64);
                entity.HasIndex(e => new { e.TenantId, e.IndicadorReferenciaId });
            });

            modelBuilder.Entity<PendenciaRelatorio>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("rel_pendencia");
                entity.Property(e => e.Criticidade).HasMaxLength(20);
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.Resolucao).HasMaxLength(2000);
                entity.HasIndex(e => new { e.TenantId, e.RelatorioId });
            });

            // ================= ESG-ECO =================
            modelBuilder.Entity<DevolucaoEco>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("eco_devolucao");
                entity.Property(e => e.Motivo).HasMaxLength(500);
                entity.Property(e => e.Observacao).HasMaxLength(1000);
                entity.Property(e => e.Estado).HasMaxLength(30);
                entity.Property(e => e.ChaveNfEntrada).HasMaxLength(44);
                entity.Property(e => e.NumeroNf).HasMaxLength(20);
                entity.Property(e => e.ChaveGerada).HasMaxLength(44);
                entity.Property(e => e.NumeroGerado).HasMaxLength(20);
                entity.Property(e => e.Tipo).HasMaxLength(30);
                entity.Property(e => e.TransportadoraNome).HasMaxLength(200);
                entity.Property(e => e.TransportadoraCidade).HasMaxLength(100);
                entity.Property(e => e.TransportadoraUf).HasMaxLength(2);
                entity.Property(e => e.TransportadoraCpfCnpj).HasMaxLength(20);
                entity.Property(e => e.TransportadoraIe).HasMaxLength(20);
                entity.Property(e => e.TransportadoraEndereco).HasMaxLength(300);
                entity.Property(e => e.FreteEspecie).HasMaxLength(50);
                entity.Property(e => e.FreteMarca).HasMaxLength(50);
                entity.Property(e => e.FreteNumero).HasMaxLength(30);
                entity.Property(e => e.FreteTipo).HasMaxLength(30);
                entity.Property(e => e.VeiculoPlaca).HasMaxLength(10);
                entity.Property(e => e.VeiculoUf).HasMaxLength(2);
                entity.HasIndex(e => new { e.TenantId, e.ChaveNfEntrada });
                entity.HasMany(e => e.Itens)
                    .WithOne()
                    .HasForeignKey(i => i.DevolucaoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<DevolucaoItemEco>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("eco_devolucao_item");
                entity.Property(e => e.Codigo).HasMaxLength(60);
                entity.Property(e => e.Nome).HasMaxLength(200);
                entity.Property(e => e.Ncm).HasMaxLength(10);
                entity.Property(e => e.Cfop).HasMaxLength(4);
                entity.Property(e => e.CodigoBarras).HasMaxLength(30);
                entity.Property(e => e.UnidadeMedida).HasMaxLength(10);
                entity.Property(e => e.CstCsosn).HasMaxLength(4);
                entity.Property(e => e.CstPis).HasMaxLength(4);
                entity.Property(e => e.CstCofins).HasMaxLength(4);
                entity.Property(e => e.CstIpi).HasMaxLength(4);
                entity.Property(e => e.CodigoAnp).HasMaxLength(20);
                entity.Property(e => e.DescricaoAnp).HasMaxLength(100);
                entity.Property(e => e.UfConsumo).HasMaxLength(2);
                entity.Property(e => e.UnidadeTributavel).HasMaxLength(10);
                entity.Property(e => e.ModalidadeBcSt).HasMaxLength(10);
                entity.Property(e => e.Origem).HasMaxLength(4);
                entity.HasIndex(e => new { e.TenantId, e.DevolucaoId });
            });

            modelBuilder.Entity<FluxoCircular>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("eco_fluxo");
                entity.Property(e => e.Codigo).HasMaxLength(30);
                entity.Property(e => e.Descricao).HasMaxLength(500);
                entity.Property(e => e.Tipo).HasMaxLength(50);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.HasIndex(e => new { e.TenantId, e.Codigo }).IsUnique();
            });

            modelBuilder.Entity<TriagemEco>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("eco_triagem");
                entity.Property(e => e.Unidade).HasMaxLength(30);
                entity.Property(e => e.Condicao).HasMaxLength(50);
                entity.Property(e => e.DestinoProposto).HasMaxLength(50);
                entity.Property(e => e.Motivo).HasMaxLength(500);
                entity.HasIndex(e => new { e.TenantId, e.FluxoId });
            });

            modelBuilder.Entity<DestinoEco>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("eco_destino");
                entity.Property(e => e.TipoDestino).HasMaxLength(50);
                entity.Property(e => e.Unidade).HasMaxLength(30);
                entity.Property(e => e.Observacao).HasMaxLength(500);
                entity.HasIndex(e => new { e.TenantId, e.TriagemId });
            });

            modelBuilder.Entity<MetaCircular>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("eco_meta");
                entity.Property(e => e.TipoIndicador).HasMaxLength(50);
                entity.Property(e => e.Unidade).HasMaxLength(30);
                entity.Property(e => e.Formula).HasMaxLength(200);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.HasIndex(e => new { e.TenantId, e.FluxoId });
            });

            modelBuilder.Entity<MedicaoCircular>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("eco_medicao");
                entity.Property(e => e.TipoIndicador).HasMaxLength(50);
                entity.Property(e => e.Periodo).HasMaxLength(20);
                entity.Property(e => e.Unidade).HasMaxLength(30);
                entity.Property(e => e.Fonte).HasMaxLength(100);
                entity.HasIndex(e => new { e.TenantId, e.FluxoId });
            });

            // ================= ESG-DIV =================
            modelBuilder.Entity<DivPrograma>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("div_programa");
                entity.Property(e => e.Codigo).HasMaxLength(30);
                entity.Property(e => e.Descricao).HasMaxLength(500);
                entity.Property(e => e.Observacao).HasMaxLength(1000);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.HasIndex(e => new { e.TenantId, e.Codigo }).IsUnique();
            });

            modelBuilder.Entity<DivIndicador>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("div_indicador");
                entity.Property(e => e.Codigo).HasMaxLength(50);
                entity.Property(e => e.Descricao).HasMaxLength(500);
                entity.Property(e => e.Unidade).HasMaxLength(30);
                entity.Property(e => e.Fonte).HasMaxLength(200);
                entity.Property(e => e.Formula).HasMaxLength(500);
                entity.HasIndex(e => new { e.TenantId, e.Codigo, e.Versao }).IsUnique();
            });

            modelBuilder.Entity<DivMedicao>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("div_medicao");
                entity.Property(e => e.Dimensao).HasMaxLength(100);
                entity.Property(e => e.DimensoesHash).HasMaxLength(64);
                entity.Property(e => e.Origem).HasMaxLength(100);
                entity.HasIndex(e => new { e.TenantId, e.IndicadorId, e.DimensoesHash }).IsUnique();
            });

            modelBuilder.Entity<DivMeta>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("div_meta");
                entity.Property(e => e.Unidade).HasMaxLength(30);
                entity.HasIndex(e => new { e.TenantId, e.IndicadorId });
            });

            modelBuilder.Entity<DivAcao>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("div_acao");
                entity.Property(e => e.Descricao).HasMaxLength(1000);
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.Resultado).HasMaxLength(1000);
                entity.HasIndex(e => new { e.TenantId, e.ProgramaId });
            });

            modelBuilder.Entity<DivParametro>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("div_parametro");
                entity.Property(e => e.Chave).HasMaxLength(100);
                entity.Property(e => e.ValorJson).HasMaxLength(2000);
                entity.HasIndex(e => new { e.TenantId, e.Chave }).IsUnique();
            });

            // ================= ESG-TSU =================
            modelBuilder.Entity<TsuRegistro>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("tsu_registro");
                entity.Property(e => e.Codigo).HasMaxLength(30);
                entity.Property(e => e.Descricao).HasMaxLength(500);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.HasIndex(e => new { e.TenantId, e.Codigo }).IsUnique();
            });

            modelBuilder.Entity<TsuOperacao>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("tsu_operacao");
                entity.Property(e => e.Referencia).HasMaxLength(60);
                entity.Property(e => e.Modal).HasMaxLength(30);
                entity.HasIndex(e => new { e.TenantId, e.RegistroTsuId });
            });

            modelBuilder.Entity<TsuTrecho>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("tsu_trecho");
                entity.Property(e => e.Modal).HasMaxLength(30);
                entity.Property(e => e.UnidadeDistancia).HasMaxLength(10);
                entity.Property(e => e.UnidadeMassa).HasMaxLength(10);
                entity.Property(e => e.UnidadeEnergia).HasMaxLength(20);
                entity.HasIndex(e => new { e.TenantId, e.OperacaoId });
            });

            modelBuilder.Entity<TsuCalculo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("tsu_calculo");
                entity.Property(e => e.FormulaVersao).HasMaxLength(30);
                entity.Property(e => e.FatorCodigo).HasMaxLength(30);
                entity.Property(e => e.FatorVersao).HasMaxLength(30);
                entity.Property(e => e.FatorFonte).HasMaxLength(200);
                entity.Property(e => e.MemoriaCalculo).HasMaxLength(2000);
                entity.HasIndex(e => new { e.TenantId, e.TrechoId });
                entity.HasIndex(e => new { e.TenantId, e.FatorPendente });
            });

            modelBuilder.Entity<TsuMetaModal>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("tsu_meta_modal");
                entity.Property(e => e.Modal).HasMaxLength(30);
                entity.HasIndex(e => new { e.TenantId, e.RegistroTsuId });
            });

            modelBuilder.Entity<TsuMedicao>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("tsu_medicao");
                entity.Property(e => e.Periodo).HasMaxLength(20);
                entity.HasIndex(e => new { e.TenantId, e.MetaModalId });
            });

            modelBuilder.Entity<TsuParametro>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("tsu_parametro");
                entity.Property(e => e.Chave).HasMaxLength(100);
                entity.Property(e => e.ValorJson).HasMaxLength(2000);
                entity.HasIndex(e => new { e.TenantId, e.Chave }).IsUnique();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
