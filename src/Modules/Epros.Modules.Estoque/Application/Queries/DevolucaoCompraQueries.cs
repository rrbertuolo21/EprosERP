using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Epros.Shared.Application.Models;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Modules.Estoque.Application.Queries
{
    /// <summary>
    /// Consultas do submódulo Devolução de Compra (CD4). Tenant + soft-delete aplicados pelo filtro global.
    /// </summary>
    public record ListarDevolucoesCompraQuery(
        EStatusDevolucaoCompra? Status = null,
        Guid? CompraOrigemId = null,
        Guid? FornecedorId = null,
        int Pagina = 1,
        int TamanhoPagina = 20) : IRequest<CommandResult>;

    public record ObterDevolucaoCompraPorIdQuery(Guid Id) : IRequest<CommandResult>;

    public class ListarDevolucoesCompraQueryHandler : IRequestHandler<ListarDevolucoesCompraQuery, CommandResult>
    {
        private readonly ContextEstoque _context;
        public ListarDevolucoesCompraQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarDevolucoesCompraQuery request, CancellationToken cancellationToken)
        {
            var query = _context.DevolucoesCompra.AsNoTracking().AsQueryable();
            if (request.Status.HasValue)
                query = query.Where(d => d.Status == request.Status.Value);
            if (request.CompraOrigemId.HasValue)
                query = query.Where(d => d.CompraOrigemId == request.CompraOrigemId.Value);
            if (request.FornecedorId.HasValue)
                query = query.Where(d => d.FornecedorId == request.FornecedorId.Value);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(d => d.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(d => new { d.Id, d.Numero, d.CompraOrigemId, d.FornecedorId, d.DataDevolucao, d.Tipo, d.Status, d.Cfop, d.Total, d.CriadoEm, d.ConfirmadaEm })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterDevolucaoCompraPorIdQueryHandler : IRequestHandler<ObterDevolucaoCompraPorIdQuery, CommandResult>
    {
        private readonly ContextEstoque _context;
        public ObterDevolucaoCompraPorIdQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ObterDevolucaoCompraPorIdQuery request, CancellationToken cancellationToken)
        {
            var d = await _context.DevolucoesCompra.AsNoTracking()
                .Include(x => x.Itens)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (d == null)
                return CommandResult.Falha("Devolução não encontrada.");

            return CommandResult.Ok("OK", new
            {
                d.Id,
                d.Numero,
                d.CompraOrigemId,
                d.FornecedorId,
                d.DataDevolucao,
                d.Tipo,
                d.Motivo,
                d.Status,
                d.DocumentoFiscalId,
                d.Cfop,
                d.Total,
                d.CriadoEm,
                d.ConfirmadaEm,
                d.CanceladaEm,
                Itens = d.Itens.Select(i => new { i.Id, i.CompraItemOrigemId, i.ProdutoId, i.Quantidade, i.ValorUnitario, i.ValorTotal })
            });
        }
    }
}
