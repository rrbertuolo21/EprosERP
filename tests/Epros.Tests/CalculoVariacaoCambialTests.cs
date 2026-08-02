using Epros.Modules.Financeiro.Domain.Services;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// FIN-CAM — motor de mark-to-market / variação cambial (identidade aritmética universal).
    /// A cotação (PTAX) é fato de mercado por data (valida-contador); estes testes exercitam a
    /// conversão e a classificação contábil ganho/perda, não fixam cotação.
    /// </summary>
    public class CalculoVariacaoCambialTests
    {
        [Fact(DisplayName = "Câmbio | conversão para base = valor ME × cotação")]
        public void Conversao_ValorVezesCotacao()
        {
            // US$ 1.000 a R$ 5,00 = R$ 5.000
            Assert.Equal(5000.00m, CalculoVariacaoCambial.ConverterParaBase(1000m, 5.00m));
        }

        [Fact(DisplayName = "Câmbio | variação = valor ME × (cotação_atual − cotação_registro)")]
        public void Variacao_DiferencaDeCotacao()
        {
            // US$ 1.000: registro 5,00 → atual 5,20 ⇒ +200,00
            Assert.Equal(200.00m, CalculoVariacaoCambial.Variacao(1000m, 5.00m, 5.20m));
            // cotação cai 5,00 → 4,80 ⇒ −200,00
            Assert.Equal(-200.00m, CalculoVariacaoCambial.Variacao(1000m, 5.00m, 4.80m));
            // sem variação
            Assert.Equal(0m, CalculoVariacaoCambial.Variacao(1000m, 5.00m, 5.00m));
        }

        [Theory(DisplayName = "Câmbio | classificação ganho/perda por natureza da posição")]
        // Ativo (recebível): moeda sobe ⇒ ganho; cai ⇒ perda
        [InlineData(200, ENaturezaPosicaoCambial.Ativo, EResultadoCambial.Ganho)]
        [InlineData(-200, ENaturezaPosicaoCambial.Ativo, EResultadoCambial.Perda)]
        // Passivo (dívida): moeda sobe ⇒ perda; cai ⇒ ganho
        [InlineData(200, ENaturezaPosicaoCambial.Passivo, EResultadoCambial.Perda)]
        [InlineData(-200, ENaturezaPosicaoCambial.Passivo, EResultadoCambial.Ganho)]
        [InlineData(0, ENaturezaPosicaoCambial.Ativo, EResultadoCambial.Nulo)]
        public void Classificacao_PorNatureza(decimal variacao, ENaturezaPosicaoCambial natureza, EResultadoCambial esperado)
        {
            Assert.Equal(esperado, CalculoVariacaoCambial.Classificar(variacao, natureza));
        }

        [Fact(DisplayName = "Câmbio | mark-to-market: variação = base_atual − base_registro (consistência)")]
        public void MarkToMarket_ConsistenteComConversao()
        {
            const decimal valorMe = 1000m, cotReg = 5.00m, cotAtu = 5.20m;
            var baseReg = CalculoVariacaoCambial.ConverterParaBase(valorMe, cotReg);
            var baseAtu = CalculoVariacaoCambial.ConverterParaBase(valorMe, cotAtu);
            Assert.Equal(baseAtu - baseReg, CalculoVariacaoCambial.Variacao(valorMe, cotReg, cotAtu));
        }
    }
}
