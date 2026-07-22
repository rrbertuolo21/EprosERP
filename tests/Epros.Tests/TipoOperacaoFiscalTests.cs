using System;
using System.Collections.Generic;
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
    public class TipoOperacaoFiscalTests
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
        public void CriarTipoOperacaoFiscal_Valido_Deve_Ser_Valido()
        {
            // Arrange & Act
            var operacao = new TipoOperacaoFiscal(
                tributarioGrupoId: Guid.NewGuid(),
                cfopNfeId: Guid.NewGuid(),
                cfopNfceId: Guid.NewGuid(),
                descricao: "Venda Presencial Est.",
                sobescreveTributacaoNcm: false,
                finalidade: EFinalidadeEmissao.fnNormal,
                atendimento: ETipoAtendimento.pcPresencial,
                tipoFrete: EModalidadeFrete.mfSemFrete,
                tipoMovimento: ETipoMovimento.Saida,
                tenantId: "tenant-123",
                criadoPor: "user-123"
            );

            // Assert
            Assert.True(operacao.IsValid);
            Assert.Empty(operacao.Notifications);
        }

        [Fact]
        public void CriarTipoOperacaoFiscal_Invalido_Deve_Retornar_Erro_Validao()
        {
            // Arrange & Act
            var operacao = new TipoOperacaoFiscal(
                tributarioGrupoId: Guid.NewGuid(),
                cfopNfeId: Guid.NewGuid(),
                cfopNfceId: Guid.NewGuid(),
                descricao: "", // Inválido: vazio
                sobescreveTributacaoNcm: false,
                finalidade: EFinalidadeEmissao.fnNormal,
                atendimento: ETipoAtendimento.pcPresencial,
                tipoFrete: EModalidadeFrete.mfSemFrete,
                tipoMovimento: ETipoMovimento.Saida,
                tenantId: "tenant-123",
                criadoPor: "user-123"
            );

            // Assert
            Assert.False(operacao.IsValid);
            Assert.Contains(operacao.Notifications, n => n.Message.Contains("A descrição é obrigatória"));
        }

        [Fact]
        public async Task Handler_CriarTipoOperacaoFiscal_Deve_Persistir_No_Banco()
        {
            // Arrange
            var context = CreateInMemoryContext("db_handler_criar_tipo_operacao");
            var tenantProvider = new TestTenantProvider("tenant-123");
            var currentUser = new TestCurrentUser("user-123");
            var handler = new CriarTipoOperacaoFiscalCommandHandler(context, tenantProvider, currentUser);

            var command = new CriarTipoOperacaoFiscalCommand(
                TributarioGrupoId: Guid.NewGuid(),
                CfopNfeId: Guid.NewGuid(),
                CfopNfceId: Guid.NewGuid(),
                Descricao: "Venda de mercadoria",
                SobescreveTributacaoNcm: false,
                Finalidade: EFinalidadeEmissao.fnNormal,
                Atendimento: ETipoAtendimento.pcPresencial,
                TipoFrete: EModalidadeFrete.mfSemFrete,
                TipoMovimento: ETipoMovimento.Saida
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);
            var totalInDb = await context.TiposOperacoesFiscais.CountAsync();
            Assert.Equal(1, totalInDb);

            var opInDb = await context.TiposOperacoesFiscais.FirstAsync();
            Assert.Equal("Venda de mercadoria", opInDb.Descricao);
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
