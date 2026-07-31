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

        // 1.06 — Limite de CLIENTES (customers do tenant) e de PERMISSÕES/PAPÉIS (RBAC) por tenant.
        Task<(bool Excedido, string Mensagem)> ValidarLimiteClientesAsync(string tenantId, CancellationToken cancellationToken = default);
        Task<(bool Excedido, string Mensagem)> ValidarLimitePermissoesAsync(string tenantId, CancellationToken cancellationToken = default);
    }
}
