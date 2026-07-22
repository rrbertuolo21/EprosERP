using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Infrastructure.Data;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Obtém Pais Por Id.</summary>
    public class ObterPaisPorIdQueryHandler : IQueryHandler<ObterPaisPorIdQuery, PaisDto?>
    {
        private readonly ContextGestaoClientes _context;

        public ObterPaisPorIdQueryHandler(ContextGestaoClientes context)
        {
            _context = context;
        }

        public async Task<PaisDto?> Handle(ObterPaisPorIdQuery request, CancellationToken cancellationToken)
        {
            return await _context.Paises
                .Where(p => p.Id == request.Id)
                .Select(p => new PaisDto
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    CodigoIsoAlpha2 = p.CodigoIsoAlpha2,
                    CodigoIsoAlpha3 = p.CodigoIsoAlpha3,
                    CodigoNumerico = p.CodigoNumerico,
                    Capital = p.Capital,
                    CodigoDiscagem = p.CodigoDiscagem,
                    Ativo = p.Ativo
                })
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
