using System;
using Xunit;
using Epros.Modules.GestaoClientes.Domain.Entities;

namespace Epros.Tests
{
    /// <summary>
    /// Testes de domínio (puros) da entidade Endereco do módulo GestaoClientes.
    /// Cobrem regras portadas do legado que não eram exercitadas diretamente:
    /// endereço de entrega exige NomeDoRecebedor/DocumentoDoRecebedor, validação de CEP,
    /// limites de tamanho e a formatação de endereço para transportadora.
    /// </summary>
    public class EnderecoEntidadeTests
    {
        private const string TenantId = "tenant-test";
        private const string Usuario = "user-test";

        private static Endereco CriarEndereco(
            ETipoEndereco tipo,
            string? cep = "01310-100",
            string logradouro = "Av Paulista",
            string bairro = "Bela Vista",
            string? nomeRecebedor = null,
            string? documentoRecebedor = null)
        {
            return new Endereco(
                pessoaId: Guid.NewGuid(),
                tipoEndereco: tipo,
                paisId: Guid.NewGuid(),
                municipioId: Guid.NewGuid(),
                subdivisaoId: null,
                uf: "SP",
                cep: cep,
                logradouro: logradouro,
                numero: "1000",
                complemento: null,
                bairro: bairro,
                referencia: null,
                codigoPostalInternacional: null,
                linhaEndereco1: null,
                linhaEndereco2: null,
                latitude: null,
                longitude: null,
                tenantId: TenantId,
                criadoPor: Usuario,
                nomeDoRecebedor: nomeRecebedor,
                documentoDoRecebedor: documentoRecebedor);
        }

        [Fact]
        public void Endereco_Principal_ComDadosValidos_DeveSerValido()
        {
            var endereco = CriarEndereco(ETipoEndereco.Principal);

            Assert.True(endereco.IsValid);
            Assert.Equal(ETipoEndereco.Principal, endereco.TipoEndereco);
        }

        [Fact]
        public void Endereco_Entrega_SemNomeRecebedor_DeveSerInvalido()
        {
            var endereco = CriarEndereco(ETipoEndereco.Entrega, nomeRecebedor: null, documentoRecebedor: "12345678909");

            Assert.False(endereco.IsValid);
            Assert.Contains(endereco.Notifications, n => n.Key == nameof(Endereco.NomeDoRecebedor));
        }

        [Fact]
        public void Endereco_Entrega_SemDocumentoRecebedor_DeveSerInvalido()
        {
            var endereco = CriarEndereco(ETipoEndereco.Entrega, nomeRecebedor: "João Recebedor", documentoRecebedor: null);

            Assert.False(endereco.IsValid);
            Assert.Contains(endereco.Notifications, n => n.Key == nameof(Endereco.DocumentoDoRecebedor));
        }

        [Fact]
        public void Endereco_Entrega_ComRecebedorCompleto_DeveSerValido()
        {
            var endereco = CriarEndereco(ETipoEndereco.Entrega, nomeRecebedor: "João Recebedor", documentoRecebedor: "12345678909");

            Assert.True(endereco.IsValid);
            Assert.Equal("João Recebedor", endereco.NomeDoRecebedor);
        }

        [Fact]
        public void Endereco_ComCepInvalido_DeveSerInvalido()
        {
            var endereco = CriarEndereco(ETipoEndereco.Principal, cep: "123");

            Assert.False(endereco.IsValid);
        }

        [Fact]
        public void Endereco_ComPaisVazio_DeveSerInvalido()
        {
            var endereco = new Endereco(
                pessoaId: Guid.NewGuid(),
                tipoEndereco: ETipoEndereco.Principal,
                paisId: Guid.Empty,
                municipioId: Guid.NewGuid(),
                subdivisaoId: null,
                uf: "SP",
                cep: "01310-100",
                logradouro: "Av Paulista",
                numero: "1000",
                complemento: null,
                bairro: "Bela Vista",
                referencia: null,
                codigoPostalInternacional: null,
                linhaEndereco1: null,
                linhaEndereco2: null,
                latitude: null,
                longitude: null,
                tenantId: TenantId,
                criadoPor: Usuario);

            Assert.False(endereco.IsValid);
            Assert.Contains(endereco.Notifications, n => n.Key == nameof(Endereco.PaisId));
        }

        [Fact]
        public void Endereco_ComLogradouroAcimaDoLimite_DeveSerInvalido()
        {
            var endereco = CriarEndereco(ETipoEndereco.Principal, logradouro: new string('L', 151));

            Assert.False(endereco.IsValid);
            Assert.Contains(endereco.Notifications, n => n.Key == nameof(Endereco.Logradouro));
        }

        [Fact]
        public void Endereco_ObterEnderecoCompletoTransportadora_DeveFormatarERespeitarLimite()
        {
            var endereco = CriarEndereco(ETipoEndereco.Principal);

            var texto = endereco.ObterEnderecoCompletoTransportadora();

            Assert.Contains("Av Paulista", texto);
            Assert.Contains("1000", texto);
            Assert.True(texto.Length <= 60);
        }
    }
}
