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
    /// <summary>Lista Zonas Entrega.</summary>
    public class ListarZonasEntregaQueryHandler : IQueryHandler<ListarZonasEntregaQuery, IEnumerable<ZonaEntregaDto>>
    {
        private readonly ContextGestaoClientes _context;

        public ListarZonasEntregaQueryHandler(ContextGestaoClientes context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ZonaEntregaDto>> Handle(ListarZonasEntregaQuery request, CancellationToken cancellationToken)
        {
            return await _context.ZonasEntrega
                .Where(z => z.DeletadoEm == null)
                .Select(z => new ZonaEntregaDto
                {
                    Id = z.Id,
                    Nome = z.Nome,
                    CepInicio = z.CepInicio,
                    CepFim = z.CepFim,
                    Ativo = z.Ativo
                })
                .ToListAsync(cancellationToken);
        }
    }
}
