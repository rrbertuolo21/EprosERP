using System;
using System.Linq;
using Xunit;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Domain.ValueObjects;
using EnderecoEntity = Epros.Modules.GestaoClientes.Domain.Entities.Endereco;

namespace Epros.Tests
{
    public class PessoaTests
    {
        private const string TenantId = "tenant-test";
        private const string Usuario = "user-test";

        [Fact]
        public void Deve_Criar_Pessoa_Fisica_Cliente_Valida()
        {
            // Arrange
            var pessoa = new Pessoa(
                ETipoPessoa.PessoaFisica,
                ETipoIndicadorIe.NaoContribuinte,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                "Observacoes de teste",
                TenantId,
                Usuario
            );

            var pf = new PessoaFisica(
                pessoa.Id,
                new Cpf("12345678909"), // Valid CPF
                "Maria",
                "Silva",
                "1234567",
                "SSP",
                ETipoGenero.Feminino,
                new DateTime(1990, 5, 15),
                TenantId,
                Usuario
            );

            var cliente = new PessoaCliente(
                pessoa.Id,
                true,
                ETipoContribuinte.NaoInformado,
                TenantId,
                Usuario
            );

            var endereco = new EnderecoEntity(
                pessoa.Id,
                ETipoEndereco.Principal,
                Guid.NewGuid(), // PaisId
                Guid.NewGuid(), // MunicipioId
                null, // SubdivisaoId
                "SP",
                "01310-100",
                "Av Paulista",
                "1000",
                "Apto 12",
                "Bela Vista",
                "Perto do metro",
                null, // CodigoPostalInternacional
                null, // LinhaEndereco1
                null, // LinhaEndereco2
                null, // Latitude
                null, // Longitude
                TenantId,
                Usuario
            );

            var contato = new PessoaContato(
                pessoa.Id,
                "Maria Principal",
                ETipoContatoTelefonico.Comercial,
                "11999999999",
                ETipoContatoEmail.EnvioNFe,
                "maria@teste.com",
                true,
                TenantId,
                Usuario
            );

            // Act
            pessoa.VincularFisica(pf);
            pessoa.VincularCliente(cliente);
            pessoa.AdicionarEndereco(endereco);
            pessoa.AdicionarContato(contato);
            pessoa.Validar();

            // Assert
            Assert.True(pessoa.IsValid);
            Assert.True(pessoa.EhCliente);
            Assert.False(pessoa.EhFuncionario);
            Assert.NotNull(pessoa.PessoaFisica);
            Assert.Equal("Maria", pessoa.PessoaFisica.Nome);
            Assert.Single(pessoa.Enderecos);
            Assert.Single(pessoa.Contatos);
            Assert.Equal(EEstadoPessoa.Ativo, pessoa.Status);
        }

        [Fact]
        public void Deve_Criar_Pessoa_Juridica_Fornecedor_Valida()
        {
            // Arrange
            var pessoa = new Pessoa(
                ETipoPessoa.PessoaJuridica,
                ETipoIndicadorIe.ContribuinteICMS,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                TenantId,
                Usuario
            );

            var pj = new PessoaJuridica(
                pessoa.Id,
                new Cnpj("12345678000195"), // Valid CNPJ
                "Empresa Teste Ltda",
                "Teste Fantasia",
                "123456789",
                "987654",
                "6201501",
                TenantId,
                Usuario
            );

            var endereco = new EnderecoEntity(
                pessoa.Id,
                ETipoEndereco.Principal,
                Guid.NewGuid(), // PaisId
                Guid.NewGuid(), // MunicipioId
                null, // SubdivisaoId
                "SP",
                "01310-100",
                "Av Paulista",
                "1000",
                null,
                "Bela Vista",
                null,
                null, // CodigoPostalInternacional
                null, // LinhaEndereco1
                null, // LinhaEndereco2
                null, // Latitude
                null, // Longitude
                TenantId,
                Usuario
            );

            var contato = new PessoaContato(
                pessoa.Id,
                "Contato PJ",
                ETipoContatoTelefonico.Comercial,
                "11999999999",
                ETipoContatoEmail.EnvioNFe,
                "contato@pj.com",
                true,
                TenantId,
                Usuario
            );

            // Act
            pessoa.VincularJuridica(pj);
            pessoa.DefinirFornecedor(true);
            pessoa.AdicionarEndereco(endereco);
            pessoa.AdicionarContato(contato);
            pessoa.Validar();

            // Assert
            Assert.True(pessoa.IsValid);
            Assert.True(pessoa.EhFornecedor);
            Assert.NotNull(pessoa.PessoaJuridica);
            Assert.Equal("Empresa Teste Ltda", pj.RazaoSocial);
        }

        [Fact]
        public void Deve_Retornar_Erro_Se_Nao_Houver_Nenhum_Papel_Vinculado()
        {
            // Arrange
            var pessoa = new Pessoa(
                ETipoPessoa.PessoaFisica,
                ETipoIndicadorIe.NaoContribuinte,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                TenantId,
                Usuario
            );

            var pf = new PessoaFisica(
                pessoa.Id,
                new Cpf("12345678909"),
                "Maria",
                "Silva",
                "1234567",
                "SSP",
                ETipoGenero.Feminino,
                new DateTime(1990, 5, 15),
                TenantId,
                Usuario
            );

            pessoa.VincularFisica(pf);

            // Act
            pessoa.Validar();

            // Assert
            Assert.False(pessoa.IsValid);
            Assert.Contains(pessoa.Notifications, n => n.Key == "EhCliente" && n.Message.Contains("Pelo menos uma opção deve estar marcada"));
        }

        [Fact]
        public void Deve_Retornar_Erro_Se_Pessoa_For_Motorista_E_Transportadora_Simultaneamente()
        {
            // Arrange
            var pessoa = new Pessoa(
                ETipoPessoa.PessoaFisica,
                ETipoIndicadorIe.NaoContribuinte,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                TenantId,
                Usuario
            );

            var pf = new PessoaFisica(
                pessoa.Id,
                new Cpf("12345678909"),
                "Maria",
                "Silva",
                null,
                null,
                ETipoGenero.Feminino,
                null,
                TenantId,
                Usuario
            );

            var motorista = new PessoaMotorista(
                pessoa.Id,
                ETipoVinculoMotorista.Proprio,
                ETipoCategoriaCnh.B,
                DateTime.UtcNow,
                DateTime.UtcNow.AddYears(5),
                "123456",
                TenantId,
                Usuario
            );

            var transportadora = new PessoaTransportadora(
                pessoa.Id,
                "123456",
                "123456",
                TenantId,
                Usuario
            );

            pessoa.VincularFisica(pf);
            pessoa.VincularMotorista(motorista);
            pessoa.VincularTransportadora(transportadora);

            // Act
            pessoa.Validar();

            // Assert
            Assert.False(pessoa.IsValid);
            Assert.Contains(pessoa.Notifications, n => n.Key == "EhMotorista" && n.Message.Contains("Motorista e Transportadora"));
        }

        [Fact]
        public void Deve_Retornar_Erro_Se_Motorista_For_Pessoa_Juridica()
        {
            // Arrange
            var pessoa = new Pessoa(
                ETipoPessoa.PessoaJuridica,
                ETipoIndicadorIe.NaoContribuinte,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                TenantId,
                Usuario
            );

            var pj = new PessoaJuridica(
                pessoa.Id,
                new Cnpj("12345678000195"),
                "Empresa Motorista",
                null,
                null,
                null,
                null,
                TenantId,
                Usuario
            );

            var motorista = new PessoaMotorista(
                pessoa.Id,
                ETipoVinculoMotorista.Proprio,
                ETipoCategoriaCnh.B,
                null,
                null,
                null,
                TenantId,
                Usuario
            );

            pessoa.VincularJuridica(pj);
            pessoa.VincularMotorista(motorista);

            // Act
            pessoa.Validar();

            // Assert
            Assert.False(pessoa.IsValid);
            Assert.Contains(pessoa.Notifications, n => n.Key == "EhMotorista" && n.Message.Contains("não pode ser Pessoa Jurídica"));
        }

        [Fact]
        public void Deve_Retornar_Erro_Se_Houver_Mais_De_Um_Endereco_Principal()
        {
            // Arrange
            var pessoa = new Pessoa(
                ETipoPessoa.PessoaFisica,
                ETipoIndicadorIe.NaoContribuinte,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                TenantId,
                Usuario
            );

            var pf = new PessoaFisica(
                pessoa.Id,
                new Cpf("12345678909"),
                "Maria",
                "Silva",
                null,
                null,
                ETipoGenero.Feminino,
                null,
                TenantId,
                Usuario
            );

            var e1 = new EnderecoEntity(pessoa.Id, ETipoEndereco.Principal, Guid.NewGuid(), Guid.NewGuid(), null, "SP", "01310-100", "Av 1", "10", null, "Bairro", null, null, null, null, null, null, TenantId, Usuario);
            var e2 = new EnderecoEntity(pessoa.Id, ETipoEndereco.Principal, Guid.NewGuid(), Guid.NewGuid(), null, "SP", "01310-200", "Av 2", "20", null, "Bairro", null, null, null, null, null, null, TenantId, Usuario);

            pessoa.VincularFisica(pf);
            pessoa.VincularCliente(new PessoaCliente(pessoa.Id, true, ETipoContribuinte.NaoInformado, TenantId, Usuario));
            pessoa.AdicionarEndereco(e1);
            pessoa.AdicionarEndereco(e2);

            // Act
            pessoa.Validar();

            // Assert
            Assert.False(pessoa.IsValid);
            Assert.Contains(pessoa.Notifications, n => n.Key == "Enderecos" && n.Message.Contains("Já existe um endereço Principal"));
        }

        [Fact]
        public void Deve_Retornar_Erro_Se_Nao_Estrangeiro_Tiver_CEP_Nulo()
        {
            // Arrange
            var pessoa = new Pessoa(
                ETipoPessoa.PessoaFisica,
                ETipoIndicadorIe.NaoContribuinte,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                TenantId,
                Usuario
            );

            var pf = new PessoaFisica(
                pessoa.Id,
                new Cpf("12345678909"),
                "Maria",
                "Silva",
                null,
                null,
                ETipoGenero.Feminino,
                null,
                TenantId,
                Usuario
            );

            // CEP passed as null
            var e = new EnderecoEntity(pessoa.Id, ETipoEndereco.Principal, Guid.NewGuid(), Guid.NewGuid(), null, "SP", null, "Av 1", "10", null, "Bairro", null, null, null, null, null, null, TenantId, Usuario);

            pessoa.VincularFisica(pf);
            pessoa.VincularCliente(new PessoaCliente(pessoa.Id, true, ETipoContribuinte.NaoInformado, TenantId, Usuario));
            pessoa.AdicionarEndereco(e);

            // Act
            pessoa.Validar();

            // Assert
            Assert.False(pessoa.IsValid);
            Assert.Contains(pessoa.Notifications, n => n.Key == "Enderecos" && n.Message.Contains("CEP só pode ser Nulo para estrangeiro"));
        }

        [Fact]
        public void Deve_Retornar_Erro_Se_Contatos_Nao_Tiverem_Principal()
        {
            // Arrange
            var pessoa = new Pessoa(
                ETipoPessoa.PessoaFisica,
                ETipoIndicadorIe.NaoContribuinte,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                TenantId,
                Usuario
            );

            var pf = new PessoaFisica(
                pessoa.Id,
                new Cpf("12345678909"),
                "Maria",
                "Silva",
                null,
                null,
                ETipoGenero.Feminino,
                null,
                TenantId,
                Usuario
            );

            var contato = new PessoaContato(
                pessoa.Id,
                "Contato 1",
                ETipoContatoTelefonico.Residencial,
                "1155555555",
                ETipoContatoEmail.EnvioNFe,
                "email@email.com",
                false, // NOT Principal
                TenantId,
                Usuario
            );

            pessoa.VincularFisica(pf);
            pessoa.VincularCliente(new PessoaCliente(pessoa.Id, true, ETipoContribuinte.NaoInformado, TenantId, Usuario));
            pessoa.AdicionarContato(contato);

            // Act
            pessoa.Validar();

            // Assert
            Assert.False(pessoa.IsValid);
            Assert.Contains(pessoa.Notifications, n => n.Key == "Contatos" && n.Message.Contains("Deve haver um contato como Principal"));
        }

        [Fact]
        public void Deve_Retornar_Erro_Se_Contatos_Tiverem_Mais_De_Um_Principal()
        {
            // Arrange
            var pessoa = new Pessoa(
                ETipoPessoa.PessoaFisica,
                ETipoIndicadorIe.NaoContribuinte,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                TenantId,
                Usuario
            );

            var pf = new PessoaFisica(
                pessoa.Id,
                new Cpf("12345678909"),
                "Maria",
                "Silva",
                null,
                null,
                ETipoGenero.Feminino,
                null,
                TenantId,
                Usuario
            );

            var c1 = new PessoaContato(pessoa.Id, "C1", ETipoContatoTelefonico.Residencial, "11", ETipoContatoEmail.Contador, "e1@test.com", true, TenantId, Usuario);
            var c2 = new PessoaContato(pessoa.Id, "C2", ETipoContatoTelefonico.Comercial, "12", ETipoContatoEmail.EnvioNFe, "e2@test.com", true, TenantId, Usuario);

            pessoa.VincularFisica(pf);
            pessoa.VincularCliente(new PessoaCliente(pessoa.Id, true, ETipoContribuinte.NaoInformado, TenantId, Usuario));
            pessoa.AdicionarContato(c1);
            pessoa.AdicionarContato(c2);

            // Act
            pessoa.Validar();

            // Assert
            Assert.False(pessoa.IsValid);
            Assert.Contains(pessoa.Notifications, n => n.Key == "Contatos" && n.Message.Contains("Deve haver apenas um contato como Principal"));
        }

        [Fact]
        public void Deve_Validar_Percentual_Comissao_Funcionario()
        {
            // Arrange
            var fInvalidoAlto = new PessoaFuncionario(Guid.NewGuid(), ETipoCargo.Vendedor, 100.5m, TenantId, Usuario);
            var fInvalidoBaixo = new PessoaFuncionario(Guid.NewGuid(), ETipoCargo.Vendedor, -0.1m, TenantId, Usuario);
            var fValido = new PessoaFuncionario(Guid.NewGuid(), ETipoCargo.Vendedor, 15.5m, TenantId, Usuario);

            // Assert
            Assert.False(fInvalidoAlto.IsValid);
            Assert.Contains(fInvalidoAlto.Notifications, n => n.Key == "ValorPercentualComissao");

            Assert.False(fInvalidoBaixo.IsValid);
            Assert.Contains(fInvalidoBaixo.Notifications, n => n.Key == "ValorPercentualComissao");

            Assert.True(fValido.IsValid);
        }

        [Fact]
        public void Deve_Validar_PessoaVeiculo()
        {
            // Arrange
            var vInvalidoPlaca = new PessoaVeiculo(Guid.NewGuid(), Guid.NewGuid(), ETipoVeiculo.Veiculo, "SP", "ABC12", "Rntrc", TenantId, Usuario); // Placa invalid format
            var vInvalidoPais = new PessoaVeiculo(Guid.NewGuid(), Guid.Empty, ETipoVeiculo.Veiculo, "SP", "ABC1234", "Rntrc", TenantId, Usuario);
            var vValido = new PessoaVeiculo(Guid.NewGuid(), Guid.NewGuid(), ETipoVeiculo.Veiculo, "SP", "ABC1234", "Rntrc", TenantId, Usuario);

            // Assert
            Assert.False(vInvalidoPlaca.IsValid);
            Assert.Contains(vInvalidoPlaca.Notifications, n => n.Key == "Placa");

            Assert.False(vInvalidoPais.IsValid);
            Assert.Contains(vInvalidoPais.Notifications, n => n.Key == "PaisId");

            Assert.True(vValido.IsValid);
        }

        [Fact]
        public void Deve_Validar_ValueObjects()
        {
            // Cpf
            Assert.True(new Cpf("123.456.789-09").IsValid); // Valid logic
            Assert.False(new Cpf("12345678900").IsValid);

            // Cnpj
            Assert.True(new Cnpj("12.345.678/0001-95").IsValid);
            Assert.False(new Cnpj("11222333000100").IsValid);

            // Cep
            Assert.True(new Cep("01310-100").IsValid);
            Assert.False(new Cep("1310-100").IsValid);

            // Placa
            Assert.True(new Placa("ABC-1234").IsValid);
            Assert.True(new Placa("ABC1D23").IsValid);
            Assert.False(new Placa("AB12345").IsValid);
        }
    }
}
