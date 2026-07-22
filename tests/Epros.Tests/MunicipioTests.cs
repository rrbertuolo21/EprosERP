using System;
using Xunit;
using Epros.Modules.GestaoClientes.Domain.Entities;

namespace Epros.Tests
{
    public class MunicipioTests
    {
        [Fact]
        public void Deve_Criar_Municipio_Valido()
        {
            var paisId = Guid.NewGuid();
            var subdivisaoId = Guid.NewGuid();
            var municipio = new Municipio(paisId, subdivisaoId, "São Paulo", 3550308, -23.5505m, -46.6333m, "test");

            Assert.True(municipio.IsValid);
            Assert.Equal("São Paulo", municipio.Nome);
            Assert.Equal(3550308, municipio.CodigoIbge);
            Assert.Equal(-23.5505m, municipio.Latitude);
            Assert.Equal(-46.6333m, municipio.Longitude);
            Assert.True(municipio.Ativo);
        }

        [Fact]
        public void Deve_Retornar_Erro_Se_Nome_For_Vazio()
        {
            var paisId = Guid.NewGuid();
            var subdivisaoId = Guid.NewGuid();
            var municipio = new Municipio(paisId, subdivisaoId, "", 3550308, null, null, "test");

            Assert.False(municipio.IsValid);
            Assert.Contains(municipio.Notifications, n => n.Key == "Nome");
        }

        [Fact]
        public void Deve_Retornar_Erro_Se_Codigo_Ibge_For_Zero_Ou_Negativo()
        {
            var paisId = Guid.NewGuid();
            var subdivisaoId = Guid.NewGuid();
            var municipio = new Municipio(paisId, subdivisaoId, "São Paulo", 0, null, null, "test");

            Assert.False(municipio.IsValid);
            Assert.Contains(municipio.Notifications, n => n.Key == "CodigoIbge");
        }
    }
}
