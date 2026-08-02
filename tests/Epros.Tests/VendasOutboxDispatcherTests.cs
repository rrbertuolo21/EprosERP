using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Outbox;
using Epros.Shared.Domain.Events;
using Epros.Infrastructure.Outbox;
using Epros.Modules.Vendas.Application.Outbox;
using Epros.Modules.Vendas.Infrastructure.Data;

namespace Epros.Tests
{
    /// <summary>
    /// TRANSVERSAL T1 — prova a migração do Outbox de VENDAS para o DISPATCHER CENTRAL:
    ///  - VendaFaturada é roteada ao consumidor REAL (que preserva o fan-out MediatR do job legado);
    ///  - ven.ExpedicaoConfirmada (antes órfão, "morria na fila") agora cai no fallback (pendência),
    ///    SEM baixar estoque de novo (VendaFaturada já baixou — anti-dupla-contagem).
    /// </summary>
    public class VendasOutboxDispatcherTests
    {
        [Fact]
        public async Task Dispatcher_Deve_Rotear_VendaFaturada_Para_Consumidor_Real()
        {
            var tenantId = "tenant-vnd-disp";
            var tp = new TestTenantProvider(tenantId);
            var cu = new TestCurrentUser("user-test");
            var ctx = CreateVendasContext("db_vnd_disp", tp, cu);

            var vendaId = Guid.NewGuid();
            var produtoId = Guid.NewGuid();
            var payload = JsonSerializer.Serialize(new
            {
                VendaId = vendaId,
                TenantId = tenantId,
                Total = 900m,
                CriadoEm = DateTime.UtcNow,
                Itens = new[] { new { ProdutoId = produtoId, Quantidade = 3m, PrecoUnitario = 300m } }
            });

            ctx.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Vendas.VendaFaturada, payload));
            await ctx.SaveChangesAsync();

            VendaFaturadaEventNotification? capturada = null;
            var mediator = new CapturingMediator(n =>
            {
                if (n is VendaFaturadaEventNotification vf) capturada = vf;
                return Task.CompletedTask;
            });
            var consumer = new VendaFaturadaConsumer(mediator, new TestHttpContextAccessor());
            var dispatcher = new OutboxDispatcher(
                new IOutboxConsumer[] { consumer }, NullLogger<OutboxDispatcher>.Instance);

            var roteadas = await dispatcher.ProcessAsync(ctx);

            Assert.Equal(1, roteadas);
            Assert.NotNull(capturada);
            Assert.Equal(vendaId, capturada!.VendaId);
            Assert.Equal(tenantId, capturada.TenantId);
            Assert.Equal(900m, capturada.Total);
            Assert.Single(capturada.Itens);
            Assert.Equal(produtoId, capturada.Itens[0].ProdutoId);
            Assert.Equal(3m, capturada.Itens[0].Quantidade);

            var msgs = await ctx.OutboxMessages.IgnoreQueryFilters().ToListAsync();
            Assert.NotNull(msgs[0].ProcessadoEm);
        }

        [Fact]
        public async Task Dispatcher_Deve_Cair_No_Fallback_Para_ExpedicaoConfirmada_Sem_Consumidor()
        {
            var tenantId = "tenant-vnd-fb";
            var tp = new TestTenantProvider(tenantId);
            var cu = new TestCurrentUser("user-test");
            var ctx = CreateVendasContext("db_vnd_fb", tp, cu);

            ctx.OutboxMessages.Add(new OutboxMessage(
                tenantId, CatalogoEventosIntegracao.Vendas.ExpedicaoConfirmada, "{}"));
            await ctx.SaveChangesAsync();

            var fallback = new CountingFallback();
            var dispatcher = new OutboxDispatcher(
                Array.Empty<IOutboxConsumer>(), NullLogger<OutboxDispatcher>.Instance, fallback);

            var roteadas = await dispatcher.ProcessAsync(ctx);

            Assert.Equal(1, roteadas);
            Assert.Equal(1, fallback.Count);
            var msgs = await ctx.OutboxMessages.IgnoreQueryFilters().ToListAsync();
            Assert.NotNull(msgs[0].ProcessadoEm);
        }

        private ContextVendas CreateVendasContext(string db, ITenantProvider tp, ICurrentUser cu)
            => new ContextVendas(new DbContextOptionsBuilder<ContextVendas>().UseInMemoryDatabase(db).Options, tp, cu);

        private class CountingFallback : IOutboxFallbackConsumer
        {
            public int Count;
            public Task HandleUnroutedAsync(OutboxMessage message, CancellationToken cancellationToken = default)
            {
                Count++;
                return Task.CompletedTask;
            }
        }

        private class CapturingMediator : IMediator
        {
            private readonly Func<INotification, Task> _onPublish;
            public CapturingMediator(Func<INotification, Task> onPublish) => _onPublish = onPublish;

            public Task Publish(object notification, CancellationToken cancellationToken = default)
                => _onPublish((INotification)notification);
            public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
                => _onPublish(notification);

            public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest => throw new NotImplementedException();
            public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        }

        private class TestHttpContextAccessor : IHttpContextAccessor
        {
            public HttpContext? HttpContext { get; set; }
        }

        private class TestTenantProvider : ITenantProvider
        {
            private readonly string _t;
            public TestTenantProvider(string t) => _t = t;
            public string GetTenantId() => _t;
        }

        private class TestCurrentUser : ICurrentUser
        {
            private readonly string _u;
            public TestCurrentUser(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "Test User";
            public string? GetUserEmail() => "test@epros.com";
        }
    }
}
