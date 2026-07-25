using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Domain.Entities;

namespace Epros.Modules.Aplicativo.Application.Services
{
    public interface IPermissaoCacheManager
    {
        Task<List<PerfilAcessoMenu>> ObterPermissoesEfetivasAsync(string tenantId, Guid usuarioId, Guid empresaId, Func<Task<List<PerfilAcessoMenu>>> factory);
        void InvalidarCacheUsuario(string tenantId, Guid usuarioId);
        void InvalidarCacheEmpresa(string tenantId, Guid empresaId);
        void InvalidarCachePerfil(string tenantId, Guid perfilId);
    }
}
