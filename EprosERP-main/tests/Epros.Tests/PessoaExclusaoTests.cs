using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Handlers;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Domain.ValueObjects;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;

namespace Epros.Tests
{
    // O PessoasController agora é fino (apenas IMediator): as regras de exclusão de Pessoa
    // (REG-PEM-126/128/129) migraram para ExcluirPessoaCommandHandler. Os testes cobrem o
    // handler diretamente, preservando a intenção original (cada regra de bloqueio de exclusão).
    // Os títulos financeiros são validados via lookups cross-module (ContaAReceberLookup/
    // ContaAPagarLookup) expostos pelo ContextGestaoClientes.
    public class PessoaExclusaoTests
    {
        private const string TenantId = "tenant-exclusao-test";
        private const string UsuarioId = "user-exclusao-test";

        private ContextGestaoClientes CreateInMemoryContext(string databaseName)
        {
            var gcOptions = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase(databaseName + "_gc")
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var tenantProvider = new TestTenantProvider(TenantId);
            var currentUser = new TestCurrentUser(UsuarioId);

            return new ContextGestaoClientes(gcOptions, tenantProvider, currentUser);
        }

        private ExcluirPessoaCommandHandler CreateHandler(ContextGestaoClientes gcContext)
        {
            var tenantProvider = new TestTenantProvider(TenantId);
            var currentUser = new TestCurrentUser(UsuarioId);
            return new ExcluirPessoaCommandHandler(gcContext, tenantProvider, currentUser);
        }

        private static Pessoa NovaPessoa() => new Pessoa(
            ETipoPessoa.PessoaFisica,
            ETipoIndicadorIe.NaoContribuinte,
            null, null, null, null, null, null, null, null,
            TenantId,
            UsuarioId
        );

        [Fact]
        public async Task Deve_Excluir_Pessoa_Com_Sucesso_Quando_Nao_Houver_Vinculos()
        {
            // Arrange
            var gcContext = CreateInMemoryContext("db_excluir_pessoa_sucesso");
            var handler = CreateHandler(gcContext);

            var pessoa = NovaPessoa();
            gcContext.Pessoas.Add(pessoa);
            await gcContext.SaveChangesAsync();

            // Act
            var result = await handler.Handle(new ExcluirPessoaCommand(pessoa.Id), CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);

            var deletedPessoa = await gcContext.Pessoas.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == pessoa.Id);
            Assert.NotNull(deletedPessoa);
            Assert.NotNull(deletedPessoa.DeletadoEm);
        }

        [Fact]
        public async Task Nao_Deve_Excluir_Pessoa_Que_For_Cliente_Padrao_Pdv()
        {
            // Arrange
            var gcContext = CreateInMemoryContext("db_excluir_pessoa_cliente_padrao");
            var handler = CreateHandler(gcContext);

            var pessoa = NovaPessoa();
            gcContext.Pessoas.Add(pessoa);

            var config = new ConfiguracaoGlobal("pdv.cliente_padrao_id", pessoa.Id.ToString(), false, "Cliente padrão", TenantId, UsuarioId);
            gcContext.ConfiguracoesGlobais.Add(config);

            await gcContext.SaveChangesAsync();

            // Act
            var result = await handler.Handle(new ExcluirPessoaCommand(pessoa.Id), CancellationToken.None);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains("O Cliente padrão de PDV não pode ser excluído.", result.Erros);
        }

        [Fact]
        public async Task Nao_Deve_Excluir_Pessoa_Com_Contratos_Vinculados()
        {
            // Arrange
            var gcContext = CreateInMemoryContext("db_excluir_pessoa_contratos");
            var handler = CreateHandler(gcContext);

            var pessoa = NovaPessoa();
            gcContext.Pessoas.Add(pessoa);

            var contrato = new Contrato(pessoa.Id, 10, DateTime.UtcNow, null, TenantId, UsuarioId);
            gcContext.Contratos.Add(contrato);

            await gcContext.SaveChangesAsync();

            // Act
            var result = await handler.Handle(new ExcluirPessoaCommand(pessoa.Id), CancellationToken.None);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("Não é possível excluir uma pessoa com contratos vinculados"));
        }

        [Fact]
        public async Task Nao_Deve_Excluir_Pessoa_Com_Contas_A_Receber()
        {
            // Arrange
            var gcContext = CreateInMemoryContext("db_excluir_pessoa_contas_receber");
            var handler = CreateHandler(gcContext);

            var pessoa = NovaPessoa();
            gcContext.Pessoas.Add(pessoa);
            await gcContext.SaveChangesAsync();

            // Título financeiro (conta a receber) vinculado, via lookup cross-module.
            gcContext.ContasAReceberLookup.Add(new ContaAReceberLookup
            {
                Id = Guid.NewGuid(),
                PessoaId = pessoa.Id,
                TenantId = TenantId
            });
            await gcContext.SaveChangesAsync();

            // Act
            var result = await handler.Handle(new ExcluirPessoaCommand(pessoa.Id), CancellationToken.None);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("Não é possível excluir uma pessoa com títulos financeiros"));
        }

        [Fact]
        public async Task Nao_Deve_Excluir_Pessoa_Com_Contas_A_Pagar()
        {
            // Arrange
            var gcContext = CreateInMemoryContext("db_excluir_pessoa_contas_pagar");
            var handler = CreateHandler(gcContext);

            var pessoa = NovaPessoa();
            gcContext.Pessoas.Add(pessoa);
            await gcContext.SaveChangesAsync();

            // Título financeiro (conta a pagar) vinculado, via lookup cross-module.
            gcContext.ContasAPagarLookup.Add(new ContaAPagarLookup
            {
                Id = Guid.NewGuid(),
                PessoaId = pessoa.Id,
                TenantId = TenantId
            });
            await gcContext.SaveChangesAsync();

            // Act
            var result = await handler.Handle(new ExcluirPessoaCommand(pessoa.Id), CancellationToken.None);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("Não é possível excluir uma pessoa com títulos financeiros"));
        }

        private class TestTenantProvider : ITenantProvider
        {
            private readonly string _tenantId;
            public TestTenantProvider(string tenantId) => _tenantId = tenantId;
            public string GetTenantId() => _tenantId;
        }

        private class TestCurrentUser : ICurrentUser
        {
            private readonly string _userId;
            public TestCurrentUser(string userId) => _userId = userId;
            public string? GetUserId() => _userId;
            public string? GetUserName() => "Exclusão Tester";
            public string? GetUserEmail() => "exclusao@epros.com";
        }
    }
}
