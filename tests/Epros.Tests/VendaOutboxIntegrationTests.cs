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
using Epros.Modules.Vendas.Domain.Entities;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Application.Handlers;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Modules.Vendas.Infrastructure.Jobs;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Modules.Financeiro.Application.EventHandlers;
using Epros.Modules.Financeiro.Domain.Entities;
using Epros.Modules.Fiscal.Infrastructure.Data;
using Epros.Modules.Fiscal.Infrastructure.Services;
using Epros.Modules.Fiscal.Application.Handlers;
using Epros.Modules.Fiscal.Application.Services;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Modules.Estoque.Application.Handlers;
using Epros.Modules.Estoque.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Epros.Tests
{
    public class VendaOutboxIntegrationTests
    {
        [Fact]
        public async Task Deve_Sincronizar_Venda_E_Executar_Outbox_Integrando_Financeiro_E_Fiscal()
        {
            // Arrange
            var dbName = "db_venda_outbox_integration";
            var tenantId = "tenant-test";
            var userId = "user-test";

            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            // DbContexts
            var vendasContext = CreateVendasContext(dbName, tenantProvider, currentUser);
            var financeiroContext = CreateFinanceiroContext(dbName, tenantProvider, currentUser);
            var fiscalContext = CreateFiscalContext(dbName, tenantProvider, currentUser);

            // Seed a product in the fiscal context's product lookup table
            var prodId = Guid.NewGuid();
            var produtoLookup = new Epros.Modules.Fiscal.Infrastructure.Data.ProdutoLookup
            {
                Id = prodId,
                Sku = "SKU-TEST-123",
                Nome = "Produto Integrado Teste",
                TenantId = tenantId
            };
            fiscalContext.ProdutosLookup.Add(produtoLookup);
            await fiscalContext.SaveChangesAsync();

            // Sincronizar Venda Handler
            var sincronizarHandler = new SincronizarVendasCommandHandler(vendasContext, tenantProvider, currentUser);

            var vendaId = Guid.NewGuid();
            var syncId = Guid.NewGuid();
            var itens = new List<VendaItemSyncDto>
            {
                new VendaItemSyncDto { ProdutoId = prodId, Quantidade = 2, PrecoUnitario = 45m }
            };

            var vendaDto = new VendaSyncDto
            {
                Id = vendaId,
                SyncId = syncId,
                CaixaId = "caixa-01",
                Total = 90m,
                Status = "Emitida",
                CriadoEm = DateTime.UtcNow,
                Itens = itens
            };

            var command = new SincronizarVendasCommand(new List<VendaSyncDto> { vendaDto });

            // Act 1: Sincroniza a venda (isso deve criar a venda e enfileirar a OutboxMessage)
            var syncResult = await sincronizarHandler.Handle(command, CancellationToken.None);

            // Assert 1: Venda e Outbox salvos com sucesso
            Assert.True(syncResult.Sucesso);
            var outboxMessages = await vendasContext.OutboxMessages.ToListAsync();
            Assert.Single(outboxMessages);
            Assert.Equal("VendaFaturada", outboxMessages[0].EventType);
            Assert.Null(outboxMessages[0].ProcessadoEm);

            // Setup Event Handlers for Financeiro and Fiscal
            var handlerFinanceiro = new VendaFaturadaEventHandler(financeiroContext);
            var handlerFiscal = new VendaFaturadaFiscalHandler(fiscalContext, new CalculadoraImpostosDocumentoFiscal(new MotorLegadoCalculoFiscalService(), new EmitenteFiscalProviderNulo()));

            // Setup Mediator Stub to route notification to both handlers
            var mediator = new TestMediator(async (notification, ct) =>
            {
                if (notification is VendaFaturadaEventNotification vfn)
                {
                    await handlerFinanceiro.Handle(vfn, ct);
                    await handlerFiscal.Handle(vfn, ct);
                }
            });

            var httpContextAccessor = new TestHttpContextAccessor();
            var outboxJob = new VendasOutboxProcessorJob(vendasContext, mediator, httpContextAccessor);

            // Act 2: Executar o Job do Outbox de Vendas
            await outboxJob.Execute(null!);

            // Assert 2: Job processou e atualizou a mensagem
            var outboxMessagesPost = await vendasContext.OutboxMessages.ToListAsync();
            Assert.Single(outboxMessagesPost);
            Assert.NotNull(outboxMessagesPost[0].ProcessadoEm);

            // Assert 3: Financeiro recebeu a notificação e gerou a ContaReceber (modelo fiel)
            var contasReceber = await financeiroContext.ContasAReceberAgregado.Include(c => c.FatoGeradorFinanceiro).ToListAsync();
            Assert.Single(contasReceber);
            Assert.Equal(90m, contasReceber[0].ValorTitulo);
            Assert.Equal(vendaId, contasReceber[0].FatoGeradorFinanceiro.VendaId);

            // Assert 4: Fiscal recebeu a notificação e gerou o DocumentoFiscal em Rascunho
            var documentosFiscais = await fiscalContext.DocumentosFiscais.Include(d => d.Itens).ToListAsync();
            Assert.Single(documentosFiscais);
            Assert.Equal("65", documentosFiscais[0].Modelo);
            Assert.Equal("Rascunho", documentosFiscais[0].Status);
            Assert.Equal(90m, documentosFiscais[0].Total);
            Assert.Equal(vendaId, documentosFiscais[0].VendaOrigemId);
            Assert.Single(documentosFiscais[0].Itens);
            Assert.Equal("SKU-TEST-123", documentosFiscais[0].Itens[0].Sku);
            Assert.Equal("Produto Integrado Teste", documentosFiscais[0].Itens[0].NomeProduto);
        }

        [Fact]
        public async Task Deve_Cancelar_Venda_E_Executar_Outbox_Cancelando_Integracoes()
        {
            // Arrange
            var dbName = "db_venda_cancelamento_integration";
            var tenantId = "tenant-test";
            var userId = "user-test";

            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            // DbContexts
            var vendasContext = CreateVendasContext(dbName, tenantProvider, currentUser);
            var financeiroContext = CreateFinanceiroContext(dbName, tenantProvider, currentUser);
            var fiscalContext = CreateFiscalContext(dbName, tenantProvider, currentUser);
            var estoqueContext = CreateEstoqueContext(dbName, tenantProvider, currentUser);

            // Seed a product in the fiscal context's product lookup table
            var prodId = Guid.NewGuid();
            var produtoLookup = new Epros.Modules.Fiscal.Infrastructure.Data.ProdutoLookup
            {
                Id = prodId,
                Sku = "SKU-TEST-123",
                Nome = "Produto Integrado Teste",
                TenantId = tenantId
            };
            fiscalContext.ProdutosLookup.Add(produtoLookup);
            await fiscalContext.SaveChangesAsync();

            // Seed a product in the estoque context with initial stock
            var produtoEstoque = new Produto("SKU-TEST-123", "Produto Integrado Teste", 45m, tenantId, userId);
            typeof(Epros.Shared.Domain.Entities.EntidadeSaaSBase).GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.SetValue(produtoEstoque, prodId);
            produtoEstoque.LancarEntradaEstoque(10, 15m, userId); // Saldo inicial = 10, CustoMedio = 15m
            estoqueContext.Produtos.Add(produtoEstoque);
            await estoqueContext.SaveChangesAsync();

            // Sincronizar Venda Handler
            var sincronizarHandler = new SincronizarVendasCommandHandler(vendasContext, tenantProvider, currentUser);

            var vendaId = Guid.NewGuid();
            var syncId = Guid.NewGuid();
            var itens = new List<VendaItemSyncDto>
            {
                new VendaItemSyncDto { ProdutoId = prodId, Quantidade = 2, PrecoUnitario = 45m }
            };

            var vendaDto = new VendaSyncDto
            {
                Id = vendaId,
                SyncId = syncId,
                CaixaId = "caixa-01",
                Total = 90m,
                Status = "Emitida",
                CriadoEm = DateTime.UtcNow,
                Itens = itens
            };

            var command = new SincronizarVendasCommand(new List<VendaSyncDto> { vendaDto });

            // Act 1: Sincroniza a venda (isso deve criar a venda e enfileirar a OutboxMessage VendaFaturada)
            var syncResult = await sincronizarHandler.Handle(command, CancellationToken.None);
            Assert.True(syncResult.Sucesso);

            // Setup Event Handlers for faturamento
            var handlerFinanceiroFat = new VendaFaturadaEventHandler(financeiroContext);
            var handlerFiscalFat = new VendaFaturadaFiscalHandler(fiscalContext, new CalculadoraImpostosDocumentoFiscal(new MotorLegadoCalculoFiscalService(), new EmitenteFiscalProviderNulo()));

            // Routing for VendaFaturada
            var mediatorFat = new TestMediator(async (notification, ct) =>
            {
                if (notification is VendaFaturadaEventNotification vfn)
                {
                    await handlerFinanceiroFat.Handle(vfn, ct);
                    await handlerFiscalFat.Handle(vfn, ct);
                }
            });

            var httpContextAccessor = new TestHttpContextAccessor();
            var outboxJob = new VendasOutboxProcessorJob(vendasContext, mediatorFat, httpContextAccessor);

            // Processar faturamento
            await outboxJob.Execute(null!);

            // Verificar faturamento (modelo fiel)
            var contasReceber = await financeiroContext.ContasAReceberAgregado.ToListAsync();
            Assert.Single(contasReceber);
            Assert.Equal(Epros.Shared.Domain.Enums.ESituacao.Aberto, contasReceber[0].Situacao);

            var documentosFiscais = await fiscalContext.DocumentosFiscais.ToListAsync();
            Assert.Single(documentosFiscais);
            Assert.Equal("Rascunho", documentosFiscais[0].Status);

            // Act 2: Cancelar a venda
            var cancelarHandler = new CancelarVendaCommandHandler(vendasContext, tenantProvider, currentUser);
            var cancelarCommand = new CancelarVendaCommand(vendaId, "Erro operacional na digitação");
            var cancelResult = await cancelarHandler.Handle(cancelarCommand, CancellationToken.None);

            // Assert 2: Cancelamento bem-sucedido e venda cancelada
            Assert.True(cancelResult.Sucesso);
            var venda = await vendasContext.Vendas.FirstOrDefaultAsync(v => v.Id == vendaId);
            Assert.NotNull(venda);
            // O cancelamento operacional do PDV é rastreado pela flag Cancelada; o enum EVendaStatus não possui membro "Cancelada".
            Assert.True(venda.Cancelada);

            // Deve haver 2 mensagens no outbox agora (1: VendaFaturada - já processado, 2: VendaCancelada)
            var outboxMessages = await vendasContext.OutboxMessages.ToListAsync();
            Assert.Equal(2, outboxMessages.Count);
            var cancelOutbox = outboxMessages.FirstOrDefault(o => o.EventType == "VendaCancelada");
            Assert.NotNull(cancelOutbox);
            Assert.Null(cancelOutbox.ProcessadoEm);

            // Setup Handlers for cancellation
            var handlerFinanceiroCancel = new VendaCanceladaFinanceiroHandler(financeiroContext);
            var handlerFiscalCancel = new VendaCanceladaFiscalHandler(fiscalContext);
            var handlerEstoqueCancel = new VendaCanceladaEstoqueHandler(estoqueContext);

            var mediatorCancel = new TestMediator(async (notification, ct) =>
            {
                if (notification is VendaCanceladaEventNotification vcn)
                {
                    await handlerFinanceiroCancel.Handle(vcn, ct);
                    await handlerFiscalCancel.Handle(vcn, ct);
                    await handlerEstoqueCancel.Handle(vcn, ct);
                }
            });

            var outboxJobCancel = new VendasOutboxProcessorJob(vendasContext, mediatorCancel, httpContextAccessor);

            // Act 3: Executar outbox job para processar o cancelamento
            await outboxJobCancel.Execute(null!);

            // Assert 3: Outbox processado
            var cancelOutboxPost = await vendasContext.OutboxMessages.FirstOrDefaultAsync(o => o.EventType == "VendaCancelada");
            Assert.NotNull(cancelOutboxPost);
            Assert.NotNull(cancelOutboxPost!.ProcessadoEm);

            // Assert 4: Financeiro cancelado (modelo fiel)
            var contasReceberPost = await financeiroContext.ContasAReceberAgregado.ToListAsync();
            Assert.Single(contasReceberPost);
            Assert.Equal(Epros.Shared.Domain.Enums.ESituacao.Cancelado, contasReceberPost[0].Situacao);

            // Assert 5: Fiscal cancelado
            var documentosFiscaisPost = await fiscalContext.DocumentosFiscais.ToListAsync();
            Assert.Single(documentosFiscaisPost);
            Assert.Equal("Cancelada", documentosFiscaisPost[0].Status);

            // Assert 6: Estoque estornado sem alterar custo médio (Saldo inicial era 10, estornou 2 -> 12)
            var produtoEstoquePost = await estoqueContext.Produtos.FirstOrDefaultAsync(p => p.Id == prodId);
            Assert.NotNull(produtoEstoquePost);
            Assert.Equal(12m, produtoEstoquePost.SaldoEstoque);
            Assert.Equal(15m, produtoEstoquePost.CustoMedio); // Custo médio deve permanecer inalterado

            // E deve existir um MovimentoEstoque de Entrada correspondente
            var movimentosEstoque = await estoqueContext.MovimentosEstoque.ToListAsync();
            Assert.Single(movimentosEstoque);
            Assert.Equal("Entrada", movimentosEstoque[0].Tipo);
            Assert.Equal(2m, movimentosEstoque[0].Quantidade);
            Assert.Contains($"Venda ID: {vendaId}", movimentosEstoque[0].Historico);
        }

        private ContextEstoque CreateEstoqueContext(string dbName, ITenantProvider tp, ICurrentUser cu)
        {
            var options = new DbContextOptionsBuilder<ContextEstoque>().UseInMemoryDatabase(dbName).Options;
            return new ContextEstoque(options, tp, cu);
        }

        private ContextVendas CreateVendasContext(string dbName, ITenantProvider tp, ICurrentUser cu)
        {
            var options = new DbContextOptionsBuilder<ContextVendas>().UseInMemoryDatabase(dbName).Options;
            return new ContextVendas(options, tp, cu);
        }

        private ContextFinanceiro CreateFinanceiroContext(string dbName, ITenantProvider tp, ICurrentUser cu)
        {
            var options = new DbContextOptionsBuilder<ContextFinanceiro>().UseInMemoryDatabase(dbName).Options;
            return new ContextFinanceiro(options, tp, cu);
        }

        private ContextFiscal CreateFiscalContext(string dbName, ITenantProvider tp, ICurrentUser cu)
        {
            var options = new DbContextOptionsBuilder<ContextFiscal>().UseInMemoryDatabase(dbName).Options;
            return new ContextFiscal(options, tp, cu);
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

        // Provider de emitente que sempre retorna null (nenhuma empresa emitente configurada no teste):
        // a calculadora degrada mantendo o ICMS informado, sem interromper a geração do rascunho.
        private sealed class EmitenteFiscalProviderNulo : IEmitenteFiscalProvider
        {
            public ContextoTransmissaoFiscal? ObterContexto(DocumentoFiscal documento) => null;
            public ContextoTransmissaoFiscal? ObterContextoPorDocumento(string documentoEmitente, string modelo, int ambiente) => null;
        }
    }
}
