using System;
using System.Linq;
using Xunit;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Domain.ValueObjects;
using EnderecoVo = Epros.Modules.GestaoClientes.Domain.ValueObjects.Endereco;

namespace Epros.Tests
{
    public class EmpresaTests
    {
        private const string TenantId = "tenant-test";
        private const string Usuario = "user-test";

        [Fact]
        public void Deve_Criar_Empresa_Valida_Corretamente()
        {
            // Arrange
            var endereco = new EnderecoVo(
                "Av Paulista",
                "1000",
                "Apto 12",
                "Bela Vista",
                "01310-100",
                "São Paulo",
                "SP"
            );

            // Act
            var empresa = new Empresa(
                "Empresa Teste S.A.",
                "Teste Fantasia",
                "12345678000195", // Valid CNPJ
                "123456789",
                "987654",
                "555666",
                "6201501",
                RegimeTributario.SimplesNacional,
                RegimeApuracao.Cumulativo,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                "https://api.vendas.com",
                "mp-token-123",
                "https://logo.com/logo.png",
                endereco,
                TenantId,
                Usuario
            );

            // Assert
            Assert.True(empresa.IsValid);
            Assert.Equal("Empresa Teste S.A.", empresa.RazaoSocial);
            Assert.Equal("12345678000195", empresa.Cnpj);
            Assert.Equal(RegimeTributario.SimplesNacional, empresa.RegimeTributario);
            Assert.Equal("Av Paulista", empresa.Endereco.Logradouro);
            Assert.True(empresa.Ativo);
        }

        [Fact]
        public void Deve_Retornar_Erro_Se_RazaoSocial_Ou_Cnpj_Forem_Invalidos()
        {
            // Arrange
            var endereco = new EnderecoVo("Av Paulista", "1000", null, "Bela Vista", "01310-100", "São Paulo", "SP");

            // Empty RazaoSocial
            var empresaSemRazao = new Empresa(
                "",
                "Teste",
                "12345678000195",
                null, null, null, null,
                RegimeTributario.SimplesNacional,
                RegimeApuracao.Cumulativo,
                null, null, null, null, null, null, null, null, null, null,
                endereco,
                TenantId,
                Usuario
            );

            // Invalid CNPJ
            var empresaCnpjInvalido = new Empresa(
                "Empresa Teste S.A.",
                "Teste",
                "11222333000100", // Invalid CNPJ logic
                null, null, null, null,
                RegimeTributario.SimplesNacional,
                RegimeApuracao.Cumulativo,
                null, null, null, null, null, null, null, null, null, null,
                endereco,
                TenantId,
                Usuario
            );

            // Assert
            Assert.False(empresaSemRazao.IsValid);
            Assert.Contains(empresaSemRazao.Notifications, n => n.Key == "RazaoSocial");

            Assert.False(empresaCnpjInvalido.IsValid);
            Assert.Contains(empresaCnpjInvalido.Notifications, n => n.Key == "Cnpj" && n.Message.Contains("CNPJ inválido"));
        }

        [Fact]
        public void Deve_Retornar_Erro_Se_Endereco_For_Invalido()
        {
            // Arrange
            // Empty logradouro on Endereco
            var enderecoInvalido = new EnderecoVo(
                "",
                "1000",
                null,
                "Bela Vista",
                "01310-100",
                "São Paulo",
                "SP"
            );

            // Act
            var empresa = new Empresa(
                "Empresa Teste S.A.",
                "Teste Fantasia",
                "12345678000195",
                null, null, null, null,
                RegimeTributario.SimplesNacional,
                RegimeApuracao.Cumulativo,
                null, null, null, null, null, null, null, null, null, null,
                enderecoInvalido,
                TenantId,
                Usuario
            );

            // Assert
            Assert.False(empresa.IsValid);
            Assert.Contains(empresa.Notifications, n => n.Key == "Logradouro" && n.Message.Contains("Logradouro é obrigatório"));
        }

        [Fact]
        public void Deve_Atualizar_Dados_Empresa_Corretamente()
        {
            // Arrange
            var enderecoOriginal = new EnderecoVo("Av Paulista", "1000", null, "Bela Vista", "01310-100", "São Paulo", "SP");
            var empresa = new Empresa(
                "Empresa Original S.A.",
                "Original",
                "12345678000195",
                null, null, null, null,
                RegimeTributario.SimplesNacional,
                RegimeApuracao.Cumulativo,
                null, null, null, null, null, null, null, null, null, null,
                enderecoOriginal,
                TenantId,
                Usuario
            );

            var novoEndereco = new EnderecoVo("Rua Augusta", "500", null, "Consolação", "01305-100", "São Paulo", "SP");

            // Act
            empresa.Atualizar(
                "Empresa Atualizada S.A.",
                "Atualizada",
                "123", "456", "789", "CNAE",
                RegimeTributario.LucroReal,
                RegimeApuracao.NaoCumulativo,
                null, null, null, null, null, null, null,
                "https://api.vendas2.com",
                "mp-token-new",
                "https://logo.com/new.png",
                novoEndereco,
                "user-editor"
            );

            // Assert
            Assert.True(empresa.IsValid);
            Assert.Equal("Empresa Atualizada S.A.", empresa.RazaoSocial);
            Assert.Equal("Atualizada", empresa.NomeFantasia);
            Assert.Equal(RegimeTributario.LucroReal, empresa.RegimeTributario);
            Assert.Equal("Rua Augusta", empresa.Endereco.Logradouro);
            Assert.Equal("https://logo.com/new.png", empresa.Logo);
            Assert.Equal("user-editor", empresa.AlteradoPor);
        }

        [Fact]
        public void Deve_Alterar_Logo_Independentemente()
        {
            // Arrange
            var endereco = new EnderecoVo("Av Paulista", "1000", null, "Bela Vista", "01310-100", "São Paulo", "SP");
            var empresa = new Empresa(
                "Empresa Original S.A.",
                "Original",
                "12345678000195",
                null, null, null, null,
                RegimeTributario.SimplesNacional,
                RegimeApuracao.Cumulativo,
                null, null, null, null, null, null, null, null, null, null,
                endereco,
                TenantId,
                Usuario
            );

            // Act
            empresa.AlterarLogo("https://novologo.com/img.jpg", "user-editor");

            // Assert
            Assert.Equal("https://novologo.com/img.jpg", empresa.Logo);
            Assert.Equal("user-editor", empresa.AlteradoPor);
        }
    }
}
