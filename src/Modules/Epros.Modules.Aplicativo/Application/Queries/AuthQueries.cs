using System;
using System.Collections.Generic;
using Epros.Modules.Aplicativo.Application.Dtos;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Aplicativo.Application.Queries
{
    public record ListarEmpresasDisponiveisQuery(Guid UsuarioId) : IQuery<IEnumerable<UsuarioEmpresaDto>>;

    /// <summary>Acesso multi-tenant (1.04 PASS 2): lista os tenants a que a identidade global tem acesso ativo.</summary>
    public record ListarTenantsDisponiveisQuery(Guid UsuarioId) : IQuery<IEnumerable<TenantDisponivelDto>>;
}
