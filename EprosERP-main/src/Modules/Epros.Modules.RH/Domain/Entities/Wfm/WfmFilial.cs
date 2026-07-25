using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-WFM, tabela rh_wfm_filial). Fidelidade campo a campo.</summary>
    public partial class WfmFilial : EntidadeSaaSBase
    {
        public string? Nome { get; private set; }
        public Guid? CriadoPorId { get; private set; }
        public Guid? OwnerId { get; private set; }
        public bool Ativo { get; private set; }

        protected WfmFilial() { } // EF Core

        public WfmFilial(
            string? nome,
            Guid? criadoPorId,
            Guid? ownerId,
            bool ativo,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            CriadoPorId = criadoPorId;
            OwnerId = ownerId;
            Ativo = ativo;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<WfmFilial>().Requires();
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
