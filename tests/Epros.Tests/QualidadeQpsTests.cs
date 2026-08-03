using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Qualidade.Application.Commands.Qps;
using Epros.Modules.Qualidade.Application.Handlers.Qps;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Modules.Qualidade.Domain.Services.Qps;
using Epros.Modules.Qualidade.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>QLD-QPS: homologacao, bloqueio e motor de score parametrizavel do fornecedor.</summary>
    public class QualidadeQpsTests
    {
        private static readonly ITenantProvider Tenant = new TP("tenant-1");
        private static readonly ICurrentUser User = new CU("user-1");

        private static ContextQualidade Novo(string db)
            => new(new DbContextOptionsBuilder<ContextQualidade>().UseInMemoryDatabase(db).Options, Tenant, User);

        private static async Task<Guid> CriarRegistro(string db, string codigo)
        {
            using var ctx = Novo(db);
            var r = await new CriarQpsRegistroCommandHandler(ctx, Tenant, User).Handle(
                new CriarQpsRegistroCommand(codigo, Guid.NewGuid(), Guid.NewGuid(), "Fornecedor X"), CancellationToken.None);
            Assert.True(r.Sucesso);
            return (await ctx.QpsRegistros.FirstAsync(x => x.Codigo == codigo)).Id;
        }

        // ================= Motor de score =================
        [Fact]
        public void MotorScore_Media_Ponderada()
        {
            var motor = new MotorScoreFornecedor();
            var r = motor.Calcular(new[]
            {
                new IndicadorScore("PPM", 80m, 2m),
                new IndicadorScore("OTIF", 90m, 1m)
            });
            // (80*2 + 90*1) / 3 = 83.33
            Assert.Equal(83.33m, r.Score);
        }

        [Fact]
        public void MotorScore_Abaixo_Do_Limite_Sinaliza()
        {
            var motor = new MotorScoreFornecedor();
            var r = motor.Calcular(new[] { new IndicadorScore("PPM", 50m, 1m) }, limiteBloqueio: 60m);
            Assert.True(r.AbaixoLimite);
        }

        [Fact]
        public void MotorScore_Sem_Indicadores_Eh_Zero()
        {
            var r = new MotorScoreFornecedor().Calcular(Enumerable.Empty<IndicadorScore>());
            Assert.Equal(0m, r.Score);
        }

        // ================= Homologacao / bloqueio =================
        [Fact]
        public async Task Homologar_Muda_Status_E_Define_Validade()
        {
            const string db = nameof(Homologar_Muda_Status_E_Define_Validade);
            var id = await CriarRegistro(db, "QPS-1");
            using (var ctx = Novo(db))
                await new HomologarFornecedorCommandHandler(ctx, User).Handle(
                    new HomologarFornecedorCommand(id, DateTime.UtcNow.AddYears(1)), CancellationToken.None);
            using (var ctx = Novo(db))
            {
                var reg = await ctx.QpsRegistros.FirstAsync();
                Assert.Equal(EQpsStatusHomologacao.Homologado, reg.StatusHomologacao);
                Assert.NotNull(reg.DataValidadeHomologacao);
            }
        }

        [Fact]
        public async Task Bloquear_Sem_Motivo_Falha_Com_Motivo_Cria_Bloqueio()
        {
            const string db = nameof(Bloquear_Sem_Motivo_Falha_Com_Motivo_Cria_Bloqueio);
            var id = await CriarRegistro(db, "QPS-2");
            using (var ctx = Novo(db))
            {
                var semMotivo = await new BloquearFornecedorCommandHandler(ctx, Tenant, User).Handle(
                    new BloquearFornecedorCommand(id, EQpsTipoBloqueio.Manual, "", null), CancellationToken.None);
                Assert.False(semMotivo.Sucesso);
            }
            using (var ctx = Novo(db))
            {
                var r = await new BloquearFornecedorCommandHandler(ctx, Tenant, User).Handle(
                    new BloquearFornecedorCommand(id, EQpsTipoBloqueio.NcrRecorrente, "3 NCRs no trimestre", Guid.NewGuid()),
                    CancellationToken.None);
                Assert.True(r.Sucesso);
            }
            using (var ctx = Novo(db))
            {
                Assert.Equal(EQpsStatusHomologacao.Bloqueado, (await ctx.QpsRegistros.FirstAsync()).StatusHomologacao);
                Assert.Equal(1, await ctx.QpsBloqueios.CountAsync(b => b.Ativo));
            }
        }

        [Fact]
        public async Task CalcularScore_Persiste_Scorecard_E_Atualiza_Registro()
        {
            const string db = nameof(CalcularScore_Persiste_Scorecard_E_Atualiza_Registro);
            var id = await CriarRegistro(db, "QPS-3");
            using (var ctx = Novo(db))
            {
                var r = await new CalcularScoreFornecedorCommandHandler(ctx, Tenant, User, new MotorScoreFornecedor()).Handle(
                    new CalcularScoreFornecedorCommand(id, "2026-Q3", new List<IndicadorScoreDto>
                    {
                        new("PPM", 90m, 2m, "rejeicoes"),
                        new("OTIF", 80m, 1m, "recebimentos")
                    }, 60m), CancellationToken.None);
                Assert.True(r.Sucesso);
            }
            using (var ctx = Novo(db))
            {
                Assert.Equal(1, await ctx.QpsScorecards.CountAsync());
                Assert.Equal(2, await ctx.QpsIndicadores.CountAsync());
                var reg = await ctx.QpsRegistros.FirstAsync();
                Assert.NotNull(reg.ScoreAtual);
            }
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
