using System;
using Epros.Modules.Producao.Domain.Entities;
using Epros.Modules.Producao.Domain.Services;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// PRD-PLN/CTR — Testes do motor de capacidade e ATP (PD4/PD22).
    /// </summary>
    public class ProducaoCapacidadeTests
    {
        private const string TenantId = "tenant-cap-eng";
        private const string UserId = "user-cap-eng";

        [Fact(DisplayName = "CTR | Centro exige código, nome e turnos válidos")]
        public void Centro_Validacoes()
        {
            var semCodigo = new CentroTrabalho("", "Corte", 480m, 1, 5, TenantId, UserId);
            Assert.False(semCodigo.IsValid);

            var turnosZero = new CentroTrabalho("CT-1", "Corte", 480m, 0, 5, TenantId, UserId);
            Assert.False(turnosZero.IsValid);

            var diasInvalidos = new CentroTrabalho("CT-1", "Corte", 480m, 1, 8, TenantId, UserId);
            Assert.False(diasInvalidos.IsValid);

            var ok = new CentroTrabalho("CT-1", "Corte", 480m, 2, 5, TenantId, UserId);
            Assert.True(ok.IsValid);
        }

        [Fact(DisplayName = "CAP | Capacidade diária = turnos * minutos * eficiência")]
        public void Capacidade_Diaria()
        {
            var svc = new CapacidadeService();
            var centro = new CentroTrabalho("CT-2", "Costura", 480m, 2, 5, TenantId, UserId, eficienciaPercentual: 90m);
            // 480 * 2 * 0.90 = 864
            Assert.Equal(864m, svc.CapacidadeDiariaMinutos(centro));
            // período de 5 dias => 4320
            Assert.Equal(4320m, svc.CapacidadePeriodoMinutos(centro, 5));
        }

        [Fact(DisplayName = "CAP | Carga acima da capacidade sinaliza PendenteCapacidade (gargalo)")]
        public void Carga_AcimaCapacidade_Gargalo()
        {
            var svc = new CapacidadeService();
            var okAval = svc.Avaliar(1000m, 800m);
            Assert.Equal(CapacidadeService.EStatusCapacidade.Ok, okAval.Status);
            Assert.Equal(200m, okAval.Folga);
            Assert.Equal(80m, okAval.UtilizacaoPercentual);

            var gargalo = svc.Avaliar(1000m, 1200m);
            Assert.Equal(CapacidadeService.EStatusCapacidade.PendenteCapacidade, gargalo.Status);
            Assert.Equal(-200m, gargalo.Folga);
        }

        [Fact(DisplayName = "CAP | ATP disponível para prometer nunca é negativo")]
        public void Atp_NaoNegativo()
        {
            var svc = new CapacidadeService();
            Assert.Equal(300m, svc.DisponivelParaPrometer(1000m, 700m));
            Assert.Equal(0m, svc.DisponivelParaPrometer(1000m, 1500m));
        }
    }
}
