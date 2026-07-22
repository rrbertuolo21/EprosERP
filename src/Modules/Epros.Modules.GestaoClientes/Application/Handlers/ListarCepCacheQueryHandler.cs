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
    /// <summary>Lista Cep Cache.</summary>
    public class ListarCepCacheQueryHandler : IQueryHandler<ListarCepCacheQuery, IEnumerable<CepCacheDto>>
    {
        private readonly ContextGestaoClientes _context;

        public ListarCepCacheQueryHandler(ContextGestaoClientes context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CepCacheDto>> Handle(ListarCepCacheQuery request, CancellationToken cancellationToken)
        {
            return await _context.CodigosPostaisCache
                .IgnoreQueryFilters() // cache é global e contém falhas globais/tenantizadas
                .Include(c => c.Municipio)
                .OrderByDescending(c => c.ConsultadoEm)
                .Select(c => new CepCacheDto
                {
                    Id = c.Id,
                    CodigoPostal = c.CodigoPostal,
                    Logradouro = c.Logradouro,
                    Bairro = c.Bairro,
                    Localidade = c.Municipio != null ? c.Municipio.Nome : string.Empty,
                    Uf = c.Uf,
                    Provedor = c.Provedor,
                    ConsultadoEm = c.ConsultadoEm,
                    Falhou = c.Falhou,
                    MotivoFalha = c.MotivoFalha
                })
                .ToListAsync(cancellationToken);
        }
    }
}
