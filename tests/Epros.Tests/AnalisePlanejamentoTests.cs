using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Handlers;
using Epros.Modules.Estoque.Application.Queries;
using Epros.Modules.Estoque.Application.Services;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Tests
{
    /// <summary>
    /// EST-APE — Análise e Planejamento. Parametrização sem tocar saldo (APE-012), reprocessamento de
    /// alertas de reposição/excesso (APE-010/011 — gatilho p/ Compras) e disponível = saldo − reservado (APE-009).
    /// </summary>
    public class AnalisePlanejamentoTests
    {
        private const string TenantId = "tenant-ape";
        private const string UserId = "user-ape";

        [Fact(DisplayName = "APE | Parametrizar altera min/max/custeio sem tocar saldo (APE-012)")]
        public async Task Parametrizar_NaoTocaSaldo()
        {
            var ctx = CreateContext(nameof(Parametrizar_NaoTocaSaldo));
            var tp = new TestTenantProvider(TenantId); var cu = new TestCurrentUser(UserId);
            var produtoId = Guid.NewGuid();
            await EstoqueTestSeed.SemearSaldoAsync(ctx, TenantId, UserId, produtoId, 12m, 10m);
            var pos = await ctx.EstoqueProdutos.FirstAsync(e => e.ProdutoId == produtoId);
            var saldoAntes = pos.QuantidadeSaldoEstoque;

            var r = await new ParametrizarPosicaoEstoqueCommandHandler(ctx, tp, cu).Handle(
                new ParametrizarPosicaoEstoqueCommand(pos.Id, 5m, 20m, ETipoCusteioEstoque.PEPS, "política inicial"), CancellationToken.None);
            Assert.True(r.Sucesso);

            var pos2 = await ctx.EstoqueProdutos.AsNoTracking().FirstAsync(e => e.Id == pos.Id);
            Assert.Equal(5m, pos2.QuantidadeEstoqueMinimo);
            Assert.Equal(20m, pos2.QuantidadeEstoqueMaximo);
            Assert.Equal(ETipoCusteioEstoque.PEPS, pos2.TipoCusteioEstoque);
            Assert.Equal(saldoAntes, pos2.QuantidadeSaldoEstoque); // saldo intacto

            var eventos = await ctx.OutboxMessages.AsNoTracking().Select(o => o.EventType).ToListAsync();
            Assert.Contains(CatalogoEventosIntegracao.Estoque.AnaliseParametrosAlterados, eventos);
        }

        [Fact(DisplayName = "APE | Reprocessar gera alerta de reposição quando saldo ≤ mínimo (APE-010)")]
        public async Task Reprocessar_GeraAlertaReposicao()
        {
            var ctx = CreateContext(nameof(Reprocessar_GeraAlertaReposicao));
            var tp = new TestTenantProvider(TenantId); var cu = new TestCurrentUser(UserId);
            var produtoId = Guid.NewGuid();
            await EstoqueTestSeed.SemearSaldoAsync(ctx, TenantId, UserId, produtoId, 3m, 10m); // saldo 3
            var pos = await ctx.EstoqueProdutos.FirstAsync(e => e.ProdutoId == produtoId);
            pos.AlterarDadosAutorizados(pos.EmpresaId, pos.ProdutoId, 5m, 20m, ETipoCusteioEstoque.CustoMedio, UserId); // min 5
            await ctx.SaveChangesAsync();

            var r = await new ReprocessarAlertasEstoqueCommandHandler(ctx, tp, cu).Handle(new ReprocessarAlertasEstoqueCommand(), CancellationToken.None);
            Assert.True(r.Sucesso);

            var alertas = await ctx.AlertasEstoque.AsNoTracking().Where(a => a.ProdutoId == produtoId).ToListAsync();
            Assert.Single(alertas);
            Assert.Equal(ETipoAlertaEstoque.Reposicao, alertas[0].TipoAlerta);
            Assert.Equal(EStatusAlertaEstoque.Aberto, alertas[0].StatusAlerta);

            var eventos = await ctx.OutboxMessages.AsNoTracking().Select(o => o.EventType).ToListAsync();
            Assert.Contains(CatalogoEventosIntegracao.Estoque.AnaliseAlertaReposicao, eventos);

            // Reprocessar de novo não duplica o alerta aberto.
            await new ReprocessarAlertasEstoqueCommandHandler(ctx, tp, cu).Handle(new ReprocessarAlertasEstoqueCommand(), CancellationToken.None);
            Assert.Single(await ctx.AlertasEstoque.AsNoTracking().Where(a => a.ProdutoId == produtoId && a.StatusAlerta == EStatusAlertaEstoque.Aberto).ToListAsync());
        }

        [Fact(DisplayName = "APE | Excesso de máximo gera alerta e some ao normalizar (APE-011 + auto-resolução)")]
        public async Task Excesso_Maximo_AutoResolve()
        {
            var ctx = CreateContext(nameof(Excesso_Maximo_AutoResolve));
            var tp = new TestTenantProvider(TenantId); var cu = new TestCurrentUser(UserId);
            var produtoId = Guid.NewGuid();
            await EstoqueTestSeed.SemearSaldoAsync(ctx, TenantId, UserId, produtoId, 30m, 10m); // saldo 30
            var pos = await ctx.EstoqueProdutos.FirstAsync(e => e.ProdutoId == produtoId);
            pos.AlterarDadosAutorizados(pos.EmpresaId, pos.ProdutoId, 5m, 20m, ETipoCusteioEstoque.CustoMedio, UserId); // max 20
            await ctx.SaveChangesAsync();

            await new ReprocessarAlertasEstoqueCommandHandler(ctx, tp, cu).Handle(new ReprocessarAlertasEstoqueCommand(), CancellationToken.None);
            var abertos = await ctx.AlertasEstoque.AsNoTracking().Where(a => a.ProdutoId == produtoId && a.StatusAlerta == EStatusAlertaEstoque.Aberto).ToListAsync();
            Assert.Single(abertos);
            Assert.Equal(ETipoAlertaEstoque.ExcessoMaximo, abertos[0].TipoAlerta);

            // Eleva o máximo para 40 → condição de excesso deixa de valer → auto-resolve.
            pos.AlterarDadosAutorizados(pos.EmpresaId, pos.ProdutoId, 5m, 40m, ETipoCusteioEstoque.CustoMedio, UserId);
            await ctx.SaveChangesAsync();
            await new ReprocessarAlertasEstoqueCommandHandler(ctx, tp, cu).Handle(new ReprocessarAlertasEstoqueCommand(), CancellationToken.None);
            Assert.Empty(await ctx.AlertasEstoque.AsNoTracking().Where(a => a.ProdutoId == produtoId && a.StatusAlerta == EStatusAlertaEstoque.Aberto).ToListAsync());
        }

        [Fact(DisplayName = "APE | Consulta calcula disponível = saldo − reservado e status (APE-009)")]
        public async Task Consulta_Disponivel_E_Status()
        {
            var ctx = CreateContext(nameof(Consulta_Disponivel_E_Status));
            var produtoId = Guid.NewGuid();
            await EstoqueTestSeed.SemearSaldoAsync(ctx, TenantId, UserId, produtoId, 15m, 10m);
            var pos = await ctx.EstoqueProdutos.FirstAsync(e => e.ProdutoId == produtoId);
            pos.SomarQuantidadeEstoqueReservado(4m);
            pos.AlterarDadosAutorizados(pos.EmpresaId, pos.ProdutoId, 5m, 20m, ETipoCusteioEstoque.CustoMedio, UserId);
            await ctx.SaveChangesAsync();

            var r = await new ConsultarPosicaoEstoqueQueryHandler(ctx).Handle(new ConsultarPosicaoEstoqueQuery(produtoId), CancellationToken.None);
            Assert.True(r.Sucesso);
            var itens = (System.Collections.IEnumerable)r.Dados!.GetType().GetProperty("Itens")!.GetValue(r.Dados)!;
            var first = itens.Cast<object>().First();
            var disponivel = (decimal)first.GetType().GetProperty("QuantidadeDisponivel")!.GetValue(first)!;
            var status = first.GetType().GetProperty("StatusPlanejamento")!.GetValue(first)!;
            Assert.Equal(11m, disponivel); // 15 − 4
            Assert.Equal(EStatusPlanejamentoEstoque.Normal, status);
        }

        [Fact(DisplayName = "APE | AlertaEstoque resolver/ignorar muda status")]
        public void Alerta_Resolver_Ignorar()
        {
            var a = new AlertaEstoque(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), ETipoAlertaEstoque.Reposicao, 5m, 2m, TenantId, UserId);
            a.Resolver(UserId);
            Assert.Equal(EStatusAlertaEstoque.Resolvido, a.StatusAlerta);
            Assert.NotNull(a.ResolvidoEm);
        }

        private ContextEstoque CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ContextEstoque>().UseInMemoryDatabase("db_ape_" + dbName).Options;
            return new ContextEstoque(options, new TestTenantProvider(TenantId), new TestCurrentUser(UserId));
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
            public string? GetUserName() => "Test User";
            public string? GetUserEmail() => "test@epros.com";
        }
    }
}
