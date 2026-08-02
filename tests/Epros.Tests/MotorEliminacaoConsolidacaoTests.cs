using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Application.Commands;
using Epros.Modules.Financeiro.Application.Handlers;
using Epros.Modules.Financeiro.Domain.Entities;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Motor de eliminação intercompany (FIN-CON gap 2): aplica eliminações registradas ao balancete
    /// consolidado provisório (estrutura + agregação). Conta de-para = // valida-contador.
    /// </summary>
    public class MotorEliminacaoConsolidacaoTests
    {
        private const string Tenant = "tenant-con";
        private const string User = "user-con";

        private static ContextFinanceiro Ctx(string db)
        {
            var options = new DbContextOptionsBuilder<ContextFinanceiro>().UseInMemoryDatabase(db).Options;
            var c = new ContextFinanceiro(options, new TP(Tenant), new CU(User));
            c.Database.EnsureCreated();
            return c;
        }

        // Semeia grupo + balancete provisório + duas eliminações no MESMO contexto (evita o artefato de
        // token de concorrência do provider InMemory ao inserir filhos em pai carregado noutro contexto —
        // em Postgres o fluxo load+add-children+save é normal).
        private static async Task<Guid> Semear(ContextFinanceiro ctx, string periodo, decimal elim1, decimal elim2)
        {
            var grupo = new GrupoConsolidacao("G1", "Grupo 1", null, Tenant, User);
            ctx.GruposConsolidacao.Add(grupo);

            var bal = new BalanceteConsolidado(grupo.Id, periodo, Tenant, User);
            bal.AdicionarLinha(null, "1.1", "Caixa", 0m, 1000m, 0m, 1000m, Tenant, User);
            bal.AdicionarLinha(null, "2.1", "Fornecedores", 0m, 0m, 1000m, -1000m, Tenant, User);
            bal.RecomputarTotais(User);
            ctx.BalancetesConsolidados.Add(bal);

            ctx.EliminacoesIntercompany.Add(new EliminacaoIntercompany(grupo.Id, periodo, Guid.NewGuid(), Guid.NewGuid(), elim1, "Mútuo AR/AP", Tenant, User));
            ctx.EliminacoesIntercompany.Add(new EliminacaoIntercompany(grupo.Id, periodo, Guid.NewGuid(), Guid.NewGuid(), elim2, "Vendas intercompany", Tenant, User));
            await ctx.SaveChangesAsync();
            return grupo.Id;
        }

        private static AplicarEliminacoesConsolidadoCommandHandler Handler(ContextFinanceiro ctx)
            => new(ctx, new TP(Tenant), new CU(User));

        [Fact]
        public async Task Aplicar_Eliminacoes_Gera_Linhas_E_Marca_Aplicadas()
        {
            const string db = nameof(Aplicar_Eliminacoes_Gera_Linhas_E_Marca_Aplicadas);
            const string periodo = "2026-01";
            using (var ctx = Ctx(db))
            {
                var grupo = await Semear(ctx, periodo, 300m, 200m);
                var r = await Handler(ctx).Handle(new AplicarEliminacoesConsolidadoCommand(grupo, periodo), CancellationToken.None);
                Assert.True(r.Sucesso);
            }

            using (var ctx = Ctx(db))
            {
                var bal = await ctx.BalancetesConsolidados.Include(b => b.Linhas).FirstAsync();
                Assert.True(bal.EliminacoesAplicadas);
                Assert.Equal(500m, bal.TotalEliminacoes);
                // 2 linhas originais + 2 de eliminação; balanceado (débito=crédito preservado).
                Assert.Equal(4, bal.Linhas.Count);
                Assert.Equal(bal.TotalDebito, bal.TotalCredito);
                Assert.All(await ctx.EliminacoesIntercompany.ToListAsync(), e => Assert.True(e.Aplicada));
            }
        }

        [Fact]
        public async Task Aplicar_Duas_Vezes_E_Idempotente()
        {
            const string db = nameof(Aplicar_Duas_Vezes_E_Idempotente);
            const string periodo = "2026-01";
            using (var ctx = Ctx(db))
            {
                var grupo = await Semear(ctx, periodo, 300m, 200m);
                await Handler(ctx).Handle(new AplicarEliminacoesConsolidadoCommand(grupo, periodo), CancellationToken.None);
                var r = await Handler(ctx).Handle(new AplicarEliminacoesConsolidadoCommand(grupo, periodo), CancellationToken.None);
                Assert.True(r.Sucesso); // nada pendente — não duplica.
            }

            using (var ctx = Ctx(db))
            {
                var bal = await ctx.BalancetesConsolidados.Include(b => b.Linhas).FirstAsync();
                Assert.Equal(500m, bal.TotalEliminacoes); // não dobrou.
                Assert.Equal(4, bal.Linhas.Count);
            }
        }

        [Fact]
        public async Task Sem_Balancete_Provisorio_Falha()
        {
            const string db = nameof(Sem_Balancete_Provisorio_Falha);
            using var ctx = Ctx(db);
            var r = await Handler(ctx).Handle(new AplicarEliminacoesConsolidadoCommand(Guid.NewGuid(), "2026-01"), CancellationToken.None);
            Assert.False(r.Sucesso);
        }

        private sealed class TP : ITenantProvider
        {
            private readonly string _t; public TP(string t) => _t = t;
            public string GetTenantId() => _t;
        }
        private sealed class CU : ICurrentUser
        {
            private readonly string _u; public CU(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "con";
            public string? GetUserEmail() => "con@epros.local";
        }
    }
}
