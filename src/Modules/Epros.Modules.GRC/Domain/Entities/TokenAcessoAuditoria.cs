using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-CIA — Token de acesso externo à auditoria (grc_cia_token_acesso). D-CIA-04:
    /// acesso externo (auditor) por token com EXPIRAÇÃO OBRIGATÓRIA parametrizada (RN-CIA-006..009).
    /// Guarda só o HASH do token (nunca o valor em claro). Gap genuíno da EF (EF×código #8).
    /// </summary>
    public class TokenAcessoAuditoria : EntidadeSaaSBase
    {
        public Guid PlanoAuditoriaId { get; private set; }
        public string TokenHash { get; private set; } = string.Empty;
        public Guid? AuditorId { get; private set; }
        public string? Escopo { get; private set; }
        public DateTime ExpiraEm { get; private set; }
        public bool Revogado { get; private set; }
        public DateTime? UltimoAcessoEm { get; private set; }

        protected TokenAcessoAuditoria() { } // EF Core

        public TokenAcessoAuditoria(
            Guid planoAuditoriaId,
            string tokenHash,
            Guid? auditorId,
            string? escopo,
            DateTime expiraEm,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<TokenAcessoAuditoria>()
                .Requires()
                .IsTrue(planoAuditoriaId != Guid.Empty, nameof(PlanoAuditoriaId), "O plano de auditoria e obrigatorio.")
                .IsNotNullOrEmpty(tokenHash, nameof(TokenHash), "O hash do token e obrigatorio.")
                // D-CIA-04 — expiração obrigatória: token de acesso externo sem prazo é proibido.
                .IsTrue(expiraEm > DateTime.UtcNow, nameof(ExpiraEm), "O token deve ter expiracao futura obrigatoria.")
            );

            PlanoAuditoriaId = planoAuditoriaId;
            TokenHash = tokenHash;
            AuditorId = auditorId;
            Escopo = escopo;
            ExpiraEm = expiraEm;
            Revogado = false;
        }

        public bool EstaValido(DateTime referencia) => !Revogado && ExpiraEm > referencia;

        public void RegistrarAcesso(DateTime referencia)
        {
            UltimoAcessoEm = referencia;
        }

        public void Revogar(string usuario)
        {
            Revogado = true;
            MarcarAlterado(usuario);
        }
    }
}
