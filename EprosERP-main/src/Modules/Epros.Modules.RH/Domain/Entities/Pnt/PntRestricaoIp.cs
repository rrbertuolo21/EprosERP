using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-PNT). Fidelidade campo a campo.</summary>
    public partial class PntRestricaoIp : EntidadeSaaSBase
    {
        public string? Ip { get; private set; }
        public Guid? CriadoPorId { get; private set; }
        public Guid? OwnerId { get; private set; }
        public bool Ativo { get; private set; }

        protected PntRestricaoIp() { } // EF Core

        public PntRestricaoIp(
            string? ip,
            Guid? criadoPorId,
            Guid? ownerId,
            bool ativo,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Ip = ip;
            CriadoPorId = criadoPorId;
            OwnerId = ownerId;
            Ativo = ativo;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<PntRestricaoIp>().Requires();
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
