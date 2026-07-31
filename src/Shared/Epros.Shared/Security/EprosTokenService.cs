using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Epros.Shared.Security
{
    /// <summary>
    /// Serviço central de emissão e validação do token nativo do EprosERP.
    ///
    /// Substitui o antigo "token" em texto plano (jwt-token-completo-{tenant}-{user}-...),
    /// que era forjável por qualquer um via string, por um JWT assinado (HS256) com expiração.
    /// A validação verifica assinatura, emissor, audiência e tempo de vida — token forjado ou
    /// adulterado é rejeitado (Validar retorna null).
    ///
    /// Claims (shape idêntico ao que os handlers de autorização consomem hoje):
    ///   - ClaimTypes.NameIdentifier = usuarioId
    ///   - "tenantId"                = tenantId
    ///   - "empresaId"              = empresaId (apenas token completo)
    ///   - "perfilId"               = perfilId  (apenas token completo)
    ///   - jti                       = Guid da sessão
    ///   - exp                       = agora + 8h
    /// </summary>
    public interface IEprosTokenService
    {
        string GerarCompleto(string tenantId, string usuarioId, string empresaId, string perfilId, string? jti = null);
        string GerarBasico(string tenantId, string usuarioId, string? jti = null);
        ClaimsPrincipal? Validar(string token);

        /// <summary>Tempo de vida único do token emitido (fonte de verdade da expiração — REG-024).</summary>
        TimeSpan Validade { get; }
    }

    /// <summary>
    /// Convenção de revogação de sessão (logout — REG-013). O logout revoga as <c>SessaoUsuario</c>
    /// ativas do usuário (registro persistente/auditoria) e grava em cache um marco "logout em T".
    /// O handler de autenticação rejeita qualquer token cujo instante de emissão (nbf/iat) seja
    /// ANTERIOR a esse marco — invalidando, na borda e para toda a API, todos os tokens (básico e
    /// completo) emitidos antes do logout, sem depender de estado por-jti. Chave por usuário.
    /// </summary>
    public static class RevogacaoSessao
    {
        public static string ChaveCache(string usuarioId) => $"auth:logout-after:{usuarioId}";
    }

    public sealed class EprosTokenService : IEprosTokenService
    {
        public const string Issuer = "epros";
        public const string Audience = "epros";

        // Fonte ÚNICA da expiração do token (8h). Antes o DTO/sessão anunciavam 10h enquanto o JWT
        // expirava em 8h (REG-024 DIVERGENTE). Agora todos os pontos derivam desta propriedade.
        private static readonly TimeSpan ValidadeToken = TimeSpan.FromHours(8);

        /// <inheritdoc />
        public TimeSpan Validade => ValidadeToken;

        private readonly SymmetricSecurityKey _chave;
        private readonly JwtSecurityTokenHandler _handler = new();

        public EprosTokenService(string signingKey)
        {
            if (string.IsNullOrWhiteSpace(signingKey) || Encoding.UTF8.GetByteCount(signingKey) < 32)
            {
                throw new InvalidOperationException(
                    "Chave de assinatura JWT inválida: configure 'Seguranca:JwtSigningKey' com pelo menos 32 caracteres.");
            }

            _chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        }

        public string GerarBasico(string tenantId, string usuarioId, string? jti = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuarioId),
                new Claim("tenantId", tenantId),
                new Claim(JwtRegisteredClaimNames.Jti, string.IsNullOrWhiteSpace(jti) ? Guid.NewGuid().ToString() : jti)
            };

            return Gerar(claims);
        }

        public string GerarCompleto(string tenantId, string usuarioId, string empresaId, string perfilId, string? jti = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuarioId),
                new Claim("tenantId", tenantId),
                new Claim(JwtRegisteredClaimNames.Jti, string.IsNullOrWhiteSpace(jti) ? Guid.NewGuid().ToString() : jti)
            };

            if (!string.IsNullOrEmpty(empresaId) && empresaId != "null")
            {
                claims.Add(new Claim("empresaId", empresaId));
            }
            if (!string.IsNullOrEmpty(perfilId) && perfilId != "null")
            {
                claims.Add(new Claim("perfilId", perfilId));
            }

            return Gerar(claims);
        }

        public ClaimsPrincipal? Validar(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            var parametros = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _chave,
                ValidateLifetime = true,
                ValidateIssuer = true,
                ValidIssuer = Issuer,
                ValidateAudience = true,
                ValidAudience = Audience,
                ClockSkew = TimeSpan.FromSeconds(30),
                NameClaimType = ClaimTypes.NameIdentifier
            };

            try
            {
                return _handler.ValidateToken(token, parametros, out _);
            }
            catch
            {
                // Assinatura inválida, expirado, emissor/audiência divergente, malformado: rejeita.
                return null;
            }
        }

        private string Gerar(IEnumerable<Claim> claims)
        {
            var agora = DateTime.UtcNow;
            var credenciais = new SigningCredentials(_chave, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                notBefore: agora,
                expires: agora.Add(ValidadeToken),
                signingCredentials: credenciais);

            return _handler.WriteToken(token);
        }
    }
}
