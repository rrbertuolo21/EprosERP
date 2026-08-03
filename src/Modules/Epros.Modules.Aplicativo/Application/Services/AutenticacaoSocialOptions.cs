using Epros.Modules.Aplicativo.Domain.Entities;

namespace Epros.Modules.Aplicativo.Application.Services
{
    /// <summary>
    /// Configuração de um provedor de login social (OAuth 2.0 / OIDC), lida de <c>IConfiguration</c>
    /// (seção <c>Autenticacao:Social:{Google|Microsoft}</c>). NENHUM segredo é hardcoded — o
    /// ClientSecret vem de env/secret em produção. Se não configurado (<see cref="Configurado"/> = false),
    /// os endpoints sociais retornam erro claro "provedor não configurado" sem quebrar o app.
    /// </summary>
    public sealed class ProvedorSocialOptions
    {
        /// <summary>ID do cliente OAuth registrado no provedor.</summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>Segredo do cliente OAuth (via env/secret em produção; placeholder vazio em appsettings).</summary>
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>URI de redirecionamento registrada no provedor (deve bater exatamente com a do callback).</summary>
        public string RedirectUri { get; set; } = string.Empty;

        /// <summary>
        /// Authority OIDC (emissor). Ex.: Google = <c>https://accounts.google.com</c>;
        /// Microsoft = <c>https://login.microsoftonline.com/common/v2.0</c>. O discovery document é
        /// buscado em <c>{Authority}/.well-known/openid-configuration</c>.
        /// </summary>
        public string Authority { get; set; } = string.Empty;

        /// <summary>Escopos solicitados. Padrão OIDC mínimo com e-mail e perfil.</summary>
        public string Scopes { get; set; } = "openid email profile";

        /// <summary>True quando os campos mínimos para iniciar o fluxo estão preenchidos.</summary>
        public bool Configurado =>
            !string.IsNullOrWhiteSpace(ClientId)
            && !string.IsNullOrWhiteSpace(ClientSecret)
            && !string.IsNullOrWhiteSpace(RedirectUri)
            && !string.IsNullOrWhiteSpace(Authority);
    }

    /// <summary>Raiz das configurações de login social (bind da seção <c>Autenticacao:Social</c>).</summary>
    public sealed class AutenticacaoSocialOptions
    {
        public const string SecaoConfig = "Autenticacao:Social";

        public ProvedorSocialOptions Google { get; set; } = new();
        public ProvedorSocialOptions Microsoft { get; set; } = new();

        /// <summary>Resolve as opções do provedor pedido.</summary>
        public ProvedorSocialOptions? Para(ProvedorSocial provedor) => provedor switch
        {
            ProvedorSocial.Google => Google,
            ProvedorSocial.Microsoft => Microsoft,
            _ => null
        };
    }
}
