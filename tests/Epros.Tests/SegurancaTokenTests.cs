using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Epros.Shared.Security;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Prova adversarial do fechamento do furo cross-tenant: o token antigo em texto plano
    /// (jwt-token-completo-{tenant}-{user}-...) era forjável por qualquer um. Agora o token é um
    /// JWT assinado (HS256) com expiração e validação de assinatura/emissor/audiência.
    /// </summary>
    public class SegurancaTokenTests
    {
        private const string ChaveValida = "epros-test-signing-key-0123456789-abcdef";
        private readonly IEprosTokenService _service = new EprosTokenService(ChaveValida);

        [Fact(DisplayName = "SegurancaToken | Token forjado em texto plano deve ser REJEITADO")]
        public void TokenForjadoTextoPlano_DeveSerRejeitado()
        {
            // Ataque real: string montada à mão personificando outro tenant/usuário.
            var tokenForjado = $"jwt-token-completo-tenantX-{Guid.NewGuid()}-{Guid.NewGuid()}-{Guid.NewGuid()}-{Guid.NewGuid()}";

            var principal = _service.Validar(tokenForjado);

            Assert.Null(principal);
        }

        [Fact(DisplayName = "SegurancaToken | Token válido adulterado em 1 caractere deve ser REJEITADO")]
        public void TokenAdulterado_DeveSerRejeitado()
        {
            var tenantId = "tenant-A";
            var usuarioId = Guid.NewGuid().ToString();
            var tokenValido = _service.GerarBasico(tenantId, usuarioId);

            // Adultera 1 caractere no MEIO do payload (2º segmento). Isso muda o conteúdo
            // assinado de forma determinística (chars centrais do base64url usam os 6 bits),
            // invalidando a assinatura sempre — evita o não-determinismo de mexer no último
            // caractere, que pode cair em bits de padding e não alterar os bytes decodificados.
            var partes = tokenValido.Split('.');
            var payload = partes[1];
            var meio = payload.Length / 2;
            var atual = payload[meio];
            var novo = atual == 'A' ? 'B' : 'A';
            partes[1] = payload[..meio] + novo + payload[(meio + 1)..];
            var tokenAdulterado = string.Join('.', partes);

            Assert.NotEqual(tokenValido, tokenAdulterado);
            Assert.Null(_service.Validar(tokenAdulterado));
        }

        [Fact(DisplayName = "SegurancaToken | Token assinado com OUTRA chave deve ser REJEITADO")]
        public void TokenAssinadoComOutraChave_DeveSerRejeitado()
        {
            // Atacante que conhece o formato mas não a chave secreta.
            var outroServico = new EprosTokenService("chave-do-atacante-totalmente-diferente-123456");
            var tokenAtacante = outroServico.GerarCompleto("tenantX", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            Assert.Null(_service.Validar(tokenAtacante));
        }

        [Fact(DisplayName = "SegurancaToken | Token válido deve ser ACEITO com os claims corretos")]
        public void TokenValido_DeveSerAceito_ComClaimsCorretos()
        {
            var tenantId = "tenant-A";
            var usuarioId = Guid.NewGuid().ToString();
            var empresaId = Guid.NewGuid().ToString();
            var perfilId = Guid.NewGuid().ToString();

            var token = _service.GerarCompleto(tenantId, usuarioId, empresaId, perfilId);
            var principal = _service.Validar(token);

            Assert.NotNull(principal);
            Assert.Equal(usuarioId, principal!.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            Assert.Equal(tenantId, principal.FindFirst("tenantId")?.Value);
            Assert.Equal(empresaId, principal.FindFirst("empresaId")?.Value);
            Assert.Equal(perfilId, principal.FindFirst("perfilId")?.Value);
        }

        [Fact(DisplayName = "SegurancaToken | Token expirado deve ser REJEITADO")]
        public void TokenExpirado_DeveSerRejeitado()
        {
            // Token corretamente assinado com a MESMA chave, mas já expirado.
            var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ChaveValida));
            var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);
            var passado = DateTime.UtcNow.AddHours(-2);

            var jwt = new JwtSecurityToken(
                issuer: EprosTokenService.Issuer,
                audience: EprosTokenService.Audience,
                claims: new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) },
                notBefore: passado,
                expires: passado.AddHours(1), // expirou há ~1h
                signingCredentials: credenciais);

            var tokenExpirado = new JwtSecurityTokenHandler().WriteToken(jwt);

            Assert.Null(_service.Validar(tokenExpirado));
        }

        [Fact(DisplayName = "SegurancaToken | Chave curta (< 32 chars) deve falhar no startup (fail-closed)")]
        public void ChaveCurta_DeveLancarNoStartup()
        {
            Assert.Throws<InvalidOperationException>(() => new EprosTokenService("curta"));
        }
    }
}
