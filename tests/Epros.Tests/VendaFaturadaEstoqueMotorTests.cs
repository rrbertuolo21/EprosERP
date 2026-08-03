using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Modules.Estoque.Application.Handlers;
using Epros.Modules.Estoque.Domain.Entities;

namespace Epros.Tests
{
    /// <summary>
    /// CORREÇÃO T3/D8 — o VendaFaturadaEstoqueHandler RESPEITA o motor único: quando o motor bloqueia a
    /// saída (saldo insuficiente, produto sem estoque negativo) NÃO grava MovimentoEstoque nem emite
    /// evento; quando aplica, grava o físico E emite o evento de saída no Outbox.
    /// </summary>
    public class VendaFaturadaEstoqueMotorTests
    {
        [Fact]
        public async Task Motor_Bloqueia_Saida_Insuficiente_Nao_Grava_Movimento_Nem_Evento()
        {
            var db = "db_estq_bloqueio_" + Guid.NewGuid().ToString("N");
            var tenantId = "tenant-estq"; var userId = "user-estq";
            var tp = new TestTenantProvider(tenantId); var cu = new TestCurrentUser(userId);
            var ctx = CreateEstoqueContext(db, tp, cu);

            // Produto SEM saldo e SEM permissão de estoque negativo (default false).
            var produto = new Produto("SKU-BLOQ", "Produto Bloqueia", 10m, tenantId, userId);
            ctx.Produtos.Add(produto);
            await ctx.SaveChangesAsync();

            var handler = new VendaFaturadaEstoqueHandler(ctx);
            var notif = new VendaFaturadaEventNotification(Guid.NewGuid(), tenantId, 50m, DateTime.UtcNow,
                new List<VendaFaturadaItemNotification> { new(produto.Id, 5m, 10m) }, userId);

            await handler.Handle(notif, CancellationToken.None);

            // Nada gravado: sem MovimentoEstoque, sem evento no Outbox, sem fato gerador.
            Assert.Empty(await ctx.MovimentosEstoque.IgnoreQueryFilters().ToListAsync());
            Assert.Empty(await ctx.OutboxMessages.IgnoreQueryFilters().ToListAsync());
            Assert.Empty(await ctx.FatosGeradoresEstoque.IgnoreQueryFilters().ToListAsync());
            var prodPos = await ctx.Produtos.FirstAsync(p => p.Id == produto.Id);
            Assert.Equal(0m, prodPos.SaldoEstoque);
        }

        [Fact]
        public async Task Motor_Aplica_Saida_Grava_Movimento_E_Emite_Evento_No_Outbox()
        {
            var db = "db_estq_ok_" + Guid.NewGuid().ToString("N");
            var tenantId = "tenant-estq2"; var userId = "user-estq2";
            var tp = new TestTenantProvider(tenantId); var cu = new TestCurrentUser(userId);
            var ctx = CreateEstoqueContext(db, tp, cu);

            var produto = new Produto("SKU-OK", "Produto OK", 10m, tenantId, userId);
            ctx.Produtos.Add(produto);
            await ctx.SaveChangesAsync();
            await EstoqueTestSeed.SemearSaldoAsync(ctx, tenantId, userId, produto.Id, 10m, 8m);

            var handler = new VendaFaturadaEstoqueHandler(ctx);
            var vendaId = Guid.NewGuid();
            var notif = new VendaFaturadaEventNotification(vendaId, tenantId, 30m, DateTime.UtcNow,
                new List<VendaFaturadaItemNotification> { new(produto.Id, 3m, 10m) }, userId);

            await handler.Handle(notif, CancellationToken.None);

            // Saldo baixou 3 -> 7; movimento físico gravado; evento emitido.
            var prodPos = await ctx.Produtos.FirstAsync(p => p.Id == produto.Id);
            Assert.Equal(7m, prodPos.SaldoEstoque);

            var movs = await ctx.MovimentosEstoque.IgnoreQueryFilters().Where(m => m.Tipo == "Saida").ToListAsync();
            Assert.Single(movs);
            Assert.Contains($"Venda ID: {vendaId}", movs[0].Historico);

            var eventos = await ctx.OutboxMessages.IgnoreQueryFilters()
                .Where(m => m.EventType == VendaFaturadaEstoqueHandler.EventoSaidaConfirmada).ToListAsync();
            Assert.Single(eventos);
            Assert.Contains(vendaId.ToString(), eventos[0].Payload);

            // Idempotência: reprocessar não duplica movimento nem evento.
            await handler.Handle(notif, CancellationToken.None);
            Assert.Single(await ctx.MovimentosEstoque.IgnoreQueryFilters().Where(m => m.Tipo == "Saida").ToListAsync());
            Assert.Single(await ctx.OutboxMessages.IgnoreQueryFilters()
                .Where(m => m.EventType == VendaFaturadaEstoqueHandler.EventoSaidaConfirmada).ToListAsync());
        }

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
    }
}
