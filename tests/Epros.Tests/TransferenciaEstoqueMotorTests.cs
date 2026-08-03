using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Handlers;
using Epros.Modules.Estoque.Application.Services;
using Epros.Modules.Estoque.Domain.Entities;

namespace Epros.Tests
{
    /// <summary>
    /// GAP 1 (Estoque) — a transferência entre locais MOVE O KARDEX pelo motor único: saída na origem +
    /// entrada no destino, numa única transação, idempotente. D1: intra-empresa mantém saldo agregado e
    /// custo médio invariantes; as fichas carregam LocalOrigemId/LocalDestinoId.
    /// </summary>
    public class TransferenciaEstoqueMotorTests
    {
        [Fact]
        public async Task Transferencia_Move_Kardex_Saida_Origem_Entrada_Destino_Saldo_Agregado_Invariante()
        {
            var db = "db_transf_" + Guid.NewGuid().ToString("N");
            var tenantId = "tenant-tr"; var userId = "user-tr";
            var ctx = CreateEstoqueContext(db, tenantId, userId);

            var produto = new Produto("SKU-TR", "Produto Transf", 10m, tenantId, userId);
            ctx.Produtos.Add(produto);
            await ctx.SaveChangesAsync();
            await EstoqueTestSeed.SemearSaldoAsync(ctx, tenantId, userId, produto.Id, 20m, 8m); // 20 @ 8

            var origem = Guid.NewGuid(); var destino = Guid.NewGuid();
            var handler = new CriarTransferenciaEstoqueCommandHandler(ctx, new TestTenantProvider(tenantId), new TestCurrentUser(userId));
            var cmd = new CriarTransferenciaEstoqueCommand(
                MotorMovimentacaoEstoque.EmpresaPadrao, origem, destino, DateTime.UtcNow, null, "transf teste",
                new List<TransferenciaItemInput> { new(produto.Id, 5m, null, "L1", null) });

            var res = await handler.Handle(cmd, CancellationToken.None);
            Assert.True(res.Sucesso, string.Join("; ", res.Erros));

            // Saldo agregado (empresa+produto) invariante: 20 @ 8 (transferência intra-empresa).
            var saldo = await ctx.EstoqueProdutos.IgnoreQueryFilters().FirstAsync(e => e.ProdutoId == produto.Id);
            Assert.Equal(20m, saldo.QuantidadeSaldoEstoque);
            Assert.Equal(8m, saldo.ValorCustoMedio);

            // Kardex mexeu: uma ficha de saída (LocalOrigemId) e uma de entrada extra (LocalDestinoId).
            var saidas = await ctx.ProdutoFichaEstoqueSaidas.IgnoreQueryFilters().Where(f => f.ProdutoId == produto.Id).ToListAsync();
            Assert.Single(saidas);
            Assert.Equal(origem, saidas[0].LocalId);

            var entradas = await ctx.ProdutoFichaEstoqueEntradas.IgnoreQueryFilters().Where(f => f.ProdutoId == produto.Id).ToListAsync();
            var entradaDestino = entradas.Single(f => f.LocalId == destino);
            Assert.Equal(5m, entradaDestino.QuantidadeMovimentada);
            Assert.Equal("L1", entradaDestino.Lote);

            var fato = await ctx.FatosGeradoresEstoque.IgnoreQueryFilters().SingleAsync(f => f.ReferenciaExterna != null && f.ReferenciaExterna.StartsWith("Transferencia "));
            Assert.NotNull(fato);
        }

        [Fact]
        public async Task Transferencia_Saldo_Insuficiente_Nao_Move_Kardex()
        {
            var db = "db_transf_ins_" + Guid.NewGuid().ToString("N");
            var tenantId = "tenant-tr2"; var userId = "user-tr2";
            var ctx = CreateEstoqueContext(db, tenantId, userId);

            var produto = new Produto("SKU-TR2", "Produto Transf2", 10m, tenantId, userId);
            ctx.Produtos.Add(produto);
            await ctx.SaveChangesAsync();
            await EstoqueTestSeed.SemearSaldoAsync(ctx, tenantId, userId, produto.Id, 2m, 8m); // só 2

            var handler = new CriarTransferenciaEstoqueCommandHandler(ctx, new TestTenantProvider(tenantId), new TestCurrentUser(userId));
            var cmd = new CriarTransferenciaEstoqueCommand(
                MotorMovimentacaoEstoque.EmpresaPadrao, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, null, null,
                new List<TransferenciaItemInput> { new(produto.Id, 5m, null, null, null) });

            var res = await handler.Handle(cmd, CancellationToken.None);
            Assert.False(res.Sucesso);
            Assert.Empty(await ctx.FatosGeradoresEstoque.IgnoreQueryFilters().Where(f => f.ReferenciaExterna != null && f.ReferenciaExterna.StartsWith("Transferencia ")).ToListAsync());
        }

        private ContextEstoque CreateEstoqueContext(string db, string tenantId, string userId)
            => new ContextEstoque(new DbContextOptionsBuilder<ContextEstoque>().UseInMemoryDatabase(db).Options,
                new TestTenantProvider(tenantId), new TestCurrentUser(userId));

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
