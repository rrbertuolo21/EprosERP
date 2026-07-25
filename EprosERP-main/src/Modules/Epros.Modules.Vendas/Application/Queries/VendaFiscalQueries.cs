using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Vendas.Application.Queries
{
    /// <summary>Obtém uma venda fiscal completa (com todos os agregados filhos).</summary>
    public record ObterVendaFiscalCompletaQuery(Guid Id) : IQuery<CommandResult>;

    /// <summary>Lista vendas fiscais paginadas por tenant.</summary>
    public record ListarVendasFiscaisQuery(
        EVendaStatus? Status = null,
        DateTime? DataInicio = null,
        DateTime? DataFim = null,
        int Pagina = 1,
        int TamanhoPagina = 20
    ) : IQuery<CommandResult>;

    /// <summary>
    /// Porte fiel do relatório "Simplificado01" (legado: VendaReportsController.Get,
    /// rota api/v1/relatorios/vendas/simplificado01). No legado o relatório filtra vendas
    /// do período por data e pelo status interno (EDocumentoFiscalStatus) da NFC-e/NF-e
    /// vinculada, e devolve as colunas: Data, Número, Destinatário, Valor, Modelo, Status, Chave.
    /// Aqui devolvemos os mesmos dados como JSON paginado (a geração do arquivo .xlsx fica a
    /// cargo do consumidor/frontend); segregação por TenantId (não há EmpresaId no ICurrentUser
    /// novo — o legado segregava por EmpresaId dentro do usuário autenticado).
    /// </summary>
    public record ObterRelatorioVendasSimplificadoQuery(
        DateTime DataVendaInicial,
        DateTime DataVendaFinal,
        EDocumentoFiscalStatus Status,
        int Pagina = 1,
        int TamanhoPagina = 20
    ) : IQuery<CommandResult>;

    public class ObterVendaFiscalCompletaQueryHandler : IRequestHandler<ObterVendaFiscalCompletaQuery, CommandResult>
    {
        private readonly ContextVendas _context;

        public ObterVendaFiscalCompletaQueryHandler(ContextVendas context) => _context = context;

        public async Task<CommandResult> Handle(ObterVendaFiscalCompletaQuery request, CancellationToken ct)
        {
            var venda = await _context.Vendas
                .AsNoTracking()
                .Include(v => v.Itens).ThenInclude(i => i.Imposto)
                .Include(v => v.Itens).ThenInclude(i => i.ImpostoIbsCbs).ThenInclude(x => x!.VendaItemImpostoIbsCbsTributacaoRegular)
                .Include(v => v.Itens).ThenInclude(i => i.ImpostoValorAproximado)
                .Include(v => v.Itens).ThenInclude(i => i.Combustivel).ThenInclude(c => c!.Origens)
                .Include(v => v.Pagamentos)
                .Include(v => v.Emitente).ThenInclude(e => e!.Endereco)
                .Include(v => v.Destinatario).ThenInclude(d => d!.Enderecos)
                .Include(v => v.Transporte).ThenInclude(t => t!.Transportadora)
                .Include(v => v.Transporte).ThenInclude(t => t!.Veiculo)
                .Include(v => v.Transporte).ThenInclude(t => t!.Volumes)
                .Include(v => v.Transporte).ThenInclude(t => t!.Reboques)
                .Include(v => v.Total_)
                .Include(v => v.TotalIbsCbs)
                .Include(v => v.Configuracao)
                .Include(v => v.Imposto)
                .Include(v => v.Fatura).ThenInclude(f => f!.Duplicatas)
                .Include(v => v.Nfce)
                .Include(v => v.Nfe).ThenInclude(n => n!.Intermediador)
                .Include(v => v.Nfe).ThenInclude(n => n!.VendaNfeExportacao)
                .Include(v => v.Nfe).ThenInclude(n => n!.CartasCorrecoes)
                .Include(v => v.VendaEntrega)
                .Include(v => v.VendaCobrancaEndereco)
                .Include(v => v.AutorizacoesXml)
                .Include(v => v.Referenciadas)
                .FirstOrDefaultAsync(v => v.Id == request.Id && v.DeletadoEm == null, ct);

            if (venda == null) return CommandResult.Falha("Venda não encontrada.");

            return CommandResult.Ok("OK", venda);
        }
    }

    public class ListarVendasFiscaisQueryHandler : IRequestHandler<ListarVendasFiscaisQuery, CommandResult>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenant;

        public ListarVendasFiscaisQueryHandler(ContextVendas context, ITenantProvider tenant)
        { _context = context; _tenant = tenant; }

        public async Task<CommandResult> Handle(ListarVendasFiscaisQuery request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();

            var query = _context.Vendas
                .AsNoTracking()
                .Where(v => v.TenantId == tenantId && v.DeletadoEm == null);

            if (request.Status.HasValue)
                query = query.Where(v => v.Status == request.Status.Value);
            if (request.DataInicio.HasValue)
                query = query.Where(v => v.DataVenda >= request.DataInicio.Value);
            if (request.DataFim.HasValue)
                query = query.Where(v => v.DataVenda <= request.DataFim.Value);

            var total = await query.CountAsync(ct);
            var itens = await query
                .OrderByDescending(v => v.DataVenda)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(v => new
                {
                    v.Id,
                    v.CaixaId,
                    v.ModeloFiscal,
                    v.NaturezaOperacao,
                    v.Status,
                    v.Total,
                    v.ValorDesconto,
                    v.ValorFrete,
                    v.ClienteId,
                    v.DataVenda,
                    v.DocumentoFiscalId,
                    v.CriadoEm
                })
                .ToListAsync(ct);

            return CommandResult.Ok("OK", new { Total = total, request.Pagina, Itens = itens });
        }
    }

    /// <summary>
    /// Handler do relatório "Simplificado01". Porte fiel de VendaReportsController.Get:
    /// filtra Vendas do tenant no período [DataVendaInicial, DataVendaFinal] (por dia, como
    /// no legado, que usa .Date) cujo Nfce ou Nfe tenha StatusInterno == status informado.
    /// </summary>
    public class ObterRelatorioVendasSimplificadoQueryHandler : IRequestHandler<ObterRelatorioVendasSimplificadoQuery, CommandResult>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenant;

        public ObterRelatorioVendasSimplificadoQueryHandler(ContextVendas context, ITenantProvider tenant)
        { _context = context; _tenant = tenant; }

        public async Task<CommandResult> Handle(ObterRelatorioVendasSimplificadoQuery request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var dataInicial = request.DataVendaInicial.Date;
            var dataFinal = request.DataVendaFinal.Date;

            var query = _context.Vendas
                .AsNoTracking()
                .Include(v => v.Emitente)
                .Include(v => v.Destinatario)
                .Include(v => v.Total_)
                .Include(v => v.Nfce)
                .Include(v => v.Nfe)
                .Where(v => v.TenantId == tenantId && v.DeletadoEm == null
                    && v.DataVenda.Date >= dataInicial && v.DataVenda.Date <= dataFinal
                    && ((v.Nfce != null && v.Nfce.StatusInterno == request.Status)
                        || (v.Nfe != null && v.Nfe.StatusInterno == request.Status)));

            var total = await query.CountAsync(ct);

            var vendas = await query
                .OrderBy(v => v.DataVenda)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .ToListAsync(ct);

            var itens = vendas.Select(v => new
            {
                Data = v.DataVenda.Date,
                Numero = v.Nfe != null ? v.Nfe.Numero : v.Nfce?.Numero,
                Destinatario = v.Destinatario?.RazaoSocial,
                Valor = v.Total_?.ValorNotaFiscal ?? v.Total,
                Modelo = v.ModeloFiscal,
                Status = v.Nfce != null ? v.Nfce.StatusInterno.ToString() : v.Nfe?.StatusInterno.ToString(),
                Chave = v.Nfe != null ? v.Nfe.Chave : v.Nfce?.Chave
            }).ToList();

            return CommandResult.Ok("OK", new { Total = total, request.Pagina, Itens = itens });
        }
    }
}
