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
    public class CodigoBeneficioFiscalTests
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
        public void CriarCodigoBeneficioFiscal_Valido_Deve_Ser_Valido()
        {
            // Arrange & Act
            var beneficio = new CodigoBeneficioFiscal(
                codigo: "PR850001",
                uf: EEstado.PR,
                descricao: "Benefício Fiscal Diferimento ICMS",
                tenantId: "tenant-123",
                criadoPor: "user-123"
            );

            // Assert
            Assert.True(beneficio.IsValid);
            Assert.Empty(beneficio.Notifications);
        }

        [Fact]
        public void CriarCodigoBeneficioFiscal_Invalido_Deve_Retornar_Erro_Validao()
        {
            // Arrange & Act
            var beneficio = new CodigoBeneficioFiscal(
                codigo: "CODIGO_LONGO_INVALIDO", // Inválido: > 10 caracteres
                uf: EEstado.PR,
                descricao: "Descrição",
                tenantId: "tenant-123",
                criadoPor: "user-123"
            );

            // Assert
            Assert.False(beneficio.IsValid);
            Assert.Contains(beneficio.Notifications, n => n.Message.Contains("máximo 10 caracteres"));
        }

        [Fact]
        public async Task Handler_CriarCodigoBeneficioFiscal_Deve_Persistir_No_Banco()
        {
            // Arrange
            var context = CreateInMemoryContext("db_handler_criar_beneficio");
            var tenantProvider = new TestTenantProvider("tenant-123");
            var currentUser = new TestCurrentUser("user-123");
            var handler = new CriarCodigoBeneficioFiscalCommandHandler(context, tenantProvider, currentUser);

            var command = new CriarCodigoBeneficioFiscalCommand(
                Codigo: "SP90001",
                Uf: EEstado.SP,
                Descricao: "Diferimento de SP",
                Csts: new List<ECodigoSituacaoTributariaIcms> { ECodigoSituacaoTributariaIcms.Cst51 },
                Csosns: new List<ECodigoSituacaoOperacaoSimplesNacional> { ECodigoSituacaoOperacaoSimplesNacional.Csosn900 }
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);
            var totalInDb = await context.CodigosBeneficiosFiscais.CountAsync();
            Assert.Equal(1, totalInDb);

            var benInDb = await context.CodigosBeneficiosFiscais
                .Include(b => b.Csts)
                .Include(b => b.Csosns)
                .FirstAsync();
            Assert.Equal("SP90001", benInDb.Codigo);
            Assert.Single(benInDb.Csts);
            Assert.Single(benInDb.Csosns);
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
