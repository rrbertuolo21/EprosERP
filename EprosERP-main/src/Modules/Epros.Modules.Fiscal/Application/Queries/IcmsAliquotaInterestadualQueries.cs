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
    public record ListarIcmsAliquotasInterestaduaisQuery(int Pagina = 1, int TamanhoPagina = 50) : IQuery<CommandResult>;
    public record ObterIcmsAliquotaInterestadualPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarIcmsAliquotasInterestaduaisQueryHandler : IRequestHandler<ListarIcmsAliquotasInterestaduaisQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public ListarIcmsAliquotasInterestaduaisQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ListarIcmsAliquotasInterestaduaisQuery request, CancellationToken cancellationToken)
        {
            var query = _context.IcmsAliquotasInterestaduais.AsNoTracking().Where(i => i.DeletadoEm == null);
            var total = await query.CountAsync(cancellationToken);
            var itens = await query.OrderBy(i => i.UfOrigem).ThenBy(i => i.UfDestino)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(i => new { i.Id, i.UfOrigem, i.UfDestino, i.ValorAliquota })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterIcmsAliquotaInterestadualPorIdQueryHandler : IRequestHandler<ObterIcmsAliquotaInterestadualPorIdQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public ObterIcmsAliquotaInterestadualPorIdQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ObterIcmsAliquotaInterestadualPorIdQuery request, CancellationToken cancellationToken)
        {
            var i = await _context.IcmsAliquotasInterestaduais.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);
            if (i == null) return CommandResult.Falha("Alíquota ICMS Interestadual não encontrada.");
            return CommandResult.Ok("OK", new { i.Id, i.UfOrigem, i.UfDestino, i.ValorAliquota });
        }
    }
}
