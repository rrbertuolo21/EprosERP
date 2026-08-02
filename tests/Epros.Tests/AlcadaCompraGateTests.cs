using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Handlers;
using Epros.Modules.Estoque.Application.Security;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Shared.Application.Contracts;

namespace Epros.Tests
{
    /// <summary>
    /// CD3 / SRC-008 — a alçada de compras é aplicada nos handlers de lançamento/faturamento: uma compra
    /// cuja origem está sob workflow de aprovação só efetiva com o pedido de aprovação APROVADO.
    /// </summary>
    public class AlcadaCompraGateTests
    {
        private const string TenantId = "tenant-alcada";
        private const string UserId = "user-alcada";

        private static ContextEstoque NovoContexto(string db)
        {
            var options = new DbContextOptionsBuilder<ContextEstoque>().UseInMemoryDatabase(db).Options;
            return new ContextEstoque(options, new FakeTenant(TenantId), new FakeUser(UserId));
        }

        private static async Task<Guid> SemearPedidoAsync(ContextEstoque ctx, bool aprovado)
        {
            var origemId = Guid.NewGuid();
            var pedido = new ComprasPedidoAprovacao(EOrigemAprovacaoCompra.Compra, origemId, 5000m, null, null, TenantId, UserId);
            if (aprovado)
            {
                // Sem níveis → FinalizarMontagem deixa o pedido Aprovado (não há alçada a exercer).
                pedido.FinalizarMontagem(UserId);
            }
            // Sem FinalizarMontagem, permanece Pendente.
            ctx.ComprasPedidosAprovacao.Add(pedido);
            await ctx.SaveChangesAsync();
            return origemId;
        }

        [Fact(DisplayName = "Alçada | Gate libera quando o pedido de aprovação está APROVADO")]
        public async Task Gate_Libera_Quando_Aprovado()
        {
            using var ctx = NovoContexto(nameof(Gate_Libera_Quando_Aprovado));
            var origemId = await SemearPedidoAsync(ctx, aprovado: true);
            var erro = await AlcadaCompraGate.GarantirAprovadaAsync(ctx, origemId, CancellationToken.None);
            Assert.Null(erro);
        }

        [Fact(DisplayName = "Alçada | Gate BLOQUEIA quando o pedido de aprovação está pendente")]
        public async Task Gate_Bloqueia_Quando_Pendente()
        {
            using var ctx = NovoContexto(nameof(Gate_Bloqueia_Quando_Pendente));
            var origemId = await SemearPedidoAsync(ctx, aprovado: false);
            var erro = await AlcadaCompraGate.GarantirAprovadaAsync(ctx, origemId, CancellationToken.None);
            Assert.NotNull(erro);
            Assert.True(erro!.Block);
        }

        [Fact(DisplayName = "Alçada | Gate BLOQUEIA quando a origem está sob alçada mas não há pedido")]
        public async Task Gate_Bloqueia_Quando_Sem_Pedido()
        {
            using var ctx = NovoContexto(nameof(Gate_Bloqueia_Quando_Sem_Pedido));
            var erro = await AlcadaCompraGate.GarantirAprovadaAsync(ctx, Guid.NewGuid(), CancellationToken.None);
            Assert.NotNull(erro);
            Assert.True(erro!.Block);
        }

        [Fact(DisplayName = "Alçada | Gate não interfere quando não há origem sob alçada (null)")]
        public async Task Gate_Sem_Origem_Nao_Interfere()
        {
            using var ctx = NovoContexto(nameof(Gate_Sem_Origem_Nao_Interfere));
            Assert.Null(await AlcadaCompraGate.GarantirAprovadaAsync(ctx, null, CancellationToken.None));
        }

        [Fact(DisplayName = "Alçada | LancarCompra é BLOQUEADO quando a origem sob alçada não está aprovada (SRC-008)")]
        public async Task LancarCompra_Bloqueado_Sem_Aprovacao()
        {
            using var ctx = NovoContexto(nameof(LancarCompra_Bloqueado_Sem_Aprovacao));
            var origemId = await SemearPedidoAsync(ctx, aprovado: false);
            var handler = new LancarCompraCommandHandler(ctx, new FakeTenant(TenantId), new FakeUser(UserId));

            var cmd = new LancarCompraCommand(
                "12345678000199", "Fornecedor X", "NF-1", new string('1', 44), 5000m, DateTime.UtcNow,
                new List<ItemCompraInput> { new("SKU-1", "Produto 1", 10, 500m, 0m, 0m) },
                AprovacaoOrigemId: origemId);

            var r = await handler.Handle(cmd, CancellationToken.None);
            Assert.False(r.Sucesso);
            Assert.True(r.Block);
            // Nada foi lançado — o bloqueio ocorre antes de tocar estoque/compra.
            Assert.Empty(await ctx.Compras.ToListAsync());
        }

        private sealed class FakeTenant : ITenantProvider
        {
            private readonly string _t; public FakeTenant(string t) => _t = t;
            public string GetTenantId() => _t;
        }

        private sealed class FakeUser : ICurrentUser
        {
            private readonly string _u; public FakeUser(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "test";
            public string? GetUserEmail() => "t@epros.com";
        }
    }
}
