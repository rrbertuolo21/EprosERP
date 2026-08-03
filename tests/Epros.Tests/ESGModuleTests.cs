using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.ESG.Application.Commands;
using Epros.Modules.ESG.Application.Handlers;
using Epros.Modules.ESG.Application.EventHandlers;
using Epros.Modules.ESG.Application.Queries;
using Epros.Modules.ESG.Domain.Entities;
using Epros.Modules.ESG.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    public class ESGModuleTests
    {
        [Fact]
        public async Task Deve_Registrar_Emissao_Carbono_Manual()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextESG>()
                .UseInMemoryDatabase("db_esg_emissoes_manual")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextESG(options, tenantProvider, currentUser);

            var handler = new RegistrarEmissaoCommandHandler(context, tenantProvider, currentUser);
            // Consumo de 500 litros de diesel a 2.68 kg CO2e / litro = 1340 kg CO2e
            var command = new RegistrarEmissaoCommand("Frota Veículos - Diesel", 1, "CombustaoMovel", 500m, "Litro", 2.68m, DateTime.UtcNow);

            // Agir
            var result = await handler.Handle(command, CancellationToken.None);

            // Assertiva
            Assert.True(result.Sucesso);

            var emissoes = await context.EmissoesCarbono.ToListAsync();
            Assert.Single(emissoes);
            Assert.Equal("Frota Veículos - Diesel", emissoes[0].FonteEmissao);
            Assert.Equal(1, emissoes[0].Escopo);
            Assert.Equal(1340m, emissoes[0].TotalCo2e);
        }

        [Fact]
        public async Task Deve_Criar_E_Consolidar_Relatorio_ESG()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextESG>()
                .UseInMemoryDatabase("db_esg_relatorio_consolidado")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextESG(options, tenantProvider, currentUser);

            // Criar rascunho de relatório
            var criarHandler = new CriarRelatorioESGCommandHandler(context, tenantProvider, currentUser);
            var criarResult = await criarHandler.Handle(new CriarRelatorioESGCommand(2026, "Relatório Geral de Sustentabilidade 2026"), CancellationToken.None);
            Assert.True(criarResult.Sucesso);

            // Adicionar emissões do ano
            context.EmissoesCarbono.Add(new EmissaoCarbono("Frota", 1, "CombustaoMovel", 100m, "Litro", 2.5m, new DateTime(2026, 5, 10), "tenant-1", "user-1")); // 250
            context.EmissoesCarbono.Add(new EmissaoCarbono("Eletricidade", 2, "EnergiaEletrica", 1000m, "KWh", 0.12m, new DateTime(2026, 6, 12), "tenant-1", "user-1")); // 120
            context.EmissoesCarbono.Add(new EmissaoCarbono("Viagens Aéreas", 3, "ViagensNegocio", 10m, "Km", 0.5m, new DateTime(2026, 7, 15), "tenant-1", "user-1")); // 5
            await context.SaveChangesAsync();

            var consolidarHandler = new ConsolidarRelatorioESGCommandHandler(context, currentUser);

            // Agir
            var result = await consolidarHandler.Handle(new ConsolidarRelatorioESGCommand(2026), CancellationToken.None);

            // Assertiva
            Assert.True(result.Sucesso);

            var relatorio = await context.RelatoriosESG.FirstAsync(r => r.AnoFiscal == 2026);
            Assert.Equal("Rascunho", relatorio.Status);
            Assert.Equal(250m, relatorio.TotalEscopo1);
            Assert.Equal(120m, relatorio.TotalEscopo2);
            Assert.Equal(5m, relatorio.TotalEscopo3);
            Assert.Equal(375m, relatorio.TotalGeralCo2e);
        }

        [Fact]
        public async Task Deve_Gerar_Pegada_Scope3_Ao_Processar_CompraLancada()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextESG>()
                .UseInMemoryDatabase("db_esg_compra_integracao")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextESG(options, tenantProvider, currentUser);

            var handler = new CompraLancadaESGHandler(context);

            // NF-01/A-01: fator vem do catalogo versionado esg.ghg_fator_emissao (nao mais hardcoded 1.5).
            // valida-humano (base oficial GHG Protocol/IPCC/DEFRA) — aqui semeamos um fator de teste.
            context.FatoresEmissaoGee.Add(new FatorEmissaoGee(
                GhgFatorCodigos.BensAdquiridosPorPeca, "2026.1", "TESTE — base oficial pendente de homologacao",
                1.5m, "PC", "kgCO2e", new DateTime(2026, 1, 1), null, "tenant-1", "user-1"));
            await context.SaveChangesAsync();

            var itens = new List<CompraLancadaItemNotification>
            {
                new("SKU-1", "Aço Inox", 100m, 20m)
            };

            var notification = new CompraLancadaEventNotification(
                CompraId: Guid.NewGuid(),
                FornecedorId: Guid.NewGuid(),
                ValorTotal: 2000m,
                DataVencimento: DateTime.UtcNow.AddDays(30),
                NumeroNota: "NF-1234",
                TenantId: "tenant-1",
                UserId: "user-1",
                Itens: itens
            );

            // Agir
            await handler.Handle(notification, CancellationToken.None);

            // Assertiva
            var emissoes = await context.EmissoesCarbono.ToListAsync();
            Assert.Single(emissoes);
            Assert.Equal(3, emissoes[0].Escopo);
            Assert.Equal("BensEServicosAdquiridos", emissoes[0].CategoriaGhg);
            // 100 itens * 1.5 (fator do catalogo) = 150 kg CO2e
            Assert.Equal(150m, emissoes[0].TotalCo2e);
            Assert.False(emissoes[0].FatorPendente);
            Assert.Equal(GhgFatorCodigos.BensAdquiridosPorPeca, emissoes[0].FatorCodigo);
            Assert.Equal("2026.1", emissoes[0].FatorVersao);
            Assert.Contains("Aço Inox", emissoes[0].FonteEmissao);
        }

        [Fact]
        public async Task Compra_Sem_Fator_No_Catalogo_Fica_Pendente_Sem_Numero_Inventado()
        {
            // Regra #0 (NF-01/A-01): sem fator oficial vigente no catalogo, a emissao NAO recebe
            // numero inventado — entra como "pendente de fator" com TotalCo2e = 0.
            var options = new DbContextOptionsBuilder<ContextESG>()
                .UseInMemoryDatabase("db_esg_compra_pendente")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextESG(options, tenantProvider, currentUser);

            var handler = new CompraLancadaESGHandler(context);
            var notification = new CompraLancadaEventNotification(
                CompraId: Guid.NewGuid(), FornecedorId: Guid.NewGuid(), ValorTotal: 2000m,
                DataVencimento: DateTime.UtcNow.AddDays(30), NumeroNota: "NF-1", TenantId: "tenant-1",
                UserId: "user-1", Itens: new List<CompraLancadaItemNotification> { new("SKU-1", "Aço Inox", 100m, 20m) });

            await handler.Handle(notification, CancellationToken.None);

            var emissoes = await context.EmissoesCarbono.ToListAsync();
            Assert.Single(emissoes);
            Assert.True(emissoes[0].FatorPendente);
            Assert.Equal(0m, emissoes[0].TotalCo2e);       // Regra #0: nao inventa numero
            Assert.Equal(0m, emissoes[0].FatorEmissao);
            Assert.Equal(GhgFatorCodigos.BensAdquiridosPorPeca, emissoes[0].FatorCodigo);
            Assert.Null(emissoes[0].FatorVersao);
        }

        [Fact]
        public async Task Venda_Sem_Fator_No_Catalogo_Fica_Pendente_Sem_Numero_Inventado()
        {
            var options = new DbContextOptionsBuilder<ContextESG>()
                .UseInMemoryDatabase("db_esg_venda_pendente")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextESG(options, tenantProvider, currentUser);

            var handler = new VendaFaturadaESGHandler(context);
            var notification = new VendaFaturadaEventNotification(
                VendaId: Guid.NewGuid(), TenantId: "tenant-1", Total: 950m, CriadoEm: DateTime.UtcNow,
                Itens: new List<VendaFaturadaItemNotification> { new(Guid.NewGuid(), 50m, 10m), new(Guid.NewGuid(), 30m, 15m) },
                UserId: "user-1");

            await handler.Handle(notification, CancellationToken.None);

            var emissoes = await context.EmissoesCarbono.ToListAsync();
            Assert.Single(emissoes);
            Assert.True(emissoes[0].FatorPendente);
            Assert.Equal(0m, emissoes[0].TotalCo2e);       // Regra #0: nao inventa numero
            Assert.Equal(GhgFatorCodigos.TransporteDistribuicaoDownstreamPorPeca, emissoes[0].FatorCodigo);
        }

        [Fact]
        public async Task Deve_Gerar_Pegada_Scope3_Ao_Processar_VendaFaturada()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextESG>()
                .UseInMemoryDatabase("db_esg_venda_integracao")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextESG(options, tenantProvider, currentUser);

            var handler = new VendaFaturadaESGHandler(context);

            // NF-01/A-01: fator do catalogo versionado (nao mais hardcoded 0.8). valida-humano.
            context.FatoresEmissaoGee.Add(new FatorEmissaoGee(
                GhgFatorCodigos.TransporteDistribuicaoDownstreamPorPeca, "2026.1", "TESTE — base oficial pendente de homologacao",
                0.8m, "PC", "kgCO2e", new DateTime(2026, 1, 1), null, "tenant-1", "user-1"));
            await context.SaveChangesAsync();

            var itens = new List<VendaFaturadaItemNotification>
            {
                new(Guid.NewGuid(), 50m, 10m),
                new(Guid.NewGuid(), 30m, 15m)
            };

            var notification = new VendaFaturadaEventNotification(
                VendaId: Guid.NewGuid(),
                TenantId: "tenant-1",
                Total: 950m,
                CriadoEm: new DateTime(2026, 6, 1),
                Itens: itens,
                UserId: "user-1"
            );

            // Agir
            await handler.Handle(notification, CancellationToken.None);

            // Assertiva
            var emissoes = await context.EmissoesCarbono.ToListAsync();
            Assert.Single(emissoes);
            Assert.Equal(3, emissoes[0].Escopo);
            Assert.Equal("TransporteEDistribuicaoDownstream", emissoes[0].CategoriaGhg);
            // (50 + 30) itens * 0.8 (fator do catalogo) = 64 kg CO2e
            Assert.Equal(64m, emissoes[0].TotalCo2e);
            Assert.False(emissoes[0].FatorPendente);
            Assert.Equal("2026.1", emissoes[0].FatorVersao);
            Assert.Contains(notification.VendaId.ToString(), emissoes[0].FonteEmissao);
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
            public string? GetUserName() => "test_user";
            public string? GetUserEmail() => "test@epros.com.br";
        }
    }
}
