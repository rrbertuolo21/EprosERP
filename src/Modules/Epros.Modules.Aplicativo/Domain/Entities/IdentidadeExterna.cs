using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Aplicativo.Domain.Entities
{
    /// <summary>Provedor de identidade social (OAuth 2.0 / OIDC) suportado no login social (1.04 PASS 3).</summary>
    public enum ProvedorSocial
    {
        /// <summary>Google (accounts.google.com).</summary>
        Google = 1,
        /// <summary>Microsoft / Entra ID (login.microsoftonline.com).</summary>
        Microsoft = 2
    }

    /// <summary>
    /// Vínculo entre uma conta social (Google/Microsoft) e a identidade global (<see cref="Usuario"/>,
    /// chaveada por e-mail único global). É a entidade que faltava na 1.04: liga o <c>sub</c>/<c>oid</c>
    /// do provedor OIDC à identidade do EprosERP, permitindo login social sem senha.
    ///
    /// O par (<see cref="Provedor"/>, <see cref="SubjectId"/>) é ÚNICO — o <c>sub</c> é o identificador
    /// estável e imutável da conta no provedor (nunca reciclado), então é a chave de correspondência
    /// correta (o e-mail pode mudar e não deve ser a chave). O <see cref="EmailProvedor"/> é guardado
    /// apenas para auditoria/exibição. O <c>TenantId</c> herdado é o tenant de origem da identidade
    /// global vinculada; a leitura no callback (anônimo quanto ao inquilino) é cross-tenant via a
    /// política RLS <c>auth_cross_tenant_select</c>, como em <c>usuarios</c>.
    /// </summary>
    public class IdentidadeExterna : EntidadeSaaSBase
    {
        /// <summary>Identidade global vinculada (Usuario.Id — e-mail é único global).</summary>
        public Guid UsuarioId { get; private set; }

        /// <summary>Provedor social da conta (Google/Microsoft).</summary>
        public ProvedorSocial Provedor { get; private set; }

        /// <summary>Identificador estável do usuário no provedor (claim <c>sub</c> do OIDC; <c>oid</c> na Microsoft).</summary>
        public string SubjectId { get; private set; } = string.Empty;

        /// <summary>E-mail informado pelo provedor (auditoria/exibição — não é a chave de vínculo).</summary>
        public string EmailProvedor { get; private set; } = string.Empty;

        /// <summary>Vínculo ativo? Vínculo inativo não permite login social por essa conta.</summary>
        public bool Ativo { get; private set; }

        protected IdentidadeExterna() { } // EF Core

        public IdentidadeExterna(
            string tenantId,
            Guid usuarioId,
            ProvedorSocial provedor,
            string subjectId,
            string emailProvedor,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
                AddNotification(nameof(TenantId), "O tenant do vínculo é obrigatório.");
            if (usuarioId == Guid.Empty)
                AddNotification(nameof(UsuarioId), "O ID do usuário é obrigatório.");
            if (string.IsNullOrWhiteSpace(subjectId))
                AddNotification(nameof(SubjectId), "O identificador do provedor (sub) é obrigatório.");

            UsuarioId = usuarioId;
            Provedor = provedor;
            SubjectId = subjectId?.Trim() ?? string.Empty;
            EmailProvedor = emailProvedor?.ToLowerInvariant().Trim() ?? string.Empty;
            Ativo = true;
        }

        /// <summary>Atualiza o e-mail informado pelo provedor (o usuário pode trocar o e-mail da conta social).</summary>
        public void AtualizarEmailProvedor(string emailProvedor, string alteradoPor)
        {
            EmailProvedor = emailProvedor?.ToLowerInvariant().Trim() ?? string.Empty;
            MarcarAlterado(alteradoPor);
        }

        public void Ativar(string alteradoPor)
        {
            Ativo = true;
            MarcarAlterado(alteradoPor);
        }

        public void Desativar(string alteradoPor)
        {
            Ativo = false;
            MarcarAlterado(alteradoPor);
        }
    }
}
