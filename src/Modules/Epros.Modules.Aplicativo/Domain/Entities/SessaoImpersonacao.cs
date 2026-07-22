using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Aplicativo.Domain.Entities
{
    public class SessaoImpersonacao : EntidadeSaaSBase, IHardDeletable
    {
        public Guid UsuarioOriginalId { get; private set; }
        public Guid UsuarioAlvoId { get; private set; }
        public Guid? EmpresaId { get; private set; }
        public DateTime InicioEm { get; private set; }
        public DateTime? FimEm { get; private set; }
        public string? Motivo { get; private set; }
        public string? IpOrigem { get; private set; }

        protected SessaoImpersonacao() { } // EF Core

        public SessaoImpersonacao(
            string tenantId,
            Guid usuarioOriginalId,
            Guid usuarioAlvoId,
            Guid? empresaId,
            string? motivo,
            string? ipOrigem,
            string criadoPor)
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
        }

        public void Encerrar(string alteradoPor)
        {
            FimEm = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }
    }
}
