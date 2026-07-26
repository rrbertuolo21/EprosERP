using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Fiscal.Infrastructure.Data;

namespace Epros.Modules.Fiscal.Application.Queries
{
    /// <summary>
    /// Listagem paginada das inutilizações de faixa de numeração já realizadas (histórico).
    /// Porte fiel do legado <c>GET api/v1/inutilizacao-dfe</c> (filtros por documento/modelo e ambiente).
    /// </summary>
    public record ListarInutilizacoesFiscaisQuery(
        int? ModeloDocumento = null,
        int? Ambiente = null,
        int Pagina = 1,
        int TamanhoPagina = 25
    ) : IQuery<CommandResult>;

    public class ListarInutilizacoesFiscaisQueryHandler : IRequestHandler<ListarInutilizacoesFiscaisQuery, CommandResult>
    {
        private readonly ContextFiscal _context;

        public ListarInutilizacoesFiscaisQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ListarInutilizacoesFiscaisQuery request, CancellationToken cancellationToken)
        {
            var query = _context.InutilizacoesFiscais
                .AsNoTracking()
                .Where(i => i.DeletadoEm == null)
                .AsQueryable();

            if (request.ModeloDocumento.HasValue)
                query = query.Where(i => i.ModeloDocumento == request.ModeloDocumento.Value);

            if (request.Ambiente.HasValue)
                query = query.Where(i => i.Ambiente == request.Ambiente.Value);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(i => i.DataInutilizacao)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(i => new
                {
                    i.Id,
                    i.ModeloDocumento,
                    i.Serie,
                    i.NrNfInicial,
                    i.NrNfFinal,
                    i.Ano,
                    i.Ambiente,
                    i.Justificativa,
                    Status = i.StatusSefaz,
                    i.Motivo,
                    i.Protocolo,
                    i.DataInutilizacao
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }
}
