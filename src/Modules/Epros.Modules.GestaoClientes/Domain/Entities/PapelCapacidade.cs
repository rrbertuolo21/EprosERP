using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>Vínculo papel-capacidade (RBAC) - APP-TEN-003 11.7.</summary>
    public class PapelCapacidade : EntidadeSaaSBase
    {
        public Guid PapelId { get; private set; }
        public Guid CapacidadeId { get; private set; }

        protected PapelCapacidade() { } // EF Core

        public PapelCapacidade(Guid papelId, Guid capacidadeId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            PapelId = papelId;
            CapacidadeId = capacidadeId;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<PapelCapacidade>()
                .Requires()
                .AreNotEquals(PapelId, Guid.Empty, nameof(PapelId), "PapelId é obrigatório [Origem: PapelCapacidade]")
                .AreNotEquals(CapacidadeId, Guid.Empty, nameof(CapacidadeId), "CapacidadeId é obrigatório [Origem: PapelCapacidade]")
            );
        }
    }
}
