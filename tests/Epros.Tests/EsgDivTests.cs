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
    /// <summary>ESG-DIV (Diversidade/Social) — submodulo construido no V1 (CD1). NF-09/T1: supressao parametrica.</summary>
    public class EsgDivTests
    {
        private const string Tenant = "tenant-1";
        private const string User = "user-1";

        private static ContextESG NovoContexto(string db)
        {
            var options = new DbContextOptionsBuilder<ContextESG>().UseInMemoryDatabase(db).Options;
            return new ContextESG(options, new TP(Tenant), new CU(User));
        }

        [Fact(DisplayName = "DIV | Programa criado; codigo duplicado e bloqueado")]
        public async Task Programa_CodigoDuplicado_Falha()
        {
            using var ctx = NovoContexto("db_div_prog_dup");
            var h = new CriarProgramaDivCommandHandler(ctx, new TP(Tenant), new CU(User));
            var r1 = await h.Handle(new CriarProgramaDivCommand("DIV-01", "Inclusao", Guid.NewGuid(), null), CancellationToken.None);
            Assert.True(r1.Sucesso);
            var r2 = await h.Handle(new CriarProgramaDivCommand("DIV-01", "Outro", Guid.NewGuid(), null), CancellationToken.None);
            Assert.False(r2.Sucesso);
        }

        [Fact(DisplayName = "DIV | Workflow Rascunho -> EmAnalise -> Ativo")]
        public async Task Programa_Workflow_SubmeterAprovar()
        {
            using var ctx = NovoContexto("db_div_prog_wf");
            var criar = new CriarProgramaDivCommandHandler(ctx, new TP(Tenant), new CU(User));
            var r = await criar.Handle(new CriarProgramaDivCommand("DIV-02", "Equidade", Guid.NewGuid(), null), CancellationToken.None);
            var id = (Guid)r.Dados!.GetType().GetProperty("Id")!.GetValue(r.Dados)!;

            await new SubmeterProgramaDivCommandHandler(ctx, new CU(User)).Handle(new SubmeterProgramaDivCommand(id), CancellationToken.None);
            var apr = await new AprovarProgramaDivCommandHandler(ctx, new CU(User)).Handle(new AprovarProgramaDivCommand(id), CancellationToken.None);
            Assert.True(apr.Sucesso);

            var prog = await ctx.ProgramasDiv.FirstAsync(p => p.Id == id);
            Assert.Equal(EStatusWorkflowEsg.Ativo, prog.Status);
        }

        [Fact(DisplayName = "DIV | Grupo abaixo do limiar parametrizado e SUPRIMIDO (NF-09), sem expor valor")]
        public async Task Medicao_GrupoPequeno_Suprimido()
        {
            using var ctx = NovoContexto("db_div_supr");
            var tp = new TP(Tenant); var cu = new CU(User);

            // Limiar de supressao homologado = 5 (parametro do tenant, nao constante inventada).
            await new DefinirParametroDivCommandHandler(ctx, tp, cu)
                .Handle(new DefinirParametroDivCommand(DivParametro.ChaveLimiteSupressaoGrupo, "5"), CancellationToken.None);

            var ind = new DivIndicador(null, "IND-1", 1, "% mulheres lideranca", "%", null, null, Tenant, User);
            ctx.IndicadoresDiv.Add(ind);
            await ctx.SaveChangesAsync();

            // Grupo com 3 individuos < 5 -> suprimido
            var res = await new RegistrarMedicaoDivCommandHandler(ctx, tp, cu)
                .Handle(new RegistrarMedicaoDivCommand(ind.Id, new DateTime(2026, 1, 1), new DateTime(2026, 3, 31), "genero=F", 3, 42m, "RH"), CancellationToken.None);
            Assert.True(res.Sucesso);

            var med = await ctx.MedicoesDiv.FirstAsync();
            Assert.True(med.Suprimido);
            Assert.Null(med.ValorAgregado); // NF-09/T1: nao expoe grupo pequeno
        }

        [Fact(DisplayName = "DIV | Grupo no/acima do limiar NAO e suprimido e expoe o valor agregado")]
        public async Task Medicao_GrupoGrande_NaoSuprimido()
        {
            using var ctx = NovoContexto("db_div_ok");
            var tp = new TP(Tenant); var cu = new CU(User);
            await new DefinirParametroDivCommandHandler(ctx, tp, cu)
                .Handle(new DefinirParametroDivCommand(DivParametro.ChaveLimiteSupressaoGrupo, "5"), CancellationToken.None);

            var ind = new DivIndicador(null, "IND-2", 1, "% PCD", "%", null, null, Tenant, User);
            ctx.IndicadoresDiv.Add(ind);
            await ctx.SaveChangesAsync();

            await new RegistrarMedicaoDivCommandHandler(ctx, tp, cu)
                .Handle(new RegistrarMedicaoDivCommand(ind.Id, new DateTime(2026, 1, 1), new DateTime(2026, 3, 31), "pcd=sim", 40, 6.5m, "RH"), CancellationToken.None);

            var med = await ctx.MedicoesDiv.FirstAsync();
            Assert.False(med.Suprimido);
            Assert.Equal(6.5m, med.ValorAgregado);
        }

        [Fact(DisplayName = "DIV | Sem parametro de supressao, medicao registra mas sinaliza pendencia de privacidade")]
        public async Task Medicao_SemParametro_Sinaliza_Pendencia()
        {
            using var ctx = NovoContexto("db_div_pend");
            var tp = new TP(Tenant); var cu = new CU(User);
            var ind = new DivIndicador(null, "IND-3", 1, "% negros", "%", null, null, Tenant, User);
            ctx.IndicadoresDiv.Add(ind);
            await ctx.SaveChangesAsync();

            var res = await new RegistrarMedicaoDivCommandHandler(ctx, tp, cu)
                .Handle(new RegistrarMedicaoDivCommand(ind.Id, new DateTime(2026, 1, 1), new DateTime(2026, 3, 31), "raca=negra", 2, 30m, "RH"), CancellationToken.None);
            Assert.True(res.Sucesso);
            Assert.Contains("NF-09", res.Mensagem);
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
