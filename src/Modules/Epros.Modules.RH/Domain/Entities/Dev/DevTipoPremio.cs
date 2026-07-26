using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-DEV, tabela rh_dev_tipo_premio). Fidelidade campo a campo.</summary>
    public partial class DevTipoPremio : EntidadeSaaSBase
    {
        public string? Nome { get; private set; }
        public string? Descricao { get; private set; }

        protected DevTipoPremio() { } // EF Core

        public DevTipoPremio(
            string? nome,
            string? descricao,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            Descricao = descricao;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<DevTipoPremio>().Requires();
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
