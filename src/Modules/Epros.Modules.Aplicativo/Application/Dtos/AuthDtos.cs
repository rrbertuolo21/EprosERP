using System;

namespace Epros.Modules.Aplicativo.Application.Dtos
{
    public record AuthResponseDto(
        string Token,
        DateTime Expiracao,
        Guid UsuarioId,
        string Nome,
        string Email,
        bool ExigeSelecaoEmpresa,
        string TenantId,
        bool Block = false,
        List<UsuarioEmpresaDto>? Empresas = null,
        // Acesso multi-tenant (1.04 PASS 2): quando a identidade global tem acesso a mais de um tenant,
        // o login não escopa direto — devolve a lista e exige seleção de tenant antes da de empresa.
        bool ExigeSelecaoTenant = false,
        List<TenantDisponivelDto>? Tenants = null,
        // UX de entrada (decisão Rafael): identidade que autenticou mas não tem acesso a NENHUMA
        // empresa (0 memberships e 0 vínculos). O login não falha (não é erro cru) — devolve este
        // estado para o front oferecer onboarding/contato em vez de entrar num tenant vazio.
        bool SemAcesso = false
    );

    public record UsuarioEmpresaDto(
        Guid EmpresaId,
        string RazaoSocial,
        bool EhAdmin,
        Guid? PerfilUsuarioId
    );

    /// <summary>Tenant a que a identidade global tem acesso (membership ativa) — item da seleção de tenant.</summary>
    public record TenantDisponivelDto(
        string TenantId,
        string Nome,
        string Papel
    );

    /// <summary>
    /// Login social (1.04 PASS 3): resposta do <c>/start</c>. A URL de autorização já contém o
    /// <c>state</c> (anti-CSRF) e o <c>nonce</c>; o front redireciona o navegador para ela.
    /// </summary>
    public record IniciarLoginSocialDto(
        string Provedor,
        string UrlAutorizacao
    );

    /// <summary>
    /// Login social (1.04 PASS 3): resultado do callback quando a conta social é NOVA (não existe
    /// vínculo por sub e o e-mail verificado não casa com identidade existente). Social não cria tenant
    /// fiscal sozinho (não burla a validação fiscal do self-register — REG-036): o front encaminha ao
    /// onboarding pré-preenchendo com estes dados verificados do provedor.
    /// </summary>
    public record LoginSocialPendenteDto(
        bool PrecisaCompletarCadastro,
        string Provedor,
        string SubjectId,
        string? Email,
        string? Nome
    );
}
