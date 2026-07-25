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
    /// <summary>Lista Municipios Por Uf.</summary>
    public class ListarMunicipiosPorUfQueryHandler : IQueryHandler<ListarMunicipiosPorUfQuery, IEnumerable<MunicipioDto>>
    {
        private readonly ContextGestaoClientes _context;

        public ListarMunicipiosPorUfQueryHandler(ContextGestaoClientes context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MunicipioDto>> Handle(ListarMunicipiosPorUfQuery request, CancellationToken cancellationToken)
        {
            var isoCode = $"BR-{request.Uf.ToUpper()}";
            return await _context.Municipios
                .Include(m => m.Pais)
                .Include(m => m.Subdivisao)
                .Where(m => m.Subdivisao.CodigoISO31662 == isoCode)
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
