using System.Threading;
using System.Threading.Tasks;

namespace Epros.Shared.Application.Contracts
{
    public interface IValidadorLimitesSaaS
    {
        Task<bool> PossuiFolgaUsuariosAsync(string tenantId, CancellationToken cancellationToken = default);
        Task<bool> PossuiFolgaEmpresasAsync(string tenantId, CancellationToken cancellationToken = default);
        Task<(bool Excedido, string Mensagem)> ValidarLimiteUsuariosAsync(string tenantId, CancellationToken cancellationToken = default);
        Task<(bool Excedido, string Mensagem)> ValidarLimiteEmpresasAsync(string tenantId, CancellationToken cancellationToken = default);
    }
}
