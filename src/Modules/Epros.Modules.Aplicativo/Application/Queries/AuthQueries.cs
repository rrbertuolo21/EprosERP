using System;
using System.Collections.Generic;
using Epros.Modules.Aplicativo.Application.Dtos;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Aplicativo.Application.Queries
{
    public record ListarEmpresasDisponiveisQuery(Guid UsuarioId) : IQuery<IEnumerable<UsuarioEmpresaDto>>;
}
