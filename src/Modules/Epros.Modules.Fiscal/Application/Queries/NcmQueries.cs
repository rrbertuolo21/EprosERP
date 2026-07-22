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
    public record ListarNcmsQuery(string? Localizar = null, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;

    public record ObterNcmPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarNcmsQueryHandler : IRequestHandler<ListarNcmsQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public ListarNcmsQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ListarNcmsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Ncms.AsNoTracking().Where(n => n.DeletadoEm == null).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Localizar))
                query = query.Where(n => n.CodigoNcm.Contains(request.Localizar) || n.Descricao.Contains(request.Localizar));

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(n => n.CodigoNcm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(n => new { n.Id, n.CodigoNcm, n.Descricao, n.DataInicio, n.DataFim })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterNcmPorIdQueryHandler : IRequestHandler<ObterNcmPorIdQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public ObterNcmPorIdQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ObterNcmPorIdQuery request, CancellationToken cancellationToken)
        {
            var n = await _context.Ncms.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);

            if (n == null)
                return CommandResult.Falha("NCM não encontrado.");

            return CommandResult.Ok("OK", new
            {
                n.Id, n.CodigoNcm, n.Descricao, n.DataInicio, n.DataFim, n.TipoAtoIni, n.NumeroAtoIni, n.AnoAtoIni
            });
        }
    }
}
