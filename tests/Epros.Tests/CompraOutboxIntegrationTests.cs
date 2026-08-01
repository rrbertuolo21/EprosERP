using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Handlers;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Modules.Financeiro.Application.EventHandlers;
using Epros.Modules.Financeiro.Domain.Entities;
using Epros.Modules.Financeiro.Infrastructure.Jobs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Epros.Tests
{
    public class CompraOutboxIntegrationTests
    {
        [Fact]
        public async Task Deve_Lancar_Compra_E_Executar_Outbox_Criando_ContasPagar_No_Financeiro()
        {
            // Arrange
            var dbName = "db_compra_lancar_outbox_integration";
            var tenantId = "tenant-test-compra";
            var userId = "user-test-compra";

            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            var estoqueContext = CreateEstoqueContext(dbName, tenantProvider, currentUser);
            var financeiroContext = CreateFinanceiroContext(dbName, tenantProvider, currentUser);

            // Pre-seed a product in Estoque
            var prodSku = "SKU-COMPRA-123";
            var produto = new Produto(prodSku, "Produto Compra Teste", 10m, tenantId, userId);
            estoqueContext.Produtos.Add(produto);
            await estoqueContext.SaveChangesAsync();

            var lancarHandler = new LancarCompraCommandHandler(estoqueContext, tenantProvider, currentUser);

            var command = new LancarCompraCommand(
                FornecedorCnpj: "12.345.678/0001-90",
                FornecedorNome: "Fornecedor Teste Ltda",
                NumeroNota: "000123",
                ChaveAcesso: "12345678901234567890123456789012345678901234", // 44 chars
                ValorTotal: 150m,
                DataEmissao: DateTime.UtcNow,
                Itens: new List<ItemCompraInput>
                {
                    new ItemCompraInput(prodSku, "Produto Compra Teste", 10, 15m, 1.5m, 0.5m)
                }
            );

            // Act 1: Launch the purchase
            var result = await lancarHandler.Handle(command, CancellationToken.None);

            // Assert 1: Purchase launched and outbox written
            Assert.True(result.Sucesso);
            var outboxMessages = await estoqueContext.OutboxMessages.ToListAsync();
            Assert.Single(outboxMessages);
            Assert.Equal("CompraLancada", outboxMessages[0].EventType);
            Assert.Null(outboxMessages[0].ProcessadoEm);

            var outboxFinanceiro = await financeiroContext.OutboxMessages.ToListAsync();
            Assert.Single(outboxFinanceiro);

            // Setup financeiro handlers & mediator stub
            var handlerFinanceiro = new CompraLancadaEventHandler(financeiroContext);
            var mediator = new TestMediator(async (notification, ct) =>
            {
                if (notification is CompraLancadaEventNotification cln)
                {
                    await handlerFinanceiro.Handle(cln, ct);
                }
            });

            var httpContextAccessor = new TestHttpContextAccessor();
            var outboxJob = new OutboxProcessorJob(financeiroContext, mediator, httpContextAccessor);

            // Act 2: Run OutboxProcessorJob
            await outboxJob.Execute(null!);

            // Assert 2: Outbox processed
            var outboxMessagesPost = await estoqueContext.OutboxMessages.AsNoTracking().ToListAsync();
            Assert.Single(outboxMessagesPost);
            Assert.Null(outboxMessagesPost[0].Erro);
            Assert.NotNull(outboxMessagesPost[0].ProcessadoEm);

            // Assert 3: Account Payable generated in Financeiro (modelo fiel)
            var contasPagar = await financeiroContext.ContasAPagarAgregado.ToListAsync();
            Assert.Single(contasPagar);
            Assert.Equal(150m, contasPagar[0].ValorTitulo);
            Assert.Equal(Epros.Shared.Domain.Enums.ESituacao.Aberto, contasPagar[0].Situacao);
            Assert.Contains("000123", contasPagar[0].Documento);
        }

        [Fact]
        public async Task Deve_Cancelar_Compra_E_Executar_Outbox_Estornando_Estoque_E_Financeiro()
        {
            // Arrange
            var dbName = "db_compra_cancelar_outbox_integration";
            var tenantId = "tenant-test-compra-cancel";
            var userId = "user-test-compra-cancel";

            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            var estoqueContext = CreateEstoqueContext(dbName, tenantProvider, currentUser);
            var financeiroContext = CreateFinanceiroContext(dbName, tenantProvider, currentUser);

            // Pre-seed a product in Estoque
            var prodSku = "SKU-COMPRA-456";
            var produto = new Produto(prodSku, "Produto Cancelar Teste", 10m, tenantId, userId);
            produto.LancarEntradaEstoque(5, 10m, userId); // Initial: qty = 5, average cost = 10m
            estoqueContext.Produtos.Add(produto);
            await estoqueContext.SaveChangesAsync();

            // 1. Launch a purchase
            var lancarHandler = new LancarCompraCommandHandler(estoqueContext, tenantProvider, currentUser);
            var commandLancar = new LancarCompraCommand(
                FornecedorCnpj: "98.765.432/0001-10",
                FornecedorNome: "Fornecedor Cancelar Ltda",
                NumeroNota: "000456",
                ChaveAcesso: "98765432109876543210987654321098765432109876", // 44 chars
                ValorTotal: 200m,
                DataEmissao: DateTime.UtcNow,
                Itens: new List<ItemCompraInput>
                {
                    new ItemCompraInput(prodSku, "Produto Cancelar Teste", 10, 20m, 2.0m, 1.0m) // qty = 10, price = 20m
                }
            );

            var resultLancar = await lancarHandler.Handle(commandLancar, CancellationToken.None);
            Assert.True(resultLancar.Sucesso);

            // Verify stock after launch:
            // Initial: 5 * 10 = 50.
            // New: 10 * 20 = 200.
            // Total cost: 250. Total qty: 15. New average cost: 250 / 15 = 16.666...
            var prodAfterLancar = await estoqueContext.Produtos.FirstOrDefaultAsync(p => p.Sku == prodSku);
            Assert.NotNull(prodAfterLancar);
            Assert.Equal(15m, prodAfterLancar.SaldoEstoque);

            // Process Lancar Outbox to create ContaPagar
            var handlerFinanceiroLancar = new CompraLancadaEventHandler(financeiroContext);
            var mediatorLancar = new TestMediator(async (notification, ct) =>
            {
                if (notification is CompraLancadaEventNotification cln)
                {
                    await handlerFinanceiroLancar.Handle(cln, ct);
                }
            });

            var httpContextAccessor = new TestHttpContextAccessor();
            var outboxJobLancar = new OutboxProcessorJob(financeiroContext, mediatorLancar, httpContextAccessor);
            await outboxJobLancar.Execute(null!);

            // Verify ContaPagar exists (modelo fiel)
            var contasPagar = await financeiroContext.ContasAPagarAgregado.ToListAsync();
            Assert.Single(contasPagar);
            Assert.Equal(Epros.Shared.Domain.Enums.ESituacao.Aberto, contasPagar[0].Situacao);

            // 2. Cancel the purchase
            Assert.NotNull(resultLancar.Dados);
            var compraIdProperty = resultLancar.Dados.GetType().GetProperty("CompraId");
            Assert.NotNull(compraIdProperty);
            var compraIdObj = compraIdProperty.GetValue(resultLancar.Dados);
            Assert.NotNull(compraIdObj);
            var compraId = (Guid)compraIdObj;

            var cancelarHandler = new CancelarCompraCommandHandler(estoqueContext, tenantProvider, currentUser);
            var commandCancelar = new CancelarCompraCommand(compraId, "Erro no lancamento da nota fiscal");

            var resultCancelar = await cancelarHandler.Handle(commandCancelar, CancellationToken.None);
            Assert.True(resultCancelar.Sucesso);

            // Verify Stock decreased but Average Cost is stable:
            // Output of 10 items. Qty should be 15 - 10 = 5.
            // Average cost should remain same as before output (16.666...).
            var prodAfterCancelar = await estoqueContext.Produtos.FirstOrDefaultAsync(p => p.Sku == prodSku);
            Assert.NotNull(prodAfterCancelar);
            Assert.Equal(5m, prodAfterCancelar.SaldoEstoque);
            Assert.Equal(prodAfterLancar.CustoMedio, prodAfterCancelar.CustoMedio);

            // E deve existir um MovimentoEstoque de Saída correspondente
            var movimentosEstoque = await estoqueContext.MovimentosEstoque.Where(m => m.ProdutoId == prodAfterCancelar.Id && m.Tipo == "Saída").ToListAsync();
            Assert.Single(movimentosEstoque);
            Assert.Contains("000456", movimentosEstoque[0].Historico);

            // Setup cancellation mediator & processor
            var handlerFinanceiroCancel = new CompraCanceladaEventHandler(financeiroContext);
            var mediatorCancel = new TestMediator(async (notification, ct) =>
            {
                if (notification is CompraCanceladaEventNotification ccn)
                {
                    await handlerFinanceiroCancel.Handle(ccn, ct);
                }
            });

            var outboxJobCancel = new OutboxProcessorJob(financeiroContext, mediatorCancel, httpContextAccessor);

            // Execute outbox for cancellation
            await outboxJobCancel.Execute(null!);

            // Assert: Outbox processed
            var outboxMessagesCancel = await estoqueContext.OutboxMessages.AsNoTracking().ToListAsync();
            Assert.Equal(2, outboxMessagesCancel.Count);
            var cancelOutboxMsg = outboxMessagesCancel.FirstOrDefault(o => o.EventType == "CompraCancelada");
            Assert.NotNull(cancelOutboxMsg);
            Assert.Null(cancelOutboxMsg.Erro);
            Assert.NotNull(cancelOutboxMsg.ProcessadoEm);

            // Assert: ContaPagar should be Cancelada (modelo fiel)
            var contasPagarPost = await financeiroContext.ContasAPagarAgregado.ToListAsync();
            Assert.Single(contasPagarPost);
            Assert.Equal(Epros.Shared.Domain.Enums.ESituacao.Cancelado, contasPagarPost[0].Situacao);
        }

        private ContextEstoque CreateEstoqueContext(string dbName, ITenantProvider tp, ICurrentUser cu)
        {
            var options = new DbContextOptionsBuilder<ContextEstoque>().UseInMemoryDatabase(dbName).Options;
            return new ContextEstoque(options, tp, cu);
        }

        private ContextFinanceiro CreateFinanceiroContext(string dbName, ITenantProvider tp, ICurrentUser cu)
        {
            var options = new DbContextOptionsBuilder<ContextFinanceiro>().UseInMemoryDatabase(dbName).Options;
            return new ContextFinanceiro(options, tp, cu);
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

        private class TestHttpContextAccessor : IHttpContextAccessor
        {
            public HttpContext? HttpContext { get; set; }
        }

        private class TestMediator : IMediator
        {
            private readonly Func<INotification, CancellationToken, Task> _publishHandler;

            public TestMediator(Func<INotification, CancellationToken, Task> publishHandler)
            {
                _publishHandler = publishHandler;
            }

            public Task Publish(object notification, CancellationToken cancellationToken = default)
            {
                return _publishHandler((INotification)notification, cancellationToken);
            }

            public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
            {
                return _publishHandler(notification, cancellationToken);
            }

            public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
            {
                throw new NotImplementedException();
            }

            public Task<object?> Send(object request, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }
        }
    }
}
