using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-DEV, tabela rh_dev_tipo_reclamacao). Fidelidade campo a campo.</summary>
    public partial class DevTipoReclamacao : EntidadeSaaSBase
    {
        public string? Nome { get; private set; }

        protected DevTipoReclamacao() { } // EF Core

        public DevTipoReclamacao(
            string? nome,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<DevTipoReclamacao>().Requires();
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
