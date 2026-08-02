using System;

namespace Epros.Shared.Application.Contracts
{
    public interface ICurrentUser
    {
        string? GetUserId();
        string? GetUserName();
        string? GetUserEmail();

        /// <summary>
        /// Lê uma claim arbitrária do principal autenticado. Usado pelos portais externos
        /// (Portal do Cliente / Portal do Fornecedor) para derivar o vínculo (clienteId/fornecedorId)
        /// SEMPRE do sujeito autenticado, NUNCA do request. Default null preserva os fakes de teste
        /// existentes; a implementação real (CurrentUser) lê da identidade HTTP.
        /// valida-ambiente: wiring de login externo do portal (emissão dessas claims no token).
        /// </summary>
        string? GetClaim(string claimType) => null;

        /// <summary>Vínculo de cliente do Portal do Cliente (claim "clienteId"), quando o principal é externo.</summary>
        Guid? GetClienteId()
            => Guid.TryParse(GetClaim("clienteId"), out var id) ? id : (Guid?)null;

        /// <summary>Vínculo de fornecedor do Portal do Fornecedor (claim "fornecedorId"), quando o principal é externo.</summary>
        Guid? GetFornecedorId()
            => Guid.TryParse(GetClaim("fornecedorId"), out var id) ? id : (Guid?)null;

        /// <summary>Usuário externo do portal (claim "portalUsuarioId"), quando emitida pelo login externo.</summary>
        Guid? GetPortalUsuarioId()
            => Guid.TryParse(GetClaim("portalUsuarioId"), out var id) ? id : (Guid?)null;
    }
}
