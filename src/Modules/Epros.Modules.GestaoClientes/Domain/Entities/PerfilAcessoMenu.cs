using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class PerfilAcessoMenu : EntidadeSaaSBase
    {
        public Guid PerfilAcessoId { get; private set; }
        public Guid MenuId { get; private set; }
        public Guid? MenuItemNivel1Id { get; private set; }
        public Guid? MenuItemNivel2Id { get; private set; }
        public bool Ver { get; private set; }
        public bool Editar { get; private set; }
        public bool Excluir { get; private set; }

        protected PerfilAcessoMenu() { } // EF Core

        public PerfilAcessoMenu(
            Guid perfilAcessoId,
            Guid menuId,
            Guid? menuItemNivel1Id,
            Guid? menuItemNivel2Id,
            bool ver,
            bool editar,
            bool excluir,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<PerfilAcessoMenu>()
                .Requires()
                .AreNotEquals(perfilAcessoId, Guid.Empty, nameof(PerfilAcessoId), "O ID do perfil de acesso é obrigatório")
                .AreNotEquals(menuId, Guid.Empty, nameof(MenuId), "O ID do menu é obrigatório")
            );

            PerfilAcessoId = perfilAcessoId;
            MenuId = menuId;
            MenuItemNivel1Id = menuItemNivel1Id;
            MenuItemNivel2Id = menuItemNivel2Id;
            Ver = ver;
            Editar = editar;
            Excluir = excluir;
        }

        public void AtualizarPermissoes(bool ver, bool editar, bool excluir, string alteradoPor)
        {
            Ver = ver;
            Editar = editar;
            Excluir = excluir;
            MarcarAlterado(alteradoPor);
        }
    }
}
