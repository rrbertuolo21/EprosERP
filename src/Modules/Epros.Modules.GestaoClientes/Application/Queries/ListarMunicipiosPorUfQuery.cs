using System.Collections.Generic;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    public record ListarMunicipiosPorUfQuery(string Uf) : IQuery<IEnumerable<MunicipioDto>>;
}
