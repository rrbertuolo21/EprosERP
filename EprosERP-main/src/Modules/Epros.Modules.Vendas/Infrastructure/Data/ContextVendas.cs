using Epros.Infrastructure.Data;
using Epros.Modules.Vendas.Domain.Entities;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Vendas.Infrastructure.Data
{
    public class ContextVendas : ContextBase
    {
        public DbSet<Venda> Vendas => Set<Venda>();
        public DbSet<VendaItem> VendaItens => Set<VendaItem>();
        public DbSet<Caixa> Caixas => Set<Caixa>();
        public DbSet<CaixaMovimento> CaixaMovimentos => Set<CaixaMovimento>();

        // Fiscal por item
        public DbSet<VendaItemImposto> VendaItemImpostos => Set<VendaItemImposto>();
        public DbSet<VendaItemImpostoIbsCbs> VendaItemImpostosIbsCbs => Set<VendaItemImpostoIbsCbs>();
        public DbSet<VendaItemImpostoIbsCbsTributacaoRegular> VendaItemImpostoIbsCbsTributacoesRegulares => Set<VendaItemImpostoIbsCbsTributacaoRegular>();
        public DbSet<VendaItemImpostoValorAproximado> VendaItemImpostosValorAproximado => Set<VendaItemImpostoValorAproximado>();
        public DbSet<VendaItemCombustivel> VendaItemCombustiveis => Set<VendaItemCombustivel>();
        public DbSet<VendaItemCombustivelOrigem> VendaItemCombustivelOrigens => Set<VendaItemCombustivelOrigem>();

        // Totais / impostos da venda
        public DbSet<VendaTotal> VendaTotais => Set<VendaTotal>();
        public DbSet<VendaTotalIbsCbs> VendaTotaisIbsCbs => Set<VendaTotalIbsCbs>();
        public DbSet<VendaImposto> VendaImpostos => Set<VendaImposto>();

        // Emitente / destinatário / endereços
        public DbSet<VendaEmitente> VendaEmitentes => Set<VendaEmitente>();
        public DbSet<VendaEmitenteEndereco> VendaEmitenteEnderecos => Set<VendaEmitenteEndereco>();
        public DbSet<VendaDestinatario> VendaDestinatarios => Set<VendaDestinatario>();
        public DbSet<VendaDestinatarioEndereco> VendaDestinatarioEnderecos => Set<VendaDestinatarioEndereco>();
        public DbSet<VendaEntrega> VendaEntregas => Set<VendaEntrega>();
        public DbSet<VendaCobrancaEndereco> VendaCobrancaEnderecos => Set<VendaCobrancaEndereco>();

        // Transporte
        public DbSet<VendaTransporte> VendaTransportes => Set<VendaTransporte>();
        public DbSet<VendaTransporteTransportadora> VendaTransporteTransportadoras => Set<VendaTransporteTransportadora>();
        public DbSet<VendaTransporteVeiculo> VendaTransporteVeiculos => Set<VendaTransporteVeiculo>();
        public DbSet<VendaTransporteReboque> VendaTransporteReboques => Set<VendaTransporteReboque>();
        public DbSet<VendaTransporteVolume> VendaTransporteVolumes => Set<VendaTransporteVolume>();

        // Configuração
        public DbSet<VendaConfiguracao> VendaConfiguracoes => Set<VendaConfiguracao>();

        // Pagamento
        public DbSet<VendaPagamento> VendaPagamentos => Set<VendaPagamento>();

        // NF-e / NFC-e
        public DbSet<VendaNfce> VendaNfces => Set<VendaNfce>();
        public DbSet<VendaNfe> VendaNfes => Set<VendaNfe>();
        public DbSet<VendaNfeCartaCorrecao> VendaNfeCartasCorrecao => Set<VendaNfeCartaCorrecao>();
        public DbSet<VendaNfeExportacao> VendaNfeExportacoes => Set<VendaNfeExportacao>();
        public DbSet<VendaNfeIntermediador> VendaNfeIntermediadores => Set<VendaNfeIntermediador>();
        public DbSet<VendaNfeReferenciada> VendaNfeReferenciadas => Set<VendaNfeReferenciada>();

        // Fatura
        public DbSet<VendaFatura> VendaFaturas => Set<VendaFatura>();
        public DbSet<VendaFaturaDuplicata> VendaFaturaDuplicatas => Set<VendaFaturaDuplicata>();

        // Autorizações / histórico
        public DbSet<VendaAutorizacaoXml> VendaAutorizacoesXml => Set<VendaAutorizacaoXml>();
        public DbSet<VendaNfHistorico> VendaNfHistoricos => Set<VendaNfHistorico>();
        public DbSet<VendaNfeHistorico> VendaNfeHistoricos => Set<VendaNfeHistorico>();

        // Gestão de Serviços (VEN-GSV)
        public DbSet<ServicoCatalogo> ServicoCatalogos => Set<ServicoCatalogo>();
        public DbSet<ServicoFatura> ServicoFaturas => Set<ServicoFatura>();
        public DbSet<ServicoFaturaLinha> ServicoFaturaLinhas => Set<ServicoFaturaLinha>();
        public DbSet<ServicoLancamentoFinanceiroRef> ServicoLancamentoFinanceiroRefs => Set<ServicoLancamentoFinanceiroRef>();
        public DbSet<ServicoHistorico> ServicoHistoricos => Set<ServicoHistorico>();

        // CRM (VEN-CRM)
        public DbSet<CrmPipeline> CrmPipelines => Set<CrmPipeline>();
        public DbSet<CrmEtapa> CrmEtapas => Set<CrmEtapa>();
        public DbSet<CrmEtiqueta> CrmEtiquetas => Set<CrmEtiqueta>();
        public DbSet<CrmOrigem> CrmOrigens => Set<CrmOrigem>();
        public DbSet<CrmLead> CrmLeads => Set<CrmLead>();
        public DbSet<CrmLeadUsuario> CrmLeadUsuarios => Set<CrmLeadUsuario>();
        public DbSet<CrmOportunidade> CrmOportunidades => Set<CrmOportunidade>();
        public DbSet<CrmOportunidadeParticipante> CrmOportunidadeParticipantes => Set<CrmOportunidadeParticipante>();
        public DbSet<CrmAtividade> CrmAtividades => Set<CrmAtividade>();
        public DbSet<CrmCampanha> CrmCampanhas => Set<CrmCampanha>();
        public DbSet<CrmListaPublico> CrmListasPublico => Set<CrmListaPublico>();
        public DbSet<CrmListaPublicoMembro> CrmListaPublicoMembros => Set<CrmListaPublicoMembro>();
        public DbSet<CrmCampanhaLista> CrmCampanhaListas => Set<CrmCampanhaLista>();
        public DbSet<CrmCampanhaRastreador> CrmCampanhaRastreadores => Set<CrmCampanhaRastreador>();
        public DbSet<CrmCampanhaLog> CrmCampanhaLogs => Set<CrmCampanhaLog>();
        public DbSet<CrmWebform> CrmWebforms => Set<CrmWebform>();
        public DbSet<CrmWebformResponsavel> CrmWebformResponsaveis => Set<CrmWebformResponsavel>();
        public DbSet<CrmTicket> CrmTickets => Set<CrmTicket>();
        public DbSet<CrmTicketStatus> CrmTicketStatus => Set<CrmTicketStatus>();
        public DbSet<CrmTicketResposta> CrmTicketRespostas => Set<CrmTicketResposta>();
        public DbSet<CrmClienteFidelizado> CrmClientesFidelizados => Set<CrmClienteFidelizado>();
        public DbSet<CrmPontuacaoFidelizacao> CrmPontuacoesFidelizacao => Set<CrmPontuacaoFidelizacao>();
        public DbSet<CrmConfiguracaoPixRelacional> CrmConfiguracoesPixRelacional => Set<CrmConfiguracaoPixRelacional>();
        public DbSet<CrmPagamentoPixRelacional> CrmPagamentosPixRelacional => Set<CrmPagamentoPixRelacional>();
        public DbSet<CrmHistorico> CrmHistoricos => Set<CrmHistorico>();

        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        // Lookups read-only cross-module (Guid FK) — ver VendasLookups.cs e seção 6.4 da convenção.
        public DbSet<ProdutoLookup> ProdutosLookup => Set<ProdutoLookup>();
        public DbSet<NcmConfiguracaoLookup> NcmConfiguracoesLookup => Set<NcmConfiguracaoLookup>();
        public DbSet<NcmTributacaoLookup> NcmTributacoesLookup => Set<NcmTributacaoLookup>();
        public DbSet<EmpresaLookup> EmpresasLookup => Set<EmpresaLookup>();

        public ContextVendas(
            DbContextOptions<ContextVendas> options,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser) : base(options, tenantProvider, currentUser)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("vendas");

            modelBuilder.Entity<Venda>(entity =>
            {
                entity.HasKey(v => v.Id);
                entity.Property(v => v.Total).HasPrecision(18, 2);
                // Status é enum tipado (EVendaStatus) — armazenado como int pelo EF.
                // Enums fiscais nullable armazenados como int (coluna int?), valores 1:1 com o legado.
                entity.Property(v => v.ModeloFiscal).HasConversion<int?>();
                entity.Property(v => v.ModalidadeFrete).HasConversion<int?>();
                entity.Property(v => v.VendaOrigem).HasConversion<int?>();
                entity.Property(v => v.CaixaId).HasMaxLength(100);

                entity.Property(v => v.NaturezaOperacao).HasMaxLength(60);
                entity.Property(v => v.InformacoesComplementares).HasMaxLength(5000);
                entity.Property(v => v.InformacoesAdicionaisFisco).HasMaxLength(2000);
                entity.Property(v => v.ValorDesconto).HasPrecision(18, 2);
                entity.Property(v => v.ValorFrete).HasPrecision(18, 2);
                entity.Property(v => v.FormaPagamento).HasMaxLength(50);
                entity.Property(v => v.CaminhoPdfCupomNaoFiscal).HasMaxLength(500);

                entity.HasMany(v => v.Itens)
                      .WithOne(i => i.Venda)
                      .HasForeignKey(i => i.VendaId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(v => v.Pagamentos)
                      .WithOne(p => p.Venda)
                      .HasForeignKey(p => p.VendaId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(v => v.AutorizacoesXml)
                      .WithOne(a => a.Venda)
                      .HasForeignKey(a => a.VendaId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(v => v.Referenciadas)
                      .WithOne(r => r.Venda)
                      .HasForeignKey(r => r.VendaId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(v => v.NfHistoricos)
                      .WithOne(h => h.Venda)
                      .HasForeignKey(h => h.VendaId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(v => v.Emitente).WithOne(e => e.Venda).HasForeignKey<VendaEmitente>(e => e.VendaId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(v => v.Destinatario).WithOne(d => d.Venda).HasForeignKey<VendaDestinatario>(d => d.VendaId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(v => v.Transporte).WithOne(t => t.Venda).HasForeignKey<VendaTransporte>(t => t.VendaId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(v => v.Total_).WithOne(t => t.Venda).HasForeignKey<VendaTotal>(t => t.VendaId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(v => v.Configuracao).WithOne(c => c.Venda).HasForeignKey<VendaConfiguracao>(c => c.VendaId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(v => v.Nfce).WithOne(n => n.Venda).HasForeignKey<VendaNfce>(n => n.VendaId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(v => v.Nfe).WithOne(n => n.Venda).HasForeignKey<VendaNfe>(n => n.VendaId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(v => v.Fatura).WithOne(f => f.Venda).HasForeignKey<VendaFatura>(f => f.VendaId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(v => v.Imposto).WithOne(i => i.Venda).HasForeignKey<VendaImposto>(i => i.VendaId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(v => v.TotalIbsCbs).WithOne(t => t.Venda).HasForeignKey<VendaTotalIbsCbs>(t => t.VendaId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(v => v.VendaEntrega).WithOne(e => e.Venda).HasForeignKey<VendaEntrega>(e => e.VendaId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(v => v.VendaCobrancaEndereco).WithOne(c => c.Venda).HasForeignKey<VendaCobrancaEndereco>(c => c.VendaId).OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(v => new { v.TenantId, v.CriadoEm }).HasDatabaseName("ix_vendas_tenant_criado_em");
                entity.HasIndex(v => v.SyncId).IsUnique().HasDatabaseName("uq_vendas_sync_id");
            });

            modelBuilder.Entity<VendaItem>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.PrecoUnitario).HasPrecision(18, 2);
                entity.Property(i => i.ValorTotal).HasPrecision(18, 2);
                entity.Property(i => i.Quantidade).HasPrecision(18, 4);

                entity.Property(i => i.CodigoProduto).HasMaxLength(60);
                entity.Property(i => i.CodigoEan).HasMaxLength(14);
                entity.Property(i => i.DescricaoProduto).HasMaxLength(120);
                entity.Property(i => i.Ncm).HasMaxLength(8);
                entity.Property(i => i.ExcecaoNcmTipi).HasMaxLength(3);
                entity.Property(i => i.Cest).HasMaxLength(7);
                entity.Property(i => i.CodigoAnp).HasMaxLength(9);
                entity.Property(i => i.UnidadeComercial).HasMaxLength(6);
                entity.Property(i => i.CodigoEanTributavel).HasMaxLength(14);
                entity.Property(i => i.UnidadeTributavel).HasMaxLength(6);
                entity.Property(i => i.InformacoesAdicionaisDoProduto).HasMaxLength(500);
                entity.Property(i => i.NumeroPedidoCompra).HasMaxLength(60);
                entity.Property(i => i.FichaConteudoImportacao).HasMaxLength(36);
                entity.Property(i => i.CodigoBeneficioFiscal).HasMaxLength(36);
                entity.Property(i => i.QuantidadeComercial).HasPrecision(18, 4);
                entity.Property(i => i.ValorUnitarioComercial).HasPrecision(21, 10);
                entity.Property(i => i.ValorTotalBrutoProdutos).HasPrecision(18, 2);
                entity.Property(i => i.QuantidadeTributavel).HasPrecision(18, 4);
                entity.Property(i => i.ValorUnitarioTributavel).HasPrecision(21, 10);
                entity.Property(i => i.ValorDesconto).HasPrecision(18, 2);
                entity.Property(i => i.ValorDescontoRateado).HasPrecision(18, 2);
                entity.Property(i => i.ValorFreteRateado).HasPrecision(18, 2);
                entity.Property(i => i.ValorSeguroRateado).HasPrecision(18, 2);
                entity.Property(i => i.ValorOutrasDepesasAcessoriasRateado).HasPrecision(18, 2);
                entity.Property(i => i.ValorCusto).HasPrecision(18, 2);

                entity.HasOne(i => i.Imposto).WithOne(x => x.VendaItem).HasForeignKey<VendaItemImposto>(x => x.VendaItemId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(i => i.ImpostoValorAproximado).WithOne(x => x.VendaItem).HasForeignKey<VendaItemImpostoValorAproximado>(x => x.VendaItemId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(i => i.Combustivel).WithOne(x => x.VendaItem).HasForeignKey<VendaItemCombustivel>(x => x.VendaItemId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(i => i.ImpostoIbsCbs).WithOne(x => x.VendaItem).HasForeignKey<VendaItemImpostoIbsCbs>(x => x.VendaItemId).OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(i => new { i.TenantId, i.VendaId }).HasDatabaseName("ix_venda_itens_tenant_venda");
                entity.HasIndex(i => i.SyncId).IsUnique().HasDatabaseName("uq_venda_itens_sync_id");
            });

            ConfigurarImpostosItem(modelBuilder);
            ConfigurarTotais(modelBuilder);
            ConfigurarPessoas(modelBuilder);
            ConfigurarTransporte(modelBuilder);
            ConfigurarConfiguracaoEPagamento(modelBuilder);
            ConfigurarNfe(modelBuilder);
            ConfigurarFatura(modelBuilder);
            ConfigurarAutorizacoesEHistorico(modelBuilder);
            ConfigurarCaixa(modelBuilder);
            ConfigurarGestaoServicos(modelBuilder);
            ConfigurarCrm(modelBuilder);

            modelBuilder.Entity<OutboxMessage>(entity =>
            {
                entity.ToTable("outbox_messages", "vendas");
                entity.HasKey(o => o.Id);
            });

            ConfigurarLookups(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        private static void ConfigurarImpostosItem(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VendaItemImposto>(e =>
            {
                e.HasKey(x => x.Id);
                // Todos os valores monetários / bases 15,2 e alíquotas 5,2 -> HasPrecision(18,2/4)
                e.Property(x => x.ValorBaseDeCalculoIcms).HasPrecision(18, 2);
                e.Property(x => x.PercentualReducaoBaseDeCalculoIcms).HasPrecision(18, 4);
                e.Property(x => x.AliquotaIcms).HasPrecision(18, 4);
                e.Property(x => x.ValorImpostoIcms).HasPrecision(18, 2);
                e.Property(x => x.PercentualMvaBaseDeCalculoST).HasPrecision(18, 4);
                e.Property(x => x.PercentualReducaoBaseDeCalculoST).HasPrecision(18, 4);
                e.Property(x => x.ValorBaseDeCalculoSt).HasPrecision(18, 2);
                e.Property(x => x.AliquotaSt).HasPrecision(18, 4);
                e.Property(x => x.ValorImpostoSt).HasPrecision(18, 2);
                e.Property(x => x.ValorBaseDeCalculoStRetido).HasPrecision(18, 2);
                e.Property(x => x.ValorImpostoStRetido).HasPrecision(18, 2);
                e.Property(x => x.PercentualCreditoSimplesNacionalIcms).HasPrecision(18, 4);
                e.Property(x => x.ValorImpostoCreditoSimplesNacionalIcms).HasPrecision(18, 2);
                e.Property(x => x.ValorBaseDeCalculoFcp).HasPrecision(18, 2);
                e.Property(x => x.PercentualFcp).HasPrecision(18, 4);
                e.Property(x => x.ValorImpostoFcp).HasPrecision(18, 2);
                e.Property(x => x.ValorOperacaoDiferimentoIcms).HasPrecision(18, 2);
                e.Property(x => x.PercentualDiferimentoIcms).HasPrecision(18, 4);
                e.Property(x => x.ValorImpostoDiferimentoIcms).HasPrecision(18, 2);
                e.Property(x => x.ValorBaseDeCalculoIpi).HasPrecision(18, 2);
                e.Property(x => x.AliquotaIpi).HasPrecision(18, 4);
                e.Property(x => x.ValorImpostoDiferimentoIpi).HasPrecision(18, 2);
                e.Property(x => x.ValorQuantidadeTotalParaTributacaoIpi).HasPrecision(18, 4);
                e.Property(x => x.ValorPorUnidadeTributavelIpi).HasPrecision(18, 4);
                e.Property(x => x.ValorBaseDeCalculoPis).HasPrecision(18, 2);
                e.Property(x => x.AliquotaPis).HasPrecision(18, 4);
                e.Property(x => x.ValorQuantidadeVendidaProdutoPis).HasPrecision(18, 4);
                e.Property(x => x.AliquotaPorUnidadeVendidaPis).HasPrecision(18, 4);
                e.Property(x => x.ValorImpostoDiferimentoPis).HasPrecision(18, 2);
                e.Property(x => x.ValorBaseDeCalculoCofins).HasPrecision(18, 2);
                e.Property(x => x.AliquotaCofins).HasPrecision(18, 4);
                e.Property(x => x.ValorQuantidadeVendidaProdutoCofins).HasPrecision(18, 4);
                e.Property(x => x.AliquotaPorUnidadeVendidaCofins).HasPrecision(18, 4);
                e.Property(x => x.ValorImpostoDiferimentoCofins).HasPrecision(18, 2);
                e.Property(x => x.ValorBaseDeCalculoFcpSt).HasPrecision(18, 2);
                e.Property(x => x.PercentualFcpSt).HasPrecision(18, 4);
                e.Property(x => x.ValorImpostoFcpSt).HasPrecision(18, 2);
                e.Property(x => x.ValorIcmsProprioSubistituto).HasPrecision(18, 2);
                e.Property(x => x.ValorAliquotaIcmsInterna).HasPrecision(18, 4);
                e.Property(x => x.ValorAliquotaIcmsInternaEstadual).HasPrecision(18, 4);
                e.Property(x => x.ValorReducaoIpiPercentual).HasPrecision(18, 4);
                e.Property(x => x.ValorUnitFixadoIcmsSt).HasPrecision(18, 4);
                e.Property(x => x.ValorBaseDeCalculoDifal).HasPrecision(18, 2);
                e.Property(x => x.ValorImpostoDevidoDifal).HasPrecision(18, 2);
                e.Property(x => x.ValorImpostoDevidoRecolherSt).HasPrecision(18, 2);
                e.Property(x => x.ValorImpostoDevidoFcp).HasPrecision(18, 2);
                e.Property(x => x.ValorIcmsIsento).HasPrecision(18, 2);
                e.Property(x => x.ValorIcmsOutros).HasPrecision(18, 2);
                e.Property(x => x.ValorIpiIsento).HasPrecision(18, 2);
                e.Property(x => x.ValorIpiOutros).HasPrecision(18, 2);
                e.Property(x => x.IcmsObservacao).HasMaxLength(500);
                e.Property(x => x.IpiObservacao).HasMaxLength(500);
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_venda_item_imposto_sync_id");
            });

            modelBuilder.Entity<VendaItemImpostoIbsCbs>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Cst).HasMaxLength(10);
                e.Property(x => x.CClassTrib).HasMaxLength(10);
                e.Property(x => x.AliquotaEstadual).HasPrecision(18, 4);
                e.Property(x => x.AliquotaMunicipal).HasPrecision(18, 4);
                e.Property(x => x.AliquotaCbs).HasPrecision(18, 4);
                e.Property(x => x.AliquotaEstadualReducao).HasPrecision(18, 4);
                e.Property(x => x.AliquotaMunicipalReducao).HasPrecision(18, 4);
                e.Property(x => x.AliquotaCbsReducao).HasPrecision(18, 4);
                e.Property(x => x.AliquotaEstadualDiferimento).HasPrecision(18, 4);
                e.Property(x => x.AliquotaMunicipalDiferimento).HasPrecision(18, 4);
                e.Property(x => x.AliquotaCbsDiferimento).HasPrecision(18, 4);
                e.Property(x => x.AliquotaEfetivaEstadual).HasPrecision(18, 4);
                e.Property(x => x.AliquotaEfetivaMunicipal).HasPrecision(18, 4);
                e.Property(x => x.AliquotaEfetivaCbs).HasPrecision(18, 4);
                e.Property(x => x.ValorBaseDeCalculo).HasPrecision(18, 2);
                e.Property(x => x.ValorImpostoDevidoEstadual).HasPrecision(18, 2);
                e.Property(x => x.ValorImpostoDevidoMunicipal).HasPrecision(18, 2);
                e.Property(x => x.ValorImpostoDevidoCbs).HasPrecision(18, 2);
                e.HasOne(x => x.VendaItemImpostoIbsCbsTributacaoRegular)
                 .WithOne(t => t.VendaItemImpostoIbsCbs)
                 .HasForeignKey<VendaItemImpostoIbsCbsTributacaoRegular>(t => t.VendaItemImpostoIbsCbsId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_venda_item_ibscbs_sync_id");
            });

            modelBuilder.Entity<VendaItemImpostoIbsCbsTributacaoRegular>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Cst).HasMaxLength(10);
                e.Property(x => x.CClassTrib).HasMaxLength(10);
                e.Property(x => x.AliquotaEfetivaIbsEstadual).HasPrecision(18, 4);
                e.Property(x => x.ValorIbsEstadual).HasPrecision(18, 2);
                e.Property(x => x.AliquotaEfetivaIbsMunicipal).HasPrecision(18, 4);
                e.Property(x => x.ValorIbsMunicipal).HasPrecision(18, 2);
                e.Property(x => x.AliquotaEfetivaCbs).HasPrecision(18, 4);
                e.Property(x => x.ValorCbs).HasPrecision(18, 2);
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_venda_item_ibscbs_trib_reg_sync_id");
            });

            modelBuilder.Entity<VendaItemImpostoValorAproximado>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.AliquotaNacionalFederal).HasPrecision(18, 2);
                e.Property(x => x.AliquotaImportadoFederal).HasPrecision(18, 2);
                e.Property(x => x.AliquotaEstadual).HasPrecision(18, 2);
                e.Property(x => x.AliquotaMunicipal).HasPrecision(18, 2);
                e.Property(x => x.Versao).HasMaxLength(10);
                e.Property(x => x.Fonte).HasMaxLength(60);
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_venda_item_imp_aprox_sync_id");
            });

            modelBuilder.Entity<VendaItemCombustivel>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.CodigoAnp).HasMaxLength(9);
                e.Property(x => x.DescricaoAnp).HasMaxLength(120);
                e.Property(x => x.QuantidadeCombustivelFaturada).HasPrecision(18, 4);
                e.Property(x => x.PercentualGlpDerivadoPetroleo).HasPrecision(18, 4);
                e.Property(x => x.PercentualGasNaturalNacional).HasPrecision(18, 4);
                e.Property(x => x.PercentualGasNaturalImportado).HasPrecision(18, 4);
                e.Property(x => x.ValorPartida).HasPrecision(18, 2);
                e.HasMany(x => x.Origens).WithOne(o => o.VendaItemCombustivel).HasForeignKey(o => o.VendaItemCombustivelId).OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_venda_item_combustivel_sync_id");
            });

            modelBuilder.Entity<VendaItemCombustivelOrigem>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.PercentualOrigem).HasPrecision(18, 4);
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_venda_item_comb_origem_sync_id");
            });
        }

        private static void ConfigurarTotais(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VendaTotal>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.ValorBaseDeCalculoIcms).HasPrecision(18, 2);
                e.Property(x => x.ValorIcms).HasPrecision(18, 2);
                e.Property(x => x.ValorIcmsDesonerado).HasPrecision(18, 2);
                e.Property(x => x.ValorFcp).HasPrecision(18, 2);
                e.Property(x => x.ValorBaseDeCalculoSt).HasPrecision(18, 2);
                e.Property(x => x.ValorSt).HasPrecision(18, 2);
                e.Property(x => x.ValorFcpSt).HasPrecision(18, 2);
                e.Property(x => x.ValorFcpRetido).HasPrecision(18, 2);
                e.Property(x => x.ValorProduto).HasPrecision(18, 2);
                e.Property(x => x.ValorFrete).HasPrecision(18, 2);
                e.Property(x => x.ValorSeguro).HasPrecision(18, 2);
                e.Property(x => x.ValorDesconto).HasPrecision(18, 2);
                e.Property(x => x.ValorImpostoImportacao).HasPrecision(18, 2);
                e.Property(x => x.ValorIpi).HasPrecision(18, 2);
                e.Property(x => x.ValorIpiDevolucao).HasPrecision(18, 2);
                e.Property(x => x.ValorPis).HasPrecision(18, 2);
                e.Property(x => x.ValorCofins).HasPrecision(18, 2);
                e.Property(x => x.ValorOutro).HasPrecision(18, 2);
                e.Property(x => x.ValorNotaFiscal).HasPrecision(18, 2);
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_venda_total_sync_id");
            });

            modelBuilder.Entity<VendaTotalIbsCbs>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.ValorBaseDeCalculo).HasPrecision(18, 2);
                e.Property(x => x.ValorImpostoDevidoEstadual).HasPrecision(18, 2);
                e.Property(x => x.ValorImpostoDevidoMunicipal).HasPrecision(18, 2);
                e.Property(x => x.ValorImpostoDevidoCbs).HasPrecision(18, 2);
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_venda_total_ibscbs_sync_id");
            });

            modelBuilder.Entity<VendaImposto>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.ValorAliquotaCreditoIcms).HasPrecision(18, 4);
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_venda_imposto_sync_id");
            });
        }

        private static void ConfigurarPessoas(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VendaEmitente>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Cnpj).HasMaxLength(14);
                e.Property(x => x.Cpf).HasMaxLength(11);
                e.Property(x => x.RazaoSocial).HasMaxLength(60);
                e.Property(x => x.NomeFantasia).HasMaxLength(60);
                e.Property(x => x.Telefone).HasMaxLength(14);
                e.Property(x => x.InscricaoEstadual).HasMaxLength(20);
                e.Property(x => x.InscricaoEstadualST).HasMaxLength(14);
                e.Property(x => x.InscricaoMunicipal).HasMaxLength(15);
                e.HasOne(x => x.Endereco).WithOne(en => en.VendaEmitente).HasForeignKey<VendaEmitenteEndereco>(en => en.VendaEmitenteId).OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_venda_emitente_sync_id");
            });

            modelBuilder.Entity<VendaEmitenteEndereco>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Logradouro).HasMaxLength(60);
                e.Property(x => x.Numero).HasMaxLength(60);
                e.Property(x => x.Complemento).HasMaxLength(60);
                e.Property(x => x.Bairro).HasMaxLength(60);
                e.Property(x => x.MunicipioNome).HasMaxLength(60);
                e.Property(x => x.Cep).HasMaxLength(8);
                e.Property(x => x.PaisNome).HasMaxLength(60);
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_venda_emitente_end_sync_id");
            });

            modelBuilder.Entity<VendaDestinatario>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Cnpj).HasMaxLength(14);
                e.Property(x => x.Cpf).HasMaxLength(11);
                e.Property(x => x.RazaoSocial).HasMaxLength(60);
                e.Property(x => x.Telefone).HasMaxLength(14);
                e.Property(x => x.InscricaoEstadual).HasMaxLength(14);
                e.Property(x => x.IdentificadorEstrangeiro).HasMaxLength(20);
                e.Property(x => x.Email).HasMaxLength(60);
                e.Property(x => x.DocumentoConsumidor).HasMaxLength(20);
                e.HasMany(x => x.Enderecos).WithOne(en => en.VendaDestinatario).HasForeignKey(en => en.VendaDestinatarioId).OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_venda_destinatario_sync_id");
            });

            modelBuilder.Entity<VendaDestinatarioEndereco>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Logradouro).HasMaxLength(60);
                e.Property(x => x.Numero).HasMaxLength(60);
                e.Property(x => x.Complemento).HasMaxLength(60);
                e.Property(x => x.Bairro).HasMaxLength(60);
                e.Property(x => x.MunicipioNome).HasMaxLength(60);
                e.Property(x => x.Cep).HasMaxLength(8);
                e.Property(x => x.PaisNome).HasMaxLength(60);
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_venda_destinatario_end_sync_id");
            });

            modelBuilder.Entity<VendaEntrega>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Nome).HasMaxLength(60);
                e.Property(x => x.Fone).HasMaxLength(14);
                e.Property(x => x.Email).HasMaxLength(60);
                e.Property(x => x.IE).HasMaxLength(20);
                e.Property(x => x.Documento).HasMaxLength(20);
                e.Property(x => x.Logradouro).HasMaxLength(60);
                e.Property(x => x.Numero).HasMaxLength(60);
                e.Property(x => x.Complemento).HasMaxLength(60);
                e.Property(x => x.Bairro).HasMaxLength(60);
                e.Property(x => x.MunicipioNome).HasMaxLength(60);
                e.Property(x => x.Cep).HasMaxLength(8);
                e.Property(x => x.PaisNome).HasMaxLength(60);
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_venda_entrega_sync_id");
            });

            modelBuilder.Entity<VendaCobrancaEndereco>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Nome).HasMaxLength(60);
                e.Property(x => x.Fone).HasMaxLength(14);
                e.Property(x => x.Email).HasMaxLength(60);
                e.Property(x => x.IE).HasMaxLength(20);
                e.Property(x => x.Documento).HasMaxLength(20);
                e.Property(x => x.Logradouro).HasMaxLength(60);
                e.Property(x => x.Numero).HasMaxLength(60);
                e.Property(x => x.Complemento).HasMaxLength(60);
                e.Property(x => x.Bairro).HasMaxLength(60);
                e.Property(x => x.MunicipioNome).HasMaxLength(60);
                e.Property(x => x.Cep).HasMaxLength(8);
                e.Property(x => x.PaisNome).HasMaxLength(60);
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_venda_cobranca_end_sync_id");
            });
        }

        private static void ConfigurarTransporte(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VendaTransporte>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasOne(x => x.Transportadora).WithOne(t => t.VendaTransporte).HasForeignKey<VendaTransporteTransportadora>(t => t.VendaTransporteId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.Veiculo).WithOne(v => v.Transporte).HasForeignKey<VendaTransporteVeiculo>(v => v.VendaTransporteId).OnDelete(DeleteBehavior.Cascade);
                e.HasMany(x => x.Volumes).WithOne(v => v.VendaTransporte).HasForeignKey(v => v.VendaTransporteId).OnDelete(DeleteBehavior.Cascade);
                e.HasMany(x => x.Reboques).WithOne(r => r.Transporte).HasForeignKey(r => r.VendaTransporteId).OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_venda_transporte_sync_id");
            });

            modelBuilder.Entity<VendaTransporteTransportadora>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Cnpj).HasMaxLength(14);
                e.Property(x => x.Cpf).HasMaxLength(11);
                e.Property(x => x.RazaoSocial).HasMaxLength(60);
                e.Property(x => x.InscricaoEstadual).HasMaxLength(20);
                e.Property(x => x.Logradouro).HasMaxLength(60);
                e.Property(x => x.Numero).HasMaxLength(60);
                e.Property(x => x.Complemento).HasMaxLength(60);
                e.Property(x => x.Bairro).HasMaxLength(60);
                e.Property(x => x.Municipio).HasMaxLength(60);
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_venda_transp_transportadora_sync_id");
            });

            modelBuilder.Entity<VendaTransporteVeiculo>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Placa).HasMaxLength(8);
                e.Property(x => x.Rntrc).HasMaxLength(14);
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_venda_transp_veiculo_sync_id");
            });

            modelBuilder.Entity<VendaTransporteReboque>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Placa).HasMaxLength(8);
                e.Property(x => x.Rntrc).HasMaxLength(14);
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_venda_transp_reboque_sync_id");
            });

            modelBuilder.Entity<VendaTransporteVolume>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Especie).HasMaxLength(60);
                e.Property(x => x.NumeroVolumes).HasMaxLength(60);
                e.Property(x => x.Marca).HasMaxLength(60);
                e.Property(x => x.PesoLiquido).HasPrecision(18, 3);
                e.Property(x => x.PesoBruto).HasPrecision(18, 3);
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_venda_transp_volume_sync_id");
            });
        }

        private static void ConfigurarConfiguracaoEPagamento(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VendaConfiguracao>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_venda_configuracao_sync_id");
            });

            modelBuilder.Entity<VendaPagamento>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.ValorTroco).HasPrecision(18, 2);
                e.Property(x => x.ValorPagamento).HasPrecision(18, 2);
                e.Property(x => x.CartaoCnpjIntermediadorFinanceira).HasMaxLength(14);
                e.Property(x => x.CartaoCodigoAutorizacaoOperacao).HasMaxLength(20);
                e.HasIndex(x => new { x.TenantId, x.VendaId }).HasDatabaseName("ix_venda_pagamentos_tenant_venda");
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_venda_pagamento_sync_id");
            });
        }

        private static void ConfigurarNfe(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VendaNfce>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.IdCsc).HasMaxLength(20);
                e.Property(x => x.Csc).HasMaxLength(50);
                e.Property(x => x.Chave).HasMaxLength(44);
                e.Property(x => x.Protocolo).HasMaxLength(30);
                e.Property(x => x.UltimoRetornoMensagemSefaz).HasMaxLength(300);
                e.Property(x => x.ProtocoloCancelamento).HasMaxLength(30);
                e.Property(x => x.MotivoCancelamento).HasMaxLength(300);
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_venda_nfce_sync_id");
            });

            modelBuilder.Entity<VendaNfe>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Chave).HasMaxLength(44);
                e.Property(x => x.Protocolo).HasMaxLength(30);
                e.Property(x => x.UltimoRetornoMensagemSefaz).HasMaxLength(300);
                e.Property(x => x.ProtocoloCancelamento).HasMaxLength(30);
                e.Property(x => x.MotivoCancelamento).HasMaxLength(300);
                e.HasOne(x => x.Intermediador).WithOne(i => i.VendaNfe).HasForeignKey<VendaNfeIntermediador>(i => i.VendaNfeId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.VendaNfeExportacao).WithOne(ex => ex.VendaNfe).HasForeignKey<VendaNfeExportacao>(ex => ex.VendaNfeId).OnDelete(DeleteBehavior.Cascade);
                e.HasMany(x => x.CartasCorrecoes).WithOne(c => c.VendaNfe).HasForeignKey(c => c.VendaNfeId).OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_venda_nfe_sync_id");
            });

            modelBuilder.Entity<VendaNfeCartaCorrecao>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.TextoCorrecao).HasMaxLength(1000);
                e.Property(x => x.MotivoRejeicaoSefaz).HasMaxLength(300);
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_venda_nfe_cc_sync_id");
            });

            modelBuilder.Entity<VendaNfeExportacao>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.LocalExportacao).HasMaxLength(60);
                e.Property(x => x.LocalDespacho).HasMaxLength(60);
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_venda_nfe_exportacao_sync_id");
            });

            modelBuilder.Entity<VendaNfeIntermediador>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Documento).HasMaxLength(14);
                e.Property(x => x.IdentificadorIntermediador).HasMaxLength(60);
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_venda_nfe_intermediador_sync_id");
            });

            modelBuilder.Entity<VendaNfeReferenciada>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Chave).HasMaxLength(44);
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_venda_nfe_referenciada_sync_id");
            });
        }

        private static void ConfigurarFatura(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VendaFatura>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.NumeroFatura).HasMaxLength(60);
                e.Property(x => x.ValorOriginal).HasPrecision(18, 2);
                e.Property(x => x.ValorDesconto).HasPrecision(18, 2);
                e.Property(x => x.ValorLiquido).HasPrecision(18, 2);
                e.HasMany(x => x.Duplicatas).WithOne(d => d.VendaFatura).HasForeignKey(d => d.VendaFaturaId).OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_venda_fatura_sync_id");
            });

            modelBuilder.Entity<VendaFaturaDuplicata>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.NumeroDuplicata).HasMaxLength(60);
                e.Property(x => x.ValorDuplicata).HasPrecision(18, 2);
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_venda_fatura_duplicata_sync_id");
            });
        }

        private static void ConfigurarAutorizacoesEHistorico(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VendaAutorizacaoXml>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Documento).HasMaxLength(14);
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_venda_autorizacao_xml_sync_id");
            });

            modelBuilder.Entity<VendaNfHistorico>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Descricao).HasMaxLength(300);
                e.HasIndex(x => new { x.TenantId, x.VendaId }).HasDatabaseName("ix_venda_nf_historico_tenant_venda");
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_venda_nf_historico_sync_id");
            });

            // Porte fiel de VendaNfeHistorico (legado só tinha VendaId). Sem coleção de navegação
            // no agregado Venda (idêntico ao legado); vínculo por FK VendaId.
            modelBuilder.Entity<VendaNfeHistorico>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasOne(x => x.Venda).WithMany().HasForeignKey(x => x.VendaId).OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => new { x.TenantId, x.VendaId }).HasDatabaseName("ix_venda_nfe_historico_tenant_venda");
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_venda_nfe_historico_sync_id");
            });
        }

        private static void ConfigurarCaixa(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Caixa>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.SaldoAbertura).HasPrecision(18, 2);
                entity.Property(c => c.SaldoFechamento).HasPrecision(18, 2);
                entity.Property(c => c.DiferencaFechamento).HasPrecision(18, 2);
                // Status é enum tipado (ECaixaStatus) — armazenado como int pelo EF.
                entity.Property(c => c.OperadorId).HasMaxLength(100);
                entity.HasIndex(c => new { c.TenantId, c.OperadorId }).HasDatabaseName("ix_caixas_tenant_operador");
                entity.HasIndex(c => c.SyncId).IsUnique().HasDatabaseName("uq_caixas_sync_id");
            });

            modelBuilder.Entity<CaixaMovimento>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Valor).HasPrecision(18, 2);
                entity.Property(m => m.Tipo).HasMaxLength(20);
                entity.Property(m => m.Observacao).HasMaxLength(500);
                entity.HasIndex(m => new { m.TenantId, m.CaixaId }).HasDatabaseName("ix_caixa_movimentos_tenant_caixa");
                entity.HasIndex(m => m.SyncId).IsUnique().HasDatabaseName("uq_caixa_movimentos_sync_id");
            });
        }

        /// <summary>
        /// Mapping do submódulo Gestão de Serviços (VEN-GSV). Schema "vendas" (herdado do HasDefaultSchema).
        /// Fonte: EF_7_VENDAS_GESTAO_DE_SERVICOS_V1 §19. Sem movimento de estoque; integração financeira
        /// por referência + Outbox.
        /// </summary>
        private static void ConfigurarGestaoServicos(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ServicoCatalogo>(e =>
            {
                e.ToTable("servico_catalogos");
                e.HasKey(x => x.Id);
                e.Property(x => x.Nome).HasMaxLength(250);
                e.Property(x => x.Descricao).HasMaxLength(4000);
                e.Property(x => x.Valor).HasPrecision(18, 2);
                e.Property(x => x.Taxa).HasPrecision(18, 4);
                e.Property(x => x.TaxaEmbalagemEntrega).HasPrecision(18, 2);
                e.Property(x => x.TipoTaxaEmbalagemEntrega).HasMaxLength(60);
                e.HasIndex(x => new { x.TenantId, x.Nome }).HasDatabaseName("ix_servico_catalogos_tenant_nome");
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_servico_catalogos_sync_id");
                e.HasQueryFilter(x => x.DeletadoEm == null);
            });

            modelBuilder.Entity<ServicoFatura>(e =>
            {
                e.ToTable("servico_faturas");
                e.HasKey(x => x.Id);
                e.Property(x => x.NumeroFatura).HasMaxLength(60);
                e.Property(x => x.Status).HasConversion<int>();
                e.Property(x => x.DescontoCabecalho).HasPrecision(18, 2);
                e.Property(x => x.TotalDesconto).HasPrecision(18, 2);
                e.Property(x => x.ValorImposto).HasPrecision(18, 2);
                e.Property(x => x.TotalImposto).HasPrecision(18, 2);
                e.Property(x => x.CustoEnvioEntrega).HasPrecision(18, 2);
                e.Property(x => x.TotalGeral).HasPrecision(18, 2);
                e.Property(x => x.TotalLiquido).HasPrecision(18, 2);
                e.Property(x => x.ValorPago).HasPrecision(18, 2);
                e.Property(x => x.SaldoDevido).HasPrecision(18, 2);
                e.Property(x => x.Troco).HasPrecision(18, 2);
                e.Property(x => x.Detalhes).HasMaxLength(4000);
                e.HasMany(x => x.Linhas).WithOne(l => l.Fatura).HasForeignKey(l => l.FaturaId).OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => new { x.TenantId, x.ClienteId }).HasDatabaseName("ix_servico_faturas_tenant_cliente");
                e.HasIndex(x => new { x.TenantId, x.NumeroFatura }).IsUnique().HasDatabaseName("uq_servico_faturas_tenant_numero");
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_servico_faturas_sync_id");
                e.HasQueryFilter(x => x.DeletadoEm == null);
            });

            modelBuilder.Entity<ServicoFaturaLinha>(e =>
            {
                e.ToTable("servico_fatura_linhas");
                e.HasKey(x => x.Id);
                e.Property(x => x.NomeServico).HasMaxLength(250);
                e.Property(x => x.Descricao).HasMaxLength(500);
                e.Property(x => x.Quantidade).HasPrecision(18, 4);
                e.Property(x => x.PrecoUnitario).HasPrecision(18, 2);
                e.Property(x => x.DescontoPercentual).HasPrecision(18, 4);
                e.Property(x => x.Total).HasPrecision(18, 2);
                e.HasIndex(x => new { x.TenantId, x.FaturaId }).HasDatabaseName("ix_servico_fatura_linhas_tenant_fatura");
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_servico_fatura_linhas_sync_id");
            });

            modelBuilder.Entity<ServicoLancamentoFinanceiroRef>(e =>
            {
                e.ToTable("servico_lancamento_financeiro_refs");
                e.HasKey(x => x.Id);
                e.Property(x => x.NumeroFatura).HasMaxLength(60);
                e.Property(x => x.TipoLancamento).HasConversion<int>();
                e.Property(x => x.StatusIntegracao).HasConversion<int>();
                e.Property(x => x.Valor).HasPrecision(18, 2);
                e.Property(x => x.MensagemIntegracao).HasMaxLength(500);
                e.HasIndex(x => new { x.TenantId, x.FaturaId }).HasDatabaseName("ix_servico_lanc_fin_ref_tenant_fatura");
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_servico_lanc_fin_ref_sync_id");
            });

            modelBuilder.Entity<ServicoHistorico>(e =>
            {
                e.ToTable("servico_historicos");
                e.HasKey(x => x.Id);
                e.Property(x => x.Entidade).HasConversion<int>();
                e.Property(x => x.UsuarioId).HasMaxLength(100);
                e.Property(x => x.Evento).HasMaxLength(120);
                e.Property(x => x.StatusAnterior).HasMaxLength(60);
                e.Property(x => x.StatusNovo).HasMaxLength(60);
                e.Property(x => x.TraceId).HasMaxLength(80);
                e.HasIndex(x => new { x.TenantId, x.EntidadeId }).HasDatabaseName("ix_servico_historicos_tenant_entidade");
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_servico_historicos_sync_id");
            });
        }

        /// <summary>
        /// Mapping do submódulo CRM (VEN-CRM). Schema "vendas" (herdado do HasDefaultSchema).
        /// Fonte: EF_7_VENDAS_CRM_V1 §10. Vínculos cross-módulo (cliente, contato, usuário, projeto,
        /// moeda) por Guid FK, sem navegação EF cross-module (convenção §6.4).
        /// </summary>
        private static void ConfigurarCrm(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CrmPipeline>(e =>
            {
                e.ToTable("crm_pipelines");
                e.HasKey(x => x.Id);
                e.Property(x => x.Nome).HasMaxLength(120);
                e.HasIndex(x => x.TenantId).HasDatabaseName("ix_crm_pipelines_tenant");
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_crm_pipelines_sync_id");
                e.HasQueryFilter(x => x.DeletadoEm == null);
            });

            modelBuilder.Entity<CrmEtapa>(e =>
            {
                e.ToTable("crm_etapas");
                e.HasKey(x => x.Id);
                e.Property(x => x.TipoEtapa).HasConversion<int>();
                e.Property(x => x.Nome).HasMaxLength(120);
                e.HasIndex(x => new { x.TenantId, x.PipelineId }).HasDatabaseName("ix_crm_etapas_tenant_pipeline");
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_crm_etapas_sync_id");
                e.HasQueryFilter(x => x.DeletadoEm == null);
            });

            modelBuilder.Entity<CrmEtiqueta>(e =>
            {
                e.ToTable("crm_etiquetas");
                e.HasKey(x => x.Id);
                e.Property(x => x.Nome).HasMaxLength(120);
                e.Property(x => x.Cor).HasMaxLength(20);
                e.HasIndex(x => x.TenantId).HasDatabaseName("ix_crm_etiquetas_tenant");
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_crm_etiquetas_sync_id");
                e.HasQueryFilter(x => x.DeletadoEm == null);
            });

            modelBuilder.Entity<CrmOrigem>(e =>
            {
                e.ToTable("crm_origens");
                e.HasKey(x => x.Id);
                e.Property(x => x.Nome).HasMaxLength(120);
                e.HasIndex(x => x.TenantId).HasDatabaseName("ix_crm_origens_tenant");
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_crm_origens_sync_id");
                e.HasQueryFilter(x => x.DeletadoEm == null);
            });

            modelBuilder.Entity<CrmLead>(e =>
            {
                e.ToTable("crm_leads");
                e.HasKey(x => x.Id);
                e.Property(x => x.Nome).HasMaxLength(200);
                e.Property(x => x.PrimeiroNome).HasMaxLength(120);
                e.Property(x => x.Sobrenome).HasMaxLength(120);
                e.Property(x => x.Email).HasMaxLength(160);
                e.Property(x => x.Telefone).HasMaxLength(40);
                e.Property(x => x.Assunto).HasMaxLength(250);
                e.Property(x => x.ValorEstimado).HasPrecision(18, 2);
                e.Property(x => x.Status).HasConversion<int>();
                e.Property(x => x.EstadoArquivo).HasConversion<int>();
                e.Property(x => x.Visibilidade).HasMaxLength(60);
                e.Property(x => x.EnderecoIpCaptacao).HasMaxLength(60);
                e.Property(x => x.ImagemCapa).HasMaxLength(500);
                e.HasIndex(x => new { x.TenantId, x.EtapaId }).HasDatabaseName("ix_crm_leads_tenant_etapa");
                e.HasIndex(x => new { x.TenantId, x.Status }).HasDatabaseName("ix_crm_leads_tenant_status");
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_crm_leads_sync_id");
                e.HasQueryFilter(x => x.DeletadoEm == null);
            });

            modelBuilder.Entity<CrmLeadUsuario>(e =>
            {
                e.ToTable("crm_lead_usuarios");
                e.HasKey(x => x.Id);
                e.Property(x => x.Papel).HasMaxLength(60);
                e.HasIndex(x => new { x.TenantId, x.LeadId }).HasDatabaseName("ix_crm_lead_usuarios_tenant_lead");
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_crm_lead_usuarios_sync_id");
            });

            modelBuilder.Entity<CrmOportunidade>(e =>
            {
                e.ToTable("crm_oportunidades");
                e.HasKey(x => x.Id);
                e.Property(x => x.Nome).HasMaxLength(200);
                e.Property(x => x.Valor).HasPrecision(18, 2);
                e.Property(x => x.ValorConvertido).HasPrecision(18, 2);
                e.Property(x => x.Probabilidade).HasPrecision(9, 4);
                e.Property(x => x.Status).HasConversion<int>();
                e.Property(x => x.Telefone).HasMaxLength(40);
                e.Property(x => x.MotivoPerda).HasMaxLength(500);
                e.HasIndex(x => new { x.TenantId, x.EtapaId }).HasDatabaseName("ix_crm_oportunidades_tenant_etapa");
                e.HasIndex(x => new { x.TenantId, x.Status }).HasDatabaseName("ix_crm_oportunidades_tenant_status");
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_crm_oportunidades_sync_id");
                e.HasQueryFilter(x => x.DeletadoEm == null);
            });

            modelBuilder.Entity<CrmOportunidadeParticipante>(e =>
            {
                e.ToTable("crm_oportunidade_participantes");
                e.HasKey(x => x.Id);
                e.Property(x => x.TipoParticipante).HasConversion<int>();
                e.HasIndex(x => new { x.TenantId, x.OportunidadeId }).HasDatabaseName("ix_crm_oport_participantes_tenant_oport");
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_crm_oport_participantes_sync_id");
            });

            modelBuilder.Entity<CrmAtividade>(e =>
            {
                e.ToTable("crm_atividades");
                e.HasKey(x => x.Id);
                e.Property(x => x.EntidadeTipo).HasConversion<int>();
                e.Property(x => x.TipoAtividade).HasConversion<int>();
                e.Property(x => x.Prioridade).HasConversion<int?>();
                e.Property(x => x.Status).HasConversion<int?>();
                e.Property(x => x.TipoChamada).HasConversion<int?>();
                e.Property(x => x.Nome).HasMaxLength(200);
                e.Property(x => x.Assunto).HasMaxLength(250);
                e.Property(x => x.Hora).HasMaxLength(10);
                e.Property(x => x.Duracao).HasMaxLength(30);
                e.Property(x => x.Destinatario).HasMaxLength(250);
                e.Property(x => x.Resultado).HasMaxLength(500);
                e.Property(x => x.ArquivoNome).HasMaxLength(250);
                e.Property(x => x.ArquivoReferencia).HasMaxLength(500);
                e.HasIndex(x => new { x.TenantId, x.LeadId }).HasDatabaseName("ix_crm_atividades_tenant_lead");
                e.HasIndex(x => new { x.TenantId, x.OportunidadeId }).HasDatabaseName("ix_crm_atividades_tenant_oport");
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_crm_atividades_sync_id");
            });

            modelBuilder.Entity<CrmCampanha>(e =>
            {
                e.ToTable("crm_campanhas");
                e.HasKey(x => x.Id);
                e.Property(x => x.Nome).HasMaxLength(200);
                e.Property(x => x.Tipo).HasMaxLength(60);
                e.Property(x => x.Status).HasMaxLength(60);
                e.Property(x => x.Frequencia).HasMaxLength(60);
                e.Property(x => x.Orcamento).HasPrecision(18, 2);
                e.Property(x => x.CustoEsperado).HasPrecision(18, 2);
                e.Property(x => x.CustoReal).HasPrecision(18, 2);
                e.Property(x => x.ReceitaEsperada).HasPrecision(18, 2);
                e.HasIndex(x => x.TenantId).HasDatabaseName("ix_crm_campanhas_tenant");
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_crm_campanhas_sync_id");
                e.HasQueryFilter(x => x.DeletadoEm == null);
            });

            modelBuilder.Entity<CrmListaPublico>(e =>
            {
                e.ToTable("crm_listas_publico");
                e.HasKey(x => x.Id);
                e.Property(x => x.Nome).HasMaxLength(200);
                e.Property(x => x.Tipo).HasMaxLength(60);
                e.Property(x => x.DominioExclusao).HasMaxLength(160);
                e.HasIndex(x => x.TenantId).HasDatabaseName("ix_crm_listas_publico_tenant");
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_crm_listas_publico_sync_id");
                e.HasQueryFilter(x => x.DeletadoEm == null);
            });

            modelBuilder.Entity<CrmListaPublicoMembro>(e =>
            {
                e.ToTable("crm_lista_publico_membros");
                e.HasKey(x => x.Id);
                e.Property(x => x.TipoMembro).HasConversion<int>();
                e.HasIndex(x => new { x.TenantId, x.ListaPublicoId }).HasDatabaseName("ix_crm_lista_membros_tenant_lista");
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_crm_lista_membros_sync_id");
            });

            modelBuilder.Entity<CrmCampanhaLista>(e =>
            {
                e.ToTable("crm_campanha_listas");
                e.HasKey(x => x.Id);
                e.Property(x => x.TipoUso).HasMaxLength(30);
                e.HasIndex(x => new { x.TenantId, x.CampanhaId }).HasDatabaseName("ix_crm_campanha_listas_tenant_campanha");
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_crm_campanha_listas_sync_id");
            });

            modelBuilder.Entity<CrmCampanhaRastreador>(e =>
            {
                e.ToTable("crm_campanha_rastreadores");
                e.HasKey(x => x.Id);
                e.Property(x => x.Nome).HasMaxLength(200);
                e.Property(x => x.Url).HasMaxLength(1000);
                e.Property(x => x.ChaveRastreamento).HasMaxLength(80);
                e.Property(x => x.UrlMensagem).HasMaxLength(1000);
                e.HasIndex(x => new { x.TenantId, x.CampanhaId }).HasDatabaseName("ix_crm_campanha_rastreadores_tenant_campanha");
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_crm_campanha_rastreadores_sync_id");
            });

            modelBuilder.Entity<CrmCampanhaLog>(e =>
            {
                e.ToTable("crm_campanha_logs");
                e.HasKey(x => x.Id);
                e.Property(x => x.AlvoTipo).HasMaxLength(30);
                e.Property(x => x.AtividadeTipo).HasMaxLength(30);
                e.Property(x => x.Email).HasMaxLength(160);
                e.Property(x => x.EnderecoIp).HasMaxLength(60);
                e.HasIndex(x => new { x.TenantId, x.CampanhaId }).HasDatabaseName("ix_crm_campanha_logs_tenant_campanha");
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_crm_campanha_logs_sync_id");
            });

            modelBuilder.Entity<CrmWebform>(e =>
            {
                e.ToTable("crm_webforms");
                e.HasKey(x => x.Id);
                e.Property(x => x.IdentificadorUnico).HasMaxLength(80);
                e.Property(x => x.Titulo).HasMaxLength(200);
                e.Property(x => x.TextoBotao).HasMaxLength(60);
                e.Property(x => x.LeadTituloPadrao).HasMaxLength(200);
                e.Property(x => x.LeadStatusPadrao).HasMaxLength(60);
                e.HasIndex(x => new { x.TenantId, x.IdentificadorUnico }).IsUnique().HasDatabaseName("uq_crm_webforms_tenant_identificador");
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_crm_webforms_sync_id");
                e.HasQueryFilter(x => x.DeletadoEm == null);
            });

            modelBuilder.Entity<CrmWebformResponsavel>(e =>
            {
                e.ToTable("crm_webform_responsaveis");
                e.HasKey(x => x.Id);
                e.HasIndex(x => new { x.TenantId, x.WebformId }).HasDatabaseName("ix_crm_webform_responsaveis_tenant_webform");
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_crm_webform_responsaveis_sync_id");
            });

            modelBuilder.Entity<CrmTicket>(e =>
            {
                e.ToTable("crm_tickets");
                e.HasKey(x => x.Id);
                e.Property(x => x.Numero).HasMaxLength(40);
                e.Property(x => x.Titulo).HasMaxLength(250);
                e.Property(x => x.Prioridade).HasConversion<int?>();
                e.Property(x => x.Origem).HasConversion<int?>();
                e.Property(x => x.EstadoArquivo).HasConversion<int>();
                e.Property(x => x.TipoUsuarioAbertura).HasMaxLength(30);
                e.HasIndex(x => new { x.TenantId, x.StatusId }).HasDatabaseName("ix_crm_tickets_tenant_status");
                e.HasIndex(x => new { x.TenantId, x.ClienteId }).HasDatabaseName("ix_crm_tickets_tenant_cliente");
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_crm_tickets_sync_id");
                e.HasQueryFilter(x => x.DeletadoEm == null);
            });

            modelBuilder.Entity<CrmTicketStatus>(e =>
            {
                e.ToTable("crm_ticket_status");
                e.HasKey(x => x.Id);
                e.Property(x => x.Titulo).HasMaxLength(120);
                e.Property(x => x.Cor).HasMaxLength(20);
                e.HasIndex(x => x.TenantId).HasDatabaseName("ix_crm_ticket_status_tenant");
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_crm_ticket_status_sync_id");
                e.HasQueryFilter(x => x.DeletadoEm == null);
            });

            modelBuilder.Entity<CrmTicketResposta>(e =>
            {
                e.ToTable("crm_ticket_respostas");
                e.HasKey(x => x.Id);
                e.Property(x => x.Tipo).HasConversion<int>();
                e.Property(x => x.Origem).HasConversion<int?>();
                e.Property(x => x.RemetenteTipo).HasMaxLength(30);
                e.HasIndex(x => new { x.TenantId, x.TicketId }).HasDatabaseName("ix_crm_ticket_respostas_tenant_ticket");
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_crm_ticket_respostas_sync_id");
            });

            modelBuilder.Entity<CrmClienteFidelizado>(e =>
            {
                e.ToTable("crm_clientes_fidelizados");
                e.HasKey(x => x.Id);
                e.Property(x => x.Nome).HasMaxLength(200);
                e.Property(x => x.Email).HasMaxLength(160);
                e.Property(x => x.Celular).HasMaxLength(40);
                e.Property(x => x.TotalPontos).HasPrecision(18, 2);
                e.HasIndex(x => new { x.TenantId, x.ClienteId }).HasDatabaseName("ix_crm_clientes_fidelizados_tenant_cliente");
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_crm_clientes_fidelizados_sync_id");
                e.HasQueryFilter(x => x.DeletadoEm == null);
            });

            modelBuilder.Entity<CrmPontuacaoFidelizacao>(e =>
            {
                e.ToTable("crm_pontuacoes_fidelizacao");
                e.HasKey(x => x.Id);
                e.Property(x => x.Pontos).HasPrecision(18, 2);
                e.Property(x => x.Observacao).HasMaxLength(250);
                e.HasIndex(x => new { x.TenantId, x.ClienteFidelizadoId }).HasDatabaseName("ix_crm_pontuacoes_tenant_cliente");
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_crm_pontuacoes_sync_id");
            });

            modelBuilder.Entity<CrmConfiguracaoPixRelacional>(e =>
            {
                e.ToTable("crm_configuracoes_pix_relacional");
                e.HasKey(x => x.Id);
                e.Property(x => x.LinkApiAppVendas).HasMaxLength(500);
                e.Property(x => x.TokenPix).HasMaxLength(500);
                e.Property(x => x.CnpjEmpresa).HasMaxLength(14);
                e.HasIndex(x => x.TenantId).HasDatabaseName("ix_crm_config_pix_tenant");
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_crm_config_pix_sync_id");
                e.HasQueryFilter(x => x.DeletadoEm == null);
            });

            modelBuilder.Entity<CrmPagamentoPixRelacional>(e =>
            {
                e.ToTable("crm_pagamentos_pix_relacional");
                e.HasKey(x => x.Id);
                e.Property(x => x.EntidadeOrigemTipo).HasMaxLength(30);
                e.Property(x => x.Valor).HasPrecision(18, 2);
                e.Property(x => x.ProvedorPagamentoId).HasMaxLength(120);
                e.Property(x => x.Status).HasConversion<int>();
                e.HasIndex(x => new { x.TenantId, x.Status }).HasDatabaseName("ix_crm_pagamentos_pix_tenant_status");
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_crm_pagamentos_pix_sync_id");
            });

            modelBuilder.Entity<CrmHistorico>(e =>
            {
                e.ToTable("crm_historicos");
                e.HasKey(x => x.Id);
                e.Property(x => x.EntidadeTipo).HasMaxLength(60);
                e.Property(x => x.Evento).HasMaxLength(120);
                e.Property(x => x.Observacao).HasMaxLength(500);
                e.HasIndex(x => new { x.TenantId, x.EntidadeId }).HasDatabaseName("ix_crm_historicos_tenant_entidade");
                e.HasIndex(x => x.SyncId).IsUnique().HasDatabaseName("uq_crm_historicos_sync_id");
            });
        }

        /// <summary>
        /// Mapeia os Lookups read-only cross-module. Todos apontam para tabelas de OUTROS módulos
        /// (schemas estoque/plataforma) e são EXCLUÍDOS das migrations do Vendas (o dono da tabela
        /// gera a migration). Filtro de soft-delete espelha as tabelas de origem.
        /// </summary>
        private static void ConfigurarLookups(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProdutoLookup>(entity =>
            {
                entity.ToTable("produtos", "estoque", t => t.ExcludeFromMigrations());
                entity.HasKey(x => x.Id);
                entity.HasQueryFilter(x => x.DeletadoEm == null);
            });

            modelBuilder.Entity<NcmConfiguracaoLookup>(entity =>
            {
                entity.ToTable("ncm_configuracoes", "plataforma", t => t.ExcludeFromMigrations());
                entity.HasKey(x => x.Id);
                entity.HasQueryFilter(x => x.DeletadoEm == null);
            });

            modelBuilder.Entity<NcmTributacaoLookup>(entity =>
            {
                entity.ToTable("ncm_tributacoes", "plataforma", t => t.ExcludeFromMigrations());
                entity.HasKey(x => x.Id);
                entity.Property(x => x.InformacoesComplementares).HasMaxLength(5000);
                entity.HasQueryFilter(x => x.DeletadoEm == null);
            });

            modelBuilder.Entity<EmpresaLookup>(entity =>
            {
                entity.ToTable("empresas", "plataforma", t => t.ExcludeFromMigrations());
                entity.HasKey(x => x.Id);
                entity.HasQueryFilter(x => x.DeletadoEm == null);
            });
        }
    }
}
