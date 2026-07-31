using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Dtos;
using Epros.Modules.Aplicativo.Application.Services;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.Aplicativo.Infrastructure.Services;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Handlers
{
    /// <summary>Estado do fluxo social guardado entre <c>/start</c> e <c>/callback</c> (anti-CSRF + anti-replay).</summary>
    internal sealed record EstadoLoginSocial(ProvedorSocial Provedor, string Nonce);

    internal static class LoginSocialSuporte
    {
        /// <summary>Chave de cache do state (uso único).</summary>
        public static string ChaveState(string state) => $"social:state:{state}";

        /// <summary>Traduz o parâmetro de rota em <see cref="ProvedorSocial"/> (case-insensitive).</summary>
        public static bool TentarResolverProvedor(string? provedor, out ProvedorSocial resultado)
        {
            switch ((provedor ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "google": resultado = ProvedorSocial.Google; return true;
                case "microsoft": resultado = ProvedorSocial.Microsoft; return true;
                default: resultado = default; return false;
            }
        }
    }

    /// <summary>
    /// Login social (1.04 PASS 3) — início do fluxo OAuth/OIDC (Authorization Code). Gera state (anti-CSRF)
    /// e nonce (anti-replay), guarda-os em cache (TTL curto) e devolve a URL de autorização do provedor.
    /// Se o provedor não estiver configurado, retorna erro claro sem quebrar o app.
    /// </summary>
    public class IniciarLoginSocialCommandHandler : ICommandHandler<IniciarLoginSocialCommand>
    {
        private static readonly TimeSpan ValidadeState = TimeSpan.FromMinutes(10);

        private readonly IOidcSocialClient _oidc;
        private readonly IMemoryCache _cache;
        private readonly AutenticacaoSocialOptions _opcoes;

        public IniciarLoginSocialCommandHandler(IOidcSocialClient oidc, IMemoryCache cache, IOptions<AutenticacaoSocialOptions> opcoes)
        {
            _oidc = oidc;
            _cache = cache;
            _opcoes = opcoes.Value;
        }

        public async Task<CommandResult> Handle(IniciarLoginSocialCommand request, CancellationToken cancellationToken)
        {
            if (!LoginSocialSuporte.TentarResolverProvedor(request.Provedor, out var provedor))
            {
                return CommandResult.Falha(new[] { "Provedor social inválido. Use 'google' ou 'microsoft'." });
            }

            var opcoesProvedor = _opcoes.Para(provedor);
            if (opcoesProvedor == null || !opcoesProvedor.Configurado)
            {
                return CommandResult.Falha(new[] { $"O provedor '{provedor}' não está configurado nesta instalação." });
            }

            var state = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            var nonce = Guid.NewGuid().ToString("N");

            string url;
            try
            {
                url = await _oidc.MontarUrlAutorizacaoAsync(opcoesProvedor, state, nonce, cancellationToken);
            }
            catch (OidcSocialException ex)
            {
                return CommandResult.Falha(new[] { ex.Message });
            }

            _cache.Set(LoginSocialSuporte.ChaveState(state), new EstadoLoginSocial(provedor, nonce), ValidadeState);

            return CommandResult.Ok("URL de autorização gerada.", new IniciarLoginSocialDto(provedor.ToString(), url));
        }
    }

    /// <summary>
    /// Login social (1.04 PASS 3) — callback do provedor. Valida state (anti-CSRF), troca o code por token,
    /// valida o id_token e resolve a identidade:
    ///  (a) existe IdentidadeExterna (provedor, sub) → loga aquele Usuario;
    ///  (b) senão, e-mail VERIFICADO casa com Usuario existente → vincula (cria IdentidadeExterna) e loga;
    ///  (c) senão (identidade nova) → encaminha ao onboarding (não cria tenant fiscal sozinho — REG-036).
    /// Após autenticar, cai no MESMO fluxo multi-tenant do Pass 2 (EmitirResultadoLoginAsync).
    /// </summary>
    public class CallbackLoginSocialCommandHandler : ICommandHandler<CallbackLoginSocialCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ContextGestaoClientes _contextGestao;
        private readonly IOidcSocialClient _oidc;
        private readonly IMemoryCache _cache;
        private readonly AutenticacaoSocialOptions _opcoes;
        private readonly IEprosTokenService _tokenService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CallbackLoginSocialCommandHandler(
            ContextAplicativo context,
            ContextGestaoClientes contextGestao,
            IOidcSocialClient oidc,
            IMemoryCache cache,
            IOptions<AutenticacaoSocialOptions> opcoes,
            IEprosTokenService tokenService,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _contextGestao = contextGestao;
            _oidc = oidc;
            _cache = cache;
            _opcoes = opcoes.Value;
            _tokenService = tokenService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CommandResult> Handle(CallbackLoginSocialCommand request, CancellationToken cancellationToken)
        {
            if (!LoginSocialSuporte.TentarResolverProvedor(request.Provedor, out var provedor))
            {
                return CommandResult.Falha(new[] { "Provedor social inválido." });
            }

            // O provedor pode retornar erro (ex.: usuário negou consentimento).
            if (!string.IsNullOrWhiteSpace(request.Error))
            {
                return CommandResult.Falha(new[] { "Autenticação social não concluída no provedor." });
            }

            if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.State))
            {
                return CommandResult.Falha(new[] { "Parâmetros do callback social ausentes (code/state)." });
            }

            // 1. Valida o state (anti-CSRF). Uso único: remove ao consumir.
            var chaveState = LoginSocialSuporte.ChaveState(request.State);
            if (!_cache.TryGetValue<EstadoLoginSocial>(chaveState, out var estado) || estado is null || estado.Provedor != provedor)
            {
                return CommandResult.Falha(new[] { "State inválido ou expirado. Reinicie o login social." });
            }
            _cache.Remove(chaveState);

            var opcoesProvedor = _opcoes.Para(provedor);
            if (opcoesProvedor == null || !opcoesProvedor.Configurado)
            {
                return CommandResult.Falha(new[] { $"O provedor '{provedor}' não está configurado nesta instalação." });
            }

            // 2. Troca o code por token e valida o id_token (assinatura/emissor/audiência/nonce).
            PerfilSocial perfil;
            try
            {
                perfil = await _oidc.TrocarCodigoPorPerfilAsync(opcoesProvedor, provedor, request.Code!, estado.Nonce, cancellationToken);
            }
            catch (OidcSocialException ex)
            {
                return CommandResult.Falha(new[] { $"Falha na autenticação social: {ex.Message}" });
            }

            var emailProvedor = perfil.Email?.ToLowerInvariant().Trim();

            // 3. Existe vínculo (provedor, sub)? Leitura cross-tenant (callback é anônimo quanto ao inquilino).
            await AuthRlsBypass.EnableAsync(_context, cancellationToken);
            var identidade = await _context.IdentidadesExternas
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(i => i.Provedor == provedor && i.SubjectId == perfil.SubjectId && i.DeletadoEm == null, cancellationToken);
            await AuthRlsBypass.DisableAsync(_context, cancellationToken);

            if (identidade != null)
            {
                if (!identidade.Ativo)
                {
                    return CommandResult.Falha(new[] { "Vínculo social inativo. Contate o administrador." });
                }

                var usuarioVinculado = await CarregarUsuarioAtivoAsync(identidade.UsuarioId, cancellationToken);
                if (usuarioVinculado == null)
                {
                    return CommandResult.Falha(new[] { "Não foi possível autenticar com o provedor social." });
                }

                // Mantém o e-mail do provedor atualizado (auditoria) sem trocar a chave de vínculo (sub).
                if (!string.IsNullOrWhiteSpace(emailProvedor) && emailProvedor != identidade.EmailProvedor)
                {
                    identidade.AtualizarEmailProvedor(emailProvedor!, "login-social");
                }

                return await AcessoMultiTenant.EmitirResultadoLoginAsync(
                    _context, _contextGestao, _tokenService, _httpContextAccessor,
                    usuarioVinculado, request.IpAddress, request.UserAgent,
                    emailProvedor ?? usuarioVinculado.Email, $"Login social ({provedor}).", cancellationToken);
            }

            // 4. Sem vínculo. Só é seguro vincular a uma conta existente se o e-mail for VERIFICADO.
            //    (Não confiar em e-mail não verificado para assumir a identidade de um usuário existente.)
            if (!perfil.EmailVerificado || string.IsNullOrWhiteSpace(emailProvedor))
            {
                return CommandResult.Ok(
                    "Cadastro social pendente. Complete o onboarding para concluir.",
                    new LoginSocialPendenteDto(true, provedor.ToString(), perfil.SubjectId, emailProvedor, perfil.Nome));
            }

            // 4.1. E-mail verificado casa com um Usuario existente? Vincula (cria IdentidadeExterna) e loga.
            await AuthRlsBypass.EnableAsync(_context, cancellationToken);
            var usuarioPorEmail = await _context.Usuarios
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == emailProvedor && u.DeletadoEm == null, cancellationToken);
            await AuthRlsBypass.DisableAsync(_context, cancellationToken);

            if (usuarioPorEmail == null)
            {
                // 5. Identidade nova de fato: social NÃO cria tenant fiscal sozinho (REG-036) — encaminha
                //    ao onboarding, que aplica a validação fiscal do self-register. O front pré-preenche
                //    com estes dados verificados; a vinculação da IdentidadeExterna ocorre no onboarding.
                return CommandResult.Ok(
                    "Cadastro social pendente. Complete o onboarding para concluir.",
                    new LoginSocialPendenteDto(true, provedor.ToString(), perfil.SubjectId, emailProvedor, perfil.Nome));
            }

            if (usuarioPorEmail.Status != UsuarioStatus.Active)
            {
                return CommandResult.Falha(new[] { "Não foi possível autenticar com o provedor social." });
            }

            // Escopa a requisição ao tenant do usuário para a escrita do vínculo satisfazer a RLS (WITH CHECK).
            if (_httpContextAccessor.HttpContext != null)
            {
                _httpContextAccessor.HttpContext.Items["TenantId"] = usuarioPorEmail.TenantId;
            }

            var novoVinculo = new IdentidadeExterna(
                tenantId: usuarioPorEmail.TenantId,
                usuarioId: usuarioPorEmail.Id,
                provedor: provedor,
                subjectId: perfil.SubjectId,
                emailProvedor: emailProvedor!,
                criadoPor: "login-social");

            if (!novoVinculo.IsValid)
            {
                return CommandResult.Falha(novoVinculo.Notifications.Select(n => n.Message), "Erro ao vincular a identidade social.");
            }

            _context.IdentidadesExternas.Add(novoVinculo);
            await _context.SaveChangesAsync(cancellationToken);

            return await AcessoMultiTenant.EmitirResultadoLoginAsync(
                _context, _contextGestao, _tokenService, _httpContextAccessor,
                usuarioPorEmail, request.IpAddress, request.UserAgent,
                emailProvedor, $"Login social ({provedor}) — vínculo criado.", cancellationToken);
        }

        /// <summary>Carrega o Usuario (cross-tenant) exigindo status Ativo. Null se ausente/inativo.</summary>
        private async Task<Usuario?> CarregarUsuarioAtivoAsync(Guid usuarioId, CancellationToken cancellationToken)
        {
            await AuthRlsBypass.EnableAsync(_context, cancellationToken);
            var usuario = await _context.Usuarios
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == usuarioId && u.DeletadoEm == null, cancellationToken);
            await AuthRlsBypass.DisableAsync(_context, cancellationToken);

            return usuario is { Status: UsuarioStatus.Active } ? usuario : null;
        }
    }
}
