using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.ESG.Application.Commands;
using Epros.Modules.ESG.Application.Handlers;
using Epros.Modules.ESG.Domain.Entities;
using Epros.Modules.ESG.Domain.Enums;
using Epros.Modules.ESG.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Testes das regras-chave dos submodulos ESG-GHG, ESG-EHS, ESG-REL e ESG-ECO.
    /// InMemory DbContext, mesmo padrao de ESGModuleTests.
    /// </summary>
    public class ESGSubmodulosTests
    {
        private const string Tenant = "tenant-esg-001";
        private const string User = "user-esg-001";

        private static ContextESG NovoContexto(string dbName)
        {
            var options = new DbContextOptionsBuilder<ContextESG>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new ContextESG(options, new TestTenantProvider(Tenant), new TestCurrentUser(User));
        }

        // ==================== ESG-GHG ====================

        [Fact(DisplayName = "GHG | Inventario inicia em Rascunho e transita ate Ativo")]
        public void Inventario_Workflow_Rascunho_EmAnalise_Ativo()
        {
            var inv = new InventarioGee("INV-2026", 1, new DateTime(2026, 1, 1), new DateTime(2026, 12, 31),
                "Controle operacional", "GHG Protocol", Guid.NewGuid(), null, Tenant, User);
            Assert.True(inv.IsValid);
            Assert.Equal(EStatusWorkflowEsg.Rascunho, inv.Status);

            inv.Submeter(User);
            Assert.Equal(EStatusWorkflowEsg.EmAnalise, inv.Status);

            inv.Aprovar(User);
            Assert.Equal(EStatusWorkflowEsg.Ativo, inv.Status);
        }

        [Fact(DisplayName = "GHG | Aprovar inventario em Rascunho gera notificacao")]
        public void Inventario_Aprovar_EmRascunho_Falha()
        {
            var inv = new InventarioGee("INV-X", 1, new DateTime(2026, 1, 1), new DateTime(2026, 12, 31),
                "Fronteira", "Metodo", Guid.NewGuid(), null, Tenant, User);
            inv.Aprovar(User);
            Assert.False(inv.IsValid);
        }

        [Fact(DisplayName = "GHG | Periodo com fim antes do inicio invalida inventario")]
        public void Inventario_PeriodoInvalido_Falha()
        {
            var inv = new InventarioGee("INV-P", 1, new DateTime(2026, 12, 31), new DateTime(2026, 1, 1),
                "Fronteira", "Metodo", Guid.NewGuid(), null, Tenant, User);
            Assert.False(inv.IsValid);
        }

        [Fact(DisplayName = "GHG | Calculo bloqueia fator fora de vigencia na data do dado")]
        public async Task Calculo_FatorForaVigencia_Falha()
        {
            using var ctx = NovoContexto("db_ghg_calculo_vigencia");
            var tp = new TestTenantProvider(Tenant);
            var cu = new TestCurrentUser(User);

            var fonte = new FonteEmissaoGee(Guid.NewGuid(), "F1", "Frota", 1, "CombustaoMovel", Guid.NewGuid(), Tenant, User);
            ctx.FontesEmissaoGee.Add(fonte);
            var dado = new DadoAtividadeGee(fonte.Id, new DateTime(2026, 6, 1), 100m, "Litro", EOrigemDadoGhg.Manual, null, Tenant, User);
            ctx.DadosAtividadeGee.Add(dado);
            // Fator vigente apenas em 2025 -> nao cobre a data 2026-06-01.
            var fator = new FatorEmissaoGee("FE1", "v1", "GHG", 2.5m, "Litro", "kgCO2e", new DateTime(2025, 1, 1), new DateTime(2025, 12, 31), Tenant, User);
            ctx.FatoresEmissaoGee.Add(fator);
            await ctx.SaveChangesAsync();

            var handler = new CalcularEmissaoGeeCommandHandler(ctx, tp, cu);
            var res = await handler.Handle(new CalcularEmissaoGeeCommand(dado.Id, fator.Id, "f-v1"), CancellationToken.None);
            Assert.False(res.Sucesso);
        }

        [Fact(DisplayName = "GHG | Consolidar soma CO2e por escopo")]
        public async Task Consolidar_SomaPorEscopo()
        {
            using var ctx = NovoContexto("db_ghg_consolidar");
            var tp = new TestTenantProvider(Tenant);
            var cu = new TestCurrentUser(User);

            var inv = new InventarioGee("INV-C", 1, new DateTime(2026, 1, 1), new DateTime(2026, 12, 31), "Fronteira", "Metodo", Guid.NewGuid(), null, Tenant, User);
            ctx.InventariosGee.Add(inv);
            var f1 = new FonteEmissaoGee(inv.Id, "F1", "Frota", 1, "Movel", Guid.NewGuid(), Tenant, User);
            var f2 = new FonteEmissaoGee(inv.Id, "F2", "Energia", 2, "Eletrica", Guid.NewGuid(), Tenant, User);
            ctx.FontesEmissaoGee.AddRange(f1, f2);
            var d1 = new DadoAtividadeGee(f1.Id, new DateTime(2026, 5, 1), 100m, "Litro", EOrigemDadoGhg.Manual, null, Tenant, User);
            var d2 = new DadoAtividadeGee(f2.Id, new DateTime(2026, 5, 1), 1000m, "KWh", EOrigemDadoGhg.Manual, null, Tenant, User);
            ctx.DadosAtividadeGee.AddRange(d1, d2);
            // Calculo: quantidade * fator
            ctx.CalculosGee.Add(new CalculoGee(d1.Id, Guid.NewGuid(), "v1", 100m, 2.5m, Tenant, User)); // 250 escopo1
            ctx.CalculosGee.Add(new CalculoGee(d2.Id, Guid.NewGuid(), "v1", 1000m, 0.12m, Tenant, User)); // 120 escopo2
            await ctx.SaveChangesAsync();

            var handler = new ConsolidarInventarioGeeCommandHandler(ctx, tp, cu);
            var res = await handler.Handle(new ConsolidarInventarioGeeCommand(inv.Id), CancellationToken.None);
            Assert.True(res.Sucesso);

            var consolidacoes = await ctx.ConsolidacoesGee.Where(c => c.InventarioId == inv.Id).ToListAsync();
            Assert.Equal(250m, consolidacoes.Single(c => c.Dimensao == "Escopo1").TotalCO2e);
            Assert.Equal(120m, consolidacoes.Single(c => c.Dimensao == "Escopo2").TotalCO2e);
            Assert.Equal(370m, consolidacoes.Single(c => c.Dimensao == "TotalGeral").TotalCO2e);
        }

        // ==================== ESG-EHS ====================

        [Fact(DisplayName = "EHS | Atividade exige descricao (RN-EHS-009)")]
        public void Atividade_SemDescricao_Falha()
        {
            var atv = new AtividadeOcupacional(Guid.NewGuid(), null, null, null, "", Tenant, User);
            Assert.False(atv.IsValid);
        }

        [Fact(DisplayName = "EHS | Atividade com fim antes do inicio invalida (RN-EHS-008)")]
        public void Atividade_PeriodoInvalido_Falha()
        {
            var atv = new AtividadeOcupacional(Guid.NewGuid(), null, new DateTime(2026, 5, 1), new DateTime(2026, 1, 1), "Solda", Tenant, User);
            Assert.False(atv.IsValid);
        }

        [Fact(DisplayName = "EHS | Fator de risco exige Tipo, FatorRisco, Intensidade e Tecnica (RN-EHS-010)")]
        public void FatorRisco_CamposObrigatorios_Falha()
        {
            var fr = new FatorRisco(Guid.NewGuid(), null, null, null, "", "", "", "", "Sim", "Sim", null, "S", "S", "S", "S", "S", Tenant, User);
            Assert.False(fr.IsValid);
        }

        [Fact(DisplayName = "EHS | Registro EHS duplicado por codigo e bloqueado")]
        public async Task RegistroEhs_CodigoDuplicado_Falha()
        {
            using var ctx = NovoContexto("db_ehs_registro_dup");
            var tp = new TestTenantProvider(Tenant);
            var cu = new TestCurrentUser(User);
            var handler = new CriarRegistroEhsCommandHandler(ctx, tp, cu);

            var cmd = new CriarRegistroEhsCommand("REG-1", "Licenca de operacao", "Licenca", Guid.NewGuid(), null);
            var r1 = await handler.Handle(cmd, CancellationToken.None);
            Assert.True(r1.Sucesso);
            var r2 = await handler.Handle(cmd, CancellationToken.None);
            Assert.False(r2.Sucesso);
        }

        [Fact(DisplayName = "EHS | Licenca com validade anterior a emissao e bloqueada")]
        public void Licenca_ValidadeInvalida_Falha()
        {
            var lic = new LicencaAmbiental(Guid.NewGuid(), "LO", "123", "IBAMA", new DateTime(2026, 6, 1), new DateTime(2026, 1, 1), Guid.NewGuid(), null, Tenant, User);
            Assert.False(lic.IsValid);
        }

        // ==================== ESG-REL ====================

        [Fact(DisplayName = "REL | Snapshot exige valor numerico ou textual")]
        public void Snapshot_SemValor_Falha()
        {
            var snap = new SnapshotIndicador(Guid.NewGuid(), "v1", DateTime.UtcNow, null, null, null, null, "Ativo", Tenant, User);
            Assert.False(snap.IsValid);
        }

        [Fact(DisplayName = "REL | Snapshot gera hash SHA-256 deterministico e verificavel (TE-02)")]
        public void Snapshot_Hash_Sha256_Deterministico()
        {
            var id = Guid.NewGuid();
            var data = new DateTime(2026, 6, 30, 0, 0, 0, DateTimeKind.Utc);
            var a = new SnapshotIndicador(id, "v1", data, 123.45m, null, "tCO2e", "uf=SP", "Ativo", Tenant, User);
            var b = new SnapshotIndicador(id, "v1", data, 123.45m, null, "tCO2e", "uf=SP", "Ativo", Tenant, User);

            Assert.Equal(64, a.HashConteudo.Length);                 // SHA-256 hex
            Assert.Equal(a.HashConteudo, b.HashConteudo);            // deterministico (mesmo conteudo)
            Assert.True(a.HashConfere());                            // RN-REL-013: verificavel
            Assert.Matches("^[0-9a-f]{64}$", a.HashConteudo);        // hex SHA-256 puro (nao GetHashCode X8)

            // Conteudo diferente -> hash diferente (deteccao de adulteracao)
            var c = new SnapshotIndicador(id, "v1", data, 123.46m, null, "tCO2e", "uf=SP", "Ativo", Tenant, User);
            Assert.NotEqual(a.HashConteudo, c.HashConteudo);
        }

        [Fact(DisplayName = "REL | Item com sequencia duplicada no mesmo relatorio e bloqueado")]
        public async Task Item_SequenciaDuplicada_Falha()
        {
            using var ctx = NovoContexto("db_rel_item_seq");
            var tp = new TestTenantProvider(Tenant);
            var cu = new TestCurrentUser(User);

            var relatorio = new RelatorioESG(2026, "Relatorio 2026", Tenant, User);
            ctx.RelatoriosESG.Add(relatorio);
            await ctx.SaveChangesAsync();

            var handler = new AdicionarItemRelatorioCommandHandler(ctx, tp, cu);
            var r1 = await handler.Handle(new AdicionarItemRelatorioCommand(relatorio.Id, 1, null, "Item A", null, "Narrativo", null), CancellationToken.None);
            Assert.True(r1.Sucesso);
            var r2 = await handler.Handle(new AdicionarItemRelatorioCommand(relatorio.Id, 1, null, "Item B", null, "Narrativo", null), CancellationToken.None);
            Assert.False(r2.Sucesso);
        }

        [Fact(DisplayName = "REL | Framework + requisito criados com sucesso")]
        public async Task Framework_ComRequisito_Sucesso()
        {
            using var ctx = NovoContexto("db_rel_framework");
            var tp = new TestTenantProvider(Tenant);
            var cu = new TestCurrentUser(User);

            var fwHandler = new CriarFrameworkRelCommandHandler(ctx, tp, cu);
            var fwRes = await fwHandler.Handle(new CriarFrameworkRelCommand("GRI", "2021", "Global Reporting Initiative", null, null), CancellationToken.None);
            Assert.True(fwRes.Sucesso);

            var framework = await ctx.FrameworksRel.FirstAsync();
            var reqHandler = new AdicionarRequisitoRelCommandHandler(ctx, tp, cu);
            var reqRes = await reqHandler.Handle(new AdicionarRequisitoRelCommand(framework.Id, "GRI-305", "Emissoes", "Quantitativo", true, 1), CancellationToken.None);
            Assert.True(reqRes.Sucesso);
        }

        // ==================== ESG-ECO ====================

        [Fact(DisplayName = "ECO | Devolucao com chave duplicada e bloqueada (RN-ECO)")]
        public async Task Devolucao_ChaveDuplicada_Falha()
        {
            using var ctx = NovoContexto("db_eco_dev_dup");
            var tp = new TestTenantProvider(Tenant);
            var cu = new TestCurrentUser(User);
            var handler = new ImportarDevolucaoCommandHandler(ctx, tp, cu);

            var cmd = new ImportarDevolucaoCommand(null, null, 100m, 100m, "Avaria", null, null, false,
                "35260600000000000000000000000000000000000001", "123", null, null, null, null, "Entrada",
                new List<DevolucaoItemInput> { new("P1", "Produto 1", "12345678", "1202", 10m, 5m, false, "UN") });

            var r1 = await handler.Handle(cmd, CancellationToken.None);
            Assert.True(r1.Sucesso);
            var r2 = await handler.Handle(cmd, CancellationToken.None);
            Assert.False(r2.Sucesso);
        }

        [Fact(DisplayName = "ECO | Devolucao importa itens preservando quantidade")]
        public async Task Devolucao_ImportaItens()
        {
            using var ctx = NovoContexto("db_eco_dev_itens");
            var tp = new TestTenantProvider(Tenant);
            var cu = new TestCurrentUser(User);
            var handler = new ImportarDevolucaoCommandHandler(ctx, tp, cu);

            var cmd = new ImportarDevolucaoCommand(null, null, 200m, 200m, "Retorno", null, null, true, null, "555", 10m, 5m, null, null, "Entrada",
                new List<DevolucaoItemInput>
                {
                    new("A", "Item A", "1111", "1202", 10m, 2m, false, "UN"),
                    new("B", "Item B", "2222", "1202", 20m, 3m, true, "UN")
                });

            var res = await handler.Handle(cmd, CancellationToken.None);
            Assert.True(res.Sucesso);
            Assert.Equal(2, await ctx.DevolucoesItensEco.CountAsync());
        }

        [Fact(DisplayName = "ECO | Destino nao pode exceder quantidade triada")]
        public async Task Destino_ExcedeTriagem_Falha()
        {
            using var ctx = NovoContexto("db_eco_destino_excede");
            var tp = new TestTenantProvider(Tenant);
            var cu = new TestCurrentUser(User);

            var fluxo = new FluxoCircular("FLX-1", "Reciclagem plastico", "Reciclagem", null, Guid.NewGuid(), Tenant, User);
            ctx.FluxosCirculares.Add(fluxo);
            var triagem = new TriagemEco(fluxo.Id, null, 10m, "KG", "Bom", "Reciclagem", null, Guid.NewGuid(), Tenant, User);
            ctx.TriagensEco.Add(triagem);
            await ctx.SaveChangesAsync();

            var handler = new RegistrarDestinoCommandHandler(ctx, tp, cu);
            var ok = await handler.Handle(new RegistrarDestinoCommand(triagem.Id, "Reciclagem", 8m, "KG", DateTime.UtcNow, Guid.NewGuid(), null, null), CancellationToken.None);
            Assert.True(ok.Sucesso);
            var excede = await handler.Handle(new RegistrarDestinoCommand(triagem.Id, "Reuso", 5m, "KG", DateTime.UtcNow, Guid.NewGuid(), null, null), CancellationToken.None);
            Assert.False(excede.Sucesso);
        }

        [Fact(DisplayName = "ECO | Medicao calcula KPI numerador/denominador")]
        public async Task Medicao_CalculaKpi()
        {
            using var ctx = NovoContexto("db_eco_medicao_kpi");
            var tp = new TestTenantProvider(Tenant);
            var cu = new TestCurrentUser(User);

            var fluxo = new FluxoCircular("FLX-2", "Recuperacao", "Recuperacao", null, Guid.NewGuid(), Tenant, User);
            ctx.FluxosCirculares.Add(fluxo);
            await ctx.SaveChangesAsync();

            var handler = new RegistrarMedicaoCircularCommandHandler(ctx, tp, cu);
            var res = await handler.Handle(new RegistrarMedicaoCircularCommand(fluxo.Id, "ConteudoReciclado", "2026-Q1", 30m, 120m, "%", "MRP"), CancellationToken.None);
            Assert.True(res.Sucesso);

            var medicao = await ctx.MedicoesCirculares.FirstAsync();
            Assert.Equal(0.25m, medicao.Valor);
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
