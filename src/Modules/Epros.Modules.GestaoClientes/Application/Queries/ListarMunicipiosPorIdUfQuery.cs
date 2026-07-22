using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    public record ListarMunicipiosPorIdUfQuery(Guid SubdivisaoId) : IQuery<IEnumerable<MunicipioDto>>;
}
