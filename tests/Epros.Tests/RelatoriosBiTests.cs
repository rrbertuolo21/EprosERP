using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Modules.Financeiro.Domain.Entities;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Modules.Relatorios.Application.Dtos;
using Epros.Modules.Relatorios.Application.Handlers;
using Epros.Modules.Relatorios.Application.Queries;
using Epros.Modules.Vendas.Domain.Entities;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;
using EVendaStatus = Epros.Modules.Vendas.Domain.Enums.EVendaStatus;

namespace Epros.Tests
{
    /// <summary>
    /// RPT-ONM — BI (OneManager). Testes de KPIs e series com dados semeados.
    /// </summary>
    public class RelatoriosBiTests
    {
        private const string Tenant = "tenant-bi";
        private const string User = "user-bi";

        private sealed class TP : ITenantProvider { public string GetTenantId() => Tenant; }
        private sealed class CU : ICurrentUser
        {
            public string? GetUserId() => User;
            public string? GetUserName() => User;
            public string? GetUserEmail() => "bi@test.local";
        }

        private static ContextEstoque Estoque(string db)
            => new(new DbContextOptionsBuilder<ContextEstoque>().UseInMemoryDatabase(db).Options, new TP(), new CU());
        private static ContextVendas Vendas(string db)
            => new(new DbContextOptionsBuilder<ContextVendas>().UseInMemoryDatabase(db).Options, new TP(), new CU());
        private static ContextFinanceiro Financeiro(string db)
            => new(new DbContextOptionsBuilder<ContextFinanceiro>().UseInMemoryDatabase(db).Options, new TP(), new CU());

        private static Venda NovaVenda(decimal total, DateTime data)
            => new(Guid.NewGuid(), Guid.NewGuid(), "caixa-1", total, EVendaStatus.Transmitido, Tenant, User, data);

        private static ContasAReceber NovaCR(decimal valor, DateTime venc)
            => new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Cliente", ESituacao.Aberto, venc,
                venc.AddDays(-30), null, "DOC", valor, 0m, 0m, 0m, 0m, 1, null, null, Tenant, User);

        [Fact]
        public async Task KpiFaturamento_Deve_Somar_Vendas_Nao_Canceladas_No_Periodo()
        {
            var db = nameof(KpiFaturamento_Deve_Somar_Vendas_Nao_Canceladas_No_Periodo);
            var ctx = Vendas(db);
            var dentro = new DateTime(2026, 3, 10, 10, 0, 0, DateTimeKind.Utc);
            var fora = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            ctx.Vendas.Add(NovaVenda(100m, dentro));
            ctx.Vendas.Add(NovaVenda(250m, dentro));
            ctx.Vendas.Add(NovaVenda(999m, fora));
            var cancelada = NovaVenda(500m, dentro); cancelada.Cancelar(User);
            ctx.Vendas.Add(cancelada);
            await ctx.SaveChangesAsync();

            var inicio = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);
            var fim = new DateTime(2026, 3, 31, 23, 59, 59, DateTimeKind.Utc);
            var res = await new KpiFaturamentoQueryHandler(ctx).Handle(new KpiFaturamentoQuery(inicio, fim), CancellationToken.None);
            var dto = Assert.IsType<KpiValorDto>(res.Dados);
            Assert.Equal(350m, dto.Valor); // 100+250, cancelada e fora do periodo excluidas
            Assert.Equal("BRL", dto.Unidade);
        }

        [Fact]
        public async Task SerieFaturamento_Mensal_Deve_Agrupar_Por_Mes()
        {
            var db = nameof(SerieFaturamento_Mensal_Deve_Agrupar_Por_Mes);
            var ctx = Vendas(db);
            ctx.Vendas.Add(NovaVenda(100m, new DateTime(2026, 2, 5, 0, 0, 0, DateTimeKind.Utc)));
            ctx.Vendas.Add(NovaVenda(40m, new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc)));
            ctx.Vendas.Add(NovaVenda(70m, new DateTime(2026, 3, 3, 0, 0, 0, DateTimeKind.Utc)));
            await ctx.SaveChangesAsync();

            var res = await new SerieFaturamentoQueryHandler(ctx).Handle(new SerieFaturamentoQuery(Granularidade: "Mes"), CancellationToken.None);
            var dto = Assert.IsType<SerieTemporalDto>(res.Dados);
            Assert.Equal("Mes", dto.Granularidade);
            Assert.Equal(2, dto.Pontos.Count);
            Assert.Equal(140m, dto.Pontos.Single(p => p.Periodo.Month == 2).Valor);
            Assert.Equal(210m, dto.Total);
        }

        [Fact]
        public async Task KpiMargem_Deve_Calcular_Receita_Custo_E_Percentual()
        {
            var db = nameof(KpiMargem_Deve_Calcular_Receita_Custo_E_Percentual);
            var ctx = Vendas(db);
            var data = new DateTime(2026, 3, 10, 10, 0, 0, DateTimeKind.Utc);
            var v = NovaVenda(0m, data);
            ctx.Vendas.Add(v);
            // item: qtd 2, preco 10 => receita 20 ; custo unit 6 => custo 12
            ctx.VendaItens.Add(new VendaItem(v.Id, Guid.NewGuid(), 2m, 10m, Tenant, User, descricaoProduto: "X", valorCusto: 6m));
            await ctx.SaveChangesAsync();

            var res = await new KpiMargemQueryHandler(ctx).Handle(new KpiMargemQuery(), CancellationToken.None);
            var dto = Assert.IsType<MargemKpiDto>(res.Dados);
            Assert.Equal(20m, dto.Receita);
            Assert.Equal(12m, dto.Custo);
            Assert.Equal(8m, dto.MargemValor);
            Assert.Equal(40m, dto.MargemPercentual);
        }

        [Fact]
        public async Task KpiInadimplencia_Deve_Calcular_Percentual_Vencido()
        {
            var db = nameof(KpiInadimplencia_Deve_Calcular_Percentual_Vencido);
            var ctx = Financeiro(db);
            var refDate = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
            ctx.ContasAReceberAgregado.Add(NovaCR(300m, refDate.AddDays(-10))); // vencido
            ctx.ContasAReceberAgregado.Add(NovaCR(700m, refDate.AddDays(10)));  // a vencer
            await ctx.SaveChangesAsync();

            var res = await new KpiInadimplenciaQueryHandler(ctx).Handle(new KpiInadimplenciaQuery(refDate), CancellationToken.None);
            var dto = Assert.IsType<InadimplenciaKpiDto>(res.Dados);
            Assert.Equal(1000m, dto.SaldoTotal);
            Assert.Equal(300m, dto.SaldoVencido);
            Assert.Equal(30m, dto.PercentualInadimplencia);
        }

        [Fact]
        public async Task KpiRuptura_Deve_Contar_Itens_Zerados_E_Abaixo_Do_Minimo()
        {
            var db = nameof(KpiRuptura_Deve_Contar_Itens_Zerados_E_Abaixo_Do_Minimo);
            var ctx = Estoque(db);
            ctx.EstoqueProdutos.Add(new EstoqueProduto(Guid.NewGuid(), Guid.NewGuid(), 0m, 5m, 100m, 0m, 0m, ETipoCusteioEstoque.CustoMedio, Tenant, User));   // ruptura
            ctx.EstoqueProdutos.Add(new EstoqueProduto(Guid.NewGuid(), Guid.NewGuid(), 3m, 5m, 100m, 0m, 0m, ETipoCusteioEstoque.CustoMedio, Tenant, User));   // abaixo min
            ctx.EstoqueProdutos.Add(new EstoqueProduto(Guid.NewGuid(), Guid.NewGuid(), 50m, 5m, 100m, 0m, 0m, ETipoCusteioEstoque.CustoMedio, Tenant, User));  // ok
            ctx.EstoqueProdutos.Add(new EstoqueProduto(Guid.NewGuid(), Guid.NewGuid(), 0m, 5m, 100m, 0m, 0m, ETipoCusteioEstoque.CustoMedio, Tenant, User));   // ruptura
            await ctx.SaveChangesAsync();

            var res = await new KpiRupturaQueryHandler(ctx).Handle(new KpiRupturaQuery(), CancellationToken.None);
            var dto = Assert.IsType<RupturaKpiDto>(res.Dados);
            Assert.Equal(4, dto.TotalItens);
            Assert.Equal(2, dto.ItensEmRuptura);
            Assert.Equal(1, dto.ItensAbaixoMinimo);
            Assert.Equal(50m, dto.PercentualRuptura); // 2/4
        }

        [Fact]
        public async Task TopProdutos_Deve_Ordenar_Por_Valor_E_Limitar()
        {
            var db = nameof(TopProdutos_Deve_Ordenar_Por_Valor_E_Limitar);
            var ctx = Vendas(db);
            var data = new DateTime(2026, 3, 10, 10, 0, 0, DateTimeKind.Utc);
            var v = NovaVenda(0m, data);
            ctx.Vendas.Add(v);
            var pA = Guid.NewGuid(); var pB = Guid.NewGuid(); var pC = Guid.NewGuid();
            ctx.VendaItens.Add(new VendaItem(v.Id, pA, 1m, 10m, Tenant, User, descricaoProduto: "A"));   // 10
            ctx.VendaItens.Add(new VendaItem(v.Id, pB, 1m, 100m, Tenant, User, descricaoProduto: "B"));  // 100
            ctx.VendaItens.Add(new VendaItem(v.Id, pC, 1m, 50m, Tenant, User, descricaoProduto: "C"));   // 50
            await ctx.SaveChangesAsync();

            var res = await new TopProdutosQueryHandler(ctx).Handle(new TopProdutosQuery(TopN: 2, Criterio: "Valor"), CancellationToken.None);
            var dto = Assert.IsType<TopProdutosDto>(res.Dados);
            Assert.Equal(2, dto.Itens.Count);
            Assert.Equal(pB, dto.Itens[0].ProdutoId);
            Assert.Equal(pC, dto.Itens[1].ProdutoId);
        }

        [Fact]
        public async Task PainelGerencial_Deve_Consolidar_Multi_Modulo()
        {
            var seed = Guid.NewGuid().ToString("N");
            var ctxV = Vendas("painel-v-" + seed);
            var ctxF = Financeiro("painel-f-" + seed);
            var ctxE = Estoque("painel-e-" + seed);
            var inicio = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);
            var fim = new DateTime(2026, 3, 31, 23, 59, 59, DateTimeKind.Utc);
            var data = new DateTime(2026, 3, 10, 10, 0, 0, DateTimeKind.Utc);

            var v = NovaVenda(200m, data);
            ctxV.Vendas.Add(v);
            ctxV.VendaItens.Add(new VendaItem(v.Id, Guid.NewGuid(), 2m, 100m, Tenant, User, descricaoProduto: "P", valorCusto: 60m));
            await ctxV.SaveChangesAsync();

            ctxF.ContasAReceberAgregado.Add(NovaCR(400m, fim.AddDays(-40))); // vencido
            ctxF.ContasAReceberAgregado.Add(NovaCR(600m, fim.AddDays(20)));  // a vencer
            ctxF.MovimentosFinanceiros.Add(new MovimentoFinanceiro(null, data, null, null, 1000m, 200m, null, null, null, null, false, Tenant, User));
            await ctxF.SaveChangesAsync();

            ctxE.EstoqueProdutos.Add(new EstoqueProduto(Guid.NewGuid(), Guid.NewGuid(), 0m, 5m, 100m, 0m, 0m, ETipoCusteioEstoque.CustoMedio, Tenant, User));
            ctxE.EstoqueProdutos.Add(new EstoqueProduto(Guid.NewGuid(), Guid.NewGuid(), 50m, 5m, 100m, 0m, 0m, ETipoCusteioEstoque.CustoMedio, Tenant, User));
            await ctxE.SaveChangesAsync();

            var res = await new PainelGerencialQueryHandler(ctxV, ctxF, ctxE).Handle(new PainelGerencialQuery(inicio, fim), CancellationToken.None);
            var dto = Assert.IsType<PainelGerencialDto>(res.Dados);
            Assert.Equal(200m, dto.FaturamentoPeriodo);
            Assert.Equal(200m, dto.Margem.Receita);
            Assert.Equal(120m, dto.Margem.Custo);           // 60 * 2
            Assert.Equal(40m, dto.Inadimplencia.PercentualInadimplencia); // 400/1000
            Assert.Equal(800m, dto.SaldoCaixaLiquido);      // 1000 - 200
            Assert.Equal(1, dto.Ruptura.ItensEmRuptura);
            Assert.Equal(2, dto.Ruptura.TotalItens);
        }
    }
}
