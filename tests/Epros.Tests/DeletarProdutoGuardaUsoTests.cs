using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Handlers;
using Epros.Modules.Estoque.Domain.Entities;

namespace Epros.Tests
{
    /// <summary>
    /// GAP 2 (Estoque) — DeletarProduto ganha GUARDA DE USO (espelha a exclusão de Pessoa): bloqueia o
    /// soft-delete quando o produto tem saldo/movimento/compra; produto sem uso pode ser excluído.
    /// </summary>
    public class DeletarProdutoGuardaUsoTests
    {
        [Fact]
        public async Task Bloqueia_Delete_De_Produto_Com_Saldo_Em_Estoque()
        {
            var db = "db_delprod_saldo_" + Guid.NewGuid().ToString("N");
            var tenantId = "tenant-dp"; var userId = "user-dp";
            var ctx = CreateEstoqueContext(db, tenantId, userId);

            var produto = new Produto("SKU-DP", "Produto DP", 10m, tenantId, userId);
            ctx.Produtos.Add(produto);
            await ctx.SaveChangesAsync();
            await EstoqueTestSeed.SemearSaldoAsync(ctx, tenantId, userId, produto.Id, 5m, 8m);

            var handler = new DeletarProdutoCommandHandler(ctx, new TestCurrentUser(userId));
            var res = await handler.Handle(new DeletarProdutoCommand(produto.Id), CancellationToken.None);

            Assert.False(res.Sucesso);
            var prod = await ctx.Produtos.IgnoreQueryFilters().FirstAsync(p => p.Id == produto.Id);
            Assert.Null(prod.DeletadoEm);
        }

        [Fact]
        public async Task Bloqueia_Delete_De_Produto_Com_Movimento_De_Kardex()
        {
            var db = "db_delprod_mov_" + Guid.NewGuid().ToString("N");
            var tenantId = "tenant-dp2"; var userId = "user-dp2";
            var ctx = CreateEstoqueContext(db, tenantId, userId);

            var produto = new Produto("SKU-DP2", "Produto DP2", 10m, tenantId, userId);
            ctx.Produtos.Add(produto);
            await ctx.SaveChangesAsync();
            // Entrada e saída total: saldo volta a zero, mas há histórico de kardex (fichas).
            await EstoqueTestSeed.SemearSaldoAsync(ctx, tenantId, userId, produto.Id, 5m, 8m);
            var motor = new Epros.Modules.Estoque.Application.Services.MotorMovimentacaoEstoque(ctx, tenantId, userId);
            var fato = new FatoGeradorEstoque(null, null, null, Epros.Modules.Estoque.Domain.Enums.EOrigemFatoGeradorEstoque.Avaria, tenantId, userId, referenciaExterna: "baixa-teste");
            ctx.FatosGeradoresEstoque.Add(fato);
            await motor.AplicarSaidaAsync(Epros.Modules.Estoque.Application.Services.MotorMovimentacaoEstoque.EmpresaPadrao, produto.Id, 5m, fato.Id, null, CancellationToken.None);
            await ctx.SaveChangesAsync();

            var handler = new DeletarProdutoCommandHandler(ctx, new TestCurrentUser(userId));
            var res = await handler.Handle(new DeletarProdutoCommand(produto.Id), CancellationToken.None);

            Assert.False(res.Sucesso);
            var prod = await ctx.Produtos.IgnoreQueryFilters().FirstAsync(p => p.Id == produto.Id);
            Assert.Null(prod.DeletadoEm);
        }

        [Fact]
        public async Task Permite_Delete_De_Produto_Sem_Uso()
        {
            var db = "db_delprod_ok_" + Guid.NewGuid().ToString("N");
            var tenantId = "tenant-dp3"; var userId = "user-dp3";
            var ctx = CreateEstoqueContext(db, tenantId, userId);

            var produto = new Produto("SKU-DP3", "Produto DP3", 10m, tenantId, userId);
            ctx.Produtos.Add(produto);
            await ctx.SaveChangesAsync();

            var handler = new DeletarProdutoCommandHandler(ctx, new TestCurrentUser(userId));
            var res = await handler.Handle(new DeletarProdutoCommand(produto.Id), CancellationToken.None);

            Assert.True(res.Sucesso, string.Join("; ", res.Erros));
            var prod = await ctx.Produtos.IgnoreQueryFilters().FirstAsync(p => p.Id == produto.Id);
            Assert.NotNull(prod.DeletadoEm);
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
