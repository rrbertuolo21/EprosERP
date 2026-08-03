using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using MediatR;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Enums;
using Epros.Shared.Domain.Events;
using Epros.Modules.Vendas.Domain.Entities;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Application.Handlers;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Modules.Vendas.Infrastructure.Jobs;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Modules.Financeiro.Application.EventHandlers;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Modules.Estoque.Application.Handlers;
using Epros.Modules.Estoque.Domain.Entities;

namespace Epros.Tests
{
    /// <summary>
    /// TEMA T3 — prova que faturar uma venda pelo PDV no caminho FISCAL-INTERATIVO
    /// (TransmitirVendaSimplificadaNfe) LIGA os efeitos reais: credita CaixaMovimento por forma de
    /// pagamento, enfileira VendaFaturada no Outbox e — ao despachar — gera financeiro (Contas a Receber)
    /// e baixa estoque pelo MOTOR ÚNICO. Idempotente na re-transmissão.
    /// </summary>
    public class PdvFaturamentoEfeitosTests
    {
        [Fact]
        public async Task Transmitir_Nfe_Simplificada_Credita_Caixa_Por_Forma_E_Enfileira_VendaFaturada()
        {
            var dbName = "db_pdv_efeitos_" + Guid.NewGuid().ToString("N");
            var tenantId = "tenant-pdv";
            var userId = "user-pdv";
            var tp = new TestTenantProvider(tenantId);
            var cu = new TestCurrentUser(userId);

            var vendasContext = CreateVendasContext(dbName, tp, cu);

            var caixa = new Caixa(Guid.NewGuid(), Guid.NewGuid(), "operador-1", 100m, tenantId, userId, DateTime.UtcNow);
            vendasContext.Caixas.Add(caixa);

            var produtoId = Guid.NewGuid();
            var venda = new Venda(Guid.NewGuid(), Guid.NewGuid(), caixa.Id.ToString(), 90m,
                EVendaStatus.Salvar, tenantId, userId, DateTime.UtcNow, modeloFiscal: EModeloDocumento.NFe,
                vendaOrigem: EVendaOrigem.NfeSimplificada);
            venda.AdicionarItem(new VendaItem(venda.Id, produtoId, 2m, 45m, tenantId, userId));
            // Duas formas de pagamento: Dinheiro 50 (troco 10 -> líquido 40) + Cartão de Crédito 50.
            venda.AdicionarPagamento(new VendaPagamento(venda.Id, 10m, ETipoPagamento.Dinheiro, 50m,
                ETipoIntegracaoPagamentoCArtao.NaoUtiliza, null, EBandeiraCartao.NaoUtiliza, null, tenantId, userId));
            venda.AdicionarPagamento(new VendaPagamento(venda.Id, 0m, ETipoPagamento.CartaoCredito, 50m,
                ETipoIntegracaoPagamentoCArtao.PagamentoIntegradoComSistemaAutomacao, null, EBandeiraCartao.bcVisa, "AUT123", tenantId, userId));
            vendasContext.Vendas.Add(venda);
            await vendasContext.SaveChangesAsync();

            var cmd = new TransmitirVendaSimplificadaNfeCommand(venda.Id, 1, 1001, DateTime.UtcNow);

            // Act (contexto fresco = fronteira de request)
            var handlerContext = CreateVendasContext(dbName, tp, cu);
            var res = await new TransmitirVendaSimplificadaNfeCommandHandler(handlerContext, tp, cu)
                .Handle(cmd, CancellationToken.None);

            // Assert: transmissão OK e venda marcada Transmitido
            Assert.True(res.Sucesso, res.Mensagem + " | " + string.Join("; ", res.Erros));

            var assertContext = CreateVendasContext(dbName, tp, cu);
            var vendaPos = await assertContext.Vendas.FirstAsync(v => v.Id == venda.Id);
            Assert.Equal(EVendaStatus.Transmitido, vendaPos.Status);

            // Assert: CaixaMovimento "Venda" por forma (2), com valores líquidos por forma
            var movs = await assertContext.CaixaMovimentos
                .Where(m => m.CaixaId == caixa.Id && m.Tipo == "Venda").ToListAsync();
            Assert.Equal(2, movs.Count);
            Assert.Equal(90m, movs.Sum(m => m.Valor)); // 40 (dinheiro líq.) + 50 (cartão)
            Assert.Contains(movs, m => m.Valor == 40m && m.Observacao!.Contains("Dinheiro"));
            Assert.Contains(movs, m => m.Valor == 50m && m.Observacao!.Contains("CartaoCredito"));

            // Assert: VendaFaturada enfileirada no Outbox
            var outbox = await assertContext.OutboxMessages.Where(m => m.EventType == "VendaFaturada").ToListAsync();
            Assert.Single(outbox);
            Assert.Null(outbox[0].ProcessadoEm);

            // Idempotência: re-transmitir (novo contexto) não duplica movimento nem outbox
            var idemContext = CreateVendasContext(dbName, tp, cu);
            var res2 = await new TransmitirVendaSimplificadaNfeCommandHandler(idemContext, tp, cu)
                .Handle(cmd, CancellationToken.None);
            Assert.True(res2.Sucesso);
            var finalContext = CreateVendasContext(dbName, tp, cu);
            Assert.Equal(2, await finalContext.CaixaMovimentos.CountAsync(m => m.Tipo == "Venda"));
            Assert.Equal(1, await finalContext.OutboxMessages.CountAsync(m => m.EventType == "VendaFaturada"));
        }

        [Fact]
        public async Task Despachar_VendaFaturada_Do_Pdv_Gera_Financeiro_E_Baixa_Estoque_Pelo_Motor()
        {
            var dbName = "db_pdv_despacho_" + Guid.NewGuid().ToString("N");
            var tenantId = "tenant-pdv2";
            var userId = "user-pdv2";
            var tp = new TestTenantProvider(tenantId);
            var cu = new TestCurrentUser(userId);

            var vendasContext = CreateVendasContext(dbName, tp, cu);
            var financeiroContext = CreateFinanceiroContext(dbName, tp, cu);
            var estoqueContext = CreateEstoqueContext(dbName, tp, cu);

            // Estoque inicial = 10, custo médio 15
            var produtoEstoque = new Produto("SKU-PDV-1", "Produto PDV", 45m, tenantId, userId);
            var produtoId = produtoEstoque.Id;
            estoqueContext.Produtos.Add(produtoEstoque);
            await estoqueContext.SaveChangesAsync();
            await EstoqueTestSeed.SemearSaldoAsync(estoqueContext, tenantId, userId, produtoId, 10m, 15m);

            var caixa = new Caixa(Guid.NewGuid(), Guid.NewGuid(), "operador-2", 0m, tenantId, userId, DateTime.UtcNow);
            vendasContext.Caixas.Add(caixa);
            var venda = new Venda(Guid.NewGuid(), Guid.NewGuid(), caixa.Id.ToString(), 90m,
                EVendaStatus.Salvar, tenantId, userId, DateTime.UtcNow, modeloFiscal: EModeloDocumento.NFe,
                vendaOrigem: EVendaOrigem.NfeSimplificada);
            venda.AdicionarItem(new VendaItem(venda.Id, produtoId, 2m, 45m, tenantId, userId));
            venda.AdicionarPagamento(new VendaPagamento(venda.Id, 0m, ETipoPagamento.Dinheiro, 90m,
                ETipoIntegracaoPagamentoCArtao.NaoUtiliza, null, EBandeiraCartao.NaoUtiliza, null, tenantId, userId));
            vendasContext.Vendas.Add(venda);
            await vendasContext.SaveChangesAsync();

            var opContext = CreateVendasContext(dbName, tp, cu);
            var handler = new TransmitirVendaSimplificadaNfeCommandHandler(opContext, tp, cu);
            await handler.Handle(new TransmitirVendaSimplificadaNfeCommand(venda.Id, 1, 2001, DateTime.UtcNow), CancellationToken.None);

            // Despacho do Outbox de Vendas -> Financeiro + Estoque (motor único)
            var handlerFinanceiro = new VendaFaturadaEventHandler(financeiroContext);
            var handlerEstoque = new VendaFaturadaEstoqueHandler(estoqueContext);
            var mediator = new TestMediator(async (n, ct) =>
            {
                if (n is VendaFaturadaEventNotification vfn)
                {
                    await handlerFinanceiro.Handle(vfn, ct);
                    await handlerEstoque.Handle(vfn, ct);
                }
            });
            var job = new VendasOutboxProcessorJob(opContext, mediator, new TestHttpContextAccessor());
            await job.Execute(null!);

            // Financeiro: título gerado
            var contas = await financeiroContext.ContasAReceberAgregado.Include(c => c.FatoGeradorFinanceiro).ToListAsync();
            Assert.Single(contas);
            Assert.Equal(90m, contas[0].ValorTitulo);
            Assert.Equal(venda.Id, contas[0].FatoGeradorFinanceiro.VendaId);

            // Estoque: baixou 2 -> saldo 8, custo médio inalterado
            var produtoPos = await estoqueContext.Produtos.FirstAsync(p => p.Id == produtoId);
            Assert.Equal(8m, produtoPos.SaldoEstoque);
            Assert.Equal(15m, produtoPos.CustoMedio);
            var movs = await estoqueContext.MovimentosEstoque.Where(m => m.Tipo == "Saida").ToListAsync();
            Assert.Single(movs);
            Assert.Contains($"Venda ID: {venda.Id}", movs[0].Historico);
        }

        private ContextVendas CreateVendasContext(string db, ITenantProvider tp, ICurrentUser cu)
            => new ContextVendas(new DbContextOptionsBuilder<ContextVendas>().UseInMemoryDatabase(db).Options, tp, cu);
        private ContextFinanceiro CreateFinanceiroContext(string db, ITenantProvider tp, ICurrentUser cu)
            => new ContextFinanceiro(new DbContextOptionsBuilder<ContextFinanceiro>().UseInMemoryDatabase(db).Options, tp, cu);
        private ContextEstoque CreateEstoqueContext(string db, ITenantProvider tp, ICurrentUser cu)
            => new ContextEstoque(new DbContextOptionsBuilder<ContextEstoque>().UseInMemoryDatabase(db).Options, tp, cu);

        private class TestTenantProvider : ITenantProvider
        {
            private readonly string _t; public TestTenantProvider(string t) => _t = t;
            public string GetTenantId() => _t;
        }
        private class TestCurrentUser : ICurrentUser
        {
            private readonly string _u; public TestCurrentUser(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "Test";
            public string? GetUserEmail() => "t@e.com";
        }
        private class TestHttpContextAccessor : IHttpContextAccessor { public HttpContext? HttpContext { get; set; } }
        private class TestMediator : IMediator
        {
            private readonly Func<INotification, CancellationToken, Task> _h;
            public TestMediator(Func<INotification, CancellationToken, Task> h) => _h = h;
            public Task Publish(object n, CancellationToken ct = default) => _h((INotification)n, ct);
            public Task Publish<T>(T n, CancellationToken ct = default) where T : INotification => _h(n, ct);
            public Task<TR> Send<TR>(IRequest<TR> r, CancellationToken ct = default) => throw new NotImplementedException();
            public Task Send<TR>(TR r, CancellationToken ct = default) where TR : IRequest => throw new NotImplementedException();
            public Task<object?> Send(object r, CancellationToken ct = default) => throw new NotImplementedException();
            public IAsyncEnumerable<TR> CreateStream<TR>(IStreamRequest<TR> r, CancellationToken ct = default) => throw new NotImplementedException();
            public IAsyncEnumerable<object?> CreateStream(object r, CancellationToken ct = default) => throw new NotImplementedException();
        }
    }
}
