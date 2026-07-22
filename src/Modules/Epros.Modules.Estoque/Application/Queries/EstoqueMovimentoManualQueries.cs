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
    public record ListarEstoqueMovimentosManuaisQuery(Guid? ProdutoId = null, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;

    public record ObterEstoqueMovimentoManualPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarEstoqueMovimentosManuaisQueryHandler : IRequestHandler<ListarEstoqueMovimentosManuaisQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ListarEstoqueMovimentosManuaisQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarEstoqueMovimentosManuaisQuery request, CancellationToken cancellationToken)
        {
            var query = _context.EstoqueMovimentosManuais.AsNoTracking().Where(m => m.DeletadoEm == null).AsQueryable();

            if (request.ProdutoId.HasValue)
                query = query.Where(m => m.ProdutoId == request.ProdutoId.Value);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(m => m.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(m => new
                {
                    m.Id,
                    m.ProdutoId,
                    m.TipoEstoque,
                    m.TipoMovimento,
                    m.QuantidadeMovimentada,
                    m.ValorUnitario,
                    m.CriadoEm
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterEstoqueMovimentoManualPorIdQueryHandler : IRequestHandler<ObterEstoqueMovimentoManualPorIdQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ObterEstoqueMovimentoManualPorIdQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ObterEstoqueMovimentoManualPorIdQuery request, CancellationToken cancellationToken)
        {
            var m = await _context.EstoqueMovimentosManuais.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);
            if (m == null)
                return CommandResult.Falha("Movimento manual de estoque não encontrado.");

            return CommandResult.Ok("OK", new
            {
                m.Id,
                m.ProdutoId,
                m.TipoEstoque,
                m.TipoMovimento,
                m.QuantidadeMovimentada,
                m.ValorUnitario,
                m.CriadoEm
            });
        }
    }
}
