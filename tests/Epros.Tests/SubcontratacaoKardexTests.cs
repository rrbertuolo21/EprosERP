using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Handlers;
using Epros.Modules.Estoque.Application.Services;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Tests
{
    /// <summary>
    /// GAP 5 (Estoque/Subcontratação) — envio e retorno movem o KARDEX pelo motor único: a remessa é SAÍDA do
    /// estoque próprio e o retorno é ENTRADA (pela quantidade que volta; perda/sucata não reentram), idempotente.
    /// D1 (bucket EmpresaPadrao). CFOP/fiscal = valida-contador.
    /// </summary>
    public class SubcontratacaoKardexTests
    {
        private const string TenantId = "tenant-sub-kdx"; private const string UserId = "user-sub-kdx";

        private ContextEstoque CreateContext(string db)
            => new ContextEstoque(new DbContextOptionsBuilder<ContextEstoque>().UseInMemoryDatabase(db).Options,
                new TestTenantProvider(TenantId), new TestCurrentUser(UserId));

        [Fact]
        public async Task Envio_Da_Saida_E_Retorno_Da_Entrada_No_Kardex()
        {
            var db = "db_sub_kdx_" + Guid.NewGuid().ToString("N");
            var tp = new TestTenantProvider(TenantId); var cu = new TestCurrentUser(UserId);

            Guid produtoId, ordemId;
            using (var ctx = CreateContext(db))
            {
                var produto = new Produto("SKU-SUB", "Produto Sub", 10m, TenantId, UserId);
                ctx.Produtos.Add(produto);
                await ctx.SaveChangesAsync();
                await EstoqueTestSeed.SemearSaldoAsync(ctx, TenantId, UserId, produto.Id, 100m, 10m);
                produtoId = produto.Id;

                var ordem = new SubOrdem(null, "OS-KDX", null, Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow.AddDays(15), null, TenantId, UserId);
                ctx.SubOrdens.Add(ordem);
                await ctx.SaveChangesAsync();
                ordemId = ordem.Id;
            }

            // Envio de 30 → saldo próprio 100 - 30 = 70.
            using (var ctx = CreateContext(db))
            {
                var r = await new RegistrarSubEnvioCommandHandler(ctx, tp, cu).Handle(
                    new RegistrarSubEnvioCommand(ordemId, DateTime.UtcNow, null,
                        new List<SubEnvioItemInput> { new(produtoId, 30m, null, null) }), CancellationToken.None);
                Assert.True(r.Sucesso, string.Join("; ", r.Erros));
            }
            using (var ctx = CreateContext(db))
            {
                var saldo = await ctx.EstoqueProdutos.IgnoreQueryFilters().FirstAsync(e => e.ProdutoId == produtoId);
                Assert.Equal(70m, saldo.QuantidadeSaldoEstoque);
            }

            // Retorno de 25 (perda 5) → saldo próprio 70 + 25 = 95; custo médio preservado (10).
            using (var ctx = CreateContext(db))
            {
                var r = await new RegistrarSubRetornoCommandHandler(ctx, tp, cu).Handle(
                    new RegistrarSubRetornoCommand(ordemId, DateTime.UtcNow, null,
                        new List<SubRetornoItemInput> { new(produtoId, 25m, 25m, 5m, null, null) }), CancellationToken.None);
                Assert.True(r.Sucesso, string.Join("; ", r.Erros));
            }
            using (var ctx = CreateContext(db))
            {
                var saldo = await ctx.EstoqueProdutos.IgnoreQueryFilters().FirstAsync(e => e.ProdutoId == produtoId);
                Assert.Equal(95m, saldo.QuantidadeSaldoEstoque);
                Assert.Equal(10m, saldo.ValorCustoMedio);

                Assert.True(await ctx.FatosGeradoresEstoque.IgnoreQueryFilters().AnyAsync(f => f.ReferenciaExterna!.StartsWith("SubEnvio ")));
                Assert.True(await ctx.FatosGeradoresEstoque.IgnoreQueryFilters().AnyAsync(f => f.ReferenciaExterna!.StartsWith("SubRetorno ")));
            }
        }

        [Fact]
        public async Task Envio_Sem_Saldo_Suficiente_Bloqueia()
        {
            var db = "db_sub_kdx_bloq_" + Guid.NewGuid().ToString("N");
            var tp = new TestTenantProvider(TenantId); var cu = new TestCurrentUser(UserId);

            Guid produtoId, ordemId;
            using (var ctx = CreateContext(db))
            {
                var produto = new Produto("SKU-SUB2", "Produto Sub2", 10m, TenantId, UserId);
                ctx.Produtos.Add(produto);
                await ctx.SaveChangesAsync();
                await EstoqueTestSeed.SemearSaldoAsync(ctx, TenantId, UserId, produto.Id, 5m, 10m); // só 5
                produtoId = produto.Id;

                var ordem = new SubOrdem(null, "OS-BLOQ", null, Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow.AddDays(15), null, TenantId, UserId);
                ctx.SubOrdens.Add(ordem);
                await ctx.SaveChangesAsync();
                ordemId = ordem.Id;
            }

            using (var ctx = CreateContext(db))
            {
                var r = await new RegistrarSubEnvioCommandHandler(ctx, tp, cu).Handle(
                    new RegistrarSubEnvioCommand(ordemId, DateTime.UtcNow, null,
                        new List<SubEnvioItemInput> { new(produtoId, 30m, null, null) }), CancellationToken.None);
                Assert.False(r.Sucesso);
            }
        }

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
