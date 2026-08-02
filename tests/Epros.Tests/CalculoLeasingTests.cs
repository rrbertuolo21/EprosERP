using System.Linq;
using Epros.Modules.Financeiro.Domain.Services;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// FIN-GCF / arrendamento — motor de leasing IFRS-16 / CPC 06 (R2). A regra universal é
    /// "passivo = valor presente das contraprestações à taxa incremental" (cita
    /// Negocio-acumulado/financeiro/credito). Estes testes exercitam a matemática (VP, custo amortizado,
    /// depreciação linear), não fixam número legal — taxa/prazo/contas = valida-contador.
    /// </summary>
    public class CalculoLeasingTests
    {
        [Fact(DisplayName = "IFRS-16 | passivo = VP das contraprestações (1000×12 @ 1% a.p. ≈ 11.255,08)")]
        public void Passivo_ValorPresente()
        {
            var pv = CalculoLeasing.ValorPresenteContraprestacoes(1000m, 0.01m, 12);
            Assert.InRange(pv, 11255.00m, 11255.15m);
        }

        [Fact(DisplayName = "IFRS-16 | taxa zero: passivo = contraprestação × n (sem desconto)")]
        public void Passivo_TaxaZero()
        {
            var pv = CalculoLeasing.ValorPresenteContraprestacoes(1000m, 0m, 12);
            Assert.Equal(12000m, pv);
        }

        [Fact(DisplayName = "IFRS-16 | reconhecimento inicial: direito de uso = passivo + custos diretos − incentivos")]
        public void Reconhecimento_DireitoDeUso()
        {
            var rec = CalculoLeasing.ReconhecerInicial(1000m, 0.01m, 12, custosDiretosIniciais: 500m, incentivosRecebidos: 200m);
            Assert.InRange(rec.PassivoArrendamento, 11255.00m, 11255.15m);
            Assert.Equal(rec.PassivoArrendamento + 500m - 200m, rec.DireitoDeUso);
        }

        [Fact(DisplayName = "IFRS-16 | cronograma zera passivo e direito de uso; principal soma o passivo inicial")]
        public void Cronograma_Zera_E_Concilia()
        {
            var rec = CalculoLeasing.ReconhecerInicial(1000m, 0.01m, 12);
            var crono = CalculoLeasing.Cronograma(1000m, 0.01m, 12);

            Assert.Equal(12, crono.Count);
            Assert.Equal(0m, crono.Last().SaldoPassivo);            // passivo amortizado por completo
            Assert.Equal(0m, crono.Last().SaldoDireitoUso);         // direito de uso depreciado por completo
            Assert.Equal(rec.PassivoArrendamento, crono.Sum(l => l.AmortizacaoPrincipal)); // Σ principal = passivo
            Assert.Equal(rec.DireitoDeUso, crono.Sum(l => l.DepreciacaoDireitoUso));        // Σ depreciação = direito
            Assert.Equal(12000m, crono.Sum(l => l.Pagamento));      // 12 pagamentos de 1.000
        }

        [Fact(DisplayName = "IFRS-16 | juros = despesa financeira = total pago − passivo inicial")]
        public void Cronograma_Juros()
        {
            var rec = CalculoLeasing.ReconhecerInicial(1000m, 0.01m, 12);
            var crono = CalculoLeasing.Cronograma(1000m, 0.01m, 12);
            var esperado = 12000m - rec.PassivoArrendamento;
            Assert.InRange(CalculoLeasing.TotalJuros(crono), esperado - 0.10m, esperado + 0.10m);
        }

        [Fact(DisplayName = "IFRS-16 | pagamento antecipado: 1ª contraprestação não rende juros")]
        public void Cronograma_Antecipado_PrimeiraSemJuros()
        {
            var crono = CalculoLeasing.Cronograma(1000m, 0.01m, 12, pagamentoAntecipado: true);
            Assert.Equal(0m, crono.First().Juros);
            Assert.Equal(0m, crono.Last().SaldoPassivo);
        }
    }
}
