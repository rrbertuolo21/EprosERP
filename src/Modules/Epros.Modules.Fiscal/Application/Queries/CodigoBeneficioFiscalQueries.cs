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
    public record ListarCodigosBeneficiosFiscaisQuery(
        string? Localizar = null,
        int Pagina = 1,
        int TamanhoPagina = 20
    ) : IQuery<CommandResult>;

    public record ObterCodigoBeneficioFiscalPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarCodigosBeneficiosFiscaisQueryHandler : IRequestHandler<ListarCodigosBeneficiosFiscaisQuery, CommandResult>
    {
        private readonly ContextFiscal _context;

        public ListarCodigosBeneficiosFiscaisQueryHandler(ContextFiscal context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ListarCodigosBeneficiosFiscaisQuery request, CancellationToken cancellationToken)
        {
            var query = _context.CodigosBeneficiosFiscais
                .AsNoTracking()
                .Where(b => b.DeletadoEm == null)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Localizar))
            {
                query = query.Where(b => b.Codigo.Contains(request.Localizar) || b.Descricao!.Contains(request.Localizar));
            }

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(b => b.Codigo)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(b => new
                {
                    b.Id,
                    b.Codigo,
                    b.Descricao,
                    b.Uf,
                    Csts = b.Csts.Select(x => x.Cst).ToList(),
                    Csosns = b.Csosns.Select(x => x.Csosn).ToList()
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterCodigoBeneficioFiscalPorIdQueryHandler : IRequestHandler<ObterCodigoBeneficioFiscalPorIdQuery, CommandResult>
    {
        private readonly ContextFiscal _context;

        public ObterCodigoBeneficioFiscalPorIdQueryHandler(ContextFiscal context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterCodigoBeneficioFiscalPorIdQuery request, CancellationToken cancellationToken)
        {
            var b = await _context.CodigosBeneficiosFiscais
                .AsNoTracking()
                .Include(x => x.Csts)
                .Include(x => x.Csosns)
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);

            if (b == null)
            {
                return CommandResult.Falha("Código de Benefício Fiscal não encontrado.");
            }

            var dto = new
            {
                b.Id,
                b.Codigo,
                b.Descricao,
                b.Uf,
                Csts = b.Csts.Select(x => x.Cst).ToList(),
                Csosns = b.Csosns.Select(x => x.Csosn).ToList()
            };

            return CommandResult.Ok("OK", dto);
        }
    }
}
