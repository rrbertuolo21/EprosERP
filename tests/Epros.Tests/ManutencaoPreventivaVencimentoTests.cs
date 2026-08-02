using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Manutencao.Application.Commands;
using Epros.Modules.Manutencao.Domain.Entities;
using Epros.Modules.Manutencao.Domain.Enums;
using Epros.Modules.Manutencao.Domain.Services;
using Epros.Modules.Manutencao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// MAN-PRV — D7/D8/D9: motor de vencimento (calendario/contador) + geracao de execucao com dedup.
    /// </summary>
    public class ManutencaoPreventivaVencimentoTests
    {
        private const string TenantId = "tenant-man-prv";
        private const string UserId = "user-man-prv";

        private static ContextManutencao NovoContexto(string db)
        {
            var options = new DbContextOptionsBuilder<ContextManutencao>().UseInMemoryDatabase(db).Options;
            return new ContextManutencao(options, new TP(TenantId), new CU(UserId));
        }

        // ===================== Motor puro =====================
        [Fact(DisplayName = "Motor PRV | Calendario vencido quando referencia passou do alvo")]
        public void Motor_Calendario_Vencido()
        {
            var baseData = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var av = MotorVencimentoPreventiva.Avaliar(
                ETipoPeriodicidade.Calendario, baseData, 30, "dias", null, null, null, null, baseData.AddDays(31));
            Assert.True(av.Vencido);
            Assert.Equal(MotorVencimentoPreventiva.MotivoVencimento.Calendario, av.Motivo);
            Assert.Equal(baseData.AddDays(30), av.DataAlvo);
            Assert.Equal(baseData.AddDays(60), av.ProximaData);
        }

        [Fact(DisplayName = "Motor PRV | Calendario nao vencido antes do alvo")]
        public void Motor_Calendario_NaoVencido()
        {
            var baseData = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var av = MotorVencimentoPreventiva.Avaliar(
                ETipoPeriodicidade.Calendario, baseData, 30, "dias", null, null, null, null, baseData.AddDays(10));
            Assert.False(av.Vencido);
        }

        [Fact(DisplayName = "Motor PRV | Tolerancia antecipa o vencimento")]
        public void Motor_Tolerancia_Antecipa()
        {
            var baseData = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            // alvo dia 30; tolerancia 5 dias -> gatilho dia 25
            var av = MotorVencimentoPreventiva.Avaliar(
                ETipoPeriodicidade.Calendario, baseData, 30, "dias", null, "5 dias", null, null, baseData.AddDays(26));
            Assert.True(av.Vencido);
        }

        [Fact(DisplayName = "Motor PRV | Contador vence ao alcancar o marco")]
        public void Motor_Contador_Vencido()
        {
            var av = MotorVencimentoPreventiva.Avaliar(
                ETipoPeriodicidade.Contador, null, null, null, null, null, 1000m, 1050m, DateTime.UtcNow);
            Assert.True(av.Vencido);
            Assert.Equal(MotorVencimentoPreventiva.MotivoVencimento.Contador, av.Motivo);
        }

        [Fact(DisplayName = "Motor PRV | Contador nao vence abaixo do marco")]
        public void Motor_Contador_NaoVencido()
        {
            var av = MotorVencimentoPreventiva.Avaliar(
                ETipoPeriodicidade.Contador, null, null, null, null, null, 1000m, 950m, DateTime.UtcNow);
            Assert.False(av.Vencido);
        }

        [Fact(DisplayName = "Motor PRV | Meses avancam corretamente")]
        public void Motor_Meses()
        {
            var d = new DateTime(2026, 1, 31, 0, 0, 0, DateTimeKind.Utc);
            var prox = MotorVencimentoPreventiva.CalcularProximaData(d, 1, "meses");
            Assert.Equal(new DateTime(2026, 2, 28, 0, 0, 0, DateTimeKind.Utc), prox);
        }

        // ===================== Handler =====================
        private static (Guid planoId, Guid perId) SeedPlanoAtivo(string db, DateTime baseData, int intervalo)
        {
            using var seed = NovoContexto(db);
            var plano = new PlanoPreventivo("PRV-01", "Plano bomba", Guid.NewGuid(), "Equipamento", Guid.NewGuid(), null, TenantId, UserId);
            var per = new PlanoPreventivoPeriodicidade(plano.Id, ETipoPeriodicidade.Calendario, baseData, intervalo, "dias", null, null, null, null, null, TenantId, UserId);
            plano.AdicionarPeriodicidade(per, UserId);
            plano.Ativar(UserId);
            seed.PlanosPreventivos.Add(plano);
            seed.SaveChanges();
            return (plano.Id, per.Id);
        }

        [Fact(DisplayName = "MAN-PRV | Vencimento gera execucao elegivel")]
        public async Task Prv_Vencimento_GeraElegivel()
        {
            var db = nameof(Prv_Vencimento_GeraElegivel);
            var baseData = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var (planoId, perId) = SeedPlanoAtivo(db, baseData, 30);

            using var ctx = NovoContexto(db);
            var handler = new AvaliarVencimentoPreventivaCommandHandler(ctx, new TP(TenantId), new CU(UserId));
            var result = await handler.Handle(new AvaliarVencimentoPreventivaCommand(planoId, baseData.AddDays(31), null), CancellationToken.None);
            Assert.True(result.Sucesso);

            var execs = await ctx.PlanoPreventivoExecucoes.Where(e => e.PeriodicidadeId == perId).ToListAsync();
            Assert.Single(execs);
            Assert.Equal(EStatusExecucaoPreventiva.Elegivel, execs[0].Status);
        }

        [Fact(DisplayName = "MAN-PRV | Segunda avaliacao na mesma janela nao duplica (D8)")]
        public async Task Prv_Vencimento_DedupJanela()
        {
            var db = nameof(Prv_Vencimento_DedupJanela);
            var baseData = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var (planoId, perId) = SeedPlanoAtivo(db, baseData, 30);

            using (var ctx1 = NovoContexto(db))
            {
                var h1 = new AvaliarVencimentoPreventivaCommandHandler(ctx1, new TP(TenantId), new CU(UserId));
                await h1.Handle(new AvaliarVencimentoPreventivaCommand(planoId, baseData.AddDays(31), null), CancellationToken.None);
            }
            // avanca a base para o proximo alvo (dia 60); reavaliar no MESMO dia 31 nao deve criar nova (alvo mudou p/ 60, nao vencido)
            using var ctx2 = NovoContexto(db);
            var h2 = new AvaliarVencimentoPreventivaCommandHandler(ctx2, new TP(TenantId), new CU(UserId));
            await h2.Handle(new AvaliarVencimentoPreventivaCommand(planoId, baseData.AddDays(31), null), CancellationToken.None);

            var execs = await ctx2.PlanoPreventivoExecucoes.Where(e => e.PeriodicidadeId == perId).ToListAsync();
            Assert.Single(execs); // continua uma so
        }

        [Fact(DisplayName = "MAN-PRV | Plano nao vencido nao gera execucao")]
        public async Task Prv_NaoVencido_NaoGera()
        {
            var db = nameof(Prv_NaoVencido_NaoGera);
            var baseData = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var (planoId, perId) = SeedPlanoAtivo(db, baseData, 30);

            using var ctx = NovoContexto(db);
            var handler = new AvaliarVencimentoPreventivaCommandHandler(ctx, new TP(TenantId), new CU(UserId));
            await handler.Handle(new AvaliarVencimentoPreventivaCommand(planoId, baseData.AddDays(5), null), CancellationToken.None);

            var execs = await ctx.PlanoPreventivoExecucoes.Where(e => e.PeriodicidadeId == perId).ToListAsync();
            Assert.Empty(execs);
        }

        private class TP : ITenantProvider
        {
            private readonly string _t;
            public TP(string t) => _t = t;
            public string GetTenantId() => _t;
        }

        private class CU : ICurrentUser
        {
            private readonly string _u;
            public CU(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "man-prv";
            public string? GetUserEmail() => "man-prv@epros.com.br";
        }
    }
}
