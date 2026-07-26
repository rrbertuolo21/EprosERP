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
    public record ListarFcpAliquotasUfQuery(int Pagina = 1, int TamanhoPagina = 50) : IQuery<CommandResult>;
    public record ObterFcpAliquotaUfPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarFcpAliquotasUfQueryHandler : IRequestHandler<ListarFcpAliquotasUfQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public ListarFcpAliquotasUfQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ListarFcpAliquotasUfQuery request, CancellationToken cancellationToken)
        {
            var query = _context.FcpAliquotasUf.AsNoTracking().Where(f => f.DeletadoEm == null);
            var total = await query.CountAsync(cancellationToken);
            var itens = await query.OrderBy(f => f.Uf)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(f => new { f.Id, f.Uf, f.ValorAliquota, f.Observacao })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterFcpAliquotaUfPorIdQueryHandler : IRequestHandler<ObterFcpAliquotaUfPorIdQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public ObterFcpAliquotaUfPorIdQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ObterFcpAliquotaUfPorIdQuery request, CancellationToken cancellationToken)
        {
            var f = await _context.FcpAliquotasUf.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);
            if (f == null) return CommandResult.Falha("Alíquota FCP não encontrada.");
            return CommandResult.Ok("OK", new { f.Id, f.Uf, f.ValorAliquota, f.Observacao });
        }
    }
}
