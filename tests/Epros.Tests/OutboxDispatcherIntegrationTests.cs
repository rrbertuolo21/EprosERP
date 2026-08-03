using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Outbox;
using Epros.Shared.Domain.Events;
using Epros.Infrastructure.Outbox;
using Epros.Modules.Imobiliaria.Infrastructure.Data;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Modules.Financeiro.Application.EventHandlers;

namespace Epros.Tests
{
    /// <summary>
    /// TRANSVERSAL T1 — prova a fiação Outbox -> Dispatcher -> Consumidor (a causa-raiz que "a CI não via").
    /// Publicar um evento no Outbox de um schema e rodar o OutboxDispatcher central deve disparar o
    /// consumidor real e marcar a mensagem como processada — idempotente.
    /// </summary>
    public class OutboxDispatcherIntegrationTests
    {
        [Fact]
        public async Task Dispatcher_Deve_Rotear_AluguelCobrancaGerada_Para_ContasAReceber()
        {
            var tenantId = "tenant-outbox-disp";
            var tp = new TestTenantProvider(tenantId);
            var cu = new TestCurrentUser("user-test");

            var imobContext = CreateImobiliariaContext("db_disp_imob", tp, cu);
            var finContext = CreateFinanceiroContext("db_disp_fin", tp, cu);

            var cobrancaId = Guid.NewGuid();
            var locacaoId = Guid.NewGuid();
            var payload = JsonSerializer.Serialize(new
            {
                cobrancaId,
                locacaoId,
                imovelId = Guid.NewGuid(),
                competencia = "2026-08",
                tipo = "Aluguel",
                valor = 1500m,
                vencimentoDia = 10,
                tenantId
            });

            imobContext.OutboxMessages.Add(new OutboxMessage(
                tenantId, CatalogoEventosIntegracao.Imobiliaria.AluguelCobrancaGerada, payload));
            await imobContext.SaveChangesAsync();

            var consumer = new AluguelCobrancaGeradaConsumer(finContext);
            var dispatcher = new OutboxDispatcher(
                new IOutboxConsumer[] { consumer },
                NullLogger<OutboxDispatcher>.Instance,
                fallback: null);

            // Act
            var roteadas = await dispatcher.ProcessAsync(imobContext);

            // Assert: 1 mensagem roteada e marcada processada
            Assert.Equal(1, roteadas);
            var msgs = await imobContext.OutboxMessages.IgnoreQueryFilters().ToListAsync();
            Assert.Single(msgs);
            Assert.NotNull(msgs[0].ProcessadoEm);

            // Assert: título em Contas a Receber criado com o valor da cobrança
            var contas = await finContext.ContasAReceberAgregado.IgnoreQueryFilters().ToListAsync();
            Assert.Single(contas);
            Assert.Equal(1500m, contas[0].ValorTitulo);
            Assert.Equal(Epros.Shared.Domain.Enums.ESituacao.Aberto, contas[0].Situacao);
            Assert.Equal(new DateTime(2026, 8, 10), contas[0].DataVencimento);
        }

        [Fact]
        public async Task Dispatcher_Deve_Ser_Idempotente_Em_Reexecucao()
        {
            var tenantId = "tenant-outbox-idem";
            var tp = new TestTenantProvider(tenantId);
            var cu = new TestCurrentUser("user-test");

            var imobContext = CreateImobiliariaContext("db_idem_imob", tp, cu);
            var finContext = CreateFinanceiroContext("db_idem_fin", tp, cu);

            var cobrancaId = Guid.NewGuid();
            var payload = JsonSerializer.Serialize(new
            {
                cobrancaId,
                locacaoId = Guid.NewGuid(),
                imovelId = Guid.NewGuid(),
                competencia = "2026-09",
                tipo = "Aluguel",
                valor = 999m,
                vencimentoDia = 5,
                tenantId
            });
            imobContext.OutboxMessages.Add(new OutboxMessage(
                tenantId, CatalogoEventosIntegracao.Imobiliaria.AluguelCobrancaGerada, payload));
            await imobContext.SaveChangesAsync();

            var consumer = new AluguelCobrancaGeradaConsumer(finContext);
            var dispatcher = new OutboxDispatcher(
                new IOutboxConsumer[] { consumer }, NullLogger<OutboxDispatcher>.Instance);

            await dispatcher.ProcessAsync(imobContext);
            // Segunda passada: mensagem já processada não é reprocessada; consumidor idempotente por Documento.
            var roteadasSegunda = await dispatcher.ProcessAsync(imobContext);

            Assert.Equal(0, roteadasSegunda);
            var contas = await finContext.ContasAReceberAgregado.IgnoreQueryFilters().ToListAsync();
            Assert.Single(contas);
        }

        [Fact]
        public async Task Dispatcher_Deve_Cair_No_Fallback_Para_Evento_Conhecido_Sem_Consumidor()
        {
            var tenantId = "tenant-outbox-fb";
            var tp = new TestTenantProvider(tenantId);
            var cu = new TestCurrentUser("user-test");
            var imobContext = CreateImobiliariaContext("db_fb_imob", tp, cu);

            imobContext.OutboxMessages.Add(new OutboxMessage(
                tenantId, CatalogoEventosIntegracao.Imobiliaria.LocacaoFormalizada, "{}"));
            await imobContext.SaveChangesAsync();

            var fallback = new CountingFallback();
            var dispatcher = new OutboxDispatcher(
                Array.Empty<IOutboxConsumer>(), NullLogger<OutboxDispatcher>.Instance, fallback);

            var roteadas = await dispatcher.ProcessAsync(imobContext);

            Assert.Equal(1, roteadas);
            Assert.Equal(1, fallback.Count);
            var msgs = await imobContext.OutboxMessages.IgnoreQueryFilters().ToListAsync();
            Assert.NotNull(msgs[0].ProcessadoEm);
        }

        private ContextImobiliaria CreateImobiliariaContext(string db, ITenantProvider tp, ICurrentUser cu)
            => new ContextImobiliaria(new DbContextOptionsBuilder<ContextImobiliaria>().UseInMemoryDatabase(db).Options, tp, cu);

        private ContextFinanceiro CreateFinanceiroContext(string db, ITenantProvider tp, ICurrentUser cu)
            => new ContextFinanceiro(new DbContextOptionsBuilder<ContextFinanceiro>().UseInMemoryDatabase(db).Options, tp, cu);

        private class CountingFallback : IOutboxFallbackConsumer
        {
            public int Count;
            public Task HandleUnroutedAsync(OutboxMessage message, CancellationToken cancellationToken = default)
            {
                Count++;
                return Task.CompletedTask;
            }
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
