using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using Epros.Modules.Aplicativo.Application.Dtos;

namespace Epros.Modules.Aplicativo.Application.Queries
{
    public record ObterAssinaturaVigenteQuery : IQuery<AssinaturaClienteDto>;

    public record ListarPlanosPublicosQuery : IQuery<IEnumerable<PlanoPublicoDto>>;

    public record ListarFaturasTenantQuery(
        int PageIndex = 1,
        int PageSize = 50
    ) : IQuery<IEnumerable<FaturaTenantDto>>;
}
