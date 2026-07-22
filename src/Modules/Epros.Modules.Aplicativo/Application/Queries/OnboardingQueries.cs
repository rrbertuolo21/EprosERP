using System;
using System.Collections.Generic;
using Epros.Modules.Aplicativo.Application.Dtos;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Aplicativo.Application.Queries
{
    public record ObterConfiguracaoEmpresaQuery(Guid EmpresaId) : IQuery<ConfiguracaoEmpresaDto>;

    public record ListarIdiomasHabilitadosQuery : IQuery<IEnumerable<IdiomaDto>>;

    public record ObterContextoSessaoQuery(string TenantId, Guid UsuarioId) : IQuery<ContextoSessaoDto>;
}
