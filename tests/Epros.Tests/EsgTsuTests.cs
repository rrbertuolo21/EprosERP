using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.ESG.Application.Commands;
using Epros.Modules.ESG.Application.Handlers;
using Epros.Modules.ESG.Domain.Entities;
using Epros.Modules.ESG.Domain.Enums;
using Epros.Modules.ESG.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>ESG-TSU (Transporte Sustentavel) — submodulo construido no V1 (CD1). NF-07: fator compartilhado com GHG.</summary>
    public class EsgTsuTests
    {
        private const string Tenant = "tenant-1";
        private const string User = "user-1";

        private static ContextESG NovoContexto(string db)
        {
            var options = new DbContextOptionsBuilder<ContextESG>().UseInMemoryDatabase(db).Options;
            return new ContextESG(options, new TP(Tenant), new CU(User));
        }

        private async Task<(ContextESG ctx, Guid trechoId)> MontarTrecho(string db, DateTime dataOp)
        {
            var ctx = NovoContexto(db);
            var tp = new TP(Tenant); var cu = new CU(User);

            var reg = await new CriarRegistroTsuCommandHandler(ctx, tp, cu)
                .Handle(new CriarRegistroTsuCommand("TSU-01", "Frota Siser", Guid.NewGuid()), CancellationToken.None);
            var regId = (Guid)reg.Dados!.GetType().GetProperty("Id")!.GetValue(reg.Dados)!;

            var op = await new AdicionarOperacaoTsuCommandHandler(ctx, tp, cu)
                .Handle(new AdicionarOperacaoTsuCommand(regId, "CTE-1", dataOp, "Rodoviario"), CancellationToken.None);
            var opId = (Guid)op.Dados!.GetType().GetProperty("Id")!.GetValue(op.Dados)!;

            // 10 t × 200 km = 2000 tkm
            var tr = await new AdicionarTrechoTsuCommandHandler(ctx, tp, cu)
                .Handle(new AdicionarTrechoTsuCommand(opId, "Rodoviario", 200m, "km", 10m, "t", null, null), CancellationToken.None);
            var trId = (Guid)tr.Dados!.GetType().GetProperty("Id")!.GetValue(tr.Dados)!;
            return (ctx, trId);
        }

        [Fact(DisplayName = "TSU | tkm = massa × distancia (2000 tkm)")]
        public async Task Trecho_CalculaTkm()
        {
            var (ctx, trId) = await MontarTrecho("db_tsu_tkm", new DateTime(2026, 6, 1));
            using (ctx)
            {
                var trecho = await ctx.TrechosTsu.FirstAsync(t => t.Id == trId);
                Assert.Equal(2000m, trecho.CalcularTkm());
            }
        }

        [Fact(DisplayName = "TSU | Com fator do catalogo compartilhado: CO2e = tkm × fator; intensidade = fator")]
        public async Task Calculo_ComFator_ProduzCO2e()
        {
            var (ctx, trId) = await MontarTrecho("db_tsu_calc", new DateTime(2026, 6, 1));
            using (ctx)
            {
                // NF-07: fator por tkm no MESMO catalogo do GHG. valida-humano. Semeado no teste = 0.10 kgCO2e/tkm.
                ctx.FatoresEmissaoGee.Add(new FatorEmissaoGee(
                    GhgFatorCodigos.TransportePorTkm, "2026.1", "TESTE — base oficial pendente de homologacao",
                    0.10m, "tkm", "kgCO2e", new DateTime(2026, 1, 1), null, Tenant, User));
                await ctx.SaveChangesAsync();

                var res = await new CalcularEmissaoTsuCommandHandler(ctx, new TP(Tenant), new CU(User))
                    .Handle(new CalcularEmissaoTsuCommand(trId, "v1"), CancellationToken.None);
                Assert.True(res.Sucesso);

                var calc = await ctx.CalculosTsu.FirstAsync();
                Assert.False(calc.FatorPendente);
                Assert.Equal(2000m, calc.Tkm);
                Assert.Equal(200.0m, calc.ResultadoCO2e); // 2000 × 0.10
                Assert.Equal(0.10m, calc.Intensidade);     // CO2e/tkm
                Assert.Equal("2026.1", calc.FatorVersao);  // RN-TSU-014
            }
        }

        [Fact(DisplayName = "TSU | Sem fator no catalogo: calculo fica PENDENTE (Regra #0/NF-07), sem numero")]
        public async Task Calculo_SemFator_FicaPendente()
        {
            var (ctx, trId) = await MontarTrecho("db_tsu_pend", new DateTime(2026, 6, 1));
            using (ctx)
            {
                var res = await new CalcularEmissaoTsuCommandHandler(ctx, new TP(Tenant), new CU(User))
                    .Handle(new CalcularEmissaoTsuCommand(trId, "v1"), CancellationToken.None);
                Assert.True(res.Sucesso);

                var calc = await ctx.CalculosTsu.FirstAsync();
                Assert.True(calc.FatorPendente);
                Assert.Equal(2000m, calc.Tkm);      // trabalho de transporte apurado
                Assert.Null(calc.ResultadoCO2e);    // Regra #0: nao inventa numero
                Assert.Null(calc.Intensidade);
            }
        }

        [Fact(DisplayName = "TSU | Trecho com distancia zero e bloqueado (RN-TSU-013)")]
        public async Task Trecho_DistanciaZero_Falha()
        {
            using var ctx = NovoContexto("db_tsu_dist0");
            var tp = new TP(Tenant); var cu = new CU(User);
            var reg = await new CriarRegistroTsuCommandHandler(ctx, tp, cu).Handle(new CriarRegistroTsuCommand("TSU-9", "X", Guid.NewGuid()), CancellationToken.None);
            var regId = (Guid)reg.Dados!.GetType().GetProperty("Id")!.GetValue(reg.Dados)!;
            var op = await new AdicionarOperacaoTsuCommandHandler(ctx, tp, cu).Handle(new AdicionarOperacaoTsuCommand(regId, "CTE", new DateTime(2026, 6, 1), "Rodoviario"), CancellationToken.None);
            var opId = (Guid)op.Dados!.GetType().GetProperty("Id")!.GetValue(op.Dados)!;
            var tr = await new AdicionarTrechoTsuCommandHandler(ctx, tp, cu).Handle(new AdicionarTrechoTsuCommand(opId, "Rodoviario", 0m, "km", 10m, "t", null, null), CancellationToken.None);
            Assert.False(tr.Sucesso);
        }

        [Fact(DisplayName = "TSU | Registro workflow Rascunho -> EmAnalise -> Ativo")]
        public async Task Registro_Workflow()
        {
            using var ctx = NovoContexto("db_tsu_wf");
            var tp = new TP(Tenant); var cu = new CU(User);
            var reg = await new CriarRegistroTsuCommandHandler(ctx, tp, cu).Handle(new CriarRegistroTsuCommand("TSU-5", "Frota", Guid.NewGuid()), CancellationToken.None);
            var id = (Guid)reg.Dados!.GetType().GetProperty("Id")!.GetValue(reg.Dados)!;
            await new SubmeterRegistroTsuCommandHandler(ctx, cu).Handle(new SubmeterRegistroTsuCommand(id), CancellationToken.None);
            var apr = await new AprovarRegistroTsuCommandHandler(ctx, cu).Handle(new AprovarRegistroTsuCommand(id), CancellationToken.None);
            Assert.True(apr.Sucesso);
            var r = await ctx.RegistrosTsu.FirstAsync();
            Assert.Equal(EStatusWorkflowEsg.Ativo, r.Status);
        }

        private class TP : ITenantProvider
        {
            private readonly string _t; public TP(string t) => _t = t; public string GetTenantId() => _t;
        }
        private class CU : ICurrentUser
        {
            private readonly string _u; public CU(string u) => _u = u;
            public string? GetUserId() => _u; public string? GetUserName() => "test"; public string? GetUserEmail() => "t@e.com";
        }
    }
}
