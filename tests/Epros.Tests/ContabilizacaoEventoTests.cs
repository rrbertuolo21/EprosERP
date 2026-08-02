using System;
using System.Linq;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Application.Services;
using Epros.Modules.Financeiro.Domain.Entities;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// FIN-CGL / TEC-8 — wiring evento→ledger. Verifica que um fato de integração gera o lançamento
    /// contábil por partida dobrada (cita Negocio-acumulado/contabil), com default seguro (conta
    /// transitória, em Rascunho) quando o de-para não está configurado, e idempotência por origem.
    /// </summary>
    public class ContabilizacaoEventoTests
    {
        private const string TenantId = "tenant-cont-evt";
        private const string User = "user-cont-evt";

        private static ContextFinanceiro NovoContexto(string db)
        {
            var options = new DbContextOptionsBuilder<ContextFinanceiro>().UseInMemoryDatabase(db).Options;
            return new ContextFinanceiro(options, new TP(TenantId), new CU(User));
        }

        [Fact(DisplayName = "Wiring | sem de-para: gera lançamento BALANCEADO em Rascunho contra conta transitória")]
        public async Task SemDePara_DefaultSeguro_Rascunho()
        {
            using var ctx = NovoContexto(nameof(SemDePara_DefaultSeguro_Rascunho));
            var origem = Guid.NewGuid();

            var lanc = await ContabilizacaoEventoService.GerarLancamentoAsync(
                ctx, TenantId, User, "VendaFaturada", origem, 1000m, "Venda faturada");
            await ctx.SaveChangesAsync();

            Assert.NotNull(lanc);
            Assert.True(lanc!.Balanceado);
            Assert.Equal(1000m, lanc.TotalDebitos);
            Assert.Equal(1000m, lanc.TotalCreditos);
            Assert.Equal(EEstadoLancamentoContabil.Rascunho, lanc.Estado); // não impacta saldos até confirmar
            // conta transitória auto-provisionada
            Assert.True(await ctx.ContasContabeis.AnyAsync(c => c.CodigoConta == "9.9.9.99"));
            // ambas as linhas apontam para a mesma conta transitória (default seguro)
            var contas = lanc.Linhas.Select(l => l.ContaContabilId).Distinct().ToList();
            Assert.Single(contas);
        }

        [Fact(DisplayName = "Wiring | com de-para configurado: usa as contas do contador e o histórico da regra")]
        public async Task ComDePara_UsaContasDoContador()
        {
            using var ctx = NovoContexto(nameof(ComDePara_UsaContasDoContador));
            var debito = Guid.NewGuid();
            var credito = Guid.NewGuid();
            ctx.RegrasContabilizacao.Add(new RegraContabilizacao("VendaFaturada", debito, credito, "Receita de vendas", TenantId, User));
            await ctx.SaveChangesAsync();

            var lanc = await ContabilizacaoEventoService.GerarLancamentoAsync(
                ctx, TenantId, User, "VendaFaturada", Guid.NewGuid(), 500m, "Venda faturada");
            await ctx.SaveChangesAsync();

            Assert.NotNull(lanc);
            Assert.Equal("Receita de vendas", lanc!.Historico);
            var debitoLinha = lanc.Linhas.First(l => l.Debito > 0m);
            var creditoLinha = lanc.Linhas.First(l => l.Credito > 0m);
            Assert.Equal(debito, debitoLinha.ContaContabilId);
            Assert.Equal(credito, creditoLinha.ContaContabilId);
        }

        [Fact(DisplayName = "Wiring | idempotente por (evento + origem): segundo processamento não duplica")]
        public async Task Idempotente_PorOrigem()
        {
            using var ctx = NovoContexto(nameof(Idempotente_PorOrigem));
            var origem = Guid.NewGuid();

            var primeiro = await ContabilizacaoEventoService.GerarLancamentoAsync(ctx, TenantId, User, "CompraLancada", origem, 300m, "Compra");
            await ctx.SaveChangesAsync();
            var segundo = await ContabilizacaoEventoService.GerarLancamentoAsync(ctx, TenantId, User, "CompraLancada", origem, 300m, "Compra");
            await ctx.SaveChangesAsync();

            Assert.NotNull(primeiro);
            Assert.Null(segundo); // idempotência
            Assert.Equal(1, await ctx.LancamentosContabeis.CountAsync());
        }

        [Fact(DisplayName = "Wiring | valor não-positivo não gera lançamento")]
        public async Task ValorInvalido_NaoGera()
        {
            using var ctx = NovoContexto(nameof(ValorInvalido_NaoGera));
            var lanc = await ContabilizacaoEventoService.GerarLancamentoAsync(ctx, TenantId, User, "VendaFaturada", Guid.NewGuid(), 0m, "x");
            Assert.Null(lanc);
        }

        private class TP : ITenantProvider
        {
            private readonly string _t; public TP(string t) => _t = t; public string GetTenantId() => _t;
        }
        private class CU : ICurrentUser
        {
            private readonly string _u; public CU(string u) => _u = u;
            public string? GetUserId() => _u; public string? GetUserName() => "Test"; public string? GetUserEmail() => "t@e.com";
        }
    }
}
