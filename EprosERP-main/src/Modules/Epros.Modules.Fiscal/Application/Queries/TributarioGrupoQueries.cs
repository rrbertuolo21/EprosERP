using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Fiscal.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Fiscal.Application.Queries
{
    public record ListarTributarioGruposQuery(string? Localizar = null, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ObterTributarioGrupoPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarTributarioGruposQueryHandler : IRequestHandler<ListarTributarioGruposQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public ListarTributarioGruposQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ListarTributarioGruposQuery request, CancellationToken cancellationToken)
        {
            var query = _context.TributarioGrupos.AsNoTracking().Where(g => g.DeletadoEm == null).AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.Localizar))
                query = query.Where(g => g.Descricao.Contains(request.Localizar));

            var total = await query.CountAsync(cancellationToken);
            var itens = await query.OrderBy(g => g.Descricao)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(g => new { g.Id, g.Descricao })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterTributarioGrupoPorIdQueryHandler : IRequestHandler<ObterTributarioGrupoPorIdQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public ObterTributarioGrupoPorIdQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ObterTributarioGrupoPorIdQuery request, CancellationToken cancellationToken)
        {
            var g = await _context.TributarioGrupos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);
            if (g == null) return CommandResult.Falha("Grupo Tributário não encontrado.");
            return CommandResult.Ok("OK", new { g.Id, g.Descricao });
        }
    }
}
