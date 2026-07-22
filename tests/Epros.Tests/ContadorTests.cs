using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Application.Handlers;
using Epros.Modules.Fiscal.Application.Queries;
using Epros.Modules.Fiscal.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Enums;

namespace Epros.Tests
{
    public class ContadorTests
    {
        private ContextFiscal CreateInMemoryContext(string databaseName)
        {
            var options = new DbContextOptionsBuilder<ContextFiscal>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-123");
            var currentUser = new TestCurrentUser("user-123");

            return new ContextFiscal(options, tenantProvider, currentUser);
        }

        [Fact]
        public void CriarContador_Valido_Deve_Ser_Valido()
        {
            // Arrange & Act
            var contador = new Contador(
                razaoSocial: "Razão Contábil Ltda",
                nomeContador: "João Contador",
                cpf: "12345678901",
                cnpj: "12345678000100",
                numeroCrc: "CRC12345",
                ufCrc: EEstado.PR,
                dataVencimentoCrc: DateTime.Now.AddYears(1),
                qualificacao: "Sócio",
                funcao: "Responsável",
                telefone: "41999998888",
                email: "joao@contabilidade.com",
                permissaoTransmissao: EPermissaoTransmissao.Ambos,
                ativo: true,
                logradouro: "Rua das Flores",
                numero: "100",
                complemento: "Sala 2",
                bairro: "Centro",
                cep: "80020100",
                municipioId: 4106902,
                uf: EEstado.PR,
                tenantId: "tenant-123",
                criadoPor: "user-123"
            );

            // Assert
            Assert.True(contador.IsValid);
            Assert.Empty(contador.Notifications);
        }

        [Fact]
        public void CriarContador_Invalido_Deve_Retornar_Erro_Validao()
        {
            // Arrange & Act
            var contador = new Contador(
                razaoSocial: "Razão Contábil Ltda",
                nomeContador: "João Contador",
                cpf: "12345678901",
                cnpj: "12345678000100",
                numeroCrc: "", // Inválido: vazio
                ufCrc: EEstado.PR,
                dataVencimentoCrc: DateTime.Now.AddYears(1),
                qualificacao: "Sócio",
                funcao: "Responsável",
                telefone: "41999998888",
                email: "joao@contabilidade.com",
                permissaoTransmissao: EPermissaoTransmissao.Ambos,
                ativo: true,
                logradouro: "", // Inválido: vazio
                numero: "100",
                complemento: "Sala 2",
                bairro: "Centro",
                cep: "80020100",
                municipioId: 4106902,
                uf: EEstado.PR,
                tenantId: "tenant-123",
                criadoPor: "user-123"
            );

            // Assert
            Assert.False(contador.IsValid);
            Assert.Contains(contador.Notifications, n => n.Message.Contains("O número do CRC deve ter no máximo 15 caracteres") || n.Message.Contains("O logradouro é obrigatório"));
        }

        [Fact]
        public async Task Handler_CriarContador_Deve_Persistir_No_Banco()
        {
            // Arrange
            var context = CreateInMemoryContext("db_handler_criar_contador");
            var tenantProvider = new TestTenantProvider("tenant-123");
            var currentUser = new TestCurrentUser("user-123");
            var handler = new CriarContadorCommandHandler(context, tenantProvider, currentUser);

            var command = new CriarContadorCommand(
                RazaoSocial: "Escritório Contábil",
                NomeContador: "José Contador",
                Cpf: "11122233344",
                Cnpj: null,
                NumeroCrc: "CRC54321",
                UfCrc: EEstado.SP,
                DataVencimentoCrc: DateTime.Now.AddYears(2),
                Qualificacao: "Sócio",
                Funcao: "Contador Geral",
                Telefone: "11988887777",
                Email: "jose@contabil.com",
                PermissaoTransmissao: EPermissaoTransmissao.Cpf,
                Ativo: true,
                Logradouro: "Av Paulista",
                Numero: "1000",
                Complemento: null,
                Bairro: "Bela Vista",
                Cep: "01310100",
                MunicipioId: 3550308,
                Uf: EEstado.SP
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);
            var totalInDb = await context.Contadores.CountAsync();
            Assert.Equal(1, totalInDb);

            var contInDb = await context.Contadores.FirstAsync();
            Assert.Equal("José Contador", contInDb.NomeContador);
            Assert.Equal("Av Paulista", contInDb.Logradouro);
        }

        [Fact]
        public async Task Handler_AtualizarContador_Deve_Alterar_No_Banco()
        {
            // Arrange
            var context = CreateInMemoryContext("db_handler_atualizar_contador");
            var tenantProvider = new TestTenantProvider("tenant-123");
            var currentUser = new TestCurrentUser("user-123");

            var contador = new Contador(
                razaoSocial: "Razão Antiga",
                nomeContador: "Nome Antigo",
                cpf: null,
                cnpj: null,
                numeroCrc: "CRC123",
                ufCrc: EEstado.PR,
                dataVencimentoCrc: DateTime.Now,
                qualificacao: null,
                funcao: null,
                telefone: null,
                email: null,
                permissaoTransmissao: EPermissaoTransmissao.NaoUtiliza,
                ativo: true,
                logradouro: "Rua Antiga",
                numero: "1",
                complemento: null,
                bairro: "Bairro Antigo",
                cep: "12345678",
                municipioId: 4106902,
                uf: EEstado.PR,
                tenantId: "tenant-123",
                criadoPor: "user-123"
            );

            context.Contadores.Add(contador);
            await context.SaveChangesAsync();

            var handler = new AtualizarContadorCommandHandler(context, tenantProvider, currentUser);
            var command = new AtualizarContadorCommand(
                Id: contador.Id,
                RazaoSocial: "Razão Nova",
                NomeContador: "Nome Novo",
                Cpf: "12345678901",
                Cnpj: null,
                NumeroCrc: "CRC777",
                UfCrc: EEstado.SC,
                DataVencimentoCrc: DateTime.Now.AddDays(10),
                Qualificacao: "Gerente",
                Funcao: "Auditor",
                Telefone: "4832221111",
                Email: "novo@contabil.com",
                PermissaoTransmissao: EPermissaoTransmissao.Ambos,
                Ativo: true,
                Logradouro: "Rua Nova",
                Numero: "99",
                Complemento: "Apto 3",
                Bairro: "Centro",
                Cep: "88010000",
                MunicipioId: 4205407,
                Uf: EEstado.SC
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);
            var contInDb = await context.Contadores.FirstAsync(x => x.Id == contador.Id);
            Assert.Equal("Nome Novo", contInDb.NomeContador);
            Assert.Equal("Rua Nova", contInDb.Logradouro);
            Assert.Equal(EEstado.SC, contInDb.UfCrc);
        }

        [Fact]
        public async Task Handler_DeletarContador_Deve_Realizar_Exclusao_Logica()
        {
            // Arrange
            var context = CreateInMemoryContext("db_handler_deletar_contador");
            var tenantProvider = new TestTenantProvider("tenant-123");
            var currentUser = new TestCurrentUser("user-123");

            var contador = new Contador(
                razaoSocial: "Para Deletar",
                nomeContador: "Contador Deletável",
                cpf: null,
                cnpj: null,
                numeroCrc: "CRC999",
                ufCrc: EEstado.RS,
                dataVencimentoCrc: DateTime.Now,
                qualificacao: null,
                funcao: null,
                telefone: null,
                email: null,
                permissaoTransmissao: EPermissaoTransmissao.NaoUtiliza,
                ativo: true,
                logradouro: "Rua Deletar",
                numero: "0",
                complemento: null,
                bairro: "Bairro",
                cep: "90000000",
                municipioId: 4314902,
                uf: EEstado.RS,
                tenantId: "tenant-123",
                criadoPor: "user-123"
            );

            context.Contadores.Add(contador);
            await context.SaveChangesAsync();

            var handler = new DeletarContadorCommandHandler(context, tenantProvider, currentUser);
            var command = new DeletarContadorCommand(contador.Id);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);
            var contInDb = await context.Contadores.IgnoreQueryFilters().FirstAsync(x => x.Id == contador.Id);
            Assert.NotNull(contInDb.DeletadoEm);
            Assert.Equal("user-123", contInDb.AlteradoPor);
        }

        private class TestTenantProvider : ITenantProvider
        {
            private readonly string _tenantId;
            public TestTenantProvider(string tenantId) => _tenantId = tenantId;
            public string GetTenantId() => _tenantId;
            public bool EhTenantDemo() => false;
        }

        private class TestCurrentUser : ICurrentUser
        {
            private readonly string _userId;
            public TestCurrentUser(string userId) => _userId = userId;
            public string? GetUserId() => _userId;
            public string? GetUserName() => "Test User";
            public string? GetUserEmail() => "test@epros.com";
        }
    }
}
