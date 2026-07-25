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
    public class ServicoTests
    {
        private static readonly Guid EmpresaId = Guid.NewGuid();

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
        public void CriarServico_Valido_Deve_Ser_Valido()
        {
            // Arrange & Act
            var servico = new Servico(
                empresaId: EmpresaId,
                unidadeMedidaId: Guid.NewGuid(),
                codigoServicoSefazId: Guid.NewGuid(),
                codigo: "SERV001",
                descricao: "Consultoria em TI",
                valor: 150.00m,
                informacaoAdicional: "Desenvolvimento de Software",
                servicoAtivo: true,
                cnae: 6202300,
                codigoNbs: "102030",
                indicadorIss: true,
                indicadorIncentivo: false,
                cstIbsCbs: "IBS_CST",
                cClassTrib: "Classificacao",
                aliquotaIss: 5.00m,
                aliquotaIssRetido: 0.00m,
                aliquotaIrrfRetido: 1.50m,
                aliquotaInss: 11.00m,
                cstPisCofins: ECodigoSituacaoTributariaPisCofins.Cst01,
                aliquotaPis: 1.65m,
                aliquotaCofins: 7.60m,
                calcularRetencao: true,
                anexoSimplesNacional: EAnexoSimlesNacional.AnexoIII,
                tenantId: "tenant-123",
                criadoPor: "user-123"
            );

            // Assert
            Assert.True(servico.IsValid);
            Assert.Empty(servico.Notifications);
        }

        [Fact]
        public void CriarServico_Invalido_Deve_Retornar_Erro_Validao()
        {
            // Arrange & Act
            var servico = new Servico(
                empresaId: EmpresaId,
                unidadeMedidaId: Guid.Empty, // Inválido: vazio
                codigoServicoSefazId: Guid.NewGuid(),
                codigo: "", // Inválido: vazio
                descricao: "Consultoria em TI",
                valor: 150.00m,
                informacaoAdicional: "Desenvolvimento de Software",
                servicoAtivo: true,
                cnae: 0, // Inválido: <= 0
                codigoNbs: "102030",
                indicadorIss: true,
                indicadorIncentivo: false,
                cstIbsCbs: "IBS_CST",
                cClassTrib: "Classificacao",
                aliquotaIss: 5.00m,
                aliquotaIssRetido: 0.00m,
                aliquotaIrrfRetido: 1.50m,
                aliquotaInss: 11.00m,
                cstPisCofins: ECodigoSituacaoTributariaPisCofins.Cst01,
                aliquotaPis: 1.65m,
                aliquotaCofins: 7.60m,
                calcularRetencao: true,
                anexoSimplesNacional: EAnexoSimlesNacional.AnexoIII,
                tenantId: "tenant-123",
                criadoPor: "user-123"
            );

            // Assert
            Assert.False(servico.IsValid);
            Assert.Contains(servico.Notifications, n => n.Message.Contains("O ID da unidade de medida é obrigatório") || n.Message.Contains("O código é obrigatório") || n.Message.Contains("O código CNAE é obrigatório"));
        }

        [Fact]
        public async Task Handler_CriarServico_Deve_Persistir_No_Banco()
        {
            // Arrange
            var context = CreateInMemoryContext("db_handler_criar_servico");
            var tenantProvider = new TestTenantProvider("tenant-123");
            var currentUser = new TestCurrentUser("user-123");
            var handler = new CriarServicoCommandHandler(context, tenantProvider, currentUser);

            var command = new CriarServicoCommand(
                EmpresaId: EmpresaId,
                UnidadeMedidaId: Guid.NewGuid(),
                CodigoServicoSefazId: Guid.NewGuid(),
                Codigo: "SERV99",
                Descricao: "Manutenção Preventiva",
                Valor: 350.00m,
                InformacaoAdicional: "Limpeza física de hardware",
                ServicoAtivo: true,
                Cnae: 9511800,
                CodigoNbs: "2.01",
                IndicadorIss: true,
                IndicadorIncentivo: false,
                CstIbsCbs: null,
                CClassTrib: null,
                AliquotaIss: 2.00m,
                AliquotaIssRetido: 0.00m,
                AliquotaIrrfRetido: 0.00m,
                AliquotaInss: 0.00m,
                CstPisCofins: ECodigoSituacaoTributariaPisCofins.Cst07,
                AliquotaPis: 0.00m,
                AliquotaCofins: 0.00m,
                CalcularRetencao: false,
                AnexoSimplesNacional: EAnexoSimlesNacional.NaoSeAplica
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);
            var totalInDb = await context.Servicos.CountAsync();
            Assert.Equal(1, totalInDb);

            var servInDb = await context.Servicos.FirstAsync();
            Assert.Equal("Manutenção Preventiva", servInDb.Descricao);
            Assert.Equal(350.00m, servInDb.Valor);
        }

        [Fact]
        public async Task Handler_AtualizarServico_Deve_Alterar_No_Banco()
        {
            // Arrange
            var context = CreateInMemoryContext("db_handler_atualizar_servico");
            var tenantProvider = new TestTenantProvider("tenant-123");
            var currentUser = new TestCurrentUser("user-123");

            var servico = new Servico(
                empresaId: EmpresaId,
                unidadeMedidaId: Guid.NewGuid(),
                codigoServicoSefazId: Guid.NewGuid(),
                codigo: "SERV11",
                descricao: "Instalação Rede",
                valor: 500.00m,
                informacaoAdicional: null,
                servicoAtivo: true,
                cnae: 6190699,
                codigoNbs: "3.02",
                indicadorIss: true,
                indicadorIncentivo: false,
                cstIbsCbs: null,
                cClassTrib: null,
                aliquotaIss: 3.00m,
                aliquotaIssRetido: 0.00m,
                aliquotaIrrfRetido: 0.00m,
                aliquotaInss: 0.00m,
                cstPisCofins: ECodigoSituacaoTributariaPisCofins.Cst08,
                aliquotaPis: 0.00m,
                aliquotaCofins: 0.00m,
                calcularRetencao: false,
                anexoSimplesNacional: EAnexoSimlesNacional.AnexoIV,
                tenantId: "tenant-123",
                criadoPor: "user-123"
            );

            context.Servicos.Add(servico);
            await context.SaveChangesAsync();

            var handler = new AtualizarServicoCommandHandler(context, tenantProvider, currentUser);
            var command = new AtualizarServicoCommand(
                Id: servico.Id,
                UnidadeMedidaId: servico.UnidadeMedidaId,
                CodigoServicoSefazId: servico.CodigoServicoSefazId,
                Codigo: "SERV11_NEW",
                Descricao: "Instalação de Redes Fibra",
                Valor: 650.00m,
                InformacaoAdicional: "Acoplamento de conectores",
                ServicoAtivo: false,
                Cnae: 6190699,
                CodigoNbs: "3.02",
                IndicadorIss: true,
                IndicadorIncentivo: true,
                CstIbsCbs: "IBS_RET",
                CClassTrib: "ClasseT",
                AliquotaIss: 4.00m,
                AliquotaIssRetido: 1.00m,
                AliquotaIrrfRetido: 1.50m,
                AliquotaInss: 11.00m,
                CstPisCofins: ECodigoSituacaoTributariaPisCofins.Cst01,
                AliquotaPis: 1.65m,
                AliquotaCofins: 7.60m,
                CalcularRetencao: true,
                AnexoSimplesNacional: EAnexoSimlesNacional.AtividadeSujeitaAoFatorRAnexoIIIOuV
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);
            var servInDb = await context.Servicos.FirstAsync(x => x.Id == servico.Id);
            Assert.Equal("Instalação de Redes Fibra", servInDb.Descricao);
            Assert.Equal(650.00m, servInDb.Valor);
            Assert.False(servInDb.ServicoAtivo);
        }

        [Fact]
        public async Task Handler_DeletarServico_Deve_Realizar_Exclusao_Logica()
        {
            // Arrange
            var context = CreateInMemoryContext("db_handler_deletar_servico");
            var tenantProvider = new TestTenantProvider("tenant-123");
            var currentUser = new TestCurrentUser("user-123");

            var servico = new Servico(
                empresaId: EmpresaId,
                unidadeMedidaId: Guid.NewGuid(),
                codigoServicoSefazId: Guid.NewGuid(),
                codigo: "DEL",
                descricao: "Servico para Deletar",
                valor: 0.00m,
                informacaoAdicional: null,
                servicoAtivo: true,
                cnae: 1111,
                codigoNbs: "0",
                indicadorIss: false,
                indicadorIncentivo: false,
                cstIbsCbs: null,
                cClassTrib: null,
                aliquotaIss: 0.00m,
                aliquotaIssRetido: 0.00m,
                aliquotaIrrfRetido: 0.00m,
                aliquotaInss: 0.00m,
                cstPisCofins: ECodigoSituacaoTributariaPisCofins.Cst99,
                aliquotaPis: 0.00m,
                aliquotaCofins: 0.00m,
                calcularRetencao: false,
                anexoSimplesNacional: EAnexoSimlesNacional.NaoSeAplica,
                tenantId: "tenant-123",
                criadoPor: "user-123"
            );

            context.Servicos.Add(servico);
            await context.SaveChangesAsync();

            var handler = new DeletarServicoCommandHandler(context, tenantProvider, currentUser);
            var command = new DeletarServicoCommand(servico.Id);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);
            var servInDb = await context.Servicos.IgnoreQueryFilters().FirstAsync(x => x.Id == servico.Id);
            Assert.NotNull(servInDb.DeletadoEm);
            Assert.Equal("user-123", servInDb.AlteradoPor);
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
