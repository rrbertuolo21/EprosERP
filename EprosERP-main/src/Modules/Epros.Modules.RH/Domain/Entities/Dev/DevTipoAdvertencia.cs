using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-DEV, tabela rh_dev_tipo_advertencia). Fidelidade campo a campo.</summary>
    public partial class DevTipoAdvertencia : EntidadeSaaSBase
    {
        public string? Nome { get; private set; }

        protected DevTipoAdvertencia() { } // EF Core

        public DevTipoAdvertencia(
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
            var contract = new Contract<DevTipoAdvertencia>().Requires();
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
