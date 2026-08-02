using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Modules.Relatorios.Application.Dtos;
using Epros.Modules.Relatorios.Application.Queries;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Relatorios.Application.Handlers
{
    // =====================================================================
    // RPT-OPB — Handlers dos relatorios operacionais (read-side).
    // Convencao: aplicar filtros no servidor (Where), materializar e agregar
    // em memoria — garante paridade entre Npgsql (prod) e InMemory (testes) e
    // nao depende de traducao de GroupBy por data. Nenhum estado e alterado.
    // O isolamento por empresa/tenant vem do global query filter do ContextBase.
    // =====================================================================

    // ---- ESTOQUE: posicao -------------------------------------------------
    public sealed class PosicaoEstoqueQueryHandler : IQueryHandler<PosicaoEstoqueQuery, CommandResult>
    {
        private readonly ContextEstoque _ctx;
        public PosicaoEstoqueQueryHandler(ContextEstoque ctx) => _ctx = ctx;

        public async Task<CommandResult> Handle(PosicaoEstoqueQuery q, CancellationToken ct)
        {
            var produtos = await _ctx.Produtos.AsNoTracking().ToListAsync(ct);
            var categorias = await _ctx.Categorias.AsNoTracking().ToListAsync(ct);
            var estoques = await _ctx.EstoqueProdutos.AsNoTracking().ToListAsync(ct);

            var catNome = categorias.ToDictionary(c => c.Id, c => c.Descricao);
            var prodById = produtos.ToDictionary(p => p.Id);

            var linhas = new List<PosicaoEstoqueLinhaDto>();
            foreach (var e in estoques)
            {
                if (q.ProdutoId.HasValue && q.ProdutoId.Value != Guid.Empty && e.ProdutoId != q.ProdutoId.Value)
                    continue;

                prodById.TryGetValue(e.ProdutoId, out var p);
                var categoriaId = p?.CategoriaId;

                if (q.CategoriaId.HasValue && q.CategoriaId.Value != Guid.Empty && categoriaId != q.CategoriaId.Value)
                    continue;

                var abaixo = e.QuantidadeSaldoEstoque < e.QuantidadeEstoqueMinimo;
                if (q.SomenteAbaixoMinimo && !abaixo)
                    continue;

                linhas.Add(new PosicaoEstoqueLinhaDto(
                    ProdutoId: e.ProdutoId,
                    Sku: p?.Sku ?? string.Empty,
                    Produto: p?.Nome ?? string.Empty,
                    CategoriaId: categoriaId,
                    Categoria: categoriaId.HasValue && catNome.TryGetValue(categoriaId.Value, out var cn) ? cn : null,
                    SaldoEstoque: e.QuantidadeSaldoEstoque,
                    EstoqueMinimo: e.QuantidadeEstoqueMinimo,
                    EstoqueReservado: e.QuantidadeEstoqueReservado,
                    ValorSaldo: e.ValorSaldo,
                    CustoMedio: e.ValorCustoMedio,
                    AbaixoDoMinimo: abaixo));
            }

            linhas = linhas.OrderBy(l => l.Produto).ToList();
            var dto = new PosicaoEstoqueRelatorioDto(
                linhas,
                TotalSaldoEstoque: linhas.Sum(l => l.SaldoEstoque),
                TotalValorSaldo: linhas.Sum(l => l.ValorSaldo),
                QuantidadeItens: linhas.Count);

            return CommandResult.Ok("Posicao de estoque consultada com sucesso.", dto);
        }
    }

    // ---- ESTOQUE: giro ----------------------------------------------------
    public sealed class GiroEstoqueQueryHandler : IQueryHandler<GiroEstoqueQuery, CommandResult>
    {
        private readonly ContextEstoque _ctx;
        public GiroEstoqueQueryHandler(ContextEstoque ctx) => _ctx = ctx;

        public async Task<CommandResult> Handle(GiroEstoqueQuery q, CancellationToken ct)
        {
            var movs = await _ctx.MovimentosEstoque.AsNoTracking().ToListAsync(ct);
            var estoques = await _ctx.EstoqueProdutos.AsNoTracking().ToListAsync(ct);
            var produtos = await _ctx.Produtos.AsNoTracking().ToListAsync(ct);
            var prodById = produtos.ToDictionary(p => p.Id, p => p.Nome);
            var saldoById = estoques.GroupBy(e => e.ProdutoId)
                                    .ToDictionary(g => g.Key, g => g.Sum(x => x.QuantidadeSaldoEstoque));

            var saidas = movs
                .Where(m => string.Equals(m.Tipo, "Saida", StringComparison.OrdinalIgnoreCase))
                .Where(m => !q.ProdutoId.HasValue || q.ProdutoId.Value == Guid.Empty || m.ProdutoId == q.ProdutoId.Value)
                .Where(m => (!q.Inicio.HasValue || m.CriadoEm >= q.Inicio.Value)
                         && (!q.Fim.HasValue || m.CriadoEm <= q.Fim.Value))
                .GroupBy(m => m.ProdutoId)
                .Select(g =>
                {
                    var qtdSaida = g.Sum(x => x.Quantidade);
                    var saldo = saldoById.TryGetValue(g.Key, out var s) ? s : 0m;
                    var giro = saldo > 0 ? qtdSaida / saldo : 0m;
                    return new GiroEstoqueLinhaDto(
                        g.Key,
                        prodById.TryGetValue(g.Key, out var n) ? n : string.Empty,
                        qtdSaida,
                        saldo,
                        Math.Round(giro, 4));
                })
                .OrderByDescending(l => l.QuantidadeSaida)
                .ToList();

            var dto = new GiroEstoqueRelatorioDto(saidas, saidas.Sum(l => l.QuantidadeSaida));
            return CommandResult.Ok("Giro de estoque consultado com sucesso.", dto);
        }
    }

    // ---- VENDAS: por periodo ---------------------------------------------
    public sealed class VendasPorPeriodoQueryHandler : IQueryHandler<VendasPorPeriodoQuery, CommandResult>
    {
        private readonly ContextVendas _ctx;
        public VendasPorPeriodoQueryHandler(ContextVendas ctx) => _ctx = ctx;

        public async Task<CommandResult> Handle(VendasPorPeriodoQuery q, CancellationToken ct)
        {
            var vendas = await _ctx.Vendas.AsNoTracking()
                .Where(v => q.IncluirCanceladas || !v.Cancelada)
                .Where(v => (!q.Inicio.HasValue || v.DataVenda >= q.Inicio.Value)
                         && (!q.Fim.HasValue || v.DataVenda <= q.Fim.Value))
                .ToListAsync(ct);

            var linhas = vendas
                .GroupBy(v => v.DataVenda.Date)
                .Select(g => new VendasPorPeriodoLinhaDto(g.Key, g.Count(), g.Sum(x => x.Total)))
                .OrderBy(l => l.Dia)
                .ToList();

            var dto = new VendasPorPeriodoRelatorioDto(linhas, vendas.Sum(v => v.Total), vendas.Count);
            return CommandResult.Ok("Vendas por periodo consultadas com sucesso.", dto);
        }
    }

    // ---- VENDAS: por produto ---------------------------------------------
    public sealed class VendasPorProdutoQueryHandler : IQueryHandler<VendasPorProdutoQuery, CommandResult>
    {
        private readonly ContextVendas _ctx;
        public VendasPorProdutoQueryHandler(ContextVendas ctx) => _ctx = ctx;

        public async Task<CommandResult> Handle(VendasPorProdutoQuery q, CancellationToken ct)
        {
            // vendas validas do periodo
            var vendasValidas = await _ctx.Vendas.AsNoTracking()
                .Where(v => q.IncluirCanceladas || !v.Cancelada)
                .Where(v => (!q.Inicio.HasValue || v.DataVenda >= q.Inicio.Value)
                         && (!q.Fim.HasValue || v.DataVenda <= q.Fim.Value))
                .Select(v => v.Id)
                .ToListAsync(ct);
            var vendaIds = vendasValidas.ToHashSet();

            var itens = await _ctx.VendaItens.AsNoTracking()
                .Where(i => !q.ProdutoId.HasValue || q.ProdutoId.Value == Guid.Empty || i.ProdutoId == q.ProdutoId.Value)
                .ToListAsync(ct);

            var linhas = itens
                .Where(i => vendaIds.Contains(i.VendaId))
                .GroupBy(i => new { i.ProdutoId, i.DescricaoProduto })
                .Select(g => new VendasPorProdutoLinhaDto(
                    g.Key.ProdutoId,
                    g.Key.DescricaoProduto ?? string.Empty,
                    g.Sum(x => x.Quantidade),
                    g.Sum(x => x.ValorTotal)))
                .OrderByDescending(l => l.ValorTotal)
                .ToList();

            var dto = new VendasPorProdutoRelatorioDto(linhas, linhas.Sum(l => l.ValorTotal));
            return CommandResult.Ok("Vendas por produto consultadas com sucesso.", dto);
        }
    }

    // ---- VENDAS: por cliente ---------------------------------------------
    public sealed class VendasPorClienteQueryHandler : IQueryHandler<VendasPorClienteQuery, CommandResult>
    {
        private readonly ContextVendas _ctx;
        public VendasPorClienteQueryHandler(ContextVendas ctx) => _ctx = ctx;

        public async Task<CommandResult> Handle(VendasPorClienteQuery q, CancellationToken ct)
        {
            var vendas = await _ctx.Vendas.AsNoTracking()
                .Where(v => q.IncluirCanceladas || !v.Cancelada)
                .Where(v => (!q.Inicio.HasValue || v.DataVenda >= q.Inicio.Value)
                         && (!q.Fim.HasValue || v.DataVenda <= q.Fim.Value))
                .Where(v => !q.ClienteId.HasValue || q.ClienteId.Value == Guid.Empty || v.ClienteId == q.ClienteId.Value)
                .ToListAsync(ct);

            var linhas = vendas
                .GroupBy(v => v.ClienteId)
                .Select(g => new VendasPorClienteLinhaDto(g.Key, g.Count(), g.Sum(x => x.Total)))
                .OrderByDescending(l => l.ValorTotal)
                .ToList();

            var dto = new VendasPorClienteRelatorioDto(linhas, vendas.Sum(v => v.Total));
            return CommandResult.Ok("Vendas por cliente consultadas com sucesso.", dto);
        }
    }

    // ---- COMPRAS: por item -----------------------------------------------
    public sealed class ComprasPorItemQueryHandler : IQueryHandler<ComprasPorItemQuery, CommandResult>
    {
        private readonly ContextEstoque _ctx;
        public ComprasPorItemQueryHandler(ContextEstoque ctx) => _ctx = ctx;

        public async Task<CommandResult> Handle(ComprasPorItemQuery q, CancellationToken ct)
        {
            var comprasValidas = await _ctx.Compras.AsNoTracking()
                .Where(c => q.IncluirCanceladas || !c.Cancelada)
                .Where(c => (!q.Inicio.HasValue || c.DataCompra >= q.Inicio.Value)
                         && (!q.Fim.HasValue || c.DataCompra <= q.Fim.Value))
                .Select(c => c.Id)
                .ToListAsync(ct);
            var compraIds = comprasValidas.ToHashSet();

            var itens = await _ctx.CompraItens.AsNoTracking()
                .Where(i => !q.ProdutoId.HasValue || q.ProdutoId.Value == Guid.Empty || i.ProdutoId == q.ProdutoId.Value)
                .ToListAsync(ct);

            var linhas = itens
                .Where(i => compraIds.Contains(i.CompraId))
                .GroupBy(i => new { i.ProdutoId, i.DescricaoProduto })
                .Select(g => new ComprasPorItemLinhaDto(
                    g.Key.ProdutoId,
                    g.Key.DescricaoProduto ?? string.Empty,
                    g.Sum(x => x.Quantidade),
                    g.Sum(x => x.ValorTotal)))
                .OrderByDescending(l => l.ValorTotal)
                .ToList();

            var dto = new ComprasPorItemRelatorioDto(linhas, linhas.Sum(l => l.ValorTotal));
            return CommandResult.Ok("Compras por item consultadas com sucesso.", dto);
        }
    }

    // ---- COMPRAS: por parceiro -------------------------------------------
    public sealed class ComprasPorParceiroQueryHandler : IQueryHandler<ComprasPorParceiroQuery, CommandResult>
    {
        private readonly ContextEstoque _ctx;
        public ComprasPorParceiroQueryHandler(ContextEstoque ctx) => _ctx = ctx;

        public async Task<CommandResult> Handle(ComprasPorParceiroQuery q, CancellationToken ct)
        {
            var compras = await _ctx.Compras.AsNoTracking()
                .Where(c => q.IncluirCanceladas || !c.Cancelada)
                .Where(c => (!q.Inicio.HasValue || c.DataCompra >= q.Inicio.Value)
                         && (!q.Fim.HasValue || c.DataCompra <= q.Fim.Value))
                .Where(c => string.IsNullOrEmpty(q.FornecedorCnpj) || c.FornecedorCnpj == q.FornecedorCnpj)
                .ToListAsync(ct);

            var linhas = compras
                .GroupBy(c => new { c.FornecedorCnpj, c.FornecedorNome })
                .Select(g => new ComprasPorParceiroLinhaDto(
                    g.Key.FornecedorCnpj ?? string.Empty,
                    g.Key.FornecedorNome ?? string.Empty,
                    g.Count(),
                    g.Sum(x => x.ValorTotal)))
                .OrderByDescending(l => l.ValorTotal)
                .ToList();

            var dto = new ComprasPorParceiroRelatorioDto(linhas, compras.Sum(c => c.ValorTotal));
            return CommandResult.Ok("Compras por parceiro consultadas com sucesso.", dto);
        }
    }

    // ---- FINANCEIRO: aging CR --------------------------------------------
    public sealed class AgingContasReceberQueryHandler : IQueryHandler<AgingContasReceberQuery, CommandResult>
    {
        private readonly ContextFinanceiro _ctx;
        public AgingContasReceberQueryHandler(ContextFinanceiro ctx) => _ctx = ctx;

        public async Task<CommandResult> Handle(AgingContasReceberQuery q, CancellationToken ct)
        {
            var titulos = await _ctx.ContasAReceberAgregado.AsNoTracking()
                .Where(t => t.Situacao != ESituacao.Cancelado)
                .ToListAsync(ct);

            var refDate = (q.DataReferencia ?? DateTime.UtcNow).Date;
            var registros = titulos.Select(t => (t.DataVencimento, Saldo: t.ValorTotalAReceberTitulo));
            var dto = AgingCalculo.Calcular("ContasAReceber", refDate, registros);
            return CommandResult.Ok("Aging de contas a receber consultado com sucesso.", dto);
        }
    }

    // ---- FINANCEIRO: aging CP --------------------------------------------
    public sealed class AgingContasPagarQueryHandler : IQueryHandler<AgingContasPagarQuery, CommandResult>
    {
        private readonly ContextFinanceiro _ctx;
        public AgingContasPagarQueryHandler(ContextFinanceiro ctx) => _ctx = ctx;

        public async Task<CommandResult> Handle(AgingContasPagarQuery q, CancellationToken ct)
        {
            var titulos = await _ctx.ContasAPagarAgregado.AsNoTracking()
                .Where(t => t.Situacao != ESituacao.Cancelado)
                .ToListAsync(ct);

            var refDate = (q.DataReferencia ?? DateTime.UtcNow).Date;
            var registros = titulos.Select(t => (t.DataVencimento, Saldo: t.ValorTotalAPagarTitulo));
            var dto = AgingCalculo.Calcular("ContasAPagar", refDate, registros);
            return CommandResult.Ok("Aging de contas a pagar consultado com sucesso.", dto);
        }
    }

    // ---- FINANCEIRO: resumo CR -------------------------------------------
    public sealed class ResumoContasReceberQueryHandler : IQueryHandler<ResumoContasReceberQuery, CommandResult>
    {
        private readonly ContextFinanceiro _ctx;
        public ResumoContasReceberQueryHandler(ContextFinanceiro ctx) => _ctx = ctx;

        public async Task<CommandResult> Handle(ResumoContasReceberQuery q, CancellationToken ct)
        {
            var titulos = await _ctx.ContasAReceberAgregado.AsNoTracking()
                .Where(t => !q.SomenteEmAberto || t.Situacao == ESituacao.Aberto || t.Situacao == ESituacao.PagoParcial)
                .Where(t => !q.Situacao.HasValue || (int)t.Situacao == q.Situacao.Value)
                .Where(t => (!q.Inicio.HasValue || t.DataEmissao >= q.Inicio.Value)
                         && (!q.Fim.HasValue || t.DataEmissao <= q.Fim.Value))
                .ToListAsync(ct);

            var refDate = DateTime.UtcNow.Date;
            var linhas = titulos.Select(t => new TituloLinhaDto(
                t.Id, t.Documento, t.PessoaId, t.NomePessoa, t.DataEmissao, t.DataVencimento,
                (int)t.Situacao, t.ValorTitulo, t.ValorTotalAReceberTitulo,
                Vencido: t.DataVencimento.Date < refDate && t.ValorTotalAReceberTitulo > 0 && t.Situacao != ESituacao.Cancelado))
                .OrderBy(l => l.DataVencimento)
                .ToList();

            var dto = new TitulosRelatorioDto("ContasAReceber", linhas,
                linhas.Sum(l => l.ValorTitulo), linhas.Sum(l => l.SaldoDevido), linhas.Count);
            return CommandResult.Ok("Resumo de contas a receber consultado com sucesso.", dto);
        }
    }

    // ---- FINANCEIRO: resumo CP -------------------------------------------
    public sealed class ResumoContasPagarQueryHandler : IQueryHandler<ResumoContasPagarQuery, CommandResult>
    {
        private readonly ContextFinanceiro _ctx;
        public ResumoContasPagarQueryHandler(ContextFinanceiro ctx) => _ctx = ctx;

        public async Task<CommandResult> Handle(ResumoContasPagarQuery q, CancellationToken ct)
        {
            var titulos = await _ctx.ContasAPagarAgregado.AsNoTracking()
                .Where(t => !q.SomenteEmAberto || t.Situacao == ESituacao.Aberto || t.Situacao == ESituacao.PagoParcial)
                .Where(t => !q.Situacao.HasValue || (int)t.Situacao == q.Situacao.Value)
                .Where(t => (!q.Inicio.HasValue || t.DataEmissao >= q.Inicio.Value)
                         && (!q.Fim.HasValue || t.DataEmissao <= q.Fim.Value))
                .ToListAsync(ct);

            var refDate = DateTime.UtcNow.Date;
            var linhas = titulos.Select(t => new TituloLinhaDto(
                t.Id, t.Documento, t.PessoaId, t.NomePessoa, t.DataEmissao, t.DataVencimento,
                (int)t.Situacao, t.ValorTitulo, t.ValorTotalAPagarTitulo,
                Vencido: t.DataVencimento.Date < refDate && t.ValorTotalAPagarTitulo > 0 && t.Situacao != ESituacao.Cancelado))
                .OrderBy(l => l.DataVencimento)
                .ToList();

            var dto = new TitulosRelatorioDto("ContasAPagar", linhas,
                linhas.Sum(l => l.ValorTitulo), linhas.Sum(l => l.SaldoDevido), linhas.Count);
            return CommandResult.Ok("Resumo de contas a pagar consultado com sucesso.", dto);
        }
    }

    // ---- FINANCEIRO: pagamentos recebidos --------------------------------
    public sealed class PagamentosRecebidosQueryHandler : IQueryHandler<PagamentosRecebidosQuery, CommandResult>
    {
        private readonly ContextFinanceiro _ctx;
        public PagamentosRecebidosQueryHandler(ContextFinanceiro ctx) => _ctx = ctx;

        public async Task<CommandResult> Handle(PagamentosRecebidosQuery q, CancellationToken ct)
        {
            var itens = await _ctx.ContasAReceberItens.AsNoTracking()
                .Where(i => (!q.Inicio.HasValue || i.DataRecebimento >= q.Inicio.Value)
                         && (!q.Fim.HasValue || i.DataRecebimento <= q.Fim.Value))
                .ToListAsync(ct);

            var linhas = itens
                .Select(i => new PagamentoLinhaDto(i.DataRecebimento, i.ValorPago, (int)i.TipoPagamento))
                .OrderBy(l => l.Data)
                .ToList();

            var dto = new PagamentosRelatorioDto("Recebidos", linhas, linhas.Sum(l => l.ValorPago), linhas.Count);
            return CommandResult.Ok("Pagamentos recebidos consultados com sucesso.", dto);
        }
    }

    // ---- FINANCEIRO: pagamentos efetuados --------------------------------
    public sealed class PagamentosEfetuadosQueryHandler : IQueryHandler<PagamentosEfetuadosQuery, CommandResult>
    {
        private readonly ContextFinanceiro _ctx;
        public PagamentosEfetuadosQueryHandler(ContextFinanceiro ctx) => _ctx = ctx;

        public async Task<CommandResult> Handle(PagamentosEfetuadosQuery q, CancellationToken ct)
        {
            var itens = await _ctx.ContasAPagarItens.AsNoTracking()
                .Where(i => (!q.Inicio.HasValue || i.DataPagamento >= q.Inicio.Value)
                         && (!q.Fim.HasValue || i.DataPagamento <= q.Fim.Value))
                .ToListAsync(ct);

            var linhas = itens
                .Select(i => new PagamentoLinhaDto(i.DataPagamento, i.ValorPago, (int)i.TipoPagamento))
                .OrderBy(l => l.Data)
                .ToList();

            var dto = new PagamentosRelatorioDto("Efetuados", linhas, linhas.Sum(l => l.ValorPago), linhas.Count);
            return CommandResult.Ok("Pagamentos efetuados consultados com sucesso.", dto);
        }
    }

    // ---- FINANCEIRO: fluxo de caixa --------------------------------------
    public sealed class FluxoCaixaQueryHandler : IQueryHandler<FluxoCaixaQuery, CommandResult>
    {
        private readonly ContextFinanceiro _ctx;
        public FluxoCaixaQueryHandler(ContextFinanceiro ctx) => _ctx = ctx;

        public async Task<CommandResult> Handle(FluxoCaixaQuery q, CancellationToken ct)
        {
            var movs = await _ctx.MovimentosFinanceiros.AsNoTracking()
                .Where(m => q.IncluirPlanejamento || !m.Planejamento)
                .Where(m => (!q.Inicio.HasValue || m.Emissao >= q.Inicio.Value)
                         && (!q.Fim.HasValue || m.Emissao <= q.Fim.Value))
                .ToListAsync(ct);

            var linhas = movs
                .GroupBy(m => m.Emissao.Date)
                .Select(g =>
                {
                    var entradas = g.Sum(x => x.Credito);
                    var saidas = g.Sum(x => x.Debito);
                    return new FluxoCaixaLinhaDto(g.Key, entradas, saidas, entradas - saidas);
                })
                .OrderBy(l => l.Dia)
                .ToList();

            var totEnt = linhas.Sum(l => l.Entradas);
            var totSai = linhas.Sum(l => l.Saidas);
            var dto = new FluxoCaixaRelatorioDto(linhas, totEnt, totSai, totEnt - totSai);
            return CommandResult.Ok("Fluxo de caixa consultado com sucesso.", dto);
        }
    }

    // =====================================================================
    // Calculo de aging compartilhado por CP e CR.
    // Faixas por dias vencidos: "A vencer", "1-30", "31-60", "61-90", "Acima de 90".
    // "Vencido" = vencimento < referencia E saldo devido > 0 (EF RN-016).
    // NOTA (fork): os limites de faixa (30/60/90) sao convencao gerencial padrao
    // e foram fixados pelo agente por ausencia de definicao em Negocio-acumulado/financeiro;
    // registrado em DECISOES-PENDENTES-RAFAEL.md para validacao humana.
    // =====================================================================
    internal static class AgingCalculo
    {
        public static AgingRelatorioDto Calcular(
            string natureza, DateTime refDate,
            IEnumerable<(DateTime DataVencimento, decimal Saldo)> registros)
        {
            decimal aVencer = 0, f1 = 0, f2 = 0, f3 = 0, f4 = 0;
            int cAVencer = 0, c1 = 0, c2 = 0, c3 = 0, c4 = 0;
            decimal saldoTotal = 0, saldoVencido = 0;

            foreach (var (venc, saldo) in registros)
            {
                if (saldo <= 0) continue;
                saldoTotal += saldo;
                var diasVencidos = (refDate - venc.Date).Days;

                if (diasVencidos <= 0) { aVencer += saldo; cAVencer++; continue; }

                saldoVencido += saldo;
                if (diasVencidos <= 30) { f1 += saldo; c1++; }
                else if (diasVencidos <= 60) { f2 += saldo; c2++; }
                else if (diasVencidos <= 90) { f3 += saldo; c3++; }
                else { f4 += saldo; c4++; }
            }

            var faixas = new List<AgingFaixaDto>
            {
                new("A vencer", cAVencer, aVencer),
                new("1-30", c1, f1),
                new("31-60", c2, f2),
                new("61-90", c3, f3),
                new("Acima de 90", c4, f4),
            };

            return new AgingRelatorioDto(natureza, refDate, faixas, saldoTotal, saldoVencido, aVencer);
        }
    }
}
