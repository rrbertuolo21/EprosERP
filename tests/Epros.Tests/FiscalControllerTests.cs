using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Epros.API.Controllers;
using Epros.Modules.Fiscal.Application.Queries;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Modules.Fiscal.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using MediatR;


namespace Epros.Tests
{
    // O DocumentosFiscaisController agora é fino (apenas IMediator): a lógica de listagem/filtro
    // vive em ListarDocumentosFiscaisQueryHandler. Os testes cobrem o handler diretamente,
    // preservando a intenção original (listagem e filtro por status).
    public class FiscalControllerTests
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
        public async Task Listar_Deve_Retornar_Documentos_Filtrados_Com_Sucesso()
        {
            // Arrange
            var context = CreateInMemoryContext("db_fiscal_controller_listar");
            var handler = new ListarDocumentosFiscaisQueryHandler(context);

            var doc1 = new DocumentoFiscal("55", 2, 1, 100, 150.00m, "12345678000199", "Destinatario A", "tenant-123", "user-123");
            var doc2 = new DocumentoFiscal("65", 2, 1, 200, 50.00m, "12345678000199", "Destinatario B", "tenant-123", "user-123");
            doc2.Submeter(); // Mudar status para Pendente

            context.DocumentosFiscais.AddRange(doc1, doc2);
            await context.SaveChangesAsync();

            // Act
            var result = await handler.Handle(new ListarDocumentosFiscaisQuery(Status: null, Pagina: 1, TamanhoPagina: 10), default);

            // Assert
            Assert.True(result.Sucesso);
            var dados = result.Dados!;
            var totalProp = dados.GetType().GetProperty("Total")?.GetValue(dados, null);
            var itensProp = dados.GetType().GetProperty("Itens")?.GetValue(dados, null) as IEnumerable<object>;

            Assert.Equal(2, totalProp);
            Assert.NotNull(itensProp);
            Assert.Equal(2, itensProp!.Count());
        }

        [Fact]
        public async Task Listar_Filtrando_Por_Status_Deve_Retornar_Apenas_Correspondentes()
        {
            // Arrange
            var context = CreateInMemoryContext("db_fiscal_controller_listar_filtrado");
            var handler = new ListarDocumentosFiscaisQueryHandler(context);

            var doc1 = new DocumentoFiscal("55", 2, 1, 100, 150.00m, "12345678000199", "Destinatario A", "tenant-123", "user-123");
            var doc2 = new DocumentoFiscal("65", 2, 1, 200, 50.00m, "12345678000199", "Destinatario B", "tenant-123", "user-123");
            doc2.Submeter(); // Pendente

            context.DocumentosFiscais.AddRange(doc1, doc2);
            await context.SaveChangesAsync();

            // Act
            var result = await handler.Handle(new ListarDocumentosFiscaisQuery(Status: "Pendente", Pagina: 1, TamanhoPagina: 10), default);

            // Assert
            Assert.True(result.Sucesso);
            var dados = result.Dados!;
            var totalProp = dados.GetType().GetProperty("Total")?.GetValue(dados, null);
            var itensProp = dados.GetType().GetProperty("Itens")?.GetValue(dados, null) as IEnumerable<object>;

            Assert.Equal(1, totalProp);
            Assert.NotNull(itensProp);
            Assert.Single(itensProp!);
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
            public string? GetUserName() => "Test User";
            public string? GetUserEmail() => "test@epros.com";
        }
    }
}
