using System;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Application.Dtos;
using Epros.Modules.GestaoClientes.Domain.Entities;

namespace Epros.Modules.GestaoClientes.Application.Contracts
{
    public interface IConfiguracaoGlobalCache
    {
        Task<ConfiguracaoGlobalCacheDto?> ObterAsync(string chave, Func<Task<ConfiguracaoGlobal?>> factory);
        Task InvalidarAsync(string chave);
    }
}
