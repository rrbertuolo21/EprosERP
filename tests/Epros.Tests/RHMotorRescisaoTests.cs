using System;
using Epros.Modules.RH.Domain.Folha.Calculo;
using Xunit;

namespace Epros.Tests
{
    // Motor de rescisão (RN-07) — matriz de verbas por tipo de desligamento.
    // Oráculo = tabela de verbas da skill departamento-pessoal/folha.
    public class RHMotorRescisaoTests
    {
        private static readonly TabelasFolha T = TabelasFolha.Vigente(2026);

        private static EntradaRescisao Base(TipoDesligamento tipo) => new(
            Tipo: tipo,
            SalarioMensal: 3000m,
            DataAdmissao: new DateTime(2024, 1, 10),
            DataDesligamento: new DateTime(2026, 7, 10),
            DiasTrabalhadosNoMes: 10,
            SaldoFgtsDepositado: 10000m,
            TemFeriasVencidas: true,
            RemuneracaoFeriasVencidas: 3000m);

        [Fact]
        public void Aviso_Previo_Proporcional_30_Mais_3_Por_Ano()
        {
            // 2 anos completos → 30 + 3×2 = 36 dias (Lei 12.506/2011).
            var r = MotorRescisao.Calcular(Base(TipoDesligamento.SemJustaCausaEmpregador), T);
            Assert.Equal(2, r.AnosCompletos);
            Assert.Equal(36, r.DiasAvisoPrevio);
        }

        [Fact]
        public void Sem_Justa_Causa_Tem_Todas_As_Verbas()
        {
            var r = MotorRescisao.Calcular(Base(TipoDesligamento.SemJustaCausaEmpregador), T);
            Assert.True(r.TemVerba("SALDO"));
            Assert.True(r.TemVerba("AVISO"));
            Assert.True(r.TemVerba("13PROP"));
            Assert.True(r.TemVerba("FERVENC"));
            Assert.True(r.TemVerba("FERPROP"));
            Assert.Equal(4000.00m, r.MultaFgts);        // 40% × 10.000
            Assert.True(r.TemDireitoSeguroDesemprego);
            Assert.Equal(1000.00m, r.ValorVerba("SALDO")); // 3000/30×10
        }

        [Fact]
        public void Justa_Causa_So_Saldo_E_Ferias_Vencidas()
        {
            var r = MotorRescisao.Calcular(Base(TipoDesligamento.JustaCausa), T);
            Assert.True(r.TemVerba("SALDO"));
            Assert.True(r.TemVerba("FERVENC"));
            Assert.False(r.TemVerba("AVISO"));
            Assert.False(r.TemVerba("13PROP"));
            Assert.False(r.TemVerba("FERPROP"));
            Assert.Equal(0m, r.MultaFgts);
            Assert.False(r.TemDireitoSeguroDesemprego);
        }

        [Fact]
        public void Pedido_Demissao_Sem_Aviso_Indenizado_Sem_Multa()
        {
            var r = MotorRescisao.Calcular(Base(TipoDesligamento.PedidoDemissao), T);
            Assert.False(r.TemVerba("AVISO"));
            Assert.Equal(0m, r.MultaFgts);
            Assert.False(r.TemDireitoSeguroDesemprego);
            Assert.True(r.TemVerba("13PROP"));   // 13º e férias proporcionais continuam devidos
            Assert.True(r.TemVerba("FERPROP"));
        }

        [Fact]
        public void Acordo_484A_Aviso_50_E_Multa_20()
        {
            var r = MotorRescisao.Calcular(Base(TipoDesligamento.Acordo484A), T);
            Assert.Equal(2000.00m, r.MultaFgts);   // 20% × 10.000
            Assert.True(r.TemVerba("AVISO"));
            var semJusta = MotorRescisao.Calcular(Base(TipoDesligamento.SemJustaCausaEmpregador), T);
            // Aviso do acordo é 50% do aviso sem justa causa (mesmos dias, metade do valor).
            Assert.Equal(Math.Round(semJusta.ValorVerba("AVISO") / 2m, 2), r.ValorVerba("AVISO"));
            Assert.False(r.TemDireitoSeguroDesemprego);
        }

        [Fact]
        public void Ferias_Vencidas_Tem_Um_Terco()
        {
            var r = MotorRescisao.Calcular(Base(TipoDesligamento.SemJustaCausaEmpregador), T);
            // 3000 + 1/3 = 4000.
            Assert.Equal(4000.00m, r.ValorVerba("FERVENC"));
        }

        [Fact]
        public void Liquido_Desconta_Inss_E_Irrf()
        {
            var r = MotorRescisao.Calcular(Base(TipoDesligamento.SemJustaCausaEmpregador), T);
            Assert.True(r.Inss > 0m);
            Assert.Equal(Math.Round(r.TotalProventos - r.TotalDescontos, 2), r.Liquido);
        }

        [Fact]
        public void Desligamento_Antes_Da_Admissao_Rejeita()
        {
            var e = Base(TipoDesligamento.SemJustaCausaEmpregador) with { DataDesligamento = new DateTime(2023, 1, 1) };
            Assert.Throws<ArgumentException>(() => MotorRescisao.Calcular(e, T));
        }
    }
}
