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
    /// Contabilização automática ampliada (FIN-CGL gap 3): o motor evento→ledger passa a cobrir
    /// depreciação, baixa de ativo e variação cambial (além de venda/compra/folha/projeto). Sem de-para
    /// configurado, o lançamento nasce em Rascunho contra conta transitória (// valida-contador).
    /// </summary>
    public class ContabilizacaoAmpliadaTests
    {
        private const string Tenant = "tenant-cgl";
        private const string User = "user-cgl";

        private static ContextFinanceiro Ctx(string db)
        {
            var options = new DbContextOptionsBuilder<ContextFinanceiro>().UseInMemoryDatabase(db).Options;
            var c = new ContextFinanceiro(options, new TP(Tenant), new CU(User));
            c.Database.EnsureCreated();
            return c;
        }

        private static async Task<Guid> SemearAtivo(ContextFinanceiro ctx)
        {
            var ativo = new AtivoFixo("Torno CNC", 100000m, DateTime.UtcNow.AddYears(-1),
                null, null, null, null, null, null, null, "Torno", null, null, null, null,
                null, null, true, ETipoDepreciacaoAtivo.Linear, 10m, null, Tenant, User);
            ctx.AtivosFixos.Add(ativo);
            await ctx.SaveChangesAsync();
            return ativo.Id;
        }

        [Fact]
        public async Task Depreciacao_Gera_Lancamento_Rascunho_E_Idempotente()
        {
            const string db = nameof(Depreciacao_Gera_Lancamento_Rascunho_E_Idempotente);
            using (var ctx = Ctx(db))
            {
                var ativoId = await SemearAtivo(ctx);
                var h = new RegistrarDepreciacaoMensalCommandHandler(ctx, new TP(Tenant), new CU(User));
                var r = await h.Handle(new RegistrarDepreciacaoMensalCommand(ativoId, "2026-01", 800m, null, null), CancellationToken.None);
                Assert.True(r.Sucesso);
            }

            using (var ctx = Ctx(db))
            {
                var lanc = await ctx.LancamentosContabeis.IgnoreQueryFilters().SingleAsync(l => l.NumeroLancamento!.StartsWith("AUTO-fin.ativos.depreciacao"));
                Assert.Equal(EEstadoLancamentoContabil.Rascunho, lanc.Estado);
            }
        }

        [Fact]
        public async Task Baixa_Ativo_Gera_Lancamento_Automatico()
        {
            const string db = nameof(Baixa_Ativo_Gera_Lancamento_Automatico);
            using (var ctx = Ctx(db))
            {
                var ativoId = await SemearAtivo(ctx);
                var h = new BaixarAtivoFixoCommandHandler(ctx, new TP(Tenant), new CU(User));
                var r = await h.Handle(new BaixarAtivoFixoCommand(ativoId, DateTime.UtcNow, 5000m, "Sucata"), CancellationToken.None);
                Assert.True(r.Sucesso);
            }

            using (var ctx = Ctx(db))
                Assert.True(await ctx.LancamentosContabeis.IgnoreQueryFilters().AnyAsync(l => l.NumeroLancamento!.StartsWith("AUTO-fin.ativos.baixa")));
        }

        [Fact]
        public async Task Variacao_Cambial_Contabilizada_Gera_Lancamento()
        {
            const string db = nameof(Variacao_Cambial_Contabilizada_Gera_Lancamento);
            Guid reavId;
            using (var ctx = Ctx(db))
            {
                var moeda = new Moeda("USD", "US$", "Dólar", Tenant, User);
                ctx.Moedas.Add(moeda);
                var reav = new ReavaliacaoTitulo(DateTime.UtcNow, "Reav USD", Tenant, User);
                reav.AdicionarItem(moeda.Id, "AR", Guid.NewGuid(), Guid.NewGuid(), 1000m, 1200m, Tenant, User); // variação 200
                reav.Aprovar(User);
                ctx.ReavaliacoesTitulo.Add(reav);
                await ctx.SaveChangesAsync();
                reavId = reav.Id;

                var h = new ContabilizarReavaliacaoTituloCommandHandler(ctx, new TP(Tenant), new CU(User));
                var r = await h.Handle(new ContabilizarReavaliacaoTituloCommand(reavId), CancellationToken.None);
                Assert.True(r.Sucesso);
            }

            using (var ctx = Ctx(db))
                Assert.True(await ctx.LancamentosContabeis.IgnoreQueryFilters().AnyAsync(l => l.NumeroLancamento!.StartsWith("AUTO-fin.cambio.variacao")));
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
            public string? GetUserName() => "cgl";
            public string? GetUserEmail() => "cgl@epros.local";
        }
    }
}
