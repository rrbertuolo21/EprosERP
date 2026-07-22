using Xunit;
using Epros.Modules.GestaoClientes.Domain.Entities;

namespace Epros.Tests
{
    public class PaisTests
    {
        [Fact]
        public void Deve_Criar_Pais_Valido()
        {
            var pais = new Pais("Argentina", "AR", "ARG", "032", "Buenos Aires", "+54", "test");

            Assert.True(pais.IsValid);
            Assert.Equal("Argentina", pais.Nome);
            Assert.Equal("AR", pais.CodigoIsoAlpha2);
            Assert.Equal("ARG", pais.CodigoIsoAlpha3);
            Assert.Equal("032", pais.CodigoNumerico);
            Assert.Equal("Buenos Aires", pais.Capital);
            Assert.Equal("+54", pais.CodigoDiscagem);
            Assert.True(pais.Ativo);
        }

        [Fact]
        public void Deve_Retornar_Erro_Se_Nome_For_Vazio()
        {
            var pais = new Pais("", "AR", "ARG", "032", "Buenos Aires", "+54", "test");

            Assert.False(pais.IsValid);
            Assert.Contains(pais.Notifications, n => n.Key == "Nome");
        }

        [Fact]
        public void Deve_Retornar_Erro_Se_Iso2_Tiver_Tamanho_Invalido()
        {
            var pais = new Pais("Argentina", "A", "ARG", "032", "Buenos Aires", "+54", "test");

            Assert.False(pais.IsValid);
            Assert.Contains(pais.Notifications, n => n.Key == "CodigoIsoAlpha2");
        }

        [Fact]
        public void Deve_Retornar_Erro_Se_Iso3_Tiver_Tamanho_Invalido()
        {
            var pais = new Pais("Argentina", "AR", "AR", "032", "Buenos Aires", "+54", "test");

            Assert.False(pais.IsValid);
            Assert.Contains(pais.Notifications, n => n.Key == "CodigoIsoAlpha3");
        }
    }
}
