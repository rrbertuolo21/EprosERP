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
    public class CfopTests
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
        public void CriarCfop_Com_CfopDevolucao_Valido_Deve_Ser_Valido()
        {
            // Arrange & Act
            var cfop = new Cfop(
                cfopCodigo: 1102,
                descricao: "Compra para industrialização",
                naturezaOperacao: "Compra",
                cfopCorrelacao: "1102",
                integraFaturamento: true,
                indicadorNfe: true,
                indicadorComunicacao: false,
                indicadorTransporte: false,
                indicadorDevolucao: false,
                indicadorRetorno: false,
                indicadorAnulacao: false,
                indicadorRemessa: false,
                indicadorCombustivel: false,
                indicadorTransferencia: false,
                indicadorNfce: false,
                indicadorCiap: false,
                indicadorUsoConsumo: false,
                indicadorUsoSemOperacao: false,
                indicadorSt: false,
                indicadorMei: false,
                incidenciaSimples: EIncidenciaSimples.RevendaMercadorias,
                cfopDevolucao: "5202",
                tenantId: "tenant-123",
                criadoPor: "user-123"
            );

            // Assert
            Assert.True(cfop.IsValid);
            Assert.Empty(cfop.Notifications);
        }

        [Fact]
        public void CriarCfop_Com_CfopDevolucao_Invalido_Deve_Retornar_Erro_Validao()
        {
            // Arrange & Act
            var cfop = new Cfop(
                cfopCodigo: 1102,
                descricao: "Compra para industrialização",
                naturezaOperacao: "Compra",
                cfopCorrelacao: "1102",
                integraFaturamento: true,
                indicadorNfe: true,
                indicadorComunicacao: false,
                indicadorTransporte: false,
                indicadorDevolucao: false,
                indicadorRetorno: false,
                indicadorAnulacao: false,
                indicadorRemessa: false,
                indicadorCombustivel: false,
                indicadorTransferencia: false,
                indicadorNfce: false,
                indicadorCiap: false,
                indicadorUsoConsumo: false,
                indicadorUsoSemOperacao: false,
                indicadorSt: false,
                indicadorMei: false,
                incidenciaSimples: EIncidenciaSimples.RevendaMercadorias,
                cfopDevolucao: "2202", // Inválido: CFOP 1xxx exige devolução iniciando com 5
                tenantId: "tenant-123",
                criadoPor: "user-123"
            );

            // Assert
            Assert.False(cfop.IsValid);
            Assert.Contains(cfop.Notifications, n => n.Message.Contains("CFOP de devolução deve iniciar com 5"));
        }

        [Fact]
        public void CriarCfop_Com_CfopDevolucao_Saida_Invalido_Deve_Retornar_Erro_Validao()
        {
            // Arrange & Act
            var cfop = new Cfop(
                cfopCodigo: 5102,
                descricao: "Venda de mercadoria",
                naturezaOperacao: "Venda",
                cfopCorrelacao: "5102",
                integraFaturamento: true,
                indicadorNfe: true,
                indicadorComunicacao: false,
                indicadorTransporte: false,
                indicadorDevolucao: false,
                indicadorRetorno: false,
                indicadorAnulacao: false,
                indicadorRemessa: false,
                indicadorCombustivel: false,
                indicadorTransferencia: false,
                indicadorNfce: false,
                indicadorCiap: false,
                indicadorUsoConsumo: false,
                indicadorUsoSemOperacao: false,
                indicadorSt: false,
                indicadorMei: false,
                incidenciaSimples: EIncidenciaSimples.RevendaMercadorias,
                cfopDevolucao: "5202", // Inválido: CFOP 5xxx exige devolução iniciando com 1
                tenantId: "tenant-123",
                criadoPor: "user-123"
            );

            // Assert
            Assert.False(cfop.IsValid);
            Assert.Contains(cfop.Notifications, n => n.Message.Contains("CFOP de devolução deve iniciar with 1") || n.Message.Contains("deve iniciar com 1"));
        }

        [Fact]
        public async Task Handler_CriarCfop_Deve_Persistir_No_Banco()
        {
            // Arrange
            var context = CreateInMemoryContext("db_handler_criar_cfop");
            var tenantProvider = new TestTenantProvider("tenant-123");
            var currentUser = new TestCurrentUser("user-123");
            var handler = new CriarCfopCommandHandler(context, tenantProvider, currentUser);

            var command = new CriarCfopCommand(
                CfopCodigo: 1102,
                Descricao: "Compra de mercadoria",
                NaturezaOperacao: "Compra",
                CfopCorrelacao: "1102",
                IntegraFaturamento: true,
                IndicadorNfe: true,
                IndicadorComunicacao: false,
                IndicadorTransporte: false,
                IndicadorDevolucao: false,
                IndicadorRetorno: false,
                IndicadorAnulacao: false,
                IndicadorRemessa: false,
                IndicadorCombustivel: false,
                IndicadorTransferencia: false,
                IndicadorNfce: false,
                IndicadorCiap: false,
                IndicadorUsoConsumo: false,
                IndicadorUsoSemOperacao: false,
                IndicadorSt: false,
                IndicadorMei: false,
                IncidenciaSimples: EIncidenciaSimples.RevendaMercadorias,
                CfopDevolucao: "5202"
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);
            var totalInDb = await context.Cfops.CountAsync();
            Assert.Equal(1, totalInDb);

            var cfopInDb = await context.Cfops.FirstAsync();
            Assert.Equal(1102, cfopInDb.CfopCodigo);
            Assert.Equal("5202", cfopInDb.CfopDevolucao);
        }

        [Fact]
        public async Task Handler_ListarCfops_Deve_Retornar_Itens_Corretamente()
        {
            // Arrange
            var context = CreateInMemoryContext("db_handler_listar_cfop");
            var queryHandler = new ListarCfopsQueryHandler(context);

            var cfop1 = new Cfop(1102, "Compra A", "Compra", "1102", true, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, EIncidenciaSimples.RevendaMercadorias, "5202", "tenant-123", "user-123");
            var cfop2 = new Cfop(5102, "Venda B", "Venda", "5102", true, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, EIncidenciaSimples.RevendaMercadorias, "1202", "tenant-123", "user-123");
            
            context.Cfops.AddRange(cfop1, cfop2);
            await context.SaveChangesAsync();

            // Act
            var query = new ListarCfopsQuery(Localizar: "Compra");
            var result = await queryHandler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);
            Assert.NotNull(result.Dados);
            var totalProp = result.Dados.GetType().GetProperty("Total")?.GetValue(result.Dados, null);
            Assert.Equal(1, totalProp);
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
