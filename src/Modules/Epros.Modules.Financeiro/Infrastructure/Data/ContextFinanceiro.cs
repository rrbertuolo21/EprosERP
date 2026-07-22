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

            // Aplica convenções globais do ContextBase (snake_case, tenant filter, soft-delete)
            base.OnModelCreating(modelBuilder);
        }
    }
}
