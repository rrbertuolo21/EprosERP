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
    /// <summary>Lista Municipios Por Id Uf.</summary>
    public class ListarMunicipiosPorIdUfQueryHandler : IQueryHandler<ListarMunicipiosPorIdUfQuery, IEnumerable<MunicipioDto>>
    {
        private readonly ContextGestaoClientes _context;

        public ListarMunicipiosPorIdUfQueryHandler(ContextGestaoClientes context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MunicipioDto>> Handle(ListarMunicipiosPorIdUfQuery request, CancellationToken cancellationToken)
        {
            return await _context.Municipios
                .Include(m => m.Pais)
                .Include(m => m.Subdivisao)
                .Where(m => m.SubdivisaoId == request.SubdivisaoId)
                .OrderBy(m => m.Nome)
                .Select(m => new MunicipioDto
                {
                    Id = m.Id,
                    PaisId = m.PaisId,
                    SubdivisaoId = m.SubdivisaoId,
                    Nome = m.Nome,
                    CodigoIbge = m.CodigoIbge,
                    Latitude = m.Latitude,
                    Longitude = m.Longitude,
                    Ativo = m.Ativo,
                    Uf = m.Subdivisao.CodigoISO31662.Replace("BR-", ""),
                    PaisNome = m.Pais.Nome
                })
                .ToListAsync(cancellationToken);
        }
    }
}
