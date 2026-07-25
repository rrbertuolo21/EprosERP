using Epros.Infrastructure.Data;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Estoque.Infrastructure.Data
{
    public class ContextEstoque : ContextBase
    {
        // Produtos e satélites
        public DbSet<Produto> Produtos => Set<Produto>();
        public DbSet<ProdutoGrupo> ProdutoGrupos => Set<ProdutoGrupo>();
        public DbSet<ProdutoGrupoEmpresa> ProdutoGrupoEmpresas => Set<ProdutoGrupoEmpresa>();
        public DbSet<CategoriaProduto> Categorias => Set<CategoriaProduto>();
        public DbSet<MarcaProduto> Marcas => Set<MarcaProduto>();
        public DbSet<UnidadeMedidaComercial> UnidadesMedida => Set<UnidadeMedidaComercial>();
        public DbSet<UnidadeMedidaTributavel> UnidadesMedidaTributavel => Set<UnidadeMedidaTributavel>();
        public DbSet<Adicionais> Adicionais => Set<Adicionais>();
        public DbSet<AdicionaisProduto> AdicionaisProdutos => Set<AdicionaisProduto>();
        public DbSet<Balanca> Balancas => Set<Balanca>();
        public DbSet<ProdutoEspecifico> ProdutosEspecificos => Set<ProdutoEspecifico>();
        public DbSet<ProdutoEspecificoCombustivelOrigem> ProdutoEspecificoCombustivelOrigens => Set<ProdutoEspecificoCombustivelOrigem>();
        public DbSet<ProdutoHistoricoReajuste> ProdutoHistoricoReajustes => Set<ProdutoHistoricoReajuste>();

        // Estoque
        public DbSet<EstoqueProduto> EstoqueProdutos => Set<EstoqueProduto>();
        public DbSet<ProdutoFichaEstoqueEntrada> ProdutoFichaEstoqueEntradas => Set<ProdutoFichaEstoqueEntrada>();
        public DbSet<ProdutoFichaEstoqueSaida> ProdutoFichaEstoqueSaidas => Set<ProdutoFichaEstoqueSaida>();
        public DbSet<FatoGeradorEstoque> FatosGeradoresEstoque => Set<FatoGeradorEstoque>();
        public DbSet<EstoqueMovimentoManual> EstoqueMovimentosManuais => Set<EstoqueMovimentoManual>();
        public DbSet<MovimentoEstoque> MovimentosEstoque => Set<MovimentoEstoque>();

        // Movimentação manual e ajustes (EST-MVM-001) — EF §15.6-15.15
        public DbSet<TransferenciaEstoque> TransferenciasEstoque => Set<TransferenciaEstoque>();
        public DbSet<TransferenciaEstoqueItem> TransferenciasEstoqueItens => Set<TransferenciaEstoqueItem>();
        public DbSet<AjusteEstoque> AjustesEstoque => Set<AjusteEstoque>();
        public DbSet<AjusteEstoqueItem> AjustesEstoqueItens => Set<AjusteEstoqueItem>();
        public DbSet<AvariaEstoque> AvariasEstoque => Set<AvariaEstoque>();
        public DbSet<RequisicaoInterna> RequisicoesInternas => Set<RequisicaoInterna>();
        public DbSet<RequisicaoInternaItem> RequisicoesInternasItens => Set<RequisicaoInternaItem>();
        public DbSet<SaldoInicialImportacao> SaldosIniciaisImportacoes => Set<SaldoInicialImportacao>();
        public DbSet<SaldoInicialItem> SaldosIniciaisItens => Set<SaldoInicialItem>();
        public DbSet<HistoricoEstoque> HistoricosEstoque => Set<HistoricoEstoque>();

        // Compras
        public DbSet<Compra> Compras => Set<Compra>();
        public DbSet<CompraItem> CompraItens => Set<CompraItem>();
        public DbSet<CompraTotal> CompraTotais => Set<CompraTotal>();
        public DbSet<CompraTotalIbsCbs> CompraTotaisIbsCbs => Set<CompraTotalIbsCbs>();
        public DbSet<CompraImposto> CompraImpostos => Set<CompraImposto>();
        public DbSet<CompraFatura> CompraFaturas => Set<CompraFatura>();
        public DbSet<CompraFaturaDuplicata> CompraFaturaDuplicatas => Set<CompraFaturaDuplicata>();
        public DbSet<CompraItemImposto> CompraItemImpostos => Set<CompraItemImposto>();
        public DbSet<CompraItemImpostoIbsCbs> CompraItemImpostosIbsCbs => Set<CompraItemImpostoIbsCbs>();
        public DbSet<CompraItemImpostoValorAproximado> CompraItemImpostosValorAproximado => Set<CompraItemImpostoValorAproximado>();

        // Compras — sub-entidades portadas
        public DbSet<CompraConfiguracao> CompraConfiguracoes => Set<CompraConfiguracao>();
        public DbSet<CompraEmitente> CompraEmitentes => Set<CompraEmitente>();
        public DbSet<CompraEmitenteEndereco> CompraEmitenteEnderecos => Set<CompraEmitenteEndereco>();
        public DbSet<CompraDestinatario> CompraDestinatarios => Set<CompraDestinatario>();
        public DbSet<CompraDestinatarioEndereco> CompraDestinatarioEnderecos => Set<CompraDestinatarioEndereco>();
        public DbSet<CompraEntrega> CompraEntregas => Set<CompraEntrega>();
        public DbSet<CompraCobrancaEndereco> CompraCobrancaEnderecos => Set<CompraCobrancaEndereco>();
        public DbSet<CompraPagamento> CompraPagamentos => Set<CompraPagamento>();
        public DbSet<CompraAutorizacaoXml> CompraAutorizacoesXml => Set<CompraAutorizacaoXml>();
        public DbSet<CompraNfeReferenciada> CompraNfeReferenciadas => Set<CompraNfeReferenciada>();
        public DbSet<CompraNfeHistorico> CompraNfeHistoricos => Set<CompraNfeHistorico>();
        public DbSet<CompraNfe> CompraNfes => Set<CompraNfe>();
        public DbSet<CompraNfeCartaCorrecao> CompraNfeCartasCorrecao => Set<CompraNfeCartaCorrecao>();
        public DbSet<CompraNfeIntermediador> CompraNfeIntermediadores => Set<CompraNfeIntermediador>();
        public DbSet<CompraTransporte> CompraTransportes => Set<CompraTransporte>();
        public DbSet<CompraTransporteTransportadora> CompraTransporteTransportadoras => Set<CompraTransporteTransportadora>();
        public DbSet<CompraTransporteVeiculo> CompraTransporteVeiculos => Set<CompraTransporteVeiculo>();
        public DbSet<CompraTransporteReboque> CompraTransporteReboques => Set<CompraTransporteReboque>();
        public DbSet<CompraTransporteVolume> CompraTransporteVolumes => Set<CompraTransporteVolume>();
        public DbSet<CompraItemCombustivel> CompraItemCombustiveis => Set<CompraItemCombustivel>();
        public DbSet<CompraItemCombustivelOrigem> CompraItemCombustivelOrigens => Set<CompraItemCombustivelOrigem>();
        public DbSet<CompraItemImportacao> CompraItemImportacoes => Set<CompraItemImportacao>();
        public DbSet<CompraItemImportacaoAdicao> CompraItemImportacaoAdicoes => Set<CompraItemImportacaoAdicao>();

        // Importações
        public DbSet<ImportacaoXml> ImportacoesXml => Set<ImportacaoXml>();
        public DbSet<ImportacaoArquivoXmlSaida> ImportacoesArquivoXmlSaida => Set<ImportacaoArquivoXmlSaida>();

        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        // ============ SOURCING E COMPRAS (EST-SC-001 / COM-GC-001) ============
        public DbSet<ScTipoRequisicao> ScTiposRequisicao => Set<ScTipoRequisicao>();
        public DbSet<ScRequisicao> ScRequisicoes => Set<ScRequisicao>();
        public DbSet<ScRequisicaoItem> ScRequisicaoItens => Set<ScRequisicaoItem>();
        public DbSet<ScCotacao> ScCotacoes => Set<ScCotacao>();
        public DbSet<ScCotacaoFornecedor> ScCotacaoFornecedores => Set<ScCotacaoFornecedor>();
        public DbSet<ScCotacaoItem> ScCotacaoItens => Set<ScCotacaoItem>();
        public DbSet<ScCotacaoPedidoItem> ScCotacaoPedidoItens => Set<ScCotacaoPedidoItem>();
        public DbSet<ScTipoPedido> ScTiposPedido => Set<ScTipoPedido>();
        public DbSet<ScPedidoCompra> ScPedidosCompra => Set<ScPedidoCompra>();
        public DbSet<ScPedidoCompraItem> ScPedidoCompraItens => Set<ScPedidoCompraItem>();

        // ============ LOGÍSTICA DE ENTRADA (EST-LDE) ============
        public DbSet<LdeEntrada> LdeEntradas => Set<LdeEntrada>();
        public DbSet<LdeLocalEntregaCompra> LdeLocaisEntregaCompra => Set<LdeLocalEntregaCompra>();
        public DbSet<LdeDocumentoEntrada> LdeDocumentosEntrada => Set<LdeDocumentoEntrada>();
        public DbSet<LdeDocumentoEntradaItem> LdeDocumentoEntradaItens => Set<LdeDocumentoEntradaItem>();
        public DbSet<LdeDocumentoEntradaDuplicata> LdeDocumentoEntradaDuplicatas => Set<LdeDocumentoEntradaDuplicata>();
        public DbSet<LdeDocumentoEntradaTransporte> LdeDocumentoEntradaTransportes => Set<LdeDocumentoEntradaTransporte>();
        public DbSet<LdeDocumentoEntradaFatura> LdeDocumentoEntradaFaturas => Set<LdeDocumentoEntradaFatura>();
        public DbSet<LdeHistorico> LdeHistoricos => Set<LdeHistorico>();

        // Lookup somente leitura (dono: módulo Fiscal, schema plataforma).
        public DbSet<ServicoLookup> ServicosLookup => Set<ServicoLookup>();

        public ContextEstoque(
            DbContextOptions<ContextEstoque> options,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser) : base(options, tenantProvider, currentUser)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define o schema do banco do PostgreSQL para o macrodomínio do estoque
            modelBuilder.HasDefaultSchema("estoque");

            // ============================ PRODUTOS ============================

            modelBuilder.Entity<ProdutoGrupo>(entity =>
            {
                entity.HasKey(g => g.Id);
                entity.Property(g => g.Descricao).HasMaxLength(100);
                entity.HasMany(g => g.Empresas)
                      .WithOne(e => e.ProdutoGrupo)
                      .HasForeignKey(e => e.ProdutoGrupoId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(g => new { g.TenantId, g.Descricao });
            });

            modelBuilder.Entity<ProdutoGrupoEmpresa>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.ProdutoGrupoId, e.EmpresaId }).IsUnique();
                entity.HasIndex(e => new { e.TenantId, e.EmpresaId });
            });

            modelBuilder.Entity<CategoriaProduto>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Descricao).HasMaxLength(150);
                entity.HasOne(c => c.ProdutoGrupo)
                      .WithMany()
                      .HasForeignKey(c => c.ProdutoGrupoId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasIndex(c => new { c.TenantId, c.Descricao });
            });

            modelBuilder.Entity<MarcaProduto>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Descricao).HasMaxLength(150);
                entity.HasOne(m => m.ProdutoGrupo)
                      .WithMany()
                      .HasForeignKey(m => m.ProdutoGrupoId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasIndex(m => new { m.TenantId, m.Descricao });
            });

            modelBuilder.Entity<UnidadeMedidaComercial>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.UnidadeMedida).HasMaxLength(6);
                entity.Property(u => u.Descricao).HasMaxLength(30);
                entity.Property(u => u.Fator).HasPrecision(18, 4);
                entity.HasOne(u => u.ProdutoGrupo)
                      .WithMany()
                      .HasForeignKey(u => u.ProdutoGrupoId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasIndex(u => new { u.TenantId, u.UnidadeMedida });
            });

            modelBuilder.Entity<UnidadeMedidaTributavel>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.CodigoNcm).HasMaxLength(8);
                entity.Property(u => u.UnidadeMedida).HasMaxLength(6);
                entity.Property(u => u.Descricao).HasMaxLength(120);
                entity.HasIndex(u => new { u.TenantId, u.CodigoNcm });
            });

            modelBuilder.Entity<Adicionais>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Descricao).HasMaxLength(60);
                entity.Property(a => a.ValorPreco).HasPrecision(18, 2);
                entity.HasIndex(a => new { a.TenantId, a.Descricao });
            });

            modelBuilder.Entity<AdicionaisProduto>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.HasOne(a => a.Produto)
                      .WithMany(p => p.AdicionaisProduto)
                      .HasForeignKey(a => a.ProdutoId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(a => a.Adicionais)
                      .WithMany(a => a.AdicionaisProdutos)
                      .HasForeignKey(a => a.AdicionaisId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(a => new { a.TenantId, a.ProdutoId });
            });

            modelBuilder.Entity<Balanca>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.Nome).HasMaxLength(60);
                entity.HasIndex(b => new { b.TenantId, b.Nome });
            });

            modelBuilder.Entity<ProdutoEspecifico>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.ValorPercentualGlpDerivadoPetroleo).HasPrecision(18, 2);
                entity.Property(p => p.ValorPercentualGasNaturalNacional).HasPrecision(18, 2);
                entity.Property(p => p.ValorPercentualGasNaturalImportado).HasPrecision(18, 2);
                entity.Property(p => p.ValorPartida).HasPrecision(18, 2);
                entity.HasOne(p => p.Produto)
                      .WithOne(pr => pr.ProdutoEspecifico)
                      .HasForeignKey<ProdutoEspecifico>(p => p.ProdutoId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(p => new { p.TenantId, p.ProdutoId });
            });

            modelBuilder.Entity<ProdutoEspecificoCombustivelOrigem>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.Property(o => o.ValorPercentualUf).HasPrecision(18, 2);
                entity.HasOne(o => o.ProdutoEspecifico)
                      .WithMany(p => p.Origens)
                      .HasForeignKey(o => o.ProdutoEspecificoId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(o => new { o.TenantId, o.ProdutoEspecificoId });
            });

            modelBuilder.Entity<ProdutoHistoricoReajuste>(entity =>
            {
                entity.HasKey(h => h.Id);
                entity.Property(h => h.CodigoProduto).HasMaxLength(60);
                entity.Property(h => h.Motivo).HasMaxLength(500);
                entity.Property(h => h.ValorAntigo).HasPrecision(18, 2);
                entity.Property(h => h.Fator).HasPrecision(18, 4);
                entity.Property(h => h.ValorFixo).HasPrecision(18, 2);
                entity.Property(h => h.ValorNovo).HasPrecision(18, 2);
                entity.HasOne(h => h.Produto)
                      .WithMany(p => p.ProdutoHistoricoReajustes)
                      .HasForeignKey(h => h.ProdutoId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(h => new { h.TenantId, h.ProdutoId });
                entity.HasIndex(h => h.SyncId).IsUnique();
            });

            modelBuilder.Entity<Produto>(entity =>
            {
                entity.HasKey(p => p.Id);
                // SKU deve ser exclusivo por tenant
                entity.HasIndex(p => new { p.TenantId, p.Sku }).IsUnique();

                entity.Property(p => p.PrecoVenda).HasPrecision(18, 2);
                entity.Property(p => p.SaldoEstoque).HasPrecision(18, 4);
                entity.Property(p => p.CustoMedio).HasPrecision(18, 2);
                entity.Property(p => p.PesoLiquido).HasPrecision(18, 3);
                entity.Property(p => p.PesoBruto).HasPrecision(18, 3);
                entity.Property(p => p.ValorVenda).HasPrecision(18, 2);
                entity.Property(p => p.ValorVendaPrazo).HasPrecision(18, 2);
                entity.Property(p => p.ValorCompra).HasPrecision(18, 2);
                entity.Property(p => p.Codigo).HasMaxLength(60);
                entity.Property(p => p.Descricao).HasMaxLength(120);
                entity.Property(p => p.Ean).HasMaxLength(14);
                entity.Property(p => p.CodigoProdutoBalanca).HasMaxLength(13);

                // Relacionamentos
                entity.HasOne(p => p.Categoria)
                      .WithMany()
                      .HasForeignKey(p => p.CategoriaId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(p => p.MarcaProduto)
                      .WithMany()
                      .HasForeignKey(p => p.MarcaProdutoId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(p => p.UnidadeMedidaComercial)
                      .WithMany()
                      .HasForeignKey(p => p.UnidadeMedidaComercialId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(p => p.ProdutoGrupo)
                      .WithMany()
                      .HasForeignKey(p => p.ProdutoGrupoId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(p => p.Balanca)
                      .WithMany(b => b.Produtos)
                      .HasForeignKey(p => p.BalancaId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ============================ ESTOQUE ============================

            modelBuilder.Entity<EstoqueProduto>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.QuantidadeSaldoEstoque).HasPrecision(18, 4);
                entity.Property(e => e.QuantidadeEstoqueMinimo).HasPrecision(18, 4);
                entity.Property(e => e.QuantidadeEstoqueMaximo).HasPrecision(18, 4);
                entity.Property(e => e.QuantidadeEstoqueReservado).HasPrecision(18, 4);
                entity.Property(e => e.ValorSaldo).HasPrecision(18, 2);
                entity.Property(e => e.ValorCustoMedio).HasPrecision(18, 2);
                entity.HasOne(e => e.Produto)
                      .WithMany()
                      .HasForeignKey(e => e.ProdutoId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => new { e.TenantId, e.EmpresaId, e.ProdutoId }).IsUnique();
            });

            modelBuilder.Entity<FatoGeradorEstoque>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.HasOne(f => f.Compra)
                      .WithMany()
                      .HasForeignKey(f => f.CompraId)
                      .OnDelete(DeleteBehavior.Restrict);
                // 1:1 opcional com o movimento manual (FK do lado do FatoGerador),
                // com a navegação inversa explícita em EstoqueMovimentoManual.
                entity.HasOne(f => f.EstoqueMovimentoManual)
                      .WithOne(m => m.FatoGeradorEstoque)
                      .HasForeignKey<FatoGeradorEstoque>(f => f.EstoqueMovimentoManualId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.Property(f => f.ReferenciaExterna).HasMaxLength(200);
                entity.HasIndex(f => new { f.TenantId, f.Origem });
                entity.HasIndex(f => f.SyncId).IsUnique();
            });

            modelBuilder.Entity<ProdutoFichaEstoqueEntrada>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.Property(f => f.QuantidadeMovimentada).HasPrecision(18, 4);
                entity.Property(f => f.ValorUnitario).HasPrecision(18, 4);
                entity.Property(f => f.QuantidadeSaldo).HasPrecision(18, 4);
                entity.Property(f => f.ValorSaldo).HasPrecision(18, 2);
                entity.Property(f => f.Lote).HasMaxLength(60);
                entity.HasOne(f => f.Produto)
                      .WithMany()
                      .HasForeignKey(f => f.ProdutoId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(f => f.FatoGeradorEstoque)
                      .WithMany(fg => fg.ProdutoFichaEstoqueEntradas)
                      .HasForeignKey(f => f.FatoGeradorEstoqueId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(f => new { f.TenantId, f.EmpresaId, f.ProdutoId });
                entity.HasIndex(f => f.SyncId).IsUnique();
            });

            modelBuilder.Entity<ProdutoFichaEstoqueSaida>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.Property(f => f.QuantidadeMovimentada).HasPrecision(18, 4);
                entity.Property(f => f.ValorUnitario).HasPrecision(18, 4);
                entity.Property(f => f.ValorTotal).HasPrecision(18, 2);
                entity.Property(f => f.ValorCustoMedio).HasPrecision(18, 2);
                entity.Property(f => f.ValorTotalCustoMedio).HasPrecision(18, 2);
                entity.HasOne(f => f.Produto)
                      .WithMany()
                      .HasForeignKey(f => f.ProdutoId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(f => f.FatoGeradorEstoque)
                      .WithMany(fg => fg.ProdutoFichaEstoqueSaidas)
                      .HasForeignKey(f => f.FatoGeradorEstoqueId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(f => f.ProdutoFichaEstoqueEntrada)
                      .WithMany(e => e.ProdutoFichaEstoqueSaidas)
                      .HasForeignKey(f => f.ProdutoFichaEstoqueEntradaId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(f => new { f.TenantId, f.EmpresaId, f.ProdutoId });
                entity.HasIndex(f => f.SyncId).IsUnique();
            });

            modelBuilder.Entity<EstoqueMovimentoManual>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.QuantidadeMovimentada).HasPrecision(18, 4);
                entity.Property(m => m.ValorUnitario).HasPrecision(18, 4);
                entity.Property(m => m.Motivo).HasMaxLength(500);
                entity.HasOne(m => m.Produto)
                      .WithMany()
                      .HasForeignKey(m => m.ProdutoId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(m => new { m.TenantId, m.ProdutoId });
                entity.HasIndex(m => new { m.TenantId, m.Situacao });
                entity.HasIndex(m => m.SyncId).IsUnique();
            });

            modelBuilder.Entity<MovimentoEstoque>(entity =>
            {
                entity.HasKey(m => m.Id);
            });

            // ============ MOVIMENTAÇÃO MANUAL E AJUSTES (EST-MVM-001) ============

            modelBuilder.Entity<TransferenciaEstoque>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.ValorFrete).HasPrecision(18, 2);
                entity.Property(t => t.Observacao).HasMaxLength(1000);
                entity.HasMany(t => t.Itens)
                      .WithOne(i => i.Transferencia)
                      .HasForeignKey(i => i.TransferenciaEstoqueId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(t => new { t.TenantId, t.Situacao });
                entity.HasIndex(t => t.SyncId).IsUnique();
            });

            modelBuilder.Entity<TransferenciaEstoqueItem>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Quantidade).HasPrecision(18, 4);
                entity.Property(i => i.ValorUnitario).HasPrecision(18, 4);
                entity.Property(i => i.Lote).HasMaxLength(60);
                entity.HasOne(i => i.Produto)
                      .WithMany()
                      .HasForeignKey(i => i.ProdutoId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(i => new { i.TenantId, i.TransferenciaEstoqueId });
                entity.HasIndex(i => i.SyncId).IsUnique();
            });

            modelBuilder.Entity<AjusteEstoque>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.ValorTotal).HasPrecision(18, 2);
                entity.Property(a => a.ValorRecuperado).HasPrecision(18, 2);
                entity.Property(a => a.Observacao).HasMaxLength(1000);
                entity.HasMany(a => a.Itens)
                      .WithOne(i => i.Ajuste)
                      .HasForeignKey(i => i.AjusteEstoqueId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(a => new { a.TenantId, a.Situacao });
                entity.HasIndex(a => a.SyncId).IsUnique();
            });

            modelBuilder.Entity<AjusteEstoqueItem>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Quantidade).HasPrecision(18, 4);
                entity.Property(i => i.ValorUnitario).HasPrecision(18, 4);
                entity.Property(i => i.Lote).HasMaxLength(60);
                entity.HasOne(i => i.Produto)
                      .WithMany()
                      .HasForeignKey(i => i.ProdutoId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(i => new { i.TenantId, i.AjusteEstoqueId });
                entity.HasIndex(i => i.SyncId).IsUnique();
            });

            modelBuilder.Entity<AvariaEstoque>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Codigo).HasMaxLength(60);
                entity.Property(a => a.Nome).HasMaxLength(120);
                entity.Property(a => a.PrecoCompra).HasPrecision(18, 2);
                entity.Property(a => a.Quantidade).HasPrecision(18, 4);
                entity.Property(a => a.Nota).HasMaxLength(4000);
                entity.Property(a => a.Referencia).HasMaxLength(60);
                entity.HasOne(a => a.Produto)
                      .WithMany()
                      .HasForeignKey(a => a.ProdutoId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(a => new { a.TenantId, a.ProdutoId });
                entity.HasIndex(a => a.SyncId).IsUnique();
            });

            modelBuilder.Entity<RequisicaoInterna>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.HasMany(r => r.Itens)
                      .WithOne(i => i.Requisicao)
                      .HasForeignKey(i => i.RequisicaoInternaId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(r => new { r.TenantId, r.ColaboradorId });
                entity.HasIndex(r => r.SyncId).IsUnique();
            });

            modelBuilder.Entity<RequisicaoInternaItem>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Quantidade).HasPrecision(18, 4);
                entity.HasOne(i => i.Produto)
                      .WithMany()
                      .HasForeignKey(i => i.ProdutoId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(i => new { i.TenantId, i.RequisicaoInternaId });
                entity.HasIndex(i => i.SyncId).IsUnique();
            });

            modelBuilder.Entity<SaldoInicialImportacao>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.ArquivoNome).HasMaxLength(260);
                entity.HasMany(s => s.Itens)
                      .WithOne(i => i.Importacao)
                      .HasForeignKey(i => i.SaldoInicialImportacaoId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(s => new { s.TenantId, s.Situacao });
                entity.HasIndex(s => s.SyncId).IsUnique();
            });

            modelBuilder.Entity<SaldoInicialItem>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.ProdutoCodigo).HasMaxLength(60);
                entity.Property(i => i.LocalNome).HasMaxLength(120);
                entity.Property(i => i.Quantidade).HasPrecision(18, 4);
                entity.Property(i => i.CustoUnitario).HasPrecision(18, 4);
                entity.Property(i => i.Lote).HasMaxLength(60);
                entity.Property(i => i.MensagemErro).HasMaxLength(500);
                entity.HasIndex(i => new { i.TenantId, i.SaldoInicialImportacaoId });
                entity.HasIndex(i => i.SyncId).IsUnique();
            });

            modelBuilder.Entity<HistoricoEstoque>(entity =>
            {
                entity.HasKey(h => h.Id);
                entity.Property(h => h.Entidade).HasMaxLength(100);
                entity.Property(h => h.Evento).HasMaxLength(100);
                entity.Property(h => h.SituacaoAnterior).HasMaxLength(60);
                entity.Property(h => h.SituacaoNova).HasMaxLength(60);
                entity.Property(h => h.Motivo).HasMaxLength(1000);
                entity.Property(h => h.UsuarioId).HasMaxLength(200);
                entity.HasIndex(h => new { h.TenantId, h.Entidade, h.EntidadeId });
                entity.HasIndex(h => h.SyncId).IsUnique();
            });

            // ============================ COMPRAS ============================

            modelBuilder.Entity<Compra>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.FornecedorCnpj).HasMaxLength(20);
                entity.Property(c => c.FornecedorNome).HasMaxLength(150);
                entity.Property(c => c.NumeroNota).HasMaxLength(20);
                entity.Property(c => c.ChaveAcesso).HasMaxLength(44);
                entity.Property(c => c.ValorTotal).HasPrecision(18, 2);
                // Status é enum tipado (EVendaStatus) — armazenado como int pelo EF.

                entity.Property(c => c.NaturezaOperacao).HasMaxLength(60);
                entity.Property(c => c.InformacoesComplementares).HasMaxLength(5000);
                entity.Property(c => c.InformacoesAdicionaisFisco).HasMaxLength(2000);
                entity.Property(c => c.FormaPagamento).HasMaxLength(50);

                entity.HasMany(c => c.Itens)
                      .WithOne(i => i.Compra)
                      .HasForeignKey(i => i.CompraId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(c => new { c.TenantId, c.ChaveAcesso }).IsUnique();
            });

            modelBuilder.Entity<CompraItem>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Quantidade).HasPrecision(18, 4);
                entity.Property(i => i.PrecoUnitario).HasPrecision(18, 4);
                entity.Property(i => i.ValorIms).HasPrecision(18, 2);
                entity.Property(i => i.ValorIpi).HasPrecision(18, 2);
                entity.Property(i => i.ValorTotal).HasPrecision(18, 2);

                entity.Property(i => i.CodigoProduto).HasMaxLength(60);
                entity.Property(i => i.CodigoEan).HasMaxLength(14);
                entity.Property(i => i.DescricaoProduto).HasMaxLength(120);
                entity.Property(i => i.Ncm).HasMaxLength(8);
                entity.Property(i => i.Cest).HasMaxLength(7);
                entity.Property(i => i.CodigoAnp).HasMaxLength(9);
                entity.Property(i => i.UnidadeComercial).HasMaxLength(6);
                entity.Property(i => i.QuantidadeComercial).HasPrecision(18, 4);
                entity.Property(i => i.ValorUnitarioComercial).HasPrecision(18, 4);
                entity.Property(i => i.ValorTotalBrutoProdutos).HasPrecision(18, 2);
                entity.Property(i => i.ValorDesconto).HasPrecision(18, 2);
                entity.Property(i => i.ValorFreteRateado).HasPrecision(18, 2);
                entity.Property(i => i.ValorCusto).HasPrecision(18, 2);

                // Campos tributáveis/rateados adicionados no porte fiel do legado
                entity.Property(i => i.ExcecaoNcmTipi).HasMaxLength(3);
                entity.Property(i => i.CodigoEanTributavel).HasMaxLength(14);
                entity.Property(i => i.UnidadeTributavel).HasMaxLength(6);
                entity.Property(i => i.QuantidadeTributavel).HasPrecision(18, 4);
                entity.Property(i => i.ValorUnitarioTributavel).HasPrecision(18, 4);
                entity.Property(i => i.ValorDescontoRateado).HasPrecision(18, 2);
                entity.Property(i => i.ValorSeguroRateado).HasPrecision(18, 2);
                entity.Property(i => i.ValorOutrasDespesasAcessoriasRateado).HasPrecision(18, 2);
                entity.Property(i => i.InformacoesAdicionaisDoProduto).HasMaxLength(500);
                entity.Property(i => i.NumeroPedidoCompra).HasMaxLength(60);
                entity.Property(i => i.FichaConteudoImportacao).HasMaxLength(36);
                entity.Property(i => i.CodigoBeneficioFiscal).HasMaxLength(10);

                // Agregados/satélites do item (1:1 e 1:N intra-módulo)
                entity.HasOne(i => i.Imposto)
                      .WithOne(x => x.CompraItem)
                      .HasForeignKey<CompraItemImposto>(x => x.CompraItemId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(i => i.ImpostoIbsCbs)
                      .WithOne(x => x.CompraItem)
                      .HasForeignKey<CompraItemImpostoIbsCbs>(x => x.CompraItemId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(i => i.ImpostoValorAproximado)
                      .WithOne(x => x.CompraItem)
                      .HasForeignKey<CompraItemImpostoValorAproximado>(x => x.CompraItemId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(i => i.Combustivel)
                      .WithOne(x => x.CompraItem)
                      .HasForeignKey<CompraItemCombustivel>(x => x.CompraItemId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(i => i.Importacoes)
                      .WithOne(x => x.CompraItem)
                      .HasForeignKey(x => x.CompraItemId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(i => i.SyncId)
                      .IsUnique()
                      .HasDatabaseName("uq_compra_itens_sync_id");
            });

            modelBuilder.Entity<CompraTotal>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.ValorBaseDeCalculoIcms).HasPrecision(18, 2);
                entity.Property(t => t.ValorIcms).HasPrecision(18, 2);
                entity.Property(t => t.ValorIcmsDesonerado).HasPrecision(18, 2);
                entity.Property(t => t.ValorFcp).HasPrecision(18, 2);
                entity.Property(t => t.ValorBaseDeCalculoSt).HasPrecision(18, 2);
                entity.Property(t => t.ValorSt).HasPrecision(18, 2);
                entity.Property(t => t.ValorFcpSt).HasPrecision(18, 2);
                entity.Property(t => t.ValorFcpRetido).HasPrecision(18, 2);
                entity.Property(t => t.ValorProduto).HasPrecision(18, 2);
                entity.Property(t => t.ValorFrete).HasPrecision(18, 2);
                entity.Property(t => t.ValorSeguro).HasPrecision(18, 2);
                entity.Property(t => t.ValorDesconto).HasPrecision(18, 2);
                entity.Property(t => t.ValorImpostoImportacao).HasPrecision(18, 2);
                entity.Property(t => t.ValorIpi).HasPrecision(18, 2);
                entity.Property(t => t.ValorIpiDevolucao).HasPrecision(18, 2);
                entity.Property(t => t.ValorPis).HasPrecision(18, 2);
                entity.Property(t => t.ValorCofins).HasPrecision(18, 2);
                entity.Property(t => t.ValorOutro).HasPrecision(18, 2);
                entity.Property(t => t.ValorNotaFiscal).HasPrecision(18, 2);
                entity.HasOne(t => t.Compra)
                      .WithOne(c => c.Total)
                      .HasForeignKey<CompraTotal>(t => t.CompraId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(t => t.SyncId).IsUnique();
            });

            modelBuilder.Entity<CompraTotalIbsCbs>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.ValorBaseDeCalculo).HasPrecision(18, 2);
                entity.Property(t => t.ValorImpostoDevidoEstadual).HasPrecision(18, 2);
                entity.Property(t => t.ValorImpostoDevidoMunicipal).HasPrecision(18, 2);
                entity.Property(t => t.ValorImpostoDevidoCbs).HasPrecision(18, 2);
                entity.HasOne(t => t.Compra)
                      .WithOne()
                      .HasForeignKey<CompraTotalIbsCbs>(t => t.CompraId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(t => t.SyncId).IsUnique();
            });

            modelBuilder.Entity<CompraImposto>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.ValorAliquotaCreditoIcms).HasPrecision(18, 2);
                // Correção do mapping FK CompraImposto->Compra (1:1, cascade). Antes o relacionamento
                // não era declarado, deixando a navegação Compra.Imposto sem FK configurada.
                entity.HasOne(i => i.Compra)
                      .WithOne(c => c.Imposto)
                      .HasForeignKey<CompraImposto>(i => i.CompraId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(i => new { i.TenantId, i.CompraId });
                entity.HasIndex(i => i.SyncId).IsUnique();
            });

            modelBuilder.Entity<CompraFatura>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.Property(f => f.NumeroFatura).HasMaxLength(60);
                entity.Property(f => f.ValorOriginal).HasPrecision(18, 2);
                entity.Property(f => f.ValorDesconto).HasPrecision(18, 2);
                entity.Property(f => f.ValorLiquido).HasPrecision(18, 2);
                entity.HasOne(f => f.Compra)
                      .WithOne(c => c.Fatura)
                      .HasForeignKey<CompraFatura>(f => f.CompraId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(f => f.Duplicatas)
                      .WithOne(d => d.CompraFatura)
                      .HasForeignKey(d => d.CompraFaturaId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(f => f.SyncId).IsUnique();
            });

            modelBuilder.Entity<CompraFaturaDuplicata>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.NumeroDuplicata).HasMaxLength(60);
                entity.Property(d => d.ValorDuplicata).HasPrecision(18, 2);
                entity.HasIndex(d => new { d.TenantId, d.CompraFaturaId });
                entity.HasIndex(d => d.SyncId).IsUnique();
            });

            modelBuilder.Entity<CompraItemImpostoIbsCbs>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Cst).HasMaxLength(4);
                entity.Property(i => i.CClassTrib).HasMaxLength(10);
                entity.Property(i => i.AliquotaEstadual).HasPrecision(18, 4);
                entity.Property(i => i.AliquotaMunicipal).HasPrecision(18, 4);
                entity.Property(i => i.AliquotaCbs).HasPrecision(18, 4);
                entity.Property(i => i.AliquotaEstadualReducao).HasPrecision(18, 4);
                entity.Property(i => i.AliquotaMunicipalReducao).HasPrecision(18, 4);
                entity.Property(i => i.AliquotaCbsReducao).HasPrecision(18, 4);
                entity.Property(i => i.AliquotaEstadualDiferimento).HasPrecision(18, 4);
                entity.Property(i => i.AliquotaMunicipalDiferimento).HasPrecision(18, 4);
                entity.Property(i => i.AliquotaCbsDiferimento).HasPrecision(18, 4);
                entity.Property(i => i.AliquotaEfetivaEstadual).HasPrecision(18, 4);
                entity.Property(i => i.AliquotaEfetivaMunicipal).HasPrecision(18, 4);
                entity.Property(i => i.AliquotaEfetivaCbs).HasPrecision(18, 4);
                entity.Property(i => i.ValorBaseDeCalculo).HasPrecision(18, 2);
                entity.Property(i => i.ValorImpostoDevidoEstadual).HasPrecision(18, 2);
                entity.Property(i => i.ValorImpostoDevidoMunicipal).HasPrecision(18, 2);
                entity.Property(i => i.ValorImpostoDevidoCbs).HasPrecision(18, 2);
                entity.HasIndex(i => new { i.TenantId, i.CompraItemId });
                entity.HasIndex(i => i.SyncId).IsUnique();
            });

            modelBuilder.Entity<CompraItemImpostoValorAproximado>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.AliquotaNacionalFederal).HasPrecision(18, 2);
                entity.Property(i => i.AliquotaImportadoFederal).HasPrecision(18, 2);
                entity.Property(i => i.AliquotaEstadual).HasPrecision(18, 2);
                entity.Property(i => i.AliquotaMunicipal).HasPrecision(18, 2);
                entity.Property(i => i.Versao).HasMaxLength(10);
                entity.Property(i => i.Fonte).HasMaxLength(60);
                entity.HasIndex(i => new { i.TenantId, i.CompraItemId });
                entity.HasIndex(i => i.SyncId).IsUnique();
            });

            modelBuilder.Entity<CompraItemImposto>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.ValorBaseDeCalculoIcms).HasPrecision(18, 2);
                entity.Property(i => i.PercentualReducaoBaseDeCalculoIcms).HasPrecision(9, 4);
                entity.Property(i => i.AliquotaIcms).HasPrecision(9, 4);
                entity.Property(i => i.ValorImpostoIcms).HasPrecision(18, 2);
                entity.Property(i => i.PercentualMvaBaseDeCalculoST).HasPrecision(9, 4);
                entity.Property(i => i.PercentualReducaoBaseDeCalculoST).HasPrecision(9, 4);
                entity.Property(i => i.ValorBaseDeCalculoSt).HasPrecision(18, 2);
                entity.Property(i => i.AliquotaSt).HasPrecision(9, 4);
                entity.Property(i => i.ValorImpostoSt).HasPrecision(18, 2);
                entity.Property(i => i.ValorBaseDeCalculoStRetido).HasPrecision(18, 2);
                entity.Property(i => i.ValorImpostoStRetido).HasPrecision(18, 2);
                entity.Property(i => i.PercentualCreditoSimplesNacionalIcms).HasPrecision(9, 4);
                entity.Property(i => i.ValorImpostoCreditoSimplesNacionalIcms).HasPrecision(18, 2);
                entity.Property(i => i.ValorBaseDeCalculoFcp).HasPrecision(18, 2);
                entity.Property(i => i.PercentualFcp).HasPrecision(9, 4);
                entity.Property(i => i.ValorImpostoFcp).HasPrecision(18, 2);
                entity.Property(i => i.ValorOperacaoDiferimentoIcms).HasPrecision(18, 2);
                entity.Property(i => i.PercentualDiferimentoIcms).HasPrecision(9, 4);
                entity.Property(i => i.ValorImpostoDiferimentoIcms).HasPrecision(18, 2);
                entity.Property(i => i.ValorBaseDeCalculoIpi).HasPrecision(18, 2);
                entity.Property(i => i.AliquotaIpi).HasPrecision(9, 4);
                entity.Property(i => i.ValorImpostoDiferimentoIpi).HasPrecision(18, 2);
                entity.Property(i => i.ValorQuantidadeTotalParaTributacaoIpi).HasPrecision(18, 4);
                entity.Property(i => i.ValorPorUnidadeTributavelIpi).HasPrecision(18, 4);
                entity.Property(i => i.ValorBaseDeCalculoPis).HasPrecision(18, 2);
                entity.Property(i => i.AliquotaPis).HasPrecision(9, 4);
                entity.Property(i => i.ValorQuantidadeVendidaProdutoPis).HasPrecision(18, 4);
                entity.Property(i => i.AliquotaPorUnidadeVendidaPis).HasPrecision(18, 4);
                entity.Property(i => i.ValorImpostoDiferimentoPis).HasPrecision(18, 2);
                entity.Property(i => i.ValorBaseDeCalculoCofins).HasPrecision(18, 2);
                entity.Property(i => i.AliquotaCofins).HasPrecision(9, 4);
                entity.Property(i => i.ValorQuantidadeVendidaProdutoCofins).HasPrecision(18, 4);
                entity.Property(i => i.AliquotaPorUnidadeVendidaCofins).HasPrecision(18, 4);
                entity.Property(i => i.ValorImpostoDiferimentoCofins).HasPrecision(18, 2);
                entity.Property(i => i.ValorBaseDeCalculoFcpSt).HasPrecision(18, 2);
                entity.Property(i => i.PercentualFcpSt).HasPrecision(9, 4);
                entity.Property(i => i.ValorImpostoFcpSt).HasPrecision(18, 2);
                entity.Property(i => i.ValorIcmsProprioSubistituto).HasPrecision(18, 2);
                entity.Property(i => i.ValorAliquotaIcmsInterna).HasPrecision(18, 2);
                entity.Property(i => i.ValorAliquotaIcmsInternaEstadual).HasPrecision(18, 2);
                entity.Property(i => i.ValorReducaoIpiPercentual).HasPrecision(18, 2);
                entity.Property(i => i.ValorUnitFixadoIcmsSt).HasPrecision(18, 2);
                entity.Property(i => i.ValorBaseDeCalculoDifal).HasPrecision(18, 2);
                entity.Property(i => i.ValorImpostoDevidoDifal).HasPrecision(18, 2);
                entity.Property(i => i.ValorImpostoDevidoRecolherSt).HasPrecision(18, 2);
                entity.Property(i => i.ValorImpostoDevidoFcp).HasPrecision(18, 2);
                entity.Property(i => i.ValorIcmsIsento).HasPrecision(18, 2);
                entity.Property(i => i.ValorIcmsOutros).HasPrecision(18, 2);
                entity.Property(i => i.IcmsObservacao).HasMaxLength(500);
                entity.Property(i => i.ValorIpiIsento).HasPrecision(18, 2);
                entity.Property(i => i.ValorIpiOutros).HasPrecision(18, 2);
                entity.Property(i => i.IpiObservacao).HasMaxLength(500);
                entity.HasIndex(i => new { i.TenantId, i.CompraItemId });
                entity.HasIndex(i => i.SyncId).IsUnique();
            });

            // ==================== COMPRAS — SUB-ENTIDADES PORTADAS ====================

            modelBuilder.Entity<CompraConfiguracao>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasOne(c => c.Compra)
                      .WithOne()
                      .HasForeignKey<CompraConfiguracao>(c => c.CompraId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(c => new { c.TenantId, c.CompraId });
                entity.HasIndex(c => c.SyncId).IsUnique();
            });

            modelBuilder.Entity<CompraEmitente>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Cnpj).HasMaxLength(14);
                entity.Property(e => e.Cpf).HasMaxLength(11);
                entity.Property(e => e.RazaoSocial).HasMaxLength(60);
                entity.Property(e => e.NomeFantasia).HasMaxLength(60);
                entity.Property(e => e.Telefone).HasMaxLength(14);
                entity.Property(e => e.InscricaoEstadual).HasMaxLength(20);
                entity.Property(e => e.InscricaoEstadualST).HasMaxLength(14);
                entity.Property(e => e.InscricaoMunicipal).HasMaxLength(15);
                entity.HasOne(e => e.Compra)
                      .WithOne(c => c.Emitente)
                      .HasForeignKey<CompraEmitente>(e => e.CompraId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Endereco)
                      .WithOne(en => en.CompraEmitente)
                      .HasForeignKey<CompraEmitenteEndereco>(en => en.CompraEmitenteId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => new { e.TenantId, e.CompraId });
                entity.HasIndex(e => e.SyncId).IsUnique();
            });

            modelBuilder.Entity<CompraEmitenteEndereco>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Logradouro).HasMaxLength(60);
                entity.Property(e => e.Numero).HasMaxLength(60);
                entity.Property(e => e.Complemento).HasMaxLength(60);
                entity.Property(e => e.Bairro).HasMaxLength(60);
                entity.Property(e => e.MunicipioNome).HasMaxLength(60);
                entity.Property(e => e.Cep).HasMaxLength(8);
                entity.Property(e => e.PaisNome).HasMaxLength(60);
                entity.HasIndex(e => new { e.TenantId, e.CompraEmitenteId });
                entity.HasIndex(e => e.SyncId).IsUnique();
            });

            modelBuilder.Entity<CompraDestinatario>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Cnpj).HasMaxLength(14);
                entity.Property(d => d.Cpf).HasMaxLength(11);
                entity.Property(d => d.RazaoSocial).HasMaxLength(60);
                entity.Property(d => d.Telefone).HasMaxLength(14);
                entity.Property(d => d.InscricaoEstadual).HasMaxLength(14);
                entity.Property(d => d.IdentificadorEstrangeiro).HasMaxLength(20);
                entity.Property(d => d.Email).HasMaxLength(60);
                entity.HasOne(d => d.Compra)
                      .WithOne(c => c.Destinatario)
                      .HasForeignKey<CompraDestinatario>(d => d.CompraId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(d => d.Enderecos)
                      .WithOne(en => en.CompraDestinatario)
                      .HasForeignKey(en => en.CompraDestinatarioId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(d => new { d.TenantId, d.CompraId });
                entity.HasIndex(d => d.SyncId).IsUnique();
            });

            modelBuilder.Entity<CompraDestinatarioEndereco>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Logradouro).HasMaxLength(60);
                entity.Property(e => e.Numero).HasMaxLength(60);
                entity.Property(e => e.Complemento).HasMaxLength(60);
                entity.Property(e => e.Bairro).HasMaxLength(60);
                entity.Property(e => e.MunicipioNome).HasMaxLength(60);
                entity.Property(e => e.Cep).HasMaxLength(8);
                entity.Property(e => e.PaisNome).HasMaxLength(60);
                entity.HasIndex(e => new { e.TenantId, e.CompraDestinatarioId });
                entity.HasIndex(e => e.SyncId).IsUnique();
            });

            modelBuilder.Entity<CompraEntrega>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nome).HasMaxLength(60);
                entity.Property(e => e.Fone).HasMaxLength(14);
                entity.Property(e => e.Email).HasMaxLength(60);
                entity.Property(e => e.IE).HasMaxLength(20);
                entity.Property(e => e.Documento).HasMaxLength(14);
                entity.Property(e => e.Logradouro).HasMaxLength(60);
                entity.Property(e => e.Numero).HasMaxLength(60);
                entity.Property(e => e.Complemento).HasMaxLength(60);
                entity.Property(e => e.Bairro).HasMaxLength(60);
                entity.Property(e => e.MunicipioNome).HasMaxLength(60);
                entity.Property(e => e.Cep).HasMaxLength(8);
                entity.Property(e => e.PaisNome).HasMaxLength(60);
                entity.HasOne(e => e.Compra)
                      .WithOne()
                      .HasForeignKey<CompraEntrega>(e => e.CompraId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => new { e.TenantId, e.CompraId });
                entity.HasIndex(e => e.SyncId).IsUnique();
            });

            modelBuilder.Entity<CompraCobrancaEndereco>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nome).HasMaxLength(60);
                entity.Property(e => e.Fone).HasMaxLength(14);
                entity.Property(e => e.Email).HasMaxLength(60);
                entity.Property(e => e.IE).HasMaxLength(20);
                entity.Property(e => e.Documento).HasMaxLength(14);
                entity.Property(e => e.Logradouro).HasMaxLength(60);
                entity.Property(e => e.Numero).HasMaxLength(60);
                entity.Property(e => e.Complemento).HasMaxLength(60);
                entity.Property(e => e.Bairro).HasMaxLength(60);
                entity.Property(e => e.MunicipioNome).HasMaxLength(60);
                entity.Property(e => e.Cep).HasMaxLength(8);
                entity.Property(e => e.PaisNome).HasMaxLength(60);
                entity.HasOne(e => e.Compra)
                      .WithOne()
                      .HasForeignKey<CompraCobrancaEndereco>(e => e.CompraId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => new { e.TenantId, e.CompraId });
                entity.HasIndex(e => e.SyncId).IsUnique();
            });

            modelBuilder.Entity<CompraPagamento>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.ValorTroco).HasPrecision(18, 2);
                entity.Property(p => p.ValorPagamento).HasPrecision(18, 2);
                entity.Property(p => p.CartaoCnpjIntermediadorFinanceira).HasMaxLength(14);
                entity.Property(p => p.CartaoCodigoAutorizacaoOperacao).HasMaxLength(20);
                entity.HasOne(p => p.Compra)
                      .WithMany(c => c.Pagamentos)
                      .HasForeignKey(p => p.CompraId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(p => new { p.TenantId, p.CompraId });
                entity.HasIndex(p => p.SyncId).IsUnique();
            });

            modelBuilder.Entity<CompraAutorizacaoXml>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Documento).HasMaxLength(14);
                entity.HasOne(a => a.Compra)
                      .WithMany()
                      .HasForeignKey(a => a.CompraId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(a => new { a.TenantId, a.CompraId });
                entity.HasIndex(a => a.SyncId).IsUnique();
            });

            modelBuilder.Entity<CompraNfeReferenciada>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Chave).HasMaxLength(44);
                entity.HasOne(r => r.Compra)
                      .WithMany()
                      .HasForeignKey(r => r.CompraId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(r => new { r.TenantId, r.CompraId });
                entity.HasIndex(r => r.SyncId).IsUnique();
            });

            modelBuilder.Entity<CompraNfeHistorico>(entity =>
            {
                entity.HasKey(h => h.Id);
                entity.HasOne(h => h.Compra)
                      .WithMany()
                      .HasForeignKey(h => h.CompraId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(h => new { h.TenantId, h.CompraId });
                entity.HasIndex(h => h.SyncId).IsUnique();
            });

            modelBuilder.Entity<CompraNfe>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.Property(n => n.Chave).HasMaxLength(44);
                entity.Property(n => n.Protocolo).HasMaxLength(60);
                entity.Property(n => n.UltimoRetornoMensagemSefaz).HasMaxLength(300);
                entity.Property(n => n.ProtocoloCancelamento).HasMaxLength(60);
                entity.Property(n => n.MotivoCancelamento).HasMaxLength(300);
                entity.HasOne(n => n.Compra)
                      .WithOne(c => c.Nfe)
                      .HasForeignKey<CompraNfe>(n => n.CompraId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(n => n.Intermediador)
                      .WithOne(i => i.CompraNfe)
                      .HasForeignKey<CompraNfeIntermediador>(i => i.CompraNfeId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(n => n.CartasCorrecoes)
                      .WithOne(c => c.CompraNfe)
                      .HasForeignKey(c => c.CompraNfeId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(n => new { n.TenantId, n.CompraId });
                entity.HasIndex(n => n.SyncId).IsUnique();
            });

            modelBuilder.Entity<CompraNfeCartaCorrecao>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.TextoCorrecao).HasMaxLength(1000);
                entity.Property(c => c.MotivoRejeicaoSefaz).HasMaxLength(300);
                entity.HasIndex(c => new { c.TenantId, c.CompraNfeId });
                entity.HasIndex(c => c.SyncId).IsUnique();
            });

            modelBuilder.Entity<CompraNfeIntermediador>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Documento).HasMaxLength(14);
                entity.Property(i => i.IdentificadorIntermediador).HasMaxLength(60);
                entity.HasIndex(i => new { i.TenantId, i.CompraNfeId });
                entity.HasIndex(i => i.SyncId).IsUnique();
            });

            modelBuilder.Entity<CompraTransporte>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.HasOne(t => t.Compra)
                      .WithOne(c => c.Transporte)
                      .HasForeignKey<CompraTransporte>(t => t.CompraId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.Transportadora)
                      .WithOne(x => x.Transporte)
                      .HasForeignKey<CompraTransporteTransportadora>(x => x.CompraTransporteId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.Veiculo)
                      .WithOne(x => x.Transporte)
                      .HasForeignKey<CompraTransporteVeiculo>(x => x.CompraTransporteId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(t => t.Volumes)
                      .WithOne(x => x.CompraTransporte)
                      .HasForeignKey(x => x.CompraTransporteId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(t => t.Reboques)
                      .WithOne(x => x.Transporte)
                      .HasForeignKey(x => x.CompraTransporteId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(t => new { t.TenantId, t.CompraId });
                entity.HasIndex(t => t.SyncId).IsUnique();
            });

            modelBuilder.Entity<CompraTransporteTransportadora>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Cnpj).HasMaxLength(14);
                entity.Property(t => t.Cpf).HasMaxLength(11);
                entity.Property(t => t.RazaoSocial).HasMaxLength(60);
                entity.Property(t => t.InscricaoEstadual).HasMaxLength(20);
                entity.Property(t => t.EnderecoCompleto).HasMaxLength(60);
                entity.Property(t => t.NomeMunicipio).HasMaxLength(60);
                entity.HasIndex(t => new { t.TenantId, t.CompraTransporteId });
                entity.HasIndex(t => t.SyncId).IsUnique();
            });

            modelBuilder.Entity<CompraTransporteVeiculo>(entity =>
            {
                entity.HasKey(v => v.Id);
                entity.Property(v => v.Placa).HasMaxLength(8);
                entity.Property(v => v.Rntrc).HasMaxLength(14);
                entity.HasIndex(v => new { v.TenantId, v.CompraTransporteId });
                entity.HasIndex(v => v.SyncId).IsUnique();
            });

            modelBuilder.Entity<CompraTransporteReboque>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Placa).HasMaxLength(8);
                entity.Property(r => r.Rntrc).HasMaxLength(14);
                entity.HasOne(r => r.Transporte)
                      .WithMany(t => t.Reboques)
                      .HasForeignKey(r => r.CompraTransporteId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(r => new { r.TenantId, r.CompraTransporteId });
                entity.HasIndex(r => r.SyncId).IsUnique();
            });

            modelBuilder.Entity<CompraTransporteVolume>(entity =>
            {
                entity.HasKey(v => v.Id);
                entity.Property(v => v.Especie).HasMaxLength(60);
                entity.Property(v => v.NumeroVolumes).HasMaxLength(60);
                entity.Property(v => v.Marca).HasMaxLength(60);
                entity.Property(v => v.PesoLiquido).HasPrecision(18, 3);
                entity.Property(v => v.PesoBruto).HasPrecision(18, 3);
                entity.HasIndex(v => new { v.TenantId, v.CompraTransporteId });
                entity.HasIndex(v => v.SyncId).IsUnique();
            });

            modelBuilder.Entity<CompraItemCombustivel>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.CodigoAnp).HasMaxLength(9);
                entity.Property(c => c.DescricaoAnp).HasMaxLength(95);
                entity.Property(c => c.QuantidadeCombustivelFaturada).HasPrecision(18, 4);
                entity.Property(c => c.PercentualGlpDerivadoPetroleo).HasPrecision(18, 2);
                entity.Property(c => c.PercentualGasNaturalNacional).HasPrecision(18, 2);
                entity.Property(c => c.PercentualGasNaturalImportado).HasPrecision(18, 2);
                entity.Property(c => c.ValorPartida).HasPrecision(18, 2);
                entity.HasMany(c => c.Origens)
                      .WithOne(o => o.CompraItemCombustivel)
                      .HasForeignKey(o => o.CompraItemCombustivelId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(c => new { c.TenantId, c.CompraItemId });
                entity.HasIndex(c => c.SyncId).IsUnique();
            });

            modelBuilder.Entity<CompraItemCombustivelOrigem>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.Property(o => o.PercentualOrigem).HasPrecision(18, 2);
                entity.HasIndex(o => new { o.TenantId, o.CompraItemCombustivelId });
                entity.HasIndex(o => o.SyncId).IsUnique();
            });

            modelBuilder.Entity<CompraItemImportacao>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.NumeroDeclaracaoImportacao).HasMaxLength(12);
                entity.Property(i => i.LocalDesembaraco).HasMaxLength(60);
                entity.Property(i => i.UfDesembaraco).HasMaxLength(2);
                entity.Property(i => i.ValorAFRMM).HasPrecision(18, 2);
                entity.Property(i => i.Cnpj).HasMaxLength(14);
                entity.Property(i => i.Cpf).HasMaxLength(11);
                entity.Property(i => i.UfTerceiro).HasMaxLength(2);
                entity.Property(i => i.CodigoExportador).HasMaxLength(60);
                entity.HasMany(i => i.Adicoes)
                      .WithOne(a => a.CompraItemImportacao)
                      .HasForeignKey(a => a.CompraItemImportacaoId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(i => new { i.TenantId, i.CompraItemId });
                entity.HasIndex(i => i.SyncId).IsUnique();
            });

            modelBuilder.Entity<CompraItemImportacaoAdicao>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.CodigoFabricante).HasMaxLength(60);
                entity.Property(a => a.NumeroAtoConcessorio).HasMaxLength(20);
                entity.Property(a => a.ValorDesconto).HasPrecision(18, 2);
                entity.HasIndex(a => new { a.TenantId, a.CompraItemImportacaoId });
                entity.HasIndex(a => a.SyncId).IsUnique();
            });

            // ============================ IMPORTAÇÕES ============================

            modelBuilder.Entity<ImportacaoXml>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.NfeId).HasMaxLength(44);
                entity.Property(i => i.TipoEvento).HasMaxLength(60);
                entity.Property(i => i.MensagemErroImportacaoXml).HasMaxLength(500);
                entity.Property(i => i.MensagemErroCadastro).HasMaxLength(500);
                entity.Property(i => i.MensagemErroSalvarPdf).HasMaxLength(500);
                entity.HasIndex(i => new { i.TenantId, i.NfeId });
                entity.HasIndex(i => new { i.TenantId, i.EmpresaId });
                entity.HasIndex(i => i.SyncId).IsUnique();
            });

            modelBuilder.Entity<ImportacaoArquivoXmlSaida>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.NomeArquivo).HasMaxLength(260);
                entity.Property(i => i.MensagemErro).HasMaxLength(500);
                entity.HasIndex(i => new { i.TenantId, i.Status });
                entity.HasIndex(i => i.SyncId).IsUnique();
            });

            modelBuilder.Entity<OutboxMessage>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.ToTable("outbox_messages", "estoque");
            });

            // ============================ LOOKUPS (read-only cross-module) ============================
            // Servico é do módulo Fiscal (schema plataforma). Mapeado como lookup somente leitura para a
            // tela de compras (compras-dados/obter-servicos-por-ids). Sem navegação de projeto entre módulos.
            modelBuilder.Entity<ServicoLookup>(entity =>
            {
                entity.ToTable("servicos", "plataforma", t => t.ExcludeFromMigrations());
                entity.HasKey(s => s.Id);
                entity.HasQueryFilter(s => s.DeletadoEm == null);
                entity.Property(s => s.Valor).HasPrecision(18, 2);
                entity.Property(s => s.AliquotaIss).HasPrecision(18, 4);
                entity.Property(s => s.AliquotaIssRetido).HasPrecision(18, 4);
                entity.Property(s => s.AliquotaIrrfRetido).HasPrecision(18, 4);
                entity.Property(s => s.AliquotaInss).HasPrecision(18, 4);
                entity.Property(s => s.AliquotaPis).HasPrecision(18, 4);
                entity.Property(s => s.AliquotaCofins).HasPrecision(18, 4);
                entity.Property(s => s.Codigo).HasMaxLength(10);
                entity.Property(s => s.Descricao).HasMaxLength(120);
                entity.Property(s => s.InformacaoAdicional).HasMaxLength(120);
                entity.Property(s => s.CodigoNbs).HasMaxLength(9);
                entity.Property(s => s.CstIbsCbs).HasMaxLength(5000);
                entity.Property(s => s.CClassTrib).HasMaxLength(5000);
            });

            // ============ SOURCING E COMPRAS (EST-SC-001 / COM-GC-001) ============

            modelBuilder.Entity<ScTipoRequisicao>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Descricao).HasMaxLength(120);
                entity.HasIndex(t => new { t.TenantId, t.Descricao });
                entity.HasIndex(t => t.SyncId).IsUnique();
            });

            modelBuilder.Entity<ScRequisicao>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.HasOne(r => r.TipoRequisicao)
                      .WithMany()
                      .HasForeignKey(r => r.TipoRequisicaoId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(r => r.Itens)
                      .WithOne(i => i.Requisicao)
                      .HasForeignKey(i => i.RequisicaoId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(r => new { r.TenantId, r.ColaboradorId });
                entity.HasIndex(r => r.SyncId).IsUnique();
            });

            modelBuilder.Entity<ScRequisicaoItem>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Quantidade).HasPrecision(18, 4);
                entity.Property(i => i.QuantidadeCotada).HasPrecision(18, 4);
                entity.Property(i => i.ItemCotado).HasMaxLength(60);
                entity.HasOne(i => i.Produto)
                      .WithMany()
                      .HasForeignKey(i => i.ProdutoId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(i => new { i.TenantId, i.RequisicaoId });
                entity.HasIndex(i => i.SyncId).IsUnique();
            });

            modelBuilder.Entity<ScCotacao>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Descricao).HasMaxLength(500);
                entity.Property(c => c.Situacao).HasMaxLength(60);
                entity.HasMany(c => c.Fornecedores)
                      .WithOne(f => f.Cotacao)
                      .HasForeignKey(f => f.CotacaoId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(c => c.Itens)
                      .WithOne(i => i.Cotacao)
                      .HasForeignKey(i => i.CotacaoId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(c => new { c.TenantId, c.Situacao });
                entity.HasIndex(c => c.SyncId).IsUnique();
            });

            modelBuilder.Entity<ScCotacaoFornecedor>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.Property(f => f.PrazoEntrega).HasMaxLength(120);
                entity.Property(f => f.CondicoesPagamento).HasMaxLength(500);
                entity.Property(f => f.Subtotal).HasPrecision(18, 2);
                entity.Property(f => f.Desconto).HasPrecision(18, 2);
                entity.Property(f => f.Total).HasPrecision(18, 2);
                entity.HasIndex(f => new { f.TenantId, f.CotacaoId });
                entity.HasIndex(f => new { f.TenantId, f.FornecedorId });
                entity.HasIndex(f => f.SyncId).IsUnique();
            });

            modelBuilder.Entity<ScCotacaoItem>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Quantidade).HasPrecision(18, 4);
                entity.Property(i => i.ValorUnitario).HasPrecision(18, 4);
                entity.Property(i => i.ValorDesconto).HasPrecision(18, 2);
                entity.Property(i => i.ValorTotal).HasPrecision(18, 2);
                entity.HasOne(i => i.Produto)
                      .WithMany()
                      .HasForeignKey(i => i.ProdutoId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(i => new { i.TenantId, i.CotacaoId });
                entity.HasIndex(i => i.SyncId).IsUnique();
            });

            modelBuilder.Entity<ScCotacaoPedidoItem>(entity =>
            {
                entity.HasKey(cp => cp.Id);
                entity.HasIndex(cp => new { cp.TenantId, cp.CotacaoItemId });
                entity.HasIndex(cp => new { cp.TenantId, cp.PedidoCompraItemId });
                entity.HasIndex(cp => cp.SyncId).IsUnique();
            });

            modelBuilder.Entity<ScTipoPedido>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Descricao).HasMaxLength(120);
                entity.HasIndex(t => new { t.TenantId, t.Descricao });
                entity.HasIndex(t => t.SyncId).IsUnique();
            });

            modelBuilder.Entity<ScPedidoCompra>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.LocalEntrega).HasMaxLength(500);
                entity.Property(p => p.LocalCobranca).HasMaxLength(500);
                entity.Property(p => p.Contato).HasMaxLength(120);
                entity.Property(p => p.FormaPagamento).HasMaxLength(120);
                entity.HasOne(p => p.TipoPedido)
                      .WithMany()
                      .HasForeignKey(p => p.TipoPedidoId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(p => p.Itens)
                      .WithOne(i => i.PedidoCompra)
                      .HasForeignKey(i => i.PedidoCompraId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(p => new { p.TenantId, p.FornecedorId });
                entity.HasIndex(p => new { p.TenantId, p.CotacaoId });
                entity.HasIndex(p => p.SyncId).IsUnique();
            });

            modelBuilder.Entity<ScPedidoCompraItem>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Quantidade).HasPrecision(18, 4);
                entity.Property(i => i.ValorUnitario).HasPrecision(18, 4);
                entity.Property(i => i.ValorDesconto).HasPrecision(18, 2);
                entity.Property(i => i.ValorFrete).HasPrecision(18, 2);
                entity.Property(i => i.ValorSeguro).HasPrecision(18, 2);
                entity.Property(i => i.ValorOutrasDespesas).HasPrecision(18, 2);
                entity.Property(i => i.ValorIpi).HasPrecision(18, 2);
                entity.Property(i => i.ValorIcms).HasPrecision(18, 2);
                entity.Property(i => i.ValorTotal).HasPrecision(18, 2);
                entity.HasOne(i => i.Produto)
                      .WithMany()
                      .HasForeignKey(i => i.ProdutoId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(i => new { i.TenantId, i.PedidoCompraId });
                entity.HasIndex(i => i.SyncId).IsUnique();
            });

            // ============ LOGÍSTICA DE ENTRADA (EST-LDE) ============

            modelBuilder.Entity<LdeEntrada>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MotivoCancelamentoEstorno).HasMaxLength(1000);
                entity.HasIndex(e => new { e.TenantId, e.CompraId });
                entity.HasIndex(e => new { e.TenantId, e.Situacao });
                entity.HasIndex(e => e.SyncId).IsUnique();
            });

            modelBuilder.Entity<LdeLocalEntregaCompra>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.Property(l => l.Nome).HasMaxLength(120);
                entity.Property(l => l.Fone).HasMaxLength(14);
                entity.Property(l => l.Email).HasMaxLength(60);
                entity.Property(l => l.InscricaoEstadual).HasMaxLength(20);
                entity.Property(l => l.Documento).HasMaxLength(20);
                entity.Property(l => l.Uf).HasMaxLength(2);
                entity.Property(l => l.Logradouro).HasMaxLength(60);
                entity.Property(l => l.Numero).HasMaxLength(60);
                entity.Property(l => l.Complemento).HasMaxLength(60);
                entity.Property(l => l.Bairro).HasMaxLength(60);
                entity.Property(l => l.MunicipioNome).HasMaxLength(60);
                entity.Property(l => l.Cep).HasMaxLength(8);
                entity.Property(l => l.PaisNome).HasMaxLength(60);
                entity.HasIndex(l => new { l.TenantId, l.CompraId });
                entity.HasIndex(l => l.SyncId).IsUnique();
            });

            modelBuilder.Entity<LdeDocumentoEntrada>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.ChaveAcesso).HasMaxLength(44);
                entity.Property(d => d.Numero).HasMaxLength(20);
                entity.Property(d => d.Serie).HasMaxLength(10);
                entity.Property(d => d.NaturezaOperacao).HasMaxLength(120);
                entity.Property(d => d.ValorTotal).HasPrecision(18, 2);
                entity.Property(d => d.Situacao).HasMaxLength(60);
                entity.HasMany(d => d.Itens)
                      .WithOne(i => i.Documento)
                      .HasForeignKey(i => i.DocumentoEntradaId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(d => d.Duplicatas)
                      .WithOne(du => du.Documento)
                      .HasForeignKey(du => du.DocumentoEntradaId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(d => new { d.TenantId, d.ChaveAcesso });
                entity.HasIndex(d => d.SyncId).IsUnique();
            });

            modelBuilder.Entity<LdeDocumentoEntradaItem>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.QuantidadeDocumento).HasPrecision(18, 4);
                entity.Property(i => i.ValorItem).HasPrecision(18, 2);
                entity.Property(i => i.DadosTributariosItem).HasMaxLength(5000);
                entity.HasIndex(i => new { i.TenantId, i.DocumentoEntradaId });
                entity.HasIndex(i => i.SyncId).IsUnique();
            });

            modelBuilder.Entity<LdeDocumentoEntradaDuplicata>(entity =>
            {
                entity.HasKey(du => du.Id);
                entity.Property(du => du.Numero).HasMaxLength(60);
                entity.Property(du => du.Valor).HasPrecision(18, 2);
                entity.HasIndex(du => new { du.TenantId, du.DocumentoEntradaId });
                entity.HasIndex(du => du.SyncId).IsUnique();
            });

            modelBuilder.Entity<LdeDocumentoEntradaTransporte>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.ReferenciaTransporte).HasMaxLength(120);
                entity.HasIndex(t => new { t.TenantId, t.DocumentoEntradaId });
                entity.HasIndex(t => t.SyncId).IsUnique();
            });

            modelBuilder.Entity<LdeDocumentoEntradaFatura>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.Property(f => f.Numero).HasMaxLength(60);
                entity.Property(f => f.ValorOriginal).HasPrecision(18, 2);
                entity.Property(f => f.ValorDesconto).HasPrecision(18, 2);
                entity.Property(f => f.ValorLiquido).HasPrecision(18, 2);
                entity.HasIndex(f => new { f.TenantId, f.DocumentoEntradaId });
                entity.HasIndex(f => f.SyncId).IsUnique();
            });

            modelBuilder.Entity<LdeHistorico>(entity =>
            {
                entity.HasKey(h => h.Id);
                entity.Property(h => h.Evento).HasMaxLength(120);
                entity.Property(h => h.Motivo).HasMaxLength(1000);
                entity.Property(h => h.UsuarioId).HasMaxLength(120);
                entity.HasIndex(h => new { h.TenantId, h.EntradaId });
                entity.HasIndex(h => h.SyncId).IsUnique();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
