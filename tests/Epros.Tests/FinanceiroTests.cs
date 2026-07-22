using Epros.Modules.Financeiro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Application.Commands;
using Epros.Modules.Financeiro.Application.EventHandlers;
using Epros.Modules.Financeiro.Application.Handlers;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Modules.Financeiro.Infrastructure.Jobs;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Enums;
using Epros.Shared.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Testes unitários do módulo Financeiro no modelo FIEL (ContasAPagar / ContasAReceber).
    /// Cobertura: criação, baixa parcial (itens), baixa/estorno conciliado, cancelamento e integração via Outbox.
    /// </summary>
    public class FinanceiroTests
    {
        private const string TenantId = "tenant-financeiro-001";
        private const string UserId = "user-financeiro-001";

        private static ContasAPagar NovaContaPagar(decimal valor, ESituacao situacao = ESituacao.Aberto)
            => new ContasAPagar(
                pessoaId: Guid.NewGuid(),
                planoDeContasFinanceiroItemId: Guid.NewGuid(),
                fatoGeradorFinanceiroId: Guid.NewGuid(),
                nomePessoa: "Fornecedor Teste",
                situacao: situacao,
                dataVencimento: DateTime.UtcNow.Date.AddDays(10),
                dataEmissao: DateTime.UtcNow.Date,
                dataBaixa: null,
                documento: "NF-001",
                valorTitulo: valor,
                valorInicialDesconto: 0m,
                valorInicialMulta: 0m,
                valorInicialJuros: 0m,
                valorInicialAcrescimo: 0m,
                numeroParcela: 1,
                detalhamento: "Título de teste",
                justificativaCancelamento: null,
                tenantId: TenantId,
                criadoPor: UserId);

        private static ContasAReceber NovaContaReceber(decimal valor, ESituacao situacao = ESituacao.Aberto)
            => new ContasAReceber(
                pessoaId: Guid.NewGuid(),
                planoDeContasFinanceiroItemId: Guid.NewGuid(),
                fatoGeradorFinanceiroId: Guid.NewGuid(),
                nomePessoa: "Cliente Teste",
                situacao: situacao,
                dataVencimento: DateTime.UtcNow.Date.AddDays(30),
                dataEmissao: DateTime.UtcNow.Date,
                dataBaixa: null,
                documento: "NF-100",
                valorTitulo: valor,
                valorInicialDesconto: 0m,
                valorInicialMulta: 0m,
                valorInicialJuros: 0m,
                valorInicialAcrescimo: 0m,
                numeroParcela: 1,
                detalhamento: "Título de teste",
                justificativaCancelamento: null,
                tenantId: TenantId,
                criadoPor: UserId);

        // ─────────────────────────────────────────────────────────────────────
        // CONTAS A PAGAR (fiel)
        // ─────────────────────────────────────────────────────────────────────

        [Fact(DisplayName = "CP-01 | ContasAPagar criada com dados válidos deve ser válida e em aberto")]
        public void CriarContasAPagar_DadosValidos_DeveSerValida()
        {
            var conta = NovaContaPagar(1500.00m);

            Assert.True(conta.IsValid);
            Assert.Equal(ESituacao.Aberto, conta.Situacao);
            Assert.Null(conta.DataBaixa);
            Assert.Equal(1500.00m, conta.ValorTotalAPagarTitulo);
        }

        [Fact(DisplayName = "CP-02 | ContasAPagar sem plano de contas deve ser inválida")]
        public void CriarContasAPagar_SemPlano_DeveSerInvalida()
        {
            var conta = new ContasAPagar(
                Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), "Fornecedor",
                ESituacao.Aberto, DateTime.UtcNow.Date.AddDays(5), DateTime.UtcNow.Date, null,
                "NF-X", 500m, 0m, 0m, 0m, 0m, 1, "Teste", null, TenantId, UserId);

            Assert.False(conta.IsValid);
            Assert.Contains(conta.Notifications, n => n.Key == nameof(conta.PlanoDeContasFinanceiroItemId));
        }

        [Fact(DisplayName = "CP-03 | Baixa conciliada total deve mudar situação para Pago")]
        public void BaixarContasAPagar_Conciliado_DeveFicarPago()
        {
            var conta = NovaContaPagar(800m);

            conta.BaixarTituloConciliado(800m);

            Assert.Equal(ESituacao.Pago, conta.Situacao);
            Assert.NotNull(conta.DataBaixa);
            Assert.Equal(800m, conta.ValorTotalPago);
            Assert.Equal(0m, conta.ValorTotalAPagarTitulo);
        }

        [Fact(DisplayName = "CP-04 | Estorno da baixa conciliada deve retornar a Aberto")]
        public void EstornarContasAPagar_Conciliado_DeveVoltarParaAberto()
        {
            var conta = NovaContaPagar(800m);
            conta.BaixarTituloConciliado(800m);

            conta.EstornarTituloConciliado();

            Assert.Equal(ESituacao.Aberto, conta.Situacao);
            Assert.Equal(0m, conta.ValorTotalPago);
            Assert.Null(conta.DataBaixa);
        }

        [Fact(DisplayName = "CP-05 | Baixa parcial via item mantém saldo a pagar")]
        public void IncluirItemParcial_ContasAPagar_MantemSaldo()
        {
            var conta = NovaContaPagar(3000m);

            conta.IncluirContasAPagarItem(Guid.NewGuid(), null, ETipoPagamento.Dinheiro,
                valorParcela: 1000m, valorPago: 1000m, valorDesconto: 0m, valorMulta: 0m,
                valorJuros: 0m, valorAcrescimo: 0m, dataPagamento: DateTime.UtcNow, UserId);

            Assert.True(conta.IsValid);
            Assert.Single(conta.ContasAPagarItens);
            Assert.Equal(1000m, conta.ValorTotalPago);
            Assert.Equal(2000m, conta.ValorTotalAPagarTitulo);
        }

        [Fact(DisplayName = "CP-06 | Duas baixas parciais que somam o total zeram o saldo")]
        public void DuasBaixasParciais_ContasAPagar_ZeramSaldo()
        {
            var conta = NovaContaPagar(2000m);

            conta.IncluirContasAPagarItem(Guid.NewGuid(), null, ETipoPagamento.Dinheiro,
                1000m, 1000m, 0m, 0m, 0m, 0m, DateTime.UtcNow, UserId);
            conta.IncluirContasAPagarItem(Guid.NewGuid(), null, ETipoPagamento.Dinheiro,
                1000m, 1000m, 0m, 0m, 0m, 0m, DateTime.UtcNow, UserId);

            Assert.Equal(2, conta.ContasAPagarItens.Count);
            Assert.Equal(2000m, conta.ValorTotalPago);
            Assert.Equal(0m, conta.ValorTotalAPagarTitulo);
        }

        [Fact(DisplayName = "CP-07 | Cancelamento com justificativa deve ser válido")]
        public void CancelarContasAPagar_ComJustificativa_DeveSerCancelada()
        {
            var conta = NovaContaPagar(900m);

            conta.Cancelar("Fornecedor devolveu o produto antes do prazo", UserId);

            Assert.True(conta.IsValid);
            Assert.Equal(ESituacao.Cancelado, conta.Situacao);
        }

        [Fact(DisplayName = "CP-08 | Cancelamento sem justificativa deve ser inválido")]
        public void CancelarContasAPagar_SemJustificativa_DeveSerInvalido()
        {
            var conta = NovaContaPagar(200m);

            conta.Cancelar(null, UserId);

            Assert.False(conta.IsValid);
        }

        // ─────────────────────────────────────────────────────────────────────
        // CONTAS A RECEBER (fiel)
        // ─────────────────────────────────────────────────────────────────────

        [Fact(DisplayName = "CR-01 | ContasAReceber criada com dados válidos deve ser válida e em aberto")]
        public void CriarContasAReceber_DadosValidos_DeveSerValida()
        {
            var conta = NovaContaReceber(2500m);

            Assert.True(conta.IsValid);
            Assert.Equal(ESituacao.Aberto, conta.Situacao);
            Assert.Equal(2500m, conta.ValorTotalAReceberTitulo);
        }

        [Fact(DisplayName = "CR-02 | Baixa conciliada total deve mudar situação para Pago")]
        public void BaixarContasAReceber_Conciliado_DeveFicarPago()
        {
            var conta = NovaContaReceber(5000m);

            conta.BaixarTituloConciliado(5000m);

            Assert.Equal(ESituacao.Pago, conta.Situacao);
            Assert.NotNull(conta.DataBaixa);
            Assert.Equal(5000m, conta.ValorTotalRecebido);
        }

        [Fact(DisplayName = "CR-03 | Baixa parcial via item mantém saldo a receber")]
        public void IncluirItemParcial_ContasAReceber_MantemSaldo()
        {
            var conta = NovaContaReceber(4000m);

            conta.IncluirContasAReceberItem(Guid.NewGuid(), null, ETipoPagamento.BoletoBancario,
                valorParcela: 2000m, valorPago: 2000m, valorDesconto: 0m, valorMulta: 0m,
                valorJuros: 0m, valorAcrescimo: 0m, dataRecebimento: DateTime.UtcNow, UserId);

            Assert.True(conta.IsValid);
            Assert.Single(conta.ContasAReceberItens);
            Assert.Equal(2000m, conta.ValorTotalRecebido);
            Assert.Equal(2000m, conta.ValorTotalAReceberTitulo);
        }

        [Fact(DisplayName = "CR-04 | Cancelamento com justificativa deve ser válido")]
        public void CancelarContasAReceber_ComJustificativa_DeveSerCancelada()
        {
            var conta = NovaContaReceber(1200m);

            conta.Cancelar("Cliente desistiu da compra antes da entrega", UserId);

            Assert.True(conta.IsValid);
            Assert.Equal(ESituacao.Cancelado, conta.Situacao);
        }

        [Fact(DisplayName = "CR-05 | ContasAReceber rastreia origem via FatoGeradorFinanceiroId")]
        public void CriarContasAReceber_ComFatoGerador_DeveRastrear()
        {
            var fatoId = Guid.NewGuid();
            var conta = new ContasAReceber(
                Guid.NewGuid(), Guid.NewGuid(), fatoId, "Cliente",
                ESituacao.Aberto, DateTime.UtcNow.Date.AddDays(30), DateTime.UtcNow.Date, null,
                "NF-0001234", 8700m, 0m, 0m, 0m, 0m, 1, "NF-e", null, TenantId, UserId);

            Assert.True(conta.IsValid);
            Assert.Equal(fatoId, conta.FatoGeradorFinanceiroId);
            Assert.Equal("NF-0001234", conta.Documento);
        }

        // ─────────────────────────────────────────────────────────────────────
        // INTEGRAÇÃO OUTBOX PROCESSOR & EVENT HANDLERS (fiel)
        // ─────────────────────────────────────────────────────────────────────

        [Fact(DisplayName = "OB-01 | OutboxProcessorJob deve processar CompraLancada e criar ContasAPagar fiel")]
        public async Task OutboxProcessorJob_DeveProcessarCompraLancada_ECriarContasAPagar()
        {
            var dbName = Guid.NewGuid().ToString();
            var tenantId = "tenant-outbox-test";
            var userId = "user-outbox-test";

            using var context = CreateInMemoryContext(dbName, tenantId, userId);

            var fornecedorId = Guid.NewGuid();
            context.PessoasLookup.Add(new PessoaLookup
            {
                Id = fornecedorId,
                EhFornecedor = true,
                Status = 3,
                TipoPessoa = 2,
                TenantId = tenantId,
                CriadoPor = userId,
                CriadoEm = DateTime.UtcNow
            });
            context.PessoasJuridicasLookup.Add(new PessoaJuridicaLookup
            {
                PessoaId = fornecedorId,
                Cnpj = "12345678000199",
                RazaoSocial = "Fornecedor Outbox Test S/A",
                TenantId = tenantId,
                CriadoPor = userId,
                CriadoEm = DateTime.UtcNow
            });

            var payload = new
            {
                CompraId = Guid.NewGuid(),
                TenantId = tenantId,
                FornecedorCnpj = "12.345.678/0001-99",
                FornecedorNome = "Fornecedor Outbox Test S/A",
                NumeroNota = "NF-99988",
                ValorTotal = 4500.50m,
                Itens = new object[0]
            };
            var outboxMessage = new OutboxMessage(tenantId, "CompraLancada", JsonSerializer.Serialize(payload));
            context.OutboxMessages.Add(outboxMessage);

            await context.SaveChangesAsync();

            var services = new ServiceCollection();
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CompraLancadaEventHandler).Assembly));
            services.AddSingleton(context);

            var httpContextAccessor = new HttpContextAccessor();
            services.AddSingleton<IHttpContextAccessor>(httpContextAccessor);

            var serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var job = new OutboxProcessorJob(context, mediator, httpContextAccessor);

            await job.Execute(null!);

            var msgProcessada = await context.OutboxMessages.IgnoreQueryFilters().FirstAsync(m => m.Id == outboxMessage.Id);
            Assert.NotNull(msgProcessada.ProcessadoEm);
            Assert.Null(msgProcessada.Erro);

            var contaCriada = await context.ContasAPagarAgregado.IgnoreQueryFilters().FirstOrDefaultAsync(cp => cp.Documento == "NF-99988");
            Assert.NotNull(contaCriada);
            Assert.Equal(4500.50m, contaCriada.ValorTitulo);
            Assert.Equal(fornecedorId, contaCriada.PessoaId);
        }

        [Fact(DisplayName = "OB-02 | OutboxProcessorJob deve cadastrar fornecedor e criar ContasAPagar fiel")]
        public async Task OutboxProcessorJob_FornecedorNaoCadastrado_DeveCadastrarECriarContasAPagar()
        {
            var dbName = Guid.NewGuid().ToString();
            var tenantId = "tenant-outbox-new-supplier";
            var userId = "user-outbox-test";

            using var context = CreateInMemoryContext(dbName, tenantId, userId);

            var payload = new
            {
                CompraId = Guid.NewGuid(),
                TenantId = tenantId,
                FornecedorCnpj = "98.765.432/0001-11",
                FornecedorNome = "Novo Fornecedor S/A",
                NumeroNota = "NF-777",
                ValorTotal = 150.00m,
                Itens = new object[0]
            };
            var outboxMessage = new OutboxMessage(tenantId, "CompraLancada", JsonSerializer.Serialize(payload));
            context.OutboxMessages.Add(outboxMessage);

            await context.SaveChangesAsync();

            var services = new ServiceCollection();
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CompraLancadaEventHandler).Assembly));
            services.AddSingleton(context);

            var httpContextAccessor = new HttpContextAccessor();
            services.AddSingleton<IHttpContextAccessor>(httpContextAccessor);

            var serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var job = new OutboxProcessorJob(context, mediator, httpContextAccessor);

            await job.Execute(null!);

            var msgProcessada = await context.OutboxMessages.IgnoreQueryFilters().FirstAsync(m => m.Id == outboxMessage.Id);
            Assert.NotNull(msgProcessada.ProcessadoEm);

            var fornecedorJuridico = await context.PessoasJuridicasLookup
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(pj => pj.Cnpj == "98765432000111" && pj.TenantId == tenantId);
            Assert.NotNull(fornecedorJuridico);
            Assert.Equal("Novo Fornecedor S/A", fornecedorJuridico.RazaoSocial);

            var fornecedorPessoa = await context.PessoasLookup
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == fornecedorJuridico.PessoaId && p.TenantId == tenantId);
            Assert.NotNull(fornecedorPessoa);
            Assert.True(fornecedorPessoa.EhFornecedor);

            var contaCriada = await context.ContasAPagarAgregado.IgnoreQueryFilters().FirstOrDefaultAsync(cp => cp.Documento == "NF-777");
            Assert.NotNull(contaCriada);
            Assert.Equal(150.00m, contaCriada.ValorTitulo);
            Assert.Equal(fornecedorJuridico.PessoaId, contaCriada.PessoaId);
        }

        // ─────────────────────────────────────────────────────────────────────
        // PLANO DE CONTAS FINANCEIRO — HANDLERS (CQRS)
        // ─────────────────────────────────────────────────────────────────────

        [Fact(DisplayName = "PC-01 | CriarPlanoDeContasFinanceiro deve persistir plano válido")]
        public async Task CriarPlanoDeContas_DadosValidos_DevePersistir()
        {
            var tenantId = "tenant-plano-01";
            var userId = "user-plano-01";
            using var context = CreateInMemoryContext(Guid.NewGuid().ToString(), tenantId, userId);
            var tenant = new TestTenantProvider(tenantId);
            var user = new TestCurrentUser(userId);

            var handler = new CriarPlanoDeContasFinanceiroCommandHandler(context, tenant, user);
            var cmd = new CriarPlanoDeContasFinanceiroCommand(
                ConfiguracaoCodigoNaturezaFinanceiraRecebimentoId: null,
                ConfiguracaoCodigoNaturezaFinanceiraPagamentoId: null,
                Descricao: "Plano Padrão",
                Mascara: "9.9.9",
                EmpresaIds: null);

            var result = await handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.Sucesso);
            Assert.Single(context.PlanosDeContas);
        }

        [Fact(DisplayName = "PC-02 | CriarPlanoDeContasFinanceiroItem deve rejeitar plano inexistente")]
        public async Task CriarPlanoItem_PlanoInexistente_DeveRejeitar()
        {
            var tenantId = "tenant-plano-02";
            var userId = "user-plano-02";
            using var context = CreateInMemoryContext(Guid.NewGuid().ToString(), tenantId, userId);
            var tenant = new TestTenantProvider(tenantId);
            var user = new TestCurrentUser(userId);

            var handler = new CriarPlanoDeContasFinanceiroItemCommandHandler(context, tenant, user);
            var cmd = new CriarPlanoDeContasFinanceiroItemCommand(
                PlanoDeContasFinanceiroId: Guid.NewGuid(),
                Codigo: "1.1",
                Descricao: "Receitas",
                TipoDetalhamento: ETipoDetalhamento.Credito,
                MovimentaCaixa: true);

            var result = await handler.Handle(cmd, CancellationToken.None);

            Assert.False(result.Sucesso);
            Assert.Contains("Plano de contas financeiro não encontrado.", result.Erros);
            Assert.Empty(context.PlanoDeContasItens);
        }

        [Fact(DisplayName = "PC-03 | CriarPlanoDeContasFinanceiroItem deve persistir item de plano existente")]
        public async Task CriarPlanoItem_PlanoExistente_DevePersistir()
        {
            var tenantId = "tenant-plano-03";
            var userId = "user-plano-03";
            using var context = CreateInMemoryContext(Guid.NewGuid().ToString(), tenantId, userId);
            var tenant = new TestTenantProvider(tenantId);
            var user = new TestCurrentUser(userId);

            var criarPlano = new CriarPlanoDeContasFinanceiroCommandHandler(context, tenant, user);
            var planoResult = await criarPlano.Handle(new CriarPlanoDeContasFinanceiroCommand(
                null, null, "Plano com Item", "9.9", null), CancellationToken.None);
            Assert.True(planoResult.Sucesso);
            var planoId = (Guid)planoResult.Dados.GetType().GetProperty("Id")!.GetValue(planoResult.Dados)!;

            var criarItem = new CriarPlanoDeContasFinanceiroItemCommandHandler(context, tenant, user);
            var itemResult = await criarItem.Handle(new CriarPlanoDeContasFinanceiroItemCommand(
                PlanoDeContasFinanceiroId: planoId,
                Codigo: "1.1",
                Descricao: "Vendas",
                TipoDetalhamento: ETipoDetalhamento.Credito,
                MovimentaCaixa: true), CancellationToken.None);

            Assert.True(itemResult.Sucesso);
            var item = await context.PlanoDeContasItens.FirstAsync();
            Assert.Equal(planoId, item.PlanoDeContasFinanceiroId);
            Assert.Equal("1.1", item.Codigo);
        }

        // Helpers de Contexto InMemory
        private ContextFinanceiro CreateInMemoryContext(string databaseName, string tenantId, string userId)
        {
            var options = new DbContextOptionsBuilder<ContextFinanceiro>()
                .UseInMemoryDatabase(databaseName: databaseName)
                .Options;

            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            var context = new ContextFinanceiro(options, tenantProvider, currentUser);
            context.Database.EnsureCreated();
            return context;
        }

        private class TestTenantProvider : ITenantProvider
        {
            private readonly string _tenantId;
            public TestTenantProvider(string tenantId) => _tenantId = tenantId;
            public string GetTenantId() => _tenantId;
        }

        private class TestCurrentUser : ICurrentUser
        {
            private readonly string _userId;
            public TestCurrentUser(string userId) => _userId = userId;
            public string? GetUserId() => _userId;
            public string? GetUserName() => "Test User";
            public string? GetUserEmail() => "test@epros.com";
        }
    }
}
