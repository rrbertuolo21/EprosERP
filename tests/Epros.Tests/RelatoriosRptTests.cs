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
    /// RPT-OPB — Relatorios Operacionais (Openbook). Testes de agregacao com dados semeados.
    /// </summary>
    public class RelatoriosRptTests
    {
        private const string Tenant = "tenant-rpt";
        private const string User = "user-rpt";

        private sealed class TP : ITenantProvider { public string GetTenantId() => Tenant; }
        private sealed class CU : ICurrentUser
        {
            public string? GetUserId() => User;
            public string? GetUserName() => User;
            public string? GetUserEmail() => "rpt@test.local";
        }

        private static ContextEstoque Estoque(string db)
            => new(new DbContextOptionsBuilder<ContextEstoque>().UseInMemoryDatabase(db).Options, new TP(), new CU());
        private static ContextVendas Vendas(string db)
            => new(new DbContextOptionsBuilder<ContextVendas>().UseInMemoryDatabase(db).Options, new TP(), new CU());
        private static ContextFinanceiro Financeiro(string db)
            => new(new DbContextOptionsBuilder<ContextFinanceiro>().UseInMemoryDatabase(db).Options, new TP(), new CU());

        // ------------------------------------------------------------------
        // ESTOQUE — posicao
        // ------------------------------------------------------------------
        [Fact]
        public async Task PosicaoEstoque_Deve_Totalizar_Saldo_E_Marcar_Abaixo_Do_Minimo()
        {
            var db = nameof(PosicaoEstoque_Deve_Totalizar_Saldo_E_Marcar_Abaixo_Do_Minimo);
            var ctx = Estoque(db);
            var categoria = new CategoriaProduto("Bebidas", Tenant, User);
            var catId = categoria.Id;
            ctx.Categorias.Add(categoria);
            var p1 = new Produto(catId, null, null, "SKU1", "Agua", "Sem GTIN", 0, 0, 10m, 10m, 5m,
                ETipoProduto.MateriaPrima, true, "", null, null, null, false, null, null, Tenant, User);
            var p2 = new Produto(null, null, null, "SKU2", "Suco", "Sem GTIN", 0, 0, 20m, 20m, 8m,
                ETipoProduto.MateriaPrima, true, "", null, null, null, false, null, null, Tenant, User);
            ctx.Produtos.AddRange(p1, p2);
            // p1: saldo 100 (min 10) ok ; p2: saldo 3 (min 5) abaixo
            ctx.EstoqueProdutos.Add(new EstoqueProduto(Guid.NewGuid(), p1.Id, 100m, 10m, 500m, 0m, 1000m, ETipoCusteioEstoque.CustoMedio, Tenant, User));
            ctx.EstoqueProdutos.Add(new EstoqueProduto(Guid.NewGuid(), p2.Id, 3m, 5m, 200m, 0m, 60m, ETipoCusteioEstoque.CustoMedio, Tenant, User));
            await ctx.SaveChangesAsync();

            var res = await new PosicaoEstoqueQueryHandler(ctx).Handle(new PosicaoEstoqueQuery(), CancellationToken.None);
            Assert.True(res.Sucesso);
            var dto = Assert.IsType<PosicaoEstoqueRelatorioDto>(res.Dados);
            Assert.Equal(2, dto.QuantidadeItens);
            Assert.Equal(103m, dto.TotalSaldoEstoque);
            Assert.Equal(1060m, dto.TotalValorSaldo);
            Assert.True(dto.Linhas.Single(l => l.ProdutoId == p2.Id).AbaixoDoMinimo);
            Assert.False(dto.Linhas.Single(l => l.ProdutoId == p1.Id).AbaixoDoMinimo);

            // filtro por categoria retorna so p1
            var res2 = await new PosicaoEstoqueQueryHandler(ctx).Handle(new PosicaoEstoqueQuery(CategoriaId: catId), CancellationToken.None);
            var dto2 = Assert.IsType<PosicaoEstoqueRelatorioDto>(res2.Dados);
            Assert.Single(dto2.Linhas);
            Assert.Equal(p1.Id, dto2.Linhas[0].ProdutoId);
            Assert.Equal("Bebidas", dto2.Linhas[0].Categoria);
        }

        [Fact]
        public async Task GiroEstoque_Deve_Somar_Saidas_E_Calcular_Giro()
        {
            var db = nameof(GiroEstoque_Deve_Somar_Saidas_E_Calcular_Giro);
            var ctx = Estoque(db);
            var prodId = Guid.NewGuid();
            ctx.Produtos.Add(new Produto(prodId, prodId, null, null, "P", "Sem GTIN", 0, 0, 1m, 1m, 0m,
                ETipoProduto.MateriaPrima, true, "", null, null, null, false, null, null, Tenant, User));
            ctx.MovimentosEstoque.Add(new MovimentoEstoque(prodId, 30m, "Saida", "venda", Tenant, User));
            ctx.MovimentosEstoque.Add(new MovimentoEstoque(prodId, 20m, "Saida", "venda", Tenant, User));
            ctx.MovimentosEstoque.Add(new MovimentoEstoque(prodId, 100m, "Entrada", "compra", Tenant, User));
            ctx.EstoqueProdutos.Add(new EstoqueProduto(Guid.NewGuid(), prodId, 50m, 0m, 0m, 0m, 0m, ETipoCusteioEstoque.CustoMedio, Tenant, User));
            await ctx.SaveChangesAsync();

            var res = await new GiroEstoqueQueryHandler(ctx).Handle(new GiroEstoqueQuery(), CancellationToken.None);
            var dto = Assert.IsType<GiroEstoqueRelatorioDto>(res.Dados);
            var linha = Assert.Single(dto.Linhas);
            Assert.Equal(50m, linha.QuantidadeSaida); // 30 + 20, entrada ignorada
            Assert.Equal(50m, linha.SaldoAtual);
            Assert.Equal(1m, linha.Giro); // 50/50
        }

        // ------------------------------------------------------------------
        // VENDAS
        // ------------------------------------------------------------------
        private static Venda NovaVenda(decimal total, DateTime data, Guid? clienteId = null)
            => new(Guid.NewGuid(), Guid.NewGuid(), "caixa-1", total, EVendaStatus.Transmitido, Tenant, User, data, clienteId: clienteId);

        [Fact]
        public async Task VendasPorPeriodo_Deve_Agrupar_Por_Dia_E_Excluir_Canceladas()
        {
            var db = nameof(VendasPorPeriodo_Deve_Agrupar_Por_Dia_E_Excluir_Canceladas);
            var ctx = Vendas(db);
            var d1 = new DateTime(2026, 5, 10, 12, 0, 0, DateTimeKind.Utc);
            var d2 = new DateTime(2026, 5, 11, 9, 0, 0, DateTimeKind.Utc);
            ctx.Vendas.Add(NovaVenda(100m, d1));
            ctx.Vendas.Add(NovaVenda(50m, d1));
            ctx.Vendas.Add(NovaVenda(70m, d2));
            var cancelada = NovaVenda(999m, d2);
            cancelada.Cancelar(User);
            ctx.Vendas.Add(cancelada);
            await ctx.SaveChangesAsync();

            var res = await new VendasPorPeriodoQueryHandler(ctx).Handle(new VendasPorPeriodoQuery(), CancellationToken.None);
            var dto = Assert.IsType<VendasPorPeriodoRelatorioDto>(res.Dados);
            Assert.Equal(3, dto.QuantidadeVendas);
            Assert.Equal(220m, dto.ValorTotal); // 100+50+70, cancelada fora
            Assert.Equal(2, dto.Linhas.Count);
            Assert.Equal(150m, dto.Linhas.Single(l => l.Dia == d1.Date).ValorTotal);
        }

        [Fact]
        public async Task VendasPorProduto_Deve_Somar_Itens_De_Vendas_Validas()
        {
            var db = nameof(VendasPorProduto_Deve_Somar_Itens_De_Vendas_Validas);
            var ctx = Vendas(db);
            var data = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc);
            var v = NovaVenda(0m, data);
            ctx.Vendas.Add(v);
            var prod = Guid.NewGuid();
            ctx.VendaItens.Add(new VendaItem(v.Id, prod, 2m, 10m, Tenant, User, descricaoProduto: "Cafe"));
            ctx.VendaItens.Add(new VendaItem(v.Id, prod, 3m, 10m, Tenant, User, descricaoProduto: "Cafe"));
            await ctx.SaveChangesAsync();

            var res = await new VendasPorProdutoQueryHandler(ctx).Handle(new VendasPorProdutoQuery(), CancellationToken.None);
            var dto = Assert.IsType<VendasPorProdutoRelatorioDto>(res.Dados);
            var linha = Assert.Single(dto.Linhas);
            Assert.Equal(prod, linha.ProdutoId);
            Assert.Equal(5m, linha.Quantidade);
            Assert.Equal(50m, linha.ValorTotal);
        }

        [Fact]
        public async Task VendasPorCliente_Deve_Agrupar_Por_Cliente()
        {
            var db = nameof(VendasPorCliente_Deve_Agrupar_Por_Cliente);
            var ctx = Vendas(db);
            var data = new DateTime(2026, 6, 5, 10, 0, 0, DateTimeKind.Utc);
            var cli = Guid.NewGuid();
            ctx.Vendas.Add(NovaVenda(80m, data, cli));
            ctx.Vendas.Add(NovaVenda(20m, data, cli));
            ctx.Vendas.Add(NovaVenda(15m, data, Guid.NewGuid()));
            await ctx.SaveChangesAsync();

            var res = await new VendasPorClienteQueryHandler(ctx).Handle(new VendasPorClienteQuery(ClienteId: cli), CancellationToken.None);
            var dto = Assert.IsType<VendasPorClienteRelatorioDto>(res.Dados);
            var linha = Assert.Single(dto.Linhas);
            Assert.Equal(2, linha.QuantidadeVendas);
            Assert.Equal(100m, linha.ValorTotal);
        }

        // ------------------------------------------------------------------
        // FINANCEIRO
        // ------------------------------------------------------------------
        private static ContasAReceber NovaCR(decimal valor, DateTime venc, ESituacao situacao = ESituacao.Aberto)
            => new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Cliente", situacao, venc,
                venc.AddDays(-30), null, "DOC", valor, 0m, 0m, 0m, 0m, 1, null, null, Tenant, User);

        private static ContasAPagar NovaCP(decimal valor, DateTime venc, ESituacao situacao = ESituacao.Aberto)
            => new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Fornecedor", situacao, venc,
                venc.AddDays(-30), null, "DOC", valor, 0m, 0m, 0m, 0m, 1, null, null, Tenant, User);

        [Fact]
        public async Task AgingContasReceber_Deve_Classificar_Por_Faixa_De_Vencimento()
        {
            var db = nameof(AgingContasReceber_Deve_Classificar_Por_Faixa_De_Vencimento);
            var ctx = Financeiro(db);
            var refDate = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
            ctx.ContasAReceberAgregado.Add(NovaCR(1000m, refDate.AddDays(10)));  // a vencer
            ctx.ContasAReceberAgregado.Add(NovaCR(200m, refDate.AddDays(-15)));  // 1-30
            ctx.ContasAReceberAgregado.Add(NovaCR(300m, refDate.AddDays(-45)));  // 31-60
            ctx.ContasAReceberAgregado.Add(NovaCR(400m, refDate.AddDays(-100))); // acima de 90
            await ctx.SaveChangesAsync();

            var res = await new AgingContasReceberQueryHandler(ctx).Handle(new AgingContasReceberQuery(refDate), CancellationToken.None);
            var dto = Assert.IsType<AgingRelatorioDto>(res.Dados);
            Assert.Equal("ContasAReceber", dto.Natureza);
            Assert.Equal(1900m, dto.SaldoTotal);
            Assert.Equal(900m, dto.SaldoVencido); // 200+300+400
            Assert.Equal(1000m, dto.SaldoAVencer);
            Assert.Equal(1000m, dto.Faixas.Single(f => f.Faixa == "A vencer").SaldoDevido);
            Assert.Equal(200m, dto.Faixas.Single(f => f.Faixa == "1-30").SaldoDevido);
            Assert.Equal(300m, dto.Faixas.Single(f => f.Faixa == "31-60").SaldoDevido);
            Assert.Equal(400m, dto.Faixas.Single(f => f.Faixa == "Acima de 90").SaldoDevido);
        }

        [Fact]
        public async Task AgingContasPagar_Deve_Ignorar_Cancelados()
        {
            var db = nameof(AgingContasPagar_Deve_Ignorar_Cancelados);
            var ctx = Financeiro(db);
            var refDate = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
            ctx.ContasAPagarAgregado.Add(NovaCP(500m, refDate.AddDays(-10))); // vencido 1-30
            var cancelado = NovaCP(999m, refDate.AddDays(-10), ESituacao.Cancelado);
            ctx.ContasAPagarAgregado.Add(cancelado);
            await ctx.SaveChangesAsync();

            var res = await new AgingContasPagarQueryHandler(ctx).Handle(new AgingContasPagarQuery(refDate), CancellationToken.None);
            var dto = Assert.IsType<AgingRelatorioDto>(res.Dados);
            Assert.Equal(500m, dto.SaldoTotal); // cancelado fora
            Assert.Equal(500m, dto.SaldoVencido);
        }

        [Fact]
        public async Task ResumoContasReceber_Deve_Marcar_Vencido()
        {
            var db = nameof(ResumoContasReceber_Deve_Marcar_Vencido);
            var ctx = Financeiro(db);
            var venc = DateTime.UtcNow.Date.AddDays(-5);
            ctx.ContasAReceberAgregado.Add(NovaCR(700m, venc));
            await ctx.SaveChangesAsync();

            var res = await new ResumoContasReceberQueryHandler(ctx).Handle(new ResumoContasReceberQuery(), CancellationToken.None);
            var dto = Assert.IsType<TitulosRelatorioDto>(res.Dados);
            var linha = Assert.Single(dto.Linhas);
            Assert.True(linha.Vencido);
            Assert.Equal(700m, dto.SaldoDevidoTotal);
        }

        [Fact]
        public async Task FluxoCaixa_Deve_Separar_Entradas_E_Saidas()
        {
            var db = nameof(FluxoCaixa_Deve_Separar_Entradas_E_Saidas);
            var ctx = Financeiro(db);
            var dia = new DateTime(2026, 4, 2, 8, 0, 0, DateTimeKind.Utc);
            ctx.MovimentosFinanceiros.Add(new MovimentoFinanceiro(null, dia, null, null, 1000m, 0m, null, null, null, null, false, Tenant, User));
            ctx.MovimentosFinanceiros.Add(new MovimentoFinanceiro(null, dia, null, null, 0m, 300m, null, null, null, null, false, Tenant, User));
            await ctx.SaveChangesAsync();

            var res = await new FluxoCaixaQueryHandler(ctx).Handle(new FluxoCaixaQuery(), CancellationToken.None);
            var dto = Assert.IsType<FluxoCaixaRelatorioDto>(res.Dados);
            Assert.Equal(1000m, dto.TotalEntradas);
            Assert.Equal(300m, dto.TotalSaidas);
            Assert.Equal(700m, dto.SaldoLiquido);
            Assert.Equal(700m, Assert.Single(dto.Linhas).Saldo);
        }
    }
}
