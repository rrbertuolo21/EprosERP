using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace Epros.Modules.Aplicativo.Application.Services
{
    public class PermissaoCacheManager : IPermissaoCacheManager
    {
        private readonly IMemoryCache _cache;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

        // Controladores para mapear as chaves de cache ativas para fins de invalidação agrupada
        private readonly ConcurrentDictionary<string, HashSet<string>> _usuarioChaves = new();
        private readonly ConcurrentDictionary<string, HashSet<string>> _empresaChaves = new();
        private readonly ConcurrentDictionary<string, HashSet<string>> _perfilChaves = new();

        public PermissaoCacheManager(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<List<PerfilAcessoMenu>> ObterPermissoesEfetivasAsync(
            string tenantId,
            Guid usuarioId,
            Guid empresaId,
            Func<Task<List<PerfilAcessoMenu>>> factory)
        {
            var cacheKey = $"permissao-menu:{tenantId}:{usuarioId}:{empresaId}";

            if (!_cache.TryGetValue(cacheKey, out List<PerfilAcessoMenu>? permissoes))
            {
                permissoes = await factory();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(CacheDuration);

                _cache.Set(cacheKey, permissoes, cacheEntryOptions);

                // Registra a chave para invalidação futura
                RegistrarChave(usuarioId.ToString(), cacheKey, _usuarioChaves);
                RegistrarChave(empresaId.ToString(), cacheKey, _empresaChaves);

                // Extrai o ID do perfil de acesso para associar a chave
                if (permissoes.Count > 0)
                {
                    var perfilId = permissoes[0].PerfilAcessoId.ToString();
                    RegistrarChave(perfilId, cacheKey, _perfilChaves);
                }
            }

            return permissoes ?? new List<PerfilAcessoMenu>();
        }

        public void InvalidarCacheUsuario(string tenantId, Guid usuarioId)
        {
            InvalidarChaves(usuarioId.ToString(), _usuarioChaves);
        }

        public void InvalidarCacheEmpresa(string tenantId, Guid empresaId)
        {
            InvalidarChaves(empresaId.ToString(), _empresaChaves);
        }

        public void InvalidarCachePerfil(string tenantId, Guid perfilId)
        {
            InvalidarChaves(perfilId.ToString(), _perfilChaves);
        }

        private void RegistrarChave(string id, string cacheKey, ConcurrentDictionary<string, HashSet<string>> dicionario)
        {
            dicionario.AddOrUpdate(
                id,
                new HashSet<string> { cacheKey },
                (key, set) =>
                {
                    lock (set)
                    {
                        set.Add(cacheKey);
                    }
                    return set;
                });
        }

        private void InvalidarChaves(string id, ConcurrentDictionary<string, HashSet<string>> dicionario)
        {
            if (dicionario.TryRemove(id, out var chaves))
            {
                lock (chaves)
                {
                    foreach (var chave in chaves)
                    {
                        _cache.Remove(chave);
                    }
                }
            }
        }
    }
}
