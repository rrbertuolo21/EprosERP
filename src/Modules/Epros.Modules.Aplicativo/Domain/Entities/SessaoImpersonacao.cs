using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Aplicativo.Domain.Entities
{
    /// <summary>
    /// Natureza da sessão de acesso registrada em <see cref="SessaoImpersonacao"/>. A impersonação
    /// clássica e o acesso por perfil de suporte da Siser compartilham a MESMA trilha de auditoria
    /// (não duplicar); este campo diferencia como o acesso foi concedido.
    /// </summary>
    public enum TipoSessaoAcesso
    {
        /// <summary>Impersonação clássica (operador do sistema assume a conta de um usuário).</summary>
        Impersonacao = 0,
        /// <summary>Acesso concedido por perfil de Suporte Técnico da Siser.</summary>
        SuporteTecnico = 1,
        /// <summary>Acesso concedido por perfil de Suporte Negócio da Siser.</summary>
        SuporteNegocio = 2
    }

    public class SessaoImpersonacao : EntidadeSaaSBase, IHardDeletable
    {
        public Guid UsuarioOriginalId { get; private set; }
        public Guid UsuarioAlvoId { get; private set; }
        public Guid? EmpresaId { get; private set; }
        public DateTime InicioEm { get; private set; }
        public DateTime? FimEm { get; private set; }
        public string? Motivo { get; private set; }
        public string? IpOrigem { get; private set; }

        /// <summary>Como o acesso foi concedido (impersonação clássica ou perfil de suporte da Siser).</summary>
        public TipoSessaoAcesso TipoAcesso { get; private set; }

        /// <summary>
        /// Tenant cliente alvo do acesso. No acesso de suporte, é o cliente em que a Siser entrou
        /// (o <see cref="EntidadeSaaSBase.TenantId"/> herdado é sempre o landlord "system").
        /// </summary>
        public string? TenantAlvo { get; private set; }

        protected SessaoImpersonacao() { } // EF Core

        public SessaoImpersonacao(
            string tenantId,
            Guid usuarioOriginalId,
            Guid usuarioAlvoId,
            Guid? empresaId,
            string? motivo,
            string? ipOrigem,
            string criadoPor,
            TipoSessaoAcesso tipoAcesso = TipoSessaoAcesso.Impersonacao,
            string? tenantAlvo = null)
            : base(tenantId, criadoPor)
        {
            if (usuarioOriginalId == Guid.Empty)
                AddNotification(nameof(UsuarioOriginalId), "O ID do usuário original é obrigatório.");
            if (usuarioAlvoId == Guid.Empty)
                AddNotification(nameof(UsuarioAlvoId), "O ID do usuário alvo é obrigatório.");
            if (usuarioOriginalId == usuarioAlvoId)
                AddNotification(nameof(UsuarioAlvoId), "Não é permitido iniciar impersonação para si mesmo.");

            UsuarioOriginalId = usuarioOriginalId;
            UsuarioAlvoId = usuarioAlvoId;
            EmpresaId = empresaId;
            Motivo = motivo;
            IpOrigem = ipOrigem;
            InicioEm = DateTime.UtcNow;
            TipoAcesso = tipoAcesso;
            TenantAlvo = tenantAlvo;
        }

        public void Encerrar(string alteradoPor)
        {
            FimEm = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }
    }
}
