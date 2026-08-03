using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Application.Handlers;
using Epros.Modules.Vendas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// VEN-LDS (NF-04/T-04): a Logística NÃO escreve saldo direto. Ao confirmar a expedição, publica
    /// o evento de saída via Outbox para o MOTOR ÚNICO do Estoque consumir (baixa idempotente).
    /// </summary>
    public class LogisticaSaidaOutboxTests
    {
        private const string TenantId = "tenant-lds-001";
        private const string UserId = "user-lds-001";

        private static ContextVendas CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ContextVendas>().UseInMemoryDatabase(dbName).Options;
            return new ContextVendas(options, new TestTenantProvider(TenantId), new TestCurrentUser(UserId));
        }

        [Fact(DisplayName = "LDS | Confirmar expedição publica evento de saída no Outbox (NF-04)")]
        public async Task ConfirmarExpedicao_PublicaEventoEstoque()
        {
            var ctx = CreateContext(nameof(ConfirmarExpedicao_PublicaEventoEstoque));
            var tp = new TestTenantProvider(TenantId);
            var cu = new TestCurrentUser(UserId);

            var criar = await new CriarExpedicaoCommandHandler(ctx, tp, cu).Handle(
                new CriarExpedicaoCommand(Guid.NewGuid(), Guid.NewGuid(), null, null, DateTime.UtcNow, null), CancellationToken.None);
            Assert.True(criar.Sucesso);
            var expedicaoId = await ctx.Expedicoes.Select(e => e.Id).FirstAsync();

            await new RegistrarEntregaItemCommandHandler(ctx, tp, cu).Handle(
                new RegistrarEntregaItemCommand(expedicaoId, Guid.NewGuid(), Guid.NewGuid(), 10m, 10m, null), CancellationToken.None);

            var confirmar = await new ConfirmarExpedicaoCommandHandler(ctx, tp, cu).Handle(
                new ConfirmarExpedicaoCommand(expedicaoId), CancellationToken.None);
            Assert.True(confirmar.Sucesso);

            // Um único evento ven.ExpedicaoConfirmada é publicado — a baixa é do motor único do Estoque.
            var evento = await ctx.OutboxMessages.SingleAsync(m => m.EventType == "ven.ExpedicaoConfirmada");
            Assert.Contains(expedicaoId.ToString(), evento.Payload);
            // A Logística não escreveu saldo de estoque aqui (delegado ao motor único).
        }

        private class TestTenantProvider : Epros.Shared.Application.Contracts.ITenantProvider
        {
            private readonly string _t;
            public TestTenantProvider(string t) => _t = t;
            public string GetTenantId() => _t;
        }

        private class TestCurrentUser : Epros.Shared.Application.Contracts.ICurrentUser
        {
            private readonly string _u;
            public TestCurrentUser(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "Test User";
            public string? GetUserEmail() => "test@epros.com";
        }
    }
}
