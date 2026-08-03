using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Application.Security;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    // 1.08G — Handlers dos relatórios financeiros AGREGADOS do Landlord.
    //
    // ⛔ Regra #0 (escopo): somente AGREGADOS FACTUAIS (regime de CAIXA) sobre dados já gravados (1.08A).
    // NÃO há reconhecimento de receita por competência/diferimento, NEM apuração fiscal de comissão — isso
    // é ONDA 2 (skill de negócio + contador). Aqui é somatório factual.
    //
    // Segurança (defense-in-depth, espelha os handlers do módulo Aplicativo): a consulta é LANDLORD e
    // consolida através de TODOS os tenants (IgnoreQueryFilters), portanto exige operador interno
    // (tenant "system"); caso contrário lança UnauthorizedAccessException (fail-closed), mesmo que o
    // AbacFilter do controller seja contornado.

    internal static class RelatorioLandlordGuard
    {
        public static void ExigirOperadorInterno(ITenantProvider tenantProvider)
        {
            if (!GuardaOperadorInterno.EhOperadorInterno(tenantProvider))
            {
                throw new UnauthorizedAccessException(
                    "Acesso Proibido: relatório restrito ao operador interno da Siser (landlord).");
            }
        }
    }

    /// <summary>1.08G — Receita por período (CAIXA): faturas pagas agrupadas por mês (bruto/tarifa/líquido).</summary>
    public class ObterRelatorioReceitaPorPeriodoQueryHandler
        : IQueryHandler<ObterRelatorioReceitaPorPeriodoQuery, RelatorioReceitaPeriodoDto>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ObterRelatorioReceitaPorPeriodoQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<RelatorioReceitaPeriodoDto> Handle(ObterRelatorioReceitaPorPeriodoQuery request, CancellationToken cancellationToken)
        {
            RelatorioLandlordGuard.ExigirOperadorInterno(_tenantProvider);

            // Faturas pagas, agrupadas pelo mês do PAGAMENTO (caixa). Consolidado em todos os tenants.
            var faturasPagas = await _context.Faturas
                .IgnoreQueryFilters()
                .Where(f => f.Status == FaturaStatus.Paga
                            && f.DeletadoEm == null
                            && f.DataPagamento != null
                            && f.DataPagamento >= request.Inicio
                            && f.DataPagamento <= request.Fim)
                .Select(f => new { f.Id, DataPagamento = f.DataPagamento!.Value, f.Valor })
                .ToListAsync(cancellationToken);

            var faturaIds = faturasPagas.Select(f => f.Id).ToList();

            // Tarifa = ValorTarifa dos pagamentos liquidados dessas faturas (fato gravado pela 1.08A/1.08E).
            var tarifaPorFatura = await _context.PagamentosFaturas
                .IgnoreQueryFilters()
                .Where(p => faturaIds.Contains(p.FaturaId) && p.ValorTarifa != null && p.DeletadoEm == null)
                .GroupBy(p => p.FaturaId)
                .Select(g => new { FaturaId = g.Key, Tarifa = g.Sum(x => x.ValorTarifa!.Value) })
                .ToListAsync(cancellationToken);
            var tarifaMap = tarifaPorFatura.ToDictionary(x => x.FaturaId, x => x.Tarifa);

            var meses = faturasPagas
                .GroupBy(f => new { f.DataPagamento.Year, f.DataPagamento.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g =>
                {
                    var bruto = g.Sum(x => x.Valor);
                    var tarifa = g.Sum(x => tarifaMap.TryGetValue(x.Id, out var t) ? t : 0m);
                    return new ReceitaMesDto(
                        Ano: g.Key.Year,
                        Mes: g.Key.Month,
                        Bruto: bruto,
                        Tarifa: tarifa,
                        Liquido: bruto - tarifa,
                        QuantidadeFaturas: g.Count());
                })
                .ToList();

            var totalBruto = meses.Sum(m => m.Bruto);
            var totalTarifa = meses.Sum(m => m.Tarifa);

            return new RelatorioReceitaPeriodoDto(
                Inicio: request.Inicio,
                Fim: request.Fim,
                TotalBruto: totalBruto,
                TotalTarifa: totalTarifa,
                TotalLiquido: totalBruto - totalTarifa,
                TotalFaturas: faturasPagas.Count,
                Meses: meses);
        }
    }

    /// <summary>1.08G — Inadimplência por período: faturas vencidas e não pagas, por mês de vencimento e tenant.</summary>
    public class ObterRelatorioInadimplenciaPorPeriodoQueryHandler
        : IQueryHandler<ObterRelatorioInadimplenciaPorPeriodoQuery, RelatorioInadimplenciaPeriodoDto>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ObterRelatorioInadimplenciaPorPeriodoQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<RelatorioInadimplenciaPeriodoDto> Handle(ObterRelatorioInadimplenciaPorPeriodoQuery request, CancellationToken cancellationToken)
        {
            RelatorioLandlordGuard.ExigirOperadorInterno(_tenantProvider);

            // Vencidas e NÃO pagas cujo vencimento cai no intervalo. Consolidado em todos os tenants.
            var faturas = await _context.Faturas
                .IgnoreQueryFilters()
                .Where(f => f.DeletadoEm == null
                            && f.Status != FaturaStatus.Paga
                            && f.Status != FaturaStatus.Cancelada
                            && f.Status != FaturaStatus.Estornada
                            && f.DataVencimento >= request.Inicio
                            && f.DataVencimento <= request.Fim)
                .Select(f => new { f.TenantId, f.DataVencimento, f.Valor })
                .ToListAsync(cancellationToken);

            var itens = faturas
                .GroupBy(f => new { f.DataVencimento.Year, f.DataVencimento.Month, f.TenantId })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month).ThenBy(g => g.Key.TenantId)
                .Select(g => new InadimplenciaMesTenantDto(
                    Ano: g.Key.Year,
                    Mes: g.Key.Month,
                    TenantId: g.Key.TenantId,
                    ValorTotal: g.Sum(x => x.Valor),
                    QuantidadeFaturas: g.Count()))
                .ToList();

            return new RelatorioInadimplenciaPeriodoDto(
                Inicio: request.Inicio,
                Fim: request.Fim,
                ValorTotal: faturas.Sum(f => f.Valor),
                TotalFaturas: faturas.Count,
                Itens: itens);
        }
    }

    /// <summary>1.08G — Comissão por período (CAIXA): soma factual de ValorComissaoRevenda/Vendedor, por mês/revenda.</summary>
    public class ObterRelatorioComissaoPorPeriodoQueryHandler
        : IQueryHandler<ObterRelatorioComissaoPorPeriodoQuery, RelatorioComissaoPeriodoDto>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ObterRelatorioComissaoPorPeriodoQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<RelatorioComissaoPeriodoDto> Handle(ObterRelatorioComissaoPorPeriodoQuery request, CancellationToken cancellationToken)
        {
            RelatorioLandlordGuard.ExigirOperadorInterno(_tenantProvider);

            // Comissão é gravada na BAIXA da fatura (paga). Agrupa pelo mês do pagamento (caixa).
            var faturas = await _context.Faturas
                .IgnoreQueryFilters()
                .Where(f => f.Status == FaturaStatus.Paga
                            && f.DeletadoEm == null
                            && f.DataPagamento != null
                            && f.DataPagamento >= request.Inicio
                            && f.DataPagamento <= request.Fim)
                .Select(f => new { f.ClienteId, DataPagamento = f.DataPagamento!.Value, f.ValorComissaoRevenda, f.ValorComissaoVendedor })
                .ToListAsync(cancellationToken);

            // Revenda vem do Cliente (a Fatura não guarda RevendaId diretamente).
            var clienteIds = faturas.Select(f => f.ClienteId).Distinct().ToList();
            var revendaPorCliente = await _context.Clientes
                .IgnoreQueryFilters()
                .Where(c => clienteIds.Contains(c.Id))
                .Select(c => new { c.Id, c.RevendaId })
                .ToListAsync(cancellationToken);
            var revendaMap = revendaPorCliente.ToDictionary(c => c.Id, c => c.RevendaId);

            var itens = faturas
                .Select(f => new
                {
                    f.DataPagamento.Year,
                    f.DataPagamento.Month,
                    RevendaId = revendaMap.TryGetValue(f.ClienteId, out var r) ? r : null,
                    f.ValorComissaoRevenda,
                    f.ValorComissaoVendedor
                })
                .GroupBy(x => new { x.Year, x.Month, x.RevendaId })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new ComissaoMesRevendaDto(
                    Ano: g.Key.Year,
                    Mes: g.Key.Month,
                    RevendaId: g.Key.RevendaId,
                    ComissaoRevenda: g.Sum(x => x.ValorComissaoRevenda),
                    ComissaoVendedor: g.Sum(x => x.ValorComissaoVendedor),
                    QuantidadeFaturas: g.Count()))
                .ToList();

            return new RelatorioComissaoPeriodoDto(
                Inicio: request.Inicio,
                Fim: request.Fim,
                TotalComissaoRevenda: faturas.Sum(f => f.ValorComissaoRevenda),
                TotalComissaoVendedor: faturas.Sum(f => f.ValorComissaoVendedor),
                TotalFaturas: faturas.Count,
                Itens: itens);
        }
    }
}
