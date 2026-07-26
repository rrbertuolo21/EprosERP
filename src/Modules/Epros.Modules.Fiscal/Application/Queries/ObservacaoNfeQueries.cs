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
    public record ListarObservacoesNfeQuery(string? Localizar = null, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ObterObservacaoNfePorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarObservacoesNfeQueryHandler : IRequestHandler<ListarObservacoesNfeQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public ListarObservacoesNfeQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ListarObservacoesNfeQuery request, CancellationToken cancellationToken)
        {
            var query = _context.ObservacoesNfe.AsNoTracking().Where(o => o.DeletadoEm == null).AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.Localizar))
                query = query.Where(o => o.Descricao.Contains(request.Localizar));

            var total = await query.CountAsync(cancellationToken);
            var itens = await query.OrderBy(o => o.Descricao)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(o => new { o.Id, o.Descricao })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterObservacaoNfePorIdQueryHandler : IRequestHandler<ObterObservacaoNfePorIdQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public ObterObservacaoNfePorIdQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ObterObservacaoNfePorIdQuery request, CancellationToken cancellationToken)
        {
            var o = await _context.ObservacoesNfe.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);
            if (o == null) return CommandResult.Falha("Observação NFe não encontrada.");
            return CommandResult.Ok("OK", new { o.Id, o.Descricao });
        }
    }
}
