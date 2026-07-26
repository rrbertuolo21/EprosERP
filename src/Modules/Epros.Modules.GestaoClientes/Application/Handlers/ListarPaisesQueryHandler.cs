using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Infrastructure.Data;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Lista Paises.</summary>
    public class ListarPaisesQueryHandler : IQueryHandler<ListarPaisesQuery, IEnumerable<PaisDto>>
    {
        private readonly ContextGestaoClientes _context;

        public ListarPaisesQueryHandler(ContextGestaoClientes context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PaisDto>> Handle(ListarPaisesQuery request, CancellationToken cancellationToken)
        {
            return await _context.Paises
                .OrderBy(p => p.Nome)
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
                .ToListAsync(cancellationToken);
        }
    }
}
