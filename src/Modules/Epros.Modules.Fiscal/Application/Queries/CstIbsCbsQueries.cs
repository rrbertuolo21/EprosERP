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
    public record ListarCstsIbsCbsQuery(string? Localizar = null, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ObterCstIbsCbsPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarCstsIbsCbsQueryHandler : IRequestHandler<ListarCstsIbsCbsQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public ListarCstsIbsCbsQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ListarCstsIbsCbsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.CstsIbsCbs.AsNoTracking().Where(c => c.DeletadoEm == null).AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.Localizar))
                query = query.Where(c => c.Cst.Contains(request.Localizar) || c.Descricao.Contains(request.Localizar));

            var total = await query.CountAsync(cancellationToken);
            var itens = await query.OrderBy(c => c.Cst)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(c => new { c.Id, c.Cst, c.Descricao, c.DataInicioVigencia, c.DataFimVigencia })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterCstIbsCbsPorIdQueryHandler : IRequestHandler<ObterCstIbsCbsPorIdQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public ObterCstIbsCbsPorIdQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ObterCstIbsCbsPorIdQuery request, CancellationToken cancellationToken)
        {
            var c = await _context.CstsIbsCbs.AsNoTracking()
                .Include(x => x.ClassesTributarias.Where(ct => ct.DeletadoEm == null))
                    .ThenInclude(ct => ct.Anexos.Where(a => a.DeletadoEm == null))
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);
            if (c == null) return CommandResult.Falha("CST IBS/CBS não encontrado.");

            var dto = new
            {
                c.Id,
                c.Cst,
                c.Descricao,
                c.DataInicioVigencia,
                c.DataFimVigencia,
                ClassesTributarias = c.ClassesTributarias.Select(ct => new
                {
                    ct.Id,
                    ct.Codigo,
                    ct.Descricao,
                    ct.DataInicioVigencia,
                    ct.DataFimVigencia,
                    ct.IndNfe,
                    ct.IndNfce,
                    ct.IndCte,
                    ct.IndCteos,
                    ct.IndNfse,
                    ct.IndTribRegular,
                    Anexos = ct.Anexos.Select(a => new { a.Id, a.NroAnexo, a.Codigo, a.DataInicioVigencia, a.DataFimVigencia }).ToList()
                }).ToList()
            };

            return CommandResult.Ok("OK", dto);
        }
    }
}
