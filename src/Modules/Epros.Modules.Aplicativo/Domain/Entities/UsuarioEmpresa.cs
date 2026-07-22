using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Aplicativo.Domain.Entities
{
    public class UsuarioEmpresa : EntidadeSaaSBase
    {
        public Guid UsuarioId { get; private set; }
        public Guid EmpresaId { get; private set; }
        public Guid? PerfilAcessoId { get; private set; }
        public bool EhAdmin { get; private set; }

        protected UsuarioEmpresa() { } // EF Core

        public UsuarioEmpresa(
            string tenantId,
            Guid usuarioId,
            Guid empresaId,
            Guid? perfilUsuarioId,
            bool ehAdmin,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            if (usuarioId == Guid.Empty)
                AddNotification(nameof(UsuarioId), "O ID do usuário é obrigatório.");
            if (empresaId == Guid.Empty)
                AddNotification(nameof(EmpresaId), "O ID da empresa é obrigatório.");

            UsuarioId = usuarioId;
            EmpresaId = empresaId;
            PerfilAcessoId = perfilUsuarioId;
            EhAdmin = ehAdmin;
        }

        public void AtualizarPerfil(Guid? novoPerfilId, string alteradoPor)
        {
            PerfilAcessoId = novoPerfilId;
            MarcarAlterado(alteradoPor);
        }

        public void DefinirAdmin(bool ehAdmin, string alteradoPor)
        {
            EhAdmin = ehAdmin;
            MarcarAlterado(alteradoPor);
        }
    }
}
