using Epros.Modules.Fiscal.Application.Services;
using Epros.Modules.Fiscal.Infrastructure.Services;

namespace Epros.Tests
{
    public class CalculoFiscalMotorLegadoTests
    {
        private readonly ICalculoFiscalService _service = new MotorLegadoCalculoFiscalService();

        [Fact(DisplayName = "CalculoFiscal | ICMS CST 00 tributação normal | valor = base * alíquota")]
        public void Calcular_IcmsCst00_RegimeNormal_DeveRetornarValorEsperado()
        {
            // Arrange: NF-e, Regime Normal (CRT=3), CST 00, base conhecida de R$ 100,00 e alíquota 18%.
            var request = new CalculoFiscalRequest
            {
                RegimeTributario = 3,   // RegimeNormal
                ModeloDocumento = 55,   // NF-e
                Cfop = 5102,
                UfOrigem = "SP",
                UfDestino = "SP",
                Ncm = "22030000",       // 8 dígitos (obrigatório pelo motor)
                Origem = "0",
                Unidade = "UN",
                Quantidade = 1m,
                ValorUnitario = 100.00m,
                CstIcms = "00",
                AliquotaIcms = 18m,
                CstPisCofins = "01",
                AliquotaPis = 1.65m,
                AliquotaCofins = 7.60m
            };

            // Act
            var resultado = _service.Calcular(request);

            // Assert
            Assert.True(resultado.Sucesso, string.Join(" | ", resultado.Erros));

            // Base de cálculo do ICMS CST 00 = valor do item (sem adicionais/descontos) = 100,00
            Assert.Equal(100.00m, resultado.Icms.BaseCalculo);
            Assert.Equal(18m, resultado.Icms.Aliquota);

            // Valor do ICMS = base * (alíquota / 100) = 100 * 0,18 = 18,00
            Assert.Equal(18.00m, resultado.Icms.Valor);
            Assert.Equal("00", resultado.Icms.Cst);
            Assert.Equal("0", resultado.Icms.Origem);
        }
    }
}
