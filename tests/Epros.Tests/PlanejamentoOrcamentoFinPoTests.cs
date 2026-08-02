using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Financeiro.Application.Commands;
using Epros.Modules.Financeiro.Application.Handlers;
using Epros.Modules.Financeiro.Application.Queries;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Modules.Financeiro.Infrastructure.Data;

namespace Epros.Tests
{
    /// <summary>
    /// FIN-PO — fechamento dos 4 gaps de Planejamento e Orçamento (auditoria EF×código):
    /// ciclo de vida do Budget (§6.5), cascata período→budgets (§6.4), milestones de meta (§6.7)
    /// e CRUD do orçamento comercial (§6.2/§8.2). Handlers exercitados direto sobre InMemory.
    /// </summary>
    public class PlanejamentoOrcamentoFinPoTests
    {
        private const string TenantId = "tenant-finpo";
        private const string UserId = "user-finpo";

        // ========== Gap 2 — Ciclo de vida do Budget (§6.5/§15) ==========
        [Fact(DisplayName = "FIN-PO | Budget: Rascunho→Aprovado→Ativo→Encerrado (ciclo completo)")]
        public async Task Budget_CicloDeVida_Completo()
        {
            var ctx = CreateContext(nameof(Budget_CicloDeVida_Completo));
            var (tp, cu) = Provs();
            var periodoId = await CriarPeriodoAsync(ctx, tp, cu);
            var budgetId = await CriarBudgetAsync(ctx, tp, cu, periodoId, "OPEX");

            Assert.True((await new AprovarBudgetCommandHandler(ctx, cu).Handle(new AprovarBudgetCommand(budgetId), default)).Sucesso);
            Assert.Equal(EStatusOrcamento.Aprovado, await StatusBudget(ctx, budgetId));

            Assert.True((await new AtivarBudgetCommandHandler(ctx, cu).Handle(new AtivarBudgetCommand(budgetId), default)).Sucesso);
            Assert.Equal(EStatusOrcamento.Ativo, await StatusBudget(ctx, budgetId));

            Assert.True((await new EncerrarBudgetCommandHandler(ctx, cu).Handle(new EncerrarBudgetCommand(budgetId), default)).Sucesso);
            Assert.Equal(EStatusOrcamento.Encerrado, await StatusBudget(ctx, budgetId));
        }

        [Fact(DisplayName = "FIN-PO | Budget: ativar exige Aprovado; encerrar exige Ativo (transições ilegais rejeitadas)")]
        public async Task Budget_TransicoesIlegais_Rejeitadas()
        {
            var ctx = CreateContext(nameof(Budget_TransicoesIlegais_Rejeitadas));
            var (tp, cu) = Provs();
            var periodoId = await CriarPeriodoAsync(ctx, tp, cu);
            var budgetId = await CriarBudgetAsync(ctx, tp, cu, periodoId, "OPEX");

            // Rascunho não pode ser ativado nem encerrado.
            Assert.False((await new AtivarBudgetCommandHandler(ctx, cu).Handle(new AtivarBudgetCommand(budgetId), default)).Sucesso);
            Assert.False((await new EncerrarBudgetCommandHandler(ctx, cu).Handle(new EncerrarBudgetCommand(budgetId), default)).Sucesso);
            Assert.Equal(EStatusOrcamento.Rascunho, await StatusBudget(ctx, budgetId));
        }

        [Fact(DisplayName = "FIN-PO | Budget: excluir só em Rascunho (§15) — fora de Rascunho é rejeitado")]
        public async Task Budget_Excluir_SomenteRascunho()
        {
            var ctx = CreateContext(nameof(Budget_Excluir_SomenteRascunho));
            var (tp, cu) = Provs();
            var periodoId = await CriarPeriodoAsync(ctx, tp, cu);

            // Rascunho → exclui (soft-delete).
            var b1 = await CriarBudgetAsync(ctx, tp, cu, periodoId, "OPEX");
            Assert.True((await new ExcluirBudgetCommandHandler(ctx, cu).Handle(new ExcluirBudgetCommand(b1), default)).Sucesso);
            Assert.Null(await ctx.Budgets.FirstOrDefaultAsync(x => x.Id == b1)); // filtro de soft-delete some
            var deletado = await ctx.Budgets.IgnoreQueryFilters().FirstAsync(x => x.Id == b1);
            Assert.NotNull(deletado.DeletadoEm);

            // Aprovado → exclusão rejeitada.
            var b2 = await CriarBudgetAsync(ctx, tp, cu, periodoId, "CAPEX");
            await new AprovarBudgetCommandHandler(ctx, cu).Handle(new AprovarBudgetCommand(b2), default);
            Assert.False((await new ExcluirBudgetCommandHandler(ctx, cu).Handle(new ExcluirBudgetCommand(b2), default)).Sucesso);
            Assert.Null(await ctx.Budgets.IgnoreQueryFilters().Where(x => x.Id == b2).Select(x => x.DeletadoEm).FirstAsync());
        }

        // ========== Gap 3 — Cascata período→budgets (§6.4/§15) ==========
        [Fact(DisplayName = "FIN-PO | Encerrar período encerra budgets filhos ativos (cascata §6.4)")]
        public async Task EncerrarPeriodo_CascataBudgetsAtivos()
        {
            var ctx = CreateContext(nameof(EncerrarPeriodo_CascataBudgetsAtivos));
            var (tp, cu) = Provs();
            var periodoId = await CriarPeriodoAsync(ctx, tp, cu);

            // Budget ativo (deve ser encerrado pela cascata).
            var ativo = await CriarBudgetAsync(ctx, tp, cu, periodoId, "OPEX");
            await new AprovarBudgetCommandHandler(ctx, cu).Handle(new AprovarBudgetCommand(ativo), default);
            await new AtivarBudgetCommandHandler(ctx, cu).Handle(new AtivarBudgetCommand(ativo), default);
            // Budget em rascunho (não deve ser tocado — não é encerrável).
            var rascunho = await CriarBudgetAsync(ctx, tp, cu, periodoId, "CAPEX");

            // Período: Rascunho→Aprovado→Ativo.
            await new AprovarPeriodoOrcamentarioCommandHandler(ctx, cu).Handle(new AprovarPeriodoOrcamentarioCommand(periodoId), default);
            await new AtivarPeriodoOrcamentarioCommandHandler(ctx, cu).Handle(new AtivarPeriodoOrcamentarioCommand(periodoId), default);

            var r = await new EncerrarPeriodoOrcamentarioCommandHandler(ctx, cu).Handle(new EncerrarPeriodoOrcamentarioCommand(periodoId), default);
            Assert.True(r.Sucesso);

            Assert.Equal(EStatusOrcamento.Encerrado, await StatusBudget(ctx, ativo));
            Assert.Equal(EStatusOrcamento.Rascunho, await StatusBudget(ctx, rascunho)); // intacto
            Assert.Equal(EStatusOrcamento.Encerrado,
                await ctx.PeriodosOrcamentarios.Where(p => p.Id == periodoId).Select(p => p.Status).FirstAsync());
        }

        // ========== Gap 4 — Milestones de meta (§6.7/§8.6) ==========
        [Fact(DisplayName = "FIN-PO | Milestone: criar em meta ativa (Pendente) e concluir (Concluído)")]
        public async Task Milestone_Criar_E_Concluir()
        {
            var ctx = CreateContext(nameof(Milestone_Criar_E_Concluir));
            var (tp, cu) = Provs();
            var metaId = await CriarMetaAtivaAsync(ctx, tp, cu);

            var criado = await new CriarMilestoneMetaCommandHandler(ctx, tp, cu).Handle(new CriarMilestoneMetaCommand(metaId, "Fase 1"), default);
            Assert.True(criado.Sucesso);
            var ms = await ctx.MetaMilestones.FirstAsync(m => m.MetaId == metaId);
            Assert.Equal(EStatusMilestone.Pendente, ms.Status);

            var concl = await new ConcluirMilestoneMetaCommandHandler(ctx, cu).Handle(new ConcluirMilestoneMetaCommand(ms.Id), default);
            Assert.True(concl.Sucesso);
            Assert.Equal(EStatusMilestone.Concluido,
                await ctx.MetaMilestones.Where(m => m.Id == ms.Id).Select(m => m.Status).FirstAsync());

            // Concluir de novo é rejeitado (não é mais Pendente).
            Assert.False((await new ConcluirMilestoneMetaCommandHandler(ctx, cu).Handle(new ConcluirMilestoneMetaCommand(ms.Id), default)).Sucesso);
        }

        [Fact(DisplayName = "FIN-PO | Milestone: rejeita criação em meta não ativa (§6.6)")]
        public async Task Milestone_RejeitaMetaNaoAtiva()
        {
            var ctx = CreateContext(nameof(Milestone_RejeitaMetaNaoAtiva));
            var (tp, cu) = Provs();
            var catId = await CriarCategoriaMetaAsync(ctx, tp, cu);
            var metaRes = await new CriarMetaCommandHandler(ctx, tp, cu).Handle(
                new CriarMetaCommand(catId, "Receita", "Alta", DateTime.UtcNow, DateTime.UtcNow.AddMonths(6)), default);
            var metaId = GetId(metaRes);

            var r = await new CriarMilestoneMetaCommandHandler(ctx, tp, cu).Handle(new CriarMilestoneMetaCommand(metaId, "X"), default);
            Assert.False(r.Sucesso); // meta em rascunho não aceita milestone
            Assert.Empty(await ctx.MetaMilestones.Where(m => m.MetaId == metaId).ToListAsync());
        }

        // ========== Gap 1 — Orçamento comercial (§6.2/§8.2/§12.3-12.4) ==========
        [Fact(DisplayName = "FIN-PO | Orçamento comercial: cria e calcula totais dos itens e do cabeçalho (§8.2)")]
        public async Task OrcamentoComercial_Criar_CalculaTotais()
        {
            var ctx = CreateContext(nameof(OrcamentoComercial_Criar_CalculaTotais));
            var (tp, cu) = Provs();

            var cmd = NovoOrcamentoCmd(itens: new[]
            {
                new OrcamentoComercialItemInput(Guid.NewGuid(), 2m, 100m, null),   // subtotal 200, total 200
                new OrcamentoComercialItemInput(Guid.NewGuid(), 1m, 50m, 10m),     // subtotal 50, desc 5, total 45
            }, valorFrete: 30m, taxaComissao: 5m, taxaDesconto: 10m);

            var r = await new CriarOrcamentoComercialCommandHandler(ctx, tp, cu).Handle(cmd, default);
            Assert.True(r.Sucesso);

            var orc = await ctx.OrcamentosComerciais.Include(o => o.Itens).FirstAsync();
            Assert.Equal(245m, orc.ValorSubtotal);            // 200 + 45
            Assert.Equal(24.50m, orc.ValorDesconto);          // 10% de 245
            Assert.Equal(12.25m, orc.ValorComissao);          // 5% de 245
            Assert.Equal(250.50m, orc.ValorTotal);            // 245 - 24,50 + 30
            Assert.Equal(2, orc.Itens.Count);
            Assert.Contains(orc.Itens, i => i.ValorTotal == 45m);
        }

        [Fact(DisplayName = "FIN-PO | Orçamento comercial: rejeita cabeçalho sem itens, sem cliente e item sem produto (§6.2)")]
        public async Task OrcamentoComercial_Validacoes()
        {
            var ctx = CreateContext(nameof(OrcamentoComercial_Validacoes));
            var (tp, cu) = Provs();

            // Sem itens.
            var semItens = NovoOrcamentoCmd(itens: Array.Empty<OrcamentoComercialItemInput>());
            Assert.False((await new CriarOrcamentoComercialCommandHandler(ctx, tp, cu).Handle(semItens, default)).Sucesso);

            // Sem cliente.
            var semCliente = NovoOrcamentoCmd(
                itens: new[] { new OrcamentoComercialItemInput(Guid.NewGuid(), 1m, 10m, null) }) with { ClienteId = Guid.Empty };
            Assert.False((await new CriarOrcamentoComercialCommandHandler(ctx, tp, cu).Handle(semCliente, default)).Sucesso);

            // Item sem produto.
            var itemSemProduto = NovoOrcamentoCmd(
                itens: new[] { new OrcamentoComercialItemInput(Guid.Empty, 1m, 10m, null) });
            Assert.False((await new CriarOrcamentoComercialCommandHandler(ctx, tp, cu).Handle(itemSemProduto, default)).Sucesso);

            Assert.Empty(await ctx.OrcamentosComerciais.ToListAsync());
        }

        [Fact(DisplayName = "FIN-PO | Orçamento comercial: alterar substitui itens e recalcula; excluir faz soft-delete")]
        public async Task OrcamentoComercial_Alterar_E_Excluir()
        {
            var db = nameof(OrcamentoComercial_Alterar_E_Excluir);
            var (tp, cu) = Provs();

            var criar = NovoOrcamentoCmd(itens: new[] { new OrcamentoComercialItemInput(Guid.NewGuid(), 1m, 100m, null) });
            Guid id;
            using (var ctx = CreateContext(db))
                id = GetId(await new CriarOrcamentoComercialCommandHandler(ctx, tp, cu).Handle(criar, default));

            var alterar = new AlterarOrcamentoComercialCommand(
                id, criar.VendedorId, criar.TransportadoraId, criar.ClienteId, criar.CondicaoPagamentoId,
                "PEDIDO", "ORC-1-ALT", "CIF", "obs alterada", "EmAndamento",
                null, null, null, null, null,
                new[] { new OrcamentoComercialItemInput(Guid.NewGuid(), 3m, 100m, null),
                        new OrcamentoComercialItemInput(Guid.NewGuid(), 1m, 100m, null) });
            using (var ctx = CreateContext(db))
                Assert.True((await new AlterarOrcamentoComercialCommandHandler(ctx, tp, cu).Handle(alterar, default)).Sucesso);

            using (var ctx = CreateContext(db))
            {
                var orc = await ctx.OrcamentosComerciais.Include(o => o.Itens).FirstAsync(o => o.Id == id);
                Assert.Equal(2, orc.Itens.Count);
                Assert.Equal(400m, orc.ValorSubtotal); // 300 + 100
                Assert.Equal("ORC-1-ALT", orc.Codigo);
            }

            using (var ctx = CreateContext(db))
                Assert.True((await new ExcluirOrcamentoComercialCommandHandler(ctx, cu).Handle(new ExcluirOrcamentoComercialCommand(id), default)).Sucesso);

            using (var ctx = CreateContext(db))
            {
                Assert.Null(await ctx.OrcamentosComerciais.FirstOrDefaultAsync(o => o.Id == id));
                Assert.NotNull(await ctx.OrcamentosComerciais.IgnoreQueryFilters().Where(o => o.Id == id).Select(o => o.DeletadoEm).FirstAsync());
            }
        }

        [Fact(DisplayName = "FIN-PO | Orçamento comercial: query lista e obtém por id com itens")]
        public async Task OrcamentoComercial_Queries()
        {
            var ctx = CreateContext(nameof(OrcamentoComercial_Queries));
            var (tp, cu) = Provs();
            var id = GetId(await new CriarOrcamentoComercialCommandHandler(ctx, tp, cu).Handle(
                NovoOrcamentoCmd(itens: new[] { new OrcamentoComercialItemInput(Guid.NewGuid(), 1m, 10m, null) }), default));

            var lista = await new PlanejamentoOrcamentoQueryHandlers(ctx).Handle(new ListarOrcamentosComerciaisQuery(), default);
            Assert.True(lista.Sucesso);

            var obt = await new PlanejamentoOrcamentoQueryHandlers(ctx).Handle(new ObterOrcamentoComercialPorIdQuery(id), default);
            Assert.True(obt.Sucesso);
            Assert.False((await new PlanejamentoOrcamentoQueryHandlers(ctx).Handle(new ObterOrcamentoComercialPorIdQuery(Guid.NewGuid()), default)).Sucesso);
        }

        // ---------- helpers ----------
        private static CriarOrcamentoComercialCommand NovoOrcamentoCmd(
            IReadOnlyList<OrcamentoComercialItemInput> itens,
            decimal? valorFrete = null, decimal? taxaComissao = null, decimal? taxaDesconto = null)
            => new CriarOrcamentoComercialCommand(
                VendedorId: Guid.NewGuid(), TransportadoraId: Guid.NewGuid(), ClienteId: Guid.NewGuid(),
                CondicaoPagamentoId: Guid.NewGuid(), Tipo: "PEDIDO", Codigo: "ORC-1", TipoFrete: "FOB",
                Observacao: "obs", StatusPedido: "Aberto",
                DataCadastro: null, DataEntrega: null, Validade: null,
                ValorFrete: valorFrete, TaxaComissao: taxaComissao, TaxaDesconto: taxaDesconto, Itens: itens);

        private static async Task<Guid> CriarPeriodoAsync(ContextFinanceiro ctx, ITenantProvider tp, ICurrentUser cu)
        {
            var r = await new CriarPeriodoOrcamentarioCommandHandler(ctx, tp, cu).Handle(
                new CriarPeriodoOrcamentarioCommand(DateTime.UtcNow, DateTime.UtcNow.AddMonths(12)), default);
            return GetId(r);
        }

        private static async Task<Guid> CriarBudgetAsync(ContextFinanceiro ctx, ITenantProvider tp, ICurrentUser cu, Guid periodoId, string tipo)
        {
            var r = await new CriarBudgetCommandHandler(ctx, tp, cu).Handle(new CriarBudgetCommand(periodoId, tipo, 1000m), default);
            return GetId(r);
        }

        private static async Task<Guid> CriarCategoriaMetaAsync(ContextFinanceiro ctx, ITenantProvider tp, ICurrentUser cu)
        {
            var r = await new CriarMetaCategoriaCommandHandler(ctx, tp, cu).Handle(
                new CriarMetaCategoriaCommand("Comercial", "COM", EEscopoMeta.Own), default);
            return GetId(r);
        }

        private static async Task<Guid> CriarMetaAtivaAsync(ContextFinanceiro ctx, ITenantProvider tp, ICurrentUser cu)
        {
            var catId = await CriarCategoriaMetaAsync(ctx, tp, cu);
            var metaId = GetId(await new CriarMetaCommandHandler(ctx, tp, cu).Handle(
                new CriarMetaCommand(catId, "Receita", "Alta", DateTime.UtcNow, DateTime.UtcNow.AddMonths(6)), default));
            await new AtivarMetaCommandHandler(ctx, cu).Handle(new AtivarMetaCommand(metaId), default);
            return metaId;
        }

        private static async Task<EStatusOrcamento> StatusBudget(ContextFinanceiro ctx, Guid id)
            => await ctx.Budgets.Where(b => b.Id == id).Select(b => b.Status).FirstAsync();

        private static Guid GetId(CommandResult r)
        {
            Assert.True(r.Sucesso, r.Mensagem + " " + string.Join("; ", r.Erros));
            return (Guid)r.Dados!.GetType().GetProperty("Id")!.GetValue(r.Dados)!;
        }

        private static (ITenantProvider, ICurrentUser) Provs() => (new TestTenantProvider(TenantId), new TestCurrentUser(UserId));

        private ContextFinanceiro CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ContextFinanceiro>().UseInMemoryDatabase("db_finpo_" + dbName).Options;
            return new ContextFinanceiro(options, new TestTenantProvider(TenantId), new TestCurrentUser(UserId));
        }

        private class TestTenantProvider : ITenantProvider
        {
            private readonly string _t; public TestTenantProvider(string t) => _t = t;
            public string GetTenantId() => _t;
        }

        private class TestCurrentUser : ICurrentUser
        {
            private readonly string _u; public TestCurrentUser(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "Test User";
            public string? GetUserEmail() => "test@epros.com";
        }
    }
}
