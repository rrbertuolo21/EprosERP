using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Vendas.Infrastructure.Data;

namespace Epros.Modules.Vendas.Application.Queries
{
    public record ListarVendasQuery(
        string? Localizar = null,
        int Pagina = 1,
        int TamanhoPagina = 20
    ) : IQuery<CommandResult>;

    public record ObterVendaPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarVendasQueryHandler : IRequestHandler<ListarVendasQuery, CommandResult>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarVendasQueryHandler(ContextVendas context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarVendasQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();

            var query = _context.Vendas
                .AsNoTracking()
                .Where(v => v.TenantId == tenantId && v.DeletadoEm == null)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Localizar))
            {
                query = query.Where(v => v.CaixaId.Contains(request.Localizar) ||
                                         v.FormaPagamento!.Contains(request.Localizar));
            }

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(v => v.DataVenda)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(v => new
                {
                    v.Id,
                    v.CaixaId,
                    v.Total,
                    v.Status,
                    v.DataVenda,
                    v.ClienteId,
                    v.ValorDesconto,
                    v.ValorFrete,
                    v.FormaPagamento,
                    v.CriadoEm
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterVendaPorIdQueryHandler : IRequestHandler<ObterVendaPorIdQuery, CommandResult>
    {
        private readonly ContextVendas _context;

        public ObterVendaPorIdQueryHandler(ContextVendas context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterVendaPorIdQuery request, CancellationToken cancellationToken)
        {
            var venda = await _context.Vendas
                .AsNoTracking()
                .Include(v => v.Itens)
                .FirstOrDefaultAsync(v => v.Id == request.Id && v.DeletadoEm == null, cancellationToken);

            if (venda == null)
            {
                return CommandResult.Falha("Venda não encontrada.");
            }

            return CommandResult.Ok("OK", new
            {
                venda.Id,
                venda.CaixaId,
                venda.Total,
                venda.Status,
                venda.ModeloFiscal,
                venda.NaturezaOperacao,
                venda.DataVenda,
                venda.InformacoesComplementares,
                venda.InformacoesAdicionaisFisco,
                venda.ModalidadeFrete,
                venda.VendaOrigem,
                venda.IncluirFreteNoTotal,
                venda.ClienteId,
                venda.ValorDesconto,
                venda.ValorFrete,
                venda.FormaPagamento,
                venda.CriadoEm,
                Itens = venda.Itens.Select(i => new
                {
                    i.Id,
                    i.ProdutoId,
                    i.Quantidade,
                    i.PrecoUnitario,
                    i.ValorTotal,
                    i.CodigoProduto,
                    i.CodigoEan,
                    i.DescricaoProduto,
                    i.Ncm,
                    i.CestId,
                    i.Cest,
                    i.CodigoAnpId,
                    i.CodigoAnp,
                    i.Cfop,
                    i.UnidadeComercial,
                    i.ValorDesconto,
                    i.ValorFreteRateado,
                    i.ValorCusto
                })
            });
        }
    }
}
