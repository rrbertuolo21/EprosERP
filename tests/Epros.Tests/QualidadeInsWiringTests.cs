using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Qualidade.Application.Commands;
using Epros.Modules.Qualidade.Application.Commands.Ins;
using Epros.Modules.Qualidade.Application.Handlers;
using Epros.Modules.Qualidade.Application.Handlers.Ins;
using Epros.Modules.Qualidade.Application.Queries.Ins;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Modules.Qualidade.Domain.Services.Aql;
using Epros.Modules.Qualidade.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Testes do wiring QLD-INS: ciclo de vida do plano (caracteristica -> ativar) e execucao de
    /// inspecao com tamanho de amostra calculado pelo motor AQL.
    /// </summary>
    public class QualidadeInsWiringTests
    {
        private static readonly ITenantProvider Tenant = new TP("tenant-1");
        private static readonly ICurrentUser User = new CU("user-1");

        // Um contexto novo por operacao (mesmo banco in-memory) espelha o escopo por request em producao.
        private static ContextQualidade Novo(string db)
            => new(new DbContextOptionsBuilder<ContextQualidade>().UseInMemoryDatabase(db).Options, Tenant, User);

        private static async Task<Guid> CriarPlano(string db, string codigo)
        {
            using var ctx = Novo(db);
            var h = new CriarPlanoInspecaoCommandHandler(ctx, Tenant, User);
            var r = await h.Handle(new CriarPlanoInspecaoCommand(codigo, "Plano recebimento",
                EContextoPlano.Recebimento, Guid.NewGuid(), null, null, null, DateTime.UtcNow), CancellationToken.None);
            Assert.True(r.Sucesso);
            return (await ctx.PlanosInspecao.FirstAsync(p => p.Codigo == codigo)).Id;
        }

        private static async Task AddCaracteristica(string db, Guid planoId)
        {
            using var ctx = Novo(db);
            await new AdicionarCaracteristicaPlanoCommandHandler(ctx, Tenant, User).Handle(
                new AdicionarCaracteristicaPlanoCommand(planoId, 1, "Visual", ETipoCaracteristica.Visual,
                    ETipoDadoCaracteristica.Booleano, true, null, null, null, null, null, "Sem trinca"), CancellationToken.None);
        }

        [Fact]
        public async Task Ativar_Sem_Caracteristica_Falha_Com_Caracteristica_Ativa()
        {
            const string db = nameof(Ativar_Sem_Caracteristica_Falha_Com_Caracteristica_Ativa);
            var planoId = await CriarPlano(db, "PL-INS-1");

            using (var ctx = Novo(db))
            {
                var semCarac = await new AtivarPlanoInspecaoCommandHandler(ctx, User)
                    .Handle(new AtivarPlanoInspecaoCommand(planoId), CancellationToken.None);
                Assert.False(semCarac.Sucesso);
            }

            await AddCaracteristica(db, planoId);

            using (var ctx = Novo(db))
            {
                var comCarac = await new AtivarPlanoInspecaoCommandHandler(ctx, User)
                    .Handle(new AtivarPlanoInspecaoCommand(planoId), CancellationToken.None);
                Assert.True(comCarac.Sucesso);
            }

            using (var ctx = Novo(db))
            {
                var plano = await ctx.PlanosInspecao.FirstAsync(p => p.Id == planoId);
                Assert.Equal(EStatusRegistroQualidade.Ativo, plano.Status);
            }
        }

        [Fact]
        public async Task Executar_Com_Regra_AQL_Calcula_Amostra_Pelo_Motor()
        {
            const string db = nameof(Executar_Com_Regra_AQL_Calcula_Amostra_Pelo_Motor);
            var planoId = await CriarPlano(db, "PL-INS-2");
            await AddCaracteristica(db, planoId);

            using (var ctx = Novo(db))
                await new AdicionarRegraAmostragemCommandHandler(ctx, Tenant, User).Handle(
                    new AdicionarRegraAmostragemCommand(planoId, ETipoAmostragem.AQL, null, "II", "1.0",
                        null, null, null, null, null, "Normal"), CancellationToken.None);

            using (var ctx = Novo(db))
                await new AtivarPlanoInspecaoCommandHandler(ctx, User).Handle(
                    new AtivarPlanoInspecaoCommand(planoId), CancellationToken.None);

            using (var ctx = Novo(db))
            {
                var r = await new ExecutarInspecaoCommandHandler(ctx, Tenant, User, new MotorAql())
                    .Handle(new ExecutarInspecaoCommand(planoId, EReferenciaExecucao.Recebimento,
                        "NF-123", 5000m, Guid.NewGuid(), null, null, null), CancellationToken.None);
                Assert.True(r.Sucesso);
            }

            using (var ctx = Novo(db))
            {
                var e = await ctx.ExecucoesInspecao.FirstAsync();
                Assert.Equal(200, e.TamanhoAmostraCalculado); // letra L @ 1.0 -> n=200
                Assert.Equal(EStatusExecucaoInspecao.EmColeta, e.Status);
            }
        }

        [Fact]
        public async Task Executar_Em_Plano_Nao_Ativo_Falha()
        {
            const string db = nameof(Executar_Em_Plano_Nao_Ativo_Falha);
            var planoId = await CriarPlano(db, "PL-INS-3");
            using var ctx = Novo(db);
            var r = await new ExecutarInspecaoCommandHandler(ctx, Tenant, User, new MotorAql())
                .Handle(new ExecutarInspecaoCommand(planoId, EReferenciaExecucao.Recebimento,
                    null, 100m, null, "II", 1.0m, null), CancellationToken.None);
            Assert.False(r.Sucesso);
        }

        // Abre um plano ativo + execucao e devolve (planoId, caracteristicaId, execucaoId).
        private static async Task<(Guid plano, Guid carac, Guid exec)> AbrirExecucao(string db, string codigo)
        {
            var planoId = await CriarPlano(db, codigo);
            await AddCaracteristica(db, planoId);
            using (var ctx = Novo(db))
                await new AtivarPlanoInspecaoCommandHandler(ctx, User).Handle(
                    new AtivarPlanoInspecaoCommand(planoId), CancellationToken.None);

            Guid execId;
            using (var ctx = Novo(db))
            {
                await new ExecutarInspecaoCommandHandler(ctx, Tenant, User, new MotorAql()).Handle(
                    new ExecutarInspecaoCommand(planoId, EReferenciaExecucao.Recebimento, "NF-1", 100m, Guid.NewGuid(),
                        "II", 1.0m, "Normal"), CancellationToken.None);
                execId = (await ctx.ExecucoesInspecao.FirstAsync()).Id;
            }
            Guid caracId;
            using (var ctx = Novo(db))
                caracId = (await ctx.CaracteristicasPlano.FirstAsync(c => c.PlanoId == planoId)).Id;
            return (planoId, caracId, execId);
        }

        [Fact]
        public async Task Concluir_Com_Desvio_Reprova_E_Solicita_Ncr()
        {
            const string db = nameof(Concluir_Com_Desvio_Reprova_E_Solicita_Ncr);
            var (_, carac, exec) = await AbrirExecucao(db, "PL-INS-4");

            using (var ctx = Novo(db))
            {
                var m = await new RegistrarMedicaoCommandHandler(ctx, Tenant, User).Handle(
                    new RegistrarMedicaoCommand(exec, carac, EResultadoMedicao.NaoConforme, Guid.NewGuid(),
                        null, null, "trinca", null, "fora da spec", null), CancellationToken.None);
                Assert.True(m.Sucesso);
            }

            using (var ctx = Novo(db))
            {
                var r = await new ConcluirInspecaoCommandHandler(ctx, Tenant, User).Handle(
                    new ConcluirInspecaoCommand(exec, Guid.NewGuid(), null, null, "Reprovado por trinca"), CancellationToken.None);
                Assert.True(r.Sucesso);
            }

            using (var ctx = Novo(db))
            {
                var res = await ctx.ResultadosInspecao.FirstAsync(r => r.ExecucaoId == exec);
                Assert.Equal(EResultadoInspecaoConsolidado.Reprovado, res.Resultado);
                Assert.Equal(1, res.TotalDesvios);
                Assert.True(res.GerarNcr);
                var e = await ctx.ExecucoesInspecao.FirstAsync(x => x.Id == exec);
                Assert.Equal(EStatusExecucaoInspecao.Concluida, e.Status);
                Assert.Contains(ctx.OutboxMessages, o => o.EventType == "qld.ins.ncr_solicitada");
                Assert.Contains(ctx.OutboxMessages, o => o.EventType == "qld.ins.inspecao_concluida");
            }
        }

        [Fact]
        public async Task Concluir_Sem_Desvio_Aprova_Sem_Ncr()
        {
            const string db = nameof(Concluir_Sem_Desvio_Aprova_Sem_Ncr);
            var (_, carac, exec) = await AbrirExecucao(db, "PL-INS-5");

            using (var ctx = Novo(db))
                await new RegistrarMedicaoCommandHandler(ctx, Tenant, User).Handle(
                    new RegistrarMedicaoCommand(exec, carac, EResultadoMedicao.Conforme, Guid.NewGuid(),
                        null, null, "ok", null, null, null), CancellationToken.None);

            using (var ctx = Novo(db))
            {
                var r = await new ConcluirInspecaoCommandHandler(ctx, Tenant, User).Handle(
                    new ConcluirInspecaoCommand(exec, Guid.NewGuid(), null, null, null), CancellationToken.None);
                Assert.True(r.Sucesso);
            }

            using (var ctx = Novo(db))
            {
                var res = await ctx.ResultadosInspecao.FirstAsync(r => r.ExecucaoId == exec);
                Assert.Equal(EResultadoInspecaoConsolidado.Aprovado, res.Resultado);
                Assert.False(res.GerarNcr);
                Assert.DoesNotContain(ctx.OutboxMessages, o => o.EventType == "qld.ins.ncr_solicitada");
            }
        }

        [Fact]
        public async Task Concluir_Execucao_Inexistente_Falha()
        {
            const string db = nameof(Concluir_Execucao_Inexistente_Falha);
            using var ctx = Novo(db);
            var r = await new ConcluirInspecaoCommandHandler(ctx, Tenant, User).Handle(
                new ConcluirInspecaoCommand(Guid.NewGuid(), Guid.NewGuid(), null, null, null), CancellationToken.None);
            Assert.False(r.Sucesso);
        }

        [Fact]
        public async Task CalcularPlanoAmostragem_Query_Retorna_Sucesso()
        {
            var h = new CalcularPlanoAmostragemQueryHandler(new MotorAql());
            var r = await h.Handle(new CalcularPlanoAmostragemQuery(5000, ENivelInspecao.II, 1.0m, ESeveridadeAql.Normal),
                CancellationToken.None);
            Assert.True(r.Sucesso);
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
            public string? GetUserName() => "test_user";
            public string? GetUserEmail() => "test@epros.com.br";
        }
    }
}
