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
using Epros.Modules.Projetos.Application.Outbox;
using Epros.Modules.Projetos.Infrastructure.Data;

namespace Epros.Tests
{
    /// <summary>
    /// TRANSVERSAL T1 — prova a migração do Outbox de PROJETOS para o DISPATCHER CENTRAL:
    ///  - "ProjetoFaturado" é roteado ao consumidor REAL (que publica a notificação de faturamento);
    ///  - prj.orcamento.baseline_congelada (evento do catálogo antes órfão) cai no fallback (pendência),
    ///    em vez de morrer na fila como no job por-módulo antigo.
    /// </summary>
    public class ProjetosOutboxDispatcherTests
    {
        [Fact]
        public async Task Dispatcher_Deve_Rotear_ProjetoFaturado_Para_Consumidor_Real()
        {
            var tenantId = "tenant-prj-disp";
            var tp = new TestTenantProvider(tenantId);
            var cu = new TestCurrentUser("user-test");
            var ctx = CreateProjetosContext("db_prj_disp", tp, cu);

            var projetoId = Guid.NewGuid();
            var clienteId = Guid.NewGuid();
            var payload = JsonSerializer.Serialize(new
            {
                ProjetoId = projetoId,
                NomeProjeto = "Projeto Alfa",
                ClienteId = clienteId,
                Milestone = 50m,
                ValorFaturamento = 2500m
            });

            ctx.OutboxMessages.Add(new OutboxMessage(tenantId, "ProjetoFaturado", payload));
            await ctx.SaveChangesAsync();

            ProjetoFaturadoEventNotification? capturada = null;
            var mediator = new CapturingMediator(n =>
            {
                if (n is ProjetoFaturadoEventNotification pf) capturada = pf;
                return Task.CompletedTask;
            });
            var consumer = new ProjetoFaturadoConsumer(mediator, new TestHttpContextAccessor());
            var dispatcher = new OutboxDispatcher(
                new IOutboxConsumer[] { consumer }, NullLogger<OutboxDispatcher>.Instance);

            var roteadas = await dispatcher.ProcessAsync(ctx);

            Assert.Equal(1, roteadas);
            Assert.NotNull(capturada);
            Assert.Equal(projetoId, capturada!.ProjetoId);
            Assert.Equal(clienteId, capturada.ClienteId);
            Assert.Equal(2500m, capturada.ValorFaturamento);
            Assert.Equal(tenantId, capturada.TenantId);

            var msgs = await ctx.OutboxMessages.IgnoreQueryFilters().ToListAsync();
            Assert.Single(msgs);
            Assert.NotNull(msgs[0].ProcessadoEm);
        }

        [Fact]
        public async Task Dispatcher_Deve_Cair_No_Fallback_Para_BaselineCongelada_Sem_Consumidor()
        {
            var tenantId = "tenant-prj-fb";
            var tp = new TestTenantProvider(tenantId);
            var cu = new TestCurrentUser("user-test");
            var ctx = CreateProjetosContext("db_prj_fb", tp, cu);

            ctx.OutboxMessages.Add(new OutboxMessage(
                tenantId, CatalogoEventosIntegracao.Projetos.OrcamentoBaselineCongelada, "{}"));
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

        private ContextProjetos CreateProjetosContext(string db, ITenantProvider tp, ICurrentUser cu)
            => new ContextProjetos(new DbContextOptionsBuilder<ContextProjetos>().UseInMemoryDatabase(db).Options, tp, cu);

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
