using System;
using System.Collections.Generic;
using Epros.Modules.Aplicativo.Application.Dtos;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.Aplicativo.Application.Queries
{
    public record ObterUsuarioQuery(Guid UsuarioId) : IQuery<UsuarioDetalhadoDto>;

    public record ListarUsuariosQuery(
        string? Search, 
        int PageIndex = 1, 
        int PageSize = 200
    ) : IQuery<IEnumerable<UsuarioDto>>;

    public record ListarHistoricoLoginQuery(
        string? Search, 
        int PageIndex = 1, 
        int PageSize = 200
    ) : IQuery<IEnumerable<HistoricoLoginDto>>;
}
