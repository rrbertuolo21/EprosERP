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
    public record ListarUnidadesMedidaTributavelQuery(string? Localizar = null, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;

    public record ObterUnidadeMedidaTributavelPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarUnidadesMedidaTributavelQueryHandler : IRequestHandler<ListarUnidadesMedidaTributavelQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ListarUnidadesMedidaTributavelQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarUnidadesMedidaTributavelQuery request, CancellationToken cancellationToken)
        {
            var query = _context.UnidadesMedidaTributavel.AsNoTracking().Where(u => u.DeletadoEm == null).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Localizar))
                query = query.Where(u => u.CodigoNcm.Contains(request.Localizar) || u.UnidadeMedida.Contains(request.Localizar) || u.Descricao.Contains(request.Localizar));

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(u => u.CodigoNcm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(u => new
                {
                    u.Id,
                    u.CodigoNcm,
                    u.DataInicioVigencia,
                    u.DataFimVigencia,
                    u.UnidadeMedida,
                    u.Descricao
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterUnidadeMedidaTributavelPorIdQueryHandler : IRequestHandler<ObterUnidadeMedidaTributavelPorIdQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ObterUnidadeMedidaTributavelPorIdQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ObterUnidadeMedidaTributavelPorIdQuery request, CancellationToken cancellationToken)
        {
            var u = await _context.UnidadesMedidaTributavel.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);
            if (u == null)
                return CommandResult.Falha("Unidade de medida tributável não encontrada.");

            return CommandResult.Ok("OK", new
            {
                u.Id,
                u.CodigoNcm,
                u.DataInicioVigencia,
                u.DataFimVigencia,
                u.UnidadeMedida,
                u.Descricao
            });
        }
    }
}
