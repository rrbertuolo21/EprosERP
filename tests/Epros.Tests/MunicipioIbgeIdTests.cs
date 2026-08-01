using System;
using Xunit;
using Epros.Modules.GestaoClientes.Domain.Entities;

namespace Epros.Tests
{
    public class MunicipioIbgeIdTests
    {
        [Fact]
        public void Deve_Gerar_Guid_Id_Determinista_Com_Base_No_Codigo_Ibge()
        {
            var paisId = Guid.NewGuid();
            var subdivisaoId = Guid.NewGuid();
            long ibgeCode = 3550308; // São Paulo

            var municipio = new Municipio(paisId, subdivisaoId, "São Paulo", ibgeCode, null, null, "test");

            var expectedGuid = new Guid($"00000000-0000-0000-0000-{ibgeCode:D12}");

            Assert.True(municipio.IsValid);
            Assert.Equal(expectedGuid, municipio.Id);
        }

        [Theory]
        [InlineData(123456)] // 6 dígitos (inválido)
        [InlineData(12345678)] // 8 dígitos (inválido)
        public void Deve_Bloquear_Codigo_Ibge_Se_Nao_Tiver_Exatamente_7_Digitos(long codigoInvalido)
        {
            var paisId = Guid.NewGuid();
            var subdivisaoId = Guid.NewGuid();

            var municipio = new Municipio(paisId, subdivisaoId, "Cidade Teste", codigoInvalido, null, null, "test");

            Assert.False(municipio.IsValid);
            Assert.Contains(municipio.Notifications, n => n.Key == "CodigoIbge");
        }
    }
}
