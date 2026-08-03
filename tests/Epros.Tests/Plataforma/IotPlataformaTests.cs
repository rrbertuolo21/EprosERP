using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Plataforma.Iot;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests.Plataforma
{
    /// <summary>PLT · IoT — dispositivo/sensor, ingestão de leitura, detecção fora-de-faixa e condição.</summary>
    public class IotPlataformaTests
    {
        private static ContextAplicativo Novo(string db)
        {
            var options = new DbContextOptionsBuilder<ContextAplicativo>().UseInMemoryDatabase(db).Options;
            return new ContextAplicativo(options, T(), U());
        }

        private static ITenantProvider T() => new PlataformaTestFixtures.TestTenantProvider("tenant-1");
        private static ICurrentUser U() => new PlataformaTestFixtures.TestCurrentUser("user-1");

        private static async Task<Guid> MontarSensor(ContextAplicativo ctx, decimal? min, decimal? max, string ativoTipo = "Maquina", string ativoId = "m-1")
        {
            await new RegistrarDispositivoIotCommandHandler(ctx, T(), U()).Handle(
                new RegistrarDispositivoIotCommand("DEV-1", "Gateway", "gateway", "mqtt", ativoTipo, ativoId), CancellationToken.None);
            var devId = await ctx.DispositivosIot.Select(d => d.Id).FirstAsync();
            await new RegistrarSensorIotCommandHandler(ctx, T(), U()).Handle(
                new RegistrarSensorIotCommand(devId, "TEMP", "temperatura", "C", min, max, 30), CancellationToken.None);
            return await ctx.SensoresIot.Select(s => s.Id).FirstAsync();
        }

        [Fact]
        public async Task Sensor_Rejeita_Limite_Min_Maior_Que_Max()
        {
            using var ctx = Novo(nameof(Sensor_Rejeita_Limite_Min_Maior_Que_Max));
            await new RegistrarDispositivoIotCommandHandler(ctx, T(), U()).Handle(
                new RegistrarDispositivoIotCommand("D", "N", null, null, null, null), CancellationToken.None);
            var devId = await ctx.DispositivosIot.Select(d => d.Id).FirstAsync();
            var r = await new RegistrarSensorIotCommandHandler(ctx, T(), U()).Handle(
                new RegistrarSensorIotCommand(devId, "S", "g", "u", 100m, 10m, 30), CancellationToken.None);
            Assert.False(r.Sucesso);
        }

        [Fact]
        public async Task Leitura_Dentro_Da_Faixa_Nao_Gera_Evento()
        {
            using var ctx = Novo(nameof(Leitura_Dentro_Da_Faixa_Nao_Gera_Evento));
            var sensorId = await MontarSensor(ctx, 0m, 80m);
            var r = await new IngestarLeituraCommandHandler(ctx, T(), U()).Handle(
                new IngestarLeituraCommand(sensorId, 50m, null), CancellationToken.None);

            Assert.True(r.Sucesso);
            var leitura = await ctx.LeiturasSensor.FirstAsync();
            Assert.False(leitura.ForaFaixa);
            Assert.Empty(await ctx.OutboxMessages.Where(o => o.EventType.StartsWith("plt.iot")).ToListAsync());
        }

        [Fact]
        public async Task Leitura_Fora_Da_Faixa_Emite_Condicao_Para_Manutencao()
        {
            using var ctx = Novo(nameof(Leitura_Fora_Da_Faixa_Emite_Condicao_Para_Manutencao));
            var sensorId = await MontarSensor(ctx, 0m, 80m);
            var r = await new IngestarLeituraCommandHandler(ctx, T(), U()).Handle(
                new IngestarLeituraCommand(sensorId, 95m, null), CancellationToken.None);

            Assert.True(r.Sucesso);
            var leitura = await ctx.LeiturasSensor.FirstAsync();
            Assert.True(leitura.ForaFaixa);
            var eventos = await ctx.OutboxMessages.Select(o => o.EventType).ToListAsync();
            Assert.Contains(CatalogoEventosIntegracao.Plataforma.IotLeituraForaFaixa, eventos);
            Assert.Contains(CatalogoEventosIntegracao.Plataforma.IotCondicaoOperacionalDetectada, eventos);
        }

        [Fact]
        public async Task Ingestao_Atualiza_UltimaLeitura_Do_Dispositivo()
        {
            using var ctx = Novo(nameof(Ingestao_Atualiza_UltimaLeitura_Do_Dispositivo));
            var sensorId = await MontarSensor(ctx, null, null);
            var quando = new DateTime(2026, 7, 30, 10, 0, 0, DateTimeKind.Utc);
            await new IngestarLeituraCommandHandler(ctx, T(), U()).Handle(
                new IngestarLeituraCommand(sensorId, 12.5m, quando), CancellationToken.None);

            var disp = await ctx.DispositivosIot.FirstAsync();
            Assert.Equal(quando, disp.UltimaLeituraEm);
        }
    }
}
