using Epros.Modules.RH.Domain.Jornada.Calculo;
using Xunit;

namespace Epros.Tests
{
    // Motor de jornada (RH-PNT). Skill departamento-pessoal/jornada-clt (RN-02, RN-03).
    public class RHMotorJornadaTests
    {
        [Fact]
        public void ValorHora_Divisor_220()
        {
            // 2200 ÷ 220 = 10,00
            Assert.Equal(10m, MotorJornada.ValorHora(2200m));
        }

        [Fact]
        public void HoraExtra_50_Porcento()
        {
            // 10 × 1,5 × 10h = 150,00
            Assert.Equal(150.00m, MotorJornada.HorasExtras(10m, 10m, MotorJornada.AdicionalHeMinimo));
        }

        [Fact]
        public void HoraExtra_100_Porcento_Domingo_Feriado()
        {
            // 10 × 2 × 10h = 200,00
            Assert.Equal(200.00m, MotorJornada.HorasExtras(10m, 10m, MotorJornada.AdicionalHeDomingoFeriado));
        }

        [Fact]
        public void HoraExtra_Nunca_Abaixo_Do_Minimo_Legal()
        {
            // adicional informado 30% é elevado ao mínimo 50%.
            Assert.Equal(150.00m, MotorJornada.HorasExtras(10m, 10m, 0.30m));
        }

        [Fact]
        public void Hora_Noturna_Reduzida_7_Conta_Como_8()
        {
            // 7 horas-relógio × 60/52,5 = 8 horas reduzidas.
            Assert.Equal(8m, MotorJornada.HorasNoturnasReduzidas(7m));
        }

        [Fact]
        public void Adicional_Noturno_20_Porcento_Sobre_Reduzidas()
        {
            // valorHora 10; 7h relógio → 8 reduzidas; adicional 20% = 8 × 10 × 0,20 = 16,00.
            Assert.Equal(16.00m, MotorJornada.AdicionalNoturno(10m, 7m));
        }

        [Fact]
        public void Pagamento_Noturno_Inclui_Hora_Reduzida_E_Adicional()
        {
            // 8 reduzidas × 10 × 1,20 = 96,00.
            Assert.Equal(96.00m, MotorJornada.PagamentoNoturno(10m, 7m));
        }
    }
}
