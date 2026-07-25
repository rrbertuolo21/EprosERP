using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Imobiliaria.Application.Commands;
using Epros.Modules.Imobiliaria.Application.Handlers;
using Epros.Modules.Imobiliaria.Application.Queries;
using Epros.Modules.Imobiliaria.Domain.Entities;
using Epros.Modules.Imobiliaria.Domain.Enums;
using Epros.Modules.Imobiliaria.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Testes das regras-chave do submodulo IMO-001 (Gestao Imobiliaria).
    /// InMemory DbContext, mesmo padrao de ESGModuleTests / DMSModuleTests.
    /// </summary>
    public class ImobiliariaModuleTests
    {
        private const string Tenant = "tenant-imo-001";
        private const string User = "user-imo-001";

        private static ContextImobiliaria NovoContexto(string dbName)
        {
            var options = new DbContextOptionsBuilder<ContextImobiliaria>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new ContextImobiliaria(options, new TestTenantProvider(Tenant), new TestCurrentUser(User));
        }

        // ==================== Dominio ====================

        [Fact(DisplayName = "Imovel | Sem proprietario e invalido (RN-001)")]
        public void Imovel_SemProprietario_Invalido()
        {
            var imovel = new Imovel("Sala 101", null, null, null, null, null, null, Tenant, User);
            imovel.Validar();
            Assert.False(imovel.IsValid);
        }

        [Fact(DisplayName = "Imovel | Com descricao e proprietario e valido (RN-001)")]
        public void Imovel_ComProprietario_Valido()
        {
            var imovel = new Imovel("Sala 101", null, "13000-000", "Rua A", "100", null, "Centro", Tenant, User);
            imovel.AdicionarProprietario(new ImovelProprietario(Guid.NewGuid(), Tenant, User));
            imovel.Validar();
            Assert.True(imovel.IsValid);
            Assert.Equal(EStatusImovel.EmCadastro, imovel.Status);
        }

        [Fact(DisplayName = "Imovel | Descricao vazia invalida (RN-001)")]
        public void Imovel_SemDescricao_Invalido()
        {
            var imovel = new Imovel("", null, null, null, null, null, null, Tenant, User);
            imovel.AdicionarProprietario(new ImovelProprietario(Guid.NewGuid(), Tenant, User));
            imovel.Validar();
            Assert.False(imovel.IsValid);
        }

        [Fact(DisplayName = "Locacao | Valor zero invalida (RN-012)")]
        public void Locacao_ValorZero_Invalido()
        {
            var loc = new Locacao(Guid.NewGuid(), new DateTime(2026, 1, 1), new DateTime(2026, 12, 31), 0, 10, Tenant, User);
            Assert.False(loc.IsValid);
        }

        [Fact(DisplayName = "Locacao | Fim antes do inicio invalida (RN-012)")]
        public void Locacao_PeriodoInvertido_Invalido()
        {
            var loc = new Locacao(Guid.NewGuid(), new DateTime(2026, 12, 31), new DateTime(2026, 1, 1), 1500, 10, Tenant, User);
            Assert.False(loc.IsValid);
        }

        [Fact(DisplayName = "Locacao | Formaliza de EmElaboracao para Vigente (RN-011)")]
        public void Locacao_Formalizar_Vigente()
        {
            var loc = new Locacao(Guid.NewGuid(), new DateTime(2026, 1, 1), new DateTime(2026, 12, 31), 1500, 10, Tenant, User);
            loc.AdicionarLocatario(Guid.NewGuid(), User);
            Assert.True(loc.IsValid);
            Assert.Equal(EStatusLocacao.EmElaboracao, loc.Status);

            loc.Formalizar(User);
            Assert.Equal(EStatusLocacao.Vigente, loc.Status);
        }

        [Fact(DisplayName = "Locacao | Partes N:N preservam locatarios e fiadores (RN-013)")]
        public void Locacao_PartesNN_Preservadas()
        {
            var loc = new Locacao(Guid.NewGuid(), new DateTime(2026, 1, 1), new DateTime(2026, 12, 31), 1500, 10, Tenant, User);
            loc.AdicionarLocatario(Guid.NewGuid(), User);
            loc.AdicionarLocatario(Guid.NewGuid(), User);
            loc.AdicionarFiador(Guid.NewGuid(), User);

            Assert.Equal(2, loc.Locatarios.Count());
            Assert.Single(loc.Fiadores);
        }

        [Fact(DisplayName = "ContratoServico | Sem proprietario invalido (RN-008)")]
        public void ContratoServico_SemProprietario_Invalido()
        {
            var contrato = new ContratoServico(Guid.Empty, null, null, null, null, null, Tenant, User);
            Assert.False(contrato.IsValid);
        }

        // ==================== Handlers (CQRS) ====================

        [Fact(DisplayName = "CriarImovel | Persiste imovel e agregados atomicamente (RN-002)")]
        public async Task CriarImovel_Handler_Persiste()
        {
            var ctx = NovoContexto(nameof(CriarImovel_Handler_Persiste));
            var handler = new CriarImovelCommandHandler(ctx, new TestTenantProvider(Tenant), new TestCurrentUser(User));

            var cmd = new CriarImovelCommand(
                "Apto 202", null, "13000-000", "Rua B", "50", null, "Centro",
                new List<ProprietarioInput> { new(Guid.NewGuid()) },
                new List<VistoriaInput> { new("Cozinha", "Sem avarias", DateTime.UtcNow) },
                new List<CustoImovelInput> { new("IPTU", 350m, DateTime.UtcNow) });

            var result = await handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.Sucesso);
            Assert.Equal(1, await ctx.Imoveis.CountAsync());
            Assert.Equal(1, await ctx.ImovelProprietarios.CountAsync());
            Assert.Equal(1, await ctx.ImovelVistorias.CountAsync());
            Assert.Equal(1, await ctx.ImovelCustos.CountAsync());
        }

        [Fact(DisplayName = "CriarImovel | Sem proprietario retorna falha (RN-001)")]
        public async Task CriarImovel_SemProprietario_Falha()
        {
            var ctx = NovoContexto(nameof(CriarImovel_SemProprietario_Falha));
            var handler = new CriarImovelCommandHandler(ctx, new TestTenantProvider(Tenant), new TestCurrentUser(User));

            var cmd = new CriarImovelCommand(
                "Apto sem dono", null, null, null, null, null, null,
                new List<ProprietarioInput>(), null, null);

            var result = await handler.Handle(cmd, CancellationToken.None);

            Assert.False(result.Sucesso);
            Assert.Equal(0, await ctx.Imoveis.CountAsync());
        }

        [Fact(DisplayName = "CriarLocacao | Persiste locacao com partes (RN-011/RN-013)")]
        public async Task CriarLocacao_Handler_Persiste()
        {
            var ctx = NovoContexto(nameof(CriarLocacao_Handler_Persiste));
            var handler = new CriarLocacaoCommandHandler(ctx, new TestTenantProvider(Tenant), new TestCurrentUser(User));

            var cmd = new CriarLocacaoCommand(
                Guid.NewGuid(), new DateTime(2026, 1, 1), new DateTime(2026, 12, 31), 1800m, 5,
                new List<Guid> { Guid.NewGuid() }, new List<Guid> { Guid.NewGuid() });

            var result = await handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.Sucesso);
            Assert.Equal(1, await ctx.Locacoes.CountAsync());
            Assert.Equal(2, await ctx.LocacaoPartes.CountAsync());
        }

        [Fact(DisplayName = "ObterResumoAluguel | Retorna dados para localizar recebivel (RN-015)")]
        public async Task ObterResumoAluguel_Handler()
        {
            var ctx = NovoContexto(nameof(ObterResumoAluguel_Handler));
            var criar = new CriarLocacaoCommandHandler(ctx, new TestTenantProvider(Tenant), new TestCurrentUser(User));
            var criarResult = await criar.Handle(new CriarLocacaoCommand(
                Guid.NewGuid(), new DateTime(2026, 1, 1), new DateTime(2026, 12, 31), 1800m, 5,
                new List<Guid> { Guid.NewGuid() }, null), CancellationToken.None);

            var locacaoId = (Guid)criarResult.Dados!.GetType().GetProperty("LocacaoId")!.GetValue(criarResult.Dados)!;

            var query = new ObterResumoAluguelQueryHandler(ctx);
            var result = await query.Handle(new ObterResumoAluguelQuery(locacaoId), CancellationToken.None);

            Assert.True(result.Sucesso);
            Assert.NotNull(result.Dados);
        }

        [Fact(DisplayName = "CriarContratoServico | Persiste contrato com proprietario (RN-008)")]
        public async Task CriarContratoServico_Handler_Persiste()
        {
            var ctx = NovoContexto(nameof(CriarContratoServico_Handler_Persiste));
            var handler = new CriarContratoServicoCommandHandler(ctx, new TestTenantProvider(Tenant), new TestCurrentUser(User));

            var result = await handler.Handle(new CriarContratoServicoCommand(
                Guid.NewGuid(), Guid.NewGuid(), "Administracao mensal", null, null, 300m), CancellationToken.None);

            Assert.True(result.Sucesso);
            Assert.Equal(1, await ctx.ContratosServico.CountAsync());
        }

        private class TestTenantProvider : ITenantProvider
        {
            private readonly string _tenant;
            public TestTenantProvider(string tenant) => _tenant = tenant;
            public string GetTenantId() => _tenant;
        }

        private class TestCurrentUser : ICurrentUser
        {
            private readonly string _user;
            public TestCurrentUser(string user) => _user = user;
            public string? GetUserId() => _user;
            public string? GetUserName() => _user;
            public string? GetUserEmail() => $"{_user}@epros.local";
        }
    }
}
