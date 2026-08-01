using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Modules.Estoque.Application.Queries
{
    public record ListarComprasQuery(
        string? Localizar = null,
        int Pagina = 1,
        int TamanhoPagina = 20
    ) : IQuery<CommandResult>;

    public record ObterCompraPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarComprasQueryHandler : IRequestHandler<ListarComprasQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ListarComprasQueryHandler(ContextEstoque context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ListarComprasQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Compras.AsNoTracking().Where(c => c.DeletadoEm == null).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Localizar))
            {
                query = query.Where(c => c.FornecedorNome.Contains(request.Localizar) ||
                                         c.FornecedorCnpj.Contains(request.Localizar) ||
                                         c.NumeroNota.Contains(request.Localizar) ||
                                         c.ChaveAcesso.Contains(request.Localizar));
            }

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(c => c.DataEmissao)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(c => new
                {
                    c.Id,
                    c.FornecedorCnpj,
                    c.FornecedorNome,
                    c.NumeroNota,
                    c.ChaveAcesso,
                    c.ValorTotal,
                    c.DataEmissao,
                    c.Status,
                    c.CriadoEm
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterCompraPorIdQueryHandler : IRequestHandler<ObterCompraPorIdQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ObterCompraPorIdQueryHandler(ContextEstoque context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterCompraPorIdQuery request, CancellationToken cancellationToken)
        {
            var compra = await _context.Compras
                .AsNoTracking()
                .Include(c => c.Itens)
                .FirstOrDefaultAsync(c => c.Id == request.Id && c.DeletadoEm == null, cancellationToken);

            if (compra == null)
            {
                return CommandResult.Falha("Nota de Compra não encontrada.");
            }

            return CommandResult.Ok("OK", new
            {
                compra.Id,
                compra.FornecedorCnpj,
                compra.FornecedorNome,
                compra.NumeroNota,
                compra.ChaveAcesso,
                compra.ValorTotal,
                compra.DataEmissao,
                compra.Status,
                compra.ModeloFiscal,
                compra.NaturezaOperacao,
                compra.DataCompra,
                compra.InformacoesComplementares,
                compra.InformacoesAdicionaisFisco,
                compra.ModalidadeFrete,
                compra.CompraOrigem,
                compra.TipoNota,
                compra.FormaPagamento,
                Itens = compra.Itens.Select(i => new
                {
                    i.Id,
                    i.ProdutoId,
                    Sku = i.CodigoProduto,
                    NomeProduto = i.DescricaoProduto,
                    i.Quantidade,
                    i.PrecoUnitario,
                    i.ValorIms,
                    i.ValorIpi,
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
