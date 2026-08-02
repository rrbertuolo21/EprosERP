using System.Security.Claims;
using Epros.Shared.Application.Contracts;
using Microsoft.AspNetCore.Http;

namespace Epros.API.Providers
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public string? GetUserName()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.Name;
        }

        public string? GetUserEmail()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
        }

        /// <summary>
        /// Lê a claim do principal autenticado. Base para o isolamento dos portais externos
        /// (clienteId/fornecedorId/portalUsuarioId) — o valor vem do token assinado, nunca do request.
        /// </summary>
        public string? GetClaim(string claimType)
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(claimType)?.Value;
        }
    }
}
