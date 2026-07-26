using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.DMS.Domain.Entities
{
    public class ProspectShowroom : EntidadeSaaSBase
    {
        public Guid ContactId { get; private set; }
        public Guid UnidadeId { get; private set; }
        public string Origem { get; private set; } = string.Empty;
        public Guid VendedorId { get; private set; }
        public string Status { get; private set; } = "Ativo";

        protected ProspectShowroom() { } // EF Core

        public ProspectShowroom(
            Guid contactId,
            Guid unidadeId,
            string origem,
            Guid vendedorId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ProspectShowroom>()
                .Requires()
                .AreNotEquals(contactId, Guid.Empty, nameof(ContactId), "O contato é obrigatório.")
                .AreNotEquals(unidadeId, Guid.Empty, nameof(UnidadeId), "A unidade é obrigatória.")
                .IsNotNullOrEmpty(origem, nameof(Origem), "A origem é obrigatória.")
                .AreNotEquals(vendedorId, Guid.Empty, nameof(VendedorId), "O vendedor é obrigatório.")
            );

            ContactId = contactId;
            UnidadeId = unidadeId;
            Origem = origem;
            VendedorId = vendedorId;
            Status = "Ativo";
        }

        public void AtribuirVendedor(Guid vendedorId, string usuario)
        {
            VendedorId = vendedorId;
            MarcarAlterado(usuario);
        }
    }
}
