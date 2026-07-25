using Epros.Infrastructure.Data;
using Epros.Modules.Financeiro.Domain.Entities;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Financeiro.Infrastructure.Data
{
    /// <summary>
    /// DbContext do módulo Financeiro.
    /// Schema: financas.* — isolado dos demais módulos conforme arquitetura hexagonal.
    /// QueryFilter automático de tenant_id e soft-delete via ContextBase por reflection.
    /// </summary>
    public class ContextFinanceiro : ContextBase
    {
        public DbSet<PlanoDeContasFinanceiro> PlanosDeContas => Set<PlanoDeContasFinanceiro>();
        public DbSet<PlanoDeContasFinanceiroItem> PlanoDeContasItens => Set<PlanoDeContasFinanceiroItem>();
        public DbSet<PlanoDeContasFinanceiroEmpresa> PlanoDeContasEmpresas => Set<PlanoDeContasFinanceiroEmpresa>();
        public DbSet<ConfiguracaoCodigoNaturezaFinanceira> NaturezasFinanceiras => Set<ConfiguracaoCodigoNaturezaFinanceira>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
        public DbSet<PessoaLookup> PessoasLookup => Set<PessoaLookup>();
        public DbSet<PessoaJuridicaLookup> PessoasJuridicasLookup => Set<PessoaJuridicaLookup>();
        public DbSet<PessoaFisicaLookup> PessoasFisicasLookup => Set<PessoaFisicaLookup>();
        public DbSet<Banco> Bancos => Set<Banco>();
        public DbSet<ContaBancaria> ContasBancarias => Set<ContaBancaria>();
        public DbSet<CartaoDeCredito> CartoesDeCredito => Set<CartaoDeCredito>();
        public DbSet<CartaoDeCreditoFatura> CartoesDeCreditoFaturas => Set<CartaoDeCreditoFatura>();

        // ----- Agregados financeiros fiéis do legado -----
        public DbSet<ContasAPagar> ContasAPagarAgregado => Set<ContasAPagar>();
        public DbSet<ContasAPagarItem> ContasAPagarItens => Set<ContasAPagarItem>();
        public DbSet<ContasAReceber> ContasAReceberAgregado => Set<ContasAReceber>();
        public DbSet<ContasAReceberItem> ContasAReceberItens => Set<ContasAReceberItem>();
        public DbSet<FatoGeradorFinanceiro> FatosGeradoresFinanceiros => Set<FatoGeradorFinanceiro>();
        public DbSet<ImportacacaoArquivoOfx> ImportacoesArquivoOfx => Set<ImportacacaoArquivoOfx>();
        public DbSet<ImportacacaoArquivoOfxTransacao> ImportacoesArquivoOfxTransacoes => Set<ImportacacaoArquivoOfxTransacao>();

        // ----- FIN-CGL: Contabilidade Geral (contabilidade plena — evolução) -----
        public DbSet<ContaContabil> ContasContabeis => Set<ContaContabil>();
        public DbSet<PeriodoContabil> PeriodosContabeis => Set<PeriodoContabil>();
        public DbSet<LancamentoContabil> LancamentosContabeis => Set<LancamentoContabil>();
        public DbSet<LancamentoContabilLinha> LancamentoContabilLinhas => Set<LancamentoContabilLinha>();
        public DbSet<SaldoAbertura> SaldosAbertura => Set<SaldoAbertura>();

        // ----- FIN-SF: Serviços Financeiros (cobrança/boleto/remessa) -----
        public DbSet<ConfiguracaoCedente> ConfiguracoesCedente => Set<ConfiguracaoCedente>();
        public DbSet<ContaEmissora> ContasEmissoras => Set<ContaEmissora>();
        public DbSet<GrupoRecorrencia> GruposRecorrencia => Set<GrupoRecorrencia>();
        public DbSet<Sacado> Sacados => Set<Sacado>();
        public DbSet<FaturaCobranca> FaturasCobranca => Set<FaturaCobranca>();
        public DbSet<Boleto> Boletos => Set<Boleto>();
        public DbSet<Remessa> Remessas => Set<Remessa>();
        public DbSet<RemessaBoleto> RemessaBoletos => Set<RemessaBoleto>();
        public DbSet<CobrancaEmail> CobrancasEmail => Set<CobrancaEmail>();

        // ----- FIN-CAM: Câmbio e Risco de Mercado -----
        public DbSet<Moeda> Moedas => Set<Moeda>();
        public DbSet<TaxaCambio> TaxasCambio => Set<TaxaCambio>();
        public DbSet<ExposicaoCambial> ExposicoesCambiais => Set<ExposicaoCambial>();
        public DbSet<ReavaliacaoTitulo> ReavaliacoesTitulo => Set<ReavaliacaoTitulo>();
        public DbSet<ReavaliacaoItem> ReavaliacoesItem => Set<ReavaliacaoItem>();

        // ----- FIN-AFX: Ativos Fixos -----
        public DbSet<AtivoFixo> AtivosFixos => Set<AtivoFixo>();
        public DbSet<GrupoBem> GruposBem => Set<GrupoBem>();
        public DbSet<DepreciacaoMensal> DepreciacoesMensais => Set<DepreciacaoMensal>();
        public DbSet<MovimentacaoAtivo> MovimentacoesAtivo => Set<MovimentacaoAtivo>();

        // ----- FIN-CMG: Contabilidade Gerencial -----
        public DbSet<CentroCusto> CentrosCusto => Set<CentroCusto>();
        public DbSet<AlocacaoCentroCusto> AlocacoesCentroCusto => Set<AlocacaoCentroCusto>();
        public DbSet<DimensaoAnalitica> DimensoesAnaliticas => Set<DimensaoAnalitica>();
        public DbSet<AlocacaoDimensao> AlocacoesDimensao => Set<AlocacaoDimensao>();

        // ----- FIN-GCF: Gestão de Contratos Financeiros -----
        public DbSet<PlanoContrato> PlanosContrato => Set<PlanoContrato>();
        public DbSet<ContratoFinanceiro> ContratosFinanceiros => Set<ContratoFinanceiro>();
        public DbSet<FaturaRecorrente> FaturasRecorrentes => Set<FaturaRecorrente>();
        public DbSet<ReajusteContrato> ReajustesContrato => Set<ReajusteContrato>();

        // ----- FIN-SBF: Subsídios e Fundos -----
        public DbSet<ProgramaSubsidio> ProgramasSubsidio => Set<ProgramaSubsidio>();
        public DbSet<UtilizacaoSubsidio> UtilizacoesSubsidio => Set<UtilizacaoSubsidio>();

        // ----- FIN-CON: Consolidação e Relatórios -----
        public DbSet<GrupoConsolidacao> GruposConsolidacao => Set<GrupoConsolidacao>();
        public DbSet<GrupoEmpresa> GruposEmpresa => Set<GrupoEmpresa>();
        public DbSet<Demonstrativo> Demonstrativos => Set<Demonstrativo>();
        public DbSet<DemonstrativoLinha> DemonstrativoLinhas => Set<DemonstrativoLinha>();
        public DbSet<BalanceteConsolidado> BalancetesConsolidados => Set<BalanceteConsolidado>();
        public DbSet<BalanceteLinha> BalanceteLinhas => Set<BalanceteLinha>();
        public DbSet<EliminacaoIntercompany> EliminacoesIntercompany => Set<EliminacaoIntercompany>();

        // ----- FIN-PO: Planejamento e Orçamento -----
        public DbSet<VersaoOrcamentaria> VersoesOrcamentarias => Set<VersaoOrcamentaria>();
        public DbSet<LinhaOrcamento> LinhasOrcamento => Set<LinhaOrcamento>();
        public DbSet<PeriodoOrcamentario> PeriodosOrcamentarios => Set<PeriodoOrcamentario>();
        public DbSet<Budget> Budgets => Set<Budget>();
        public DbSet<BudgetAlocacao> BudgetAlocacoes => Set<BudgetAlocacao>();
        public DbSet<MetaCategoria> MetaCategorias => Set<MetaCategoria>();
        public DbSet<Meta> Metas => Set<Meta>();
        public DbSet<MetaMilestone> MetaMilestones => Set<MetaMilestone>();
        public DbSet<MetaContribuicao> MetaContribuicoes => Set<MetaContribuicao>();
        public DbSet<MetaTracking> MetaTrackings => Set<MetaTracking>();

        // ----- FIN-TS: Tesouraria e Gestão de Liquidez -----
        public DbSet<ContaFinanceira> ContasFinanceiras => Set<ContaFinanceira>();
        public DbSet<TransacaoContaFinanceira> TransacoesContaFinanceira => Set<TransacaoContaFinanceira>();
        public DbSet<MovimentoFinanceiro> MovimentosFinanceiros => Set<MovimentoFinanceiro>();
        public DbSet<Cheque> Cheques => Set<Cheque>();
        public DbSet<CaixaOperacional> CaixasOperacionais => Set<CaixaOperacional>();

        public ContextFinanceiro(
            DbContextOptions<ContextFinanceiro> options,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser) : base(options, tenantProvider, currentUser)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Schema do macrodomínio financeiro
            modelBuilder.HasDefaultSchema("financas");

            // ----- PlanoDeContasFinanceiro -----
            modelBuilder.Entity<PlanoDeContasFinanceiro>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Descricao).HasMaxLength(50);
                entity.Property(p => p.Mascara).HasMaxLength(100);

                entity.HasMany(p => p.Itens)
                      .WithOne(i => i.PlanoDeContasFinanceiro)
                      .HasForeignKey(i => i.PlanoDeContasFinanceiroId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.Empresas)
                      .WithOne(e => e.PlanoDeContasFinanceiro)
                      .HasForeignKey(e => e.PlanoDeContasFinanceiroId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.ConfiguracaoCodigoNaturezaFinanceiraRecebimento)
                      .WithMany()
                      .HasForeignKey(p => p.ConfiguracaoCodigoNaturezaFinanceiraRecebimentoId)
                      .HasConstraintName("fk_plano_natureza_recebimento")
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(p => p.ConfiguracaoCodigoNaturezaFinanceiraPagamento)
                      .WithMany()
                      .HasForeignKey(p => p.ConfiguracaoCodigoNaturezaFinanceiraPagamentoId)
                      .HasConstraintName("fk_plano_natureza_pagamento")
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(p => new { p.TenantId, p.Descricao });
                entity.HasIndex(p => p.ConfiguracaoCodigoNaturezaFinanceiraRecebimentoId).HasDatabaseName("ix_plano_nat_recebimento");
                entity.HasIndex(p => p.ConfiguracaoCodigoNaturezaFinanceiraPagamentoId).HasDatabaseName("ix_plano_nat_pagamento");
            });

            modelBuilder.Entity<PlanoDeContasFinanceiroItem>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Codigo).HasMaxLength(50);
                entity.Property(i => i.Descricao).HasMaxLength(100);
                entity.HasIndex(i => new { i.TenantId, i.Codigo });
            });

            // ----- PlanoDeContasFinanceiroEmpresa (junção N:N legado com Empresa) -----
            modelBuilder.Entity<PlanoDeContasFinanceiroEmpresa>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.PlanoDeContasFinanceiroId, e.EmpresaId })
                      .HasDatabaseName("ix_plano_empresa_tenant_plano_empresa");
                entity.HasIndex(e => new { e.TenantId, e.EmpresaId }).HasDatabaseName("ix_plano_empresa_tenant_empresa");
            });

            // ----- ConfiguracaoCodigoNaturezaFinanceira -----
            modelBuilder.Entity<ConfiguracaoCodigoNaturezaFinanceira>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Descricao).HasMaxLength(150);
                entity.HasIndex(c => new { c.TenantId, c.Descricao });
                entity.HasIndex(c => new { c.TenantId, c.EmpresaId }).HasDatabaseName("ix_natureza_tenant_empresa");

                entity.HasOne(c => c.ItemPlanoDeContasFinanceiroDinheiro).WithMany().HasForeignKey(c => c.ItemPlanoDeContasFinanceiroDinheiroId).HasConstraintName("fk_natureza_dinheiro").OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(c => c.ItemPlanoDeContasFinanceiroCartaoCheque).WithMany().HasForeignKey(c => c.ItemPlanoDeContasFinanceiroCartaoChequeId).HasConstraintName("fk_natureza_cheque").OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(c => c.ItemPlanoDeContasFinanceiroCartaoCredito).WithMany().HasForeignKey(c => c.ItemPlanoDeContasFinanceiroCartaoCreditoId).HasConstraintName("fk_natureza_credito").OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(c => c.ItemPlanoDeContasFinanceiroCartaoDebito).WithMany().HasForeignKey(c => c.ItemPlanoDeContasFinanceiroCartaoDebitoId).HasConstraintName("fk_natureza_debito").OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(c => c.ItemPlanoDeContasFinanceiroCartaoDaLoja).WithMany().HasForeignKey(c => c.ItemPlanoDeContasFinanceiroCartaoDaLojaId).HasConstraintName("fk_natureza_cartao_loja").OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(c => c.ItemPlanoDeContasFinanceiroValeAlimentacao).WithMany().HasForeignKey(c => c.ItemPlanoDeContasFinanceiroValeAlimentacaoId).HasConstraintName("fk_natureza_vale_alimentacao").OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(c => c.ItemPlanoDeContasFinanceiroValeRefeicao).WithMany().HasForeignKey(c => c.ItemPlanoDeContasFinanceiroValeRefeicaoId).HasConstraintName("fk_natureza_vale_refeicao").OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(c => c.ItemPlanoDeContasFinanceiroValePresente).WithMany().HasForeignKey(c => c.ItemPlanoDeContasFinanceiroValePresenteId).HasConstraintName("fk_natureza_vale_presente").OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(c => c.ItemPlanoDeContasFinanceiroValeCombustivel).WithMany().HasForeignKey(c => c.ItemPlanoDeContasFinanceiroValeCombustivelId).HasConstraintName("fk_natureza_vale_combustivel").OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(c => c.ItemPlanoDeContasFinanceiroDuplicataMercantil).WithMany().HasForeignKey(c => c.ItemPlanoDeContasFinanceiroDuplicataMercantilId).HasConstraintName("fk_natureza_duplicata").OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(c => c.ItemPlanoDeContasFinanceiroBoletoBancario).WithMany().HasForeignKey(c => c.ItemPlanoDeContasFinanceiroBoletoBancarioId).HasConstraintName("fk_natureza_boleto").OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(c => c.ItemPlanoDeContasFinanceiroDepositoBancario).WithMany().HasForeignKey(c => c.ItemPlanoDeContasFinanceiroDepositoBancarioId).HasConstraintName("fk_natureza_deposito").OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(c => c.ItemPlanoDeContasFinanceiroPagamentoInstantaneoPixDinamico).WithMany().HasForeignKey(c => c.ItemPlanoDeContasFinanceiroPagamentoInstantaneoPixDinamicoId).HasConstraintName("fk_natureza_pix_dinamico").OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(c => c.ItemPlanoDeContasFinanceiroTransferenciaBancaria).WithMany().HasForeignKey(c => c.ItemPlanoDeContasFinanceiroTransferenciaBancariaId).HasConstraintName("fk_natureza_transferencia").OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(c => c.ItemPlanoDeContasFinanceiroProgramaDeFidelidade).WithMany().HasForeignKey(c => c.ItemPlanoDeContasFinanceiroProgramaDeFidelidadeId).HasConstraintName("fk_natureza_fidelidade").OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(c => c.ItemPlanoDeContasFinanceiroPagamentoInstantaneoPixEstatico).WithMany().HasForeignKey(c => c.ItemPlanoDeContasFinanceiroPagamentoInstantaneoPixEstaticoId).HasConstraintName("fk_natureza_pix_estatico").OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(c => c.ItemPlanoDeContasFinanceiroCreditoEmLoja).WithMany().HasForeignKey(c => c.ItemPlanoDeContasFinanceiroCreditoEmLojaId).HasConstraintName("fk_natureza_credito_loja").OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(c => c.ItemPlanoDeContasFinanceiroPagamentoEletronicoNaoInformado).WithMany().HasForeignKey(c => c.ItemPlanoDeContasFinanceiroPagamentoEletronicoNaoInformadoId).HasConstraintName("fk_natureza_pagamento_eletronico").OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(c => c.ItemPlanoDeContasFinanceiroOutros).WithMany().HasForeignKey(c => c.ItemPlanoDeContasFinanceiroOutrosId).HasConstraintName("fk_natureza_outros").OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(c => c.ItemPlanoDeContasFinanceiroDesconto).WithMany().HasForeignKey(c => c.ItemPlanoDeContasFinanceiroDescontoId).HasConstraintName("fk_natureza_desconto").OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(c => c.ItemPlanoDeContasFinanceiroAcrescimo).WithMany().HasForeignKey(c => c.ItemPlanoDeContasFinanceiroAcrescimoId).HasConstraintName("fk_natureza_acrescimo").OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(c => c.ItemPlanoDeContasFinanceiroJuros).WithMany().HasForeignKey(c => c.ItemPlanoDeContasFinanceiroJurosId).HasConstraintName("fk_natureza_juros").OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(c => c.ItemPlanoDeContasFinanceiroMulta).WithMany().HasForeignKey(c => c.ItemPlanoDeContasFinanceiroMultaId).HasConstraintName("fk_natureza_multa").OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(c => c.ItemPlanoDeContasFinanceiroTroco).WithMany().HasForeignKey(c => c.ItemPlanoDeContasFinanceiroTrocoId).HasConstraintName("fk_natureza_troco").OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(c => c.ItemPlanoDeContasFinanceiroDinheiroId).HasDatabaseName("ix_nat_dinheiro");
                entity.HasIndex(c => c.ItemPlanoDeContasFinanceiroCartaoChequeId).HasDatabaseName("ix_nat_cheque");
                entity.HasIndex(c => c.ItemPlanoDeContasFinanceiroCartaoCreditoId).HasDatabaseName("ix_nat_credito");
                entity.HasIndex(c => c.ItemPlanoDeContasFinanceiroCartaoDebitoId).HasDatabaseName("ix_nat_debito");
                entity.HasIndex(c => c.ItemPlanoDeContasFinanceiroCartaoDaLojaId).HasDatabaseName("ix_nat_cartao_loja");
                entity.HasIndex(c => c.ItemPlanoDeContasFinanceiroValeAlimentacaoId).HasDatabaseName("ix_nat_vale_alimentacao");
                entity.HasIndex(c => c.ItemPlanoDeContasFinanceiroValeRefeicaoId).HasDatabaseName("ix_nat_vale_refeicao");
                entity.HasIndex(c => c.ItemPlanoDeContasFinanceiroValePresenteId).HasDatabaseName("ix_nat_vale_presente");
                entity.HasIndex(c => c.ItemPlanoDeContasFinanceiroValeCombustivelId).HasDatabaseName("ix_nat_vale_combustivel");
                entity.HasIndex(c => c.ItemPlanoDeContasFinanceiroDuplicataMercantilId).HasDatabaseName("ix_nat_duplicata");
                entity.HasIndex(c => c.ItemPlanoDeContasFinanceiroBoletoBancarioId).HasDatabaseName("ix_nat_boleto");
                entity.HasIndex(c => c.ItemPlanoDeContasFinanceiroDepositoBancarioId).HasDatabaseName("ix_nat_deposito");
                entity.HasIndex(c => c.ItemPlanoDeContasFinanceiroPagamentoInstantaneoPixDinamicoId).HasDatabaseName("ix_nat_pix_dinamico");
                entity.HasIndex(c => c.ItemPlanoDeContasFinanceiroTransferenciaBancariaId).HasDatabaseName("ix_nat_transferencia");
                entity.HasIndex(c => c.ItemPlanoDeContasFinanceiroProgramaDeFidelidadeId).HasDatabaseName("ix_nat_fidelidade");
                entity.HasIndex(c => c.ItemPlanoDeContasFinanceiroPagamentoInstantaneoPixEstaticoId).HasDatabaseName("ix_nat_pix_estatico");
                entity.HasIndex(c => c.ItemPlanoDeContasFinanceiroCreditoEmLojaId).HasDatabaseName("ix_nat_credito_loja");
                entity.HasIndex(c => c.ItemPlanoDeContasFinanceiroPagamentoEletronicoNaoInformadoId).HasDatabaseName("ix_nat_pag_eletronico");
                entity.HasIndex(c => c.ItemPlanoDeContasFinanceiroOutrosId).HasDatabaseName("ix_nat_outros");
                entity.HasIndex(c => c.ItemPlanoDeContasFinanceiroDescontoId).HasDatabaseName("ix_nat_desconto");
                entity.HasIndex(c => c.ItemPlanoDeContasFinanceiroAcrescimoId).HasDatabaseName("ix_nat_acrescimo");
                entity.HasIndex(c => c.ItemPlanoDeContasFinanceiroJurosId).HasDatabaseName("ix_nat_juros");
                entity.HasIndex(c => c.ItemPlanoDeContasFinanceiroMultaId).HasDatabaseName("ix_nat_multa");
                entity.HasIndex(c => c.ItemPlanoDeContasFinanceiroTrocoId).HasDatabaseName("ix_nat_troco");
            });

            // ----- OutboxMessage (leitura cross-module do Estoque) -----
            // Q6: este DbSet NÃO é o outbox do Financeiro — é um CONSUMIDOR do outbox do módulo Estoque
            // (estoque.outbox_messages), de onde o OutboxProcessorJob lê os eventos CompraLancada/
            // CompraCancelada para gerar Contas a Pagar. A tabela é de propriedade do módulo Estoque
            // (que a cria em sua própria migração). Portanto, marca-se ExcludeFromMigrations para que a
            // migração do Financeiro NÃO tente criar/alterar/administrar essa tabela de outro schema —
            // o mesmo padrão adotado nos lookups cross-schema (pessoas/pessoas_juridicas/pessoas_fisicas).
            modelBuilder.Entity<OutboxMessage>(entity =>
            {
                entity.ToTable("outbox_messages", "estoque", t => t.ExcludeFromMigrations());
                entity.HasKey(o => o.Id);
            });

            // ----- PessoaLookup (Plataforma) -----
            // Lookups de leitura cross-module: as tabelas pessoas/pessoas_juridicas/pessoas_fisicas
            // pertencem ao módulo GestaoClientes (schema plataforma). ExcludeFromMigrations para o
            // Financeiro não tentar criar/alterar essas tabelas (ex.: coluna eh_cliente já criada lá).
            modelBuilder.Entity<PessoaLookup>(entity =>
            {
                entity.ToTable("pessoas", "plataforma", t => t.ExcludeFromMigrations());
                entity.HasKey(p => p.Id);
            });

            // ----- PessoaJuridicaLookup (Plataforma) -----
            modelBuilder.Entity<PessoaJuridicaLookup>(entity =>
            {
                entity.ToTable("pessoas_juridicas", "plataforma", t => t.ExcludeFromMigrations());
                entity.HasKey(pj => pj.PessoaId);
                entity.Property(pj => pj.Cnpj).HasColumnName("cnpj");
            });

            // ----- PessoaFisicaLookup (Plataforma) -----
            modelBuilder.Entity<PessoaFisicaLookup>(entity =>
            {
                entity.ToTable("pessoas_fisicas", "plataforma", t => t.ExcludeFromMigrations());
                entity.HasKey(pf => pf.PessoaId);
                entity.Property(pf => pf.Cpf).HasColumnName("cpf");
                entity.Property(pf => pf.Nome).HasColumnName("nome");
            });

            // ----- Banco -----
            modelBuilder.Entity<Banco>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.Codigo).HasMaxLength(3);
                entity.Property(b => b.Descricao).HasMaxLength(250);
                entity.HasIndex(b => new { b.TenantId, b.Codigo });
            });

            // ----- ContaBancaria -----
            modelBuilder.Entity<ContaBancaria>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Apelido).HasMaxLength(150);
                entity.Property(c => c.Titular).HasMaxLength(150);
                entity.Property(c => c.Agencia).HasMaxLength(20);
                entity.Property(c => c.Conta).HasMaxLength(20);
                entity.Property(c => c.Gerente).HasMaxLength(150);
                entity.Property(c => c.FoneGerente).HasMaxLength(150);
                entity.Property(c => c.Detalhe).HasMaxLength(1000);
                entity.Property(c => c.DigitoAgencia).HasMaxLength(2);
                
                entity.HasOne(c => c.Banco)
                      .WithMany()
                      .HasForeignKey(c => c.BancoId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(c => new { c.TenantId, c.Conta });
                entity.HasIndex(c => new { c.TenantId, c.EmpresaId }).HasDatabaseName("ix_conta_bancaria_tenant_empresa");
            });

            // ----- CartaoDeCredito -----
            modelBuilder.Entity<CartaoDeCredito>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Apelido).HasMaxLength(150);
                entity.Property(c => c.Titular).HasMaxLength(200);
                entity.Property(c => c.Observacao).HasMaxLength(200);

                entity.HasOne(c => c.ContaBancaria)
                      .WithMany()
                      .HasForeignKey(c => c.ContaBancariaId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(c => new { c.TenantId, c.Apelido });
            });

            // ----- CartaoDeCreditoFatura -----
            modelBuilder.Entity<CartaoDeCreditoFatura>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.Property(f => f.Valor).HasPrecision(18, 2);

                entity.HasOne(f => f.CartaoDeCredito)
                      .WithMany(c => c.CartaoDeCreditoFaturas)
                      .HasForeignKey(f => f.CartaoDeCreditoId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(f => new { f.TenantId, f.CartaoDeCreditoId });
            });

            // ----- FatoGeradorFinanceiro -----
            modelBuilder.Entity<FatoGeradorFinanceiro>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.Property(f => f.Origem).HasConversion<int>();
                entity.Property(f => f.Descricao).HasMaxLength(500);
                entity.HasIndex(f => new { f.TenantId, f.Origem }).HasDatabaseName("ix_fato_gerador_tenant_origem");
                entity.HasIndex(f => f.SyncId).IsUnique().HasDatabaseName("uq_fato_gerador_sync_id");
            });

            // ----- ContasAPagar (agregado fiel) -----
            modelBuilder.Entity<ContasAPagar>(entity =>
            {
                entity.ToTable("contas_a_pagar");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Situacao).HasConversion<int>();
                entity.Property(c => c.NomePessoa).HasMaxLength(250);
                entity.Property(c => c.Documento).HasMaxLength(30);
                entity.Property(c => c.Detalhamento).HasMaxLength(255);
                entity.Property(c => c.JustificativaCancelamento).HasMaxLength(255);
                entity.Property(c => c.ValorTitulo).HasPrecision(18, 2);
                entity.Property(c => c.ValorTotalDesconto).HasPrecision(18, 2);
                entity.Property(c => c.ValorTotalMulta).HasPrecision(18, 2);
                entity.Property(c => c.ValorTotalJuros).HasPrecision(18, 2);
                entity.Property(c => c.ValorTotalTroco).HasPrecision(18, 2);
                entity.Property(c => c.ValorTotalAcrescimo).HasPrecision(18, 2);
                entity.Property(c => c.ValorTotalPago).HasPrecision(18, 2);
                entity.Property(c => c.ValorTotalAPagarTitulo).HasPrecision(18, 2);
                entity.Property(c => c.ValorInicialDesconto).HasPrecision(18, 2);
                entity.Property(c => c.ValorInicialMulta).HasPrecision(18, 2);
                entity.Property(c => c.ValorInicialJuros).HasPrecision(18, 2);
                entity.Property(c => c.ValorInicialAcrescimo).HasPrecision(18, 2);
                entity.Property(c => c.ValorInicialAPagarTitulo).HasPrecision(18, 2);

                entity.HasOne(c => c.PlanoDeContasFinanceiroItem)
                      .WithMany()
                      .HasForeignKey(c => c.PlanoDeContasFinanceiroItemId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.FatoGeradorFinanceiro)
                      .WithMany(f => f.ContasAPagars)
                      .HasForeignKey(c => c.FatoGeradorFinanceiroId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(c => c.ContasAPagarItens)
                      .WithOne(i => i.ContasAPagar)
                      .HasForeignKey(i => i.ContasAPagarId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(c => new { c.TenantId, c.DataVencimento }).HasDatabaseName("ix_contas_a_pagar_tenant_vencimento");
                entity.HasIndex(c => new { c.TenantId, c.PessoaId }).HasDatabaseName("ix_contas_a_pagar_tenant_pessoa");
                entity.HasIndex(c => c.SyncId).IsUnique().HasDatabaseName("uq_contas_a_pagar_sync_id");
            });

            // ----- ContasAPagarItem -----
            modelBuilder.Entity<ContasAPagarItem>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.TipoPagamento).HasConversion<int>();
                entity.Property(i => i.ValorParcela).HasPrecision(18, 2);
                entity.Property(i => i.ValorPago).HasPrecision(18, 2);
                entity.Property(i => i.ValorDesconto).HasPrecision(18, 2);
                entity.Property(i => i.ValorMulta).HasPrecision(18, 2);
                entity.Property(i => i.ValorJuros).HasPrecision(18, 2);
                entity.Property(i => i.ValorTroco).HasPrecision(18, 2);
                entity.Property(i => i.ValorAcrescimo).HasPrecision(18, 2);
                entity.Property(i => i.ValorAPagar).HasPrecision(18, 2);

                entity.HasOne(i => i.PlanoDeContasFinanceiroItem)
                      .WithMany()
                      .HasForeignKey(i => i.PlanoDeContasFinanceiroItemId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.ContaBancaria)
                      .WithMany()
                      .HasForeignKey(i => i.ContaBancariaId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(i => new { i.TenantId, i.ContasAPagarId }).HasDatabaseName("ix_contas_a_pagar_item_tenant_conta");
                entity.HasIndex(i => i.SyncId).IsUnique().HasDatabaseName("uq_contas_a_pagar_item_sync_id");
            });

            // ----- ContasAReceber (agregado fiel) -----
            modelBuilder.Entity<ContasAReceber>(entity =>
            {
                entity.ToTable("contas_a_receber");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Situacao).HasConversion<int>();
                entity.Property(c => c.NomePessoa).HasMaxLength(250);
                entity.Property(c => c.Documento).HasMaxLength(30);
                entity.Property(c => c.Detalhamento).HasMaxLength(255);
                entity.Property(c => c.JustificativaCancelamento).HasMaxLength(255);
                entity.Property(c => c.ValorTitulo).HasPrecision(18, 2);
                entity.Property(c => c.ValorTotalDesconto).HasPrecision(18, 2);
                entity.Property(c => c.ValorTotalMulta).HasPrecision(18, 2);
                entity.Property(c => c.ValorTotalJuros).HasPrecision(18, 2);
                entity.Property(c => c.ValorTotalTroco).HasPrecision(18, 2);
                entity.Property(c => c.ValorTotalAcrescimo).HasPrecision(18, 2);
                entity.Property(c => c.ValorTotalRecebido).HasPrecision(18, 2);
                entity.Property(c => c.ValorTotalAReceberTitulo).HasPrecision(18, 2);
                entity.Property(c => c.ValorInicialDesconto).HasPrecision(18, 2);
                entity.Property(c => c.ValorInicialMulta).HasPrecision(18, 2);
                entity.Property(c => c.ValorInicialJuros).HasPrecision(18, 2);
                entity.Property(c => c.ValorInicialAcrescimo).HasPrecision(18, 2);
                entity.Property(c => c.ValorInicialAReceberTitulo).HasPrecision(18, 2);

                entity.HasOne(c => c.PlanoDeContasFinanceiroItem)
                      .WithMany()
                      .HasForeignKey(c => c.PlanoDeContasFinanceiroItemId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.FatoGeradorFinanceiro)
                      .WithMany(f => f.ContasARecebers)
                      .HasForeignKey(c => c.FatoGeradorFinanceiroId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(c => c.ContasAReceberItens)
                      .WithOne(i => i.ContasAReceber)
                      .HasForeignKey(i => i.ContasAReceberId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(c => new { c.TenantId, c.DataVencimento }).HasDatabaseName("ix_contas_a_receber_tenant_vencimento");
                entity.HasIndex(c => new { c.TenantId, c.PessoaId }).HasDatabaseName("ix_contas_a_receber_tenant_pessoa");
                entity.HasIndex(c => c.SyncId).IsUnique().HasDatabaseName("uq_contas_a_receber_sync_id");
            });

            // ----- ContasAReceberItem -----
            modelBuilder.Entity<ContasAReceberItem>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.TipoPagamento).HasConversion<int>();
                entity.Property(i => i.ValorParcela).HasPrecision(18, 2);
                entity.Property(i => i.ValorPago).HasPrecision(18, 2);
                entity.Property(i => i.ValorDesconto).HasPrecision(18, 2);
                entity.Property(i => i.ValorMulta).HasPrecision(18, 2);
                entity.Property(i => i.ValorJuros).HasPrecision(18, 2);
                entity.Property(i => i.ValorTroco).HasPrecision(18, 2);
                entity.Property(i => i.ValorAcrescimo).HasPrecision(18, 2);
                entity.Property(i => i.ValorAReceber).HasPrecision(18, 2);

                entity.HasOne(i => i.PlanoDeContasFinanceiroItem)
                      .WithMany()
                      .HasForeignKey(i => i.PlanoDeContasFinanceiroItemId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.ContaBancaria)
                      .WithMany()
                      .HasForeignKey(i => i.ContaBancariaId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(i => new { i.TenantId, i.ContasAReceberId }).HasDatabaseName("ix_contas_a_receber_item_tenant_conta");
                entity.HasIndex(i => i.SyncId).IsUnique().HasDatabaseName("uq_contas_a_receber_item_sync_id");
            });

            // ----- ImportacacaoArquivoOfx -----
            modelBuilder.Entity<ImportacacaoArquivoOfx>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.Property(o => o.CodigoBanco).HasMaxLength(10);
                entity.Property(o => o.NumeroConta).HasMaxLength(30);
                entity.Property(o => o.TipoConta).HasMaxLength(30);

                entity.HasMany(o => o.Transacoes)
                      .WithOne(t => t.ImportacacaoArquivoOfx)
                      .HasForeignKey(t => t.ImportacacaoArquivoOfxId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(o => new { o.TenantId, o.NumeroConta }).HasDatabaseName("ix_importacao_ofx_tenant_conta");
                entity.HasIndex(o => o.SyncId).IsUnique().HasDatabaseName("uq_importacao_ofx_sync_id");
            });

            // ----- ImportacacaoArquivoOfxTransacao -----
            modelBuilder.Entity<ImportacacaoArquivoOfxTransacao>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.IdentificadorTransacao).HasMaxLength(100);
                entity.Property(t => t.Tipo).HasMaxLength(50);
                entity.Property(t => t.Descricao).HasMaxLength(500);
                entity.Property(t => t.Valor).HasPrecision(18, 2);

                entity.HasOne(t => t.ContasAReceber)
                      .WithMany()
                      .HasForeignKey(t => t.ContasAReceberId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(t => t.ContasAPagar)
                      .WithMany()
                      .HasForeignKey(t => t.ContasAPagarId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(t => new { t.TenantId, t.Conciliado }).HasDatabaseName("ix_ofx_transacao_tenant_conciliado");
                entity.HasIndex(t => t.SyncId).IsUnique().HasDatabaseName("uq_ofx_transacao_sync_id");
            });

            // ===== FIN-CGL: Contabilidade Geral (contabilidade plena) =====
            modelBuilder.Entity<ContaContabil>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.NomeConta).HasMaxLength(100);
                entity.Property(c => c.NomeContaPai).HasMaxLength(100);
                entity.HasOne(c => c.ContaPai)
                      .WithMany()
                      .HasForeignKey(c => c.ContaPaiId)
                      .HasConstraintName("fk_conta_contabil_pai")
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(c => new { c.TenantId, c.CodigoConta }).HasDatabaseName("ix_conta_contabil_tenant_codigo");
                entity.HasIndex(c => c.ContaPaiId).HasDatabaseName("ix_conta_contabil_pai");
            });

            modelBuilder.Entity<PeriodoContabil>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.MotivoReabertura).HasMaxLength(500);
                entity.HasIndex(p => new { p.TenantId, p.AnoFiscal }).HasDatabaseName("ix_periodo_contabil_tenant_ano");
            });

            modelBuilder.Entity<LancamentoContabil>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.Property(l => l.NumeroLancamento).HasMaxLength(50);
                entity.Ignore(l => l.TotalDebitos);
                entity.Ignore(l => l.TotalCreditos);
                entity.Ignore(l => l.Balanceado);
                entity.HasMany(l => l.Linhas)
                      .WithOne(li => li.LancamentoContabil)
                      .HasForeignKey(li => li.LancamentoContabilId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(l => new { l.TenantId, l.NumeroLancamento }).HasDatabaseName("ix_lancamento_contabil_tenant_numero");
                entity.HasIndex(l => l.PeriodoContabilId).HasDatabaseName("ix_lancamento_contabil_periodo");
            });

            modelBuilder.Entity<LancamentoContabilLinha>(entity =>
            {
                entity.HasKey(li => li.Id);
                entity.Property(li => li.Debito).HasPrecision(18, 2);
                entity.Property(li => li.Credito).HasPrecision(18, 2);
                entity.HasIndex(li => li.ContaContabilId).HasDatabaseName("ix_lancamento_linha_conta");
            });

            modelBuilder.Entity<SaldoAbertura>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Valor).HasPrecision(18, 2);
                entity.HasOne(s => s.ContaContabil)
                      .WithMany()
                      .HasForeignKey(s => s.ContaContabilId)
                      .HasConstraintName("fk_saldo_abertura_conta")
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(s => new { s.TenantId, s.ContaContabilId }).HasDatabaseName("ix_saldo_abertura_tenant_conta");
            });

            // ===== FIN-SF: Serviços Financeiros (cobrança/boleto/remessa) =====
            modelBuilder.Entity<ConfiguracaoCedente>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Nome).HasMaxLength(200);
                entity.Property(c => c.Logo).HasMaxLength(200);
                entity.Property(c => c.MultaAtraso).HasPrecision(18, 2);
                entity.Property(c => c.Juro).HasPrecision(18, 2);
                entity.Property(c => c.Instrucao1).HasMaxLength(200);
                entity.Property(c => c.Instrucao2).HasMaxLength(200);
                entity.Property(c => c.Instrucao3).HasMaxLength(200);
                entity.Property(c => c.Instrucao4).HasMaxLength(200);
                entity.HasIndex(c => new { c.TenantId, c.EmpresaId }).HasDatabaseName("ix_cedente_tenant_empresa");
            });

            modelBuilder.Entity<ContaEmissora>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.NomeBanco).HasMaxLength(200);
                entity.HasIndex(c => new { c.TenantId, c.BancoId }).HasDatabaseName("ix_conta_emissora_tenant_banco");
                entity.HasIndex(c => new { c.TenantId, c.Ativa }).HasDatabaseName("ix_conta_emissora_tenant_ativa");
            });

            modelBuilder.Entity<GrupoRecorrencia>(entity =>
            {
                entity.HasKey(g => g.Id);
                entity.Property(g => g.Descricao).HasMaxLength(200);
                entity.Property(g => g.Valor).HasPrecision(18, 2);
                entity.HasIndex(g => new { g.TenantId, g.Descricao }).HasDatabaseName("ix_grupo_recorrencia_tenant_descricao");
            });

            modelBuilder.Entity<Sacado>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Nome).HasMaxLength(255);
                entity.Property(s => s.Email).HasMaxLength(255);
                entity.Property(s => s.Valor).HasPrecision(18, 2);
                entity.HasOne(s => s.GrupoRecorrencia)
                      .WithMany()
                      .HasForeignKey(s => s.GrupoRecorrenciaId)
                      .HasConstraintName("fk_sacado_grupo")
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasIndex(s => new { s.TenantId, s.Documento }).HasDatabaseName("ix_sacado_tenant_documento");
            });

            modelBuilder.Entity<FaturaCobranca>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.Property(f => f.Valor).HasPrecision(18, 2);
                entity.Property(f => f.ValorRecebido).HasPrecision(18, 2);
                entity.Property(f => f.NumeroDocumento).HasMaxLength(50);
                entity.Ignore(f => f.ElegivelRemessa);
                entity.Ignore(f => f.ElegivelBoleto);
                entity.HasOne(f => f.Sacado)
                      .WithMany()
                      .HasForeignKey(f => f.SacadoId)
                      .HasConstraintName("fk_fatura_sacado")
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(f => new { f.SacadoId, f.DataVencimento, f.Situacao }).HasDatabaseName("ix_fatura_sacado_venc_situacao");
                entity.HasIndex(f => f.NossoNumero).HasDatabaseName("ix_fatura_nosso_numero");
            });

            modelBuilder.Entity<Boleto>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.Valor).HasPrecision(18, 2);
                entity.Property(b => b.Multa).HasPrecision(18, 2);
                entity.Property(b => b.Juros).HasPrecision(18, 2);
                entity.Property(b => b.Instrucao1).HasMaxLength(200);
                entity.Property(b => b.Instrucao2).HasMaxLength(200);
                entity.Property(b => b.Instrucao3).HasMaxLength(200);
                entity.Property(b => b.Instrucao4).HasMaxLength(200);
                entity.HasOne(b => b.FaturaCobranca)
                      .WithMany()
                      .HasForeignKey(b => b.FaturaCobrancaId)
                      .HasConstraintName("fk_boleto_fatura")
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(b => b.ContaEmissora)
                      .WithMany()
                      .HasForeignKey(b => b.ContaEmissoraId)
                      .HasConstraintName("fk_boleto_conta_emissora")
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(b => new { b.NossoNumero, b.ContaEmissoraId }).HasDatabaseName("ix_boleto_nosso_numero_conta");
            });

            modelBuilder.Entity<Remessa>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.NomeArquivo).HasMaxLength(200);
                entity.Property(r => r.ValorTotal).HasPrecision(18, 2);
                entity.HasMany(r => r.Boletos)
                      .WithOne(rb => rb.Remessa)
                      .HasForeignKey(rb => rb.RemessaId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(r => new { r.TenantId, r.NomeArquivo }).IsUnique().HasDatabaseName("uq_remessa_tenant_arquivo");
            });

            modelBuilder.Entity<RemessaBoleto>(entity =>
            {
                entity.HasKey(rb => rb.Id);
                entity.HasIndex(rb => new { rb.RemessaId, rb.BoletoId }).IsUnique().HasDatabaseName("uq_remessa_boleto");
            });

            modelBuilder.Entity<CobrancaEmail>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Nome).HasMaxLength(255);
                entity.Property(c => c.Valor).HasPrecision(18, 2);
                entity.HasIndex(c => new { c.TenantId, c.Status }).HasDatabaseName("ix_cobranca_email_tenant_status");
            });

            // ===== FIN-CAM: Câmbio e Risco de Mercado =====
            modelBuilder.Entity<Moeda>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.CodigoIso).HasMaxLength(10);
                e.Property(x => x.Simbolo).HasMaxLength(10);
                e.Property(x => x.Nome).HasMaxLength(100);
                e.HasIndex(x => new { x.TenantId, x.CodigoIso }).HasDatabaseName("ix_moeda_tenant_codigo");
            });

            modelBuilder.Entity<TaxaCambio>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.TaxaCompra).HasPrecision(18, 6);
                e.Property(x => x.TaxaVenda).HasPrecision(18, 6);
                e.Property(x => x.Observacao).HasMaxLength(500);
                e.HasOne<Moeda>().WithMany().HasForeignKey(x => x.MoedaId).HasConstraintName("fk_taxa_cambio_moeda").OnDelete(DeleteBehavior.Restrict);
                e.HasIndex(x => new { x.MoedaId, x.DataTaxa }).HasDatabaseName("ix_taxa_cambio_moeda_data");
            });

            modelBuilder.Entity<ExposicaoCambial>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.ValorExposto).HasPrecision(18, 2);
                e.Property(x => x.ValorMoedaBase).HasPrecision(18, 2);
                e.Property(x => x.OrigemExposicao).HasMaxLength(100);
                e.Property(x => x.EntidadeOrigemTipo).HasMaxLength(100);
                e.HasIndex(x => new { x.TenantId, x.MoedaId, x.Status }).HasDatabaseName("ix_exposicao_cambial_moeda_status");
            });

            modelBuilder.Entity<ReavaliacaoTitulo>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.TotalValorOriginal).HasPrecision(18, 2);
                e.Property(x => x.TotalValorReavaliado).HasPrecision(18, 2);
                e.Property(x => x.TotalVariacao).HasPrecision(18, 2);
                e.Property(x => x.Observacao).HasMaxLength(500);
                e.HasMany(x => x.Itens).WithOne(i => i.Reavaliacao).HasForeignKey(i => i.ReavaliacaoId).OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => new { x.TenantId, x.DataReavaliacao }).HasDatabaseName("ix_reavaliacao_titulo_data");
            });

            modelBuilder.Entity<ReavaliacaoItem>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.TituloTipo).HasMaxLength(50);
                e.Property(x => x.ValorOriginalMoeda).HasPrecision(18, 2);
                e.Property(x => x.ValorReavaliadoBase).HasPrecision(18, 2);
                e.Property(x => x.ValorVariacao).HasPrecision(18, 2);
                e.HasIndex(x => x.ReavaliacaoId).HasDatabaseName("ix_reavaliacao_item_reavaliacao");
            });

            // ===== FIN-AFX: Ativos Fixos =====
            modelBuilder.Entity<AtivoFixo>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.NumeroNb).HasMaxLength(50);
                e.Property(x => x.Nome).HasMaxLength(150);
                e.Property(x => x.Descricao).HasMaxLength(255);
                e.Property(x => x.NumeroSerie).HasMaxLength(100);
                e.Property(x => x.Funcao).HasMaxLength(100);
                e.Property(x => x.NumeroNotaFiscal).HasMaxLength(50);
                e.Property(x => x.ChaveNfe).HasMaxLength(50);
                e.Property(x => x.MetodoDepreciacao).HasMaxLength(50);
                e.Property(x => x.ValorOriginal).HasPrecision(18, 2);
                e.Property(x => x.ValorCompra).HasPrecision(18, 2);
                e.Property(x => x.ValorAtualizado).HasPrecision(18, 2);
                e.Property(x => x.ValorBaixa).HasPrecision(18, 2);
                e.Property(x => x.TaxaAnual).HasPrecision(18, 4);
                e.Property(x => x.TaxaMensal).HasPrecision(18, 4);
                e.Property(x => x.TaxaAcelerada).HasPrecision(18, 4);
                e.Property(x => x.TaxaIncentivada).HasPrecision(18, 4);
                e.HasIndex(x => new { x.TenantId, x.NumeroNb }).HasDatabaseName("ix_ativo_fixo_tenant_nb");
                e.HasIndex(x => new { x.TenantId, x.Status }).HasDatabaseName("ix_ativo_fixo_tenant_status");
            });

            modelBuilder.Entity<GrupoBem>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Codigo).HasMaxLength(50);
                e.Property(x => x.Nome).HasMaxLength(150);
                e.HasIndex(x => new { x.TenantId, x.Codigo }).HasDatabaseName("ix_grupo_bem_tenant_codigo");
            });

            modelBuilder.Entity<DepreciacaoMensal>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Competencia).HasMaxLength(7);
                e.Property(x => x.Valor).HasPrecision(18, 2);
                e.Property(x => x.TaxaAplicada).HasPrecision(18, 4);
                e.Property(x => x.MetodoDepreciacao).HasMaxLength(50);
                e.HasIndex(x => new { x.AtivoId, x.Competencia }).HasDatabaseName("ix_depreciacao_ativo_competencia");
            });

            modelBuilder.Entity<MovimentacaoAtivo>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Valor).HasPrecision(18, 2);
                e.Property(x => x.Observacao).HasMaxLength(500);
                e.HasIndex(x => new { x.AtivoId, x.DataMovimentacao }).HasDatabaseName("ix_movimentacao_ativo_data");
            });

            // ===== FIN-CMG: Contabilidade Gerencial =====
            modelBuilder.Entity<CentroCusto>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Codigo).HasMaxLength(50);
                e.Property(x => x.Descricao).HasMaxLength(150);
                e.HasOne(x => x.Pai).WithMany().HasForeignKey(x => x.PaiId).HasConstraintName("fk_centro_custo_pai").OnDelete(DeleteBehavior.Restrict);
                e.HasIndex(x => new { x.TenantId, x.Codigo }).HasDatabaseName("ix_centro_custo_tenant_codigo");
            });

            modelBuilder.Entity<AlocacaoCentroCusto>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Percentual).HasPrecision(18, 4);
                e.Property(x => x.ValorRateado).HasPrecision(18, 2);
                e.HasOne<CentroCusto>().WithMany().HasForeignKey(x => x.CentroCustoId).HasConstraintName("fk_alocacao_centro_custo").OnDelete(DeleteBehavior.Restrict);
                e.HasIndex(x => x.TituloId).HasDatabaseName("ix_alocacao_cc_titulo");
            });

            modelBuilder.Entity<DimensaoAnalitica>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Tipo).HasMaxLength(100);
                e.Property(x => x.Valor).HasMaxLength(255);
                e.HasIndex(x => new { x.TenantId, x.Tipo }).HasDatabaseName("ix_dimensao_analitica_tenant_tipo");
            });

            modelBuilder.Entity<AlocacaoDimensao>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasOne<AlocacaoCentroCusto>().WithMany().HasForeignKey(x => x.AlocacaoCentroCustoId).HasConstraintName("fk_alocacao_dimensao_alocacao").OnDelete(DeleteBehavior.Cascade);
                e.HasOne<DimensaoAnalitica>().WithMany().HasForeignKey(x => x.DimensaoAnaliticaId).HasConstraintName("fk_alocacao_dimensao_dimensao").OnDelete(DeleteBehavior.Restrict);
                e.HasIndex(x => new { x.AlocacaoCentroCustoId, x.DimensaoAnaliticaId }).HasDatabaseName("ix_alocacao_dimensao_vinculo");
            });

            // ===== FIN-GCF: Gestão de Contratos Financeiros =====
            modelBuilder.Entity<PlanoContrato>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Descricao).HasMaxLength(255);
                e.Property(x => x.Valor).HasPrecision(18, 2);
                e.HasIndex(x => new { x.TenantId, x.Status }).HasDatabaseName("ix_plano_contrato_tenant_status");
            });

            modelBuilder.Entity<ContratoFinanceiro>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.MotivoCancelamento).HasMaxLength(500);
                e.HasOne(x => x.Plano).WithMany().HasForeignKey(x => x.PlanoId).HasConstraintName("fk_contrato_financeiro_plano").OnDelete(DeleteBehavior.Restrict);
                e.HasIndex(x => new { x.TenantId, x.PessoaId }).HasDatabaseName("ix_contrato_financeiro_pessoa");
                e.HasIndex(x => new { x.TenantId, x.Status }).HasDatabaseName("ix_contrato_financeiro_status");
            });

            modelBuilder.Entity<FaturaRecorrente>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Competencia).HasMaxLength(20);
                e.Property(x => x.Valor).HasPrecision(18, 2);
                e.HasOne<ContratoFinanceiro>().WithMany().HasForeignKey(x => x.ContratoId).HasConstraintName("fk_fatura_recorrente_contrato").OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => new { x.ContratoId, x.Competencia }).HasDatabaseName("ix_fatura_recorrente_contrato_comp");
            });

            modelBuilder.Entity<ReajusteContrato>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.ValorAnterior).HasPrecision(18, 2);
                e.Property(x => x.ValorNovo).HasPrecision(18, 2);
                e.Property(x => x.Motivo).HasMaxLength(500);
                e.HasOne<ContratoFinanceiro>().WithMany().HasForeignKey(x => x.ContratoId).HasConstraintName("fk_reajuste_contrato").OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => x.ContratoId).HasDatabaseName("ix_reajuste_contrato");
            });

            // ===== FIN-SBF: Subsídios e Fundos =====
            modelBuilder.Entity<ProgramaSubsidio>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Orgao).HasMaxLength(255);
                e.Property(x => x.ValorTotal).HasPrecision(18, 2);
                e.HasIndex(x => new { x.TenantId, x.Estado }).HasDatabaseName("ix_programa_subsidio_tenant_estado");
            });

            modelBuilder.Entity<UtilizacaoSubsidio>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.ValorElegivel).HasPrecision(18, 2);
                e.HasOne(x => x.ProgramaSubsidio).WithMany().HasForeignKey(x => x.ProgramaSubsidioId).HasConstraintName("fk_utilizacao_subsidio_programa").OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => x.ProgramaSubsidioId).HasDatabaseName("ix_utilizacao_subsidio_programa");
            });

            // ===== FIN-CON: Consolidação e Relatórios =====
            modelBuilder.Entity<GrupoConsolidacao>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Codigo).HasMaxLength(50);
                e.Property(x => x.Nome).HasMaxLength(150);
                e.Property(x => x.Descricao).HasMaxLength(500);
                e.HasMany(x => x.Empresas).WithOne(g => g.GrupoConsolidacao).HasForeignKey(g => g.GrupoConsolidacaoId).OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => new { x.TenantId, x.Codigo }).HasDatabaseName("ix_grupo_consolidacao_tenant_codigo");
            });

            modelBuilder.Entity<GrupoEmpresa>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => new { x.GrupoConsolidacaoId, x.EmpresaId }).HasDatabaseName("ix_grupo_empresa_vinculo");
            });

            modelBuilder.Entity<Demonstrativo>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Periodo).HasMaxLength(20);
                e.Property(x => x.TotalAgregado).HasPrecision(18, 2);
                e.HasMany(x => x.Linhas).WithOne(l => l.Demonstrativo).HasForeignKey(l => l.DemonstrativoId).OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => new { x.GrupoConsolidacaoId, x.Periodo }).HasDatabaseName("ix_demonstrativo_grupo_periodo");
            });

            modelBuilder.Entity<DemonstrativoLinha>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.CodigoLinha).HasMaxLength(50);
                e.Property(x => x.DescricaoLinha).HasMaxLength(255);
                e.Property(x => x.Valor).HasPrecision(18, 2);
                e.Property(x => x.TipoLinha).HasMaxLength(50);
                e.HasIndex(x => x.DemonstrativoId).HasDatabaseName("ix_demonstrativo_linha_demo");
            });

            modelBuilder.Entity<BalanceteConsolidado>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Periodo).HasMaxLength(20);
                e.Property(x => x.TotalDebito).HasPrecision(18, 2);
                e.Property(x => x.TotalCredito).HasPrecision(18, 2);
                e.Property(x => x.SaldoFinal).HasPrecision(18, 2);
                e.HasMany(x => x.Linhas).WithOne(l => l.BalanceteConsolidado).HasForeignKey(l => l.BalanceteConsolidadoId).OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => new { x.GrupoConsolidacaoId, x.Periodo }).HasDatabaseName("ix_balancete_grupo_periodo");
            });

            modelBuilder.Entity<BalanceteLinha>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.CodigoConta).HasMaxLength(50);
                e.Property(x => x.NomeConta).HasMaxLength(150);
                e.Property(x => x.SaldoAnterior).HasPrecision(18, 2);
                e.Property(x => x.Debito).HasPrecision(18, 2);
                e.Property(x => x.Credito).HasPrecision(18, 2);
                e.Property(x => x.SaldoFinal).HasPrecision(18, 2);
                e.HasIndex(x => x.BalanceteConsolidadoId).HasDatabaseName("ix_balancete_linha_balancete");
            });

            modelBuilder.Entity<EliminacaoIntercompany>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Periodo).HasMaxLength(20);
                e.Property(x => x.Valor).HasPrecision(18, 2);
                e.Property(x => x.Descricao).HasMaxLength(500);
                e.Property(x => x.Estado).HasMaxLength(50);
                e.HasIndex(x => new { x.GrupoConsolidacaoId, x.Periodo }).HasDatabaseName("ix_eliminacao_grupo_periodo");
            });

            // ===== FIN-PO: Planejamento e Orçamento =====
            modelBuilder.Entity<VersaoOrcamentaria>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Nome).HasMaxLength(150);
                e.HasMany(x => x.Linhas).WithOne(l => l.Versao).HasForeignKey(l => l.VersaoId).OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => new { x.TenantId, x.Status }).HasDatabaseName("ix_versao_orcamentaria_tenant_status");
            });

            modelBuilder.Entity<LinhaOrcamento>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Periodo).HasMaxLength(20);
                e.Property(x => x.ValorOrcado).HasPrecision(18, 2);
                e.Property(x => x.ValorRealizado).HasPrecision(18, 2);
                e.Property(x => x.VariacaoValor).HasPrecision(18, 2);
                e.Property(x => x.VariacaoPercentual).HasPrecision(18, 4);
                e.HasIndex(x => x.VersaoId).HasDatabaseName("ix_linha_orcamento_versao");
            });

            modelBuilder.Entity<PeriodoOrcamentario>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => new { x.TenantId, x.Status }).HasDatabaseName("ix_periodo_orcamentario_tenant_status");
            });

            modelBuilder.Entity<Budget>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Tipo).HasMaxLength(50);
                e.Property(x => x.Valor).HasPrecision(18, 2);
                e.HasOne(x => x.Periodo).WithMany().HasForeignKey(x => x.PeriodoId).HasConstraintName("fk_budget_periodo").OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => x.PeriodoId).HasDatabaseName("ix_budget_periodo");
            });

            modelBuilder.Entity<BudgetAlocacao>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.ValorAlocado).HasPrecision(18, 2);
                e.HasOne<Budget>().WithMany().HasForeignKey(x => x.BudgetId).HasConstraintName("fk_budget_alocacao_budget").OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => x.BudgetId).HasDatabaseName("ix_budget_alocacao_budget");
            });

            modelBuilder.Entity<MetaCategoria>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Nome).HasMaxLength(150);
                e.Property(x => x.Codigo).HasMaxLength(50);
                e.HasIndex(x => new { x.TenantId, x.Codigo }).HasDatabaseName("ix_meta_categoria_tenant_codigo");
            });

            modelBuilder.Entity<Meta>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Tipo).HasMaxLength(50);
                e.Property(x => x.Prioridade).HasMaxLength(50);
                e.HasOne<MetaCategoria>().WithMany().HasForeignKey(x => x.CategoriaId).HasConstraintName("fk_meta_categoria").OnDelete(DeleteBehavior.Restrict);
                e.HasMany(x => x.Milestones).WithOne(m => m.Meta).HasForeignKey(m => m.MetaId).OnDelete(DeleteBehavior.Cascade);
                e.HasMany(x => x.Contribuicoes).WithOne(c => c.Meta).HasForeignKey(c => c.MetaId).OnDelete(DeleteBehavior.Cascade);
                e.HasMany(x => x.Trackings).WithOne(t => t.Meta).HasForeignKey(t => t.MetaId).OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => x.CategoriaId).HasDatabaseName("ix_meta_categoria_ref");
            });

            modelBuilder.Entity<MetaMilestone>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Descricao).HasMaxLength(255);
                e.HasIndex(x => x.MetaId).HasDatabaseName("ix_meta_milestone_meta");
            });

            modelBuilder.Entity<MetaContribuicao>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Valor).HasPrecision(18, 2);
                e.Property(x => x.Tipo).HasMaxLength(50);
                e.HasIndex(x => x.MetaId).HasDatabaseName("ix_meta_contribuicao_meta");
            });

            modelBuilder.Entity<MetaTracking>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Percentual).HasPrecision(18, 4);
                e.Property(x => x.StatusProgresso).HasMaxLength(50);
                e.HasIndex(x => x.MetaId).HasDatabaseName("ix_meta_tracking_meta");
            });

            // ===== FIN-TS: Tesouraria e Gestão de Liquidez =====
            modelBuilder.Entity<ContaFinanceira>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Nome).HasMaxLength(150);
                e.Property(x => x.NumeroConta).HasMaxLength(50);
                e.Property(x => x.Nota).HasMaxLength(500);
                e.Property(x => x.SaldoAbertura).HasPrecision(18, 2);
                e.HasMany(x => x.Transacoes).WithOne(t => t.ContaFinanceira).HasForeignKey(t => t.ContaFinanceiraId).OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => new { x.TenantId, x.Fechada }).HasDatabaseName("ix_conta_financeira_tenant_fechada");
            });

            modelBuilder.Entity<TransacaoContaFinanceira>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Valor).HasPrecision(18, 2);
                e.Property(x => x.Subtipo).HasMaxLength(50);
                e.Property(x => x.Nota).HasMaxLength(500);
                e.HasIndex(x => new { x.ContaFinanceiraId, x.DataOperacao }).HasDatabaseName("ix_transacao_conta_data");
            });

            modelBuilder.Entity<MovimentoFinanceiro>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Credito).HasPrecision(18, 2);
                e.Property(x => x.Debito).HasPrecision(18, 2);
                e.HasIndex(x => new { x.TenantId, x.Conciliado }).HasDatabaseName("ix_movimento_financeiro_conciliado");
                e.HasIndex(x => x.Emissao).HasDatabaseName("ix_movimento_financeiro_emissao");
            });

            modelBuilder.Entity<Cheque>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Valor).HasPrecision(18, 2);
                e.HasIndex(x => new { x.TenantId, x.Situacao }).HasDatabaseName("ix_cheque_tenant_situacao");
                e.HasIndex(x => x.Vencimento).HasDatabaseName("ix_cheque_vencimento");
            });

            modelBuilder.Entity<CaixaOperacional>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.ValorInicial).HasPrecision(18, 2);
                e.Property(x => x.ValorFechamento).HasPrecision(18, 2);
                e.Property(x => x.TotalComprovantesCartao).HasPrecision(18, 2);
                e.Property(x => x.TotalCheques).HasPrecision(18, 2);
                e.Property(x => x.ObservacaoFechamento).HasMaxLength(500);
                e.HasIndex(x => new { x.TenantId, x.Status }).HasDatabaseName("ix_caixa_operacional_tenant_status");
            });

            // Aplica convenções globais do ContextBase (snake_case, tenant filter, soft-delete)
            base.OnModelCreating(modelBuilder);
        }
    }
}
