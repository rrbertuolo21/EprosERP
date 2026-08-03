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
    /// MAN-PDT — D11/D12: avaliacao automatica de regra na leitura + validacao de leitura.
    /// </summary>
    public class ManutencaoPreditivaAvaliacaoTests
    {
        private const string TenantId = "tenant-man-pdt2";
        private const string UserId = "user-man-pdt2";

        private static ContextManutencao NovoContexto(string db)
        {
            var options = new DbContextOptionsBuilder<ContextManutencao>().UseInMemoryDatabase(db).Options;
            return new ContextManutencao(options, new TP(TenantId), new CU(UserId));
        }

        // ===================== Motor puro =====================
        [Fact(DisplayName = "Motor PDT | Limite > dispara acima do maximo")]
        public void Motor_LimiteMaior_Dispara()
        {
            Assert.True(MotorAvaliacaoPreditiva.RegraDispara(7m, ETipoRegraMonitoramento.Limite, ">", null, 5m));
            Assert.False(MotorAvaliacaoPreditiva.RegraDispara(4m, ETipoRegraMonitoramento.Limite, ">", null, 5m));
        }

        [Fact(DisplayName = "Motor PDT | Fora da faixa dispara nos dois extremos")]
        public void Motor_Fora_Dispara()
        {
            Assert.True(MotorAvaliacaoPreditiva.RegraDispara(1m, ETipoRegraMonitoramento.Limite, "fora", 2m, 8m));
            Assert.True(MotorAvaliacaoPreditiva.RegraDispara(9m, ETipoRegraMonitoramento.Limite, "fora", 2m, 8m));
            Assert.False(MotorAvaliacaoPreditiva.RegraDispara(5m, ETipoRegraMonitoramento.Limite, "fora", 2m, 8m));
        }

        [Fact(DisplayName = "Motor PDT | Tendencia nao dispara em V1 (fase 2)")]
        public void Motor_Tendencia_NaoDispara()
        {
            Assert.False(MotorAvaliacaoPreditiva.RegraDispara(999m, ETipoRegraMonitoramento.Tendencia, ">", null, 5m));
        }

        [Fact(DisplayName = "Motor PDT | Validacao rejeita unidade divergente")]
        public void Motor_Validacao_UnidadeDivergente()
        {
            var r = MotorAvaliacaoPreditiva.ValidarLeitura("bar", "mm/s", null, DateTime.UtcNow, null, false, 0.5m);
            Assert.False(r.Valida);
        }

        [Fact(DisplayName = "Motor PDT | Validacao rejeita fora de sequencia e qualidade baixa")]
        public void Motor_Validacao_SequenciaEQualidade()
        {
            var agora = DateTime.UtcNow;
            Assert.False(MotorAvaliacaoPreditiva.ValidarLeitura("mm/s", "mm/s", null, agora.AddMinutes(-10), agora, false, 0.5m).Valida);
            Assert.False(MotorAvaliacaoPreditiva.ValidarLeitura("mm/s", "mm/s", 0.2m, agora, null, false, 0.5m).Valida);
            Assert.True(MotorAvaliacaoPreditiva.ValidarLeitura("mm/s", "mm/s", 0.9m, agora, null, false, 0.5m).Valida);
        }

        // ===================== Handler auto-avaliacao =====================
        private static (Guid pontoId, Guid regraId) SeedMonitoramentoAtivo(string db, decimal limiteMax)
        {
            using var seed = NovoContexto(db);
            var equip = Guid.NewGuid();
            var m = new MonitoramentoPreditivo("PDT-AUTO", "Mon", Guid.NewGuid(), equip, null, TenantId, UserId);
            m.Submeter(UserId);
            m.Aprovar(UserId);
            var ponto = new PontoMedicao(m.Id, equip, "P1", "Vibracao", "mm/s", null, null, TenantId, UserId);
            var regra = new RegraMonitoramento(ponto.Id, ETipoRegraMonitoramento.Limite, ">", null, limiteMax, null, "Alta", "Alerta", null, null, TenantId, UserId);
            regra.Ativar(UserId);
            seed.MonitoramentosPreditivos.Add(m);
            seed.PontosMedicao.Add(ponto);
            seed.RegrasMonitoramento.Add(regra);
            seed.SaveChanges();
            return (ponto.Id, regra.Id);
        }

        [Fact(DisplayName = "MAN-PDT | Leitura acima do limite dispara alarme automatico (D11)")]
        public async Task Pdt_Leitura_DisparaAlarmeAutomatico()
        {
            var db = nameof(Pdt_Leitura_DisparaAlarmeAutomatico);
            var (pontoId, _) = SeedMonitoramentoAtivo(db, 5m);

            using var ctx = NovoContexto(db);
            var handler = new RegistrarLeituraCondicaoCommandHandler(ctx, new TP(TenantId), new CU(UserId));
            var result = await handler.Handle(new RegistrarLeituraCondicaoCommand(pontoId, DateTime.UtcNow, 8m, "mm/s", null, "sensor", null), CancellationToken.None);
            Assert.True(result.Sucesso);
            Assert.Equal(1, await ctx.AlarmesPreditivos.CountAsync());
        }

        [Fact(DisplayName = "MAN-PDT | Leitura dentro do limite nao dispara alarme")]
        public async Task Pdt_Leitura_NaoDispara()
        {
            var db = nameof(Pdt_Leitura_NaoDispara);
            var (pontoId, _) = SeedMonitoramentoAtivo(db, 5m);

            using var ctx = NovoContexto(db);
            var handler = new RegistrarLeituraCondicaoCommandHandler(ctx, new TP(TenantId), new CU(UserId));
            var result = await handler.Handle(new RegistrarLeituraCondicaoCommand(pontoId, DateTime.UtcNow, 3m, "mm/s", null, "sensor", null), CancellationToken.None);
            Assert.True(result.Sucesso);
            Assert.Equal(0, await ctx.AlarmesPreditivos.CountAsync());
        }

        [Fact(DisplayName = "MAN-PDT | Segunda leitura nao duplica alarme aberto (D13)")]
        public async Task Pdt_SegundaLeitura_NaoDuplicaAlarme()
        {
            var db = nameof(Pdt_SegundaLeitura_NaoDuplicaAlarme);
            var (pontoId, _) = SeedMonitoramentoAtivo(db, 5m);
            var t0 = DateTime.UtcNow;

            using (var ctx1 = NovoContexto(db))
            {
                var h1 = new RegistrarLeituraCondicaoCommandHandler(ctx1, new TP(TenantId), new CU(UserId));
                await h1.Handle(new RegistrarLeituraCondicaoCommand(pontoId, t0, 8m, "mm/s", null, "sensor", null), CancellationToken.None);
            }
            using var ctx2 = NovoContexto(db);
            var h2 = new RegistrarLeituraCondicaoCommandHandler(ctx2, new TP(TenantId), new CU(UserId));
            await h2.Handle(new RegistrarLeituraCondicaoCommand(pontoId, t0.AddMinutes(5), 9m, "mm/s", null, "sensor", null), CancellationToken.None);
            Assert.Equal(1, await ctx2.AlarmesPreditivos.CountAsync());
        }

        [Fact(DisplayName = "MAN-PDT | Leitura com unidade divergente e rejeitada (D12)")]
        public async Task Pdt_Leitura_UnidadeDivergente_Rejeita()
        {
            var db = nameof(Pdt_Leitura_UnidadeDivergente_Rejeita);
            var (pontoId, _) = SeedMonitoramentoAtivo(db, 5m);

            using var ctx = NovoContexto(db);
            var handler = new RegistrarLeituraCondicaoCommandHandler(ctx, new TP(TenantId), new CU(UserId));
            var result = await handler.Handle(new RegistrarLeituraCondicaoCommand(pontoId, DateTime.UtcNow, 8m, "bar", null, "sensor", null), CancellationToken.None);
            Assert.False(result.Sucesso);
            Assert.Equal(0, await ctx.LeiturasCondicao.CountAsync());
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
            public string? GetUserName() => "man-pdt2";
            public string? GetUserEmail() => "man-pdt2@epros.com.br";
        }
    }
}
